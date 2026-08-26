using System.Linq;
using System.Numerics;
using Content.Shared._Persistence14.PersistentIdentifier.Reference;
using Content.Shared._Persistence14.Research.ResearchTree;
using Robust.Server.GameObjects;

namespace Content.Server._Persistence14.Research.ResearchTree;

public sealed partial class ResearchTreeSystem
{
    [Dependency] private readonly TransformSystem _transform = default!;

    private void InitializeLink()
    {
        SubscribeLocalEvent<ResearchTreeClientComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(Entity<ResearchTreeClientComponent> client, ref ComponentStartup args)
    {
        ClientLinkNearest(client.AsNullable());
    }

    private void ClientLinkNearest(Entity<ResearchTreeClientComponent?> client)
    {
        if (!Resolve(client, ref client.Comp) ||
            client.Comp.SourceId != PersistentEntityReference.EmptyId)
            return;

        var validSources = GetAllValidSources(client);
        if (validSources.Count() <= 0)
            return;

        var nearestSquareDist = -1f;
        var nearest = validSources.First();

        foreach (var source in validSources)
        {
            var squareDist = Vector2.DistanceSquared(_transform.GetWorldPosition(source), _transform.GetWorldPosition(client));
            if (nearestSquareDist < 0 || squareDist < nearestSquareDist)
            {
                nearest = source;
                nearestSquareDist = squareDist;
            }
        }

        TryLink(nearest.AsNullable(), client);
    }


    /// <summary>
    /// Attempt to retrieve the source for a particular client.
    /// </summary>
    public bool TryGetClientSource(Entity<ResearchTreeClientComponent?> client, out Entity<ResearchTreeSourceComponent> source)
    {
        source = default!;
        if (!Resolve(client, ref client.Comp) ||
            !_pid.TryResolveId(client.Comp.SourceId, out var sourceIdEnt) ||
            !TryComp<ResearchTreeSourceComponent>(sourceIdEnt, out var sourceComp))
            return false;

        source = (sourceIdEnt.Owner, sourceComp);
        return true;
    }

    /// <summary>
    /// Gets all active clients of the provided research tree source.
    /// </summary>
    public IEnumerable<Entity<ResearchTreeClientComponent>> GetSourceClients(Entity<ResearchTreeSourceComponent?> source)
    {
        if (!Resolve(source, ref source.Comp))
            yield break;

        foreach (var clientPid in source.Comp.Clients)
        {
            if (!_pid.TryResolveId(clientPid, out var clientIdEnt) ||
                !TryComp<ResearchTreeClientComponent>(clientIdEnt, out var clientComp))
                continue;

            yield return (clientIdEnt.Owner, clientComp);
        }
    }

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