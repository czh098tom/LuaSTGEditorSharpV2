using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.ViewModel;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class AddChildCommand : CommandBase
    {
        public NodeData Parent { get; private set; }
        public NodeData Child { get; private set; }
        public int Position { get; private set; }

        public AddChildCommand(NodeData parent, int position, NodeData child)
        {
            Parent = parent;
            Child = child;
            Position = position;
        }

        protected override void DoExecute()
        {
            ViewModelProviderServiceBase.InsertNodeAt(Parent, Position, Child);
        }

        protected override void RevertExecution()
        {
            ViewModelProviderServiceBase.RemoveNodeAt(Parent, Position);
        }
    }
}
