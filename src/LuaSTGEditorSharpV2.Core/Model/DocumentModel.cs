using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Exception;
using System.Xml.Linq;

namespace LuaSTGEditorSharpV2.Core.Model
{
    public class DocumentModel : IDocument
    {
        public static readonly string RootUID = "Root";
        public static readonly string definitionRootUID = "DefinitionRoot";
        public static readonly string buildRootUID = "BuildRoot";
        public static readonly string compileRootUID = "CompileRoot";

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

        public static DocumentModel? CreateFromTemplate(string templateFilePath, string fileName)
        {
            try
            {
                using FileStream fs = new(templateFilePath, FileMode.Open, FileAccess.Read);
                using StreamReader sr = new(fs);
                var node = DocumentFormatBase.CreateByExtension(Path.GetExtension(templateFilePath)).CreateFromStream(sr);
                if (node == null) return null;
                return new DocumentModel(node, fileName);
            }
            catch (System.Exception e)
            {
                throw new OpenFileException($"Could not open file by path {templateFilePath}", e);
            }
        }

        public static DocumentModel? CreateEmpty(string fileName)
        {
            var node = new NodeData(RootUID);
            node.Add(new NodeData(definitionRootUID));
            node.Add(new NodeData(buildRootUID));
            node.Add(new NodeData(compileRootUID));
            return new DocumentModel(node, fileName);
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
            private set => _root = value;
        }

        public virtual string? FilePath { get; private set; }

        public virtual string FileName { get; private set; }

        public virtual bool IsOnDisk => !string.IsNullOrWhiteSpace(FilePath);

        public DocumentModel(string fileName)
        {
            FileName = fileName;
            _root = new NodeData();
        }

        private DocumentModel(string filePath, NodeData root)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            _root = root;
        }

        private DocumentModel(NodeData root, string fileName)
        {
            FileName = fileName;
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
    }
}
