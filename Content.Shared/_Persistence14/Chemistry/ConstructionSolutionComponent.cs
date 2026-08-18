namespace Content.Shared._Persistence14.Chemistry;

/// <summary>
/// A component identifying an entity as a valid source for the <see cref="Construction.Steps.ReagentConstructionGraphStep"/>.
/// </summary>
[RegisterComponent]
public sealed partial class ConstructionSolutionComponent : Component
{
    /// <summary>
    /// Solution name that used in construction.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string Solution = "default";
}