﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class SourceFromContext : IInputSourceVariable
    {
        public string Key { get; private set; }

        public SourceFromContext(string key)
        {
            Key = key;
        }

        public IReadOnlyList<string> GetVariable(BuildingContext context)
        {
            return context.GetVariables(Key);
        }
    }
}
