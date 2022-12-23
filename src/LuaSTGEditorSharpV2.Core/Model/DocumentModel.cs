using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Exception;

namespace LuaSTGEditorSharpV2.Core.Model
{
    public class DocumentModel : DocumentModelBase
    {
        private NodeData _root;

        public override NodeData Root 
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

        public DocumentModel(string filePath)
        {
            FilePath = filePath;
            _root = CreateFromFile(filePath);
        }

        public void Save()
        {
            if (FilePath == null) throw new InvalidOperationException("This document has not been saved to a path yet.");
            SaveAs(FilePath);
        }

        public override void SaveAs(string filePath)
        {
            try
            {
                using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write);
                using StreamWriter sw = new(fs);
                sw.Write(JsonConvert.SerializeObject(Root, _savingSerializer));
            }
            catch (System.Exception e)
            {
                throw new SaveFileException($"Could not save to path {filePath}", e);
            }
            FilePath = filePath;
        }
    }
}
