using System;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu
{
    /// <summary>
    /// Declares the sort order of one category level in the blueprint area's node
    /// creation menu. Applied (possibly multiple times) to <see cref="NodeCreationMenuOrdering"/>,
    /// so all category orders live in a single place and reordering only touches that file.
    /// Categories without a declaration use order 0; equal orders fall back to the
    /// localized name sort.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class NodeCreationMenuOrderAttribute : Attribute
    {
        public NodeCreationMenuOrderAttribute(string categoryPath, double order)
        {
            if (string.IsNullOrWhiteSpace(categoryPath))
            {
                throw new ArgumentException("Category path cannot be null or whitespace.", nameof(categoryPath));
            }
            CategoryPath = categoryPath;
            Order = order;
        }

        /// <summary>'/'-separated raw category path, e.g. "Operator/Math".</summary>
        public string CategoryPath { get; }

        /// <summary>Sort order among the siblings of this category; smaller comes first.</summary>
        public double Order { get; }
    }

    /// <summary>
    /// The single place that declares the display order of every category level of the
    /// node creation menu. The menu is built from the NodeCreationMenuAttribute
    /// annotations on the nodes; orders declared here are looked up by raw category path.
    /// The top-level order follows the authoring pipeline (data source -> operation ->
    /// assignment -> pattern -> movement -> shoot), which also matches the observed
    /// per-category usage frequency.
    /// </summary>
    [NodeCreationMenuOrder("Data", 0)]
    [NodeCreationMenuOrder("Operator", 1)]
    [NodeCreationMenuOrder("Operator/Math", 0)]
    [NodeCreationMenuOrder("Operator/Context", 1)]
    [NodeCreationMenuOrder("Assignment", 2)]
    [NodeCreationMenuOrder("Pattern", 3)]
    [NodeCreationMenuOrder("Pattern/Operator", 0)]
    [NodeCreationMenuOrder("Movement", 4)]
    [NodeCreationMenuOrder("Movement/Operator", 0)]
    [NodeCreationMenuOrder("Shoot", 5)]
    public static class NodeCreationMenuOrdering
    {
    }
}
