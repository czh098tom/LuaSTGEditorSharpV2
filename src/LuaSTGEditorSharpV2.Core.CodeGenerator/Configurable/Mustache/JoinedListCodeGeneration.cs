using LuaSTGEditorSharpV2.Core.Model;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;

/// <summary>Renders a shared row set into named columns, then emits a head and tail.</summary>
public sealed class JoinedListCodeGeneration(
    CodeGeneratorServiceProvider nodeServiceProvider, IServiceProvider serviceProvider)
    : CodeGeneratorServiceBase(nodeServiceProvider, serviceProvider)
{
    [JsonProperty(Required = Required.Always)] public NodePropertyCapture CountCapture { get; private set; } = null!;
    [JsonProperty] public int MaximumCount { get; private set; } = 100;
    [JsonProperty] public Dictionary<string, CaptureWithMacroOption> Captures { get; private set; } = [];
    [JsonProperty] public Dictionary<string, ContextCapture> ContextCaptures { get; private set; } = [];
    [JsonProperty] public Dictionary<string, string> ConditionTemplates { get; private set; } = [];
    [JsonProperty(Required = Required.Always)] public Dictionary<string, CaptureWithMacroOption> ItemCaptures { get; private set; } = [];
    [JsonProperty(Required = Required.Always)] public string RequiredCaptureName { get; private set; } = string.Empty;
    [JsonProperty(Required = Required.Always)] public Dictionary<string, JoinedListColumn> Columns { get; private set; } = [];
    [JsonProperty] public string[] Head { get; private set; } = [];
    [JsonProperty] public string[] Tail { get; private set; } = [];
    [JsonProperty] public bool IgnoreChildren { get; private set; }

    internal protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
    {
        ValidateConfiguration();
        var token = new NodePropertyAccessToken(ServiceProvider, node, context);
        var count = int.TryParse(CountCapture.Capture(token), out var parsed)
            ? Math.Clamp(parsed, 0, MaximumCount) : 0;
        var fragments = Columns.ToDictionary(entry => entry.Key, _ => new List<string>(), StringComparer.Ordinal);
        var rows = 0;
        for (var index = 0; index < count; index++)
        {
            // Test the raw required value first: a macro must not activate an empty-name slot.
            if (string.IsNullOrEmpty(ItemCaptures[RequiredCaptureName].CaptureByFormat(token, index)))
            {
                continue;
            }
            var item = new Dictionary<string, object?>();
            var rawItem = new Dictionary<string, object?>();
            foreach (var entry in ItemCaptures)
            {
                rawItem.Add(entry.Key, entry.Value.CaptureByFormat(token, index));
                item.Add(entry.Key, entry.Value.ApplyMacroWithFormat(token, context, index));
            }
            var itemModel = CodeFragmentTemplate.CreateModel(ServiceProvider, node, context,
                Captures, ContextCaptures, ConditionTemplates,
                new Dictionary<string, object?> { ["item"] = item, ["rawItem"] = rawItem, ["index"] = index });
            foreach (var entry in Columns)
            {
                fragments[entry.Key].Add(CodeFragmentTemplate.RenderText(entry.Value.Template, itemModel, context));
            }
            rows++;
        }
        var columns = Columns.ToDictionary(entry => entry.Key,
            entry => (object?)string.Join(entry.Value.Separator, fragments[entry.Key]), StringComparer.Ordinal);
        var model = CodeFragmentTemplate.CreateModel(ServiceProvider, node, context,
            Captures, ContextCaptures, ConditionTemplates,
            new Dictionary<string, object?> { ["columns"] = columns, ["hasItems"] = rows > 0 });
        // Keep this invocation's model across recursive calls; generators are shared by TypeUID.
        foreach (var code in CodeFragmentTemplate.Render(Head, model, node, context))
        {
            yield return code;
        }
        if (!IgnoreChildren)
        {
            foreach (var code in NodeServiceProvider.GenerateForChildren(node, context, 0))
            {
                yield return code;
            }
        }
        foreach (var code in CodeFragmentTemplate.Render(Tail, model, node, context))
        {
            yield return code;
        }
    }

    private void ValidateConfiguration()
    {
        if (CountCapture is null || MaximumCount < 0 || MaximumCount > 10000
            || Captures is null || ContextCaptures is null || ConditionTemplates is null
            || ItemCaptures is null || ItemCaptures.Values.Any(capture => capture is null)
            || string.IsNullOrEmpty(RequiredCaptureName) || !ItemCaptures.ContainsKey(RequiredCaptureName)
            || Columns is null || Columns.Count == 0 || Head is null || Tail is null
            || Columns.Values.Any(column => column is null || column.Template is null || column.Separator is null))
        {
            throw new InvalidOperationException("JoinedListCodeGeneration requires valid captures, columns, templates and a bounded MaximumCount.");
        }
    }
}
