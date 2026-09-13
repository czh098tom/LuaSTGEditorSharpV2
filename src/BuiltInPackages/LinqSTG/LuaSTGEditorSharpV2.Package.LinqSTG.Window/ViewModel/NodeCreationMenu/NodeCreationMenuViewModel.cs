using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu
{
    /// <summary>
    /// A clickable node entry inside the creation menu (category tree or search results).
    /// </summary>
    public sealed class NodeCreationNodeItemViewModel
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
    public sealed class NodeCreationCategoryViewModel : INotifyPropertyChanged
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

        public IReadOnlyList<object> Items { get; init; } = Array.Empty<object>();

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// View model of the blueprint area right-click node creation menu: a search box plus
    /// the expandable category tree that switches to filtered search results while typing.
    /// </summary>
    public sealed class NodeCreationMenuViewModel : INotifyPropertyChanged
    {
        private string searchText = string.Empty;
        private IReadOnlyList<NodeCreationNodeItemViewModel> searchResults = Array.Empty<NodeCreationNodeItemViewModel>();

        public IReadOnlyList<NodeCreationCategoryViewModel> Categories { get; private set; } = Array.Empty<NodeCreationCategoryViewModel>();

        public IReadOnlyList<NodeCreationEntry> AllEntries { get; private set; } = Array.Empty<NodeCreationEntry>();

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
        /// Rebuilds the menu from the catalog. Called each time the menu opens so that
        /// UI culture changes are picked up; the previous search is kept.
        /// </summary>
        public void Reload()
        {
            AllEntries = NodeCreationCatalog.GetEntries();
            Categories = NodeCreationCatalog.GetCategories()
                .Select(ToCategoryViewModel)
                .ToList();
            NoMatchLabel = Localized.linqstg_window_menu_noMatch;
            OnPropertyChanged(nameof(Categories));
            OnPropertyChanged(nameof(AllEntries));
            OnPropertyChanged(nameof(NoMatchLabel));
            UpdateSearchResults();
        }

        public void RaiseNodeSelected(NodeCreationNodeItemViewModel item)
        {
            NodeSelected?.Invoke(item);
        }

        public void RaiseCloseRequested()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private static NodeCreationCategoryViewModel ToCategoryViewModel(NodeCreationCategory category)
        {
            // Flat node entries render before the nested category expanders so the
            // high-frequency items of a category stay visible without scrolling past
            // its subcategory headers.
            var childNodes = category.Nodes.Select(ToNodeItemViewModel).Cast<object>();
            var childCategories = category.Subcategories.Select(ToCategoryViewModel).Cast<object>();
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

        private void UpdateSearchResults()
        {
            var query = searchText.Trim();
            IsSearching = query.Length > 0;
            SearchResults = NodeCreationSearch.Search(query, AllEntries)
                .Select(ToNodeItemViewModel)
                .ToList();
            IsNoMatch = IsSearching && SearchResults.Count == 0;
            OnPropertyChanged(nameof(IsSearching));
            OnPropertyChanged(nameof(IsNoMatch));
        }

        private void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
