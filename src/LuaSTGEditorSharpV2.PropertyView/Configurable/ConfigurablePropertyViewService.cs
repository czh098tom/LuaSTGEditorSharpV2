using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using Newtonsoft.Json;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public class ConfigurablePropertyViewService(PropertyViewServiceProvider nodeServiceProvider, IServiceProvider serviceProvider)
        : PropertyViewServiceBase(nodeServiceProvider, serviceProvider)
    {
    }
}
