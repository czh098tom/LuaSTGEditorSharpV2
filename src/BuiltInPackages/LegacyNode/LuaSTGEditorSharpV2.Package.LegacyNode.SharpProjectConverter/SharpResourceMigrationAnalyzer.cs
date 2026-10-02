using System.Globalization;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    // Runs once on the raw V1 tree, before any type/property formatter changes it.
    internal static class SharpResourceMigrationAnalyzer
    {
        internal static string GetLegacyTypeName(NodeData node)
            => node.TypeUID.Split(',')[0].Split('.')[^1];

        internal static MigrationPlan Analyze(NodeData? root, string inputFilePath, string outputFilePath)
        {
            var plan = new MigrationPlan();
            var inputDirectory = Path.GetDirectoryName(Path.GetFullPath(inputFilePath))!;
            var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputFilePath))!;
            var archivePrefix = string.Empty;
            if (root != null) Visit(root, "root", ref archivePrefix);
            return plan;

            void Visit(NodeData node, string location, ref string prefix)
            {
                // Retain disabled nodes for editing without letting their state affect siblings.
                if (!node.IsActive) return;
                var type = GetLegacyTypeName(node);
                var sourceKey = GetSourceKey(type);
                var ignoresChildren = sourceKey != null || type is "ArchiveSpaceIndicator"
                    or "CreateRenderTarget" or "UserDefinedNode" or "UserDefinedDefaultValueNode";
                if (ignoresChildren && node.PhysicalChildren.Any(child => child.IsActive))
                {
                    throw Diagnostic(node, location,
                        "V1 does not compile this node's children; active children cannot be migrated safely.");
                }

                // V1 calls AddCompileSettings after ToLua has visited the node's children.
                // Folder/Comment are transparent to this sticky state; leaving one does not restore it.
                for (var index = 0; index < node.PhysicalChildren.Count; index++)
                {
                    Visit(node.PhysicalChildren[index], $"{location}/{index}", ref prefix);
                }

                if (type == "ArchiveSpaceIndicator")
                {
                    var value = node.GetProperty("Name");
                    if (value.Length != 0)
                    {
                        if (!value.EndsWith('/') && !value.EndsWith('\\'))
                        {
                            throw Diagnostic(node, location,
                                "Archive Name must end with a directory separator; V1 registration and Lua references disagree otherwise.");
                        }
                        ValidateDestination(node, location, value + "__archive_probe__");
                    }
                    prefix = value;
                    return;
                }
                if (sourceKey == null) return;

                var rawSource = node.GetProperty(sourceKey);
                if (type == "AddFile")
                {
                    var sources = rawSource.Split('|');
                    var properties = new Dictionary<string, string>
                    {
                        ["count"] = sources.Length.ToString(CultureInfo.InvariantCulture)
                    };
                    for (var index = 0; index < sources.Length; index++)
                    {
                        var pair = MigrateFile(node, $"{location}/file[{index}]", sources[index], prefix);
                        properties[$"source_{index}"] = pair.Source;
                        properties[$"destination_{index}"] = pair.Destination;
                    }
                    plan.Add(node, properties, sourceKey);
                }
                else
                {
                    var pair = MigrateFile(node, location, rawSource, prefix);
                    var properties = new Dictionary<string, string>
                    {
                        [sourceKey] = pair.Source,
                        ["destination"] = pair.Destination
                    };
                    // V1 SetWalkImageSystem.ToLua emits the grid through InverseFromColRow, while the
                    // node stores cols,rows. The property holds what the game receives, so the stored
                    // value must already be in emission order.
                    if (type == "SetBossWalkImageSystem")
                    {
                        properties["Cols and rows"] = InverseFromColRow(node.GetProperty("Cols and rows"));
                    }
                    plan.Add(node, properties);
                }
            }

            (string Source, string Destination) MigrateFile(NodeData node, string location,
                string source, string prefix)
            {
                if (string.IsNullOrWhiteSpace(source))
                {
                    throw Diagnostic(node, location, "The resource source is empty.");
                }
                try
                {
                    var fileName = Path.GetFileName(source);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        throw Diagnostic(node, location, "The resource source has no file name.");
                    }
                    var destination = prefix + fileName;
                    ValidateDestination(node, location, destination);
                    var absoluteSource = Path.GetFullPath(source, inputDirectory);
                    // GetRelativePath keeps the absolute path when the volume differs.
                    var relocatedSource = Path.GetRelativePath(outputDirectory, absoluteSource);
                    return (relocatedSource, destination);
                }
                catch (Exception exception) when (exception is ArgumentException or NotSupportedException)
                {
                    throw Diagnostic(node, location, $"Invalid resource source '{source}': {exception.Message}");
                }
            }
        }

        private static string? GetSourceKey(string type) => type switch
        {
            "LoadSE" or "LoadBGM" or "LoadFX" or "LoadParticle" or "LoadImage"
                or "LoadAnimation" or "LoadImageGroup" or "AddFile" or "Patch" => "Path",
            "SetBossWalkImageSystem" => "Image path",
            _ => null
        };

        // V1 SetWalkImageSystem.InverseFromColRow: reverse every comma separated segment.
        private static string InverseFromColRow(string source)
        {
            var parts = source.Split(',');
            Array.Reverse(parts);
            return string.Join(",", parts);
        }

        private static void ValidateDestination(NodeData node, string location, string destination)
        {
            var path = destination.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(path) || path.StartsWith('/')
                || path.Any(character => character < 32 || "<>:\"|?*".Contains(character)))
            {
                throw Diagnostic(node, location, $"Invalid relative resource destination '{destination}'.");
            }
            if (path.Split('/')[^1] is "" or "." or "..")
            {
                throw Diagnostic(node, location, $"Resource destination must name a file: '{destination}'.");
            }
            var depth = 0;
            foreach (var part in path.Split('/'))
            {
                if (part is "" or ".") continue;
                if (part == "..")
                {
                    if (--depth < 0)
                    {
                        throw Diagnostic(node, location, $"Resource destination escapes the output root: '{destination}'.");
                    }
                }
                else depth++;
            }
        }

        private static InvalidDataException Diagnostic(NodeData node, string location, string message)
            => new($"sharpconv resource migration at {location} ({node.TypeUID}): {message}");

        internal sealed class MigrationPlan
        {
            private readonly List<(NodeData Node, Dictionary<string, string> Properties, string? Remove)> _changes = [];
            private bool _applied;

            internal void Add(NodeData node, Dictionary<string, string> properties, string? remove = null)
                => _changes.Add((node, properties, remove));

            internal void Apply()
            {
                if (_applied) throw new InvalidOperationException("The resource migration plan has already been applied.");
                _applied = true;
                foreach (var change in _changes)
                {
                    if (change.Remove != null) change.Node.Properties.Remove(change.Remove);
                    foreach (var property in change.Properties)
                    {
                        change.Node.Properties[property.Key] = property.Value;
                    }
                }
            }
        }
    }
}
