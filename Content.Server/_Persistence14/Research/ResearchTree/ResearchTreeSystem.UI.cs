using System.Linq;
using Content.Shared._Persistence14.Research.ResearchTree;

namespace Content.Server._Persistence14.Research.ResearchTree;

public sealed partial class ResearchTreeSystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    private void InitializeUI()
    {
        SubscribeLocalEvent<ResearchTreeClientComponent, BoundUIOpenedEvent>(OnBuiOpen);

        SubscribeLocalEvent<ResearchTreeClientConnectMessage>(OnClientConnectedMessageReceived);
        SubscribeLocalEvent<ResearchTreeClientDisconnectMessage>(OnClientDisconnectMessageReceived);
        SubscribeLocalEvent<ResearchTreeSourceClearClientsMessage>(OnSourceClearClientsMessageReceived);

        SubscribeLocalEvent<ResearchTreeStartResearchMessage>(OnStartResearchMessageReceived);
        SubscribeLocalEvent<ResearchTreeCancelResearchMessage>(OnCancelResearchMessageReceived);
    }

    #region UI Message Links
    private void OnBuiOpen(Entity<ResearchTreeClientComponent> client, ref BoundUIOpenedEvent args)
    {
        UpdateClientUserInterfaceState(client.AsNullable());
    }

    private void OnClientConnectedMessageReceived(ref ResearchTreeClientConnectMessage args)
    {
        var client = GetEntity(args.Client);
        var source = GetEntity(args.Source);

        TryLink(source, client);
    }

    private void OnClientDisconnectMessageReceived(ref ResearchTreeClientDisconnectMessage args)
    {
        var client = GetEntity(args.Client);
        var source = GetEntity(args.Source);

        TryUnlink(source, client);
    }

    private void OnSourceClearClientsMessageReceived(ref ResearchTreeSourceClearClientsMessage args)
    {
        var source = GetEntity(args.Source);

        UnlinkAll(source);
    }

    private void OnStartResearchMessageReceived(ref ResearchTreeStartResearchMessage args)
    {
        var client = GetEntity(args.Client);
        if (!TryGetClientSource(client, out var source))
            return;

        TryStartTechnologyUnlock(source.AsNullable(), args.TechnologyId);
    }

    private void OnCancelResearchMessageReceived(ref ResearchTreeCancelResearchMessage args)
    {
        var client = GetEntity(args.Client);
        if (!TryGetClientSource(client, out var source))
            return;

        TryCancelTechnologyUnlock(source.AsNullable(), args.TechnologyId);
    }
    #endregion

    /// <summary>
    /// Updates the UI state for all clients connected to a source.
    /// </summary>
    private void UpdateUserInterfaceState(Entity<ResearchTreeSourceComponent?> source)
    {
        if (!Resolve(source, ref source.Comp) ||
            !_prototypeManager.TryIndex(source.Comp.Tree, out var treePrototype))
            return;

        var nodes = treePrototype.GetNodes();
        var recipeUnlocks = source.Comp.ResearchUnlockTimes.ToDictionary(pick => pick.Key, pick => (TimeSpan)pick.Value);



        foreach (var client in GetSourceClients(source))
        {
            var validSources = GetAllValidSources(client.AsNullable()).Select(x => new ResearchTreeSourceSpecifier
            {
                SourceNetId = GetNetEntity(x.Owner),
                SourceName = Name(x.Owner),
                AlreadyConnected = x.Comp.Clients.Contains(_pid.EnsureId(client))
            }).ToList();

            var state = new ResearchTreeClientBoundUserInterfaceState
            {
                Nodes = nodes,
                UnlockedTechnologies = source.Comp.UnlockedTechnologies,
                RecipeUnlockTimers = recipeUnlocks,
                MaxResearch = source.Comp.MaxResearch,
                Points = source.Comp.ResearchPoints,

                Connected = true,
                ValidSources = validSources
            };

            _ui.SetUiState(client.Owner, ResearchTreeClientUiKey.Tree, state);
        }
    }

    /// <summary>
    /// Updates the UI state for a specific client.
    /// </summary>
    private void UpdateClientUserInterfaceState(Entity<ResearchTreeClientComponent?> client)
    {
        if (!Resolve(client, ref client.Comp) ||
            !_pid.TryResolveId(client.Comp.SourceId, out var sourceIdEnt) ||
            !TryComp<ResearchTreeSourceComponent>(sourceIdEnt, out var sourceComponent) ||
            !_prototypeManager.TryIndex(sourceComponent.Tree, out var treePrototype))
            return;

        var validSources = GetAllValidSources(client).Select(x => new ResearchTreeSourceSpecifier
        {
            SourceNetId = GetNetEntity(x.Owner),
            SourceName = Name(x.Owner),
            AlreadyConnected = x.Comp.Clients.Contains(_pid.EnsureId(client))
        }).ToList();

        var state = new ResearchTreeClientBoundUserInterfaceState
        {
            Nodes = treePrototype.GetNodes(),
            UnlockedTechnologies = sourceComponent.UnlockedTechnologies,
            RecipeUnlockTimers = sourceComponent.ResearchUnlockTimes.ToDictionary(pick => pick.Key, pick => (TimeSpan)pick.Value),
            MaxResearch = sourceComponent.MaxResearch,
            Points = sourceComponent.ResearchPoints,

            Connected = true,
            ValidSources = validSources
        };

        _ui.SetUiState(client.Owner, ResearchTreeClientUiKey.Tree, state);
    }

    /// <summary>
    /// Updates the UI state for a specific client to the disconnected state.
    /// </summary>
    private void ClearClientUserInterfaceState(Entity<ResearchTreeClientComponent?> client)
    {
        if (!Resolve(client, ref client.Comp))
            return;

        var state = ResearchTreeClientBoundUserInterfaceState.Disconnected;

        _ui.SetUiState(client.Owner, ResearchTreeClientUiKey.Tree, state);
    }
}