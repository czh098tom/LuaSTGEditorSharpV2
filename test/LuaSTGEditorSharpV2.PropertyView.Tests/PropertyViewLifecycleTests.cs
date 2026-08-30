using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;

using Xunit;

namespace LuaSTGEditorSharpV2.PropertyView.Tests;

public class PropertyViewLifecycleTests
{
    [Fact]
    public void DisposingPageDisposesNestedTabTreeAndStopsEvents()
    {
        using var context = new TestContext();
        var item = context.CreateTrackingItem();
        var nestedTab = new PropertyTabViewModel();
        nestedTab.Properties.Add(item);
        var wrapper = new PropertyTabWrapperItemViewModel();
        wrapper.Initialize(
            [nestedTab],
            context.Node,
            context.LocalServiceParam,
            context.WizardProviderService);
        var rootTab = new PropertyTabViewModel();
        rootTab.Properties.Add(wrapper);
        var page = new PropertyPageViewModel(context.Services);
        page.Tabs.Add(rootTab);
        var publishedCommandCount = 0;
        page.OnCommandPublishing += (_, _) => publishedCommandCount++;

        context.Node.ChangeProperty("value", "before-dispose");
        item.RaiseOnEdit(new EditResult(context.LocalServiceParam));

        Assert.Equal(1, item.SourceChangeCount);
        Assert.Equal(1, publishedCommandCount);

        page.Dispose();
        context.Node.ChangeProperty("value", "after-dispose");
        item.RaiseOnEdit(new EditResult(context.LocalServiceParam));

        Assert.Empty(page.Tabs);
        Assert.Empty(rootTab.Properties);
        Assert.Empty(wrapper.Tabs);
        Assert.Empty(nestedTab.Properties);
        Assert.Equal(1, item.SourceChangeCount);
        Assert.Equal(1, publishedCommandCount);
    }

    private sealed class TrackingPropertyItemViewModel : PropertyItemViewModelBase
    {
        public int SourceChangeCount { get; private set; }

        protected override void HandleEditorNodeOnPropertyChanged(
            object? sender,
            EditorNodePropertyChangedEventArgs e)
        {
            SourceChangeCount++;
        }
    }

    private sealed class TestContext : IDisposable
    {
        private readonly EditorDocument _document;

        public TestContext()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<PropertyEditWizardProviderService>();
            services.AddSingleton<EditorNodeFactory>();
            Services = services.BuildServiceProvider();

            var model = DocumentModel.CreateEmpty("property-view-lifecycle-tests.lstges");
            var nodeData = new NodeData("TestType");
            nodeData.Properties.Add("value", "initial");
            model.Root.Add(nodeData);
            _document = new EditorDocument(model, Services.GetRequiredService<EditorNodeFactory>());
            Node = Assert.Single(
                _document.RootEditorNode.Children,
                node => ReferenceEquals(node.Source, nodeData));
            LocalServiceParam = new LocalServiceParam(_document);
        }

        public ServiceProvider Services { get; }
        public EditorNode Node { get; }
        public LocalServiceParam LocalServiceParam { get; }
        public PropertyEditWizardProviderService WizardProviderService
            => Services.GetRequiredService<PropertyEditWizardProviderService>();

        public TrackingPropertyItemViewModel CreateTrackingItem()
        {
            var item = new TrackingPropertyItemViewModel();
            item.Initialize([Node], LocalServiceParam, WizardProviderService);
            return item;
        }

        public void Dispose()
        {
            _document.Dispose();
            Services.Dispose();
        }
    }
}
