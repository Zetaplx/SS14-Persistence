using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Persistence14.Research.ResearchTree;

/// <summary>
/// A node on the research tree. Identifies a specific tech prototype and its requirements.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public sealed partial class ResearchNode
{
    /// <summary>
    /// The technology to be unlocked
    /// </summary>
    [DataField(required: true)]
    public ProtoId<TechnologyPrototype> Technology;

    /// <summary>
    /// All required technologies. If empty, tech is always available.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<TechnologyPrototype>> Prerequisites = new();

    /// <summary>
    /// The grid position of this node in the UI. <0, 0> is the top left corner, x is to the right, y is down.
    /// </summary>
    [DataField("position", required: true)]
    public Vector2i UiPosition;

    /// <summary>
    /// The amount of time it will take to unlock this tech. If 0, unlock is instant.
    /// </summary>
    [DataField]
    public TimeSpan UnlockTime = TimeSpan.Zero;
}