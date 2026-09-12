using NodeNetwork.ViewModels;
using System.Collections.Generic;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    /// <summary>
    /// 连接图成环检测，供 <see cref="LinqSTGNodeInputViewModel{T}"/> 的
    /// <c>ConnectionValidator</c> 使用。值管线里输入直接订阅所连输出的值流，
    /// 一旦两个节点的输入互相指向对方的输出（有向环），任一次重发射都会沿订阅
    /// 同步互相递归，直至栈溢出。拟建 输出→输入 一条边时，若从输出节点沿既有
    /// 连接逆流而上能到达输入节点，加上这条边必然成环，应拒绝建立。
    /// </summary>
    public static class ConnectionCycleDetector
    {
        public static bool WouldCreateCycle(NodeInputViewModel input, NodeOutputViewModel output)
        {
            var source = output.Parent;
            var target = input.Parent;
            if (source is null || target is null || ReferenceEquals(source, target))
            {
                return true;
            }

            var visited = new HashSet<NodeViewModel> { source };
            var queue = new Queue<NodeViewModel>();
            queue.Enqueue(source);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                foreach (var nodeInput in node.Inputs.Items)
                {
                    foreach (var connection in nodeInput.Connections.Items)
                    {
                        var upstream = connection.Output.Parent;
                        if (ReferenceEquals(upstream, target))
                        {
                            return true;
                        }
                        if (upstream != null && visited.Add(upstream))
                        {
                            queue.Enqueue(upstream);
                        }
                    }
                }
            }
            return false;
        }
    }
}
