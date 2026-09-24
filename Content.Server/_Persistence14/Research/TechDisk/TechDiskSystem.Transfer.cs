using Content.Shared._Persistence14.Research.TechDisk;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Persistence14.Research.TechDisk;

public sealed partial class TechDiskSystem
{
    private void AddQueue(Entity<TechDiskTerminalComponent> terminal, ProtoId<LatheRecipePrototype> techId)
    {
        // Verify terminal/server status
        if (!_relay.TryGetRecipeContainer(terminal.Owner, out var container) ||
            !container.Comp.UnlockedRecipes.TryGetValue(techId, out var techQty) ||
            techQty <= 0) // This *shouldn't* happen, but just in case...
            return;

        var tech = ProtoMan.Index(techId);

        // Verify disk status
        if (!TryGetTechDisk(terminal.AsNullable(), out var disk, out _) ||
            CalculateQueuedTechCost(terminal).size + tech.StorageCost > disk.Comp.MaxStorage)
            return;

        // Attempt removal
        if (!_relay.TryRemoveUnlockRecipe(terminal.Owner, techId, count: 1))
            return;

        if (!terminal.Comp.QueuedTech.TryAdd(techId, 1))
            terminal.Comp.QueuedTech[techId] += 1;

        UpdateUserInterface(terminal);
    }

    private void RemoveQueue(Entity<TechDiskTerminalComponent> terminal, ProtoId<LatheRecipePrototype> techId)
    {
        // Verify terminal/server status
        if (!_relay.TryGetRecipeContainer(terminal.Owner, out var container))
            return;

        var tech = ProtoMan.Index(techId);

        // Verify queue, should this care about the disk...?
        if (!terminal.Comp.QueuedTech.TryGetValue(techId, out var qty) ||
            qty <= 0)
            return;

        if (qty - 1 <= 0)
            terminal.Comp.QueuedTech.Remove(techId);
        else
            terminal.Comp.QueuedTech[techId] = qty - 1;

        _relay.TryAddUnlockRecipe(terminal.Owner, techId, count: 1);

        UpdateUserInterface(terminal);
    }

    private void ClearQueue(Entity<TechDiskTerminalComponent> terminal)
    {
        if (!_relay.TryGetRecipeContainer(terminal.Owner, out var container))
            return;

        foreach (var (techId, qty) in terminal.Comp.QueuedTech)
        {
            _relay.TryAddUnlockRecipe(terminal.Owner, techId, count: qty);
        }
        terminal.Comp.QueuedTech.Clear();

        UpdateUserInterface(terminal);
    }
}