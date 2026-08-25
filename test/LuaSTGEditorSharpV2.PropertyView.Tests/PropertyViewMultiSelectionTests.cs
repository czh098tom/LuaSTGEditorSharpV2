using System.Globalization;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;

using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace LuaSTGEditorSharpV2.PropertyView.Tests;

public class PropertyViewMultiSelectionTests
{
    [Fact]
    public void OrdinaryTermsExposeMultiSourceCapabilityButStructuralTermsDoNot()
    {
        using var environment = new TestEnvironment();

        var ordinary = CreatePropertyTerm(environment.Services, "value");
        var child = new ChildPropertyTerm(
            environment.Services,
            environment.PropertyViewProvider,
            environment.EditorNodeFactory);
        var selector = new PorpertyTermSelector();

        Assert.IsAssignableFrom<IMultiSourcePropertyItemTerm>(ordinary);
        Assert.IsNotAssignableFrom<IMultiSourcePropertyItemTerm>(child);
        Assert.IsNotAssignableFrom<IMultiSourcePropertyItemTerm>(selector);
    }

    [Fact]
    public void SameTypeSelectionCreatesConfiguredCommonTabAndEditsEverySource()
    {
        using var environment = new TestEnvironment();
        using var document = environment.CreateDocument(
            CreateNode("TestType", ("value", "same")),
            CreateNode("TestType", ("value", "same")));
        var nodes = GetSelectedNodes(document, "TestType");

        using var registration = environment.Register(
            "TestType",
            CreateCommonTab(environment, CreatePropertyTerm(environment.Services, "value")));

        var tabs = environment.PropertyViewProvider.GetPropertyViewModelOfMultipleNodes(
            nodes,
            new LocalServiceParam(document));

        Assert.Equal(2, tabs.Count);
        var configuredTab = tabs[0];
        var viewModel = Assert.IsType<BasicPropertyItemViewModel>(Assert.Single(configuredTab.Properties));
        Assert.Equal("same", viewModel.Value);
        Assert.False(viewModel.ValueConflicted);
        Assert.Equal(nodes, viewModel.SourceNodes);

        EditResult? editResult = null;
        viewModel.OnEdit += (_, result) => editResult = result;
        viewModel.Value = "changed";

        Assert.NotNull(editResult?.Command);
        document.ExecuteCommand(editResult!.Command!);
        Assert.All(nodes, node => Assert.Equal("changed", node.Source.Properties["value"]));
    }

    [Fact]
    public void DifferentValuesUseExistingBindingConflictState()
    {
        using var environment = new TestEnvironment();
        using var document = environment.CreateDocument(
            CreateNode("TestType", ("value", "first")),
            CreateNode("TestType", ("value", "second")));
        var nodes = GetSelectedNodes(document, "TestType");

        using var registration = environment.Register(
            "TestType",
            CreateCommonTab(environment, CreatePropertyTerm(environment.Services, "value")));

        var tabs = environment.PropertyViewProvider.GetPropertyViewModelOfMultipleNodes(
            nodes,
            new LocalServiceParam(document));

        var viewModel = Assert.IsType<BasicPropertyItemViewModel>(Assert.Single(tabs[0].Properties));
        Assert.True(viewModel.ValueConflicted);
        Assert.Equal(string.Empty, viewModel.Value);
    }

    [Fact]
    public void ChildRowsKeepTheirPositionAsPlaceholderAndSingleListTabsAreHidden()
    {
        using var environment = new TestEnvironment();
        using var document = environment.CreateDocument(
            CreateNode("TestType", ("first", "a"), ("second", "b"), ("count", "0")),
            CreateNode("TestType", ("first", "a"), ("second", "b"), ("count", "0")));
        var nodes = GetSelectedNodes(document, "TestType");

        var child = new ChildPropertyTerm(
            environment.Services,
            environment.PropertyViewProvider,
            environment.EditorNodeFactory);
        var common = CreateCommonTab(
            environment,
            CreatePropertyTerm(environment.Services, "first"),
            child,
            CreatePropertyTerm(environment.Services, "second"));
        var list = CreateSingleListTab(environment, "count");
        using var registration = environment.Register("TestType", common, list);

        var multipleTabs = environment.PropertyViewProvider.GetPropertyViewModelOfMultipleNodes(
            nodes,
            new LocalServiceParam(document));

        Assert.Equal(2, multipleTabs.Count);
        Assert.Collection(
            multipleTabs[0].Properties,
            first => Assert.IsType<BasicPropertyItemViewModel>(first),
            placeholder =>
            {
                var unsupported = Assert.IsType<UnsupportedMultiSourcePropertyItemViewModel>(placeholder);
                Assert.False(unsupported.Enabled);
                Assert.Equal("captionValue", unsupported.Type?.Name);
                Assert.Equal("此属性区域不支持多选", unsupported.Value);
            },
            second => Assert.IsType<BasicPropertyItemViewModel>(second));

        var singleTabs = environment.PropertyViewProvider.GetPropertyViewModelOfNode(
            nodes[0],
            new LocalServiceParam(document));

        Assert.Equal(3, singleTabs.Count);
        Assert.IsType<PropertyTabWrapperItemViewModel>(singleTabs[0].Properties[1]);
        Assert.IsType<BasicPropertyItemViewModel>(Assert.Single(singleTabs[1].Properties));
    }

    [Fact]
    public void MixedTypeSelectionProducesOnlyNativeTab()
    {
        using var environment = new TestEnvironment();
        using var document = environment.CreateDocument(
            CreateNode("FirstType", ("value", "a")),
            CreateNode("SecondType", ("value", "b")));
        var nodes = document.RootEditorNode.Children
            .Where(node => node.Source.TypeUID is "FirstType" or "SecondType")
            .ToArray();

        using var registration = environment.Register(
            "FirstType",
            CreateCommonTab(environment, CreatePropertyTerm(environment.Services, "value")));

        var tabs = environment.PropertyViewProvider.GetPropertyViewModelOfMultipleNodes(
            nodes,
            new LocalServiceParam(document));

        var nativeTab = Assert.Single(tabs);
        Assert.Equal(environment.PropertyViewProvider.NativeViewI18NCaption, nativeTab.Caption);
    }

    [Fact]
    public void SingleSelectionChildRowsStillResolveChildPropertiesRecursively()
    {
        using var environment = new TestEnvironment();
        var childNode = CreateNode("ChildType", ("value", "child value"));
        var parentNode = CreateNode("ParentType");
        parentNode.Add(childNode);
        using var document = environment.CreateDocument(parentNode);
        var parentEditorNode = Assert.Single(
            document.RootEditorNode.Children,
            node => ReferenceEquals(node.Source, parentNode));

        using var childRegistration = environment.Register(
            "ChildType",
            CreateCommonTab(environment, CreatePropertyTerm(environment.Services, "value")));
        var childTerm = new ChildPropertyTerm(
            environment.Services,
            environment.PropertyViewProvider,
            environment.EditorNodeFactory);
        using var parentRegistration = environment.Register(
            "ParentType",
            CreateCommonTab(environment, childTerm));

        var tabs = environment.PropertyViewProvider.GetPropertyViewModelOfNode(
            parentEditorNode,
            new LocalServiceParam(document));

        var wrapper = Assert.IsType<PropertyTabWrapperItemViewModel>(Assert.Single(tabs[0].Properties));
        var childTab = Assert.Single(wrapper.Tabs);
        var childProperty = Assert.IsType<BasicPropertyItemViewModel>(Assert.Single(childTab.Properties));
        Assert.Equal("child value", childProperty.Value);
    }

    private static NodeData CreateNode(string typeUid, params (string Key, string Value)[] properties)
    {
        var node = new NodeData(typeUid);
        foreach (var (key, value) in properties)
        {
            node.Properties.Add(key, value);
        }
        return node;
    }

    private static EditorNode[] GetSelectedNodes(EditorDocument document, string typeUid)
        => document.RootEditorNode.Children
            .Where(node => node.Source.TypeUID == typeUid)
            .ToArray();

    private static PropertyItemTerm CreatePropertyTerm(IServiceProvider services, string key)
    {
        var term = new PropertyItemTerm(services);
        SetProperty(term, nameof(PropertyItemTerm.Mapping), NodePropertyCapture.FromKey(key));
        return term;
    }

    private static CommonPropertyTabTerm CreateCommonTab(
        TestEnvironment environment,
        params IPropertyItemTerm[] terms)
        => new(environment.Services, environment.PropertyViewProvider)
        {
            Mapping = terms,
        };

    private static SingleListTabTerm<TestPropertyItemListTerm> CreateSingleListTab(
        TestEnvironment environment,
        string countKey)
    {
        var tab = new SingleListTabTerm<TestPropertyItemListTerm>(
            environment.Services,
            environment.PropertyViewProvider);
        SetProperty(tab, nameof(SingleListTabTerm<TestPropertyItemListTerm>.Count),
            CreatePropertyTerm(environment.Services, countKey));
        SetProperty(tab, nameof(SingleListTabTerm<TestPropertyItemListTerm>.VariableProperty),
            new TestPropertyItemListTerm());
        return tab;
    }

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo? property = null;
        for (var type = target.GetType(); type != null && property == null; type = type.BaseType)
        {
            property = type.GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        }
        Assert.NotNull(property);
        var setter = property!.GetSetMethod(nonPublic: true);
        Assert.NotNull(setter);
        setter!.Invoke(target, [value]);
    }

    private sealed class TestPropertyItemListTerm : IPropertyItemListTerm
    {
        public IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
            EditorNode nodeData,
            PropertyViewContext context,
            int count)
            => [];

        public IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
            IReadOnlyList<EditorNode> nodes,
            PropertyViewContext context,
            int count)
            => [];
    }

    private sealed class TestEnvironment : IDisposable
    {
        private readonly CultureInfo _previousCulture = CultureInfo.CurrentUICulture;

        public TestEnvironment()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<LocalizationService>();
            services.AddSingleton<DefaultValueServiceProvider>();
            var tokenFactoryType = typeof(NodePropertyAccessToken).Assembly.GetType(
                "LuaSTGEditorSharpV2.Core.NodePropertyAccessTokenFactory");
            if (tokenFactoryType != null)
            {
                services.AddSingleton(tokenFactoryType);
            }
            services.AddSingleton<PropertyEditWizardProviderService>();
            services.AddSingleton(typeof(IPropertyItemViewModelFactory<,>),
                typeof(PropertyItemViewModelFactory<,>));
            services.AddSingleton<EditorNodeFactory>();
            services.AddSingleton<PropertyViewServiceProvider>();
            Services = services.BuildServiceProvider();

            Services.GetRequiredService<LocalizationService>()
                .SetUICulture(CultureInfo.GetCultureInfo("zh-CN"));
        }

        public ServiceProvider Services { get; }
        public PropertyViewServiceProvider PropertyViewProvider
            => Services.GetRequiredService<PropertyViewServiceProvider>();
        public EditorNodeFactory EditorNodeFactory
            => Services.GetRequiredService<EditorNodeFactory>();

        public EditorDocument CreateDocument(params NodeData[] nodes)
        {
            var model = DocumentModel.CreateEmpty("property-view-tests.lstges");
            foreach (var node in nodes)
            {
                model.Root.Add(node);
            }
            return new EditorDocument(model, EditorNodeFactory);
        }

        public IDisposable Register(string typeUid, params PropertyTabTermBase[] tabs)
        {
            var service = new ConfigurablePropertyViewService(PropertyViewProvider, Services);
            SetProperty(service, nameof(PropertyViewServiceBase.Tabs), tabs);
            return PropertyViewProvider.Register(
                typeUid,
                new PackageInfo(PackageManifest.CORE, string.Empty),
                service);
        }

        public void Dispose()
        {
            Services.GetRequiredService<LocalizationService>().SetUICulture(_previousCulture);
            Services.Dispose();
        }
    }
}
