using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Persistence14.Construction;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ConstructionTargetSpecifierComponent : Component
{
    [DataField("targets", required: true), AutoNetworkedField]
    public ConstructionTargetSpecifier[] ValidTargets { get; set; } = [];

    [DataField("index"), AutoNetworkedField]
    public int CurrentTargetIndex { get; set; } = 0;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class ConstructionTargetSpecifier
{
    [DataField("target", required: true)]
    public string TargetNode;

    [DataField("loc", required: true)]
    public LocId Loc;

    public void Deconstruct(out string targetNode, out LocId loc)
    {
        targetNode = TargetNode;
        loc = Loc;
    }
}