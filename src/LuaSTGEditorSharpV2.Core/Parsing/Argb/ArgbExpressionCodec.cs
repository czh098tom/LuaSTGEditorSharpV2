using LuaSTGEditorSharpV2.Core.Parsing.Lua;

namespace LuaSTGEditorSharpV2.Core.Parsing.Argb;

public static class ArgbExpressionCodec
{
    public static ArgbExpressionForm Decode(string expression)
    {
        var result = LuaExpressionLexer.Tokenize(expression);
        if (!result.Success)
        {
            return Invalid(expression);
        }

        var components = SplitComponents(result.Value);
        if (components.Length != 4 ||
            components.Any(component => component.Length == 0))
        {
            return Invalid(expression);
        }

        var values = components
            .Select(LuaExpressionTokenSequence.GetText)
            .ToArray();
        var argb = new ArgbExpression(
            values[0],
            values[1],
            values[2],
            values[3]);
        var preview = new ArgbPreview(
            ToPreviewChannel(components[0]),
            ToPreviewChannel(components[1]),
            ToPreviewChannel(components[2]),
            ToPreviewChannel(components[3]));

        return new ArgbExpressionForm(
            expression,
            argb,
            preview);
    }

    public static string Compose(ArgbExpression expression)
        => $"{expression.Alpha.Trim()}, " +
           $"{expression.Red.Trim()}, " +
           $"{expression.Green.Trim()}, " +
           $"{expression.Blue.Trim()}";

    private static ArgbExpressionForm Invalid(string expression)
        => new(
            expression,
            null,
            null);

    private static LuaExpressionToken[][] SplitComponents(
        IReadOnlyList<LuaExpressionToken> tokens)
    {
        var components = new List<LuaExpressionToken[]>();
        var current = new List<LuaExpressionToken>();

        foreach (var token in tokens)
        {
            if (IsComma(token))
            {
                components.Add(
                    LuaExpressionTokenSequence.Trim(current));
                current.Clear();
            }
            else
            {
                current.Add(token);
            }
        }

        components.Add(
            LuaExpressionTokenSequence.Trim(current));
        return components.ToArray();
    }

    private static bool IsComma(LuaExpressionToken token)
        => token is LuaExpressionAtom
        {
            Kind: LuaExpressionAtomKind.Punctuation,
            Text: ",",
        };

    private static ArgbPreviewChannel ToPreviewChannel(
        LuaExpressionToken[] tokens)
        => TryGetByte(tokens, out var value)
            ? new ArgbPreviewChannel(value, false)
            : new ArgbPreviewChannel(byte.MaxValue, true);

    private static bool TryGetByte(
        LuaExpressionToken[] tokens,
        out byte value)
    {
        value = default;
        if (tokens is not
            [
                LuaExpressionAtom
                {
                    Kind: LuaExpressionAtomKind.Number,
                    Value: not null,
                } atom,
            ])
        {
            return false;
        }

        switch (atom.Value)
        {
            case long integer
                when integer is >= byte.MinValue and <= byte.MaxValue:
                value = (byte)integer;
                return true;

            case double floating
                when double.IsFinite(floating) &&
                     floating is >= byte.MinValue and <= byte.MaxValue &&
                     floating == Math.Truncate(floating):
                value = (byte)floating;
                return true;

            default:
                return false;
        }
    }
}
