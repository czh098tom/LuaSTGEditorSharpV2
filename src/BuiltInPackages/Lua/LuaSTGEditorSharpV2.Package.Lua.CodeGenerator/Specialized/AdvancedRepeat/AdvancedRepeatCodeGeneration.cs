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
        CodeFragment head;
        CodeFragment loopHead;
        CodeFragment loopTail;
        CodeFragment tail;
        IReadOnlyList<VariableTransformationOutput> variables;
        try
        {
            ArgumentNullException.ThrowIfNull(Captures);
            ArgumentNullException.ThrowIfNull(ContextCaptures);
            ArgumentNullException.ThrowIfNull(ConditionTemplates);
            ArgumentNullException.ThrowIfNull(Head);
            ArgumentNullException.ThrowIfNull(LoopHead);
            ArgumentNullException.ThrowIfNull(LoopTail);
            ArgumentNullException.ThrowIfNull(Tail);
            var model = CodeFragmentTemplate.CreateModel(ServiceProvider, repeat, context,
                Captures, ContextCaptures, ConditionTemplates);
            head = CodeFragmentTemplate.Render(Head, model, repeat, context);
            loopHead = CodeFragmentTemplate.Render(LoopHead, model, repeat, context);
            loopTail = CodeFragmentTemplate.Render(LoopTail, model, repeat, context);
            tail = CodeFragmentTemplate.Render(Tail, model, repeat, context);
            using var scope = context.AcquireContextHandle(repeat, 1);
            var variablesList = new List<VariableTransformationOutput>();
            foreach (var collection in NodeServiceProvider
                .GetServicesPairForLogicalChildrenOfType<VariableCollectionCodeGeneration>(repeat))
            {
                variablesList.AddRange(collection.Service.Generate(collection.NodeData, context));
            }
            variables = variablesList.AsReadOnly();
        }
        catch (AdvancedRepeatGenerationException)
        {
            throw;
        }
        catch (System.Exception error)
        {
            throw new AdvancedRepeatGenerationException("AdvancedRepeat preparation failed.", repeat, error);
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
