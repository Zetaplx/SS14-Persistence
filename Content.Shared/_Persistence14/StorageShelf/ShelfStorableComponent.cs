using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._Persistence14.StorageShelf;

[RegisterComponent]
public sealed partial class ShelfStorableComponent : Component
{
    [DataField]
    public SpriteSpecifier? StoredSprite = null;
}