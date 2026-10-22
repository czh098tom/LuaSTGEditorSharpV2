using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

using Stubble.Core;
using NCalc;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache
{
	[Serializable]
	public class MustacheCodeGeneration(CodeGeneratorServiceProvider nodeServiceProvider, IServiceProvider serviceProvider)
		: CodeGeneratorServiceBase(nodeServiceProvider, serviceProvider)
	{
		protected struct CaptureResult
		{
			public Dictionary<string, object?> self = [];
			public Dictionary<string, Dictionary<string, object?>> context = [];
			public Dictionary<string, bool> condition = [];
			public Func<object>? logicalChildrenCount = null;
			public string indent = string.Empty;
			public CaptureResult() {}
		}
		[JsonProperty] public Dictionary<string, CaptureWithMacroOption> Captures { get; private set; } = [];
		[JsonProperty] public Dictionary<string, ContextCapture> ContextCaptures { get; private set; } = [];
		[JsonProperty] public Dictionary<string, string> ConditionTemplates { get; private set; } = [];
		[JsonProperty] public string[] Head { get; private set; } = [];
		[JsonProperty] public string[] Tail { get; private set; } = [];
		[JsonProperty] public bool IgnoreChildren { get; private set; } = false;
		[JsonProperty] public int IndentionIncrement { get; private set; } = 1;

		protected CaptureResult _captureResult;

		internal protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
		{
			_captureResult = new();
			WriteCaptureResult(ref _captureResult, node, context);
			var captureResult = _captureResult;
			if (Head.Length != 0)
			{
				foreach (var code in CodeFragmentTemplate.Render(Head, captureResult, node, context))
				{
					yield return code;
				}
			}
			if (!IgnoreChildren)
			{
				foreach (var cd in NodeServiceProvider.GenerateForChildren(node, context, IndentionIncrement))
				{
					yield return cd;
				}
			}
			if (Tail.Length != 0)
			{
				foreach (var code in CodeFragmentTemplate.Render(Tail, captureResult, node, context))
				{
					yield return code;
				}
			}
		}

		protected int WriteCaptureResult(ref CaptureResult captureResult, NodeData node, CodeGenerationContext context)
		{
			captureResult.logicalChildrenCount = () => node.GetLogicalChildren().Count();
			captureResult.indent = context.Format("{0:IND}");
			var token = new NodePropertyAccessToken(ServiceProvider, node, context);
			int n = 0;
			foreach(var namedCapture in Captures)
			{
				captureResult.self[namedCapture.Key] = Captures[namedCapture.Key].ApplyMacro(token, context);
				n++;
			}

			foreach (var namedContextCapture in ContextCaptures)
			{
				var contextCapture = namedContextCapture.Value;
				var contextNode = contextCapture.ResolveNode(context);
				captureResult.context[namedContextCapture.Key] = [];
				foreach (var namedContextPropertyCapture in contextCapture.Property)
				{
					if (contextNode != null)
					{
						var contextNodeToken = new NodePropertyAccessToken(ServiceProvider, contextNode, context);
						captureResult.context[namedContextCapture.Key][namedContextPropertyCapture.Key]
							= namedContextPropertyCapture.Value.ApplyMacro(contextNodeToken, context);
					}
					else
					{
						captureResult.context[namedContextCapture.Key][namedContextPropertyCapture.Key] = string.Empty;
					}
					n++;
				}
			}
			foreach (var kvp in ConditionTemplates)
			{
				var predicate = StaticStubbleRenderer.Render(kvp.Value, captureResult);
				bool result = (bool)(new Expression(predicate).Evaluate()??false);
				captureResult.condition[kvp.Key] = result;
			}

			return n;
		}

	}
}
