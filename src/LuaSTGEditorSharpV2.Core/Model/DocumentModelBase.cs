using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Exception;

namespace LuaSTGEditorSharpV2.Core.Model
{
    public abstract class DocumentModelBase
    {
        protected static readonly JsonSerializerSettings _savingSerializer = new()
        {
            Formatting = Formatting.Indented
        };

        public static NodeData CreateFromFile(string filePath)
        {
            try
            {
                string testSrc;
                using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read))
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

        public abstract NodeData Root { get; protected set; }

        public DocumentModelBase()
        {
            Root = new NodeData();
        }

        public abstract void SaveAs(string filePath);
    }
}
