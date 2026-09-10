using LuaSTGEditorSharpV2.Core.Parsing.Lua;

namespace LuaSTGEditorSharpV2.Core.Parsing.Argb;

public static class ArgbExpressionCodec
{
    public static ArgbExpressionForm Decode(string expression)
    {
        var result = LuaExpressionLexer.Tokenize(expression);
        if (!result.Success)
        {
            return Invalid();
        }

        var components = SplitComponents(result.Value);
        if (components.Length != 4 ||
            components.Any(component => component.Length == 0))
        {
            return Invalid();
        }

        var values = components
            .Select(LuaExpressionTokenSequence.GetText)
            .ToArray();
        var argb = new ArgbExpression(
            values[0],
            values[1],
            values[2],
            values[3]);
        var preview = BuildPreview(components);

        return new ArgbExpressionForm(argb, preview);
    }

    public static string Compose(ArgbExpression expression)
        => $"{expression.Alpha.Trim()}, " +
           $"{expression.Red.Trim()}, " +
           $"{expression.Green.Trim()}, " +
           $"{expression.Blue.Trim()}";

    private static ArgbExpressionForm Invalid()
        => new(null, null);

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

    private static ArgbPreview? BuildPreview(
        LuaExpressionToken[][] components)
    {
        var channels = new ArgbPreviewChannel[components.Length];
        var hasNumber = false;

        for (var i = 0; i < components.Length; i++)
        {
            if (TryGetNumber(components[i], out var value))
            {
                hasNumber = true;
                if (double.IsNaN(value) ||
                    value < 0 ||
                    value > byte.MaxValue)
                {
                    return null;
                }

                channels[i] = new ArgbPreviewChannel(
                    (byte)Math.Floor(value),
                    false);
            }
            else
            {
                channels[i] = new ArgbPreviewChannel(
                    byte.MaxValue,
                    true);
            }
        }

        return hasNumber
            ? new ArgbPreview(
                channels[0],
                channels[1],
                channels[2],
                channels[3])
            : null;
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
