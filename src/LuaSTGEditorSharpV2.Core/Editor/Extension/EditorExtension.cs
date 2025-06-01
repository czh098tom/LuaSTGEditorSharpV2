using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.Core.Editor.Extension
{
    public static class EditorExtension
    {
        public static T GetRequiredNodeService<T>(this EditorNode editorNode)
            where T : notnull
        {
            return editorNode.ServiceProvider.GetRequiredKeyedService<T>(ScopeKey.EditorNode);
        }
    }
}
