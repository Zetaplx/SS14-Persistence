using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Stacks;
using Content.Shared.Storage;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Construction.Graph.Steps;

public sealed partial class Material : ConstructionGraphStep
{
    [DataField(required: true)] private Dictionary<ProtoId<StackPrototype>, int> _materials;
    [DataField] private float _pickupRange = 2f;

    public override bool CanBeginStep(Entity<ConstructionComponent> construction, in ConstructionStepContext ctx, out EntityUid user)
    {
        user = default!;
        switch (ctx.TriggerArgs)
        {
            case InteractUsingEvent args:
                return CanBeginInteractUsing(construction, ctx, out user, args);
            case ConstructionStartEvent args:
                return CanBeginConstructionStart(construction, ctx, out user, args);
        }

        return false;
    }

    private bool CanBeginInteractUsing(Entity<ConstructionComponent> construction, in ConstructionStepContext ctx, out EntityUid user, InteractUsingEvent args)
    {
        user = args.User;

        return VerifyContainers(construction, ctx, user);
    }

    private bool CanBeginConstructionStart(Entity<ConstructionComponent> construction, in ConstructionStepContext ctx, out EntityUid user, ConstructionStartEvent args)
    {
        user = args.User;

        return VerifyContainers(construction, ctx, user); ;
    }

    private bool VerifyContainers(Entity<ConstructionComponent> construction, in ConstructionStepContext ctx, EntityUid user)
    {
        Dictionary<ProtoId<StackPrototype>, int> matReference = new(_materials);

        foreach (var item in EnumerateContainers(user, ctx, user))
        {
            if (!ctx.EntityManager.TryGetComponent<StackComponent>(item, out var stack))
                continue;

            if (matReference.TryGetValue(stack.StackTypeId, out var current))
            {
                matReference[stack.StackTypeId] -= Math.Max(stack.Count, current);
                if (matReference[stack.StackTypeId] <= 0)
                    matReference.Remove(stack.StackTypeId);
            }
        }

        return false;
    }

    public override void OnCompleteStep(Entity<ConstructionComponent> construction, in ConstructionStepContext ctx)
    {
        EntityUid user = default!;
        switch (ctx.TriggerArgs)
        {
            case InteractUsingEvent args:
                user = args.User;
                break;
            case ConstructionStartEvent args:
                user = args.User;
                break;
        }

        Dictionary<ProtoId<StackPrototype>, int> matReference = new(_materials);
        var stackSystem = ctx.EntityManager.System<SharedStackSystem>();

        // Make sure its all there first...
        if (!VerifyContainers(construction, ctx, user))
            return;

        foreach (var item in EnumerateContainers(user, ctx, user))
        {
            if (!ctx.EntityManager.TryGetComponent<StackComponent>(item, out var stack))
                continue;

            if (!matReference.TryGetValue(stack.StackTypeId, out var current))
                continue;

            var delta = Math.Max(stack.Count, current);
            stackSystem.ReduceCount((item, stack), delta);
            matReference[stack.StackTypeId] -= delta;

            // TODO: Store materials in container if requested

            if (matReference[stack.StackTypeId] <= 0)
                matReference.Remove(stack.StackTypeId);
        }
    }

    private IEnumerable<EntityUid> EnumerateContainers(EntityUid uid, ConstructionStepContext ctx, EntityUid rootUid, bool lookNearby = true)
    {
        var handSystem = ctx.EntityManager.System<SharedHandsSystem>();

        foreach (var hand in handSystem.EnumerateHeld(uid))
        {
            yield return hand;
            foreach (var handRecursive in EnumerateContainers(hand, ctx, rootUid, false))
                yield return handRecursive;
        }

        if (ctx.EntityManager.TryGetComponent<StorageComponent>(uid, out var storage))
        {
            foreach (var stored in storage.Container.ContainedEntities)
                foreach (var storedRecursive in EnumerateContainers(stored, ctx, rootUid, false))
                    yield return storedRecursive;
        }

        var inventorySystem = ctx.EntityManager.System<InventorySystem>();
        if (inventorySystem.TryGetContainerSlotEnumerator(uid, out var inventory))
        {
            while (inventory.MoveNext(out var container))
            {
                foreach (var contained in container.ContainedEntities)
                    foreach (var containedRecursive in EnumerateContainers(contained, ctx, rootUid, false))
                        yield return containedRecursive;
            }
        }

        if (!lookNearby)
            yield break;

        var transformSystem = ctx.EntityManager.System<SharedTransformSystem>();
        var pos = transformSystem.GetMapCoordinates(uid);

        var lookupSystem = ctx.EntityManager.System<EntityLookupSystem>();
        var interactionSystem = ctx.EntityManager.System<SharedInteractionSystem>();

        foreach (var nearby in lookupSystem.GetEntitiesInRange(uid, _pickupRange, LookupFlags.Dynamic | LookupFlags.Sundries | LookupFlags.Approximate))
            if (nearby != rootUid && interactionSystem.InRangeUnobstructed(pos, nearby, _pickupRange))
                foreach (var nearbyRecursive in EnumerateContainers(nearby, ctx, rootUid, false))
                    yield return nearbyRecursive;
    }
}