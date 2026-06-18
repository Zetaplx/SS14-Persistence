using Content.Shared._Persistence14.Dependencies;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;

namespace Content.Shared._Persistence14.Allergy.Reactions;

public sealed partial class AllergicReactionDamageSelector : AllergicReactionSelector
{
    [DataField]
    public DamageSpecifier Damage = new();

    public override void React(ContextDependencies dependencies, AllergyContext ctx)
    {
        var damagable = dependencies.Ensure<DamageableSystem>();

        damagable.TryChangeDamage(ctx.AllergicEntityUid, Damage, true);
    }
}