using System;
using System.Collections.Generic;
using System.Linq;
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

        captureResult.self["textLiteral"] = context.ToStringLiteral(text);
        return captureCount + 1;
    }


}
