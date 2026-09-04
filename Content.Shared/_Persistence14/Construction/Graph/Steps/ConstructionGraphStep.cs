using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Construction.Graph;

[ImplicitDataDefinitionForInheritors]
public abstract partial class ConstructionGraphStep
{
    [DataField]
    public TimeSpan DoAfter = TimeSpan.Zero;

    public float DoAfterMultiplier { get; protected set; } = 1f;
    public float DoAfterAddifier { get; protected set; } = 0f;

    public abstract bool CanBeginStep(Entity<ConstructionComponent> component, in ConstructionStepContext ctx, out EntityUid user);
    public abstract void OnCompleteStep(Entity<ConstructionComponent> component, in ConstructionStepContext ctx);
}

public sealed class ConstructionStepContext
{
    public required EntityEventArgs? TriggerArgs;

    public required IEntityManager EntityManager;
    public required IPrototypeManager PrototypeManager;
}