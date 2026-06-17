namespace Content.Shared._Persistence14.Allergy;

/// <summary>
/// The data necessary to run the various selectors and methods within the AllergySystem.
/// </summary>
[DataDefinition]
public sealed partial class AllergyContext
{
    [DataField]
    public float FrameTime = 0f;

    [DataField]
    public float AllergenAmount = 0f;
}