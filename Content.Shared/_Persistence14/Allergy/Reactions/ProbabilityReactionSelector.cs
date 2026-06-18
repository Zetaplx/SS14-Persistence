using Content.Shared._Persistence14.Dependencies;
using Robust.Shared.Random;

namespace Content.Shared._Persistence14.Allergy.Reactions;

public abstract partial class ProbabilityReactionSelector : AllergicReactionSelector
{
    /// <summary>
    /// The probability as a decimal that the reaction can occure.
    /// </summary>
    [DataField]
    public float Probability = 1f;

    /// <inheritdoc/>
    [MustCallBase]
    public override bool CanReact(ContextDependencies dependencies, AllergyContext ctx)
    {
        var random = dependencies.Ensure<IRobustRandom>();

        return random.Prob(Probability);
    }
}