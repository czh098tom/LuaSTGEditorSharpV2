using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Specialized.AdvancedRepeat;

public sealed class VariableCollectionCodeGeneration(
    CodeGeneratorServiceProvider provider, IServiceProvider services) : CodeGenerationSubService(provider, services)
{
    // V1 does not override ToLua on the collection, so ordinary children are compiled in
    // physical order at the collection's position inside the loop body, while variable
    // children only contribute the separately prepared initialization/update phases.
    protected override IEnumerable<CodeData> GenerateCodeWithContext(
        NodeData collection, CodeGenerationContext context)
        => NodeServiceProvider.GenerateForChildren(collection, context, 0);

    public IReadOnlyList<VariableTransformationOutput> Generate(
        NodeData collection, CodeGenerationContext context)
    {
        using var scope = context.AcquireContextHandle(collection);
        var output = new List<VariableTransformationOutput>();
        foreach (var pair in NodeServiceProvider.GetServicesPairForLogicalChildrenOfType<CodeGeneratorServiceBase>(collection))
        {
            if (pair.Service is not VariableTransformationCodeGeneration variable)
            {
                continue;
            }
            var result = variable.Generate(pair.NodeData, context);
            if (result is null)
            {
                throw new AdvancedRepeatGenerationException("Variable service returned no output.", pair.NodeData);
            }
            output.Add(new VariableTransformationOutput(
                result.Initialization.ToArray(), result.Update.ToArray()));
        }
        return output.AsReadOnly();
    }
}
