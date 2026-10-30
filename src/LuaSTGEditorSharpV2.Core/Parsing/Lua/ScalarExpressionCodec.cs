using System.Globalization;
using LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;
using Sign = LuaSTGEditorSharpV2.Core.Parsing.Legacy.Sign;

namespace LuaSTGEditorSharpV2.Core.Parsing.Lua;

public static class ScalarExpressionCodec
{
    public static string Spin(string expression, decimal step, decimal? minimum, decimal? maximum)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return step.ToString(null, CultureInfo.InvariantCulture);
        }
        var result = LuaExpressionLexer.Tokenize(expression);
        if (!result.Success) return expression;
        var pattern = new AdditiveTermsPattern(ParserOptions.Default);
        var terms = pattern.Match(result.Value);
        if (terms is not { Count: > 0 }) return expression;
        if (terms[^1].Value is decimal value)
        {
            value = Math.Clamp(value + step, minimum ?? decimal.MinValue, maximum ?? decimal.MaxValue);
            if (terms.Count == 1) return Format(value);
            if (value == 0) terms.RemoveAt(terms.Count - 1);
            else terms[^1] = NumberTerm(value);
        }
        else
        {
            terms.Add(NumberTerm(step));
        }
        return LuaExpressionTokenSequence.GetText(pattern.Reconstruct(terms));
    }

    private static LuaSignedTerm NumberTerm(decimal value)
        => new(value < 0 ? Sign.Negative : Sign.Positive,
            [new LuaExpressionAtom(Format(Math.Abs(value)), LuaExpressionAtomKind.Number)]);

    private static string Format(decimal value) => value.ToString("G29", CultureInfo.InvariantCulture);
}
