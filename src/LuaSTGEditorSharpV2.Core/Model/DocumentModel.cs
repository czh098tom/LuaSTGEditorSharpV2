using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Exception;

namespace LuaSTGEditorSharpV2.Core.Model
{
    public class DocumentModel
    {
        public static readonly string definitionRootUID = "definition root";
        public static readonly string buildRootUID = "build root";
        public static readonly string compileRootUID = "compile root";

        protected static readonly JsonSerializerSettings _savingSerializer = new()
        {
            Formatting = Formatting.Indented
        };

        public static DocumentModel? CreateFromFile(string filePath)
        {
            try
            {
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
                using StreamReader sr = new(fs);
                var node = DocumentFormatBase.CreateByExtension(Path.GetExtension(filePath)).CreateFromStream(sr);
                if (node == null) return null;
                return new DocumentModel(filePath, node);
            }
            catch (System.Exception e)
            {
                throw new OpenFileException($"Could not open file by path {filePath}", e);
            }
        }

        public static NodeData? CreateFromStream(TextReader reader)
        {
            try
            {
                return DocumentFormatBase.Create().CreateFromStream(reader);
            }
            catch (System.Exception e)
            {
                throw new OpenFileException($"Could not open file from stream", e);
            }
        }

        private NodeData _root;

        public virtual NodeData Root 
        {
            get => _root;
            protected set => _root = value;
        }

        public virtual string? FilePath { get; private set; }

        public virtual bool IsOnDisk => !string.IsNullOrWhiteSpace(FilePath);

        public DocumentModel()
        {
            _root = new NodeData();
        }

        private DocumentModel(string filePath, NodeData root)
        {
            FilePath = filePath;
            _root = root;
        }

        public virtual void Save()
        {
            if (FilePath == null) throw new InvalidOperationException("This document has not been saved to a path yet.");
            SaveAs(FilePath);
        }

        public virtual void SaveAs(string filePath)
        {
            try
            {
                using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write);
                using StreamWriter sw = new(fs);
                DocumentFormatBase.Create().SaveToStream(Root, sw);
            }
            catch (System.Exception e)
            {
                throw new SaveFileException($"Could not save to path {filePath}", e);
            }
            FilePath = filePath;
        }

        public PackageInfo GetPackageInfoForLocalNodeService()
        {
            return new PackageInfo($"_FILE_{FilePath}"
                , new Version(int.MaxValue, int.MaxValue, int.MaxValue), null);
        }

        public NodeData? FindDefinitionRoot()
        {
            return Root.PhysicalChildren.FirstOrDefault(n => n.TypeUID == definitionRootUID);
        }

        public NodeData? FindBuildRoot()
        {
            return Root.PhysicalChildren.FirstOrDefault(n => n.TypeUID == buildRootUID);
        }

        public NodeData? FindCompileRoot()
        {
            return Root.PhysicalChildren.FirstOrDefault(n => n.TypeUID == compileRootUID);
        }
    }
}
