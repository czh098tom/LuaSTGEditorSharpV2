using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using NodeNetwork.Toolkit.ValueNode;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor
{
    public class BulletShapeEditorViewModel : ValueEditorViewModel<Contextual<BulletShape>>, IContextualValueEditorViewModel<BulletShape>
    {
        private const BulletShape DefaultValue = BulletShape.Circle;

        public BulletShape RawValue
        {
            get => Value?.Invoke(Parameter.Empty) ?? DefaultValue;
            set => Value = Contextual.Create(_ => value);
        }

        public BulletShapeEditorViewModel()
        {
            Value = Contextual.Create(_ => DefaultValue);
        }
    }
}
