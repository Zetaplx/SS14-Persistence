namespace Content.Shared._Persistence14.Storage;

[RegisterComponent]
public sealed partial class StorageRackComponent : Component
{
    /// <summary>
    /// A list of slot IDs that may be filled with RackStorable entities
    /// </summary>
    [DataField(required: true)]
    public List<string> Slots = new();

    /// <summary>
    /// Determines if the rack must be anchored to accept new items
    /// </summary>
    [DataField]
    public bool RequireAnchored = true;

    /// <summary>
    /// Determines if the rack will allow only closed containers, or if opened containers may be stored.
    /// </summary>
    [DataField]
    public bool RequireClosedContainers = true;
}