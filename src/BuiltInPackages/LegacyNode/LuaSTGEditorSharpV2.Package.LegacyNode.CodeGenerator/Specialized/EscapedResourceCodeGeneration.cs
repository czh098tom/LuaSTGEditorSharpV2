using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
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
    protected override int WriteCaptureResult(
        string?[] result, NodeData node, CodeGenerationContext context)
    {
        var count = base.WriteCaptureResult(result, node, context);
        for (var i = 0; i < Captures.Length; i++)
        {
            if (Captures[i].Key == "destination")
            {
                result[i] = Escape(result[i] ?? string.Empty);
            }
        }
        return count;
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
