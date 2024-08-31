using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class AddChildCommand : ConcreteCommand
    {
        public NodeData Parent { get; private set; }
        public NodeData Child { get; private set; }
        public int Position { get; private set; }

        public AddChildCommand(ViewModelProviderServiceProvider service, NodeData parent, int position, NodeData child)
            : base(service)
        {
            Parent = parent;
            Child = child.DeepClone();
            Position = position;
        }

        protected override void DoExecute(LocalServiceParam param)
        {
            ViewModelProviderServiceProvider
                .InsertNodeAt(Parent, Position, Child, param);
        }

        protected override void RevertExecution(LocalServiceParam param)
        {
            ViewModelProviderServiceProvider
                .RemoveNodeAt(Parent, Position);
        }
    }
}
