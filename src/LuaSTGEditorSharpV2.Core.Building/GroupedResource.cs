﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public record GroupedResource(string Path, string TargetName, string ResourceGroup)
    {
    }
}
