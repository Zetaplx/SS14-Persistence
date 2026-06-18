using Content.Shared._Persistence14.Dependencies;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;

namespace Content.Shared._Persistence14.Allergy.Allergen;

public sealed partial class BloodstreamAllergenSelector : AllergenSelector
{
    [DataField(required: true)]
    public ReagentId Reagent = default!;

    [DataField]
    public FixedPoint2 Threshold = FixedPoint2.New(0.01f);

    [DataField("solution")]
    public string SolutionName = BloodstreamComponent.DefaultBloodSolutionName;

    public override bool Exposed(ContextDependencies dependencies, AllergyContext ctx)
    {
        var solutionContainer = dependencies.Ensure<SharedSolutionContainerSystem>();

        if (!solutionContainer.TryGetSolution(ctx.AllergicEntityUid, SolutionName, out var _, out var solution))
            return false; // Entity doesn't have a bloodstream to be allergic from

        if (!solution.TryGetReagentQuantity(Reagent, out var volume))
            return false; // Reagent not found in solution

        if (volume < Threshold)
            return false; // Reagent does not meet threshold

        return true;
    }
}