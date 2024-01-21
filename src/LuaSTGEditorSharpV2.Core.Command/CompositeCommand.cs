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

        public IReadOnlyList<CommandBase> InnerCommands => _innerCommands;
        public bool ShouldUnpack { get; private set; } = false;

        public CompositeCommand(params CommandBase[] innerCommands) 
            : this((IReadOnlyList<CommandBase>)innerCommands) { }

        public CompositeCommand(bool shouldUnpack, params CommandBase[] innerCommands)
            : this(innerCommands, shouldUnpack) { }

        public CompositeCommand(IReadOnlyList<CommandBase> innerCommands, bool shouldUnpack = false)
        {
            _innerCommands = new(innerCommands);
            ShouldUnpack = shouldUnpack;
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
