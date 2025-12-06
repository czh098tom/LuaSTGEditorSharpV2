using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel
{
    public interface IBasicPropertyItemViewModelFactory<TResult>
        where TResult : BasicPropertyItemViewModel
    {
        public TResult Create(IReadOnlyList<EditorNode> nodeData, string? key, BatchEditStatus isBatchSame, LocalServiceParam localServiceParam);
    }
}
