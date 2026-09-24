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
    public required List<ProtoId<LatheCategoryPrototype>> Categories;
}

[NetSerializable, Serializable]
public enum TechDiskTerminalUIKey
{
    Main
}

[NetSerializable, Serializable]
public sealed partial class TechDiskAddTechMessage : BoundUserInterfaceMessage
{
    public ProtoId<LatheRecipePrototype> TechId;

    public TechDiskAddTechMessage(ProtoId<LatheRecipePrototype> techId) => TechId = techId;
}

[NetSerializable, Serializable]
public sealed partial class TechDiskRemoveTechMessage : BoundUserInterfaceMessage
{
    public ProtoId<LatheRecipePrototype> TechId;

    public TechDiskRemoveTechMessage(ProtoId<LatheRecipePrototype> techId) => TechId = techId;
}