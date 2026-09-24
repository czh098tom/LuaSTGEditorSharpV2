using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Resources;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.MultipleFile;

public class MultipleFileCountPropertyItemViewModel
    : BoundPropertyItemViewModelBase<MultipleFileCountPropertyItemTerm>
{
    private readonly BoundProperty _count = new();
    private FileListMapping _files = null!;
    private IReadOnlyList<PropertySource> _sources = null!;
    private RelayCommand _browse = null!;
    private RelayCommand _clear = null!;
    private bool _disposed;
    private bool _canEdit;
    private bool _canClear;

    public bool CanEdit => _canEdit;
    public bool HasInvalidCount => !CanEdit && !_disposed;
    public ICommand Clear => _clear;

    public override void Initialize(IReadOnlyList<PropertySource> sources,
        LocalServiceParam localServiceParam, PropertyEditWizardProviderService propertyEditWizardProviderService)
    {
        base.Initialize(sources, localServiceParam, propertyEditWizardProviderService);
        _sources = sources;
    }

    protected override void ConfigureViewModel(MultipleFileCountPropertyItemTerm term)
    {
        _files = term.Files.WithCount(term.Mapping);
        _browse = new RelayCommand(() =>
        {
            var result = WizardProviderService.GetEditResult("multiplefile", this, LocalServiceParam);
            if (result != null)
            {
                RaiseOnEdit(result);
            }
        }, () => CanEdit);
        _clear = new RelayCommand(() =>
        {
            var result = CreateClearEdit();
            if (result != null)
            {
                RaiseOnEdit(result);
            }
        }, () => _canClear);
        ShowEditWindow = _browse;
    }

    protected override void ConfigureBinding(MultipleFileCountPropertyItemTerm term)
    {
        Bind(term.Mapping).ToOne(_count);
    }

    protected override void OnBindingValuesApplied(string key, PropertyBinding binding, string[][] rows)
    {
        _canEdit = !_disposed
            && rows.All(row => FileListMapping.TryParseCount(row[0], out _));
        _canClear = _canEdit
            && rows.Any(row => FileListMapping.TryParseCount(row[0], out var count) && count > 0);
        NotifyCommands();
    }

    private void NotifyCommands()
    {
        RaisePropertyChanged(nameof(CanEdit));
        RaisePropertyChanged(nameof(HasInvalidCount));
        _browse?.NotifyCanExecuteChanged();
        _clear?.NotifyCanExecuteChanged();
    }

    private EditResult? CreateEdit(Func<PropertySource, EditResult?> createEdit)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var commands = _sources.Select(source => createEdit(source)?.Command)
            .OfType<CommandBase>().ToArray();
        var command = Commands.FromFilteredList(commands);
        return command is null ? null : new EditResult(command, true, LocalServiceParam);
    }

    public EditResult? CreateAppendEdit(IReadOnlyList<string> paths)
    {
        return CreateEdit(source => MultipleFileEdits.Append(
            source.Node, source.Token, _files, paths, LocalServiceParam));
    }

    public EditResult? CreateClearEdit()
    {
        return CreateEdit(source => MultipleFileEdits.Clear(
            source.Node, source.Token, _files, LocalServiceParam));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _canEdit = false;
            _canClear = false;
            NotifyCommands();
        }
        base.Dispose(disposing);
    }
}
