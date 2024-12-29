using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.WPF;
using LuaSTGEditorSharpV2.WPF.Services;

namespace LuaSTGEditorSharpV2.DockingWindows
{
    [PackedServiceProvider]
    public class DockingWindowRegistrationService(IServiceProvider serviceProvider) 
        : ResourceService<DockingWindowDescriptor, DataTemplate>(serviceProvider)
    {
        public class TypedResourceDictionaryKeySelector : DataTemplateSelector
        {
            public DataTemplate? Default { get; set; }

            public ResourceDictionary? ResourceDictionary { get; set; } = [];

            public TypedResourceDictionaryKeySelector()
            {
                var dict = new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/LuaSTGEditorSharpV2.DockingWindows;component/Docking.xaml")
                };
                Default = dict["Default"] as DataTemplate;
            }

            public override DataTemplate? SelectTemplate(object item, DependencyObject container)
            {
                if (item is ContentPresenter) return null;
                var dataTemplates = GetResourceDictionary();
                if (Default == null) throw new InvalidOperationException($"{nameof(dataTemplates)} has not been assigned");
                if (item == null) return Default;
                if (dataTemplates != null && HasKeyFromSource(item))
                {
                    string key = CreateKey(item);
                    if (dataTemplates.Contains(key))
                    {
                        return (DataTemplate)dataTemplates[key];
                    }
                    else
                    {
                        return Default;
                    }
                }
                else
                {
                    return Default;
                }
            }

            public ResourceDictionary? GetResourceDictionary()
            {
                return ResourceDictionary;
            }

            public string CreateKey(object vm)
            {
                return vm.GetType().Name;
            }

            public bool HasKeyFromSource(object vm)
            {
                return true;
            }
        }

        private readonly Lazy<TypedResourceDictionaryKeySelector> _selector = new();

        public DataTemplateSelector GetDataTemplateSelector()
        {
            var selector = _selector.Value;
            while (_operations.TryDequeue(out var op))
            {
                switch (op)
                {
                    case Operation.Assign assign:
                        selector.ResourceDictionary[assign.Desc.Key] = assign.Parse();
                        break;
                    case Operation.Remove remove:
                        selector.ResourceDictionary.Remove(remove.Key);
                        break;
                    default:
                        break;
                }
            }
            return selector;
        }
    }
}
