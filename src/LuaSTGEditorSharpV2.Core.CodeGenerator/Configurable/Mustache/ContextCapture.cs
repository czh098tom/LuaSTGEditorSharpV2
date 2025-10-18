using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache
{
	[Serializable]
	public record class ContextCapture
	{
		[JsonProperty] public string TypeUID { get; private set; }
		[JsonProperty] public Dictionary<string, CaptureWithMacroOption> Property { get; private set; }

		public ContextCapture(string typeUID, Dictionary<string, CaptureWithMacroOption> property)
		{
			TypeUID = typeUID;
			Property = property;
		}
	}
}
