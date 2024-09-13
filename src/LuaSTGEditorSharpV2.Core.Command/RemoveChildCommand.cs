using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class RemoveChildCommand : ConcreteCommand
    {
        public NodeData Parent { get; private set; }
        public int Position { get; private set; }

        private NodeData? child;

        public RemoveChildCommand(ViewModelProviderServiceProvider service, NodeData parent, int position) : base(service)
        {
            Parent = parent;
            Position = position;
        }

        protected override void DoExecute(LocalServiceParam param)
        {
            child = Parent.PhysicalChildren[Position];
            ViewModelProviderServiceProvider
                .RemoveNodeAt(Parent, Position);
        }

        protected override void RevertExecution(LocalServiceParam param)
        {
            if (child == null) throw new InvalidOperationException("Command has not been executed yet.");
            ViewModelProviderServiceProvider
                .InsertNodeAt(Parent, Position, child, param);
        }
    }
}
