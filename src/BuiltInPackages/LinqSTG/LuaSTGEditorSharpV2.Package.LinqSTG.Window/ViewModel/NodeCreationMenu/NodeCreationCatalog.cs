using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu
{
    /// <summary>
    /// A node type listed in the blueprint area's right-click creation menu.
    /// </summary>
    public sealed class NodeCreationEntry
    {
        public required Type NodeType { get; init; }

        /// <summary>Localized category path segments, e.g. { "运算符", "数学" }.</summary>
        public required IReadOnlyList<string> CategorySegments { get; init; }

        /// <summary>Node title in the current culture.</summary>
        public required string Title { get; init; }

        /// <summary>Node title in English (neutral resources).</summary>
        public required string EnglishTitle { get; init; }

        /// <summary>Sort order within the category; smaller comes first.</summary>
        public required double SortOrder { get; init; }

        public LinqSTGNodeViewModel CreateNode()
        {
            return (LinqSTGNodeViewModel)(Activator.CreateInstance(NodeType)
                ?? throw new InvalidOperationException($"Could not create an instance of node type '{NodeType.Name}'."));
        }
    }

    /// <summary>
    /// A category level of the creation menu, containing nested categories and node entries.
    /// </summary>
    public sealed class NodeCreationCategory
    {
        public required string Name { get; init; }

        /// <summary>Sort order declared in <see cref="NodeCreationMenuOrdering"/>; smaller comes first.</summary>
        public required double Order { get; init; }

        /// <summary>Localized category path including this level, e.g. "运算符 / 数学".</summary>
        public required string Path { get; init; }

        public required IReadOnlyList<NodeCreationCategory> Subcategories { get; init; }

        public required IReadOnlyList<NodeCreationEntry> Nodes { get; init; }
    }

    /// <summary>
    /// Builds the node creation menu data from the <see cref="NodeCreationMenuAttribute"/>
    /// annotations on the node view models and the category orders declared on
    /// <see cref="NodeCreationMenuOrdering"/>. Results are cached per UI culture.
    /// </summary>
    public static class NodeCreationCatalog
    {
        private const string CategoryKeyPrefix = "linqstg_window_menu_category_";

        private static readonly object gate = new();
        private static string? builtCultureName;
        private static IReadOnlyList<NodeCreationCategory> categories = Array.Empty<NodeCreationCategory>();
        private static IReadOnlyList<NodeCreationEntry> entries = Array.Empty<NodeCreationEntry>();

        public static IReadOnlyList<NodeCreationCategory> GetCategories()
        {
            EnsureBuilt();
            return categories;
        }

        public static IReadOnlyList<NodeCreationEntry> GetEntries()
        {
            EnsureBuilt();
            return entries;
        }

        private static void EnsureBuilt()
        {
            var culture = CultureInfo.CurrentUICulture;
            lock (gate)
            {
                if (builtCultureName == culture.Name) return;
                Build(culture);
                builtCultureName = culture.Name;
            }
        }

        private static void Build(CultureInfo culture)
        {
            var manager = Localized.ResourceManager;
            var invariant = CultureInfo.InvariantCulture;
            var orders = GetDeclaredOrders();
            var newEntries = new List<NodeCreationEntry>();
            var root = new CategoryNode("");

            foreach (var type in GetAnnotatedNodeTypes())
            {
                var attribute = type.GetCustomAttribute<NodeCreationMenuAttribute>()!;
                var rawPath = attribute.CategoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .Select(segment => segment.Trim())
                    .Where(segment => segment.Length > 0)
                    .ToArray();
                if (rawPath.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"Node type '{type.Name}' declares an empty category path.");
                }

                var title = attribute.TitleKey is null ? null : manager.GetString(attribute.TitleKey, culture);
                var english = attribute.TitleKey is null ? null : manager.GetString(attribute.TitleKey, invariant);
                var fallback = english ?? attribute.EnglishTitle;
                if (string.IsNullOrWhiteSpace(fallback))
                {
                    throw new InvalidOperationException(
                        $"Node type '{type.Name}' has a NodeCreationMenu annotation without a resolvable title. " +
                        "Set TitleKey to an existing resource key or specify EnglishTitle.");
                }

                var entry = new NodeCreationEntry
                {
                    NodeType = type,
                    CategorySegments = LocalizeCategoryPath(rawPath, culture),
                    Title = string.IsNullOrWhiteSpace(title) ? fallback! : title!,
                    EnglishTitle = fallback!,
                    SortOrder = attribute.Order,
                };
                newEntries.Add(entry);

                var node = root;
                for (int i = 0; i < rawPath.Length; i++)
                {
                    var fullRawPath = string.Join("/", rawPath[..(i + 1)]);
                    node = node.GetOrAddChild(rawPath[i],
                        orders.GetValueOrDefault(fullRawPath),
                        ResolveCategoryName(rawPath[..(i + 1)], culture));
                }
                node.Nodes.Add(entry);
            }

            entries = newEntries
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.EnglishTitle, StringComparer.OrdinalIgnoreCase)
                .ToList();

            categories = root.Children
                .Select(c => ToCategory(c, culture, ""))
                .OrderBy(c => c.Order)
                .ThenBy(c => c.Name, StringComparer.CurrentCulture)
                .ToList();
        }

        private static Dictionary<string, double> GetDeclaredOrders()
        {
            return typeof(NodeCreationMenuOrdering)
                .GetCustomAttributes<NodeCreationMenuOrderAttribute>(false)
                .ToDictionary(a => a.CategoryPath, a => a.Order, StringComparer.Ordinal);
        }

        private static IEnumerable<Type> GetAnnotatedNodeTypes()
        {
            var baseType = typeof(LinqSTGNodeViewModel);
            return baseType.Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))
                .Where(t => t.IsDefined(typeof(NodeCreationMenuAttribute), false))
                .OrderBy(t => t.Name, StringComparer.Ordinal);
        }

        private static IReadOnlyList<string> LocalizeCategoryPath(string[] rawPath, CultureInfo culture)
        {
            var result = new string[rawPath.Length];
            for (int i = 0; i < rawPath.Length; i++)
            {
                result[i] = ResolveCategoryName(rawPath[..(i + 1)], culture);
            }
            return result;
        }

        /// <summary>
        /// Resolves the display name of the last segment of <paramref name="fullPath"/>:
        /// first via the full-path resource key ("Operator/Math" ->
        /// linqstg_window_menu_category_operator_math), then via the segment's own key,
        /// and finally the raw segment.
        /// </summary>
        private static string ResolveCategoryName(string[] fullPath, CultureInfo culture)
        {
            var manager = Localized.ResourceManager;
            var segment = fullPath[^1];
            var fullKey = CategoryKeyPrefix + string.Join("_", fullPath.Select(ToKeySegment));
            return manager.GetString(fullKey, culture)
                ?? manager.GetString(CategoryKeyPrefix + ToKeySegment(segment), culture)
                ?? segment;
        }

        private static string ToKeySegment(string segment)
        {
            // Multi-word camel-case segments ("MovementOperator") must map to
            // the fully lower-cased resource keys ("...category_movementoperator");
            // lower-casing only the first character would miss them.
            return segment.ToLowerInvariant();
        }

        private static NodeCreationCategory ToCategory(CategoryNode node, CultureInfo culture, string parentPath)
        {
            var path = parentPath.Length == 0 ? node.Name : parentPath + " / " + node.Name;
            return new NodeCreationCategory
            {
                Name = node.Name,
                Order = node.Order,
                Path = path,
                Subcategories = node.Children
                    .Select(c => ToCategory(c, culture, path))
                    .OrderBy(c => c.Order)
                    .ThenBy(c => c.Name, StringComparer.CurrentCulture)
                    .ToList(),
                Nodes = node.Nodes
                    .OrderBy(e => e.SortOrder)
                    .ThenBy(e => e.EnglishTitle, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
            };
        }

        private sealed class CategoryNode
        {
            public CategoryNode(string segment)
            {
                Segment = segment;
            }

            /// <summary>Raw path segment used to group entries; independent of the display culture.</summary>
            public string Segment { get; }

            /// <summary>Localized display name resolved with the culture this catalog was built for.</summary>
            public string Name { get; private set; } = string.Empty;

            /// <summary>Order declared in <see cref="NodeCreationMenuOrdering"/>; 0 when not declared.</summary>
            public double Order { get; private set; }

            public List<CategoryNode> Children { get; } = new();

            public List<NodeCreationEntry> Nodes { get; } = new();

            public CategoryNode GetOrAddChild(string segment, double? declaredOrder, string resolvedName)
            {
                var child = Children.FirstOrDefault(c => c.Segment == segment);
                if (child is null)
                {
                    child = new CategoryNode(segment)
                    {
                        Name = resolvedName,
                        Order = declaredOrder ?? 0,
                    };
                    Children.Add(child);
                }
                return child;
            }
        }
    }
}
