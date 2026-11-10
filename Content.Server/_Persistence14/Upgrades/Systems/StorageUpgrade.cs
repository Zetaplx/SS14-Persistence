using Content.Server.Storage.EntitySystems;
using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;
using Content.Shared.Storage;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class StorageUpgrade : BaseUpgradeSystem<StorageComponent>
{
    [Dependency] private StorageSystem _storage = default!;

    protected override ErrorCode ApplyUpgrade(Entity<StorageComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        var x = prototype.UpgradeValues[0] * multipler;
        var y = prototype.UpgradeValues[1] * multipler;
        var largest = entity.Comp.Grid.Count - 1; // This... isn't necessarily the largest grid...
        var box = entity.Comp.Grid[largest];
        entity.Comp.Grid[largest] = new Box2i(box.Left, box.Bottom, box.Right + (int)x, box.Top + (int)y);
        _storage.UpdateOccupied(entity);
        Dirty(entity);

        return ErrorCode.SUCCESS;
    }
}