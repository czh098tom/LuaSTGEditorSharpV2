﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Exception
{
    public class DuplicatedTypeIDException : System.Exception
    {
        public string TypeID { get; private set; }

        public DuplicatedTypeIDException(string typeID)
            : base($"TypeID of {typeID} duplicated.")
        {
            TypeID = typeID;
        }

        public DuplicatedTypeIDException(string typeID, System.Exception? innerException)
            : base($"TypeID of {typeID} duplicated.", innerException)
        {
            TypeID = typeID;
        }
    }
}
