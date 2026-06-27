using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command.Tests.Fakes;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Command.Tests;

public class CommandBaseTests
{
    [Fact]
    public void Execute_SetsExecutedToTrue()
    {
        var cmd = new RecordingCommand();

        cmd.Execute(null!);

        Assert.True(cmd.Executed);
        Assert.Equal(1, cmd.ExecuteCallCount);
    }

    [Fact]
    public void Execute_Twice_ThrowsInvalidOperationException()
    {
        var cmd = new RecordingCommand();
        cmd.Execute(null!);

        Assert.Throws<InvalidOperationException>(() => cmd.Execute(null!));
        Assert.Equal(1, cmd.ExecuteCallCount);
    }

    [Fact]
    public void Revert_WithoutExecute_ThrowsInvalidOperationException()
    {
        var cmd = new RecordingCommand();

        Assert.Throws<InvalidOperationException>(() => cmd.Revert(null!));
        Assert.Equal(0, cmd.RevertCallCount);
    }

    [Fact]
    public void Revert_AfterExecute_ResetsExecutedToFalse()
    {
        var cmd = new RecordingCommand();
        cmd.Execute(null!);

        cmd.Revert(null!);

        Assert.False(cmd.Executed);
        Assert.Equal(1, cmd.RevertCallCount);
    }
}
