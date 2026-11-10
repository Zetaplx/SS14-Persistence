using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Upgrades.Prototypes;

[Prototype]
public sealed partial class UpgradePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name { get; set; } = string.Empty;
    [DataField]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The values for the particular upgrade. Some types of upgrades may expect different amounts of information.
    /// Check the applicable UpgradeSystem to get a sense for what is expected.
    /// </summary>
    [DataField]
    public List<float> UpgradeValues { get; set; } = new List<float>();

    /// <summary>
    /// Used when an upgrade should modify the armor values of an item.
    /// </summary>
    [DataField]
    public DamageModifierSet? ArmorModifiers = null;


    /// <summary>
    /// Upgrades that are required in order for this upgrade to be applied.
    /// </summary>
    [DataField]
    public ProtoId<UpgradePrototype>[] Whitelist = { };

    /// <summary>
    /// Upgrades that are incompatible with this upgrade. This upgrade cannot be applied if these upgrades are present.
    /// </summary>
    [DataField]
    public ProtoId<UpgradePrototype>[] Blacklist = { };

}
