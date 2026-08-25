using System.Linq;
using Content.Shared._Persistence14.Research.ResearchTree;

namespace Content.Server._Persistence14.Research.ResearchTree;

public sealed partial class ResearchTreeSystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    private void InitializeUI()
    {

    }

    private void UpdateUserInterfaceState(Entity<ResearchTreeSourceComponent?> source)
    {
        if (!Resolve(source, ref source.Comp) ||
            !_prototypeManager.TryIndex(source.Comp.Tree, out var treePrototype))
            return;

        var state = new ResearchTreeClientBoundUserInterfaceState
        {
            Nodes = treePrototype.GetNodes(),
            UnlockedTechnologies = source.Comp.UnlockedTechnologies,
            RecipeUnlockTimers = source.Comp.ResearchUnlockTimes.ToDictionary(pick => pick.Key, pick => (TimeSpan)pick.Value),
            MaxResearch = source.Comp.MaxResearch,
            Points = source.Comp.ResearchPoints,

            Connected = true
        };

        foreach (var client in GetSourceClients(source))
        {
            _ui.SetUiState(client.Owner, ResearchTreeClientUiStateKey.Tree, state);
        }
    }

    private void UpdateClientUserInterfaceState(Entity<ResearchTreeClientComponent?> client)
    {
        if (!Resolve(client, ref client.Comp) ||
            !_pid.TryResolveId(client.Comp.SourceId, out var sourceIdEnt) ||
            !TryComp<ResearchTreeSourceComponent>(sourceIdEnt, out var sourceComponent) ||
            !_prototypeManager.TryIndex(sourceComponent.Tree, out var treePrototype))
            return;

        var state = new ResearchTreeClientBoundUserInterfaceState
        {
            Nodes = treePrototype.GetNodes(),
            UnlockedTechnologies = sourceComponent.UnlockedTechnologies,
            RecipeUnlockTimers = sourceComponent.ResearchUnlockTimes.ToDictionary(pick => pick.Key, pick => (TimeSpan)pick.Value),
            MaxResearch = sourceComponent.MaxResearch,
            Points = sourceComponent.ResearchPoints,

            Connected = true
        };

        _ui.SetUiState(client.Owner, ResearchTreeClientUiStateKey.Tree, state);
    }

    private void ClearClientUserInterfaceState(Entity<ResearchTreeClientComponent?> client)
    {
        if (!Resolve(client, ref client.Comp))
            return;

        var state = new ResearchTreeClientBoundUserInterfaceState
        {
            Nodes = new(),
            UnlockedTechnologies = new(),
            RecipeUnlockTimers = new(),
            MaxResearch = 0,
            Points = 0,

            Connected = false
        };
    }
}