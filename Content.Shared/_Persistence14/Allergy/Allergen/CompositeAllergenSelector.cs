using Content.Shared._Persistence14.Dependencies;

namespace Content.Shared._Persistence14.Allergy.Allergen;

public sealed partial class CompositeAllergenSelector : AllergenSelector
{
    [DataField]
    public AllergenSelectorMode Mode = AllergenSelectorMode.All;

    [DataField]
    public List<AllergenSelector> Children = new();

    /// <inheritdoc/>
    public override bool Exposed(ContextDependencies dependencies, AllergyContext ctx)
    {
        foreach (var child in Children)
        {
            var exposed = child.Exposed(dependencies, ctx);
            if (exposed)
            {
                if (Mode == AllergenSelectorMode.Any) return true;
                if (Mode == AllergenSelectorMode.None) return false;
            }
            if (!exposed && Mode == AllergenSelectorMode.All) return false;
        }

        // Two ways of getting here
        //  1) All children were true and Mode = All
        //  2) All children were false and Mode =/= All
        // In case 1, Mode =/= Any and we should return true.
        // In case 2, Mode = Any should return false (since at least one was false) and Mode = None thus Mode =/= Any and we should return true
        return Mode != AllergenSelectorMode.Any;
    }

    /// <inheritdoc/>
    public override float GetExposure(ContextDependencies dependencies, AllergyContext ctx)
    {
        var exposureMultipler = base.GetExposure(dependencies, ctx);

        foreach (var child in Children)
            exposureMultipler *= child.GetExposure(dependencies, ctx);

        return exposureMultipler;
    }

    /// <inheritdoc/>
    public override float GetDecay(ContextDependencies dependencies, AllergyContext ctx)
    {
        var decayMultiplier = base.GetDecay(dependencies, ctx);

        foreach (var child in Children)
            decayMultiplier *= child.GetDecay(dependencies, ctx);

        return decayMultiplier;
    }
}

public enum AllergenSelectorMode
{
    All,
    Any,
    None
}