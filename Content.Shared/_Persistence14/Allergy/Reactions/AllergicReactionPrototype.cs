using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Allergy.Reactions;

[Prototype]
public sealed partial class AllergicReactionPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField]
    public AllergicReactionSelector Reaction = default!;
}