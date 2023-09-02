using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.Core.Building.BuildAction
{
    public class DefaultBuildAction : BuildActionServiceBase
    {
        public override async Task BuildWithContextAsync(NodeData node, BuildActionContext context)
        {
            var compileRoot = context.LocalSettings.Source.FindCompileRoot();
            if (compileRoot != null)
            {
                CodeGeneratorServiceBase.GenerateCode(compileRoot, context.LocalSettings);
            }
            await base.BuildWithContextAsync(node, context);
        }
    }
}
