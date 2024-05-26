using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class JsonProxiedPropertyItemViewModel<T> : PropertyItemViewModelBase
        where T : class
    {
        private T _proxyValue;

        public T ProxyValue
        {
            get => _proxyValue;
            set
            {
                _proxyValue = value;
                Value = JsonConvert.SerializeObject(_proxyValue);
            }
        }

        public JsonProxiedPropertyItemViewModel(T proxyValue, NodeData nodeData, 
            LocalServiceParam localServiceParam) : base(nodeData, localServiceParam)
        {
            _proxyValue = proxyValue;
        }
    }
}
