using System.Linq;
using Content.Shared._Persistence14.Dependencies;
using Content.Shared.Mobs;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Allergy;

/// <summary>
/// The system managing all shared aspects of allergic reactions and allergy detection.
/// </summary>
public sealed partial class AllergySystem : EntitySystem
{
    [Dependency] private IPrototypeManager _protoMan = default!;
    private ContextDependencies _dependencies = default!;

    public override void Initialize()
    {
        _dependencies = new ContextDependencies();
    }

    public override void Update(float frameTime)
    {
        var enumerator = EntityQueryEnumerator<AllergicComponent>();

        while (enumerator.MoveNext(out var uid, out var allergicComponent))
        {
            var ent = (uid, allergicComponent);
            var ctx = AssembleContext(ent);
            DecayAllergies(ent, ctx);

        }
    }

    private void DecayAllergies(Entity<AllergicComponent> ent, AllergyContext ctx)
    {
        var keys = ent.Comp.AllergenExposure.Keys.ToList();

        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            ent.Comp.AllergenExposure[key] -= ctx.FrameTime;
            if (ent.Comp.AllergenExposure[key] <= 0)
                ent.Comp.AllergenExposure.Remove(key);
        }
    }

    private void IncrementAllergies(Entity<AllergicComponent> ent, AllergyContext ctx)
    {
        var allergies = ent.Comp.Allergies;
        foreach (var allergyProtoId in allergies)
        {
            var allergy = _protoMan.Index(allergyProtoId);
            if (allergy.Allergen.Exposed(_dependencies, ctx))
            {
                if (!ent.Comp.AllergenExposure.TryGetValue(allergyProtoId, out var val))
                    val = 0f;
                ent.Comp.AllergenExposure[allergyProtoId] = val + ctx.FrameTime * allergy.Allergen.ExposureMultiplier;
            }

        }
    }

    private AllergyContext AssembleContext(Entity<AllergicComponent> ent)
    {
        return default!;
    }
}