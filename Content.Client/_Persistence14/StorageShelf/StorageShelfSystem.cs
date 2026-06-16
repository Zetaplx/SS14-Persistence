using Content.Shared._Persistence14.StorageShelf;
using Content.Shared.Verbs;
using Robust.Shared.Timing;

namespace Content.Client._Persistence14.StorageShelf;

public sealed partial class StorageShelfSystem : SharedStorageShelfSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StorageShelfComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeNetworkEvent<ShelfPopupMessage>(OnPopupMessage);
    }

    private void OnGetVerbs(Entity<StorageShelfComponent> shelf, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanComplexInteract)
            return;

        if (!Container.TryGetContainer(shelf.Owner, shelf.Comp.ContainerId, out var container))
            return;

        var verbs = new List<AlternativeVerb>();

        var user = args.User;

        foreach (var item in container.ContainedEntities)
        {
            var name = Name(item);

            verbs.Add(new AlternativeVerb
            {
                Text = $"Remove: {name}",
                Act = () => SendEjectEvent(new ShelfEjectEvent
                {
                    Shelf = GetNetEntity(shelf),
                    Item = GetNetEntity(item),
                    User = GetNetEntity(user)
                })
            });
        }

        foreach (var verb in verbs)
            args.Verbs.Add(verb);
    }

    private void OnPopupMessage(ShelfPopupMessage args)
    {
        var msg = args.Message;
        if (Loc.TryGetString(msg, out var locMsg))
            msg = locMsg;

        Popup.PopupEntity(msg, GetEntity(args.Shelf), GetEntity(args.User));
    }

    private void SendEjectEvent(ShelfEjectEvent args)
    {
        if (!Timer.IsFirstTimePredicted)
            return;

        RaiseNetworkEvent(args);
    }
}