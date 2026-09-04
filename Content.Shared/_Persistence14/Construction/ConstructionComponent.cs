using Content.Shared._Persistence14.Construction.Graph;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Construction;

/// <summary>
/// A component used to store essential information regarding the current state of a construction graph for the entity.
/// </summary>
[RegisterComponent]
public sealed partial class ConstructionComponent : Component
{
    /// <summary>
    /// The prototype id of the relevant <see cref="ConstructionGraphPrototype"/> this entity is contained on.
    /// </summary>
    [DataField("graph", required: true)]
    public ProtoId<ConstructionGraphPrototype> GraphId;

    /// <summary>
    /// The ID of the current ConstructionGraphNode for this entity. Determines which edges are valid for future construction steps.
    /// </summary>
    [DataField("node", required: true)]
    public string CurrentNodeId = default!;

    [DataField(readOnly: true)]
    public Dictionary<int, int> CurrentEdgeSteps = new();
}