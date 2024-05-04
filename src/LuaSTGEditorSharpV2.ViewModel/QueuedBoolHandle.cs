using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class QueuedBoolHandle(
        ViewModelBase viewModel, 
        string propertyName, 
        bool normalValue = true)
    {
        private class ReleaseDisposable(QueuedBoolHandle handle) : IDisposable
        {
            private readonly QueuedBoolHandle _handle = handle;

            private readonly object _lock = new();
            private bool _disposed = false;

            public void Dispose()
            {
                lock (_lock)
                {
                    if (_disposed) return;
                    _handle.RemoveHandle();
                    _disposed = true;
                }
            }
        }

        private readonly ViewModelBase _viewModel = viewModel;
        private readonly string _propertyName = propertyName;
        private readonly bool _normalValue = normalValue;

        private readonly object _lock = new();
        private int _level = 0;

        private bool _value = normalValue;
        public bool Value 
        {
            get => _value;
            private set
            {
                _value = value;
                _viewModel.RaisePropertyChanged(_propertyName);
            }
        }

        public IDisposable RequestNonNormalState()
        {
            AddHandle();
            return new ReleaseDisposable(this);
        }

        private void AddHandle()
        {
            lock (_lock)
            {
                if (_level == 0)
                {
                    Value = !_normalValue;
                }
                _level++;
            }
        }

        private void RemoveHandle()
        {
            lock (_lock)
            {
                _level--;
                if (_level == 0)
                {
                    Value = _normalValue;
                }
            }
        }
    }
}
