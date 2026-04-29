using Microsoft.Extensions.DependencyInjection;
using LuaSTGEditorSharpV2.Core.Settings;

namespace LuaSTGEditorSharpV2.Core.Parsing;

[Inject(ServiceLifetime.Singleton)]
public class ParserSettingsProvider : ISettingsProvider
{
    private ParserServiceSettings _settings = new();

    public object Settings
    {
        get => _settings;
        set => _settings = value as ParserServiceSettings ?? _settings;
    }

    public void RefreshSettings()
    {
        ParserOptions.Default = _settings.ToParserOptions();
    }
}
