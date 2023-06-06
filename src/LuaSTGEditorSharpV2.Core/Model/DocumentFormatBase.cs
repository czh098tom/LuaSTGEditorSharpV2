using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model.DocumentFormat;

namespace LuaSTGEditorSharpV2.Core.Model
{
    public abstract class DocumentFormatBase
    {
        public static string DefaultFormat { get; set; } = "xml";

        public static DocumentFormatBase Create(string? type = null) =>
            (type?.ToLower() ?? DefaultFormat) switch
            {
                "json" => new JsonFormat(),
                "xml" or _ => new XMLFormat()
            };

        public static DocumentFormatBase CreateByExtension(string? extension) =>
            extension?.ToLower() switch
            {
                ".lstgjson" => new JsonFormat(),
                ".lstgxml" => new XMLFormat(),
                _ => throw new FormatException("Unknown format.")
            };

        public abstract NodeData CreateFromStream(TextReader streamReader);
        public abstract void SaveToStream(NodeData root, TextWriter streamWriter);
    }
}
