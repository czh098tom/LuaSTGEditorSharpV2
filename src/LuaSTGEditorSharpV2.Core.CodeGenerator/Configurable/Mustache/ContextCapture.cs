using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache
{
	[Serializable]
	public record class ContextCapture
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore, Required = Required.DisallowNull)]
		public string? TypeUID { get; private set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore, Required = Required.DisallowNull)]
		public string[]? AnyOfTypeUIDs { get; private set; }
		[JsonProperty] public Dictionary<string, CaptureWithMacroOption> Property { get; private set; }

		public ContextCapture(string typeUID, Dictionary<string, CaptureWithMacroOption> property)
			: this(typeUID, property, null) { }

		[JsonConstructor]
		public ContextCapture(string? typeUID, Dictionary<string, CaptureWithMacroOption> property, string[]? anyOfTypeUIDs)
		{
			ContextCaptureSelector.Validate(typeUID, anyOfTypeUIDs);
			TypeUID = typeUID;
			AnyOfTypeUIDs = anyOfTypeUIDs;
			Property = property;
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
			=> ContextCaptureSelector.Validate(TypeUID, AnyOfTypeUIDs);

		internal NodeData? ResolveNode(NodeContext context)
			=> ContextCaptureSelector.Resolve(context, TypeUID, AnyOfTypeUIDs);
	}
}
