namespace LuaSTGEditorSharpV2.Core.Parsing.Facade;

/// <summary>
/// Recognizes vector components written as r*cos(a), r*sin(a).
/// </summary>
public static class PolarVectorExpressionParser
{
    public static bool TryDecompose(
        string x,
        string y,
        out string radius,
        out string angle)
    {
        radius = string.Empty;
        angle = string.Empty;
        if (!TryParseComponent(x, "cos", out var xComponent)
            || !TryParseComponent(y, "sin", out var yComponent)
            || xComponent.Sign != yComponent.Sign)
        {
            return false;
        }

        var matches = new List<(Fragment[] Radius, Fragment[] Angle)>();
        foreach (var xFunction in xComponent.Functions)
        {
            foreach (var yFunction in yComponent.Functions)
            {
                if (!Equivalent(xFunction.Argument, yFunction.Argument))
                {
                    continue;
                }

                var xRadiusFactors = RemoveAt(xComponent.Factors, xFunction.Index);
                var yRadiusFactors = RemoveAt(yComponent.Factors, yFunction.Index);
                if (!Equivalent(xRadiusFactors, yRadiusFactors))
                {
                    continue;
                }

                var candidate = (
                    Radius: ReconstructRadius(xRadiusFactors, xComponent.Sign),
                    Angle: xFunction.Argument);
                if (!matches.Any(existing => Equivalent(existing.Radius, candidate.Radius)
                                             && Equivalent(existing.Angle, candidate.Angle)))
                {
                    matches.Add(candidate);
                }
            }
        }

        if (matches.Count != 1)
        {
            return false;
        }

        radius = FragmentParser.Reconstruct(matches[0].Radius);
        angle = FragmentParser.Reconstruct(matches[0].Angle);
        return true;
    }

    public static (string X, string Y) Compose(string radius, string angle)
    {
        radius = radius.Trim();
        angle = angle.Trim();

        if (radius.Length == 0)
        {
            return (string.Empty, string.Empty);
        }

        var cos = $"cos({angle})";
        var sin = $"sin({angle})";
        return radius switch
        {
            "1" or "+1" => (cos, sin),
            "-1" => ($"-{cos}", $"-{sin}"),
            _ => ($"{FormatRadius(radius)}*{cos}", $"{FormatRadius(radius)}*{sin}"),
        };
    }

    private static bool TryParseComponent(string value, string functionName, out ParsedComponent component)
    {
        component = default;
        if (!FragmentParser.TryParse(value, out var fragments))
        {
            return false;
        }

        var signed = SignedParser.Parse(NormalizeParentheses(fragments));
        var body = NormalizeParentheses(signed.Term);
        if (body.Length == 0)
        {
            return false;
        }

        if (AdditiveParser.Parse(body).Count != 1)
        {
            return false;
        }

        var factors = MultiplicativeParser.Parse(body);
        if (factors.Length == 0 || factors.Any(factor => factor.Length == 0))
        {
            return false;
        }

        var functions = new List<FunctionFactor>();
        for (var i = 0; i < factors.Length; i++)
        {
            if (TryParseFunction(factors[i], functionName, out var argument))
            {
                functions.Add(new FunctionFactor(i, argument));
            }
        }

        if (functions.Count == 0)
        {
            return false;
        }

        component = new ParsedComponent(signed.Sign, factors, functions);
        return true;
    }

    private static bool TryParseFunction(
        Fragment[] value,
        string functionName,
        out Fragment[] argument)
    {
        argument = [];
        value = NormalizeParentheses(value);
        if (value.Length <= functionName.Length)
        {
            return false;
        }

        var index = 0;
        for (; index < functionName.Length; index++)
        {
            if (value[index].Depth != 0 || value[index].Character != functionName[index])
            {
                return false;
            }
        }

        while (index < value.Length
               && value[index].Depth == 0
               && char.IsWhiteSpace(value[index].Character))
        {
            index++;
        }

        if (index >= value.Length
            || value[index] is not { Depth: 0, Character: '(' }
            || value[^1] is not { Depth: 0, Character: ')' })
        {
            return false;
        }

        var openingParenthesis = index;
        for (index = openingParenthesis + 1; index < value.Length - 1; index++)
        {
            if (value[index].Depth == 0)
            {
                return false;
            }
        }

        argument = Trim(RebaseDepth(value[(openingParenthesis + 1)..^1]));
        return argument.Length > 0;
    }

    private static Fragment[] NormalizeParentheses(Fragment[] value)
    {
        var normalized = Trim(value);
        while (HasWrappingParentheses(normalized))
        {
            normalized = Trim(RebaseDepth(normalized[1..^1]));
        }

        return normalized;
    }

    private static bool HasWrappingParentheses(Fragment[] value)
    {
        if (value.Length < 2
            || value[0] is not { Depth: 0, Character: '(' }
            || value[^1] is not { Depth: 0, Character: ')' })
        {
            return false;
        }

        return value[1..^1].All(fragment => fragment.Depth > 0);
    }

    private static Fragment[] RebaseDepth(Fragment[] value)
        => value.Select(fragment => fragment with { Depth = fragment.Depth - 1 }).ToArray();

    private static Fragment[] Trim(Fragment[] value)
    {
        var start = 0;
        while (start < value.Length && char.IsWhiteSpace(value[start].Character))
        {
            start++;
        }

        var end = value.Length - 1;
        while (end >= start && char.IsWhiteSpace(value[end].Character))
        {
            end--;
        }

        return end < start ? [] : value[start..(end + 1)];
    }

    private static Fragment[][] RemoveAt(Fragment[][] factors, int index)
        => factors.Where((_, factorIndex) => factorIndex != index).ToArray();

    private static bool Equivalent(Fragment[][] left, Fragment[][] right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        return left.Zip(right).All(pair => Equivalent(pair.First, pair.Second));
    }

    private static bool Equivalent(Fragment[] left, Fragment[] right)
    {
        left = NormalizeParentheses(left);
        right = NormalizeParentheses(right);

        return left.SequenceEqual(right);
    }

    private static Fragment[] ReconstructRadius(Fragment[][] factors, Sign sign)
    {
        var radius = factors.Length == 0
            ? [new Fragment('1', 0)]
            : MultiplicativeParser.Reconstruct(factors);
        return sign == Sign.Negative
            ? [new Fragment('-', 0), .. radius]
            : radius;
    }

    private static string FormatRadius(string radius)
    {
        var terms = AdditiveParser.Parse(FragmentParser.Parse(radius));
        return terms.Count > 1 ? $"({radius})" : radius;
    }

    private readonly record struct FunctionFactor(int Index, Fragment[] Argument);

    private readonly record struct ParsedComponent(
        Sign Sign,
        Fragment[][] Factors,
        List<FunctionFactor> Functions);
}
