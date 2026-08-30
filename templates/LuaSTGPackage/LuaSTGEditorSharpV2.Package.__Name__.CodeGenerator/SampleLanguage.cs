using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.Package.__Name__.CodeGenerator;

public class SampleLanguage(IServiceProvider serviceProvider) : LanguageBase(serviceProvider)
{
    public override string Name => "Sample";
}
