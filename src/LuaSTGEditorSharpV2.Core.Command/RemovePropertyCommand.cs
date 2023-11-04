using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class RemovePropertyCommand : CommandBase
    {
        public NodeData Node { get; private set; }
        public string PropertyName { get; private set; }

        string? _beforeEdit;

        public RemovePropertyCommand(NodeData node, string propertyName)
        {
            Node = node;
            PropertyName = propertyName;
        }

        protected override void DoExecute(LocalServiceParam param)
        {
            _beforeEdit = Node.Properties[PropertyName];
            Node.Properties.Remove(PropertyName);
            ViewModelProviderServiceBase.UpdateViewModelDataRecursive(Node, param);
        }

        protected override void RevertExecution(LocalServiceParam param)
        {
            if (_beforeEdit == null) throw new InvalidOperationException("Command has not been executed yet.");
            Node.Properties.Add(PropertyName, _beforeEdit);
            ViewModelProviderServiceBase.UpdateViewModelDataRecursive(Node, param);
        }
    }
}
