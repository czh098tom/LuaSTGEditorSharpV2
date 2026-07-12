using System;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.Core.Command.Tests.Fakes;

public class RecordingCommand : CommandBase
{
    public int ExecuteCallCount { get; private set; }
    public int RevertCallCount { get; private set; }
    public System.Exception? ExceptionToThrow { get; init; }

    protected override void DoExecute(EditorDocument editorDocument)
    {
        ExecuteCallCount++;
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }
    }

    protected override void RevertExecution(EditorDocument editorDocument)
    {
        RevertCallCount++;
    }
}
