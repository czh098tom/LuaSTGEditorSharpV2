using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor
{
    public class StringValueEditorViewModel : ValueEditorViewModel<Contextual<string>>, IContextualValueEditorViewModel<string>
    {
        private static readonly Contextual<string> DefaultValue = Contextual.Create(dict => string.Empty);

        public string RawValue
        {
            get => Value?.Invoke(Parameter.Empty) ?? string.Empty;
            set => Value = Contextual.Create(dict => value);
        }

        public StringValueEditorViewModel()
        {
            Value = DefaultValue;
        }
    }
}
