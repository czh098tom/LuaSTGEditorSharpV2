using System.Text;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Specialized;

[Inject(ServiceLifetime.Transient)]
public sealed class RepeatCodeGeneration(
    CodeGeneratorServiceProvider provider, IServiceProvider services) : CodeGeneratorServiceBase(provider, services)
{
    [JsonProperty(Required = Required.Always)] public CaptureWithMacroOption TimesCapture { get; private set; } = null!;
    [JsonProperty(Required = Required.Always)] public NodePropertyCapture CountCapture { get; private set; } = null!;
    [JsonProperty(Required = Required.Always)] public NodePropertyCapture NameRule { get; private set; } = null!;
    [JsonProperty(Required = Required.Always)] public NodePropertyCapture InitRule { get; private set; } = null!;
    [JsonProperty(Required = Required.Always)] public NodePropertyCapture IncrementRule { get; private set; } = null!;

    protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
    {
        var token = new NodePropertyAccessToken(ServiceProvider, node, context);
        var times = TimesCapture.ApplyMacro(token, context);
        var indent = context.Format("{0:IND}");
        var initialization = new StringBuilder();
        var update = new StringBuilder();
        if (int.TryParse(CountCapture.Capture(token), out var count))
        {
            for (var i = 0; i < count; i++)
            {
                var name = NameRule.CaptureByFormat(token, i);
                var initial = InitRule.CaptureByFormat(token, i);
                var increment = IncrementRule.CaptureByFormat(token, i);
                if (string.IsNullOrWhiteSpace(name)) continue;
                initialization.Append($"{indent}local {name}, _d_{name} = ({initial}), ({increment})\n");
                update.Append($"{indent}{indent}{name} = {name} + _d_{name}\n");
            }
        }
        var hasVariables = initialization.Length > 0;
        var head = hasVariables
            ? $"do\n{initialization}{indent}for _ = 1, {times} do"
            : $"for _ = 1, {times} do";
        var tail = hasVariables ? $"{update}{indent}end\nend" : "end";
        yield return new CodeData(context.ApplyIndented(context.GetIndented(), head).ToString(), node);
        foreach (var code in NodeServiceProvider.GenerateForChildren(node, context, hasVariables ? 2 : 1))
        {
            yield return code;
        }
        yield return new CodeData(context.ApplyIndented(context.GetIndented(), tail).ToString(), node);
    }
}
