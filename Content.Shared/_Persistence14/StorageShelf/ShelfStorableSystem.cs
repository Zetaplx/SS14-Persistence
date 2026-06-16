using Content.Shared.DragDrop;
using Robust.Shared.Utility;

namespace Content.Shared._Persistence14.StorageShelf;

public sealed partial class ShelfStorableSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ShelfStorableComponent, CanDragEvent>(OnCanDrag);
    }

    private void OnCanDrag(Entity<ShelfStorableComponent> item, ref CanDragEvent args)
    {
        if (!Transform(item).Anchored)
            args.Handled = true;
    }
}