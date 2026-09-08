using LuaSTGEditorSharpV2.Core.Parsing.Vector2;

namespace LuaSTGEditorSharpV2.Dialog.ViewModel.Vector2EditDialog;

internal static class Vector2ComponentEditorStateFactory
{
    internal static PolarEditorState? CreatePolarState(
        PolarVector2Form? form)
        => form is null
            ? null
            : new PolarEditorState(form.Radius, form.Angle);

    internal static MemberAccessEditorState? CreateMemberAccessState(
        string source,
        string firstAccessor,
        string secondAccessor)
        => new(source, firstAccessor, secondAccessor);

    internal static MemberAccessEditorState? CreateMemberAccessState(
        MemberAccessVector2Form? form)
        => form is null
            ? null
            : new (
                form.Source,
                form.FirstAccessor,
                form.SecondAccessor);

    internal static IndexAccessEditorState? CreateIndexAccessState(
        string source,
        string firstAccessor,
        string secondAccessor)
        => new(source, firstAccessor, secondAccessor);

    internal static IndexAccessEditorState? CreateIndexAccessState(
        IndexAccessVector2Form? form)
        => form is null
            ? null
            : new IndexAccessEditorState(
                form.Source,
                form.FirstAccessor,
                form.SecondAccessor);
}
