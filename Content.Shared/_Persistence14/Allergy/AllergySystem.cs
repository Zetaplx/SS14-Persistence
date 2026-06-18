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
            var ctx = AssembleContext(ent, frameTime);
            DecayAllergies(ent, ctx);
            IncrementAllergies(ent, ctx);
            UpdateReactions(ent, ctx);
        }
    }

    /// <summary>
    /// Decrements the exposure of all allergies.
    /// </summary>
    private void DecayAllergies(Entity<AllergicComponent> ent, AllergyContext ctx)
    {
        var keys = ent.Comp.AllergenExposure.Keys.ToList();

        for (int i = 0; i < keys.Count; i++)
        {
            var allergyProtoId = keys[i];
            var allergy = _protoMan.Index(allergyProtoId);
            ctx.CurrentAllergy = allergy;
            ent.Comp.AllergenExposure[allergyProtoId] -= ctx.FrameTime * allergy.Allergen.GetDecay(_dependencies, ctx);
            if (ent.Comp.AllergenExposure[allergyProtoId] <= 0)
                ent.Comp.AllergenExposure.Remove(allergyProtoId);
        }
    }

    /// <summary>
    /// Increments the exposure of all exposed allergies owned by the entity.
    /// </summary>
    private void IncrementAllergies(Entity<AllergicComponent> ent, AllergyContext ctx)
    {
        var allergies = ent.Comp.Allergies;
        foreach (var allergyProtoId in allergies)
        {
            var allergy = _protoMan.Index(allergyProtoId);
            ctx.CurrentAllergy = allergy;
            if (allergy.Allergen.Exposed(_dependencies, ctx))
            {
                if (!ent.Comp.AllergenExposure.TryGetValue(allergyProtoId, out var val))
                    val = 0f;
                ent.Comp.AllergenExposure[allergyProtoId] = val + ctx.FrameTime * allergy.Allergen.GetExposure(_dependencies, ctx);
            }
        }
    }

    /// <summary>
    /// Updates the reactions of any active allergy.
    /// </summary>
    private void UpdateReactions(Entity<AllergicComponent> ent, AllergyContext ctx)
    {
        var allergies = ent.Comp.AllergenExposure.Keys; // Only update the active allergens with non-zero exposure.
        foreach (var allergyProtoId in allergies)
        {
            var allergy = _protoMan.Index(allergyProtoId);
            ctx.CurrentAllergy = allergy;
            foreach (var reaction in allergy.Reactions)
                reaction.Update(_dependencies, ctx);
        }
    }

    /// <summary>
    /// Assembles an <see cref="AllergyContext"/> based on the current state of the provided <see cref="AllergicComponent"/> and the frame time.  
    /// </summary>
    private AllergyContext AssembleContext(Entity<AllergicComponent> ent, float frameTime)
    {
        var ctx = new AllergyContext
        {
            FrameTime = frameTime,
            AllergicEntityUid = ent,
            Comp = ent.Comp,
        };

        return ctx;
    }
}