using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command.Tests.Fakes;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Command.Tests;

public class CompositeCommandTests
{
    [Fact]
    public void PartialFailure_RevertsExecutedInReverseOrder()
    {
        var a = new RecordingCommand();
        var b = new RecordingCommand();
        var c = new RecordingCommand
        {
            ExceptionToThrow = new CommandExecutionException()
        };
        var composite = new CompositeCommand(a, b, c);

        Assert.Throws<CommandExecutionException>(() => composite.Execute(null!));

        Assert.Equal(1, a.ExecuteCallCount);
        Assert.Equal(1, b.ExecuteCallCount);
        Assert.Equal(1, c.ExecuteCallCount);
        Assert.Equal(1, a.RevertCallCount);
        Assert.Equal(1, b.RevertCallCount);
        Assert.Equal(0, c.RevertCallCount);
        Assert.False(composite.Executed);
    }
}
