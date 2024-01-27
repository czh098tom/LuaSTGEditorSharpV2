using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Model
{
    public static class DocumentHelper
    {
        public static bool HasPath(this IDocument document)
        {
            return document.FilePath != null;
        }

        public static NodeData? FindDefinitionRoot(this IDocument document)
        {
            return document.Root.PhysicalChildren.FirstOrDefault(n => n.TypeUID == DocumentModel.definitionRootUID);
        }

        public static NodeData? FindBuildRoot(this IDocument document)
        {
            return document.Root.PhysicalChildren.FirstOrDefault(n => n.TypeUID == DocumentModel.buildRootUID);
        }

        public static NodeData? FindCompileRoot(this IDocument document)
        {
            return document.Root.PhysicalChildren.FirstOrDefault(n => n.TypeUID == DocumentModel.compileRootUID);
        }
    }
}
