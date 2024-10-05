using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public abstract class PropertyItemViewModelBase(NodeData nodeData, LocalServiceParam localServiceParam) : ViewModelBase
    {
        private string _value = string.Empty;
        private PropertyViewEditorType? _type;

        public string Value
        {
            get => _value;
            set
            {
                var oldValue = _value;
                _value = value;
                RaisePropertyChanged();
                OnEdit?.Invoke(this, ResolveEditingNodeCommand(SourceNode, LocalServiceParam, value));
            }
        }

        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                RaisePropertyChanged();
            }
        }

        public PropertyViewEditorType? Type
        {
            get => _type;
            set
            {
                _type = value;
                RaisePropertyChanged();
            }
        }

        public NodeData SourceNode { get; private init; } = nodeData;
        public LocalServiceParam LocalServiceParam { get; private init; } = localServiceParam;

        public event EventHandler<EditResult>? OnEdit;

        protected void RaiseOnEdit(EditResult editResult)
        {
            OnEdit?.Invoke(this, editResult);
        }

        public abstract EditResult ResolveEditingNodeCommand(NodeData nodeData,
            LocalServiceParam context, string edited);
    }
}
