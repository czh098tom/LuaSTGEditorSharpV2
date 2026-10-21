using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    public class SharpProjectConverter(SharpNodeConverterServiceProvider converterServiceProvider, 
        string inputFilePath, string outputFilePath)
    {
        public async Task Convert(CancellationToken cancellationToken)
        {
            var (raw, sourceLines) = await ConvertNodeAsync(cancellationToken);
            var outputName = ResolveDefaultOutputName(raw);
            var resources = SharpResourceMigrationAnalyzer.Analyze(raw, inputFilePath, outputFilePath);
            resources.Apply();
            var context = new SharpNodeFormattingContext(raw, inputFilePath, Console.WriteLine);
            foreach (var (node, line) in sourceLines)
            {
                context.RegisterSourceLine(node, line);
            }
            var doc = WrapSharpV2Structure(raw);
            ApplyFormatter(doc, context);
            InjectDefaultBuildingNode(doc, outputName);
            doc.SaveAs(outputFilePath);
        }

        private string ResolveDefaultOutputName(NodeData? root)
        {
            // V1 uses only the first direct ProjSettings, even when it is disabled.
            var settings = root?.PhysicalChildren.FirstOrDefault(node =>
                SharpResourceMigrationAnalyzer.GetLegacyTypeName(node) == "ProjSettings");
            var name = settings?.GetProperty("Output Name");
            return string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(inputFilePath) : name;
        }

        private static void InjectDefaultBuildingNode(DocumentModel document, string outputName)
        {
            var buildRoot = document.FindBuildRoot()
                ?? throw new InvalidOperationException("The converted document has no build root.");
            var build = new NodeData("DefaultLuaSTGBuilding");
            build.Properties["task_name"] = "Default";
            build.Properties["output_name"] = outputName;
            build.Properties["initial_resource_group"] = "Default";
            build.Properties["keep_first_resource_target"] = "true";
            buildRoot.Insert(0, build);
        }

        private async Task<(NodeData? Root, Dictionary<NodeData, int> SourceLines)> ConvertNodeAsync(
            CancellationToken cancellationToken)
        {
            SharpNodeSerializer serializer = new();
            var sourceLines = new Dictionary<NodeData, int>();
            NodeData? root = null;
            NodeData? prev = null;
            int prevLevel = -1;
            int sourceLine = 0;
            int i;
            int levelgrad;
            char[] temp;
            string des;
            using var sr = new StreamReader(inputFilePath, Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                temp = ((await sr.ReadLineAsync(cancellationToken)) ?? string.Empty).ToCharArray();
                sourceLine++;
                i = 0;
                while (temp[i] != ',')
                {
                    i++;
                }
                des = new string(temp, i + 1, temp.Length - i - 1);
                if (prevLevel != -1 && prev != null)
                {
                    levelgrad = System.Convert.ToInt32(new string(temp, 0, i)) - prevLevel;
                    if (levelgrad <= 0)
                    {
                        for (int j = 0; j >= levelgrad && prev.PhysicalParent != null; j--)
                        {
                            prev = prev.PhysicalParent;
                        }
                    }
                    NodeData? tempN = serializer.DeserializeTreeNode(des) ?? new NodeData();
                    sourceLines[tempN] = sourceLine;
                    //tempN.FixAttrParent();
                    prev.Add(tempN);
                    prev = tempN;
                    prevLevel += levelgrad;
                }
                else
                {
                    root = serializer.DeserializeTreeNode(des);
                    if (root != null)
                    {
                        sourceLines[root] = sourceLine;
                    }
                    //root.FixAttrParent();
                    prev = root;
                    prevLevel = 0;
                }
            }
            return (root, sourceLines);
        }

        private DocumentModel WrapSharpV2Structure(NodeData? nodeData)
        {
            var doc = DocumentModel.CreateEmpty(Path.GetFileNameWithoutExtension(outputFilePath));
            if (nodeData != null)
            {
                doc.FindCompileRoot()?.Add(nodeData);
            }
            return doc;
        }

        private void ApplyFormatter(DocumentModel model, SharpNodeFormattingContext ctx)
        {
            do
            {
                FormatOnce(model, ctx);
            }
            while (ctx.ShouldRetry);
        }

        private void FormatOnce(DocumentModel model, SharpNodeFormattingContext context)
        {
            var stack = new Stack<NodeData>();
            stack.Push(model.Root);
            while (stack.Count > 0)
            {
                var curr = stack.Pop();
                foreach (var node in curr.PhysicalChildren.Reverse())
                {
                    stack.Push(node);
                }
                converterServiceProvider.GetConverter(curr.TypeUID).Convert(curr, context);
                if (context.ShouldRetry) return;
            }
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class SharpProjectConverterFactory(SharpNodeConverterServiceProvider converterServiceProvider)
    {
        public SharpProjectConverter Create(string inputFilePath, string outputFilePath)
        {
            return new SharpProjectConverter(converterServiceProvider, inputFilePath, outputFilePath);
        }
    }
}
