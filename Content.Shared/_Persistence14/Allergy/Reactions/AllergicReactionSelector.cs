using Content.Shared._Persistence14.Dependencies;
using JetBrains.Annotations;
using Robust.Shared.Timing;

namespace Content.Shared._Persistence14.Allergy;

[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class AllergicReactionSelector
{
    /// <summary>
    /// Allergen threshold required to activate the reaction.
    /// </summary>
    [DataField]
    public float Threshold = 0f;

    /// <summary>
    /// The amount of time between activations of the reaction, given the threshold is met.
    /// </summary>
    [DataField]
    public float Interval = 1f;

    public void Update(ContextDependencies dependencies, AllergyContext ctx)
    {
        if (CanReact(dependencies, ctx))
            React(dependencies, ctx);
    }

    protected abstract bool CanReact(ContextDependencies dependencies, AllergyContext ctx);
    protected abstract void React(ContextDependencies dependencies, AllergyContext ctx);
}