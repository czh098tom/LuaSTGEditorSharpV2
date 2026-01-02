using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

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
        public override string Title => ServiceProvider.GetRequiredService<LocalizationService>()
            ?.GetString(I18NTitleKey, GetType().Assembly) ?? GetType().Name;

        public virtual string I18NTitleKey => GetType().Name;

        public virtual string ContentID => GetType().AssemblyQualifiedName ?? string.Empty;

        private bool _isVisible = true;

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _disposedValue = false;

        public AnchorableViewModelBase(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            OnClose += AnchorableViewModelBase_OnClose;
        }

        private void AnchorableViewModelBase_OnClose(object? sender, EventArgs e)
        {
            IsVisible = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Crash if IsVisible = true while closing window
                    IsVisible = false;
                }

                _disposedValue = true;
            }
        }
    }
}
