using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public abstract class CommandBase
    {
        public bool Executed { get; private set; } = false;

        protected abstract void DoExecute(LocalServiceParam param);
        protected abstract void RevertExecution(LocalServiceParam param);

        public void Execute(LocalServiceParam param)
        {
            if (Executed) throw new InvalidOperationException("Command has already been executed.");
            DoExecute(param);
            Executed = true;
        }

        public void Revert(LocalServiceParam param)
        {
            if (!Executed) throw new InvalidOperationException("Command has not been executed yet.");
            RevertExecution(param);
            Executed = false;
        }
    }
}
