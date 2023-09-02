using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public class ServiceExtraSettings<T> where T : ServiceExtraSettings<T>, new()
    {
        private static Lazy<T> _instance = new(() => new());

        public static T Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public static void Reassign(T source)
        {
            _instance = new(() => source);
        }
    }

    internal class DefaultServiceExtraSettings : ServiceExtraSettings<DefaultServiceExtraSettings> { }
}
