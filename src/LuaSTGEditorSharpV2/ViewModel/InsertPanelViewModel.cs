using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command.Factory;
using LuaSTGEditorSharpV2.Core.Command.Service;
using LuaSTGEditorSharpV2.Services;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class InsertPanelViewModel(IServiceProvider serviceProvider) : InjectableViewModel(serviceProvider)
    {
        private bool _isInsertUp = false;
        private bool _isInsertDown = true;
        private bool _isInsertAsChild = false;
        private bool _isInsertAsParent = false;

        public bool IsInsertUp
        {
            get => _isInsertUp;
            set
            {
                _isInsertUp = value;
                if (value) SetCommandFactory(ServiceProvider.GetRequiredService<InsertBeforeFactory>());
                RaiseAllButtonChanged();
            }
        }

        public bool IsInsertDown
        {
            get => _isInsertDown;
            set
            {
                _isInsertDown = value;
                if (value) SetCommandFactory(ServiceProvider.GetRequiredService<InsertAfterFactory>());
                RaiseAllButtonChanged();
            }
        }

        public bool IsInsertAsChild
        {
            get => _isInsertAsChild;
            set
            {
                _isInsertAsChild = value;
                if (value) SetCommandFactory(ServiceProvider.GetRequiredService<InsertAsChildFactory>());
                RaiseAllButtonChanged();
            }
        }

        public bool IsInsertAsParent
        {
            get => _isInsertAsParent;
            set
            {
                _isInsertAsParent = value;
                if (value) SetCommandFactory(ServiceProvider.GetRequiredService<InsertAsParentFactory>());
                RaiseAllButtonChanged();
            }
        }

        private void RaiseAllButtonChanged()
        {
            RaisePropertyChanged(nameof(IsInsertUp));
            RaisePropertyChanged(nameof(IsInsertDown));
            RaisePropertyChanged(nameof(IsInsertAsChild));
            RaisePropertyChanged(nameof(IsInsertAsParent));
        }

        private void SetCommandFactory(IInsertCommandFactory commandFactory)
        {
            ServiceProvider.GetRequiredService<InsertCommandHostingService>().InsertCommandFactory = commandFactory;
        }
    }
}
