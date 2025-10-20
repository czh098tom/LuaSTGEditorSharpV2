using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache
{
    public struct MustacheCodeGenerationHash
    {
        public Dictionary<string, object?> self;
		public Dictionary<string, Dictionary<string, object?>> context;
		public Dictionary<string, bool> condition;

		public MustacheCodeGenerationHash()
		{
			self = [];
			context = [];
			condition = [];
		}
	}
}
