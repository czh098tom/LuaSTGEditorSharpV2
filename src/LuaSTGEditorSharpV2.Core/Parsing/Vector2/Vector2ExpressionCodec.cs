using LuaSTGEditorSharpV2.Core.Parsing.Lua;
using LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;
using LuaSTGEditorSharpV2.Core.Parsing.Vector2.Patterns;

namespace LuaSTGEditorSharpV2.Core.Parsing.Vector2;

public static class Vector2ExpressionCodec
{
    public static Vector2ExpressionForm Decode(string expression)
    {
        var result = LuaExpressionLexer.Tokenize(expression);
        if (!result.Success)
        {
            var fallback = new Vector2ExpressionPair(
                expression,
                string.Empty);
            return new Vector2ExpressionForm(
                fallback,
                [new Vector2ComponentForm(
                    fallback,
                    null,
                    null,
                    null)]);
        }

        var options = ParserOptions.Default;
        var components = new ComponentPattern(options)
            .Match(result.Value)
            ?? [];
        var cartesian = components.Length switch
        {
            0 => new Vector2ExpressionPair(
                string.Empty,
                string.Empty),
            1 => new Vector2ExpressionPair(
                LuaExpressionTokenSequence.GetText(components[0]),
                string.Empty),
            _ => new Vector2ExpressionPair(
                LuaExpressionTokenSequence.GetText(components[0]),
                LuaExpressionTokenSequence.GetText(components[1])),
        };

        return new Vector2ExpressionForm(
            cartesian,
            DecodeComponents(
                components.ElementAtOrDefault(0) ?? [],
                components.ElementAtOrDefault(1) ?? [],
                options));
    }

    public static Vector2ExpressionPair Decompose(string expression)
        => Decode(expression).Cartesian;

    public static Vector2ComponentForm DecodeComponent(
        Vector2ExpressionPair expression)
    {
        var xResult = LuaExpressionLexer.Tokenize(expression.X);
        var yResult = LuaExpressionLexer.Tokenize(expression.Y);
        if (!xResult.Success || !yResult.Success)
        {
            return new Vector2ComponentForm(
                expression,
                null,
                null,
                null);
        }

        var options = ParserOptions.Default;
        return DecodeComponentTokens(
            xResult.Value.ToArray(),
            yResult.Value.ToArray(),
            options) with
        {
            Cartesian = expression,
        };
    }

    public static string Compose(
        Vector2ExpressionPair expression)
        => Compose(expression.X, expression.Y);

    public static string Compose(
        string x,
        string y)
        => ParserOptions.Default.SpaceAfterComma
            ? $"{x}, {y}"
            : $"{x},{y}";

    public static Vector2ExpressionPair ComposeComponents(
        IEnumerable<Vector2ExpressionPair> components)
    {
        var materialized = components.ToArray();
        return new Vector2ExpressionPair(
            MergeAxis(materialized, component => component.X),
            MergeAxis(materialized, component => component.Y));
    }

    public static class Polar
    {
        public static Vector2ExpressionPair Compose(
            string radius,
            string angle)
        {
            radius = radius.Trim();
            angle = angle.Trim();
            if (radius.Length == 0)
            {
                return new Vector2ExpressionPair(
                    string.Empty,
                    string.Empty);
            }

            var cos = $"cos({angle})";
            var sin = $"sin({angle})";
            return radius switch
            {
                "1" or "+1" =>
                    new Vector2ExpressionPair(cos, sin),
                "-1" => new Vector2ExpressionPair(
                    $"-{cos}",
                    $"-{sin}"),
                _ => new Vector2ExpressionPair(
                    $"{FormatRadius(radius)}*{cos}",
                    $"{FormatRadius(radius)}*{sin}"),
            };
        }

        private static string FormatRadius(string radius)
        {
            var result = LuaExpressionLexer.Tokenize(radius);
            if (!result.Success)
            {
                return radius;
            }

            var terms = new AdditiveTermsPattern(
                    ParserOptions.Default)
                .Match(result.Value);
            return terms is { Count: > 1 }
                ? $"({radius})"
                : radius;
        }
    }

    public static class MemberAccess
    {
        public static Vector2ExpressionPair Compose(
            string source,
            string firstAccessor,
            string secondAccessor)
        {
            source = source.Trim();
            firstAccessor = firstAccessor.Trim();
            secondAccessor = secondAccessor.Trim();
            return new Vector2ExpressionPair(
                $"{source}.{firstAccessor}",
                $"{source}.{secondAccessor}");
        }
    }

    public static class IndexAccess
    {
        public static Vector2ExpressionPair Compose(
            string source,
            string firstAccessor,
            string secondAccessor)
        {
            source = source.Trim();
            firstAccessor = firstAccessor.Trim();
            secondAccessor = secondAccessor.Trim();
            return new Vector2ExpressionPair(
                $"{source}[{firstAccessor}]",
                $"{source}[{secondAccessor}]");
        }
    }

    private static Vector2ComponentForm[] DecodeComponents(
        LuaExpressionToken[] x,
        LuaExpressionToken[] y,
        ParserOptions options)
    {
        var additive = new AdditiveTermsPattern(options);
        var xTerms = additive.Match(x) ?? [];
        var yTerms = additive.Match(y) ?? [];
        var count = Math.Max(xTerms.Count, yTerms.Count);
        return Enumerable.Range(0, count)
            .Select(index => DecodeComponentTokens(
                index < xTerms.Count
                    ? additive.Reconstruct([xTerms[index]]).ToArray()
                    : Zero(),
                index < yTerms.Count
                    ? additive.Reconstruct([yTerms[index]]).ToArray()
                    : Zero(),
                options))
            .ToArray();
    }

    private static Vector2ComponentForm DecodeComponentTokens(
        LuaExpressionToken[] x,
        LuaExpressionToken[] y,
        ParserOptions options)
    {
        LuaExpressionToken[][] pair = [x, y];
        return new Vector2ComponentForm(
            new Vector2ExpressionPair(
                LuaExpressionTokenSequence.GetText(x),
                LuaExpressionTokenSequence.GetText(y)),
            ToPublic(new PolarVector2Pattern(options).Match(pair)),
            ToPublic(new MemberAccessVector2Pattern().Match(pair)),
            ToPublic(new IndexAccessVector2Pattern().Match(pair)));
    }

    private static PolarVector2Form? ToPublic(
        PolarVector2PatternForm? form)
        => form is null
            ? null
            : new PolarVector2Form(
                LuaExpressionTokenSequence.GetText(form.Radius),
                LuaExpressionTokenSequence.GetText(form.Angle));

    private static MemberAccessVector2Form? ToPublic(
        MemberAccessVector2PatternForm? form)
        => form is null
            ? null
            : new MemberAccessVector2Form(
                LuaExpressionTokenSequence.GetText(form.Source),
                LuaExpressionTokenSequence.GetText(form.FirstAccessor),
                LuaExpressionTokenSequence.GetText(form.SecondAccessor));

    private static IndexAccessVector2Form? ToPublic(
        IndexAccessVector2PatternForm? form)
        => form is null
            ? null
            : new IndexAccessVector2Form(
                LuaExpressionTokenSequence.GetText(form.Source),
                LuaExpressionTokenSequence.GetText(form.FirstAccessor),
                LuaExpressionTokenSequence.GetText(form.SecondAccessor));

    private static LuaExpressionToken[] Zero()
        =>
        [
            new LuaExpressionAtom(
                "0",
                LuaExpressionAtomKind.Number),
        ];

    private static string MergeAxis(
        Vector2ExpressionPair[] components,
        Func<Vector2ExpressionPair, string> selector)
    {
        var terms = components
            .Select(selector)
            .Select(term => term.Trim())
            .ToArray();
        var lastNonZero = Array.FindLastIndex(
            terms,
            term => !IsZero(term));
        return lastNonZero < 0
            ? string.Empty
            : string.Concat(terms
                .Take(lastNonZero + 1)
                .Select((term, index) => FormatTerm(term, index)));
    }

    private static string FormatTerm(string term, int index)
    {
        var value = term.Length == 0 ? "0" : term;
        return index > 0 && !value.StartsWith('-')
            ? "+" + value
            : value;
    }

    private static bool IsZero(string value)
        => value.Length == 0
           || double.TryParse(value, out var number)
           && number == 0;
}

