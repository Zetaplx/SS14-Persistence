using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;
using Content.Shared.Armor;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class ArmorUpgrade : BaseUpgradeSystem<ArmorComponent>
{
    protected override ErrorCode ApplyUpgrade(Entity<ArmorComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        if (prototype.ArmorModifiers == null)
            return ErrorCode.FAILURE;

        if (entity.Comp.Modifiers == null)
        {
            entity.Comp.Modifiers = new Shared.Damage.DamageModifierSet();
        }
        foreach (var damageCo in prototype.ArmorModifiers.Coefficients)
        {
            var dmgCo = damageCo.Value * multipler;
            if (entity.Comp.Modifiers.Coefficients.ContainsKey(damageCo.Key))
            {
                entity.Comp.Modifiers.Coefficients[damageCo.Key] -= dmgCo;
            }
            else
            {
                entity.Comp.Modifiers.Coefficients[damageCo.Key] = 1 - dmgCo;
            }
        }
        foreach (var damageFlat in prototype.ArmorModifiers.FlatReductions)
        {
            var dmgFlat = damageFlat.Value * multipler;
            if (entity.Comp.Modifiers.FlatReductions.ContainsKey(damageFlat.Key))
            {
                entity.Comp.Modifiers.FlatReductions[damageFlat.Key] += dmgFlat;
            }
            else
            {
                entity.Comp.Modifiers.FlatReductions[damageFlat.Key] = dmgFlat;
            }
        }
        Dirty(entity);
        return ErrorCode.SUCCESS;
    }
}