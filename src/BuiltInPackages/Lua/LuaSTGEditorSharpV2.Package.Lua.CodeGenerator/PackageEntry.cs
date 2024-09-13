using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator
{
    public class PackageEntry : IServiceInstanceProvider<LanguageBase>
    {
        public IReadOnlyCollection<LanguageBase> GetServiceInstances(IServiceProvider serviceProvider)
        {
            return [new LuaLanguage(serviceProvider)];
        }
    }
}
