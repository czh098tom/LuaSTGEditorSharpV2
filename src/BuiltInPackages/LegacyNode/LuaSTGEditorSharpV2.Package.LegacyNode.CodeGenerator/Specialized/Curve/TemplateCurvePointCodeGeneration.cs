using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Model;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized.Curve;

public sealed class TemplateCurvePointCodeGeneration(
    CodeGeneratorServiceProvider provider,
    IServiceProvider services) : CurvePointCodeGeneration(provider, services)
{
    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, CaptureWithMacroOption> Captures { get; set; } = [];

    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, ContextCapture> ContextCaptures { get; set; } = [];

    [JsonProperty(Required = Required.DisallowNull)]
    public Dictionary<string, string> ConditionTemplates { get; set; } = [];

    [JsonProperty(Required = Required.Always)]
    public CaptureWithMacroOption InterpolationCapture { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] InitialTime { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] InitialValue { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] PreviousValue { get; set; } = null!;

    [JsonProperty(Required = Required.Always)]
    public string[] Body { get; set; } = null!;

    public override CurvePointOutput Generate(
        NodeData point,
        CodeGenerationContext context,
        string variableName,
        bool isFirstPoint)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(Captures);
            ArgumentNullException.ThrowIfNull(ContextCaptures);
            ArgumentNullException.ThrowIfNull(ConditionTemplates);
            ArgumentNullException.ThrowIfNull(InterpolationCapture);
            ArgumentNullException.ThrowIfNull(InitialTime);
            ArgumentNullException.ThrowIfNull(InitialValue);
            ArgumentNullException.ThrowIfNull(PreviousValue);
            ArgumentNullException.ThrowIfNull(Body);

            var token = new NodePropertyAccessToken(ServiceProvider, point, context);
            var interpolation = InterpolationCapture.ApplyMacro(token, context);
            var arguments = new Dictionary<string, object?>
            {
                ["variableName"] = variableName,
                ["interpolation"] = interpolation,
                ["linear"] = string.IsNullOrWhiteSpace(interpolation)
            };
            var model = CodeFragmentTemplate.CreateModel(
                ServiceProvider,
                point,
                context,
                Captures,
                ContextCaptures,
                ConditionTemplates,
                arguments);
            string? initialValue = null;
            string? previousValue = null;
            if (isFirstPoint)
            {
                var initialTime = CodeFragmentTemplate.RenderText(InitialTime, model, context);
                if (Convert.ToInt32(initialTime) != 0)
                {
                    throw new CurveGenerationException(
                        "The first NumericalPoint time must be zero.",
                        point);
                }
                initialValue = CodeFragmentTemplate.RenderText(InitialValue, model, context);
                previousValue = CodeFragmentTemplate.RenderText(PreviousValue, model, context);
            }
            return new CurvePointOutput(
                initialValue,
                previousValue,
                CodeFragmentTemplate.Render(Body, model, point, context));
        }
        catch (CurveGenerationException)
        {
            throw;
        }
        catch (Exception error)
        {
            throw new CurveGenerationException("NumericalPoint generation failed.", point, error);
        }
    }
}
