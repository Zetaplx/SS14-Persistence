using Content.Shared._Persistence14.Allergy.Allergen.Components;
using Content.Shared._Persistence14.Dependencies;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Persistence14.Allergy.Allergen;

public sealed partial class ContactAllergenSelector : AllergenSelector
{
    public override bool Exposed(ContextDependencies dependencies, AllergyContext ctx)
    {
        var insulation = GetSlotInsulation(dependencies, ctx);

        if (HoldingAllergen(dependencies, ctx, insulation)) return true;
        if (WearingAllergen(dependencies, ctx, insulation)) return true;
        if (PullingAllergen(dependencies, ctx, insulation)) return true;

        return false;
    }

    private bool HoldingAllergen(ContextDependencies dependencies, AllergyContext ctx, Dictionary<SlotFlags, float> insulation)
    {
        var container = dependencies.Ensure<SharedContainerSystem>();
        var entMan = dependencies.Ensure<IEntityManager>();

        if (insulation.TryGetValue(SlotFlags.HANDS, out var value) && value >= 1f) return false;

        if (!entMan.TryGetComponent<HandsComponent>(ctx.AllergicEntityUid, out var handsComp))
            return false; // Entity has no hands

        var handIds = handsComp.Hands.Keys;
        foreach (var handId in handIds)
        {
            if (!container.TryGetContainer(ctx.AllergicEntityUid, handId, out var handContainer))
                continue;

            foreach (var held in handContainer.ContainedEntities)
            {
                if (HasMatchingAllergen(dependencies, held, ctx.CurrentAllergy)) return true;
            }
        }

        return false;
    }

    private bool WearingAllergen(ContextDependencies dependencies, AllergyContext ctx, Dictionary<SlotFlags, float> insulation)
    {
        var inventory = dependencies.Ensure<InventorySystem>();
        var entMan = dependencies.Ensure<IEntityManager>();

        if (!entMan.TryGetComponent<InventoryComponent>(ctx.AllergicEntityUid, out var inventoryComp))
            return false; //Entity has no inventory

        var enumerator = inventory.GetSlotEnumerator(ctx.AllergicEntityUid);
        while (enumerator.NextItem(out var item, out var slot))
        {
            if (insulation.TryGetValue(slot.SlotFlags, out var value) && value >= 1f) continue;

            if (HasMatchingAllergen(dependencies, item, ctx.CurrentAllergy)) return true;
        }

        return false;
    }

    private bool PullingAllergen(ContextDependencies dependencies, AllergyContext ctx, Dictionary<SlotFlags, float> insulation)
    {
        var lookup = dependencies.Ensure<EntityLookupSystem>();
        var xform = dependencies.Ensure<SharedTransformSystem>();

        if (insulation.TryGetValue(SlotFlags.HANDS, out var value) && value >= 1f) return false;

        foreach (var pullable in lookup.GetEntitiesInRange<PullableComponent>(xform.GetMapCoordinates(ctx.AllergicEntityUid), 2f))
        {
            if (pullable.Comp.Puller is not { } puller || puller != ctx.AllergicEntityUid)
                continue;

            if (HasMatchingAllergen(dependencies, pullable, ctx.CurrentAllergy)) return true;
        }

        return false;
    }

    private Dictionary<SlotFlags, float> GetSlotInsulation(ContextDependencies dependencies, AllergyContext ctx)
    {
        var inventory = dependencies.Ensure<InventorySystem>();
        var entMan = dependencies.Ensure<IEntityManager>();

        var insulationDict = new Dictionary<SlotFlags, float>();

        if (!entMan.TryGetComponent<InventoryComponent>(ctx.AllergicEntityUid, out var inventoryComp))
            return insulationDict; //Entity has no inventory

        var enumerator = inventory.GetSlotEnumerator(ctx.AllergicEntityUid);
        while (enumerator.NextItem(out var item, out var slot))
        {
            if (entMan.TryGetComponent<ContactAllergenInsulatorComponent>(item, out var insulator) && VerifyInsulatorLists(dependencies, insulator, ctx.CurrentAllergy))
            {
                foreach (var slotKey in insulator.InsulatedSlots)
                {
                    if (!insulationDict.TryGetValue(slotKey, out var val))
                        val = 0f;
                    insulationDict[slotKey] = Math.Clamp(val + insulator.Effectiveness, 0f, 1f);
                }
            }
        }
        return insulationDict;
    }

    private bool HasMatchingAllergen(ContextDependencies dependencies, EntityUid uid, ProtoId<AllergyPrototype> allergy)
    {
        var entMan = dependencies.Ensure<IEntityManager>();

        return entMan.TryGetComponent<ContactAllergenComponent>(uid, out var comp) && comp.Allergen == allergy;
    }

    private bool VerifyInsulatorLists(ContextDependencies dependencies, ContactAllergenInsulatorComponent insulator, ProtoId<AllergyPrototype> allergy)
    {
        var entMan = dependencies.Ensure<IEntityManager>();

        bool whitelist = insulator.AllergenWhitelist is null || insulator.AllergenWhitelist.Contains(allergy);
        bool blacklist = insulator.AllergenBlacklist is not null && insulator.AllergenBlacklist.Contains(allergy);

        return whitelist && !blacklist;
    }

}