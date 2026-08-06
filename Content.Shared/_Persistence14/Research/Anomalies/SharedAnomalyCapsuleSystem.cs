using System.Linq;
using Content.Shared._Persistence14.RandomTable;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Research.Anomalies;

public sealed partial class SharedAnomalyCapsuleSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly RandomTableSystem _randomTable = default!;

    private const string Sawmill = "anomaly-capsule";

    public override void Initialize()
    {
        SubscribeLocalEvent<AnomalyCapsuleCoreComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<AnomalyCapsuleComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
    }

    public void OnInteractUsing(Entity<AnomalyCapsuleCoreComponent> core, ref InteractUsingEvent args)
    {
        if (!TryComp<AnomalyCapsuleComponent>(args.Target, out var capsule))
            return;

        if (TryInsertCore((args.Target, capsule), core))
            args.Handled = true;
    }

    public bool HasCore(Entity<AnomalyCapsuleComponent> capsule, bool log = true) => TryGetCore(capsule, out _, log);

    public bool TryGetCore(Entity<AnomalyCapsuleComponent> capsule, out Entity<AnomalyCapsuleCoreComponent> core, bool log = true)
    {
        core = default!;
        var container = _containerSystem.GetContainer(capsule.Owner, capsule.Comp.CoreContainer);
        if (container.ContainedEntities.Count > 1 && log)
            LogManager.GetSawmill(Sawmill).Error($"Invalid amount of cores in container {capsule.Comp.CoreContainer} in entity {ToPrettyString(capsule)}");

        if (container.ContainedEntities.Count != 1)
            return false;

        var coreUid = container.ContainedEntities.First();
        if (!TryComp<AnomalyCapsuleCoreComponent>(coreUid, out var coreComp))
        {
            if (log) LogManager.GetSawmill(Sawmill).Error($"Invalid core. {ToPrettyString(coreUid)} missing component {typeof(AnomalyCapsuleCoreComponent).FullName}.");
            return false;
        }
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

    public bool TryInsertCore(Entity<AnomalyCapsuleComponent> capsule, Entity<AnomalyCapsuleCoreComponent> core)
    {
        var container = _containerSystem.GetContainer(capsule.Owner, capsule.Comp.CoreContainer);
        if (container.ContainedEntities.Count > 0)
            return false;

        return _containerSystem.Insert(core.Owner, container);
    }

    public bool TryRemoveCore(Entity<AnomalyCapsuleComponent> capsule, out Entity<AnomalyCapsuleCoreComponent> core)
    {
        core = default!;
        var container = _containerSystem.GetContainer(capsule.Owner, capsule.Comp.CoreContainer);
        if (!TryGetCore(capsule, out core))
            return false;

        return _containerSystem.TryRemoveFromContainer(core.Owner);
    }

    private void OnGetAltVerbs(Entity<AnomalyCapsuleComponent> capsule, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!HasCore(capsule))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = "eject core",
            Act = () => TryRemoveCore(capsule, out _),
            Priority = 1
        });
    }

    public void RelayEventToModule<TEvent>(Entity<AnomalyCapsuleComponent> capsule, ref TEvent args) where TEvent : EntityEventArgs
    {
        // TODO: Relay the event to all modules.
    }
}