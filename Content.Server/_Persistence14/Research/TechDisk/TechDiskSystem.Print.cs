using System.Linq;
using Content.Shared._Persistence14.PersistentIdentifier;
using Content.Shared._Persistence14.Research.TechDisk;
using Robust.Shared.Timing;

namespace Content.Server._Persistence14.Research.TechDisk;

public sealed partial class TechDiskSystem
{
    [Dependency] private IGameTiming _time = default!;

    /// <summary>
    /// Initializes the print timer and extracts points from the research server.
    /// </summary>
    private void StartPrint(Entity<TechDiskTerminalComponent> terminal)
    {
        if (!_research.TryGetClientServer(terminal.Owner, out var server) || // No server to get points from
            !TryGetTechDisk(terminal.AsNullable(), out var disk, out _) || // No disk to store tech
            !terminal.Comp.QueuedTech.Any() || // No tech to store
            HasComp<TechDiskTerminalPrintingComponent>(terminal.Owner)) // Already printing
            return;

        var (cost, _) = CalculateQueuedTechCost(terminal);
        if (server.Comp1.Points < cost)
            return;
        _research.ModifyServerPoints(server, -cost, server.Comp1);

        var printing = AddComp<TechDiskTerminalPrintingComponent>(terminal.Owner);
        printing.PrintEndTime = _time.CurTime + terminal.Comp.PrintDuration;
        printing.PrintStartTime = _time.CurTime;

        UpdateUserInterface(terminal);
    }

    /// <summary>
    /// Transfers prints to the Tech Disk and ejects the disk.
    /// </summary>
    private void CompletePrint(Entity<TechDiskTerminalComponent> terminal)
    {
        if (!_research.TryGetClientServer(terminal.Owner, out var server) ||
            !TryGetTechDisk(terminal.AsNullable(), out var disk, out _) ||
            !_slots.TryGetSlot(terminal.Owner, terminal.Comp.DiskSlot, out var slot))
            return;

        RemComp<TechDiskTerminalPrintingComponent>(terminal.Owner);

        foreach (var (tech, qty) in terminal.Comp.QueuedTech)
        {
            _relay.TryAddUnlockRecipe(disk, tech, qty);
        }
        terminal.Comp.QueuedTech.Clear();

        _slots.TryEject(terminal.Owner, slot, null, out _);

        UpdateUserInterface(terminal);
    }

    /// <summary>
    /// Checks in on all actively printing terminal timers and completes any complete timers.
    /// </summary>
    private void UpdatePrint(float frameTime)
    {
        var printingTerminals = EntityQueryEnumerator<TechDiskTerminalPrintingComponent, TechDiskTerminalComponent>();

        while (printingTerminals.MoveNext(out var uid, out var printing, out var terminalComp))
        {
            if (_time.CurTime < printing.PrintEndTime)
                continue;

            CompletePrint((uid, terminalComp));
        }
    }
}