using System.Linq;
using Content.Server.Popups;
using Content.Shared._Persistence14.Storage;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DragDrop;
using Content.Shared.Storage.Components;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;

namespace Content.Server._Persistence14.Storage;

public sealed partial class StorageRackSystem : EntitySystem
{
    [Dependency] private ItemSlotsSystem _slots = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private TransformSystem _transform = default!;

    [SubscribeLocalEvent]
    private void OnDragDropTarget(EntityUid uid, StorageRackComponent component, ref DragDropTargetEvent args)
    {
        if (!CanStore((uid, component), args.Dragged, args.User, out var slot))
        {
            _popup.PopupEntity("Can't do that!", args.Dragged, Shared.Popups.PopupType.SmallCaution);
            return;
        }

        Store((uid, component), args.Dragged, args.User, slot);
    }

    private bool CanStore(Entity<StorageRackComponent?> rack, EntityUid target, EntityUid? user, out ItemSlot slot)
    {
        slot = default!;
        if (!Resolve(rack, ref rack.Comp))
            return false;

        if (rack.Comp.RequireAnchored && !Transform(rack.Owner).Anchored)
            return false; // Not anchored and must be.

        if (rack.Comp.RequireClosedContainers && TryComp<EntityStorageComponent>(target, out var storageComp) && storageComp.Open)
            return false; // Rack can only store closed containers.

        foreach (var slotId in rack.Comp.Slots)
        {
            if (!_slots.TryGetSlot(rack.Owner, slotId, out var currSlot))
                continue; // Should probably send some sort of error.

            if (!_slots.CanInsert(rack.Owner, currSlot, target, user))
                continue; // Can't insert into this slot for one reason or another.

            slot = currSlot;
            return true;
        }

        return false; // There are no viable slots.
    }

    private void Store(Entity<StorageRackComponent> rack, EntityUid target, EntityUid? user, ItemSlot slot)
    {
        if (!_slots.TryInsert(rack, slot, target, user))
        {
            // Something went very wrong...
            _popup.PopupEntity("Can't do that!", target, Shared.Popups.PopupType.SmallCaution);
            return;
        }
    }
}