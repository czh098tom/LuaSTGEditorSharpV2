using LuaSTGEditorSharpV2.Core.Parsing.Lua;
using LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;
using Pidgin;

namespace LuaSTGEditorSharpV2.Core.Parsing.Vector2.Patterns;

internal sealed record PrefixExpressionForm(
    LuaExpressionToken[] Source,
    LuaExpressionToken[] Accessor,
    Vector2ReferenceSyntax Syntax);

internal sealed class PrefixExpressionPattern
    : IPattern<LuaExpressionToken, PrefixExpressionForm>
{
    private static readonly Parser<
        LuaExpressionToken,
        LuaExpressionToken[]> Primary =
            LuaExpressionTokenParsers.Identifier
                .Or(LuaExpressionTokenParsers
                    .Group(LuaExpressionGroupKind.Parentheses)
                    .Cast<LuaExpressionToken>())
                .Capture();

    private static readonly Parser<
        LuaExpressionToken,
        LuaExpressionToken> CallArguments =
            LuaExpressionTokenParsers
                .Group(LuaExpressionGroupKind.Parentheses)
                .Cast<LuaExpressionToken>()
                .Or(LuaExpressionTokenParsers
                    .Group(LuaExpressionGroupKind.Braces)
                    .Cast<LuaExpressionToken>())
                .Or(LuaExpressionTokenParsers.Atom(
                    LuaExpressionAtomKind.String));

    private static readonly Parser<
        LuaExpressionToken,
        PrefixSuffix> MemberSuffix =
            LuaExpressionTokenParsers.SkipWhitespace
                .Then(LuaExpressionTokenParsers.Atom(
                    LuaExpressionAtomKind.Punctuation,
                    "."))
                .Then(LuaExpressionTokenParsers.SkipWhitespace)
                .Then(LuaExpressionTokenParsers.Identifier)
                .Slice((tokens, accessor) =>
                    (PrefixSuffix)new MemberPrefixSuffix(
                        tokens.ToArray(),
                        [accessor]));

    private static readonly Parser<
        LuaExpressionToken,
        PrefixSuffix> IndexSuffix =
            LuaExpressionTokenParsers.SkipWhitespace
                .Then(LuaExpressionTokenParsers.Group(
                    LuaExpressionGroupKind.Brackets)
                    .Assert(group =>
                        LuaExpressionTokenSequence.Trim(
                            group.Children).Length > 0))
                .Slice((tokens, group) =>
                    (PrefixSuffix)new IndexPrefixSuffix(
                        tokens.ToArray(),
                        LuaExpressionTokenSequence.Trim(
                            group.Children)));

    private static readonly Parser<
        LuaExpressionToken,
        PrefixSuffix> MethodCallSuffix =
            LuaExpressionTokenParsers.SkipWhitespace
                .Then(LuaExpressionTokenParsers.Atom(
                    LuaExpressionAtomKind.Punctuation,
                    ":"))
                .Then(LuaExpressionTokenParsers.SkipWhitespace)
                .Then(LuaExpressionTokenParsers.Identifier)
                .Then(LuaExpressionTokenParsers.SkipWhitespace)
                .Then(CallArguments)
                .Capture()
                .Map(tokens =>
                    (PrefixSuffix)new CallPrefixSuffix(tokens));

    private static readonly Parser<
        LuaExpressionToken,
        PrefixSuffix> CallSuffix =
            LuaExpressionTokenParsers.SkipWhitespace
                .Then(CallArguments)
                .Capture()
                .Map(tokens =>
                    (PrefixSuffix)new CallPrefixSuffix(tokens));

    private static readonly Parser<
        LuaExpressionToken,
        PrefixSuffix> Suffix =
            Parser.Try(MemberSuffix)
                .Or(Parser.Try(IndexSuffix))
                .Or(Parser.Try(MethodCallSuffix))
                .Or(CallSuffix);

    private static readonly Parser<
        LuaExpressionToken,
        PrefixExpressionForm> Matcher =
            Parser.Map(
                    (primary,
suffixes) =>
                        CreateForm(primary, suffixes),
                    Primary,
                    Suffix.Many())
                .Assert(form => form is not null)
                .Map(form => form!)
                .Before(Parser<LuaExpressionToken>.End);

    public PrefixExpressionForm? Match(
        IEnumerable<LuaExpressionToken> input)
        => Matcher.ParseOrNull(
            LuaExpressionTokenSequence.Trim(input));

    public IEnumerable<LuaExpressionToken> Reconstruct(
        PrefixExpressionForm form)
        => form.Syntax switch
        {
            Vector2ReferenceSyntax.Member =>
                form.Source
                    .Append(new LuaExpressionAtom(
                        ".",
                        LuaExpressionAtomKind.Punctuation))
                    .Concat(form.Accessor),
            Vector2ReferenceSyntax.Index =>
                form.Source.Append(new LuaExpressionGroup(
                    LuaExpressionGroupKind.Brackets,
                    "[",
                    "]",
                    form.Accessor)),
            _ => throw new ArgumentOutOfRangeException(nameof(form)),
        };

    private static PrefixExpressionForm? CreateForm(
        LuaExpressionToken[] primary,
        IEnumerable<PrefixSuffix> suffixes)
    {
        var materialized = suffixes.ToArray();
        if (materialized.LastOrDefault() is not
            AccessPrefixSuffix access)
        {
            return null;
        }

        var source = materialized
            .SkipLast(1)
            .Select(suffix => suffix.Tokens)
            .Prepend(primary)
            .SelectMany(tokens => tokens);
        return new PrefixExpressionForm(
            LuaExpressionTokenSequence.Trim(source),
            access.Accessor,
            access.Syntax);
    }

    private abstract record PrefixSuffix(
        LuaExpressionToken[] Tokens);

    private abstract record AccessPrefixSuffix(
        LuaExpressionToken[] Tokens,
        LuaExpressionToken[] Accessor,
        Vector2ReferenceSyntax Syntax)
        : PrefixSuffix(Tokens);

    private sealed record MemberPrefixSuffix(
        LuaExpressionToken[] Tokens,
        LuaExpressionToken[] Accessor)
        : AccessPrefixSuffix(
            Tokens,
            Accessor,
            Vector2ReferenceSyntax.Member);

    private sealed record IndexPrefixSuffix(
        LuaExpressionToken[] Tokens,
        LuaExpressionToken[] Accessor)
        : AccessPrefixSuffix(
            Tokens,
            Accessor,
            Vector2ReferenceSyntax.Index);

    private sealed record CallPrefixSuffix(
        LuaExpressionToken[] Tokens)
        : PrefixSuffix(Tokens);
}
