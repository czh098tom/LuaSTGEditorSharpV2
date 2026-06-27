using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor
{
    public interface IContextualValueEditorViewModel<T>
    {
        public T RawValue { get; set; }
    }
}
