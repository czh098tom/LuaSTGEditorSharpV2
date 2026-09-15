using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu
{
    public interface INodeCreationMenuItem
    {
    }

    /// <summary>
    /// A clickable node entry inside the creation menu (category tree or search results).
    /// </summary>
    public sealed class NodeCreationNodeItemViewModel : INodeCreationMenuItem
    {
        public required NodeCreationEntry Entry { get; init; }

        public required string Title { get; init; }

        public required string EnglishTitle { get; init; }

        /// <summary>Localized category path shown as a hint in the search results.</summary>
        public required string Subtitle { get; init; }
    }

    /// <summary>
    /// An expandable category level of the creation menu. Its <see cref="Items"/> mix
    /// nested <see cref="NodeCreationCategoryViewModel"/> and
    /// <see cref="NodeCreationNodeItemViewModel"/> entries.
    /// </summary>
    public sealed class NodeCreationCategoryViewModel : INotifyPropertyChanged, INodeCreationMenuItem
    {
        private bool isExpanded;

        public required string Name { get; init; }

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded == value) return;
                isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }

        public IReadOnlyList<INodeCreationMenuItem> Items { get; init; } = Array.Empty<INodeCreationMenuItem>();

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// View model of the blueprint area right-click node creation menu: a search box plus
    /// the expandable category tree that switches to filtered search results while typing.
    /// </summary>
    public sealed class NodeCreationMenuViewModel : INotifyPropertyChanged
    {
        private string searchText = string.Empty;
        private IReadOnlyList<NodeCreationNodeItemViewModel> searchSource = Array.Empty<NodeCreationNodeItemViewModel>();
        private IReadOnlyList<NodeCreationNodeItemViewModel> searchResults = Array.Empty<NodeCreationNodeItemViewModel>();
        private bool showsFlatList;

        public IReadOnlyList<NodeCreationCategoryViewModel> Categories { get; private set; } = Array.Empty<NodeCreationCategoryViewModel>();

        /// <summary>Non-empty trimmed query switches the list into search mode.</summary>
        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText == value) return;
                searchText = value;
                OnPropertyChanged();
                UpdateSearchResults();
            }
        }

        public bool IsSearching { get; private set; }

        public bool IsNoMatch { get; private set; }

        public string NoMatchLabel { get; private set; } = string.Empty;

        /// <summary>
        /// True while the flat result list replaces the category tree: either a query is active,
        /// or the menu was opened with an explicit item list via <see cref="ShowItems"/>.
        /// </summary>
        public bool IsListVisible { get; private set; }

        public IReadOnlyList<NodeCreationNodeItemViewModel> SearchResults
        {
            get => searchResults;
            private set
            {
                searchResults = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Raised when the user picks a node; the blueprint window creates the node.</summary>
        public event Action<NodeCreationNodeItemViewModel>? NodeSelected;

        /// <summary>Raised when the user requests closing the menu (e.g. Escape).</summary>
        public event EventHandler? CloseRequested;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Shows the node catalog as a category tree. The caller supplies the data, so the menu
        /// never reaches for the catalog itself, and the search scope is exactly what it displays.
        /// </summary>
        public void ShowFullMenu(IReadOnlyList<NodeCreationCategory> categories)
        {
            Categories = categories
                .Select(ToCategoryViewModel)
                .ToList();
            OnPropertyChanged(nameof(Categories));
            ResetSearch(Flatten(Categories).ToList(), showsFlatList: false);
        }

        public void RaiseNodeSelected(NodeCreationNodeItemViewModel item)
        {
            NodeSelected?.Invoke(item);
        }

        public void RaiseCloseRequested()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Shows a caller-supplied flat list of node entries instead of the category tree, e.g.
        /// the nodes compatible with a connection that was dropped on empty space.
        /// </summary>
        public void ShowItems(IReadOnlyList<NodeCreationEntry> entries)
        {
            Categories = Array.Empty<NodeCreationCategoryViewModel>();
            OnPropertyChanged(nameof(Categories));
            ResetSearch(entries.Select(ToNodeItemViewModel).ToList(), showsFlatList: true);
        }

        private static NodeCreationCategoryViewModel ToCategoryViewModel(NodeCreationCategory category)
        {
            // Flat node entries render before the nested category expanders so the
            // high-frequency items of a category stay visible without scrolling past
            // its subcategory headers.
            var childNodes = category.Nodes.Select(ToNodeItemViewModel).Cast<INodeCreationMenuItem>();
            var childCategories = category.Subcategories.Select(ToCategoryViewModel).Cast<INodeCreationMenuItem>();
            return new NodeCreationCategoryViewModel
            {
                Name = category.Name,
                Items = childNodes.Concat(childCategories).ToList(),
            };
        }

        private static NodeCreationNodeItemViewModel ToNodeItemViewModel(NodeCreationEntry entry)
        {
            return new NodeCreationNodeItemViewModel
            {
                Entry = entry,
                Title = entry.Title,
                EnglishTitle = entry.EnglishTitle,
                Subtitle = string.Join(" / ", entry.CategorySegments),
            };
        }

        /// <summary>
        /// Points the menu at <paramref name="source"/> (the items currently on display) and
        /// clears any previous query. Search only ever looks at this source.
        /// </summary>
        private void ResetSearch(IReadOnlyList<NodeCreationNodeItemViewModel> source, bool showsFlatList)
        {
            searchSource = source;
            this.showsFlatList = showsFlatList;
            NoMatchLabel = Localized.linqstg_window_menu_noMatch;
            OnPropertyChanged(nameof(NoMatchLabel));
            searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            UpdateSearchResults();
        }

        private static IEnumerable<NodeCreationNodeItemViewModel> Flatten(
            IEnumerable<NodeCreationCategoryViewModel> categories)
        {
            foreach (var category in categories)
            {
                foreach (var node in Flatten(category))
                {
                    yield return node;
                }
            }
        }

        private static IEnumerable<NodeCreationNodeItemViewModel> Flatten(NodeCreationCategoryViewModel category)
        {
            foreach (var item in category.Items)
            {
                switch (item)
                {
                    case NodeCreationNodeItemViewModel node:
                        yield return node;
                        break;
                    case NodeCreationCategoryViewModel subcategory:
                        foreach (var nested in Flatten(subcategory))
                        {
                            yield return nested;
                        }
                        break;
                }
            }
        }

        private void UpdateSearchResults()
        {
            var query = searchText.Trim();
            IsSearching = query.Length > 0;
            SearchResults = IsSearching
                ? NodeCreationSearch.Search(query, searchSource.Select(item => item.Entry))
                    .Select(ToNodeItemViewModel)
                    .ToList()
                : searchSource;
            IsListVisible = IsSearching || showsFlatList;
            IsNoMatch = IsSearching && SearchResults.Count == 0;
            OnPropertyChanged(nameof(IsSearching));
            OnPropertyChanged(nameof(IsListVisible));
            OnPropertyChanged(nameof(IsNoMatch));
        }

        private void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
