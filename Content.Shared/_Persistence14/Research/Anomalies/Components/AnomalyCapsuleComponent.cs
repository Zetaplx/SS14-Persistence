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

