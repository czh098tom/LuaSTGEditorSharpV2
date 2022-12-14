using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.ViewModel
{
    [ServiceShortName("vm")]
    public class ViewModelProviderService : NodeService<ViewModelProviderService, ViewModelContext>
    {
        static ViewModelProviderService()
        {
            _defaultServiceGetter = () => new ViewModelProviderService();
        }

        public override ViewModelContext GetEmptyContext(LocalSettings localSettings)
        {
            return new ViewModelContext(localSettings);
        }

        protected virtual void UpdateViewModel(NodeViewModel viewModel, NodeData dataSource) { }
    }
}
