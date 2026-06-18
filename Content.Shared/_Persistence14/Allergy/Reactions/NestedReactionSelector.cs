using Content.Shared._Persistence14.Dependencies;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Allergy.Reactions;

public sealed partial class NestedReactionSelector : AllergicReactionSelector
{
    [DataField]
    public ProtoId<AllergicReactionPrototype> Prototype = default!;

    /// <inheritdoc/>
    public override void React(ContextDependencies dependencies, AllergyContext ctx)
    {
        var protoMan = dependencies.Ensure<IPrototypeManager>();
        var reaction = protoMan.Index(Prototype);
        reaction.Reaction.React(dependencies, ctx);
    }

    /// <inheritdoc/>
    public override bool CanReact(ContextDependencies dependencies, AllergyContext ctx)
    {
        var protoMan = dependencies.Ensure<IPrototypeManager>();
        var reaction = protoMan.Index(Prototype);
        return reaction.Reaction.CanReact(dependencies, ctx);
    }

    /// <inheritdoc/>
    public override bool CanReactPrefix(ContextDependencies dependencies, AllergyContext ctx)
    {
        var protoMan = dependencies.Ensure<IPrototypeManager>();
        var reaction = protoMan.Index(Prototype);
        return reaction.Reaction.CanReactPrefix(dependencies, ctx);
    }
}