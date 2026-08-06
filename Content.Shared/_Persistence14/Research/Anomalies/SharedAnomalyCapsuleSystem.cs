using System.Linq;
using Content.Shared._Persistence14.RandomTable;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Research.Anomalies;

public sealed partial class SharedAnomalyCapsuleSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly RandomTableSystem _randomTable = default!;

    private const string Sawmill = "anomaly-capsule";

    public override void Initialize()
    {

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

    public bool TryGetAnomalyPrototype(Entity<AnomalyCapsuleComponent> capsule, out EntityPrototype anomalyPrototype)
    {
        anomalyPrototype = default!;
        if (!TryGetCore(capsule, out var core))
            return false;

        var run = _randomTable.RunPrototype<EntityPrototype>(core.Comp.AnomalyPool);
        if (run.Count() <= 0)
            return false;

        anomalyPrototype = run.First();
        return true;
    }

    public void RelayEventToModules<TEvent>(Entity<AnomalyCapsuleComponent> capsule, ref TEvent args) where TEvent : EntityEventArgs
    {
        // TODO
    }
}