using Content.Shared._Persistence14.Storage;
using Content.Shared.DragDrop;
using Content.Shared.Storage.Components;

namespace Content.Server._Persistence14.Storage;

public sealed partial class DragableStorageSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnCanDragEvent(EntityUid uid, DragableStorageComponent comp, ref CanDragEvent args)
    {
        if (!comp.AllowAnchored && Transform(uid).Anchored)
            return;

        if (!comp.AllowOpen && TryComp<EntityStorageComponent>(uid, out var storageComp) && storageComp.Open)
            return;

        args.Handled = true;
    }
}