using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command.Tests.Fakes;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Command.Tests;

public class CommandsTests
{
    [Fact]
    public void FromFilteredList_Null_ReturnsNull()
    {
        Assert.Null(Commands.FromFilteredList(null));
    }

    [Fact]
    public void FromFilteredList_Empty_ReturnsNull()
    {
        Assert.Null(Commands.FromFilteredList([]));
    }

    [Fact]
    public void FromFilteredList_Single_ReturnsRawCommand()
    {
        var single = new RecordingCommand();

        var result = Commands.FromFilteredList([single]);

        Assert.Same(single, result);
        Assert.IsNotType<CompositeCommand>(result);
    }

    [Fact]
    public void FromFilteredList_Multiple_ReturnsCompositeCommand()
    {
        var a = new RecordingCommand();
        var b = new RecordingCommand();

        var result = Commands.FromFilteredList([a, b]);

        Assert.NotNull(result);
        Assert.IsType<CompositeCommand>(result);
    }
}
