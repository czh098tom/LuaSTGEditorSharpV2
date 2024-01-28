using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class CommandBuffer
    {
        private readonly List<CommandBase> _commands = [];
        private int _currentCount = 0;
        private int _savedCount = 0;

        public bool CanUndo => _currentCount != 0;
        public bool CanRedo => _currentCount != _commands.Count;
        public bool IsModified => _currentCount != _savedCount;

        public void Execute(CommandBase command, LocalServiceParam param)
        {
            while (_commands.Count > _currentCount)
            {
                _commands.RemoveAt(_currentCount);
            }
            command.Execute(param);
            if (_commands.Count == _currentCount)
            {
                if (command is CompositeCommand cc && cc.ShouldUnpack)
                {
                    _commands.AddRange(cc.Flatten());
                }
                else
                {
                    _commands.Add(command);
                }
            }
            _currentCount = _commands.Count;
        }

        public void Undo(LocalServiceParam param)
        {
            _currentCount--;
            _commands[_currentCount].Revert(param);
        }

        public void Redo(LocalServiceParam param)
        {
            _currentCount++;
            _commands[_currentCount].Execute(param);
        }

        public void Save()
        {
            _savedCount = _currentCount;
        }
    }
}
