using Content.Shared.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Research.RecipeRelay;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RecipeContainerComponent : Component
{
    /// <summary>
    /// A lookup dictionary for unlocked recipe quantities stored by their prototype ID.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<LatheRecipePrototype>, int> UnlockedRecipes = new();
}