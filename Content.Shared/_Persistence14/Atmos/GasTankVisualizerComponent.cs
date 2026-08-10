using Robust.Shared.Utility;

namespace Content.Shared._Persistence14.Atmos;

[RegisterComponent]
public sealed partial class GasTankVisualizerComponent : Component
{
    [DataField]
    public Dictionary<GasTankVisualLayers, Color> LayerColors = new();

    [DataField]
    public Dictionary<GasTankVisualVariants, ResPath> Variants = new();

    [DataField]
    public string BorderState = "border";

    [DataField]
    public string BaseState = "base";
}

public enum GasTankVisualLayers
{
    Border,
    Base,
    StripeLower,
    StripeUpper
}

public enum GasTankVisualVariants
{
    Icon,
    Storage,
    InhandLeft,
    InhandRight,
    EquippedBelt,
    EquippedBackpack,
    EquippedSuitStorage,
    EquippedSuitStorage_Cat,
    EquippedSuitStorage_Dog,
    EquippedSuitStorage_Fox,
    EquippedSuitStorage_Hamster,
    EquippedSuitStorage_Kangaroo,
    EquippedSuitStorage_Pig,
    EquippedSuitStorage_Possum,
    EquippedSuitStorage_Puppy,
    EquippedSuitStorage_Sloth,
}