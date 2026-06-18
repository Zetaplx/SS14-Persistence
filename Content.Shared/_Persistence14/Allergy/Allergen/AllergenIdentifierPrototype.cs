using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Allergy.Allergen;

/// <summary>
/// An incredibly simple identifier prototype for creating typable references for allergens. <br/>
/// Mainly used in contact and proximity allergen detection.
/// </summary>
[Prototype]
public sealed partial class AllergenIdentifierPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;
}