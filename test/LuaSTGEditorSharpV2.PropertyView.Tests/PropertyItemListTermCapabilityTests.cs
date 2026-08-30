using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

using Xunit;

namespace LuaSTGEditorSharpV2.PropertyView.Tests;

public class PropertyItemListTermCapabilityTests
{
    [Fact]
    public void SingleSourceTermDoesNotExposeMultiSourceCapability()
    {
        IPropertyItemListTerm term = new SingleSourcePropertyItemListTerm();

        Assert.IsNotAssignableFrom<IMultiSourcePropertyItemListTerm>(term);
    }

    [Fact]
    public void MultiSourceBaseNaturallyLiftsSingleSourceCalls()
    {
        var term = new TestMultiSourcePropertyItemListTerm();
        EditorNode node = null!;

        term.GetViewModels(node, null!, 3);

        Assert.IsAssignableFrom<IMultiSourcePropertyItemListTerm>(term);
        Assert.NotNull(term.CapturedNodes);
        Assert.Single(term.CapturedNodes);
        Assert.Equal(3, term.CapturedCount);
    }

    private sealed class SingleSourcePropertyItemListTerm : IPropertyItemListTerm
    {
        public IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
            EditorNode nodeData,
            PropertyViewContext context,
            int count)
            => [];
    }

    private sealed class TestMultiSourcePropertyItemListTerm()
        : PropertyItemListTermBase(new EmptyServiceProvider())
    {
        public IReadOnlyList<EditorNode>? CapturedNodes { get; private set; }
        public int CapturedCount { get; private set; }

        public override IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
            IReadOnlyList<EditorNode> nodes,
            PropertyViewContext context,
            int count)
        {
            CapturedNodes = nodes;
            CapturedCount = count;
            return [];
        }
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
