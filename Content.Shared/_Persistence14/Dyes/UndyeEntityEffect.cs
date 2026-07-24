using Content.Shared.EntityEffects;

namespace Content.Shared._Persistence14.Dyes;

public sealed partial class UndyeEntity : EntityEffectBase<UndyeEntity> { }

public sealed class UndyeEntityEffectSystem : EntityEffectSystem<DyeableComponent, UndyeEntity>
{
    [Dependency] private readonly SharedDyeableSystem _dye = default!;

    protected override void Effect(Entity<DyeableComponent> entity, ref EntityEffectEvent<UndyeEntity> args)
    {
        _dye.ClearColor(entity);
    }
}