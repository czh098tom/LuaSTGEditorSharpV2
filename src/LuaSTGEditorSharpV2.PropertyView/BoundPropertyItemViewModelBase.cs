using System;
using System.Collections.Generic;
using System.Linq;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.PropertyView;

public abstract class BoundPropertyItemViewModelBase<TTerm> : PropertyItemViewModelBase
    where TTerm: class
{
    private readonly Dictionary<string, PropertyBinding> _bindings = [];
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
            this,
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
        foreach (var boundProperty in binding.BoundProperties)
        {
            boundProperty.EditRequested += (_, _) => RaiseOnEdit(binding.EditResultResolver());
        }
    }

    public void Populate()
    {
        foreach (var kvp in _bindings)
        {
            var binding = kvp.Value;
            var values = _tokens.Select(binding.Capture.Capture).ToArray();
            var hasConflict = values.Any(v => v != values[0]);
            binding.HasConflict = hasConflict;
            if (!hasConflict)
            {
                binding.PullAction.Invoke(values[0]);
            }
        }
    }

    private void RefreshBinding(string key)
    {
        if (_bindings.TryGetValue(key, out var binding))
        {
            var values = _tokens.Select(binding.Capture.Capture).ToArray();
            var hasConflict = values.Any(v => v != values[0]);
            binding.HasConflict = hasConflict;
            if (!hasConflict)
            {
                binding.PullAction.Invoke(values[0]);
            }
        }
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
