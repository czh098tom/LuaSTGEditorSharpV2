using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Services
{
    public class ClipboardService
    {
        public void CopyNode(IReadOnlyList<NodeData> nodes)
        {
            Clipboard.SetText(JsonConvert.SerializeObject(nodes.ToArray()));
        }

        public IReadOnlyList<NodeData> GetNodes()
        {
            var clipBoardText = Clipboard.GetText();
            var serialized = JsonConvert.DeserializeObject<NodeData[]>(clipBoardText);
            return serialized ?? throw new InvalidOperationException();
        }

        public bool CheckHaveNodes()
        {
            try
            {
                var clipBoardText = Clipboard.GetText();
                var serialized = JsonConvert.DeserializeObject<NodeData[]>(clipBoardText);
                return serialized is not null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
