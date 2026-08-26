using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Persistence14.Research.ResearchTree;

[NetSerializable, Serializable]
public sealed partial class ResearchTreeClientBoundUserInterfaceState : BoundUserInterfaceState
{
    public required Dictionary<ProtoId<TechnologyPrototype>, ResearchNode> Nodes;
    public required HashSet<ProtoId<TechnologyPrototype>> UnlockedTechnologies;
    public required Dictionary<ProtoId<TechnologyPrototype>, TimeSpan> RecipeUnlockTimers;
    public required int MaxResearch;
    public required int Points;

    public required bool Connected;
    public required List<ResearchTreeSourceSpecifier> ValidSources;

    public static ResearchTreeClientBoundUserInterfaceState Disconnected => new ResearchTreeClientBoundUserInterfaceState
    {
        Nodes = new(),
        UnlockedTechnologies = new(),
        RecipeUnlockTimers = new(),
        MaxResearch = 0,
        Points = 0,

        Connected = false,
        ValidSources = new()
    };
}

[NetSerializable, Serializable]
public sealed class ResearchTreeSourceSpecifier
{
    public required NetEntity SourceNetId;
    public required string SourceName;
    public required bool AlreadyConnected;
}

[ByRefEvent, NetSerializable, Serializable]
public record struct ResearchTreeClientConnectMessage(NetEntity Client, NetEntity Source);

[ByRefEvent, NetSerializable, Serializable]
public record struct ResearchTreeClientDisconnectMessage(NetEntity Client, NetEntity Source);

[ByRefEvent, NetSerializable, Serializable]
public record struct ResearchTreeSourceClearClientsMessage(NetEntity Client, NetEntity Source);

[ByRefEvent, NetSerializable, Serializable]
public record struct ResearchTreeStartResearchMessage(NetEntity Client, ProtoId<TechnologyPrototype> TechnologyId);

[ByRefEvent, NetSerializable, Serializable]
public record struct ResearchTreeCancelResearchMessage(NetEntity Client, ProtoId<TechnologyPrototype> TechnologyId);

[Serializable, NetSerializable]
public enum ResearchTreeClientUiKey
{
    Tree
}