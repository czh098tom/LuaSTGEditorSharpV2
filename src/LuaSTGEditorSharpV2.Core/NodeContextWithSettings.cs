﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core
{
    public class NodeContext
    {
        protected LocalServiceParam LocalParam { get; private set; }

        private readonly Dictionary<string, Stack<NodeData>> _contextData = [];
        private readonly Stack<NodeData> _top = new();

        public NodeContext(LocalServiceParam localParam)
        {
            LocalParam = localParam;
        }

        public virtual void Push(NodeData current)
        {
            if (!_contextData.TryGetValue(current.TypeUID, out Stack<NodeData>? value))
            {
                value = new Stack<NodeData>();
                _contextData.Add(current.TypeUID, value);
            }

            value.Push(current);
            _top.Push(current);
        }

        public virtual NodeData Pop()
        {
            if (_top.Count != 0)
            {
                var node = _top.Peek();
                _contextData[node.TypeUID].Pop();
                return _top.Pop();
            }
            else
            {
                throw new InvalidOperationException("Current context is empty.");
            }
        }

        public NodeData? PeekType(string type)
        {
            if (_contextData.ContainsKey(type) && _contextData[type].Count > 0)
            {
                return _contextData[type].Peek();
            }
            return null;
        }

        public NodeData? Peek()
        {
            if (_top.Count > 0)
            {
                return _top.Peek();
            }
            return null;
        }

        public IEnumerable<NodeData> EnumerateFromNearest()
        {
            if (_top.Count > 0)
            {
                return _top;
            }
            return Enumerable.Empty<NodeData>();
        }

        public IEnumerable<NodeData> EnumerateFromFarest()
        {
            if (_top.Count > 0)
            {
                return _top.Reverse();
            }
            return Enumerable.Empty<NodeData>();
        }

        public IEnumerable<NodeData> EnumerateTypeFromNearest(string type)
        {
            if (_contextData.ContainsKey(type))
            {
                return _contextData[type];
            }
            return Enumerable.Empty<NodeData>();
        }

        public IEnumerable<NodeData> EnumerateTypeFromFarest(string type)
        {
            if (_contextData.ContainsKey(type))
            {
                return _contextData[type].Reverse();
            }
            return Enumerable.Empty<NodeData>();
        }
    }

    public abstract class NodeContextWithSettings<TSettings> : NodeContext
        where TSettings : new()
    {
        protected TSettings ServiceSettings { get; private set; }

        public NodeContextWithSettings(LocalServiceParam localParam, TSettings serviceSettings)
            : base(localParam)
        {
            ServiceSettings = serviceSettings;
        }
    }

    internal class DefaultNodeContext : NodeContextWithSettings<ServiceExtraSettingsBase>
    {
        internal DefaultNodeContext(LocalServiceParam localParam, ServiceExtraSettingsBase serviceSettings)
            : base(localParam, serviceSettings) { }
    }
}