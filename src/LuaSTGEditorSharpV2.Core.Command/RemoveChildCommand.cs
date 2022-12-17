using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.ViewModel;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class RemoveChildCommand : CommandBase
    {
        public NodeData Parent { get; private set; }
        public int Position { get; private set; }

        private NodeData? child;

        public RemoveChildCommand(NodeData parent, int position)
        {
            Parent = parent;
            Position = position;
        }

        protected override void DoExecute()
        {
            child = Parent.PhysicalChildren[Position];
            ViewModelProviderServiceBase.RemoveNodeAt(Parent, Position);
        }

        protected override void RevertExecution()
        {
            if (child == null) throw new InvalidOperationException("Command has not been executed yet.");
            ViewModelProviderServiceBase.AddNodeAt(Parent, Position, child);
        }
    }
}
