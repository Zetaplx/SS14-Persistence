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
}

public enum ResearchTreeClientUiStateKey
{
    Tree
}