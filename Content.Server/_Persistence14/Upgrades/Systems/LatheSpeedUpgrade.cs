using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;
using Content.Shared.Lathe;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class LatheSpeedUpgrade : BaseUpgradeSystem<LatheComponent>
{
    protected override ErrorCode ApplyUpgrade(Entity<LatheComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        entity.Comp.TimeMultiplier -= prototype.UpgradeValues[0] * multipler; // This needs non-zero protection
        return ErrorCode.SUCCESS;
    }
}