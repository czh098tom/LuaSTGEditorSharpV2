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

        public bool ShouldUnpack { get; private set; } = false;

        public CompositeCommand(params CommandBase[] innerCommands)
            : this((IEnumerable<CommandBase>)innerCommands) { }

        public CompositeCommand(bool shouldUnpack, params CommandBase[] innerCommands)
            : this(innerCommands, shouldUnpack) { }

        public CompositeCommand(IEnumerable<CommandBase> innerCommands, bool shouldUnpack = false)
        {
            _commandsEnumerable = innerCommands;
            ShouldUnpack = shouldUnpack;
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

        public IReadOnlyList<CommandBase> Flatten()
        {
            if (ShouldUnpack) return [this];
            List<CommandBase> commands = [];
            foreach (CommandBase command in _innerCommands)
            {
                if (command is CompositeCommand cc && cc.ShouldUnpack)
                {
                    commands.AddRange(cc.Flatten());
                }
                else
                {
                    commands.Add(command);
                }
            }
            return commands;
        }
    }
}
