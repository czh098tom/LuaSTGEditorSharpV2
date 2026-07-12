using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.Package.__Name__.SharpProjectConverter;

// This ServiceProvider is discovered by NodePackageProvider via [PackedServiceProvider] reflection.
// Drop .sharpconv JSON files into package/__Name__/SharpConverters/ to register converters
// that translate legacy sharp-format node trees into current TypeUIDs.
[PackedServiceProvider]
[ServiceShortName("sharpconv")]
public class SharpNodeConverterServiceProvider(IServiceProvider serviceProvider)
    : PackedDataProviderServiceBase<SharpNodeFormatConverter>(serviceProvider)
{
}
