namespace LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;

internal interface IPattern<TItem, TForm>
    where TForm : class
{
    TForm? Match(IEnumerable<TItem> input);

    IEnumerable<TItem> Reconstruct(TForm form);
}

