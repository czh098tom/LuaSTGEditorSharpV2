using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.Services
{
    [Inject(ServiceLifetime.Singleton)]
    public class ClipboardService
    {
        private static readonly int RETRY_COUNT = 100;

        public async void CopyNode(IReadOnlyList<NodeData> nodes)
        {
            var nodesStr = JsonConvert.SerializeObject(nodes.ToArray());
            int i = 0;
            bool finished = false;
            while (i < RETRY_COUNT && !finished)
            {
                try
                {
                    Clipboard.SetText(nodesStr);
                    finished = true;
                }
                catch (COMException) 
                {
                    await Task.Delay(10);
                }
                catch (Exception)
                {
                    throw;
                }
                i++;
            }
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
