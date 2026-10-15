using LuaSTGEditorSharpV2.Core.Parsing.Legacy;
using LuaSTGEditorSharpV2.Core.Parsing.Lua;
using LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;
using Pidgin;

namespace LuaSTGEditorSharpV2.Core.Parsing.Vector2.Patterns;

internal sealed class PolarVector2Pattern(
    ParserOptions options)
    : IPattern<LuaExpressionToken[], PolarVector2PatternForm>
{
    private static readonly Parser<
        LuaExpressionToken,
        LuaExpressionToken[]> CosFunctionFactor =
            Function("cos")
                .Before(Parser<LuaExpressionToken>.End);

    private static readonly Parser<
        LuaExpressionToken,
        LuaExpressionToken[]> SinFunctionFactor =
            Function("sin")
                .Before(Parser<LuaExpressionToken>.End);

    private readonly AdditiveTermsPattern _additive = new(options);
    private readonly MultiplicativeFactorsPattern _multiplicative =
        new(options);

    public PolarVector2PatternForm? Match(
        IEnumerable<LuaExpressionToken[]> input)
    {
        var components = input.ToArray();
        if (components is not [var xInput, var yInput])
        {
            return null;
        }

        var xTerms = _additive.Match(
            UnwrapWholeParentheses(xInput));
        var yTerms = _additive.Match(
            UnwrapWholeParentheses(yInput));
        if (xTerms is not [var xTerm]
            || yTerms is not [var yTerm])
        {
            return null;
        }

        var xFactors = _multiplicative.Match(
            UnwrapWholeParentheses(xTerm.Term));
        var yFactors = _multiplicative.Match(
            UnwrapWholeParentheses(yTerm.Term));
        if (xFactors is not { Length: > 0 }
            || yFactors is not { Length: > 0 }
            || xFactors.Any(factor => factor.Length == 0)
            || yFactors.Any(factor => factor.Length == 0))
        {
            return null;
        }

        var xFunctions = xFactors!
            .Select((factor, index) => new
            {
                Index = index,
                Argument = CosFunctionFactor.ParseOrNull(factor),
            })
            .Where(function => function.Argument is not null)
            .Select(function => new FunctionFactor(
                function.Index,
                function.Argument!))
            .ToArray();
        var yFunctions = yFactors!
            .Select((factor, index) => new
            {
                Index = index,
                Argument = SinFunctionFactor.ParseOrNull(factor),
            })
            .Where(function => function.Argument is not null)
            .Select(function => new FunctionFactor(
                function.Index,
                function.Argument!))
            .ToArray();

        return CreateForm(
            new ParsedAxis(xTerm.Sign, xFactors, xFunctions),
            new ParsedAxis(yTerm.Sign, yFactors, yFunctions),
            _multiplicative);
    }

    public IEnumerable<LuaExpressionToken[]> Reconstruct(
        PolarVector2PatternForm form)
        => new[]
        {
            BuildAxis(form.Radius, form.Angle, "cos"),
            BuildAxis(form.Radius, form.Angle, "sin"),
        };

    private static Parser<
        LuaExpressionToken,
        LuaExpressionToken[]> Function(string name)
        => LuaExpressionTokenParsers
            .Atom(LuaExpressionAtomKind.Identifier, name)
            .Before(LuaExpressionTokenParsers.SkipWhitespace)
            .Then(LuaExpressionTokenParsers.Group(
                LuaExpressionGroupKind.Parentheses))
            .Map(group =>
                LuaExpressionTokenSequence.Trim(group.Children))
            .Assert(argument => argument.Length > 0);

    private static PolarVector2PatternForm? CreateForm(
        ParsedAxis x,
        ParsedAxis y,
        MultiplicativeFactorsPattern multiplicative)
    {
        if (x.Sign != y.Sign)
        {
            return null;
        }

        var candidates =
            from xFunction in x.Functions
            from yFunction in y.Functions
            where LuaExpressionTokenSequence.EquivalentTo(
                xFunction.Argument,
                yFunction.Argument)
            let xRadius = RemoveAt(x.Factors, xFunction.Index)
            let yRadius = RemoveAt(y.Factors, yFunction.Index)
            where EquivalentFactors(xRadius, yRadius)
            select new PolarVector2PatternForm(
                BuildRadius(xRadius, x.Sign, multiplicative),
                xFunction.Argument);

        var distinct = candidates
            .Aggregate(
                new List<PolarVector2PatternForm>(),
                AddIfDistinct);
        return distinct is [var match] ? match : null;
    }

    private static List<PolarVector2PatternForm> AddIfDistinct(
        List<PolarVector2PatternForm> forms,
        PolarVector2PatternForm candidate)
    {
        if (!forms.Any(form =>
                LuaExpressionTokenSequence.EquivalentTo(
                    form.Radius,
                    candidate.Radius)
                && LuaExpressionTokenSequence.EquivalentTo(
                    form.Angle,
                    candidate.Angle)))
        {
            forms.Add(candidate);
        }

        return forms;
    }

    private static LuaExpressionToken[] BuildRadius(
        LuaExpressionToken[][] factors,
        Sign sign,
        MultiplicativeFactorsPattern multiplicative)
    {
        var radius = factors.Length == 0
            ?
            [
                new LuaExpressionAtom(
                    "1",
                    LuaExpressionAtomKind.Number),
            ]
            : multiplicative.Reconstruct(factors).ToArray();
        return sign == Sign.Negative
            ?
            [
                new LuaExpressionAtom(
                    "-",
                    LuaExpressionAtomKind.Operator),
                .. radius,
            ]
            : radius;
    }

    private LuaExpressionToken[] BuildAxis(
        LuaExpressionToken[] radius,
        LuaExpressionToken[] angle,
        string functionName)
    {
        var function = new LuaExpressionToken[]
        {
            new LuaExpressionAtom(
                functionName,
                LuaExpressionAtomKind.Identifier),
            new LuaExpressionGroup(
                LuaExpressionGroupKind.Parentheses,
                "(",
                ")",
                angle),
        };

        if (IsOne(radius, out var negative))
        {
            return negative
                ?
                [
                    new LuaExpressionAtom(
                        "-",
                        LuaExpressionAtomKind.Operator),
                    .. function,
                ]
                : function;
        }

        var formattedRadius = FormatRadius(radius);
        return _multiplicative
            .Reconstruct([formattedRadius, function])
            .ToArray();
    }

    private LuaExpressionToken[] FormatRadius(
        LuaExpressionToken[] radius)
    {
        var terms = _additive.Match(radius);
        return terms is { Count: > 1 }
            ?
            [
                new LuaExpressionGroup(
                    LuaExpressionGroupKind.Parentheses,
                    "(",
                    ")",
                    radius),
            ]
            : radius;
    }

    private static bool IsOne(
        LuaExpressionToken[] radius,
        out bool negative)
    {
        var tokens = radius
            .Where(token =>
                !LuaExpressionTokenSequence.IsWhitespace(token))
            .ToArray();
        negative = tokens is
        [
            LuaExpressionAtom
            {
                Kind: LuaExpressionAtomKind.Operator,
                Text: "-",
            },
            LuaExpressionAtom
            {
                Kind: LuaExpressionAtomKind.Number,
                Text: "1",
            },
        ];
        return negative || tokens is
        [
            LuaExpressionAtom
            {
                Kind: LuaExpressionAtomKind.Number,
                Text: "1",
            },
        ] or
        [
            LuaExpressionAtom
            {
                Kind: LuaExpressionAtomKind.Operator,
                Text: "+",
            },
            LuaExpressionAtom
            {
                Kind: LuaExpressionAtomKind.Number,
                Text: "1",
            },
        ];
    }

    private static LuaExpressionToken[] UnwrapWholeParentheses(
        IEnumerable<LuaExpressionToken> input)
    {
        var tokens = LuaExpressionTokenSequence.Trim(input);
        return tokens is
        [
            LuaExpressionGroup
            {
                Kind: LuaExpressionGroupKind.Parentheses,
            } group,
        ]
            ? UnwrapWholeParentheses(group.Children)
            : tokens;
    }

    private static LuaExpressionToken[][] RemoveAt(
        LuaExpressionToken[][] factors,
        int index)
        => factors
            .Where((_, factorIndex) => factorIndex != index)
            .ToArray();

    private static bool EquivalentFactors(
        LuaExpressionToken[][] left,
        LuaExpressionToken[][] right)
        => left.Length == right.Length
           && left.Zip(right).All(pair =>
               LuaExpressionTokenSequence.EquivalentTo(
                   pair.First,
                   pair.Second));

    private sealed record FunctionFactor(
        int Index,
        LuaExpressionToken[] Argument);

    private sealed record ParsedAxis(
        Sign Sign,
        LuaExpressionToken[][] Factors,
        FunctionFactor[] Functions);
}
