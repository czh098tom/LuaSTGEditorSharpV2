using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class CommandBuffer
    {
        private readonly List<CommandBase> _commands = new();
        private int _currentIdx = 0;

        public bool CanUndo => _currentIdx != 0;
        public bool CanRedo => _currentIdx != _commands.Count;

        public void Execute(CommandBase command, LocalServiceParam param)
        {
            while (_commands.Count > _currentIdx + 1)
            {
                _commands.RemoveAt(_currentIdx);
            }
            if (_commands.Count == _currentIdx)
            {
                _commands.Add(command);
            }
            else
            {
                _commands[_currentIdx] = command;
            }
            command.Execute(param);
            _currentIdx++;
        }

        public void Undo(LocalServiceParam param)
        {
            _currentIdx--;
            _commands[_currentIdx].Revert(param);
        }

        public void Redo(LocalServiceParam param)
        {
            _currentIdx++;
            _commands[_currentIdx].Execute(param);
        }
    }
}
