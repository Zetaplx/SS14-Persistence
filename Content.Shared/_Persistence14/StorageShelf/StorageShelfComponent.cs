using Robust.Shared.GameStates;

namespace Content.Shared._Persistence14.StorageShelf;

[RegisterComponent]
public sealed partial class StorageShelfComponent : Component
{
    [DataField]
    public int Capacity = 4;

    [DataField]
    public string ContainerId = "storage_shelf";
}