using System.Globalization;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Model;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized.Curve;

public sealed class NumericalTrackCodeGeneration(
    CodeGeneratorServiceProvider provider,
    IServiceProvider services) : CodeGeneratorServiceBase(provider, services)
{
    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, CaptureWithMacroOption> Captures { get; set; } = [];

    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, ContextCapture> ContextCaptures { get; set; } = [];

    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, string> ConditionTemplates { get; set; } = [];

    [JsonProperty(Required = Required.Always)]
    public CaptureWithMacroOption MappingCapture { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string VariableNameFormat { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] Head { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] MappingTask { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] Tail { get; set; } = null!;

    protected override IEnumerable<CodeData> GenerateCodeWithContext(
        NodeData track,
        CodeGenerationContext context)
    {
        var children = track.GetLogicalChildren()
            .Select(child => (Node: child, Service: NodeServiceProvider.ResolveService(child)))
            .ToArray();
        var curves = children
            .Where(pair => pair.Service is NumericalCurveCodeGeneration)
            .ToArray();
        var names = curves
            .Select((_, index) => string.Format(
                CultureInfo.InvariantCulture,
                VariableNameFormat,
                index))
            .ToArray();
        var mapping = MappingCapture.ApplyMacro(
            new NodePropertyAccessToken(ServiceProvider, track, context),
            context);
        var arguments = new Dictionary<string, object?>
        {
            ["mapping"] = string.Format(mapping, names),
            ["curveCount"] = names.Length
        };
        var model = CodeFragmentTemplate.CreateModel(
            ServiceProvider,
            track,
            context,
            Captures,
            ContextCaptures,
            ConditionTemplates,
            arguments);
        var head = CodeFragmentTemplate.Render(Head, model, track, context);
        var mappingTask = CodeFragmentTemplate.Render(MappingTask, model, track, context);
        var tail = CodeFragmentTemplate.Render(Tail, model, track, context);
        var declarations = new List<CodeData>();
        var mixed = new List<CodeData>();
        var curveIndex = 0;
        using (context.AcquireContextHandle(track, 1))
        {
            foreach (var child in children)
            {
                if (child.Service is NumericalCurveCodeGeneration curve)
                {
                    var curveOutput = curve.Generate(child.Node, context, names[curveIndex++]);
                    if (curveOutput is null)
                    {
                        throw new CurveGenerationException(
                            "Curve service returned no output.",
                            child.Node);
                    }
                    declarations.AddRange(curveOutput.Declaration);
                    mixed.AddRange(curveOutput.Task);
                }
                else
                {
                    mixed.AddRange(NodeServiceProvider.GenerateCode(child.Node, context));
                }
            }
        }

        var output = new List<CodeData>();
        output.AddRange(head);
        output.AddRange(declarations);
        output.AddRange(mixed);
        output.AddRange(mappingTask);
        output.AddRange(tail);
        return output;
    }
}
