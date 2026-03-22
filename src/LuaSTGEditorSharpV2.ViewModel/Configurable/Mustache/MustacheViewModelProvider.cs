using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stubble.Core;
using NCalc;

namespace LuaSTGEditorSharpV2.ViewModel.Configurable.Mustache
{
	public class MustacheViewModelProvider(ViewModelProviderServiceProvider nodeServiceProvider, IServiceProvider serviceProvider)
		: ViewModelProviderServiceBase(nodeServiceProvider, serviceProvider)
	{
		protected struct CaptureResult
		{
			public Dictionary<string, object?> self = [];
			public Dictionary<string, bool> condition = [];

			public CaptureResult() { }
		}
		[JsonProperty] public Dictionary<string, NodePropertyCapture> Captures { get; private set; } = [];
		[JsonProperty] public Dictionary<string, string> ConditionTemplates { get; private set; } = [];
		[JsonProperty] public string Icon { get; private set; } = "";
		[JsonProperty] public LocalizableArray<string>? Text { get; private set; }

		private CaptureResult _captureResult;

		internal protected override void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource, NodeViewModelContext context)
		{
			var token = new NodePropertyAccessToken(ServiceProvider, dataSource, context);
			_captureResult = new();
			WriteCaptureResult(context, token, ref _captureResult);
			string template = GetLocalizedTextIfExists();
			viewModel.Text = StaticStubbleRenderer.Render(template, _captureResult);
			viewModel.Icon = Icon;
		}

		private string GetLocalizedTextIfExists()
		{
			return string.Join('\n', Text?.GetLocalized() ?? []);
		}

		protected virtual int WriteCaptureResult(NodeViewModelContext context, NodePropertyAccessToken token, ref CaptureResult captureResult)
		{
			int n = 0;
			foreach (var namedCapture in Captures)
			{
				captureResult.self[namedCapture.Key] = Captures[namedCapture.Key].Capture(token) ?? string.Empty;
				n++;
			}

			foreach (var kvp in ConditionTemplates)
			{
				var predicate = StaticStubbleRenderer.Render(kvp.Value, captureResult);
				bool result = (bool)(new Expression(predicate).Evaluate() ?? false);
				captureResult.condition[kvp.Key] = result;
			}

			return n;
		}
	}
}
