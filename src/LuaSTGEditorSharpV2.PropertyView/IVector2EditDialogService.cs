namespace LuaSTGEditorSharpV2.PropertyView;

public record struct Vector2EditResult(string Expression, string X, string Y);

public interface IVector2EditDialogService
{
    Vector2EditResult? EditVector2(string title, string initialX, string initialY);
}
