using Robust.Shared.Timing;

namespace Content.Shared._Persistence14.Allergy;


/// <summary>
/// A storage vessel for dependencies required by selectors within the AllergySystem
/// </summary>
public sealed partial class AllergyDependency
{
    public required AllergySystem AllergySystem { get; init; }
    public required IGameTiming Timing { get; init; }
}