using System.Globalization;
using Pidgin;

namespace LuaSTGEditorSharpV2.Core.Parsing.Lua;

public static class LuaExpressionLexer
{
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "and", "break", "do", "else", "elseif", "end", "false", "for",
        "function", "goto", "if", "in", "local", "nil", "not", "or",
        "repeat", "return", "then", "true", "until", "while",
    };

    private static readonly HashSet<string> OperatorSymbols = new(StringComparer.Ordinal)
    {
        "..", "==", "~=", "<=", ">=",
        "+", "-", "*", "/", "%", "^", "#", "<", ">", "=",
    };

    private static readonly Parser<char, char> Digit = Parser.Digit;

    private static readonly Parser<char, string> Digits =
        Digit.AtLeastOnceString();

    private static readonly Parser<char, char> HexDigit =
        Parser.OneOf("0123456789abcdefABCDEF");

    private static readonly Parser<char, string> HexDigits =
        HexDigit.AtLeastOnceString();

    private static readonly Parser<char, string> Exponent = Parser.Map(
        (marker, sign, digits) => marker.ToString() + sign + digits,
        Parser.OneOf('e', 'E'),
        Parser.OneOf('+', '-').Optional().Map(maybe => maybe.HasValue ? maybe.Value.ToString() : string.Empty),
        Digits);

    private static readonly Parser<char, string> HexExponent = Parser.Map(
        (marker, sign, digits) => marker.ToString() + sign + digits,
        Parser.OneOf('p', 'P'),
        Parser.OneOf('+', '-').Optional().Map(maybe => maybe.HasValue ? maybe.Value.ToString() : string.Empty),
        Digits);

    private static readonly Parser<char, string> DecimalFractionAfterWhole =
        Parser.Map(
            (dot, digits) => dot.ToString() + digits,
            Parser.Char('.'),
            Digit.ManyString());

    private static readonly Parser<char, string> DecimalNumber =
        Parser.Map(
                (whole, fraction, exponent) => whole + fraction + exponent,
                Digits,
                DecimalFractionAfterWhole.Optional()
                    .Map(maybe => maybe.HasValue ? maybe.Value : string.Empty),
                Exponent.Optional()
                    .Map(maybe => maybe.HasValue ? maybe.Value : string.Empty))
            .Or(Parser.Try(Parser.Map(
                (dot, fraction, exponent) => dot.ToString() + fraction + exponent,
                Parser.Char('.'),
                Digits,
                Exponent.Optional()
                    .Map(maybe => maybe.HasValue ? maybe.Value : string.Empty))));

    private static readonly Parser<char, string> HexFractionAfterWhole =
        Parser.Map(
            (dot, digits) => dot.ToString() + digits,
            Parser.Char('.'),
            HexDigit.ManyString());

    private static readonly Parser<char, string> HexNumber =
        Parser.Map(
            (prefix, whole, fraction, exponent) =>
                prefix + whole + fraction + exponent,
            Parser.Try(Parser.String("0x")).Or(Parser.Try(Parser.String("0X"))),
            HexDigits,
            HexFractionAfterWhole.Optional()
                .Map(maybe => maybe.HasValue ? maybe.Value : string.Empty),
            HexExponent.Optional()
                .Map(maybe => maybe.HasValue ? maybe.Value : string.Empty));

    private static readonly Parser<char, string> Number =
        Parser.Lookahead(Parser.Try(Parser.String("0x")).Or(Parser.Try(Parser.String("0X"))))
            .Then(HexNumber)
            .Or(DecimalNumber);

    private static readonly Parser<char, string> NumberText = Parser.Map(
        (number, _) => number,
        Number,
        Parser.Not(
            Parser.LetterOrDigit
                .Or(Parser.Char('_'))
                .Or(Parser.Char('.'))));

    private static readonly Parser<char, LuaExpressionAtom> WhitespaceAtom =
        Parser.OneOf(' ', '\t', '\r', '\n', '\f', '\v')
            .AtLeastOnceString()
            .Map(text => new LuaExpressionAtom(text, LuaExpressionAtomKind.Whitespace));

    private static readonly Parser<char, LuaExpressionAtom> NumberAtom =
        NumberText.Map(text => new LuaExpressionAtom(
            text,
            LuaExpressionAtomKind.Number,
            EvaluateNumber(text)));

    private static readonly Parser<char, string> Identifier = Parser.Map(
        (first, rest) => first.ToString() + rest,
        Parser.Letter.Or(Parser.Char('_')),
        Parser.LetterOrDigit.Or(Parser.Char('_')).ManyString());

    private static readonly Parser<char, LuaExpressionAtom> IdentifierOrKeywordAtom =
        Identifier.Map(text => new LuaExpressionAtom(
            text,
            Keywords.Contains(text)
                ? LuaExpressionAtomKind.Keyword
                : LuaExpressionAtomKind.Identifier));

    private static readonly Parser<char, LuaExpressionAtom> LongStringAtom = LongString();

    private static readonly Parser<char, LuaExpressionAtom> SymbolAtom =
        Parser.Try(Parser.String("..."))
            .Or(Parser.Try(Parser.String("..")))
            .Or(Parser.Try(Parser.String("::")))
            .Or(Parser.Try(Parser.String("==")))
            .Or(Parser.Try(Parser.String("~=")))
            .Or(Parser.Try(Parser.String("<=")))
            .Or(Parser.Try(Parser.String(">=")))
            .Or(Parser.OneOf('+', '-', '*', '/', '%', '^', '#', '<', '>', '=', ';', ':', ',', '.')
                .Map(character => character.ToString()))
            .Map(text => new LuaExpressionAtom(
                text,
                OperatorSymbols.Contains(text)
                    ? LuaExpressionAtomKind.Operator
                    : LuaExpressionAtomKind.Punctuation));

    private static readonly Parser<char, IReadOnlyList<LuaExpressionToken>> TokenSequence =
        Parser.Rec<char, LuaExpressionToken>(token => SingleToken(token))
            .Many()
            .Before(Parser<char>.End)
            .Map(tokens => (IReadOnlyList<LuaExpressionToken>)tokens.ToArray());

    public static Result<char, IReadOnlyList<LuaExpressionToken>> Tokenize(string input)
        => TokenSequence.Parse(input);

    private static Parser<char, LuaExpressionToken> SingleToken(
        Parser<char, LuaExpressionToken> token)
        => Parser.OneOf(new Parser<char, LuaExpressionToken>[]
        {
            WhitespaceAtom.Cast<LuaExpressionToken>(),
            LongStringAtom.Cast<LuaExpressionToken>(),
            ShortStringAtom('\'').Cast<LuaExpressionToken>(),
            ShortStringAtom('"').Cast<LuaExpressionToken>(),
            NumberAtom.Cast<LuaExpressionToken>(),
            IdentifierOrKeywordAtom.Cast<LuaExpressionToken>(),
            SymbolAtom.Cast<LuaExpressionToken>(),
            Group(token, LuaExpressionGroupKind.Parentheses, '(', ')'),
            Group(token, LuaExpressionGroupKind.Brackets, '[', ']'),
            Group(token, LuaExpressionGroupKind.Braces, '{', '}'),
        });

    private static Parser<char, LuaExpressionAtom> ShortStringAtom(char quote)
    {
        Parser<char, string> escaped = Parser.Map(
            (slash, character) => slash.ToString() + character.ToString(),
            Parser.Char('\\'),
            Parser<char>.Any);
        Parser<char, string> plain = Parser.AnyCharExcept(quote, '\\', '\n', '\r')
            .Map(character => character.ToString());
        Parser<char, string> content = escaped.Or(plain).ManyString();

        return Parser.Char(quote)
            .Then(content)
            .Before(Parser.Char(quote))
            .Map(text => new LuaExpressionAtom(
                quote.ToString() + text + quote.ToString(),
                LuaExpressionAtomKind.String));
    }

    private static Parser<char, LuaExpressionAtom> LongString()
    {
        Parser<char, (string OpeningText, int EqualsCount)> opening = Parser.Try(
            Parser.Map(
                (open, equals, close) => (open.ToString() + equals + close.ToString(), equals.Length),
                Parser.Char('['),
                Parser.Char('=').ManyString(),
                Parser.Char('[')));

        return opening.Bind(info =>
        {
            var closingText = "]" + new string('=', info.EqualsCount) + "]";
            Parser<char, string> closing = Parser.Try(Parser.String(closingText));
            Parser<char, string> content = Parser.Try(Parser.Not(closing))
                .Then(Parser<char>.Any)
                .ManyString();

            return content.Before(closing).Map(text => new LuaExpressionAtom(
                info.OpeningText + text + closingText,
                LuaExpressionAtomKind.String));
        });
    }

    private static object EvaluateNumber(string text)
    {
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (text.IndexOf('.') < 0 &&
                text.IndexOf('p') < 0 &&
                text.IndexOf('P') < 0 &&
                TryEvaluateHexInteger(text, out var integer))
            {
                return integer;
            }

            return EvaluateHexNumber(text);
        }

        if (text.IndexOf('.') < 0 &&
            text.IndexOf('e') < 0 &&
            text.IndexOf('E') < 0 &&
            long.TryParse(
                text,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var decimalInteger))
        {
            return decimalInteger;
        }

        return double.Parse(
            text,
            NumberStyles.Float,
            CultureInfo.InvariantCulture);
    }

    private static bool TryEvaluateHexInteger(
        string text,
        out long value)
    {
        value = 0;

        for (var index = 2; index < text.Length; index++)
        {
            var digit = HexDigitValue(text[index]);
            if (value > (long.MaxValue - digit) / 16)
            {
                value = default;
                return false;
            }

            value = value * 16 + digit;
        }

        return true;
    }

    private static double EvaluateHexNumber(string text)
    {
        var exponentIndex = text.IndexOf('p');
        if (exponentIndex < 0)
        {
            exponentIndex = text.IndexOf('P');
        }

        var mantissaEnd = exponentIndex < 0
            ? text.Length
            : exponentIndex;
        var pointIndex = text.IndexOf('.', 2, mantissaEnd - 2);
        var fractionalDigitCount = pointIndex < 0
            ? 0
            : mantissaEnd - pointIndex - 1;
        var explicitExponent = exponentIndex < 0
            ? 0d
            : double.Parse(
                text.AsSpan(exponentIndex + 1),
                NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture);

        const int retainedHexDigits = 14;
        var significand = 0d;
        var significantDigitCount = 0;
        var omittedDigitCount = 0;

        for (var index = 2; index < mantissaEnd; index++)
        {
            if (text[index] == '.')
            {
                continue;
            }

            var digit = HexDigitValue(text[index]);
            if (significantDigitCount == 0 && digit == 0)
            {
                continue;
            }

            if (significantDigitCount < retainedHexDigits)
            {
                significand = significand * 16d + digit;
                significantDigitCount++;
            }
            else
            {
                omittedDigitCount++;
            }
        }

        if (significantDigitCount == 0)
        {
            return 0d;
        }

        var binaryExponent =
            explicitExponent +
            4d * (omittedDigitCount - fractionalDigitCount);
        return significand * Math.Pow(2d, binaryExponent);
    }

    private static int HexDigitValue(char digit)
        => digit switch
        {
            >= '0' and <= '9' => digit - '0',
            >= 'a' and <= 'f' => digit - 'a' + 10,
            >= 'A' and <= 'F' => digit - 'A' + 10,
            _ => throw new ArgumentOutOfRangeException(nameof(digit)),
        };

    private static Parser<char, LuaExpressionToken> Group(
        Parser<char, LuaExpressionToken> token,
        LuaExpressionGroupKind kind,
        char opening,
        char closing)
        => Parser.Char(opening)
            .Then(token.Many())
            .Before(Parser.Char(closing))
            .Map(children => (LuaExpressionToken)new LuaExpressionGroup(
                kind,
                opening.ToString(),
                closing.ToString(),
                children.ToArray()));
}
