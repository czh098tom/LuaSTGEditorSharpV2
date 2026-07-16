namespace LuaSTGEditorSharpV2.PropertyView.Configurable;

public abstract class NamedPropertyItemViewModel<TTerm>: BoundPropertyItemViewModelBase<TTerm>
    where TTerm: PropertyItemTerm
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            RaisePropertyChanged();
        }
    }
    protected override void ConfigureViewModel(TTerm term)
    {
        Name = term.Caption.GetLocalized();
    }
}