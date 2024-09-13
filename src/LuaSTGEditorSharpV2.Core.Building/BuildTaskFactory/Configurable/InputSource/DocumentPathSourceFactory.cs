using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory.Configurable.InputSource
{
    public class DocumentPathSourceFactory(BuildTaskFactoryServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : BuildTaskFactorySubService<IInputSourceVariable>(nodeServiceProvider, serviceProvider)
    {
        public override IInputSourceVariable CreateOutput(NodeData nodeData, BuildTaskFactoryContext context)
        {
            return new DocumentPathSource();
        }
    }
}
