using Content.Shared.Whitelist;

namespace Content.Shared._Persistence14.Research.TechDisk;

[RegisterComponent]
public sealed partial class TechnologyDiskComponent : Component
{
    /// <summary>
    /// The maximum storage of the disk, space is consumed by recipes on the <see cref="RecipeRelay.RecipeContainerComponent"/>.
    /// </summary>
    [DataField]
    public int MaxStorage = 15;

    /// <summary>
    /// A whitelist applied to targets of this tech disk when interacting.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist = null;

    /// <summary>
    /// A blacklist applied to targets of this tech disk when interacting.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist = null;

    [DataField]
    public TimeSpan InteractDuration = TimeSpan.Zero;
}