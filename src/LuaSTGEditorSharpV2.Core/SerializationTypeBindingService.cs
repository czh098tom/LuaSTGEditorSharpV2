using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using LuaSTGEditorSharpV2.Core.Serialization;

namespace LuaSTGEditorSharpV2.Core
{
    [Inject(ServiceLifetime.Singleton)]
    public class SerializationTypeBindingService(IServiceProvider serviceProvider) : JsonConverter
    {
        private class Descriptor
        {
            private readonly Dictionary<string, SerializationTypeBinderDescriptor> _bindToTypeDict = [];
            private readonly Dictionary<Type, SerializationTypeBinderDescriptor> _bindToNameDict = [];

            public IReadOnlyDictionary<string, SerializationTypeBinderDescriptor> BindToTypeDict => _bindToTypeDict;
            public IReadOnlyDictionary<Type, SerializationTypeBinderDescriptor> BindToNameDict => _bindToNameDict;

            public void Add(SerializationTypeBinderDescriptor desc)
            {
                _bindToNameDict.Add(desc.TargetType, desc);
                _bindToTypeDict.Add(desc.Name, desc);
            }
        }

        private readonly Dictionary<Type, Descriptor> _bindToDict = [];
        private readonly Dictionary<Type, bool> _isDefined = [];

        private readonly object _lock = new();
        private readonly DefaultSerializationBinder defaultSerializationBinder = new();

        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            string? typeName = jo["$type"]?.Value<string>();
            Type type = objectType;
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(typeName))
                {
                    if (_isDefined.GetValueOrDefault(type, false))
                    {
                        if (!_bindToDict.TryGetValue(type, out var desc))
                        {
                            desc = CreateDescriptor(type);
                            _bindToDict.Add(type, desc);
                        }
                        if (desc.BindToTypeDict.TryGetValue(typeName, out var serializationTypeBinderDescriptor))
                        {
                            type = serializationTypeBinderDescriptor.TargetType;
                        }
                        else
                        {
                            type = Type.GetType(typeName, throwOnError: false, ignoreCase: true) ?? type;
                        }
                    }
                }
            }
            object result = serviceProvider.GetService(type) 
                ?? Activator.CreateInstance(type) 
                ?? throw new InvalidOperationException();
            serializer.Populate(jo.CreateReader(), result);
            return result;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null) return;
            JObject.FromObject(value).WriteTo(writer);
        }

        public override bool CanConvert(Type objectType)
        {
            lock (_lock)
            {
                if (!_isDefined.TryGetValue(objectType, out bool value))
                {
                    value = objectType.GetCustomAttribute<JsonShortNamingAttribute>() != null;
                    _isDefined.Add(objectType, value);
                }
                return value;
            }
        }

        private Descriptor CreateDescriptor(Type baseType)
        {
            var descriptor = new Descriptor();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAssignableTo(baseType))
                    {
                        var attr = type.GetCustomAttribute<JsonTypeShortNameAttribute>();
                        if (attr != null && attr.SourceType == baseType)
                        {
                            descriptor.Add(new SerializationTypeBinderDescriptor(type, attr.Name));
                        }
                    }
                }
            }
            return descriptor;
        }
    }
}
