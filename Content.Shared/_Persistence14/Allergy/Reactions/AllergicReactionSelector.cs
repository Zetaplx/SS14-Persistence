using JetBrains.Annotations;

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

    [DataField]
    public TimeSpan LastReactionTime;

    public void Update(AllergyDependency dependencies, AllergyContext ctx)
    {
        if (CanReact(dependencies, ctx))
            React(dependencies, ctx);
    }

    [MustCallBase]
    protected virtual bool CanReact(AllergyDependency dependencies, AllergyContext ctx)
    {
        var currTime = dependencies.Timing.CurTime;
        if ((currTime - LastReactionTime).TotalSeconds < Interval)
            return false;

        if (ctx.AllergenAmount < Threshold)
            return false;

        return true;
    }
    protected abstract void React(AllergyDependency dependencies, AllergyContext ctx);
}