using LuaSTGEditorSharpV2.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public class CompositeCommand : CommandBase
    {
        private readonly IEnumerable<CommandBase> _commandsEnumerable;
        private readonly List<CommandBase> _innerCommands = [];

        public CompositeCommand(params CommandBase[] innerCommands)
            : this((IEnumerable<CommandBase>)innerCommands) { }

        public CompositeCommand(IEnumerable<CommandBase> innerCommands)
        {
            _commandsEnumerable = innerCommands;
        }

        protected override void DoExecute(EditorDocument editorDocument)
        {
            _innerCommands.Clear();
            foreach (CommandBase command in _commandsEnumerable)
            {
                command.Execute(editorDocument);
                _innerCommands.Add(command);
            }
        }

        protected override void RevertExecution(EditorDocument editorDocument)
        {
            for (int i = _innerCommands.Count - 1; i >= 0; i--)
            {
                _innerCommands[i].Revert(editorDocument);
            }
            _innerCommands.Clear();
        }
    }
}
