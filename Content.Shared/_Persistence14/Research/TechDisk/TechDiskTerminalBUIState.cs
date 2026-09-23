using Content.Shared.Lathe.Prototypes;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Persistence14.Research.TechDisk;

[NetSerializable, Serializable]
public sealed partial class TechDiskTerminalBUIState : BoundUserInterfaceState
{
    public required Dictionary<ProtoId<LatheRecipePrototype>, TechCardData> ServerData;
    public required Dictionary<ProtoId<LatheRecipePrototype>, TechCardData> DiskData;
    public required int MaxTechSize;
    public required int CurrentTechSize;
    public required int ResearchPoints;
    public required int CurrentResearchPrice;
    public required bool CanPrint;
}

[NetSerializable, Serializable]
public sealed partial class TechCardData
{
    public required ProtoId<LatheRecipePrototype> TechId;
    public required string TechName;
    public required int Quantity;
    public required int TechSize;
    public required ProtoId<LatheCategoryPrototype> Category;
}

[NetSerializable, Serializable]
public enum TechDiskTerminalUIKey
{
    Main
}

[NetSerializable, Serializable]
public sealed partial class TechDiskAddTechMessage(ProtoId<LatheRecipePrototype> TechId) : BoundUserInterfaceMessage;

[NetSerializable, Serializable]
public sealed partial class TechDiskRemoveTechMessage(ProtoId<LatheRecipePrototype> TechId) : BoundUserInterfaceMessage;