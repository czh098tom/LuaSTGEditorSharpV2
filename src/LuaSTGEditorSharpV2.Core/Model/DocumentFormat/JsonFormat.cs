using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Exception;

namespace LuaSTGEditorSharpV2.Core.Model.DocumentFormat
{
    internal class JsonFormat : DocumentFormatBase
    {
        protected static readonly JsonSerializerSettings _savingSerializer = new()
        {
            Formatting = Formatting.Indented
        };

        public override NodeData? CreateFromStream(TextReader streamReader)
        {
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            return JsonConvert.DeserializeObject<NodeData>(text)
                ?? throw new InvalidOperationException("File opened is empty.");
        }

        public override void SaveToStream(NodeData root, TextWriter streamWriter)
        {
            streamWriter.Write(JsonConvert.SerializeObject(root, _savingSerializer));
            streamWriter.Close();
        }
    }
}
