using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory
{
    public abstract class BuildTaskFactorySubService<TOutput> : BuildTaskFactoryServiceBase,
        ISubNodeService<TOutput, BuildTaskFactoryContext>
    {
        public abstract TOutput CreateOutput(NodeData nodeData, BuildTaskFactoryContext context);
    }
}
