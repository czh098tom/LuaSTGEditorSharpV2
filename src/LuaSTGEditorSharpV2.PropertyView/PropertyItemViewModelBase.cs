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
    public class PropertyItemViewModelBase(NodeData nodeData, LocalServiceParam localServiceParam) : ViewModelBase
    {
        public class ValueUpdatedEventArgs(
            string old, 
            string @new, 
            NodeData nodeData, 
            LocalServiceParam localServiceParam) : EventArgs
        {
            public string OldValue { get; private set; } = old;
            public string NewValue { get; private set; } = @new;
            public NodeData NodeData { get; private set; } = nodeData;
            public LocalServiceParam LocalServiceParam { get; private set; } = localServiceParam;
        }

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
                OnValueUpdated?.Invoke(this, new ValueUpdatedEventArgs(oldValue, value, SourceNode, LocalServiceParam));
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

        public event EventHandler<ValueUpdatedEventArgs>? OnValueUpdated;
        public event EventHandler<ItemValueUpdatedEventArgs>? OnItemValueUpdatedRaw;

        protected void RaiseOnItemValueUpdatedRawEvent(ItemValueUpdatedEventArgs e)
        {
            OnItemValueUpdatedRaw?.Invoke(this, e);
        }
    }
}
