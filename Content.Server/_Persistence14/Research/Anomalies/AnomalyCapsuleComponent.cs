using Content.Shared._Persistence14.RandomTable;
using Content.Shared._Persistence14.RandomTable.Selectors;

namespace Content.Shared._Persistence14.Research.Anomalies;

[RegisterComponent]
public sealed partial class AnomalyCapsuleComponent : Component
{
    [DataField(required: true)]
    public string CoreContainer = "[unknown]";

    [DataField(required: true)]
    public string ModuleContainer = "[unknown]";

    [DataField]
    public int MaxModuleCount = 2;
}

[RegisterComponent]
public sealed partial class AnomalyCapsuleCoreComponent : Component
{
    [DataField(required: true)]
    public RandomTableSelector AnomalyPool = new RandomTableNullSelector();
}