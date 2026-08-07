using Content.Shared._Persistence14.RandomTable.Conditions;

namespace Content.Shared._Persistence14.Research.Anomalies.Modules;

[RegisterComponent]
public sealed partial class CategorySpecifierCapsuleModuleComponent : Component
{
    [DataField(required: true)]
    public AnomalyCategory Category;
}

public sealed partial class CategorySpecifierCapsuleModuleSystem : AnomalyCapsuleModuleSystem<CategorySpecifierCapsuleModuleComponent>
{
    protected override void OnGenerationAttempt(Entity<CategorySpecifierCapsuleModuleComponent> module, ref AnomalyGeneratorAttemptEvent args)
    {
        var ctx = args.Context;

        ctx.RandomTableState.Data[RTCAnomalyGeneration.ContextKey] = (int)module.Comp.Category;
    }
}