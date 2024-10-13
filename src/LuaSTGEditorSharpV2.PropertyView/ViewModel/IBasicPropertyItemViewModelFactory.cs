using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel
{
    public interface IBasicPropertyItemViewModelFactory<TResult>
        where TResult : BasicPropertyItemViewModel
    {
        public TResult Create(NodeData nodeData, LocalServiceParam localServiceParam, string? key);
    }
}
