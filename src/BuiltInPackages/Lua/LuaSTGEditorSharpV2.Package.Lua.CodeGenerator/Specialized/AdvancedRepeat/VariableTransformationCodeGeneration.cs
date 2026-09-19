using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Specialized.AdvancedRepeat;

public abstract class VariableTransformationCodeGeneration(
    CodeGeneratorServiceProvider provider, IServiceProvider services) : CodeGenerationSubService(provider, services)
{
    public abstract VariableTransformationOutput Generate(NodeData node, CodeGenerationContext context);
}
