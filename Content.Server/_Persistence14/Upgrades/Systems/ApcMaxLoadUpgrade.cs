using Content.Server.Power.Components;
using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class ApcMaxLoadUpgrade : BaseUpgradeSystem<ApcComponent>
{
    protected override ErrorCode ApplyUpgrade(Entity<ApcComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        entity.Comp.MaxLoad += prototype.UpgradeValues[0] * multipler;
        return ErrorCode.SUCCESS;
    }
}