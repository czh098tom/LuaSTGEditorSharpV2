using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.ViewModel
{
    public class ViewModelContext : NodeContext
    {
        public ViewModelContext(LocalSettings localSettings) : base(localSettings)
        {
        }
    }
}
