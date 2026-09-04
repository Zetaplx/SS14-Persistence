using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Construction.Graph;

[ImplicitDataDefinitionForInheritors]
public abstract partial class ConstructionGraphCondition
{
    public abstract bool VerifyCondition(Entity<ConstructionComponent> construction, in ConstructionConditionContext ctx);
}

public sealed class ConstructionConditionContext
{
    public required IEntityManager EntityManager;
    public required IPrototypeManager PrototypeManager;
}