using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.WPF.Services
{
    public abstract class ResourceService<TDescriptor, TResult>(IServiceProvider serviceProvider) 
        : PackedDataProviderServiceBase<TDescriptor>(serviceProvider)
        where TDescriptor : ResourceDictionaryDescriptor
        where TResult : class
    {
        protected record Operation
        {
            public record Remove(string Key) : Operation;
            public record Assign(string Key, TDescriptor Desc) : Operation
            {
                public TResult? Parse()
                {
                    var dict = new ResourceDictionary()
                    {
                        Source = Desc.DataTemplateResourceDictionaryUri
                    };
                    if (dict[Desc.DataTemplateKey] is TResult dataTemplate)
                    {
                        return dataTemplate;
                    }
                    return null;
                }
            }
        }

        protected ConcurrentQueue<Operation> _operations = [];

        protected override void OnActiveServiceAdded(TDescriptor newValue)
        {
            base.OnActiveServiceAdded(newValue);
            _operations.Enqueue(new Operation.Assign(newValue.Key, newValue));
        }

        protected override void OnActiveServiceRemoved(TDescriptor oldValue)
        {
            base.OnActiveServiceRemoved(oldValue);
            _operations.Enqueue(new Operation.Remove(oldValue.Key));
        }

        protected override void OnActiveServiceChanged(TDescriptor oldValue, TDescriptor newValue)
        {
            base.OnActiveServiceChanged(oldValue, newValue);
            _operations.Enqueue(new Operation.Assign(newValue.Key, newValue));
        }
    }
}
