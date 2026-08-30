using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command.Tests.Fakes;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Command.Tests;

public class CommandBufferTests
{
    [Fact]
    public void Execute_AppendsCommand_AndAdvancesCursor()
    {
        var buffer = new CommandBuffer(null!);
        var a = new RecordingCommand();
        var b = new RecordingCommand();
        var c = new RecordingCommand();

        buffer.Execute(a);
        buffer.Execute(b);
        buffer.Execute(c);

        Assert.True(buffer.CanUndo);
        Assert.False(buffer.CanRedo);
        Assert.True(buffer.IsModified);
        Assert.Equal(1, a.ExecuteCallCount);
        Assert.Equal(1, b.ExecuteCallCount);
        Assert.Equal(1, c.ExecuteCallCount);
    }

    [Fact]
    public void Undo_Redo_Roundtrip_RestoresCallSequence()
    {
        var buffer = new CommandBuffer(null!);
        var a = new RecordingCommand();
        var b = new RecordingCommand();
        var c = new RecordingCommand();
        buffer.Execute(a);
        buffer.Execute(b);
        buffer.Execute(c);

        buffer.Undo();
        buffer.Undo();
        Assert.Equal(1, c.RevertCallCount);
        Assert.Equal(1, b.RevertCallCount);

        buffer.Redo();
        Assert.Equal(2, b.ExecuteCallCount);

        Assert.True(buffer.CanUndo);
        Assert.True(buffer.CanRedo);
    }

    [Fact]
    public void Execute_AfterUndo_TruncatesRedoBranch()
    {
        var buffer = new CommandBuffer(null!);
        var a = new RecordingCommand();
        var b = new RecordingCommand();
        buffer.Execute(a);
        buffer.Execute(b);

        buffer.Undo();
        Assert.True(buffer.CanRedo);

        var d = new RecordingCommand();
        buffer.Execute(d);

        Assert.False(buffer.CanRedo);
        Assert.True(buffer.IsModified);
    }

    [Fact]
    public void Save_ResetsIsModified()
    {
        var buffer = new CommandBuffer(null!);
        buffer.Execute(new RecordingCommand());
        Assert.True(buffer.IsModified);

        buffer.Save();

        Assert.False(buffer.IsModified);
    }
}
