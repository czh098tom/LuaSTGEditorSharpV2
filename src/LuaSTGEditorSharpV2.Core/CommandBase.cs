using LuaSTGEditorSharpV2.Core.Editor;
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

        protected abstract void DoExecute(EditorDocument editorDocument);
        protected abstract void RevertExecution(EditorDocument editorDocument);

        public void Execute(EditorDocument editorDocument)
        {
            if (Executed) throw new InvalidOperationException("Command has already been executed.");
            DoExecute(editorDocument);
            Executed = true;
        }

        public void Revert(EditorDocument editorDocument)
        {
            if (!Executed) throw new InvalidOperationException("Command has not been executed yet.");
            RevertExecution(editorDocument);
            Executed = false;
        }
    }
}
