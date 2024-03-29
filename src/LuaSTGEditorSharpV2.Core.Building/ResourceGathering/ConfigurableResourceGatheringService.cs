﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.ResourceGathering
{
    public class ConfigurableResourceGatheringService : ResourceGatheringServiceBase
    {
        [JsonProperty] public NodePropertyCapture? PathCapture { get; private set; }

        public override IEnumerable<GroupedResource> GetResourcesToPackWithContext(NodeData node
            , ResourceGatheringContext context)
        {
            string path = PathCapture?.Capture(node) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(path))
            {
                foreach (var s in context.EnumerateResourceGroups())
                {
                    yield return new GroupedResource(path, s);
                }
            }
            foreach (var rs in GetServiceProvider().ProceedChildren(node, context))
            {
                yield return rs;
            }
        }
    }
}
