using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Upgrades;

[DataDefinition]
public sealed partial class UpgradeProfile
{
    [DataField("system", required: true)]
    public string UpgradeSystem = default!;

    [DataField("max")]
    public int MaxUpgradeCount = 1;

    [DataField("multiplier")]
    public float UpgradeMultiplier = 1f;
}