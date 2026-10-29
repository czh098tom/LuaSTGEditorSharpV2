using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Configurable;
using LuaSTGEditorSharpV2.Core.Model;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized;

[Inject(ServiceLifetime.Transient)]
public sealed class VerbatimSentenceCodeGeneration(
    CodeGeneratorServiceProvider provider, IServiceProvider services)
    : MustacheCodeGeneration(provider, services)
{
    [JsonProperty(Required = Required.Always)]
    public CaptureWithMacroOption TextCapture { get; private set; } = null!;

    protected override int WriteCaptureResult(ref CaptureResult captureResult, NodeData node, CodeGenerationContext context)
    {
        var captureCount = base.WriteCaptureResult(ref captureResult, node, context);
        var text = TextCapture.ApplyMacro(new NodePropertyAccessToken(ServiceProvider, node, context), context);

        if (context.LineObfuscated)
        {
            text = ToSingleLineLiteral(text);
        }
        else
        {
            var segs = text.Split(["\r\n", "\n\r", "\n", "\r"], StringSplitOptions.None);
            text = "[===[\n\b" + string.Join("\n\b", segs) + "]===]";
        }
        
        captureResult.self["textLiteral"] = text;
        return captureCount + 1;
    }

    private static string ToSingleLineLiteral(string text)
    {
        var builder = new StringBuilder(text.Length + 2).Append('"');
        for (var i = 0; i < text.Length; i++)
        {
            var character = text[i];
            if (character is '\r' or '\n')
            {
                if (i + 1 < text.Length && text[i + 1] != character && (text[i + 1] is '\r' or '\n')) i++;
                builder.Append("\\n");
            }
            else if (character is '\\' or '"') builder.Append('\\').Append(character);
            else if (character == '\t') builder.Append("\\t");
            else if (character < ' ' || character == '\u007f')
                builder.Append('\\').Append(((int)character).ToString("D3", CultureInfo.InvariantCulture));
            else builder.Append(character);
        }
        return builder.Append('"').ToString();
    }

}
