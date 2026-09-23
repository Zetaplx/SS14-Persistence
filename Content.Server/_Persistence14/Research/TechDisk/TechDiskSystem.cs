using System.Linq;
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

    private void UpdateUserInterface(Entity<TechDiskTerminalComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        List<TechCardData> serverData = new();
        if (_relay.TryGetRecipeContainer(ent.Owner, out var serverContainer))
            serverData = ConvertToData(serverContainer.Comp.UnlockedRecipes).ToList();

        List<TechCardData> diskData = new();
        if (TryGetTechDisk(ent, out _, out _))
            diskData = ConvertToData(ent.Comp.QueuedTech).ToList();

        var state = new TechDiskTerminalBUIState
        {
            ServerData = serverData,
            DiskData = diskData
        };
        _ui.SetUiState(ent.Owner, TechDiskTerminalUIKey.Main, state);
    }

    private IEnumerable<TechCardData> ConvertToData(Dictionary<ProtoId<LatheRecipePrototype>, int> data)
    {
        foreach (var (techId, qty) in data)
        {
            var tech = ProtoMan.Index(techId);
            if (tech.Name is not { } name)
                continue;
            var card = new TechCardData
            {
                TechId = techId,
                TechName = Loc.GetString(name),
                Quantity = qty
            };
            yield return card;
        }
    }

    public sealed partial class TechDiskInteractEvent : SimpleDoAfterEvent;
}