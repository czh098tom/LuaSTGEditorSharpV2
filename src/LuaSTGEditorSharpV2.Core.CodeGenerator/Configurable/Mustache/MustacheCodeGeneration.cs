using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		[JsonProperty] public Dictionary<string, CaptureWithMacroOption> Captures { get; private set; } = [];
		[JsonProperty] public Dictionary<string, ContextCapture> ContextCaptures { get; private set; } = [];
		[JsonProperty] public Dictionary<string, string> ConditionTemplates { get; private set; } = [];
		[JsonProperty] public string[] Head { get; private set; } = [];
		[JsonProperty] public string[] Tail { get; private set; } = [];
		[JsonProperty] public bool IgnoreChildren { get; private set; } = false;
		[JsonProperty] public int IndentionIncrement { get; private set; } = 1;

		protected MustacheCodeGenerationHash _captureResult;

		internal protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
		{
			_captureResult = new();
			WriteCaptureResult(ref _captureResult, node, context);
			if (Head.Length != 0)
				yield return new CodeData(context.RenderIndentedTemplate(string.Join('\n', Head), _captureResult).ToString(), node);
			if (!IgnoreChildren)
			{
				foreach (var cd in NodeServiceProvider.GenerateForChildren(node, context, IndentionIncrement))
				{
					yield return cd;
				}
			}
			if (Tail.Length != 0)
				yield return new CodeData(context.RenderIndentedTemplate(string.Join('\n', Tail), _captureResult).ToString(), node);
		}

		protected virtual int GetCaptureCacheLength()
		{
			int l = Captures.Count;
			foreach(var cc in ContextCaptures.Values)
			{
				l += cc.Property.Count;
			}
			l += ConditionTemplates.Count;
			return l;
		}

		protected virtual int WriteCaptureResult(ref MustacheCodeGenerationHash captureResult, NodeData node, CodeGenerationContext context)
		{
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
				var targetTypeUid = contextCapture.TypeUID;
				var contextNode = context.PeekType(targetTypeUid);
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
