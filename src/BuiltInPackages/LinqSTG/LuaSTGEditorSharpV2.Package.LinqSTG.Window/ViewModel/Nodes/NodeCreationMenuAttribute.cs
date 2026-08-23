using System;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes
{
    /// <summary>
    /// Annotates a node view model so it can be listed in the blueprint area's
    /// right-click node creation menu.
    /// The category path builds the expandable menu hierarchy, while the titles
    /// are used for display and fuzzy search (current culture title plus English title).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class NodeCreationMenuAttribute : Attribute
    {
        /// <summary>
        /// Creates the annotation.
        /// </summary>
        /// <param name="categoryPath">'/'-separated stable category identifiers, e.g. <c>"Operator/Math"</c>.</param>
        public NodeCreationMenuAttribute(string categoryPath)
        {
            if (string.IsNullOrWhiteSpace(categoryPath))
            {
                throw new ArgumentException("Category path cannot be null or whitespace.", nameof(categoryPath));
            }
            CategoryPath = categoryPath;
        }

        /// <summary>
        /// '/'-separated category identifiers that form the menu hierarchy.
        /// The display order of the categories is not declared here; it lives in
        /// <see cref="NodeCreationMenuOrdering"/> so reordering only touches that single file.
        /// Each path is localized through the convention
        /// <c>linqstg_window_menu_category_&lt;segment&gt;</c> (path separator becomes '_',
        /// e.g. "Operator/Math" -> <c>linqstg_window_menu_category_operator_math</c>);
        /// a missing key falls back to the last segment's own key and then to the raw segment.
        /// </summary>
        public string CategoryPath { get; }

        /// <summary>
        /// Resource key of the node title. The current-culture value is displayed and
        /// searched, the neutral (English) value doubles as the English title.
        /// </summary>
        public string? TitleKey { get; set; }

        /// <summary>
        /// English title used for display and search when <see cref="TitleKey"/> is null,
        /// or as a fallback when the key cannot be resolved.
        /// </summary>
        public string? EnglishTitle { get; set; }

        /// <summary>
        /// Sort order of the node within its category; smaller values come first.
        /// Entries with equal order are sorted by English title.
        /// </summary>
        public double Order { get; set; }
    }
}
