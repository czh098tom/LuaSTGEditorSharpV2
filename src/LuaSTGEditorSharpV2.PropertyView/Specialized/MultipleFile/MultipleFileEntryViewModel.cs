using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.MultipleFile;

public class MultipleFileEntryViewModel
    : BoundPropertyItemViewModelBase<MultipleFilePropertyViewItemListTerm.ItemTerm>
{
    private MultipleFilePropertyViewItemListTerm.ItemTerm _term = null!;
    private IReadOnlyList<PropertySource> _sources = null!;
    private RelayCommand _delete = null!;
    private bool _disposed;

    public int Index => _term.Mapping.Index;
    public BoundProperty SourceProperty { get; } = new();
    public BoundProperty DestinationProperty { get; } = new();
    public ICommand Delete => _delete;

    public override void Initialize(IReadOnlyList<PropertySource> sources,
        LocalServiceParam localServiceParam, PropertyEditWizardProviderService propertyEditWizardProviderService)
    {
        base.Initialize(sources, localServiceParam, propertyEditWizardProviderService);
        _sources = sources;
    }

    protected override void ConfigureViewModel(MultipleFilePropertyViewItemListTerm.ItemTerm term)
    {
        _term = term;
        _delete = new RelayCommand(() => RaiseOnEdit(CreateDeleteEdit()), () => !_disposed);
    }

    protected override void ConfigureBinding(MultipleFilePropertyViewItemListTerm.ItemTerm term)
    {
        Bind(term.Mapping.Source).ToOne(SourceProperty);
        Bind(term.Mapping.Destination).ToOne(DestinationProperty);
    }

    public EditResult CreateDeleteEdit()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var commands = _sources.Select(source => MultipleFileEdits.Remove(
            source.Node, source.Token, _term.Files, Index, LocalServiceParam).Command)
            .OfType<CommandBase>().ToArray();
        return new EditResult(Commands.FromFilteredList(commands), true, LocalServiceParam);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _delete.NotifyCanExecuteChanged();
        }
        base.Dispose(disposing);
    }
}
