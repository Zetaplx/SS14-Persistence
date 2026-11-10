using Content.Server.Kitchen.Components;
using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class MicrowaveSpeedUpgrade : BaseUpgradeSystem<MicrowaveComponent>
{
    protected override ErrorCode ApplyUpgrade(Entity<MicrowaveComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        entity.Comp.CookTimeMultiplier -= prototype.UpgradeValues[0] * multipler; // This need non-zero protection.
        return ErrorCode.SUCCESS;
    }
}