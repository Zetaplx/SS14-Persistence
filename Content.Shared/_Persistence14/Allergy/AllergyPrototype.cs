using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Allergy;

[Prototype]
public sealed partial class AllergyPrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    

    [DataField(required: true)]
    public AllergenSelector Allergen = default!;

    [DataField]
    public List<AllergicReactionSelector> Reactions = new();
}