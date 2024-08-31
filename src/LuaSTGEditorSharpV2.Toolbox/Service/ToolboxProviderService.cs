using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Toolbox.Model;
using LuaSTGEditorSharpV2.Toolbox.ViewModel;

namespace LuaSTGEditorSharpV2.Toolbox.Service
{
    [ServiceName("Toolbox"), ServiceShortName("tool")]
    public class ToolboxProviderService(IServiceProvider serviceProvider)
        : PackedDataProviderServiceBase<ToolboxItemModelBase>(serviceProvider)
    {
        public static readonly char seperator = '/';
        public static readonly int orderUndefined = int.MaxValue - 1;

        private static void AddToListBasedOnOrder(IList<ToolboxItemViewModel> target,
            ToolboxItemViewModel toInsert, IReadOnlyDictionary<ToolboxItemViewModel, int> order)
        {
            for (int i = 0; i < target.Count; i++)
            {
                if (order[target[i]] > order[toInsert])
                {
                    target.Insert(i, toInsert);
                    return;
                }
            }
            target.Add(toInsert);
        }

        public IReadOnlyList<ToolboxItemViewModel> CreateTree()
        {
            // create all defined viewmodels
            Dictionary<string, ToolboxItemViewModel> created = [];
            Dictionary<ToolboxItemViewModel, int> createdOrder = [];
            var data = GetRegisteredAvailableData();
            foreach (var kvp in data)
            {
                created.Add(kvp.Key, kvp.Value.CreateViewModel());
                createdOrder.Add(created[kvp.Key], kvp.Value.Order);
            }

            // add suppliementary viewmodels
            foreach (var kvp in data)
            {
                List<string> currPath = [.. kvp.Key.Split(seperator)];
                while (currPath.Count > 0)
                {
                    var currPathStr = string.Join(seperator, currPath);
                    if (!created.ContainsKey(currPathStr))
                    {
                        created.Add(currPathStr, new ToolboxItemViewModel()
                        {
                            Caption = currPath.Last()
                        });
                        createdOrder.Add(created[currPathStr], orderUndefined);
                    }
                    currPath.RemoveAt(currPath.Count - 1);
                }
            }

            // build result
            List<ToolboxItemViewModel> list = [];
            Dictionary<string, ToolboxItemViewModel> unvisited = new(created);
            int inserted = 0;
            while (inserted < created.Count)
            {
                var kvp = unvisited.First();
                List<string> currPath = [.. kvp.Key.Split(seperator)];
                List<string> currPathFromBottom = new(currPath.Count);
                while (currPath.Count > 0)
                {
                    var parentPathFromBottomStr = string.Join(seperator, currPathFromBottom);
                    ToolboxItemViewModel? vmParent = created.GetValueOrDefault(parentPathFromBottomStr);
                    currPathFromBottom.Add(currPath[0]);
                    currPath.RemoveAt(0);
                    var currPathFromBottomStr = string.Join(seperator, currPathFromBottom);
                    if (unvisited.ContainsKey(currPathFromBottomStr))
                    {
                        var vm = created[currPathFromBottomStr];
                        if (vmParent != null)
                        {
                            AddToListBasedOnOrder(vmParent.Children, vm, createdOrder);
                        }
                        else
                        {
                            AddToListBasedOnOrder(list, vm, createdOrder);
                        }
                        inserted++;
                        unvisited.Remove(currPathFromBottomStr);
                    }
                }
            }
            return list;
        }
    }
}
