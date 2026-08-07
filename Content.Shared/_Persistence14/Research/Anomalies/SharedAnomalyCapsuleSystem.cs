using System.Linq;
using Content.Shared._Persistence14.RandomTable;
using Content.Shared._Persistence14.RandomTable.State;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Research.Anomalies;

public sealed partial class SharedAnomalyCapsuleSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly RandomTableSystem _randomTable = default!;

    public const string Sawmill = "anomaly-capsule";

    public override void Initialize()
    {
        SubscribeModuleRelayEvent<AfterInteractEvent>();
    }

    public bool HasCore(Entity<AnomalyCapsuleComponent> capsule) => TryGetCore(capsule, out _);

    public bool TryGetCore(Entity<AnomalyCapsuleComponent> capsule, out Entity<AnomalyCapsuleCoreComponent> core)
    {
        core = default!;

        if (!_slots.TryGetSlot(capsule.Owner, capsule.Comp.CoreSlot, out var slot) ||
            slot.Item is not { } coreUid ||
            !TryComp<AnomalyCapsuleCoreComponent>(coreUid, out var coreComp))
            return false;

        core = (coreUid, coreComp);
        return true;
    }

    public bool TryGetAnomalyPrototype(Entity<AnomalyCapsuleComponent> capsule, out EntityPrototype anomalyPrototype, RandomTableStateComponent? state = null)
    {
        anomalyPrototype = default!;
        if (!TryGetCore(capsule, out var core))
            return false;

        var run = _randomTable.RunPrototype<EntityPrototype>(core.Comp.AnomalyPool, state: state);
        if (run.Count() <= 0)
            return false;

        anomalyPrototype = run.First();
        return true;
    }

    /// <summary>
    /// Sends the provided event and args to all modules in the capsule.
    /// </summary>
    public void RelayEventToModules<TEvent>(Entity<AnomalyCapsuleComponent> capsule, ref TEvent args) where TEvent : EntityEventArgs
    {
        foreach (var module in GetModules(capsule))
        {
            RaiseLocalEvent(module.Owner, args);
        }
    }

    private void SubscribeModuleRelayEvent<TEvent>() where TEvent : EntityEventArgs
    {
        SubscribeLocalEvent<AnomalyCapsuleComponent, TEvent>(RelayEventToModules);
    }

    /// <summary>
    /// Retrieves all modules in the declares module slot ids.
    /// </summary>
    public IEnumerable<Entity<AnomalyCapsuleModuleComponent>> GetModules(Entity<AnomalyCapsuleComponent> capsule)
    {
        foreach (var moduleSlot in capsule.Comp.ModuleSlots)
        {
            if (!_slots.TryGetSlot(capsule.Owner, moduleSlot, out var slot) ||
                slot.Item is not { } moduleUid ||
                !TryComp<AnomalyCapsuleModuleComponent>(moduleUid, out var moduleComp))
                continue;

            yield return (moduleUid, moduleComp);
        }
    }
}