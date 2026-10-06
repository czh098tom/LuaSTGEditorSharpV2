using System;
using System.Collections.Generic;
using System.Linq;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.PropertyView;

public abstract class BoundPropertyItemViewModelBase<TTerm> : PropertyItemViewModelBase
    where TTerm: class
{
    private readonly Dictionary<string, PropertyBinding> _bindings = [];
    private readonly Dictionary<string, string[][]> _pulledValues = [];
    private IReadOnlyList<NodePropertyAccessToken> _tokens = [];

    public virtual void Initialize(IReadOnlyList<PropertySource> sources,
        LocalServiceParam localServiceParam, PropertyEditWizardProviderService propertyEditWizardProviderService)
    {
        base.Initialize(sources.Select(ps => ps.Node).ToList(), localServiceParam,
            propertyEditWizardProviderService);
        _tokens = sources.Select(ps => ps.Token).ToList();
        foreach (var source in sources)
        {
            source.Node.OnPropertyAdded += HandleEditorNodeOnPropertyAdded;
            source.Node.OnPropertyRemoved += HandleEditorNodeOnPropertyRemoved;
        }
    }

    public void Configure(TTerm term)
    {
        ConfigureViewModel(term);
        ConfigureBinding(term);
    }

    protected abstract void ConfigureViewModel(TTerm term);

    protected abstract void ConfigureBinding(TTerm term);

    protected PropertyBindingBuildingScope<TTerm> Bind(
        NodePropertyCapture capture,
        bool shouldRefreshView = false)
    {
        return new PropertyBindingBuildingScope<TTerm>(
            capture,
            shouldRefreshView,
            AddBinding);
    }

    protected void ForwardValueChanges(BoundProperty boundProperty, string propertyName)
    {
        boundProperty.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BoundProperty.Value))
            {
                RaisePropertyChanged(propertyName);
            }
        };
    }

    private void AddBinding(string key, PropertyBinding binding)
    {
        _bindings.Add(key, binding);
        for (var fieldIndex = 0; fieldIndex < binding.BoundProperties.Count; fieldIndex++)
        {
            var capturedFieldIndex = fieldIndex;
            binding.BoundProperties[fieldIndex].EditRequested += (_, _) =>
                RaiseOnEdit(CreateEditResult(key, binding, capturedFieldIndex));
        }
    }

    public void Populate()
    {
        foreach (var key in _bindings.Keys)
        {
            RefreshBinding(key);
        }
    }

    private void RefreshBinding(string key)
    {
        if (!_bindings.TryGetValue(key, out var binding))
        {
            return;
        }

        var rows = _tokens.Select(binding.Pull).ToArray();
        if (rows.Length == 0)
        {
            return;
        }

        _pulledValues[key] = rows;
        ApplyBindingValues(binding, rows);
        OnBindingValuesApplied(key, binding, rows);
    }

    protected virtual void OnBindingValuesApplied(
        string key,
        PropertyBinding binding,
        string[][] rows)
    {
    }

    private static void ApplyBindingValues(
        PropertyBinding binding,
        string[][] rows)
    {
        for (var fieldIndex = 0;
             fieldIndex < binding.BoundProperties.Count;
             fieldIndex++)
        {
            var firstValue = rows[0][fieldIndex];
            var hasConflict = rows.Any(
                row => row[fieldIndex] != firstValue);
            var boundProperty = binding.BoundProperties[fieldIndex];
            boundProperty.HasConflict = hasConflict;
            if (!hasConflict)
            {
                boundProperty.SetValueWithoutPushingCommand(firstValue);
            }
        }
    }

    private EditResult CreateEditResult(
        string key,
        PropertyBinding binding,
        int fieldIndex)
    {
        if (!_pulledValues.TryGetValue(key, out var rows))
        {
            rows = _tokens.Select(binding.Pull).ToArray();
            _pulledValues[key] = rows;
        }

        var newValue = binding.BoundProperties[fieldIndex].Value;
        var fieldCount = binding.BoundProperties.Count;
        var commands = new List<CommandBase?>();
        for (var sourceIndex = 0; sourceIndex < SourceNodes.Count; sourceIndex++)
        {
            var sourceNode = SourceNodes[sourceIndex];
            var updated = new string[fieldCount];
            Array.Copy(rows[sourceIndex], updated, fieldCount);
            updated[fieldIndex] = newValue;
            rows[sourceIndex] = updated;
            commands.Add(CheckedCommand.Property.Modify(
                sourceNode.Document,
                sourceNode.GetPath(),
                binding.Capture.Key,
                binding.Compose(updated)));
        }

        return new EditResult(
            Commands.FromEnumerable(commands),
            binding.ShouldRefreshView,
            LocalServiceParam);
    }

    private void HandleEditorNodeOnPropertyAdded(
        object? sender,
        EditorNodePropertyAddedEventArgs e)
    {
        RefreshBinding(e.Key);
    }

    private void HandleEditorNodeOnPropertyRemoved(
        object? sender,
        EditorNodePropertyRemovedEventArgs e)
    {
        RefreshBinding(e.Key);
    }

    protected override void HandleEditorNodeOnPropertyChanged(
        object? sender,
        EditorNodePropertyChangedEventArgs e)
    {
        RefreshBinding(e.Key);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var sourceNode in SourceNodes)
            {
                sourceNode.OnPropertyAdded -= HandleEditorNodeOnPropertyAdded;
                sourceNode.OnPropertyRemoved -= HandleEditorNodeOnPropertyRemoved;
            }
        }
        base.Dispose(disposing);
    }
}
