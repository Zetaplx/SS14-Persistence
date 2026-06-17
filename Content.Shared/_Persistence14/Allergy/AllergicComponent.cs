using Robust.Shared.GameStates;

namespace Content.Shared._Persistence14.Allergy;

/// <summary>
/// A component for entities which need to handle allergic reactions to allergens.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AllergicComponent : Component
{
    /// <summary>
    /// All current allergies for the entity
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<string> Allergies = new();
}