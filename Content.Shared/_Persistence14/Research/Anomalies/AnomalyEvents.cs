using Robust.Shared.Map;

namespace Content.Shared._Persistence14.Research.Anomalies;

[ByRefEvent]
public sealed partial class AnomalyGeneratorAttemptEvent : CancellableEntityEventArgs
{
    public required AnomalyGenerationContext Context;
}

[ByRefEvent]
public sealed partial class GenerateAnomalyEvent : EntityEventArgs { }

public sealed partial class AnomalyGenerationContext
{
    public required EntityUid GeneratorUid;
    public required Entity<AnomalyCapsuleComponent> Capsule;
    public EntityCoordinates? TargetCoordinates = null;
}

public sealed class AnomalyGeneratorBUIState : BoundUserInterfaceState
{
    public required TimeSpan? GenerateEndTime;
    public bool IsGenerating => GenerateEndTime != null;
    public required TimeSpan GenerateDuration;
    public required TimeSpan? CooldownEndTime;
    public bool IsOnCooldown => CooldownEndTime != null;
    public required TimeSpan CooldownDuration;

    public required bool CanGenerateAnomaly;
}
