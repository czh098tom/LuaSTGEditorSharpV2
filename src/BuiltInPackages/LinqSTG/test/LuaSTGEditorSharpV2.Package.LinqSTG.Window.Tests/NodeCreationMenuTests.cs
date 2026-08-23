using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using Nodes = LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using NodeNetwork.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    /// <summary>
    /// Tests for the blueprint area right-click node creation menu: catalog building from
    /// the NodeCreationMenuAttribute annotations, culture-aware title resolution, fuzzy
    /// search over current-culture and English titles, and node creation.
    /// </summary>
    public class NodeCreationMenuTests : IDisposable
    {
        private readonly CultureInfo _originalCulture;

        public NodeCreationMenuTests()
        {
            _originalCulture = CultureInfo.CurrentUICulture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentUICulture = _originalCulture;
        }

        private static void UseEnglish()
        {
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        private static void UseChinese()
        {
            CultureInfo.CurrentUICulture = new CultureInfo("zh-CN");
        }

        [Fact]
        public void Score_MatchesSubsequenceCaseInsensitive()
        {
            Assert.NotNull(NodeCreationSearch.Score("sin", "Sin"));
            Assert.NotNull(NodeCreationSearch.Score("SIN", "Sin"));
            Assert.NotNull(NodeCreationSearch.Score("flt", "Float"));
            Assert.NotNull(NodeCreationSearch.Score("add", "Add (+)"));
        }

        [Fact]
        public void Score_RejectsNonSubsequence()
        {
            Assert.Null(NodeCreationSearch.Score("xyz", "Sin"));
            Assert.Null(NodeCreationSearch.Score("floax", "Float"));
            Assert.Null(NodeCreationSearch.Score("f", string.Empty));
        }

        [Fact]
        public void Score_PrefersWordStartMatches()
        {
            // 'a' at a word start should score higher than 'a' inside a word.
            Assert.True(NodeCreationSearch.Score("ad", "Add (+)")! > NodeCreationSearch.Score("dd", "Add (+)")!);
        }

        [Fact]
        public void Catalog_CoversAllNodeTypesExactlyOnce()
        {
            UseEnglish();
            var entries = NodeCreationCatalog.GetEntries();

            var baseType = typeof(LinqSTGNodeViewModel);
            var allNodeTypes = baseType.Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t) && t != baseType)
                .ToList();

            Assert.NotEmpty(allNodeTypes);
            Assert.Equal(allNodeTypes.Count, entries.Count);
            Assert.Equal(allNodeTypes.Count, entries.Select(e => e.NodeType).Distinct().Count());
            foreach (var type in allNodeTypes)
            {
                Assert.Contains(entries, e => e.NodeType == type);
            }
        }

        [Fact]
        public void Catalog_EntriesHaveValidMetadata()
        {
            UseEnglish();
            foreach (var entry in NodeCreationCatalog.GetEntries())
            {
                Assert.False(string.IsNullOrWhiteSpace(entry.Title));
                Assert.False(string.IsNullOrWhiteSpace(entry.EnglishTitle));
                Assert.NotEmpty(entry.CategorySegments);
                Assert.True(entry.NodeType.GetConstructor(Type.EmptyTypes) != null,
                    $"Node type {entry.NodeType.Name} needs a public parameterless constructor.");
            }
        }

        [Fact]
        public void Catalog_BuildsCategoryHierarchy()
        {
            UseEnglish();
            var categories = NodeCreationCatalog.GetCategories();
            Assert.Contains(categories, c => c.Name == "Data");
            Assert.Contains(categories, c => c.Name == "Shoot");

            var @operator = categories.Single(c => c.Name == "Operator");
            Assert.Contains(@operator.Nodes, n => n.NodeType == typeof(Nodes.IntrinsicOperator.AddNode));
            var math = @operator.Subcategories.Single(c => c.Name == "Math");
            Assert.Contains(math.Nodes, n => n.NodeType == typeof(Nodes.IntrinsicOperator.Math.SinNode));
            Assert.Equal("Operator / Math", math.Path);
        }

        [Fact]
        public void Catalog_ResolvesTitlesForCurrentCulture()
        {
            UseEnglish();
            Assert.Equal("Float", NodeCreationCatalog.GetEntries()
                .Single(e => e.NodeType == typeof(Nodes.Data.ConstantFloatNode)).Title);

            UseChinese();
            var floatEntry = NodeCreationCatalog.GetEntries()
                .Single(e => e.NodeType == typeof(Nodes.Data.ConstantFloatNode));
            Assert.Equal("浮点数", floatEntry.Title);
            // English title is always resolved from the neutral resources for search.
            Assert.Equal("Float", floatEntry.EnglishTitle);
        }

        [Fact]
        public void Catalog_LocalizesCategoryNames()
        {
            UseChinese();
            var shoot = NodeCreationCatalog.GetCategories().Single(c => c.Name == "发射");
            Assert.Contains(shoot.Nodes, n => n.NodeType == typeof(Nodes.ShootNode));
        }

        [Fact]
        public void Search_MatchesEnglishTitleRegardlessOfCulture()
        {
            UseChinese();
            var results = NodeCreationSearch.Search("sin", NodeCreationCatalog.GetEntries()).ToList();
            Assert.Contains(results, e => e.NodeType == typeof(Nodes.IntrinsicOperator.Math.SinNode));
        }

        [Fact]
        public void Search_MatchesCurrentCultureTitle()
        {
            UseChinese();
            var results = NodeCreationSearch.Search("浮点", NodeCreationCatalog.GetEntries()).ToList();
            Assert.Contains(results, e => e.NodeType == typeof(Nodes.Data.ConstantFloatNode));
        }

        [Fact]
        public void Search_RanksBestMatchFirst()
        {
            UseEnglish();
            var results = NodeCreationSearch.Search("float", NodeCreationCatalog.GetEntries()).ToList();
            Assert.NotEmpty(results);
            Assert.Equal(typeof(Nodes.Data.ConstantFloatNode), results[0].NodeType);
        }



        [Fact]
        public void Catalog_SortsCategoriesByDeclaredOrder()
        {
            UseEnglish();
            var root = NodeCreationCatalog.GetCategories().Select(c => c.Name).ToList();
            Assert.Equal(
                new[] { "Data", "Operator", "Movement", "Movement Operator", "Movement Transform", "Pattern", "Pattern Operator", "Transformation", "Shoot" },
                root);

            var @operator = NodeCreationCatalog.GetCategories().Single(c => c.Name == "Operator");
            Assert.Equal(new[] { "Math", "Conversion", "Context" }, @operator.Subcategories.Select(c => c.Name));
        }

        [Fact]
        public void Catalog_SortsNodesByOrderThenTitle()
        {
            UseEnglish();
            var @operator = NodeCreationCatalog.GetCategories().Single(c => c.Name == "Operator");
            Assert.Equal(
                new[] { "Add (+)", "Subtract (-)", "Multiply (*)", "Divide (/)", "Modulo (%)", "Negate" },
                @operator.Nodes.Select(n => n.EnglishTitle));
        }


        [Fact]
        public void Ordering_DeclaredPathsMatchActualCategories()
        {
            // Every order declared on NodeCreationMenuOrdering must target a path that
            // the node annotations actually use; otherwise the declaration silently has no effect.
            var declared = typeof(NodeCreationMenuOrdering)
                .GetCustomAttributes<NodeCreationMenuOrderAttribute>(false)
                .Select(a => a.CategoryPath)
                .ToList();
            Assert.Equal(12, declared.Count);

            var nodePaths = typeof(Nodes.LinqSTGNodeViewModel).Assembly.GetTypes()
                .Where(t => t.IsDefined(typeof(Nodes.NodeCreationMenuAttribute), false))
                .Select(t => t.GetCustomAttribute<Nodes.NodeCreationMenuAttribute>()!.CategoryPath)
                .ToHashSet();
            foreach (var path in declared)
            {
                Assert.True(
                    nodePaths.Contains(path) || nodePaths.Any(n => n.StartsWith(path + "/", StringComparison.Ordinal)),
                    $"Order declaration '{path}' matches no node category path.");
            }
        }

        [Fact]
        public void Catalog_LocalizesSubcategoryNames()
        {
            UseChinese();
            var @operator = NodeCreationCatalog.GetCategories().Single(c => c.Name == "运算符");
            Assert.Equal(new[] { "数学", "类型转换", "上下文" }, @operator.Subcategories.Select(c => c.Name));

            var sin = NodeCreationCatalog.GetEntries().Single(e => e.NodeType == typeof(Nodes.IntrinsicOperator.Math.SinNode));
            Assert.Equal(new[] { "运算符", "数学" }, sin.CategorySegments);
            Assert.Equal("运算符 / 数学", string.Join(" / ", sin.CategorySegments));
        }

        [Fact]
        public void Diag_CultureRoundtripKeepsCategoriesUnique()
        {
            foreach (var culture in new[] { new CultureInfo("zh-CN"), CultureInfo.InvariantCulture, new CultureInfo("zh-CN") })
            {
                CultureInfo.CurrentUICulture = culture;
                var cats = NodeCreationCatalog.GetCategories();
                var names = cats.Select(c => c.Name).ToList();
                var dups = names.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                Assert.False(dups.Any(), $"culture={culture}: duplicates {string.Join("|", dups)} in [{string.Join(", ", names)}]");
                Assert.Equal(9, names.Count);
            }
        }

        [Fact]
        public void ViewModel_SearchSwitchesListToFilteredResults()
        {
            UseEnglish();
            var viewModel = new NodeCreationMenuViewModel();
            viewModel.Reload();
            Assert.False(viewModel.IsSearching);
            Assert.Empty(viewModel.SearchResults);

            viewModel.SearchText = "sin";
            Assert.True(viewModel.IsSearching);
            Assert.NotEmpty(viewModel.SearchResults);
            Assert.Contains(viewModel.SearchResults, r => r.Entry.NodeType == typeof(Nodes.IntrinsicOperator.Math.SinNode));

            viewModel.SearchText = string.Empty;
            Assert.False(viewModel.IsSearching);
            Assert.Empty(viewModel.SearchResults);
        }

        [Fact]
        public void ViewModel_NoMatchState()
        {
            UseEnglish();
            var viewModel = new NodeCreationMenuViewModel();
            viewModel.Reload();
            viewModel.SearchText = "zzzqqq";
            Assert.True(viewModel.IsSearching);
            Assert.True(viewModel.IsNoMatch);
            Assert.False(string.IsNullOrWhiteSpace(viewModel.NoMatchLabel));
        }

        [Fact]
        public void ViewModel_RaisesNodeSelected()
        {
            UseEnglish();
            var viewModel = new NodeCreationMenuViewModel();
            viewModel.Reload();
            var entry = NodeCreationCatalog.GetEntries().First();

            NodeCreationNodeItemViewModel? selected = null;
            viewModel.NodeSelected += item => selected = item;
            viewModel.RaiseNodeSelected(viewModel.SearchResults.Count > 0
                ? viewModel.SearchResults[0]
                : new NodeCreationNodeItemViewModel { Entry = entry, Title = entry.Title, EnglishTitle = entry.EnglishTitle, Subtitle = "" });

            Assert.NotNull(selected);
        }

        [Fact]
        public void Entry_CreateNodeInstantiatesType()
        {
            UseEnglish();
            var entry = NodeCreationCatalog.GetEntries()
                .Single(e => e.NodeType == typeof(Nodes.IntrinsicOperator.Math.CosNode));
            var node = entry.CreateNode();
            Assert.IsType<Nodes.IntrinsicOperator.Math.CosNode>(node);
        }

        [Fact]
        public void MainViewModel_AddNodeAddsNodeAtPosition()
        {
            UseEnglish();
            var viewModel = new MainViewModel();
            var entry = NodeCreationCatalog.GetEntries()
                .Single(e => e.NodeType == typeof(Nodes.Data.ConstantIntNode));
            int before = viewModel.Network.Nodes.Count;
            var position = new Point(42.5, -17.25);

            viewModel.AddNode(entry, position);

            Assert.Equal(before + 1, viewModel.Network.Nodes.Count);
            var added = viewModel.Network.Nodes.Items.OfType<Nodes.Data.ConstantIntNode>().Single();
            Assert.Equal(position, added.Position);
        }
    }
}
