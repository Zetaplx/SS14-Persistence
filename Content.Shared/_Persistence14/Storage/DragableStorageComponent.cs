namespace Content.Shared._Persistence14.Storage;

[RegisterComponent]
public sealed partial class DragableStorageComponent : Component
{
    [DataField]
    public bool AllowAnchored = false;

    [DataField]
    public bool AllowOpen = false;
}