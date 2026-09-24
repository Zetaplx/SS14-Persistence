using System.Linq;
using Content.Server.Research.Systems;
using Content.Shared._Persistence14.Research.RecipeRelay;
using Content.Shared._Persistence14.Research.TechDisk;
using Content.Shared.Access.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Research.Prototypes;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Server._Persistence14.Research.TechDisk;

public sealed partial class TechDiskSystem : EntitySystem
{
    [Dependency] private SharedRecipeRelaySystem _relay = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private AccessReaderSystem _access = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private ItemSlotsSystem _slots = default!;
    [Dependency] private ResearchSystem _research = default!;

    [SubscribeLocalEvent]
    private void OnInteractWith(EntityUid uid, TechnologyDiskComponent diskComponent, ref AfterInteractEvent args)
    {
        // Verify Item
        if (!TryComp<RecipeContainerComponent>(uid, out var localContainer) ||
            localContainer.UnlockedRecipes.Count == 0) // Tech disks *really* shouldn't have permanent recipes, so unlocked check only.
            return;

        // Verify Target
        if (args.Target is not { } target ||
            _whitelist.IsWhitelistFail(diskComponent.Whitelist, target) ||
            _whitelist.IsWhitelistPass(diskComponent.Blacklist, target) ||
            !_relay.TryGetRecipeContainer(target, out var targetContainer))
            return;

        // Verify User
        if (!_access.IsAllowed(args.Used, target))
            return; // TODO: No Access Popup/Sound

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, diskComponent.InteractDuration, new TechDiskInteractEvent(), uid, target: target, used: uid)
        {
            NeedHand = true,
            BreakOnMove = true,
            BreakOnDamage = true,
        });
    }

    [SubscribeLocalEvent]
    private void OnDoAfterComplete(EntityUid uid, TechnologyDiskComponent diskComponent, ref TechDiskInteractEvent args)
    {
        // Aquire disk container
        if (!TryComp<RecipeContainerComponent>(uid, out var localContainer))
            return;

        // Aquire Target Container
        if (args.Target is not { } target || !_relay.TryGetRecipeContainer(target, out var targetContainer))
            return;

        _relay.CopyTo((uid, localContainer), targetContainer, ignorePermanent: true);
        QueueDel(uid);
    }

    /// <summary>
    /// Attemps to retrieve a tech disk stored in the terminal's item slot.
    /// </summary>
    private bool TryGetTechDisk(Entity<TechDiskTerminalComponent?> ent, out Entity<TechnologyDiskComponent> disk, out RecipeContainerComponent container)
    {
        disk = default!;
        container = default!;
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (!_slots.TryGetSlot(ent.Owner, ent.Comp.DiskSlot, out var slot) ||
            slot.Item is not { } item ||
            !TryComp<TechnologyDiskComponent>(item, out var diskComp) ||
            !TryComp<RecipeContainerComponent>(item, out var containerComp))
            return false;

        disk = (item, diskComp);
        container = containerComp;
        return true;
    }

    /// <summary>
    /// Calculates the research and tech storage costs of the QueuedTech on the terminal.
    /// </summary>
    private (int research, int size) CalculateQueuedTechCost(Entity<TechDiskTerminalComponent> ent)
    {
        var research = 0;
        var size = 0;

        foreach (var (techId, qty) in ent.Comp.QueuedTech)
        {
            var tech = ProtoMan.Index(techId);
            var techSize = tech.StorageCost * qty;
            size += techSize;
            research += ent.Comp.ResearchCostPerSize * techSize;
        }

        return (research, size);
    }

    public sealed partial class TechDiskInteractEvent : SimpleDoAfterEvent;
}