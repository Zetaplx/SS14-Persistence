using Content.Shared._Persistence14.Allergy.Allergen.Components;
using Content.Shared._Persistence14.Dependencies;

namespace Content.Shared._Persistence14.Allergy.Allergen;

public sealed partial class ProximityAllergenSelector : AllergenSelector
{
    private const float DefaultMaxRadius = 20f;

    [DataField("radius")]
    private float _maxRadius = DefaultMaxRadius;

    public override bool Exposed(ContextDependencies dependencies, AllergyContext ctx)
    {
        var lookup = dependencies.Ensure<EntityLookupSystem>();
        var xform = dependencies.Ensure<SharedTransformSystem>();

        var coords = xform.GetMapCoordinates(ctx.AllergicEntityUid);
        var pos = xform.GetWorldPosition(ctx.AllergicEntityUid);

        var allergensWithin = lookup.GetEntitiesInRange<ProximityAllergenComponent>(coords, _maxRadius);

        foreach (var allergen in allergensWithin)
        {
            if (allergen.Comp.Allergen != ctx.CurrentAllergy) continue;

            var allergenPos = xform.GetWorldPosition(allergen);
            if ((pos - allergenPos).IsShorterThanOrEqualTo(allergen.Comp.ExposureRadius))
            {
                return true;
            }
        }

        return false;
    }
}