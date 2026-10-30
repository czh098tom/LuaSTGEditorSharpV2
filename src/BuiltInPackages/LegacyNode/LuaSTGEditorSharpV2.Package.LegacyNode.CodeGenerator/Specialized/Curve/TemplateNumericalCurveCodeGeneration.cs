using System.Globalization;

using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Model;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized.Curve;

public sealed class TemplateNumericalCurveCodeGeneration(
    CodeGeneratorServiceProvider provider,
    IServiceProvider services) : NumericalCurveCodeGeneration(provider, services)
{
    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, CaptureWithMacroOption> Captures { get; set; } = [];

    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, ContextCapture> ContextCaptures { get; set; } = [];

    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, string> ConditionTemplates { get; set; } = [];

    [JsonProperty(Required = Required.Always)]
    public string[] Declaration { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] TaskHead { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] BodyHead { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] BodyTail { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] TaskTail { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] BodyIndention { get; set; } = null!;

    public override CurveOutput Generate(
        NodeData curve,
        CodeGenerationContext context,
        string variableName)
    {
        var arguments = new Dictionary<string, object?> { ["variableName"] = variableName };
        var model = CodeFragmentTemplate.CreateModel(
            ServiceProvider,
            curve,
            context,
            Captures,
            ContextCaptures,
            ConditionTemplates,
            arguments);
        var bodyIndention = int.Parse(
            CodeFragmentTemplate.RenderText(BodyIndention, model, context),
            CultureInfo.InvariantCulture);
        if (bodyIndention < 0)
        {
            throw new CurveGenerationException("Curve body indention must be nonnegative.", curve);
        }
        var children = curve.PhysicalChildren
            .Select(child => (Node: child, Service: NodeServiceProvider.ResolveService(child)))
            .ToArray();
        var first = children.FirstOrDefault(pair => pair.Service is CurvePointCodeGeneration);
        if (first.Node is null)
        {
            throw new CurveGenerationException("NumericalCurve requires a first NumericalPoint.", curve);
        }
        if (!first.Node.IsActive)
        {
            throw new CurveGenerationException("The first physical NumericalPoint is disabled.", first.Node);
        }

        var body = new List<CodeData>();
        using (context.AcquireContextHandle(curve, bodyIndention))
        {
            CurvePointOutput? firstOutput = null;
            foreach (var child in children)
            {
                if (!child.Node.IsActive)
                {
                    continue;
                }
                if (child.Service is CurvePointCodeGeneration point)
                {
                    var pointOutput = point.Generate(
                        child.Node,
                        context,
                        variableName,
                        ReferenceEquals(child.Node, first.Node));
                    if (pointOutput is null)
                    {
                        throw new CurveGenerationException(
                            "Curve point service returned no output.",
                            child.Node);
                    }
                    if (ReferenceEquals(child.Node, first.Node))
                    {
                        firstOutput = pointOutput;
                    }
                    body.AddRange(pointOutput.Body);
                }
                else
                {
                    body.AddRange(NodeServiceProvider.GenerateCode(child.Node, context));
                }
            }
            if (firstOutput?.InitialValue is null || firstOutput.PreviousValue is null)
            {
                throw new CurveGenerationException(
                    "The first NumericalPoint returned incomplete metadata.",
                    first.Node);
            }
            arguments["initialValue"] = firstOutput.InitialValue;
            arguments["previousValue"] = firstOutput.PreviousValue;
            body.InsertRange(0, CodeFragmentTemplate.Render(BodyHead, model, curve, context));
            body.AddRange(CodeFragmentTemplate.Render(BodyTail, model, curve, context));
        }

        var declaration = CodeFragmentTemplate.Render(Declaration, model, curve, context);
        var task = new List<CodeData>();
        task.AddRange(CodeFragmentTemplate.Render(TaskHead, model, curve, context));
        task.AddRange(body);
        task.AddRange(CodeFragmentTemplate.Render(TaskTail, model, curve, context));
        return new CurveOutput(declaration, task);
    }
}
