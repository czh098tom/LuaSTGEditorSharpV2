using System.Globalization;
using System.Text;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

/// <summary>Expands an instance using V1 templates captured before node formatting.</summary>
[JsonTypeShortName(typeof(ISharpNodeFormatConverter), "UserDefinedNode")]
public sealed class UserDefinedNodeConverter : ISharpNodeFormatConverter
{
    private const string UnidentifiedType = "UnidentifiedNode";
    private const string DefinitionType = "UserDefinedNode";
    private const string TypeAttribute = "Type";
    private const string CodeSegmentType = "CodeSegment";

    internal static (IReadOnlyDictionary<string, UserDefinedTemplate> Templates,
        IReadOnlySet<NodeData> Instances) CollectTemplates(NodeData? root)
    {
        var templates = new Dictionary<string, UserDefinedTemplate>(StringComparer.Ordinal);
        var instances = new HashSet<NodeData>();
        if (root != null) Collect(root, templates, instances, true);
        return (templates, instances);
    }

    private static bool IsType(NodeData node, string expected)
        => SharpResourceMigrationAnalyzer.GetLegacyTypeName(node) == expected;

    private static void Collect(NodeData node, Dictionary<string, UserDefinedTemplate> templates,
        HashSet<NodeData> instances, bool ancestorsActive)
    {
        if (node.IsActive && IsType(node, DefinitionType))
        {
            var template = UserDefinedTemplate.FromDefinition(node);
            if (template != null) templates[template.Name] = template;
        }
        // Other converters can move children, so retain eligibility from the original tree.
        var active = ancestorsActive && node.IsActive;
        if (active && IsType(node, UnidentifiedType)) instances.Add(node);
        foreach (var child in node.PhysicalChildren) Collect(child, templates, instances, active);
    }

    public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
    {
        if (!context.UserDefinedInstances.Contains(source) || context.UserDefinedTemplates.Count == 0)
        {
            return source;
        }
        var typeName = source.GetProperty(TypeAttribute);
        if (!context.UserDefinedTemplates.TryGetValue(typeName, out var template))
        {
            context.ReportDiagnostic(source,
                $"sharpconv user-defined node ({typeName}): no matching UserDefinedNode definition was "
                + "found; the node is kept as-is and only its children will be generated.");
            return source;
        }

        var (head, tail) = template.Substitute(source);
        source.Properties.Clear();
        source.Properties["head"] = head;
        source.Properties["tail"] = tail;
        source.TypeUID = CodeSegmentType;
        return source;
    }

    /// <summary>One <c>.Advanced.UserDefinedNode</c> definition, in V1's own caption spelling.</summary>
    internal sealed class UserDefinedTemplate(string name, string head, string tail, bool[] isString)
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

        /// <summary>Builds the head / tail text for one instance; the children stay in the tree.</summary>
        internal (string Head, string Tail) Substitute(NodeData instance)
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
            return (FormatRule(_head, rules), FormatRule(_tail, rules));
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
