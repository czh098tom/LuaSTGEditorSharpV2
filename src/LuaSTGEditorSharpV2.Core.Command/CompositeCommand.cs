using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class CompositeCommand : CommandBase
    {
        private readonly List<CommandBase> _innerCommands;

        public CompositeCommand(params CommandBase[] innerCommands) 
            : this((IReadOnlyList<CommandBase>)innerCommands) { }

        public CompositeCommand(IReadOnlyList<CommandBase> innerCommands)
        {
            _innerCommands = new(innerCommands);
        }

        protected override void DoExecute(LocalServiceParam param)
        {
            for (int i = 0; i < _innerCommands.Count; i++)
            {
                _innerCommands[i].Execute(param);
            }
        }

        protected override void RevertExecution(LocalServiceParam param)
        {
            for (int i = _innerCommands.Count - 1; i >= 0; i--)
            {
                _innerCommands[i].Revert(param);
            }
        }
    }
}
