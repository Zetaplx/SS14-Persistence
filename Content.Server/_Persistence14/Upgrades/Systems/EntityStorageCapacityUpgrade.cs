using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;
using Content.Shared.Storage.Components;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class EntityStorageCapacityUpgrade : BaseUpgradeSystem<EntityStorageComponent>
{
    protected override ErrorCode ApplyUpgrade(Entity<EntityStorageComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        entity.Comp.Capacity += (int)(prototype.UpgradeValues[0] * multipler);
        return ErrorCode.SUCCESS;
    }
}