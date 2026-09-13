using LuaSTGEditorSharpV2.Core.Parsing.Lua;

namespace LuaSTGEditorSharpV2.Core.Parsing.Argb;

public static class ArgbExpressionCodec
{
    public static ArgbExpression? Decode(string expression)
    {
        var result = LuaExpressionLexer.Tokenize(expression);
        if (!result.Success)
        {
            return null;
        }

        var components = SplitComponents(result.Value);
        if (components.Length != 4 ||
            components.Any(component => component.Length == 0))
        {
            return null;
        }

        return new ArgbExpression(
            LuaExpressionTokenSequence.GetText(components[0]),
            LuaExpressionTokenSequence.GetText(components[1]),
            LuaExpressionTokenSequence.GetText(components[2]),
            LuaExpressionTokenSequence.GetText(components[3]));
    }

    public static ArgbPreview? CreatePreview(ArgbExpression expression)
    {
        if (!TryCreatePreviewChannel(
                expression.Alpha,
                out var alpha,
                out var alphaIsLiteral) ||
            !TryCreatePreviewChannel(
                expression.Red,
                out var red,
                out var redIsLiteral) ||
            !TryCreatePreviewChannel(
                expression.Green,
                out var green,
                out var greenIsLiteral) ||
            !TryCreatePreviewChannel(
                expression.Blue,
                out var blue,
                out var blueIsLiteral))
        {
            return null;
        }

        return alphaIsLiteral ||
               redIsLiteral ||
               greenIsLiteral ||
               blueIsLiteral
            ? new ArgbPreview(alpha, red, green, blue)
            : null;
    }

    public static string Compose(ArgbExpression expression)
        => $"{expression.Alpha.Trim()}, " +
           $"{expression.Red.Trim()}, " +
           $"{expression.Green.Trim()}, " +
           $"{expression.Blue.Trim()}";

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

    private static bool TryCreatePreviewChannel(
        string expression,
        out byte value,
        out bool isLiteral)
    {
        value = default;
        isLiteral = false;

        var result = LuaExpressionLexer.Tokenize(expression);
        if (!result.Success)
        {
            return false;
        }

        var tokens = LuaExpressionTokenSequence.Trim(result.Value);
        if (tokens.Length == 0)
        {
            return false;
        }

        if (!TryGetNumber(tokens, out var number))
        {
            value = byte.MaxValue;
            return true;
        }

        if (double.IsNaN(number) ||
            number < 0 ||
            number > byte.MaxValue)
        {
            return false;
        }

        value = (byte)Math.Floor(number);
        isLiteral = true;
        return true;
    }

    private static bool TryGetNumber(
        LuaExpressionToken[] tokens,
        out double value)
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

        value = atom.Value switch
        {
            long integer => integer,
            double floating => floating,
            _ => double.NaN,
        };
        return true;
    }
}
