using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public abstract class CommandBase
    {
        public bool Executed { get; private set; } = false;

        protected abstract void DoExecute();
        protected abstract void RevertExecution();

        public void Execute()
        {
            if (Executed) throw new InvalidOperationException("Command has already been executed.");
            DoExecute();
            Executed = true;
        }

        public void Revert()
        {
            if (!Executed) throw new InvalidOperationException("Command has not been executed yet.");
            RevertExecution();
            Executed = false;
        }
    }
}
