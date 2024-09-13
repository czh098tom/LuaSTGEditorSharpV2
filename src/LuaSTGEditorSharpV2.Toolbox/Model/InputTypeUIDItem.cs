using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Input;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command.Service;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Toolbox.ViewModel;
using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2.Toolbox.Model
{
    public class InputTypeUIDItem(IServiceProvider serviceProvider) : ToolboxItemModelBase(serviceProvider)
    {
        public override ToolboxItemViewModel CreateViewModel()
        {
            var vm = base.CreateViewModel();
            vm.NodeCreating += (o, e) =>
            {
                InputDialog inputDialog = new()
                {
                    Owner = WindowHelper.GetMainWindow()
                };
                if (inputDialog.ShowDialog() == true)
                {
                    e.CreatedData = [new NodeData(inputDialog.ViewModel.Text)];
                }
            };
            return vm;
        }
    }
}
