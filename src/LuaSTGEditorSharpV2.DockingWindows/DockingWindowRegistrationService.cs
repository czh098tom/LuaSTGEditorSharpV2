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

namespace LuaSTGEditorSharpV2.DockingWindows
{
    [PackedServiceProvider]
    public class DockingWindowRegistrationService(IServiceProvider serviceProvider) : PackedDataProviderServiceBase<DockingWindowDescriptor>(serviceProvider)
    {
        public class TypedResourceDictionaryKeySelector : ResourceDictKeySelector<object>
        {
            public TypedResourceDictionaryKeySelector()
            {
                var dict = new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/LuaSTGEditorSharpV2.DockingWindows;component/DockingTemplate.xaml")
                };
                Default = dict["Default"] as DataTemplate;
            }

            public override string CreateKey(object vm)
            {
                return vm.GetType().Name;
            }

            public override bool HasKeyFromSource(object vm)
            {
                return true;
            }
        }

        private record Operation
        {
            public record Remove(string Key) : Operation;
            public record Assign(string Key, DockingWindowDescriptor Desc) : Operation
            {
                public DataTemplate? Parse()
                {
                    var dict = new ResourceDictionary()
                    {
                        Source = Desc.DataTemplateResourceDictionaryUri
                    };
                    if (dict[Desc.DataTemplateKey] is DataTemplate dataTemplate)
                    {
                        return dataTemplate;
                    }
                    return null;
                }
            }
        }

        private readonly Lazy<TypedResourceDictionaryKeySelector> _selector = new();
        private ConcurrentQueue<Operation> _operations = [];

        protected override void OnActiveServiceAdded(DockingWindowDescriptor newValue)
        {
            base.OnActiveServiceAdded(newValue);
            _operations.Enqueue(new Operation.Assign(newValue.Key, newValue));
        }

        protected override void OnActiveServiceRemoved(DockingWindowDescriptor oldValue)
        {
            base.OnActiveServiceRemoved(oldValue);
            _operations.Enqueue(new Operation.Remove(oldValue.Key));
        }

        protected override void OnActiveServiceChanged(DockingWindowDescriptor oldValue, DockingWindowDescriptor newValue)
        {
            base.OnActiveServiceChanged(oldValue, newValue);
            _operations.Enqueue(new Operation.Assign(newValue.Key, newValue));
        }

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
