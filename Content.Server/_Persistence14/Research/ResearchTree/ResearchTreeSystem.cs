using Content.Shared._Persistence14.PersistentIdentifier;
using Content.Shared._Persistence14.Research.RecipeRelay;
using Content.Shared._Persistence14.Research.ResearchTree;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Persistence14.Research.ResearchTree;

public sealed partial class ResearchTreeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly PersistentIdentifierSystem _pid = default!;
    [Dependency] private readonly SharedRecipeRelaySystem _recipeRelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeUI();
    }

    /// <inheritdoc/>
    public override void Update(float frameTime)
    {
        var sourceQuery = EntityQueryEnumerator<ResearchTreeSourceComponent>();

        while (sourceQuery.MoveNext(out var uid, out var sourceComponent))
        {
            UpdateSource((uid, sourceComponent));
        }
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

    public bool TryStartTechnologyUnlock(Entity<ResearchTreeSourceComponent?> source, ResearchNode node)
    {
        if (!Resolve(source, ref source.Comp) ||
            source.Comp.ResearchUnlockTimes.ContainsKey(node.Technology) ||
            source.Comp.ResearchUnlockTimes.Count >= source.Comp.MaxResearch ||
            !VerifyPrerequisites(source, node) ||
            !_prototypeManager.TryIndex(node.Technology, out var tech) ||
            source.Comp.ResearchPoints < tech.Cost)
            return false;

        if (node.UnlockTime <= TimeSpan.Zero)
        {
            if (TryUnlockTechnology(source, node))
            {
                source.Comp.ResearchPoints -= tech.Cost;
                UpdateUserInterfaceState(source);
                Dirty(source);
                return true;
            }

            return false;
        }

        source.Comp.ResearchUnlockTimes.Add(node.Technology, _time.CurTime + node.UnlockTime);
        source.Comp.ResearchPoints -= tech.Cost;
        UpdateUserInterfaceState(source);
        Dirty(source);
        return true;
    }

    /// <summary>
    /// Unlocks a recipe on the source, can optionally forcefully bypass unlock requirements.
    /// </summary>
    public bool TryUnlockTechnology(Entity<ResearchTreeSourceComponent?> source, ProtoId<TechnologyPrototype> technologyId, bool force = false)
    {
        if (!TryGetNode(source, technologyId, out var node))
            return false;

        return TryUnlockTechnology(source, node, force);
    }
    /// <summary>
    /// Unlocks a recipe on the source, can optionally forcefully bypass unlock requirements.
    /// </summary>
    public bool TryUnlockTechnology(Entity<ResearchTreeSourceComponent?> source, ResearchNode node, bool force = false)
    {
        if (!Resolve(source, ref source.Comp) ||
            !_prototypeManager.TryIndex(node.Technology, out var tech))
            return false;

        // Bypassed by force
        if (!force)
        {
            if (!VerifyPrerequisites(source, node))
                return false;
        }

        source.Comp.UnlockedTechnologies.Add(node.Technology);
        Dirty(source);
        return true;
    }

    /// <summary>
    /// Determines whether the prerequisites of a given node are met.
    /// </summary>
    public bool VerifyPrerequisites(Entity<ResearchTreeSourceComponent?> source, ProtoId<TechnologyPrototype> technologyId)
    {
        if (!TryGetNode(source, technologyId, out var node))
            return false;
        return VerifyPrerequisites(source, node);
    }
    /// <summary>
    /// Determines whether the prerequisites of a given node are met.
    /// </summary>
    public bool VerifyPrerequisites(Entity<ResearchTreeSourceComponent?> source, ResearchNode node)
    {
        if (!Resolve(source, ref source.Comp) ||
            !_prototypeManager.TryIndex(node.Technology, out var tech))
            return false;

        foreach (var prereq in node.Prerequisites)
            if (!IsTechnologyUnlocked(source, prereq))
                return false;
        return true;
    }

    public void UnlockTechnologyRecipes(EntityUid uid, ProtoId<TechnologyPrototype> technologyId)
    {
        if (!_prototypeManager.TryIndex(technologyId, out var tech))
            return;

        foreach (var (recipe, qty) in tech.RecipeUnlocks)
        {
            _recipeRelay.TryAddUnlockRecipe(uid, recipe, qty);
        }
    }

    /// <summary>
    /// Determines if a given technology is unlocked on the source.
    /// </summary>
    public bool IsTechnologyUnlocked(Entity<ResearchTreeSourceComponent?> source, ProtoId<TechnologyPrototype> technologyId)
    {
        if (!Resolve(source, ref source.Comp))
            return false;

        return source.Comp.UnlockedTechnologies.Contains(technologyId);
    }

    private bool TryGetNode(Entity<ResearchTreeSourceComponent?> source, ProtoId<TechnologyPrototype> technologyId, out ResearchNode node)
    {
        node = default!;
        if (!Resolve(source, ref source.Comp) ||
            !_prototypeManager.TryIndex(source.Comp.Tree, out var treeProto) ||
            !treeProto.GetNodes().TryGetValue(technologyId, out var n))
            return false;

        node = n;
        return true;
    }

    private void UpdateSource(Entity<ResearchTreeSourceComponent> source)
    {
        List<ProtoId<TechnologyPrototype>> toRemove = new();
        foreach (var (tech, time) in source.Comp.ResearchUnlockTimes)
        {
            if (_time.CurTime >= time)
            {
                TryUnlockTechnology(source.AsNullable(), tech);
                toRemove.Add(tech);
            }
        }

        if (toRemove.Count <= 0)
            return;

        foreach (var tech in toRemove)
            source.Comp.ResearchUnlockTimes.Remove(tech);
        Dirty(source);
    }
}