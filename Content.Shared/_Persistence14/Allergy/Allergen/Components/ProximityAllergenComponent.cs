using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Allergy.Allergen.Components;

[RegisterComponent]
public sealed partial class ProximityAllergenComponent : Component
{
    public const float DefaultExposureRadius = 10;

    [DataField(required: true)]
    public ProtoId<AllergyPrototype> Allergen;

    [DataField]
    public float ExposureRadius = DefaultExposureRadius;
}