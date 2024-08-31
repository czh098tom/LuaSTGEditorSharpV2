using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using CommunityToolkit.Mvvm.Input;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Toolbox.ViewModel;
using LuaSTGEditorSharpV2.Toolbox.Service;

namespace LuaSTGEditorSharpV2.Toolbox.Model
{
    [PackagePrimaryKey(nameof(Path))]
    public class ToolboxItemModelBase(IServiceProvider serviceProvider) : PackedDataBase(serviceProvider)
    {
        public static readonly string invalidPath = @"/\/\/\INVALID_PATH/\/\/\";

        [JsonProperty] public string Path { get; private set; } = invalidPath;
        [JsonProperty] public int Order { get; private set; } = 0;
        [JsonProperty] public string? IconSource { get; private set; } = string.Empty;
        [JsonProperty] public LocalizableString? Caption { get; private set; }

        public virtual ToolboxItemViewModel CreateViewModel()
        {
            var vm = new ToolboxItemViewModel()
            {
                Caption = Caption?.GetLocalized(Path.Split(ToolboxProviderService.seperator).Last()) 
                    ?? Path.Split(ToolboxProviderService.seperator).Last(),
                IconSource = IconSource
            };
            return vm;
        }
    }
}
