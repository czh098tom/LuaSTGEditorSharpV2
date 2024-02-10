using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Services;

namespace LuaSTGEditorSharpV2.ViewModel
{
    /// <summary>
    /// Base viewmodel for any anchorable pages (excluding document panels)
    /// </summary>
    public class AnchorableViewModelBase : DockingViewModelBase
    {
        public override string Title => HostedApplicationHelper.GetService<LocalizationService>()
            ?.GetString(I18NTitleKey, GetType().Assembly) ?? GetType().Name;

        public virtual string I18NTitleKey => GetType().Name;

        public virtual string ContentID => GetType().AssemblyQualifiedName ?? string.Empty;
    }
}
