using Content.Shared._Persistence14.PersistentIdentifier.Reference;
using Robust.Shared.GameStates;

namespace Content.Shared._Persistence14.Research.ResearchTree;

/// <summary>
/// A client depentant on a specific <see cref="ResearchTreeSourceComponent"/> for its tree. Used to access the node tree and unlock technologies.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResearchTreeClientComponent : Component
{
    [DataField(readOnly: true), AutoNetworkedField]
    public PersistentEntityReference SourceId = PersistentEntityReference.EmptyId;
}