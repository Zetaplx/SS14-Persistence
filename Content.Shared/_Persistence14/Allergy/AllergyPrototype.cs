using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared._Persistence14.Allergy;

[Prototype]
public sealed partial class AllergyPrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<AllergyPrototype>))]
    public string[]? Parents { get; private set; }

    [AbstractDataField]
    public bool Abstract { get; private set; } = false;

    [DataField(required: true)]
    public AllergenSelector Allergen = default!;

    [DataField]
    public List<AllergicReactionSelector> Reactions = new();
}