using System.Linq;
using Content.Shared._Persistence14.PersistentIdentifier.Reference;
using Content.Shared._Persistence14.Research.ResearchTree;

namespace Content.Server._Persistence14.Research.ResearchTree;

public sealed partial class ResearchTreeSystem
{
    public bool TryLink(Entity<ResearchTreeSourceComponent?> source, Entity<ResearchTreeClientComponent?> client)
    {
        if (!Resolve(source, ref source.Comp) || !Resolve(client, ref client.Comp))
            return false;

        var sourceId = _pid.EnsureId(source.Owner);
        var clientId = _pid.EnsureId(client.Owner);

        if (source.Comp.Clients.Contains(clientId))
        {
            if (client.Comp.SourceId != sourceId)
            {
                client.Comp.SourceId = sourceId;
                Dirty(client);
                return true;
            }
            return false;
        }

        if (client.Comp.SourceId == sourceId)
        {
            if (!source.Comp.Clients.Contains(clientId))
            {
                source.Comp.Clients.Add(clientId);
                Dirty(source);
                return true;
            }
            return false;
        }

        source.Comp.Clients.Add(clientId);
        client.Comp.SourceId = sourceId;
        Dirty(source);
        Dirty(client);
        UpdateClientUserInterfaceState(client);
        return true;
    }

    /// <summary>
    /// Attempts to unlinkg a particular source and client.
    /// </summary>
    public bool TryUnlink(Entity<ResearchTreeSourceComponent?> source, Entity<ResearchTreeClientComponent?> client)
    {
        if (!Resolve(client, ref client.Comp) || !Resolve(source, ref source.Comp) ||
            !_pid.CompareId(client.Comp.SourceId, source))
            return false;

        return TryUnlink(client);
    }

    /// <summary>
    /// Attempts to unlink a client from its source, if it has one. Updates the UI when disconnected.
    /// </summary>
    public bool TryUnlink(Entity<ResearchTreeClientComponent?> client)
    {
        if (!Resolve(client, ref client.Comp) ||
            !_pid.TryResolveId(client.Comp.SourceId, out var sourceIdEnt))
            return false;

        client.Comp.SourceId = PersistentEntityReference.EmptyId;
        Dirty(client);
        ClearClientUserInterfaceState(client);

        if (!TryComp<ResearchTreeSourceComponent>(sourceIdEnt, out var sourceComponent) ||
            !sourceComponent.Clients.Remove(_pid.EnsureId(client)))
            return false;
        Dirty(sourceIdEnt.Owner, sourceComponent);
        return true;
    }

    /// <summary>
    /// Unlinks all clients connected to a source. Updates UI on all disconnected clients.
    /// </summary>
    public void UnlinkAll(Entity<ResearchTreeSourceComponent?> source)
    {
        if (!Resolve(source, ref source.Comp))
            return;

        foreach (var client in source.Comp.Clients.ToArray())
        {
            if (_pid.TryResolveId(client, out var clientIdEnt) &&
                TryComp<ResearchTreeClientComponent>(clientIdEnt.Owner, out var clientComponent))
            {
                clientComponent.SourceId = PersistentEntityReference.EmptyId;
                Dirty(clientIdEnt.Owner, clientComponent);
                ClearClientUserInterfaceState(clientIdEnt.Owner);
            }

            source.Comp.Clients.Remove(client);
        }

        Dirty(source);
    }
}