using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Persistence14.Dyes;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DyeableComponent : Component
{
    public bool Dyed => Color is not null;

    [DataField, AutoNetworkedField]
    public Color? Color;

    [DataField, AutoNetworkedField]
    public Color? DefaultColor;

    [DataField]
    public HashSet<string> Layers = new();
}

[Serializable, NetSerializable]
public enum DyeableVisuals : byte
{
    Color
}