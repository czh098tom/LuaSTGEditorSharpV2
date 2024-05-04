using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class OnEnableHandleRequestedEventArgs : EventArgs
    {
        private readonly List<IDisposable> _disposables = [];
        public IEnumerable<IDisposable> Disposables => _disposables;

        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }
    }
}
