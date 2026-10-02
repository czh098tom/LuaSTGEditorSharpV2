using System.Globalization;
using System.Text;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

/// <summary>
/// Rewrites every V1 <c>.Advanced.UnidentifiedNode</c> instance into a V2 <c>Code</c> node holding the
/// Lua text the V1 editor would have emitted for it.
/// <para>
/// V1 treats a user-defined node as a template: a <c>.Advanced.UserDefinedNode</c> definition carries a
/// <c>Head parse rule</c> / <c>Tail parse rule</c> pair plus a property list, and each
/// <c>UnidentifiedNode</c> instance names one definition through its <c>Type</c> attribute and supplies
/// the arguments. <c>UnidentifiedNode.ToLua</c> looks the definition up in the document meta, substitutes
/// the instance's attributes into the rule, and wraps the node's children between head and tail.
/// </para>
/// <para>
/// Because the substitution happens in the editor rather than in the game, the result is ordinary Lua
/// text. Translating the instance to <c>Code</c> therefore keeps the converted document semantically
/// identical while removing the need for a V2 runtime implementation of the meta layer.
/// </para>
/// <para>
/// Substitution follows <c>UnidentifiedNode.ToLua</c> exactly: a property whose
/// <c>Prop n is string</c> flag parses true is passed through <c>Lua.StringParser.ParseLua</c> (escaped
/// without adding quotes), every other property is macro-expanded and inserted raw. The <c>Type</c> attribute is
/// metadata for the lookup and never reaches the template.
/// </para>
/// <para>
/// This runs once on the raw V1 tree, before any type or property formatter has renamed anything, so it
/// reads the original V1 captions (<c>Head parse rule</c>, <c>Prop 1 is string</c>, ...).
/// </para>
/// </summary>
internal static class UserDefinedNodeConverter
{
    private const string UnidentifiedType = "UnidentifiedNode";
    private const string DefinitionType = "UserDefinedNode";
    private const string TypeAttribute = "Type";
    private const string CodeType = "Code";

    /// <summary>Rewrites the whole tree in place; call once, before formatting.</summary>
    internal static void RewriteAll(NodeData? root)
    {
        if (root == null) return;
        var templates = new Dictionary<string, UserDefinedTemplate>(StringComparer.Ordinal);
        Collect(root, templates);
        if (templates.Count == 0) return;
        Rewrite(root, templates);
    }

    /// <summary>
    /// Compares through <see cref="SharpResourceMigrationAnalyzer.GetLegacyTypeName"/> because the raw V1
    /// tree carries either the fully qualified `$type` string or the short name, depending on how the
    /// project was saved.
    /// </summary>
    private static bool IsType(NodeData node, string expected)
        => SharpResourceMigrationAnalyzer.GetLegacyTypeName(node) == expected;

    private static void Collect(NodeData node, Dictionary<string, UserDefinedTemplate> templates)
    {
        if (node.IsActive && IsType(node, DefinitionType))
        {
            var template = UserDefinedTemplate.FromDefinition(node);
            if (template != null) templates[template.Name] = template;
        }
        foreach (var child in node.PhysicalChildren) Collect(child, templates);
    }

    private static void Rewrite(NodeData node, Dictionary<string, UserDefinedTemplate> templates)
    {
        foreach (var child in node.PhysicalChildren) Rewrite(child, templates);
        if (!node.IsActive || !IsType(node, UnidentifiedType)) return;

        var typeName = node.GetProperty(TypeAttribute);
        if (!templates.TryGetValue(typeName, out var template))
        {
            // V1 emits nothing for an unknown definition, but that would silently drop the node's
            // children too, so refuse instead of losing them.
            throw new InvalidDataException(
                $"sharpconv user-defined node ({typeName}): no matching UserDefinedNode definition was found.");
        }

        var code = template.Substitute(node);
        node.Properties.Clear();
        node.Properties["code"] = code;
        node.TypeUID = CodeType;
    }

    /// <summary>One <c>.Advanced.UserDefinedNode</c> definition, in V1's own caption spelling.</summary>
    private sealed class UserDefinedTemplate(string name, string head, string tail, bool[] isString)
    {
        private readonly string _head = head;
        private readonly string _tail = tail;
        private readonly bool[] _isString = isString;

        internal string Name => name;

        internal static UserDefinedTemplate? FromDefinition(NodeData node)
        {
            var name = node.GetProperty("Name");
            if (string.IsNullOrEmpty(name)) return null;
            var count = ParseCount(node.GetProperty("Number of prop"));
            // V1 keeps three parallel columns per parameter; only the is-string flag changes emission.
            var flags = new bool[count];
            for (var index = 0; index < count; index++)
            {
                var slot = (index + 1).ToString(CultureInfo.InvariantCulture);
                flags[index] = bool.TryParse(node.GetProperty($"Prop {slot} is string"), out var parsed) && parsed;
            }
            return new UserDefinedTemplate(name, node.GetProperty("Head parse rule"),
                node.GetProperty("Tail parse rule"), flags);
        }

        private static int ParseCount(string raw)
            => int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0
                ? value : 0;

        /// <summary>Builds the Lua text for one instance.</summary>
        internal string Substitute(NodeData instance)
        {
            // The instance's `Type` attribute is the lookup key and is not one of the parameters;
            // the remaining attributes are the parameters in declaration order.
            var properties = instance.Properties;
            var rules = new string[_isString.Length];
            var cursor = 0;
            foreach (var property in properties)
            {
                if (property.Key == TypeAttribute) continue;
                if (cursor >= rules.Length) break;
                var value = property.Value;
                rules[cursor] = _isString[cursor] ? EscapeLuaStringContent(value) : value;
                cursor++;
            }
            while (cursor < rules.Length)
            {
                rules[cursor] = _isString[cursor] ? EscapeLuaStringContent(string.Empty) : string.Empty;
                cursor++;
            }
            var head = FormatRule(_head, rules);
            var tail = FormatRule(_tail, rules);
            if (instance.PhysicalChildren.Count == 0) return head + tail;
            if (tail.Length == 0)
            {
                throw new InvalidDataException(
                    "sharpconv user-defined node: the definition has no tail rule, so child statements "
                    + "cannot be placed after the node's own code.");
            }
            var builder = new StringBuilder(head);
            foreach (var child in instance.PhysicalChildren)
            {
                if (child.IsActive) builder.Append('\n');
            }
            return builder.Append(tail).ToString();
        }

        private static string FormatRule(string rule, string[] rules)
            => string.IsNullOrEmpty(rule) ? string.Empty : string.Format(CultureInfo.InvariantCulture, rule, rules);

        /// <summary>Mirrors <c>Lua.StringParser.ParseLua</c>: escape string content without quoting it.</summary>
        private static string EscapeLuaStringContent(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                if (character is '\\' or '\'' or '"') builder.Append('\\');
                builder.Append(character);
            }
            return builder.ToString();
        }
    }
}
