using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Allergy;

[Prototype]
public sealed partial class AllergyPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField]
    public AllergenSelector? Allergen;

    [DataField]
    public AllergicReactionSelector? Reaction;
}