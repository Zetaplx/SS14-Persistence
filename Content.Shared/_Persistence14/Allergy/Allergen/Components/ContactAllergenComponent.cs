using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Allergy.Allergen.Components;

[RegisterComponent]
public sealed partial class ContactAllergenComponent : Component
{
    [DataField(required: true)]
    public ProtoId<AllergyPrototype> Allergen;
}