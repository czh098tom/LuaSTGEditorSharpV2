using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core
{
    public class NodeContext(IServiceProvider serviceProvider, LocalServiceParam localParam)
    {
        protected class ContextHandle(NodeContext context) : IDisposable
        {
            private enum ContextHandleState
            {
                Created,
                Stepped,
                Disposed,
            }

            protected readonly NodeContext _context = context;
            private readonly object _lock = new();

            private ContextHandleState _state = ContextHandleState.Created;

            public void Dispose()
            {
                lock (_lock)
                {
                    if (_state == ContextHandleState.Disposed || _state == ContextHandleState.Created) return;
                    DisposeImpl();
                    _state = ContextHandleState.Disposed;
                }
            }

            public void Step(NodeData nodeData)
            {
                lock (_lock)
                {
                    if (_state == ContextHandleState.Stepped || _state == ContextHandleState.Disposed) return;
                    StepImpl(nodeData);
                    _state = ContextHandleState.Stepped;
                }
            }

            protected virtual void StepImpl(NodeData nodeData)
            {
                lock (_context._lock)
                {
                    _context.PushBasicInfo(nodeData);
                }
            }

            protected virtual void DisposeImpl()
            {
                lock (_context._lock)
                {
                    _context.PopBasicInfo();
                }
            }
        }

        public LocalServiceParam LocalParam { get; private set; } = localParam;

        protected IServiceProvider ServiceProvider { get; private set; } = serviceProvider;

        private readonly Dictionary<string, Stack<NodeData>> _contextData = [];
        private readonly Stack<NodeData> _top = new();

        protected readonly object _lock = new();

        protected ContextFormatter? Formatter { private get; init; }

        public virtual IDisposable AcquireContextLevelHandle(NodeData current)
        {
            var handle = new ContextHandle(this);
            handle.Step(current);
            return handle;
        }

        protected void PushBasicInfo(NodeData current)
        {
            if (!_contextData.TryGetValue(current.TypeUID, out Stack<NodeData>? value))
            {
                value = new Stack<NodeData>();
                _contextData.Add(current.TypeUID, value);
            }

            value.Push(current);
            _top.Push(current);
        }

        protected NodeData PopBasicInfo()
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
            return [];
        }

        public IEnumerable<NodeData> EnumerateFromFarest()
        {
            if (_top.Count > 0)
            {
                return _top.Reverse();
            }
            return [];
        }

        public IEnumerable<NodeData> EnumerateTypeFromNearest(string type)
        {
            if (_contextData.ContainsKey(type))
            {
                return _contextData[type];
            }
            return [];
        }

        public IEnumerable<NodeData> EnumerateTypeFromFarest(string type)
        {
            if (_contextData.ContainsKey(type))
            {
                return _contextData[type].Reverse();
            }
            return [];
        }

        public string Format(string toAppend, params object?[] source)
        {
            return (Formatter ?? ContextFormatter.Instance).ApplyFormat(toAppend, source);
        }
    }

    public abstract class NodeContextWithSettings<TSettings> : NodeContext
        where TSettings : new()
    {
        protected TSettings ServiceSettings { get; private set; }

        public NodeContextWithSettings(IServiceProvider serviceProvider, LocalServiceParam localParam, TSettings serviceSettings)
            : base(serviceProvider, localParam)
        {
            ServiceSettings = serviceSettings;
        }
    }

    internal class DefaultNodeContext : NodeContextWithSettings<ServiceExtraSettingsBase>
    {
        internal DefaultNodeContext(IServiceProvider serviceProvider, LocalServiceParam localParam, ServiceExtraSettingsBase serviceSettings)
            : base(serviceProvider, localParam, serviceSettings) { }
    }
}
