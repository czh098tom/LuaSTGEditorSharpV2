using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Model;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Specialized.AdvancedRepeat;

public sealed class TemplateVariableTransformationCodeGeneration(
    CodeGeneratorServiceProvider provider, IServiceProvider services) : VariableTransformationCodeGeneration(provider, services)
{
    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, CaptureWithMacroOption> Captures { get; set; } = [];

    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, ContextCapture> ContextCaptures { get; set; } = [];

    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, string> ConditionTemplates { get; set; } = [];

    [JsonProperty(Required = Required.Always)]
    public string[] Initialization { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] Update { get; set; } = null!;

    public override VariableTransformationOutput Generate(NodeData node, CodeGenerationContext context)
    {
        var model = CodeFragmentTemplate.CreateModel(ServiceProvider, node, context,
            Captures, ContextCaptures, ConditionTemplates);
        return new VariableTransformationOutput(
            CodeFragmentTemplate.Render(Initialization, model, node, context),
            CodeFragmentTemplate.Render(Update, model, node, context));
    }
}
