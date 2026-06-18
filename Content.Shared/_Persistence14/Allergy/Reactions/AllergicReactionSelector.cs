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

    /// <summary>
    /// Called each frame by <see cref="AllergySystem"/> when exposure is positive. Verifies the relevant conditions before calling <see cref="React"/>. 
    /// </summary>

    public void Update(ContextDependencies dependencies, AllergyContext ctx)
    {
        if (CanReactPrefix(dependencies, ctx) & BaseCanReact(dependencies, ctx) && CanReact(dependencies, ctx))
            React(dependencies, ctx);
    }

    /// <summary>
    /// Forcibly calls <see cref="React"/> once, regardless of the state of any conditions. 
    /// </summary>
    public void ForceReact(ContextDependencies dependencies, AllergyContext ctx) => React(dependencies, ctx);

    /// <summary>
    /// The effect of the allergic reaction called each update if all conditions are met.
    /// </summary>
    public abstract void React(ContextDependencies dependencies, AllergyContext ctx);

    /// <summary>
    /// Required to be true for the reaction to react on update.
    /// </summary>
    public virtual bool CanReact(ContextDependencies dependencies, AllergyContext ctx)
    {
        return true;
    }

    /// <summary>
    /// Required to be true for the reaction to react on update.<br/><br/>
    /// Called before <see cref="BaseCanReact"/>.<br/>
    /// For code which must run every check (useful for admin log messages regarding checks). 
    /// </summary>
    public virtual bool CanReactPrefix(ContextDependencies dependencies, AllergyContext ctx)
    {
        return true;
    }

    /// <summary>
    /// Required to be true for the reaction to react on update.<br/><br/>
    /// 
    /// Base implementation checking for the interval timing and exposure threshold.<br/>
    /// Called after <see cref="CanReactPrefix"/> and before <see cref="CanReact"/>  
    /// </summary>
    private bool BaseCanReact(ContextDependencies dependencies, AllergyContext ctx)
    {
        var timing = dependencies.Ensure<IGameTiming>();

        if ((timing.CurTime - ctx.LastReactTime).TotalSeconds <= Interval)
            return false;

        if (ctx.Exposure <= Threshold)
            return false;

        return true;
    }
}