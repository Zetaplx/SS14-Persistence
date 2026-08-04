namespace Content.Shared._Persistence14.ModuleHardsuit.Plating;

[RegisterComponent]
public sealed partial class ModsuitPlatingManagerComponent : Component
{
    [DataField(required: true)]
    public string PlatingContainer = "unknown";

    [DataField]
    public int MaxPlateCount = 3;
}