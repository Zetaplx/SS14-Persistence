using Content.Shared._Persistence14.PersistentIdentifier.Reference;
using Content.Shared.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Shared._Persistence14.Research.ResearchTree;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState,]
public sealed partial class ResearchTreeSourceComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ResearchTreePrototype> Tree;

    [DataField(readOnly: true), AutoNetworkedField]
    public HashSet<ProtoId<TechnologyPrototype>> UnlockedTechnologies = new();

    [DataField(readOnly: true), AutoNetworkedField]
    public Dictionary<ProtoId<TechnologyPrototype>, ResearchEndTime> ResearchUnlockTimes = new();

    [DataField, AutoNetworkedField]
    public HashSet<PersistentEntityReference> Clients = new();

    [DataField]
    public int MaxResearch = 3;

    [DataField, AutoNetworkedField]
    public int ResearchPoints = 0;
}

[DataDefinition, NetSerializable, Serializable]
public sealed partial class ResearchEndTime
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan EndTime;

    public static implicit operator TimeSpan(ResearchEndTime var) => var.EndTime;
    public static implicit operator ResearchEndTime(TimeSpan var) => new ResearchEndTime { EndTime = var };
}
