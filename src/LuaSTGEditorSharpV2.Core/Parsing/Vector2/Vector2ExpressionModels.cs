using LuaSTGEditorSharpV2.Core.Parsing.Lua;

namespace LuaSTGEditorSharpV2.Core.Parsing.Vector2;

public readonly record struct Vector2ExpressionPair(
    string X,
    string Y);

public enum Vector2ReferenceSyntax
{
    Member,
    Index,
}

public sealed record PolarVector2Form(
    string Radius,
    string Angle);

public sealed record MemberAccessVector2Form(
    string Source,
    string FirstAccessor,
    string SecondAccessor);

public sealed record IndexAccessVector2Form(
    string Source,
    string FirstAccessor,
    string SecondAccessor);

public sealed record Vector2ComponentForm(
    Vector2ExpressionPair Cartesian,
    PolarVector2Form? Polar,
    MemberAccessVector2Form? MemberAccess,
    IndexAccessVector2Form? IndexAccess);

public sealed record Vector2ExpressionForm(
    Vector2ExpressionPair Cartesian,
    Vector2ComponentForm[] Components);

internal sealed record PolarVector2PatternForm(
    LuaExpressionToken[] Radius,
    LuaExpressionToken[] Angle);

internal sealed record MemberAccessVector2PatternForm(
    LuaExpressionToken[] Source,
    LuaExpressionToken[] FirstAccessor,
    LuaExpressionToken[] SecondAccessor);

internal sealed record IndexAccessVector2PatternForm(
    LuaExpressionToken[] Source,
    LuaExpressionToken[] FirstAccessor,
    LuaExpressionToken[] SecondAccessor);
