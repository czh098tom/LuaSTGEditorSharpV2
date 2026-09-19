using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Specialized.AdvancedRepeat;

public sealed class VariableCollectionCodeGeneration(
    CodeGeneratorServiceProvider provider, IServiceProvider services) : CodeGenerationSubService(provider, services)
{
    public IReadOnlyList<VariableTransformationOutput> Generate(
        NodeData collection, CodeGenerationContext context)
    {
        using var scope = context.AcquireContextHandle(collection);
        var output = new List<VariableTransformationOutput>();
        foreach (var pair in NodeServiceProvider.GetServicesPairForLogicalChildrenOfType<CodeGeneratorServiceBase>(collection))
        {
            if (pair.Service is not VariableTransformationCodeGeneration variable)
            {
                throw new AdvancedRepeatGenerationException(
                    "VariableCollection has an incompatible active child.", pair.NodeData);
            }
            try
            {
                var result = variable.Generate(pair.NodeData, context);
                if (result is null)
                {
                    throw new AdvancedRepeatGenerationException("Variable service returned no output.", pair.NodeData);
                }
                output.Add(result);
            }
            catch (AdvancedRepeatGenerationException)
            {
                throw;
            }
            catch (System.Exception error)
            {
                throw new AdvancedRepeatGenerationException("Variable generation failed.", pair.NodeData, error);
            }
        }
        return output.AsReadOnly();
    }
}
