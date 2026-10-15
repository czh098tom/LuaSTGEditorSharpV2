using LuaSTGEditorSharpV2.Core.Parsing.Lua;
using LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;

namespace LuaSTGEditorSharpV2.Core.Parsing.Vector2.Patterns;

internal abstract class Vector2ReferencePattern<TForm>
    : IPattern<LuaExpressionToken[], TForm>
    where TForm : class
{
    private static readonly PrefixExpressionPattern Prefix = new();

    private readonly Vector2ReferenceSyntax _syntax;
    private readonly Func<
        LuaExpressionToken[],
        LuaExpressionToken[],
        LuaExpressionToken[],
        TForm> _create;
    private readonly Func<TForm, ReferenceParts> _deconstruct;

    protected Vector2ReferencePattern(
        Vector2ReferenceSyntax syntax,
        Func<
            LuaExpressionToken[],
            LuaExpressionToken[],
            LuaExpressionToken[],
            TForm> create,
        Func<TForm, ReferenceParts> deconstruct)
    {
        _syntax = syntax;
        _create = create;
        _deconstruct = deconstruct;
    }

    public TForm? Match(
        IEnumerable<LuaExpressionToken[]> input)
    {
        var components = input.ToArray();
        if (components is not [var firstInput, var secondInput]
            || Prefix.Match(firstInput) is not { } first
            || Prefix.Match(secondInput) is not { } second
            || first.Syntax != _syntax
            || second.Syntax != _syntax
            || !LuaExpressionTokenSequence.EquivalentTo(
                first.Source,
                second.Source))
        {
            return null;
        }

        return _create(
            first.Source,
            first.Accessor,
            second.Accessor);
    }

    public IEnumerable<LuaExpressionToken[]> Reconstruct(TForm form)
    {
        var parts = _deconstruct(form);
        return new[]
        {
            BuildAccess(parts.Source, parts.FirstAccessor),
            BuildAccess(parts.Source, parts.SecondAccessor),
        };
    }

    protected sealed record ReferenceParts(
        LuaExpressionToken[] Source,
        LuaExpressionToken[] FirstAccessor,
        LuaExpressionToken[] SecondAccessor);

    private LuaExpressionToken[] BuildAccess(
        LuaExpressionToken[] source,
        LuaExpressionToken[] accessor)
        => _syntax switch
        {
            Vector2ReferenceSyntax.Member => source
                .Append(new LuaExpressionAtom(
                    ".",
                    LuaExpressionAtomKind.Punctuation))
                .Concat(accessor)
                .ToArray(),
            Vector2ReferenceSyntax.Index => source
                .Append(new LuaExpressionGroup(
                    LuaExpressionGroupKind.Brackets,
                    "[",
                    "]",
                    accessor))
                .ToArray(),
            _ => throw new ArgumentOutOfRangeException(),
        };
}

internal sealed class MemberAccessVector2Pattern()
    : Vector2ReferencePattern<MemberAccessVector2PatternForm>(
        Vector2ReferenceSyntax.Member,
        (source, first, second) =>
            new MemberAccessVector2PatternForm(
                source,
                first,
                second),
        form => new ReferenceParts(
            form.Source,
            form.FirstAccessor,
            form.SecondAccessor));

internal sealed class IndexAccessVector2Pattern()
    : Vector2ReferencePattern<IndexAccessVector2PatternForm>(
        Vector2ReferenceSyntax.Index,
        (source, first, second) =>
            new IndexAccessVector2PatternForm(
                source,
                first,
                second),
        form => new ReferenceParts(
            form.Source,
            form.FirstAccessor,
            form.SecondAccessor));
