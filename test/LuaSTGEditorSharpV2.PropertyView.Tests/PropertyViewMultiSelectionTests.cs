using System.Globalization;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.PropertyView.Converter;
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
    public void CodeWizardEditCreatesCommandForEverySource()
    {
        var dialogService = new TestCodeEditDialogService("changed");
        using var environment = new TestEnvironment(dialogService);
        using var document = environment.CreateDocument(
            CreateNode("TestType", ("value", "same")),
            CreateNode("TestType", ("value", "same")));
        var nodes = GetSelectedNodes(document, "TestType");
        var term = CreatePropertyTerm(environment.Services, "value");
        SetProperty(
            term,
            nameof(PropertyItemTermBase.Editor),
            new PropertyViewEditorType("code"));

        using var registration = environment.Register(
            "TestType",
            CreateCommonTab(environment, term));

        var tabs = environment.PropertyViewProvider.GetPropertyViewModelOfMultipleNodes(
            nodes,
            new LocalServiceParam(document));
        var viewModel = Assert.IsType<BasicPropertyItemViewModel>(Assert.Single(tabs[0].Properties));
        EditResult? editResult = null;
        viewModel.OnEdit += (_, result) => editResult = result;

        viewModel.ShowEditWindow!.Execute(null);

        Assert.NotNull(editResult?.Command);
        document.ExecuteCommand(editResult!.Command!);
        Assert.All(nodes, node => Assert.Equal("changed", node.Source.Properties["value"]));
        Assert.Equal("same", dialogService.InitialValue);
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
        Assert.True(viewModel.ValueProperty.HasConflict);
        Assert.Equal(string.Empty, viewModel.ValueProperty.Value);

        EditResult? editResult = null;
        viewModel.OnEdit += (_, result) => editResult = result;
        viewModel.Value = "shared";

        Assert.NotNull(editResult?.Command);
        document.ExecuteCommand(editResult!.Command!);
        Assert.All(nodes, node => Assert.Equal("shared", node.Source.Properties["value"]));
        Assert.False(viewModel.ValueProperty.HasConflict);
        Assert.Equal("shared", viewModel.ValueProperty.Value);
    }

    [Fact]
    public void ExternalSourceChangesRefreshValueAndConflictNotifications()
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
        var viewModel = Assert.IsType<BasicPropertyItemViewModel>(Assert.Single(tabs[0].Properties));
        var changedProperties = new List<string?>();
        viewModel.ValueProperty.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        nodes[0].ChangeProperty("value", "different");

        Assert.True(viewModel.ValueProperty.HasConflict);
        Assert.Equal(string.Empty, viewModel.ValueProperty.Value);
        Assert.Contains(nameof(BoundProperty.HasConflict), changedProperties);
        Assert.Contains(nameof(BoundProperty.Value), changedProperties);

        changedProperties.Clear();
        nodes[1].ChangeProperty("value", "different");

        Assert.False(viewModel.ValueProperty.HasConflict);
        Assert.Equal("different", viewModel.ValueProperty.Value);
        Assert.Contains(nameof(BoundProperty.HasConflict), changedProperties);
        Assert.Contains(nameof(BoundProperty.Value), changedProperties);
    }

    [Fact]
    public void BooleanEditorValueIsIndeterminateDuringConflict()
    {
        using var environment = new TestEnvironment();
        using var document = environment.CreateDocument(
            CreateNode("TestType", ("value", "true")),
            CreateNode("TestType", ("value", "false")));
        var nodes = GetSelectedNodes(document, "TestType");

        using var registration = environment.Register(
            "TestType",
            CreateCommonTab(environment, CreatePropertyTerm(environment.Services, "value")));

        var tabs = environment.PropertyViewProvider.GetPropertyViewModelOfMultipleNodes(
            nodes,
            new LocalServiceParam(document));
        var viewModel = Assert.IsType<BasicPropertyItemViewModel>(Assert.Single(tabs[0].Properties));

        var converter = new BooleanConflictConverter();
        Assert.Null(converter.Convert(
            [viewModel.ValueProperty.Value, viewModel.ValueProperty.HasConflict],
            typeof(bool?),
            null!,
            CultureInfo.InvariantCulture));

        EditResult? editResult = null;
        viewModel.OnEdit += (_, result) => editResult = result;
        var convertedValues = converter.ConvertBack(
            true,
            [typeof(string), typeof(bool)],
            null!,
            CultureInfo.InvariantCulture);
        viewModel.ValueProperty.Value = Assert.IsType<string>(convertedValues[0]);

        Assert.NotNull(editResult?.Command);
        document.ExecuteCommand(editResult!.Command!);
        Assert.All(nodes, node => Assert.Equal("true", node.Source.Properties["value"]));
        Assert.False(viewModel.ValueProperty.HasConflict);
        Assert.Equal("true", viewModel.ValueProperty.Value);
    }

    [Fact]
    public void ChildRowsKeepTheirPositionAsPlaceholderAndSingleListTabsUseMultiSourceFlow()
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

        Assert.Equal(3, multipleTabs.Count);
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
        var multipleCount = Assert.IsType<BasicPropertyItemViewModel>(
            Assert.Single(multipleTabs[1].Properties));
        Assert.Equal(nodes, multipleCount.SourceNodes);

        var singleTabs = environment.PropertyViewProvider.GetPropertyViewModelOfNode(
            nodes[0],
            new LocalServiceParam(document));

        Assert.Equal(3, singleTabs.Count);
        Assert.IsType<PropertyTabWrapperItemViewModel>(singleTabs[0].Properties[1]);
        Assert.IsType<BasicPropertyItemViewModel>(Assert.Single(singleTabs[1].Properties));
    }

    [Fact]
    public void SameCountSingleListSelectionCreatesSharedIndexedRows()
    {
        using var environment = new TestEnvironment();
        using var document = environment.CreateDocument(
            CreateNode("TestType", ("count", "2"), ("item_0", "a"), ("item_1", "b")),
            CreateNode("TestType", ("count", "2"), ("item_0", "a"), ("item_1", "b")));
        var nodes = GetSelectedNodes(document, "TestType");

        using var registration = environment.Register(
            "TestType",
            CreateSingleListTab(environment, "count"));

        var tabs = environment.PropertyViewProvider.GetPropertyViewModelOfMultipleNodes(
            nodes,
            new LocalServiceParam(document));

        Assert.Equal(2, tabs.Count);
        Assert.Collection(
            tabs[0].Properties,
            count => Assert.IsType<BasicPropertyItemViewModel>(count),
            first => Assert.IsType<BasicPropertyItemViewModel>(first),
            second => Assert.IsType<BasicPropertyItemViewModel>(second));

        var secondItem = Assert.IsType<BasicPropertyItemViewModel>(tabs[0].Properties[2]);
        Assert.Equal(nodes, secondItem.SourceNodes);
        EditResult? editResult = null;
        secondItem.OnEdit += (_, result) => editResult = result;

        secondItem.Value = "changed";

        Assert.NotNull(editResult?.Command);
        document.ExecuteCommand(editResult!.Command!);
        Assert.All(nodes, node => Assert.Equal("changed", node.Source.Properties["item_1"]));
    }

    [Fact]
    public void DifferentCountSingleListSelectionKeepsOnlyConflictedCount()
    {
        using var environment = new TestEnvironment();
        using var document = environment.CreateDocument(
            CreateNode("TestType", ("count", "1"), ("item_0", "a")),
            CreateNode("TestType", ("count", "2"), ("item_0", "a"), ("item_1", "b")));
        var nodes = GetSelectedNodes(document, "TestType");

        using var registration = environment.Register(
            "TestType",
            CreateSingleListTab(environment, "count"));

        var tabs = environment.PropertyViewProvider.GetPropertyViewModelOfMultipleNodes(
            nodes,
            new LocalServiceParam(document));

        Assert.Equal(2, tabs.Count);
        var count = Assert.IsType<BasicPropertyItemViewModel>(Assert.Single(tabs[0].Properties));
        Assert.True(count.ValueProperty.HasConflict);
        Assert.Equal(nodes, count.SourceNodes);
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
            environment.PropertyViewProvider,
            environment.Services.GetRequiredService<DefaultValueServiceProvider>());
        SetProperty(tab, nameof(SingleListTabTerm<TestPropertyItemListTerm>.Count),
            CreatePropertyTerm(environment.Services, countKey));
        SetProperty(tab, nameof(SingleListTabTerm<TestPropertyItemListTerm>.VariableProperty),
            new TestPropertyItemListTerm(environment.Services));
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

    private sealed class TestPropertyItemListTerm(IServiceProvider services)
        : PropertyItemListTermBase(services)
    {
        public override IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
            IReadOnlyList<EditorNode> nodes,
            PropertyViewContext context,
            int count)
            => Enumerable.Range(0, count)
                .Select(index => CreatePropertyTerm(ServiceProvider, $"item_{index}")
                    .GetViewModel(nodes, context))
                .ToArray();
    }

    private sealed class TestEnvironment : IDisposable
    {
        private readonly CultureInfo _previousCulture = CultureInfo.CurrentUICulture;
        private readonly IDisposable? _codeWizardRegistration;

        public TestEnvironment(ICodeEditDialogService? codeEditDialogService = null)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<LocalizationService>();
            services.AddSingleton<DefaultValueServiceProvider>();
            services.AddSingleton<PropertyEditWizardProviderService>();
            services.AddSingleton(typeof(IPropertyItemViewModelFactory<,>),
                typeof(PropertyItemViewModelFactory<,>));
            services.AddSingleton<EditorNodeFactory>();
            services.AddSingleton<PropertyViewServiceProvider>();
            if (codeEditDialogService is not null)
            {
                services.AddSingleton(codeEditDialogService);
            }
            Services = services.BuildServiceProvider();

            if (codeEditDialogService is not null)
            {
                var codeWizard = Assert.Single(
                    new PropertyEditWizardRegisterer().GetServiceInstances(Services),
                    wizard => wizard.Name == "code");
                _codeWizardRegistration = Services
                    .GetRequiredService<PropertyEditWizardProviderService>()
                    .Register(
                        "code",
                        new PackageInfo(PackageManifest.CORE, string.Empty),
                        codeWizard);
            }

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
            _codeWizardRegistration?.Dispose();
            Services.GetRequiredService<LocalizationService>().SetUICulture(_previousCulture);
            Services.Dispose();
        }
    }
}
