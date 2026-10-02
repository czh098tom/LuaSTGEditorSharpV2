using LuaSTGEditorSharpV2.Core.Model;
using NCalc;
using Stubble.Core;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;

public static class CodeFragmentTemplate
{
    public static object CreateModel(IServiceProvider services, NodeData node, CodeGenerationContext context,
        IReadOnlyDictionary<string, CaptureWithMacroOption> captures,
        IReadOnlyDictionary<string, ContextCapture> contextCaptures,
        IReadOnlyDictionary<string, string> conditionTemplates,
        IReadOnlyDictionary<string, object?>? arguments = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(captures);
        ArgumentNullException.ThrowIfNull(contextCaptures);
        ArgumentNullException.ThrowIfNull(conditionTemplates);

        var self = new Dictionary<string, object?>();
        var capturedContext = new Dictionary<string, Dictionary<string, object?>>();
        var condition = new Dictionary<string, bool>();
        var token = new NodePropertyAccessToken(services, node, context);
        foreach (var entry in captures)
        {
            self.Add(entry.Key, entry.Value.ApplyMacro(token, context));
        }
        foreach (var entry in contextCaptures)
        {
            var values = new Dictionary<string, object?>();
            var contextNode = entry.Value.ResolveNode(context);
            foreach (var property in entry.Value.Property)
            {
                values.Add(property.Key, contextNode is null
                    ? string.Empty
                    : property.Value.ApplyMacro(new NodePropertyAccessToken(services, contextNode, context), context));
            }
            capturedContext.Add(entry.Key, values);
        }

        var model = new
        {
            self,
            args = arguments ?? new Dictionary<string, object?>(),
            context = capturedContext,
            condition,
            logicalChildrenCount = new Func<object>(() => node.GetLogicalChildren().Count())
        };
        foreach (var entry in conditionTemplates)
        {
            var expression = new Expression(StaticStubbleRenderer.Render(entry.Value, model));
            if (expression.Evaluate() is not bool value)
            {
                throw new InvalidOperationException($"Condition '{entry.Key}' must evaluate to a Boolean.");
            }
            condition.Add(entry.Key, value);
        }
        return model;
    }

    public static CodeFragment Render(string[] template, object model, NodeData node, CodeGenerationContext context)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(context);
        if (template.Any(line => line is null))
        {
            throw new ArgumentException("Template lines cannot be null.", nameof(template));
        }
        if (template.Length == 0)
        {
            return new CodeFragment([]);
        }
        var rendered = RenderText(template, model, context);
        return new CodeFragment([new CodeData(context.ApplyIndented(context.GetIndented(), rendered).ToString(), node)]);
    }

    public static string RenderText(string[] template, object model, CodeGenerationContext context)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(context);
        if (template.Any(line => line is null))
        {
            throw new ArgumentException("Template lines cannot be null.", nameof(template));
        }
        return StaticStubbleRenderer.Render(string.Concat(template)
            .Replace("{0:IND}", context.Format("{0:IND}"), StringComparison.Ordinal), model);
    }
}
