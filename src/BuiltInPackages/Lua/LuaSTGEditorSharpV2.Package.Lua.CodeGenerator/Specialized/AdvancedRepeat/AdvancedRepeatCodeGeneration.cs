using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Model;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Specialized.AdvancedRepeat;

public sealed class AdvancedRepeatCodeGeneration(
    CodeGeneratorServiceProvider provider, IServiceProvider services) : CodeGeneratorServiceBase(provider, services)
{
    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, CaptureWithMacroOption> Captures { get; set; } = [];

    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, ContextCapture> ContextCaptures { get; set; } = [];

    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, string> ConditionTemplates { get; set; } = [];

    [JsonProperty(Required = Required.Always)]
    public string[] Head { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] LoopHead { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] LoopTail { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] Tail { get; set; } = null!;

    protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData repeat, CodeGenerationContext context)
    {
        var model = CodeFragmentTemplate.CreateModel(ServiceProvider, repeat, context,
            Captures, ContextCaptures, ConditionTemplates);
        var head = CodeFragmentTemplate.Render(Head, model, repeat, context);
        var loopHead = CodeFragmentTemplate.Render(LoopHead, model, repeat, context);
        var loopTail = CodeFragmentTemplate.Render(LoopTail, model, repeat, context);
        var tail = CodeFragmentTemplate.Render(Tail, model, repeat, context);
        IReadOnlyList<VariableTransformationOutput> variables;
        using (context.AcquireContextHandle(repeat, 1))
        {
            var variablesList = new List<VariableTransformationOutput>();
            foreach (var collection in NodeServiceProvider
                .GetServicesPairForLogicalChildrenOfType<VariableCollectionCodeGeneration>(repeat))
            {
                variablesList.AddRange(collection.Service.Generate(collection.NodeData, context));
            }
            variables = variablesList.AsReadOnly();
        }

        foreach (var code in head)
        {
            yield return code;
        }
        foreach (var variable in variables)
        {
            foreach (var code in variable.Initialization)
            {
                yield return code;
            }
        }
        foreach (var code in loopHead)
        {
            yield return code;
        }
        foreach (var code in NodeServiceProvider.GenerateForChildren(repeat, context, 2))
        {
            yield return code;
        }
        foreach (var variable in variables)
        {
            foreach (var code in variable.Update)
            {
                yield return code;
            }
        }
        foreach (var code in loopTail)
        {
            yield return code;
        }
        foreach (var code in tail)
        {
            yield return code;
        }
    }
}
