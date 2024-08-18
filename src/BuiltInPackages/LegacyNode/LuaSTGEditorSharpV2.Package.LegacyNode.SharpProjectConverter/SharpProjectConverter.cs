using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    public class SharpProjectConverter(string fileName)
    {
        private readonly string fileName = fileName;

        public async Task<NodeData?> ConvertAsync()
        {
            SharpNodeSerializer serializer = new();
            NodeData? root = null;
            NodeData? prev = null;
            int prevLevel = -1;
            int i;
            int levelgrad;
            char[] temp;
            string des;
            using var sr = new StreamReader(fileName, Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                temp = ((await sr.ReadLineAsync()) ?? string.Empty).ToCharArray();
                i = 0;
                while (temp[i] != ',')
                {
                    i++;
                }
                des = new string(temp, i + 1, temp.Length - i - 1);
                if (prevLevel != -1 && prev != null)
                {
                    levelgrad = Convert.ToInt32(new string(temp, 0, i)) - prevLevel;
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
    }
}
