using System.Globalization;
using System.IO;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.PropertyView.Specialized.MultipleFile;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace LuaSTGEditorSharpV2.PropertyView.Tests;

public class MultipleFilePropertyItemTests
{
    private const string Configuration = """
        {
          "$type": "LuaSTGEditorSharpV2.PropertyView.Configurable.SingleListTabTerm`1[[LuaSTGEditorSharpV2.PropertyView.Specialized.MultipleFile.MultipleFilePropertyViewItemListTerm, LuaSTGEditorSharpV2.PropertyView]], LuaSTGEditorSharpV2.PropertyView",
          "Caption": { "Neutral": "Files" },
          "Count": {
            "$type": "MultipleFile",
            "Mapping": { "Key": "count", "DefaultValue": "0" }
          },
          "VariableProperty": {}
        }
        """;

    [Fact]
    public void SingleListTabLoadsShortNamedCountAndFactoryCreatedEntries()
    {
        using var fixture = new Fixture(CreateNode(("assets/one.png", "custom/one.png")));

        Assert.IsType<MultipleFileCountPropertyItemTerm>(fixture.Term.Count);
        Assert.Equal("multiplefile", fixture.Toolbar.Type!.Name);
        Assert.Equal(2, fixture.Tab.Properties.Count);
        var row = Assert.Single(fixture.Entries);
        Assert.Equal("multiplefileEntry", row.Type!.Name);
        Assert.Equal("assets/one.png", row.SourceProperty.Value);
        Assert.Equal("custom/one.png", row.DestinationProperty.Value);
        Assert.Equal(fixture.Nodes, row.SourceNodes);
        Assert.True(fixture.Toolbar.CanEdit);
    }

    [Fact]
    public void MissingCountDefaultsToEmptyWithoutWritingOrScanningKeys()
    {
        var node = new NodeData("MultipleFileTest");
        node.Properties["source_9"] = "not-an-active-entry.png";
        using var fixture = new Fixture(node);

        Assert.Empty(fixture.Entries);
        Assert.True(fixture.Toolbar.CanEdit);
        Assert.False(fixture.Toolbar.Clear.CanExecute(null));
        Assert.False(node.Properties.ContainsKey("count"));
        Assert.False(fixture.Document.CanUndo);
    }

    [Fact]
    public void AppendAllowsDuplicatesAndPreservesExistingDestinations()
    {
        using var fixture = new Fixture(CreateNode(("same.png", "custom/same.png")));
        var before = Snapshot(fixture.Nodes[0].Source);
        fixture.Append("same.png", "assets/new.png", "same.png");

        Assert.Equal("4", fixture.Nodes[0].Source.Properties["count"]);
        Assert.Equal("custom/same.png", fixture.Entries[0].DestinationProperty.Value);
        Assert.Equal(new[] { "same.png", "same.png", "assets/new.png", "same.png" },
            fixture.Entries.Select(row => row.SourceProperty.Value));
        Assert.Equal(new[] { "custom/same.png", "same.png", "new.png", "same.png" },
            fixture.Entries.Select(row => row.DestinationProperty.Value));
        Assert.True(fixture.LastEdit!.ShouldRefreshView);

        fixture.Undo();
        Assert.Equal(before, Snapshot(fixture.Nodes[0].Source));
        Assert.False(fixture.Document.CanUndo);
        fixture.Redo();
        Assert.Equal(4, fixture.Entries.Length);
    }

    [Fact]
    public void CancellingAndClearingAnEmptyListDoNotCreateUndoEntries()
    {
        using var fixture = new Fixture(CreateNode());

        Assert.Null(fixture.Toolbar.CreateAppendEdit([]));
        Assert.Null(fixture.Toolbar.CreateClearEdit());
        Assert.False(fixture.Document.CanUndo);
    }

    [Fact]
    public void EditingSourceAndDestinationUsesIndependentBindings()
    {
        using var fixture = new Fixture(CreateNode(("first.png", "original/first.png")));
        var row = Assert.Single(fixture.Entries);
        row.SourceProperty.Value = "changed.png";

        Assert.Same(row, Assert.Single(fixture.Entries));
        Assert.Equal("original/first.png", row.DestinationProperty.Value);
        Assert.False(fixture.LastEdit!.ShouldRefreshView);
        row.DestinationProperty.Value = "new/target.png";

        Assert.Equal("changed.png", row.SourceProperty.Value);
        fixture.Undo();
        Assert.Equal("original/first.png", fixture.Entries[0].DestinationProperty.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void DeleteCompactsPairsAndCanBeUndoneAndRedone(int index)
    {
        var pairs = new[]
        {
            ("one.png", "folder/one.png"),
            ("two.png", "other/two.png"),
            ("three.png", "three.png"),
        };
        using var fixture = new Fixture(CreateNode(pairs));
        var before = Snapshot(fixture.Nodes[0].Source);
        fixture.Entries[index].Delete.Execute(null);
        var remaining = pairs.Where((pair, position) => position != index).ToArray();

        Assert.Equal("2", fixture.Nodes[0].Source.Properties["count"]);
        Assert.Equal(remaining.Select(pair => pair.Item1),
            fixture.Entries.Select(row => row.SourceProperty.Value));
        Assert.Equal(remaining.Select(pair => pair.Item2),
            fixture.Entries.Select(row => row.DestinationProperty.Value));
        Assert.False(fixture.Nodes[0].Source.Properties.ContainsKey("source_2"));
        Assert.False(fixture.Nodes[0].Source.Properties.ContainsKey("destination_2"));
        Assert.True(fixture.LastEdit!.ShouldRefreshView);

        fixture.Undo();
        Assert.Equal(before, Snapshot(fixture.Nodes[0].Source));
        Assert.Equal(3, fixture.Entries.Length);
        fixture.Redo();
        Assert.Equal(2, fixture.Entries.Length);
    }

    [Fact]
    public void DeleteTheOnlyEntryProducesAnEmptyEditableList()
    {
        using var fixture = new Fixture(CreateNode(("only.png", "only.png")));
        fixture.Entries[0].Delete.Execute(null);

        Assert.Empty(fixture.Entries);
        Assert.Equal(new[] { new KeyValuePair<string, string>("count", "0") },
            Snapshot(fixture.Nodes[0].Source));
        Assert.True(fixture.Toolbar.CanEdit);
    }

    [Fact]
    public void ClearRemovesOnlyActivePairsAndPreservesOtherProperties()
    {
        var node = CreateNode(("one.png", "custom/one.png"), ("two.png", "two.png"));
        node.Properties["note"] = "keep";
        node.Properties["source_9"] = "outside-count";
        using var fixture = new Fixture(node);
        var before = Snapshot(node);
        fixture.Toolbar.Clear.Execute(null);

        Assert.Empty(fixture.Entries);
        Assert.Equal("0", node.Properties["count"]);
        Assert.Equal("keep", node.Properties["note"]);
        Assert.Equal("outside-count", node.Properties["source_9"]);
        Assert.False(node.Properties.ContainsKey("source_0"));
        Assert.False(node.Properties.ContainsKey("destination_1"));

        fixture.Undo();
        Assert.Equal(before, Snapshot(node));
        fixture.Redo();
        Assert.Empty(fixture.Entries);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MultipleSelectionAppendsAndClearsEveryNode(bool differentCounts)
    {
        using var fixture = new Fixture(
            CreateNode(("first.png", "first.png")),
            differentCounts ? CreateNode() : CreateNode(("second.png", "second.png")));
        var before = fixture.Nodes.Select(node => Snapshot(node.Source)).ToArray();

        Assert.Equal(differentCounts ? 0 : 1, fixture.Entries.Length);
        Assert.True(fixture.Toolbar.CanEdit);
        Assert.True(fixture.Toolbar.ShowEditWindow!.CanExecute(null));
        Assert.True(fixture.Toolbar.Clear.CanExecute(null));
        Assert.False(fixture.Toolbar.HasInvalidCount);
        Assert.False(fixture.Document.CanUndo);

        fixture.Append("assets/new.png");
        var afterAppend = fixture.Nodes.Select(node => Snapshot(node.Source)).ToArray();
        for (var index = 0; index < fixture.Nodes.Length; index++)
        {
            var previousCount = index == 1 && differentCounts ? 0 : 1;
            var properties = fixture.Nodes[index].Source.Properties;
            Assert.Equal((previousCount + 1).ToString(CultureInfo.InvariantCulture), properties["count"]);
            Assert.Equal("assets/new.png", properties[$"source_{previousCount}"]);
            Assert.Equal("new.png", properties[$"destination_{previousCount}"]);
        }

        fixture.Undo();
        for (var index = 0; index < before.Length; index++)
        {
            Assert.Equal(before[index], Snapshot(fixture.Nodes[index].Source));
        }
        Assert.False(fixture.Document.CanUndo);
        fixture.Redo();
        for (var index = 0; index < afterAppend.Length; index++)
        {
            Assert.Equal(afterAppend[index], Snapshot(fixture.Nodes[index].Source));
        }

        fixture.Toolbar.Clear.Execute(null);
        Assert.Empty(fixture.Entries);
        Assert.All(fixture.Nodes, node => Assert.Equal("0", node.Source.Properties["count"]));
        fixture.Undo();
        for (var index = 0; index < afterAppend.Length; index++)
        {
            Assert.Equal(afterAppend[index], Snapshot(fixture.Nodes[index].Source));
        }
        fixture.Redo();
        Assert.All(fixture.Nodes, node => Assert.Equal("0", node.Source.Properties["count"]));
    }

    [Fact]
    public void MultipleEmptyListsAreEditableWithoutNoOpUndoEntries()
    {
        using var multiple = new Fixture(CreateNode(), CreateNode());

        Assert.True(multiple.Toolbar.CanEdit);
        Assert.False(multiple.Toolbar.Clear.CanExecute(null));
        Assert.Null(multiple.Toolbar.CreateAppendEdit([]));
        Assert.Null(multiple.Toolbar.CreateClearEdit());
        Assert.False(multiple.Document.CanUndo);
        Assert.Empty(multiple.Entries);
        multiple.Append("new.png");
        Assert.Single(multiple.Entries);
        Assert.All(multiple.Nodes, node => Assert.Equal("1", node.Source.Properties["count"]));
    }

    [Fact]
    public void MultipleSelectionUsesFieldConflictsAndDeletesTheSameIndex()
    {
        using var fixture = new Fixture(
            CreateNode(("first-a.png", "target-a.png"), ("next-a.png", "next-target-a.png")),
            CreateNode(("first-b.png", "target-b.png"), ("next-b.png", "next-target-b.png")));
        var before = fixture.Nodes.Select(node => Snapshot(node.Source)).ToArray();
        var row = fixture.Entries[0];
        Assert.True(row.SourceProperty.HasConflict);
        Assert.True(row.DestinationProperty.HasConflict);

        row.SourceProperty.Value = "shared.png";
        Assert.All(fixture.Nodes, node => Assert.Equal("shared.png", node.Source.Properties["source_0"]));
        Assert.Equal("target-a.png", fixture.Nodes[0].Source.Properties["destination_0"]);
        Assert.Equal("target-b.png", fixture.Nodes[1].Source.Properties["destination_0"]);
        var afterSource = fixture.Nodes.Select(node => Snapshot(node.Source)).ToArray();
        row.DestinationProperty.Value = "shared-target.png";
        Assert.All(fixture.Nodes, node => Assert.Equal("shared-target.png", node.Source.Properties["destination_0"]));

        fixture.Undo();
        for (var index = 0; index < afterSource.Length; index++)
        {
            Assert.Equal(afterSource[index], Snapshot(fixture.Nodes[index].Source));
        }
        fixture.Undo();
        for (var index = 0; index < before.Length; index++)
        {
            Assert.Equal(before[index], Snapshot(fixture.Nodes[index].Source));
        }

        fixture.Entries[0].Delete.Execute(null);
        Assert.All(fixture.Nodes, node => Assert.Equal("1", node.Source.Properties["count"]));
        Assert.Equal("next-a.png", fixture.Nodes[0].Source.Properties["source_0"]);
        Assert.Equal("next-target-a.png", fixture.Nodes[0].Source.Properties["destination_0"]);
        Assert.Equal("next-b.png", fixture.Nodes[1].Source.Properties["source_0"]);
        Assert.Equal("next-target-b.png", fixture.Nodes[1].Source.Properties["destination_0"]);
        var afterDelete = fixture.Nodes.Select(node => Snapshot(node.Source)).ToArray();
        fixture.Undo();
        for (var index = 0; index < before.Length; index++)
        {
            Assert.Equal(before[index], Snapshot(fixture.Nodes[index].Source));
        }
        Assert.False(fixture.Document.CanUndo);
        fixture.Redo();
        for (var index = 0; index < afterDelete.Length; index++)
        {
            Assert.Equal(afterDelete[index], Snapshot(fixture.Nodes[index].Source));
        }
    }

    [Fact]
    public void OneInvalidSelectedCountPreventsPartialStructuralEdits()
    {
        var invalid = CreateNode();
        invalid.Properties["count"] = "-1";
        using var fixture = new Fixture(CreateNode(("keep.png", "keep.png")), invalid);
        var before = fixture.Nodes.Select(node => Snapshot(node.Source)).ToArray();
        var toolbar = fixture.Toolbar;

        Assert.False(toolbar.CanEdit);
        Assert.True(toolbar.HasInvalidCount);
        Assert.Throws<InvalidOperationException>(() => toolbar.CreateAppendEdit(["new.png"]));
        Assert.Throws<InvalidOperationException>(() => toolbar.CreateClearEdit());
        Assert.False(fixture.Document.CanUndo);
        for (var index = 0; index < before.Length; index++)
        {
            Assert.Equal(before[index], Snapshot(fixture.Nodes[index].Source));
        }

        fixture.Nodes[1].ChangeProperty("count", "0");
        Assert.Same(toolbar, fixture.Toolbar);
        Assert.True(toolbar.CanEdit);
        Assert.False(toolbar.HasInvalidCount);
        Assert.True(toolbar.Clear.CanExecute(null));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("2147483648")]
    public void InvalidCountIsNotSilentlyRepaired(string count)
    {
        var node = CreateNode();
        node.Properties["count"] = count;
        using var fixture = new Fixture(node);

        Assert.False(fixture.Toolbar.CanEdit);
        Assert.True(fixture.Toolbar.HasInvalidCount);
        Assert.Empty(fixture.Entries);
        Assert.Throws<InvalidOperationException>(() => fixture.Toolbar.CreateAppendEdit(["new.png"]));
        Assert.Throws<InvalidOperationException>(() => fixture.Toolbar.CreateClearEdit());
        Assert.Equal(count, node.Properties["count"]);
        Assert.False(fixture.Document.CanUndo);
    }

    [Fact]
    public void CountBindingUpdatesButtonStateWithoutRebuildingItems()
    {
        using var fixture = new Fixture(CreateNode());
        var toolbar = fixture.Toolbar;
        fixture.Nodes[0].ChangeProperty("count", "-1");

        Assert.Same(toolbar, fixture.Toolbar);
        Assert.False(toolbar.CanEdit);
        fixture.Nodes[0].ChangeProperty("count", "0");
        Assert.True(toolbar.CanEdit);
        Assert.False(fixture.Document.CanUndo);
    }

    [Fact]
    public void InvalidPathsDoNotPartiallyAppend()
    {
        using var fixture = new Fixture(CreateNode());
        var before = Snapshot(fixture.Nodes[0].Source);

        Assert.Throws<ArgumentException>(() =>
            fixture.Toolbar.CreateAppendEdit(["valid.png", "directory/"]));
        Assert.Equal(before, Snapshot(fixture.Nodes[0].Source));
        Assert.False(fixture.Document.CanUndo);
    }

    [Fact]
    public void CountOverflowIsRejectedBeforeCreatingCommands()
    {
        using var fixture = new Fixture(CreateNode());
        fixture.Nodes[0].ChangeProperty("count", int.MaxValue.ToString(CultureInfo.InvariantCulture));

        Assert.Throws<OverflowException>(() => fixture.Toolbar.CreateAppendEdit(["new.png"]));
        Assert.Single(fixture.Nodes[0].Source.Properties);
        Assert.False(fixture.Document.CanUndo);
    }

    [Fact]
    public void MissingFieldsHaveNoDestinationFallbackAndCanBeRemoved()
    {
        var node = CreateNode(("one.png", "one.png"), ("two.png", "two.png"));
        node.Properties.Remove("destination_0");
        using var fixture = new Fixture(node);
        var before = Snapshot(node);
        Assert.Equal(string.Empty, fixture.Entries[0].DestinationProperty.Value);

        fixture.Entries[0].Delete.Execute(null);
        Assert.Equal("two.png", fixture.Entries[0].DestinationProperty.Value);
        fixture.Undo();
        Assert.Equal(before, Snapshot(node));
    }

    [Fact]
    public void CustomMappingsAreUsedForCountFieldsAndDeletion()
    {
        var configuration = """
            {
              "$type": "LuaSTGEditorSharpV2.PropertyView.Configurable.SingleListTabTerm`1[[LuaSTGEditorSharpV2.PropertyView.Specialized.MultipleFile.MultipleFilePropertyViewItemListTerm, LuaSTGEditorSharpV2.PropertyView]], LuaSTGEditorSharpV2.PropertyView",
              "Caption": { "Neutral": "Files" },
              "Count": {
                "$type": "MultipleFile",
                "Mapping": { "Key": "length", "DefaultValue": "0" },
                "Files": {
                  "Source": { "Key": "input_{0}" },
                  "Destination": { "Key": "output_{0}" }
                }
              },
              "VariableProperty": {
                "Files": {
                  "Count": { "Key": "length", "DefaultValue": "0" },
                  "Source": { "Key": "input_{0}" },
                  "Destination": { "Key": "output_{0}" }
                }
              }
            }
            """;
        using var fixture = new Fixture(configuration, new NodeData("MultipleFileTest"));
        fixture.Append("assets/custom.png", "assets/second.png");

        Assert.Equal("2", fixture.Nodes[0].Source.Properties["length"]);
        Assert.Equal("assets/custom.png", fixture.Nodes[0].Source.Properties["input_0"]);
        fixture.Entries[0].Delete.Execute(null);
        Assert.Equal("assets/second.png", fixture.Nodes[0].Source.Properties["input_0"]);
        Assert.Equal("second.png", fixture.Nodes[0].Source.Properties["output_0"]);
        Assert.False(fixture.Nodes[0].Source.Properties.ContainsKey("count"));
    }

    [Fact]
    public void ActualDocumentFormatRoundTripPreservesCountAndPairs()
    {
        using var fixture = new Fixture(CreateNode());
        fixture.Append("assets/one.png", "assets/one.png");
        fixture.Entries[1].DestinationProperty.Value = "alternate/one.png";
        fixture.Entries[0].Delete.Execute(null);
        var format = DocumentFormatBase.Create("xml");
        using var writer = new StringWriter();
        format.SaveToStream(fixture.Document.Root, writer);
        using var reader = new StringReader(writer.ToString());
        var root = format.CreateFromStream(reader)!;
        var reloadedNode = Assert.Single(root.PhysicalChildren,
            node => node.TypeUID == "MultipleFileTest");

        using var reloaded = new Fixture(reloadedNode);
        Assert.Equal("1", reloadedNode.Properties["count"]);
        Assert.Equal("assets/one.png", Assert.Single(reloaded.Entries).SourceProperty.Value);
        Assert.Equal("alternate/one.png", reloaded.Entries[0].DestinationProperty.Value);
    }

    [Fact]
    public void StructureRefreshHappensAfterAllPropertiesAndDisposesOldRows()
    {
        using var fixture = new Fixture(CreateNode(("old.png", "old.png")));
        var oldToolbar = fixture.Toolbar;
        var oldRow = fixture.Entries[0];
        var oldTab = fixture.Tab;
        fixture.Nodes[0].OnPropertyAdded += (_, _) => Assert.Same(oldTab, fixture.Tab);
        fixture.Append("new.png");

        Assert.NotSame(oldTab, fixture.Tab);
        Assert.False(oldToolbar.ShowEditWindow!.CanExecute(null));
        Assert.False(oldRow.Delete.CanExecute(null));
        Assert.Throws<ObjectDisposedException>(() => oldToolbar.CreateAppendEdit(["detached.png"]));
        Assert.Throws<ObjectDisposedException>(() => oldRow.CreateDeleteEdit());
        oldRow.SourceProperty.Value = "detached.png";
        Assert.Equal("old.png", fixture.Nodes[0].Source.Properties["source_0"]);
        fixture.Nodes[0].ChangeProperty("source_0", "external.png");
        Assert.Equal("external.png", fixture.Entries[0].SourceProperty.Value);
        Assert.Equal("detached.png", oldRow.SourceProperty.Value);
    }

    [Fact]
    public void CountViewModelIsReadyAfterFactoryCreationWithoutExtraInitialization()
    {
        using var fixture = new Fixture(CreateNode());
        var term = Assert.IsType<MultipleFileCountPropertyItemTerm>(fixture.Term.Count);
        var factory = fixture.Services.GetRequiredService<
            IPropertyItemViewModelFactory<MultipleFileCountPropertyItemViewModel, MultipleFileCountPropertyItemTerm>>();
        using var viewModel = factory.Create(fixture.Nodes, term, term.Editor, fixture.Context);

        Assert.True(viewModel.CanEdit);
        Assert.False(viewModel.HasInvalidCount);
        fixture.Execute(viewModel.CreateAppendEdit(["factory.png"])!);

        Assert.Equal("factory.png", Assert.Single(fixture.Entries).SourceProperty.Value);
    }

    [Fact]
    public void EntryFactoryGetsTheDeletionTokenFromPropertySources()
    {
        using var fixture = new Fixture(CreateNode(("one.png", "one.png")));
        var files = fixture.Term.VariableProperty!.Files;
        var term = new MultipleFilePropertyViewItemListTerm.ItemTerm(
            fixture.Services, new PropertyViewEditorType("multiplefileEntry"))
        {
            Files = files,
            Mapping = files.GetItems(1)[0],
        };
        using var viewModel = fixture.Services.GetRequiredService<
            IPropertyItemViewModelFactory<MultipleFileEntryViewModel, MultipleFilePropertyViewItemListTerm.ItemTerm>>()
            .Create(fixture.Nodes, term, term.Editor, fixture.Context);

        Assert.Equal("one.png", viewModel.SourceProperty.Value);
        fixture.Execute(viewModel.CreateDeleteEdit());

        Assert.Empty(fixture.Entries);
        Assert.Equal("0", fixture.Nodes[0].Source.Properties["count"]);
    }

    [Theory]
    [InlineData("0", 2)]
    [InlineData("3", 1)]
    [InlineData("invalid", 2)]
    [InlineData("3", 0)]
    [InlineData("3", -1)]
    public void ListTermExpandsOnlyTheCountProvidedBySingleListTab(string storedCount, int count)
    {
        using var fixture = new Fixture(CreateNode(("one.png", "one.png"), ("two.png", "two.png")));
        fixture.Nodes[0].ChangeProperty("count", storedCount);
        var rows = fixture.Term.VariableProperty!.GetViewModels(fixture.Nodes, fixture.Context, count);
        try
        {
            Assert.Equal(Math.Max(0, count), rows.Count);
            for (var index = 0; index < rows.Count; index++)
            {
                var row = Assert.IsType<MultipleFileEntryViewModel>(rows[index]);
                Assert.Equal(index, row.Index);
                Assert.Equal(fixture.Nodes[0].Source.Properties[$"source_{index}"], row.SourceProperty.Value);
            }
            Assert.Equal(storedCount, fixture.Nodes[0].Source.Properties["count"]);
            Assert.False(fixture.Document.CanUndo);
        }
        finally
        {
            foreach (var row in rows)
            {
                row.Dispose();
            }
        }
    }

    [Fact]
    public void ConfiguredEditorsReachBothToolbarAndEntryViewModels()
    {
        using var fixture = new Fixture(CreateNode(("one.png", "one.png")));
        JsonConvert.PopulateObject("""
            {
              "Mapping": { "Key": "count", "DefaultValue": "0" },
              "Editor": { "Name": "customFileToolbar", "Parameters": { "marker": "toolbar" } }
            }
            """, fixture.Term.Count!);
        JsonConvert.PopulateObject("""
            { "Editor": { "Name": "customFileEntry", "Parameters": { "marker": "entry" } } }
            """, fixture.Term.VariableProperty!);
        fixture.Refresh();

        Assert.Same(fixture.Term.Count!.Editor, fixture.Toolbar.Type);
        Assert.Equal("customFileToolbar", fixture.Toolbar.Type!.Name);
        var entry = Assert.Single(fixture.Entries);
        Assert.Same(fixture.Term.VariableProperty!.Editor, entry.Type);
        Assert.Equal("customFileEntry", entry.Type!.Name);
        Assert.True(fixture.Toolbar.CanEdit);
    }

    private static KeyValuePair<string, string>[] Snapshot(NodeData node)
        => node.Properties.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray();

    private static NodeData CreateNode(params (string Source, string Destination)[] files)
    {
        var node = new NodeData("MultipleFileTest");
        node.Properties["count"] = files.Length.ToString(CultureInfo.InvariantCulture);
        for (var index = 0; index < files.Length; index++)
        {
            node.Properties[$"source_{index}"] = files[index].Source;
            node.Properties[$"destination_{index}"] = files[index].Destination;
        }
        return node;
    }

    private sealed class Fixture : IDisposable
    {
        public ServiceProvider Services { get; }
        public EditorDocument Document { get; }
        public EditorNode[] Nodes { get; }
        public PropertyViewContext Context { get; }
        public SingleListTabTerm<MultipleFilePropertyViewItemListTerm> Term { get; }
        public PropertyTabViewModel Tab { get; private set; } = null!;
        public EditResult? LastEdit { get; private set; }
        public MultipleFileCountPropertyItemViewModel Toolbar =>
            Assert.IsType<MultipleFileCountPropertyItemViewModel>(Tab.Properties[0]);
        public MultipleFileEntryViewModel[] Entries => Tab.Properties.OfType<MultipleFileEntryViewModel>().ToArray();

        public Fixture(params NodeData[] nodes) : this(Configuration, nodes)
        {
        }

        public Fixture(string configuration, params NodeData[] nodes)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<LocalizationService>();
            services.AddSingleton<DefaultValueServiceProvider>();
            services.AddSingleton<PropertyEditWizardProviderService>();
            services.AddSingleton<PropertyViewServiceProvider>();
            services.AddSingleton<EditorNodeFactory>();
            services.AddSingleton(typeof(IPropertyItemViewModelFactory<,>),
                typeof(PropertyItemViewModelFactory<,>));
            services.AddTransient<MultipleFileCountPropertyItemTerm>();
            services.AddTransient<MultipleFilePropertyViewItemListTerm>();
            services.AddTransient<SingleListTabTerm<MultipleFilePropertyViewItemListTerm>>();
            Services = services.BuildServiceProvider();
            var resolverType = typeof(DefaultValueServiceProvider).Assembly.GetType(
                "LuaSTGEditorSharpV2.Core.ServiceProviderContractResolver", throwOnError: true)!;
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ContractResolver = (IContractResolver)Activator.CreateInstance(resolverType, Services)!,
                Converters = [new SerializationTypeBindingService(Services)],
            };
            Term = Assert.IsType<SingleListTabTerm<MultipleFilePropertyViewItemListTerm>>(
                JsonConvert.DeserializeObject<PropertyTabTermBase>(configuration, settings));
            var model = DocumentModel.CreateEmpty("multiplefile-tests.lstgxml");
            foreach (var node in nodes)
            {
                model.Root.Add(node);
            }
            Document = new EditorDocument(model, Services.GetRequiredService<EditorNodeFactory>());
            Nodes = Document.RootEditorNode.Children
                .Where(node => node.Source.TypeUID == "MultipleFileTest").ToArray();
            Context = new PropertyViewContext(
                Services, new LocalServiceParam(Document), new PropertyViewServiceSettings());
            Refresh();
        }

        public void Refresh()
        {
            Tab?.Dispose();
            Tab = Nodes.Length == 1
                ? Term.GetPropertyTabViewModel(Nodes[0], Context)
                : Term.GetPropertyTabViewModel(Nodes, Context);
            Tab.OnEdit += (_, edit) => Execute(edit);
        }

        public void Execute(EditResult edit)
        {
            LastEdit = edit;
            Assert.NotNull(edit.Command);
            Document.ExecuteCommand(edit.Command!);
            if (edit.ShouldRefreshView)
            {
                Refresh();
            }
        }

        public void Append(params string[] paths)
        {
            var edit = Toolbar.CreateAppendEdit(paths);
            Assert.NotNull(edit);
            Execute(edit!);
        }

        public void Undo()
        {
            Document.Undo();
            Refresh();
        }

        public void Redo()
        {
            Document.Redo();
            Refresh();
        }

        public void Dispose()
        {
            Tab.Dispose();
            Document.Dispose();
            Services.Dispose();
        }
    }
}
