using System.Linq;
using Content.Shared._Persistence14.Research.TechDisk;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Persistence14.Research.TechDisk;

public sealed partial class TechDiskSystem
{
    [SubscribeLocalEvent]
    private void OnOpenBUI(EntityUid uid, TechDiskTerminalComponent comp, ref BoundUIOpenedEvent args)
    {
        UpdateUserInterface((uid, comp));
    }

    [SubscribeLocalEvent]
    public void OnAddMessageReceived(TechDiskAddTechMessage args)
    {
        var terminal = GetEntity(args.Entity);
        if (!TryComp<TechDiskTerminalComponent>(terminal, out var comp))
            return;

        AddQueue((terminal, comp), args.TechId);
    }

    [SubscribeLocalEvent]
    public void OnRemoveMessageReceived(TechDiskRemoveTechMessage args)
    {
        var terminal = GetEntity(args.Entity);
        if (!TryComp<TechDiskTerminalComponent>(terminal, out var comp))
            return;

        RemoveQueue((terminal, comp), args.TechId);
    }

    private void UpdateUserInterface(Entity<TechDiskTerminalComponent> ent)
    {

        var currentPoints = 0;
        if (_research.TryGetClientServer(ent.Owner, out var server))
            currentPoints = server.Comp1.Points;

        Dictionary<ProtoId<LatheRecipePrototype>, TechCardData> serverData = new();
        if (_relay.TryGetRecipeContainer(server.Owner, out var serverContainer))
            serverData = ConvertToData(serverContainer.Comp.UnlockedRecipes);

        Dictionary<ProtoId<LatheRecipePrototype>, TechCardData> diskData = new();
        int maxDiskSize = 0;
        int currentDiskSize = 0;
        int currentPointCost = 0;
        bool hasDisk = false;
        if (TryGetTechDisk(ent.AsNullable(), out var disk, out _))
        {
            diskData = ConvertToData(ent.Comp.QueuedTech);
            maxDiskSize = disk.Comp.MaxStorage;
            var (research, size) = CalculateQueuedTechCost((ent.Owner, ent.Comp));
            currentDiskSize = size;
            currentPointCost = research;
            hasDisk = true;
        }

        var state = new TechDiskTerminalBUIState
        {
            ServerData = serverData,
            DiskData = diskData,
            MaxTechSize = maxDiskSize,
            CurrentTechSize = currentDiskSize,

            ResearchPoints = currentPoints,
            CurrentResearchPrice = currentPointCost,
            CanPrint = diskData.Any() && currentPoints >= currentPointCost,
            HasDisk = hasDisk,
        };
        _ui.SetUiState(ent.Owner, TechDiskTerminalUIKey.Main, state);
    }

    /// <summary>
    /// Converts a dictionary as produced by the recipe containers or the QueuedTech field of the terminal into a data structure to be provided to the UI.
    /// </summary>
    private Dictionary<ProtoId<LatheRecipePrototype>, TechCardData> ConvertToData(Dictionary<ProtoId<LatheRecipePrototype>, int> data)
    {
        Dictionary<ProtoId<LatheRecipePrototype>, TechCardData> result = new();
        foreach (var (techId, qty) in data)
        {
            var tech = ProtoMan.Index(techId);
            if (tech.Name is not { } name)
                name = ProtoMan.Index(tech.Result!.Value).Name;
            var card = new TechCardData
            {
                TechId = techId,
                TechName = Loc.GetString(name),
                Quantity = qty,
                TechSize = tech.StorageCost,
                Categories = tech.Categories
            };

            result.Add(techId, card);
        }
        return result;
    }
}