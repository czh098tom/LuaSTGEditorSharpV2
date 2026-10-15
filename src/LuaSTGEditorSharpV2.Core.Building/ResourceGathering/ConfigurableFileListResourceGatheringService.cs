using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.Building.ResourceGathering;

[Inject(ServiceLifetime.Transient)]
public class ConfigurableFileListResourceGatheringService(
    ResourceGatheringServiceProvider nodeServiceProvider,
    IServiceProvider serviceProvider)
    : ResourceGatheringServiceBase(nodeServiceProvider, serviceProvider)
{
    [JsonProperty]
    public FileListMapping Files { get; private set; } = new();

    public override IEnumerable<GroupedResource> GetResourcesToPackWithContext(
        NodeData node,
        ResourceGatheringContext context)
    {
        var token = new NodePropertyAccessToken(ServiceProvider, node, context);
        var mappings = Files.GetItems(Files.GetCount(token));
        var resources = new List<(string Source, string Destination)>();
        for (var index = 0; index < mappings.Count; index++)
        {
            var source = mappings[index].Source.Capture(token);
            var destination = mappings[index].Destination.Capture(token);
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
            {
                throw new InvalidOperationException(
                    $"Resource node '{node.TypeUID}', file {mappings[index].Index}: source and destination are required.");
            }
            resources.Add((source, destination));
        }

        foreach (var resource in resources)
        {
            foreach (var group in context.EnumerateResourceGroups())
            {
                yield return new GroupedResource(resource.Source, resource.Destination, group);
            }
        }
        foreach (var resource in NodeServiceProvider.ProceedChildren(node, context))
        {
            yield return resource;
        }
    }
}
