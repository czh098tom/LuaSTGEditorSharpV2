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
        private static readonly JsonSerializerSettings _savingSerializer = new()
        {
            Formatting = Formatting.Indented
        };

        public static NodeData CreateFromFile(string filePath)
        {
            try
            {
                string testSrc;
                using (FileStream fs = new(Path.Combine(filePath, "test.lstg"), FileMode.Open, FileAccess.Read))
                {
                    using StreamReader sr = new(fs);
                    testSrc = sr.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<NodeData>(testSrc)
                    ?? throw new InvalidOperationException("File opened is empty.");
            }
            catch (System.Exception e)
            {
                throw new OpenFileException($"Could not open file at {filePath}", e);
            }
        }

        public string? FilePath { get; private set; }

        public NodeData Root { get; private set; }

        public bool IsOnDisk => string.IsNullOrWhiteSpace(FilePath);

        public DocumentModel()
        {
            Root = new NodeData();
        }

        public DocumentModel(string filePath)
        {
            FilePath = filePath;
            Root = CreateFromFile(filePath);
        }

        public void Save()
        {
            if (FilePath == null) throw new InvalidOperationException("This document has not been saved to a path yet.");
            SaveAs(FilePath);
        }

        public void SaveAs(string filePath)
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
