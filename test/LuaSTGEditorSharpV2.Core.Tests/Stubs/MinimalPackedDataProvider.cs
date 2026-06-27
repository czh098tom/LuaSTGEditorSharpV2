using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.Core.Tests.Stubs;

public class MinimalPackedDataProvider(IServiceProvider serviceProvider)
    : PackedDataProviderServiceBase<string>(serviceProvider)
{
}
