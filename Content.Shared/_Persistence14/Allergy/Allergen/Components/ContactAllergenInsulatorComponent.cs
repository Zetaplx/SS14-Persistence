using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Allergy.Allergen.Components;

[RegisterComponent]
public sealed partial class ContactAllergenInsulatorComponent : Component
{
    /// <summary>
    /// Allergens on which this insulator is effective. If null, all allergens are whitelisted.
    /// </summary>
    [DataField]
    public List<ProtoId<AllergyPrototype>>? AllergenWhitelist = null;

    /// <summary>
    /// Allergens on which this insulator is ineffective. If null, no allergens are blacklisted.
    /// </summary>
    [DataField]
    public List<ProtoId<AllergyPrototype>>? AllergenBlacklist = null;

    /// <summary>
    /// A percent indicator of how effective the insulator is. Additive with other insulators.<br/><br/>
    /// Current implementation has no support for partial effectiveness beyond items working together.
    /// </summary>
    [DataField]
    public float Effectiveness = 1f;

    [DataField]
    public List<SlotFlags> InsulatedSlots = new();
}