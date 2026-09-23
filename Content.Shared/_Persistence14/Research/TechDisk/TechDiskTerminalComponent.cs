using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Research.TechDisk;

[RegisterComponent]
public sealed partial class TechDiskTerminalComponent : Component
{
    /// <summary>
    /// The item slot which can hold the tech disk.
    /// </summary>
    [DataField]
    public string DiskSlot = "disk";

    /// <summary>
    /// Technologies queued onto the tech disk in the terminal. Used to save the current state while in active use on the UI and to calculate the cost of printing.
    /// </summary>
    [ViewVariables]
    public Dictionary<ProtoId<LatheRecipePrototype>, int> QueuedTech = new();

    /// <summary>
    /// The cost per single size unit of the technology in research points.
    /// </summary>
    [DataField("cost")]
    public int ResearchCostPerSize = 100;

    /// <summary>
    /// The length of time the terminal will take to print the tech disk.
    /// </summary>
    [DataField]
    public TimeSpan PrintDuration = TimeSpan.Zero;
}