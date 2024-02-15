using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Toolbox.ViewModel
{
    public class ToolboxItemViewModel : ViewModelBase
    {
        public class NodeCreateEventArgs : EventArgs
        {
            public bool CanCreate { get; set; } = false;
            public NodeData[] CreatedData { get; set; } = [];
        }

        private string _caption = string.Empty;
        public string Caption
        {
            get => _caption;
            set
            {
                _caption = value;
                RaisePropertyChanged();
            }
        }

        private string _iconSource = string.Empty;
        public string IconSource
        {
            get => _iconSource;
            set
            {
                _iconSource = value;
                RaisePropertyChanged();
            }
        }

        private ICommand? _insertCommand = null;
        public ICommand? InsertCommand
        {
            get => _insertCommand;
        }

        public bool HasCommand => NodeCreating != null;

        private ObservableCollection<ToolboxItemViewModel> _children = [];
        public ObservableCollection<ToolboxItemViewModel> Children
        {
            get => _children;
            set
            {
                _children = value;
                RaisePropertyChanged();
            }
        }

        public event EventHandler<NodeCreateEventArgs>? NodeCreated;
        public event EventHandler<NodeCreateEventArgs>? NodeCanBeCreated;
        public event EventHandler<NodeCreateEventArgs>? NodeCreating;

        public ToolboxItemViewModel()
        {
            _insertCommand = new RelayCommand(() =>
            {
                if (!HasCommand) return;
                var args = new NodeCreateEventArgs();
                NodeCanBeCreated?.Invoke(this, args);
                if (!args.CanCreate) return;
                NodeCreating?.Invoke(this, args);
                if (args.CreatedData.Length <= 0) return;
                NodeCreated?.Invoke(this, args);
            }, () => HasCommand);
        }

        public void RaiseNodeCreated(NodeCreateEventArgs args)
        {
            NodeCreated?.Invoke(this, args);
        }
    }
}
