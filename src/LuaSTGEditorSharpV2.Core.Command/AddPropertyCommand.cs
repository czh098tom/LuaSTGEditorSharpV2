using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.ViewModel;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class AddPropertyCommand : CommandBase
    {
        public NodeData Node { get; private set; }
        public string PropertyName { get; private set; }
        public string Value { get; private set; }

        public AddPropertyCommand(NodeData node, string propertyName, string value)
        {
            Node = node;
            PropertyName = propertyName;
            Value = value;
        }

        protected override void DoExecute()
        {
            Node.Properties.Add(PropertyName, Value);
            ViewModelProviderServiceBase.UpdateViewModelDataRecursive(Node);
        }

        protected override void RevertExecution()
        {
            Node.Properties.Remove(PropertyName);
            ViewModelProviderServiceBase.UpdateViewModelDataRecursive(Node);
        }
    }
}
