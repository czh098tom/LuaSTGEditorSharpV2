using LuaSTGEditorSharpV2.Core.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class NodeViewModel : ViewModelBase
    {
        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                RaisePropertyChanged();
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                RaisePropertyChanged();
            }
        }

        public bool IsActivated
        {
            get => _isActivated;
            set
            {
                _isActivated = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<NodeViewModel> Children => _children;

        private string _icon = "";
        private string _text = "";
        private bool _isActivated = true;
        private readonly ObservableCollection<NodeViewModel> _children = new();

        public NodeData Source { get; private set; }

        public NodeViewModel(NodeData source)
        {
            Source = source;
        }
    }
}
