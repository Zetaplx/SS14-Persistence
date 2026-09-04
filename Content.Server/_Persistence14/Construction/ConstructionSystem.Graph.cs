using Content.Shared._Persistence14.Construction;
using Content.Shared._Persistence14.Construction.Graph;

namespace Content.Server._Persistence14.Construction;

public sealed partial class ConstructionSystem
{
    public bool TryGetNode(Entity<ConstructionComponent?> construction, out ConstructionGraphNode node)
    {
        node = default!;
        if (!Resolve(construction, ref construction.Comp))
            return false;

        var graphProto = _protoMan.Index(construction.Comp.GraphId);
        if (!graphProto.Nodes.TryGetValue(construction.Comp.CurrentNodeId, out var graphNode))
            return false;

        node = graphNode;
        return true;
    }
}