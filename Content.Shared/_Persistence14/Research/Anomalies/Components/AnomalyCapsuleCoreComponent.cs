using Content.Shared._Persistence14.RandomTable;
using Content.Shared._Persistence14.RandomTable.Selectors;

namespace Content.Shared._Persistence14.Research.Anomalies;

[RegisterComponent]
public sealed partial class AnomalyCapsuleCoreComponent : Component
{
    [DataField(required: true)]
    public RandomTableSelector AnomalyPool = new RandomTableNullSelector();
}