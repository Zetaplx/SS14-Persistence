using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Allergy;

/// <summary>
/// The data necessary to run the various selectors and methods within the AllergySystem.
/// </summary>
[DataDefinition]
public sealed partial class AllergyContext
{
    [DataField]
    public required float FrameTime;

    [DataField]
    public required EntityUid AllergicEntityUid;

    [DataField]
    public required AllergicComponent Comp;

    [DataField]
    public ProtoId<AllergyPrototype> CurrentAllergy;

    public float Exposure
    {
        get => Comp.AllergenExposure.TryGetValue(CurrentAllergy, out var exposure) ? exposure : 0f;
        set => Comp.AllergenExposure[CurrentAllergy] = value;
    }

    public TimeSpan LastReactTime
    {
        get => Comp.LastReactionTimes.TryGetValue(CurrentAllergy, out var time) ? time : default;
        set => Comp.LastReactionTimes[CurrentAllergy] = value;
    }
}