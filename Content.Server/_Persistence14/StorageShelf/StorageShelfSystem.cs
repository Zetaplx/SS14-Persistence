using Content.Shared._Persistence14.StorageShelf;
using Content.Shared.DragDrop;


namespace Content.Server._Persistence14.StorageShelf;

public sealed partial class StorageShelfSystem : SharedStorageShelfSystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<ShelfEjectEvent>(OnEjectEvent);
        SubscribeLocalEvent<StorageShelfComponent, DragDropTargetEvent>(OnDragDrop);
    }

    private void OnEjectEvent(ShelfEjectEvent args)
    {
        var shelf = GetEntity(args.Shelf);
        if (!TryComp<StorageShelfComponent>(shelf, out var shelfComp))
            return;
        var item = GetEntity(args.Item);
        var user = GetEntity(args.User);

        Eject((shelf, shelfComp), item, user);
    }

    private void Eject(Entity<StorageShelfComponent> shelf, EntityUid item, EntityUid user)
    {
        var container = Container.GetContainer(shelf.Owner, shelf.Comp.ContainerId);

        if (!container.Contains(item))
            return;

        Container.Remove(item, container);

        var coords = Transform(shelf.Owner).Coordinates;
        _xform.SetCoordinates(item, coords);

        Popup.PopupEntity("You removed it from the shelf!", shelf.Owner, user);
    }

    private void OnDragDrop(Entity<StorageShelfComponent> shelf, ref DragDropTargetEvent args)
    {
        if (args.Handled)
            return;
        args.Handled = true;

        var dragged = args.Dragged;

        if (!TryComp<ShelfStorableComponent>(dragged, out var storable))
            return; // Not a valid storable entity

        Store(shelf, (dragged, storable), args.User);
    }

    private void Store(Entity<StorageShelfComponent> shelf, Entity<ShelfStorableComponent> item, EntityUid user)
    {
        var container = Container.GetContainer(shelf.Owner, shelf.Comp.ContainerId);

        if (!Container.Insert(item.Owner, container))
            return;
        RaisePopupEvent(shelf, user, "You placed it on the shelf");
    }
}