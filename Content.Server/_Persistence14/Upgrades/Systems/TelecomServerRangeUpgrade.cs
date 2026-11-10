using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;
using Content.Shared.Radio.Components;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class TelecomServerRangeUpgrade : BaseUpgradeSystem<TelecomServerComponent>
{
    protected override ErrorCode ApplyUpgrade(Entity<TelecomServerComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        entity.Comp.MaxRange += prototype.UpgradeValues[0] * multipler;
        return ErrorCode.SUCCESS;
    }
}