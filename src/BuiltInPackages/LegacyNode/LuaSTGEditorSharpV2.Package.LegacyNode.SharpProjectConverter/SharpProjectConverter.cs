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
            var raw = await ConvertNodeAsync(cancellationToken);
            var doc = WrapSharpV2Structure(raw);
            ApplyFormatter(doc);
            doc.SaveAs(outputFilePath);
        }

        private async Task<NodeData?> ConvertNodeAsync(CancellationToken cancellationToken)
        {
            SharpNodeSerializer serializer = new();
            NodeData? root = null;
            NodeData? prev = null;
            int prevLevel = -1;
            int i;
            int levelgrad;
            char[] temp;
            string des;
            using var sr = new StreamReader(inputFilePath, Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                temp = ((await sr.ReadLineAsync(cancellationToken)) ?? string.Empty).ToCharArray();
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
                    //tempN.FixAttrParent();
                    prev.Add(tempN);
                    prev = tempN;
                    prevLevel += levelgrad;
                }
                else
                {
                    root = serializer.DeserializeTreeNode(des);
                    //root.FixAttrParent();
                    prev = root;
                    prevLevel = 0;
                }
            }
            return root;
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

        private void ApplyFormatter(DocumentModel model)
        {
            var ctx = new SharpNodeFormattingContext();
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
