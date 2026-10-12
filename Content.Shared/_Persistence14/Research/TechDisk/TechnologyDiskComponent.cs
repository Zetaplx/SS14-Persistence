namespace Content.Shared._Persistence14.Research.TechDisk;

[RegisterComponent]
public sealed partial class TechnologyDiskComponent : Component
{
    /// <summary>
    /// The maximum storage of the disk, space is consumed by recipes on the <see cref="RecipeRelay.RecipeContainerComponent"/>.
    /// </summary>
    [DataField]
    public int MaxStorage = 15;
}