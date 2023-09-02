using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public class ServiceExtraSettings<T> where T : ServiceExtraSettings<T>, new()
    {
        private static readonly Lazy<T> _instance = new(() => new());

        public static T Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }
}
