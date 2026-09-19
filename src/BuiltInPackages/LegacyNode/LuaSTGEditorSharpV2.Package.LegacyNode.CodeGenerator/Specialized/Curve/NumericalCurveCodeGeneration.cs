using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized.Curve;

public abstract class NumericalCurveCodeGeneration(
    CodeGeneratorServiceProvider provider,
    IServiceProvider services) : CodeGenerationSubService(provider, services)
{
    public abstract CurveOutput Generate(
        NodeData node,
        CodeGenerationContext context,
        string variableName);
}
