using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Settings
{
    public interface ISettingsProvider
    {
        object Settings { get; set; }

        void RefreshSettings();
    }
}
