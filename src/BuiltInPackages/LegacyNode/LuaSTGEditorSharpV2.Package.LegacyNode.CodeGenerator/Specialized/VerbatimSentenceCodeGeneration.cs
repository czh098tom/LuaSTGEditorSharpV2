using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable;
using LuaSTGEditorSharpV2.Core.Configurable;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized;

[Inject(ServiceLifetime.Transient)]
public sealed class VerbatimSentenceCodeGeneration(
    CodeGeneratorServiceProvider provider, IServiceProvider services)
    : SelectiveCodeGeneration(provider, services)
{
    protected override IEnumerable<CodeData> GenerateCodeWithContext(
        NodeData node, CodeGenerationContext context)
    {
        var result = new string[Captures.Length
            + ContextCaptures.Sum(capture => capture.Property.Length)];
        WriteCaptureResult(result, node, context);
        var textIndex = Array.FindIndex(Captures, capture => capture.Key == "text");
        if (textIndex < 0)
        {
            throw new InvalidOperationException("Sentence requires a text capture.");
        }

        var text = result[textIndex];
        var selections = (Head ?? Array.Empty<Selection>())
            .Concat(Tail ?? Array.Empty<Selection>()).ToArray();
        var marker = "__SENTENCE_VERBATIM_TEXT__";
        while (result.Any(value => value.Contains(marker))
            || selections.Any(selection => selection.Text.Contains(marker))
            || context.Format("{0:IND}").Contains(marker))
        {
            marker = "_" + marker;
        }

        var masked = (string[])result.Clone();
        masked[textIndex] = marker;
        if (Head != null)
        {
            yield return new CodeData(Render(Head), node);
        }
        if (!IgnoreChildren)
        {
            foreach (var code in NodeServiceProvider.GenerateForChildren(
                node, context, IndentionIncrement))
            {
                yield return code;
            }
        }
        if (Tail != null)
        {
            yield return new CodeData(Render(Tail), node);
        }

        string Render(Selection[] section)
        {
            var builder = new StringBuilder();
            foreach (var selection in section)
            {
                if (selection.ShouldAppend(result))
                {
                    builder.Append(context.ApplyIndentedFormat(
                        selection.Text, masked).ToString().Replace(marker, text));
                }
            }
            return builder.ToString();
        }
    }
}
