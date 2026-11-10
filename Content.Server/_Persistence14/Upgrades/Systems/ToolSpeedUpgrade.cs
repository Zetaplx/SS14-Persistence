using Content.Server.Tools;
using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;
using Content.Shared.Tools.Components;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class ToolSpeedUpgrade : BaseUpgradeSystem<ToolComponent>
{
    [Dependency] private ToolSystem _tool = default!;

    protected override ErrorCode ApplyUpgrade(Entity<ToolComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        _tool.SetToolSpeedModifier(entity.Owner, entity.Comp, entity.Comp.SpeedModifier + prototype.UpgradeValues[0] * multipler);
        return ErrorCode.SUCCESS;
    }
}