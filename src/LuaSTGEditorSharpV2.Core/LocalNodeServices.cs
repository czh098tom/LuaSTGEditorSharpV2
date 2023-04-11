using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using System.Diagnostics.CodeAnalysis;

namespace LuaSTGEditorSharpV2.Core
{
    public class LocalNodeServices
    {
        public static readonly string serviceDefinitionType = "service definition";
        public static readonly string serviceShortNameProperty = "serviceShortName";
        public static readonly string serviceDeclarationProperty = "serviceDeclaration";

        public static LocalNodeServices FromDoc(DocumentModel doc)
        {
            LocalNodeServices s = new();
            s.LoadFieldFromDoc(doc);
            return s;
        }

        public IReadOnlyList<(object, string)> Services => _services;
        public PackageInfo PackageInfo => _packageInfo;

        private readonly List<(object, string)> _services = new();
        private PackageInfo _packageInfo;

        public LocalNodeServices()
        {
            _packageInfo = new PackageInfo(string.Empty, new Version(), null);
        }

        [MemberNotNull(nameof(_packageInfo))]
        private void LoadFieldFromDoc(DocumentModel doc)
        {
            _packageInfo = doc.GetPackageInfoForLocalNodeService();
            var root = doc.FindDefinitionRoot();
            Stack<NodeData> stack = new();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var curr = stack.Pop();
                if (curr.TypeUID == serviceDefinitionType)
                {
                    string shortName = curr.GetProperty(serviceShortNameProperty);
                    if (string.IsNullOrWhiteSpace(shortName))
                    {
                        var obj = JsonConvert.DeserializeObject(curr.GetProperty(serviceDeclarationProperty));
                        if (obj != null)
                        {
                            _services.Add((obj, shortName));
                        }
                    }
                }
                foreach (var child in curr.GetLogicalChildren())
                {
                    stack.Push(child);
                }
            }
        }
    }
}
