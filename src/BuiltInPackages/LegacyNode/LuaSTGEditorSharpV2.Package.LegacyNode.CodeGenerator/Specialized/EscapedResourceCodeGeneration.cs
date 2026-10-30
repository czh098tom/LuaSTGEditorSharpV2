using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized;

[Inject(ServiceLifetime.Transient)]
public sealed class EscapedResourceCodeGeneration(
    CodeGeneratorServiceProvider provider, IServiceProvider services)
    : ConfigurableCodeGeneration(provider, services)
{
    [JsonProperty(Required = Required.Always)]
    public CaptureWithMacroOption EscapeRule { get; private set; } = null!;

    protected override int GetCaptureCacheLength() => base.GetCaptureCacheLength() + 1;

    protected override int WriteCaptureResult(
        string?[] result, NodeData node, CodeGenerationContext context)
    {
        var count = base.WriteCaptureResult(result, node, context);
        var token = new NodePropertyAccessToken(ServiceProvider, node, context);
        result[count] = Escape(EscapeRule.ApplyMacro(token, context));
        return count + 1;
    }

    private static string Escape(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value)
        {
            if (character == '\\' || character == '\'' || character == '"')
            {
                builder.Append('\\');
            }
            builder.Append(character);
        }
        return builder.ToString();
    }
}
