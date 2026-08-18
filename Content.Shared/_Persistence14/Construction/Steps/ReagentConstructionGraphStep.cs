using Content.Shared.Chemistry.Reagent;
using Content.Shared.Construction;
using Content.Shared.Construction.Steps;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Construction.Steps;

[DataDefinition]
public sealed partial class ReagentConstructionGraphStep : ConstructionGraphStep
{
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> Reagent;

    [DataField(required: true)]
    public FixedPoint2 Quantity;

    public override void DoExamine(ExaminedEvent examinedEvent)
    {
        var reagent = IoCManager.Resolve<IPrototypeManager>().Index<ReagentPrototype>(Reagent);

        examinedEvent.PushMarkup(Loc.GetString("reagent-construction-graph-step", ("quantity", Quantity.Float()), ("reagentName", reagent.LocalizedName)));
    }

    public override ConstructionGuideEntry GenerateGuideEntry()
    {
        var reagent = IoCManager.Resolve<IPrototypeManager>().Index<ReagentPrototype>(Reagent);

        return new ConstructionGuideEntry()
        {
            Localization = "construction-presenter-reagent-step",
            Arguments = [("quantity", Quantity.Float()), ("reagentName", reagent.LocalizedName)],
        };
    }
}
