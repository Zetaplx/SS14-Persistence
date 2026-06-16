using Content.Shared.DragDrop;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._Persistence14.StorageShelf;

public abstract partial class SharedStorageShelfSystem : EntitySystem
{
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected IGameTiming Timer = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StorageShelfComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<StorageShelfComponent, CanDropTargetEvent>(OnCanDropTarget);
    }

    private void OnComponentInit(Entity<StorageShelfComponent> shelf, ref ComponentInit args)
    {
        Container.EnsureContainer<Container>(shelf.Owner, shelf.Comp.ContainerId);
    }

    private void OnCanDropTarget(Entity<StorageShelfComponent> shelf, ref CanDropTargetEvent args)
    {
        if (args.Handled)
            return;
        args.Handled = true;

        var dragged = args.Dragged;

        if (!TryComp<ShelfStorableComponent>(dragged, out var storable))
            return; // Not a valid storable entity

        if (!CanStore(shelf, (dragged, storable), args.User))
            return; // Unable to store

        args.CanDrop = true;
    }

    private bool CanStore(Entity<StorageShelfComponent> shelf, Entity<ShelfStorableComponent> item, EntityUid user)
    {
        var container = Container.GetContainer(shelf.Owner, shelf.Comp.ContainerId);

        if (container.ContainedEntities.Count >= shelf.Comp.Capacity)
        {
            RaisePopupEvent(shelf, user, "The shelf is full!");
            return false;
        }

        if (Transform(item).Anchored)
        {
            RaisePopupEvent(shelf, user, "The item must be unanchored first!");
            return false;
        }

        if (TryComp<OpenableComponent>(shelf, out var openable) && openable.Opened)
        {
            RaisePopupEvent(shelf, user, "The item cannot be open!");
            return false;
        }

        return true;
    }

    protected void RaisePopupEvent(Entity<StorageShelfComponent> shelf, EntityUid user, string message)
    {
        RaiseNetworkEvent(new ShelfPopupMessage
        {
            User = GetNetEntity(user),
            Shelf = GetNetEntity(shelf),
            Message = message,
        });
    }
}

[NetSerializable, Serializable]
public sealed partial class ShelfEjectEvent : EntityEventArgs
{
    public NetEntity Shelf;
    public NetEntity Item;
    public NetEntity User;
}

[NetSerializable, Serializable]
public sealed partial class ShelfPopupMessage : EntityEventArgs
{
    public NetEntity Shelf;
    public NetEntity User;
    public string Message = "";
}