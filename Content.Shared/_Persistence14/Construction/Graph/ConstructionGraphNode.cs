using Robust.Shared.Serialization;

namespace Content.Shared._Persistence14.Construction.Graph;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class ConstructionGraphNode
{
    [DataField("id", required: true)]
    public string ID { get; private set; } = default!;

    [DataField]
    public ConstructionGraphEdge[] Edges = [];
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class ConstructionGraphEdge
{
    [DataField("node", required: true)]
    public string DestinationNodeId { get; private set; } = default!;

    [DataField]
    public ConstructionGraphCondition[] Conditions = [];

    [DataField]
    public ConstructionGraphStep[] Steps = [];

    public bool VerifyConditions(Entity<ConstructionComponent> construction, in ConstructionConditionContext ctx)
    {
        foreach (var condition in Conditions)
            if (!condition.VerifyCondition(construction, in ctx))
                return false;

        return true;
    }
}