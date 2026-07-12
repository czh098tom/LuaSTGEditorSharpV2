using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.Package.__Name__.CodeGenerator;

public class PackageEntry : IServiceInstanceProvider<LanguageBase>
{
    public IReadOnlyCollection<LanguageBase> GetServiceInstances(IServiceProvider serviceProvider)
    {
        return [new SampleLanguage(serviceProvider)];
    }
}
