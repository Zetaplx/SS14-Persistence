using Content.Shared._Persistence14.Dependencies;
using JetBrains.Annotations;

namespace Content.Shared._Persistence14.Allergy;

[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class AllergenSelector
{
    [DataField("exposureMultiplier")]
    private float _exposureMultiplier = 1f;

    [DataField("decayMultiplier")]
    private float _decayMultiplier = 1f;

    /// <summary>
    /// Returns whether the allergic entity is exposed to this allergen.
    /// </summary>
    public abstract bool Exposed(ContextDependencies dependencies, AllergyContext ctx);

    /// <summary>
    /// Gets the exposure multiplier for this allergen.
    /// </summary>
    public virtual float GetExposure(ContextDependencies dependencies, AllergyContext ctx) => _exposureMultiplier;

    /// <summary>
    /// Gets the exposure decay multiplier for this allergen.
    /// </summary>
    public virtual float GetDecay(ContextDependencies dependencies, AllergyContext ctx) => _decayMultiplier;
}