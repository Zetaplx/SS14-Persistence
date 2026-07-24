using Content.Shared.EntityEffects;
using Content.Shared.Mobs;

namespace Content.Shared._Persistence14.Dyes;

public sealed partial class DyeEntity : EntityEffectBase<DyeEntity>
{
    [DataField]
    public float Strength = 5f / 255f;

    [DataField]
    public float R, G, B;
}

public sealed class DyeEntityEffectSystem : EntityEffectSystem<DyeableComponent, DyeEntity>
{
    [Dependency] private readonly SharedDyeableSystem _dye = default!;

    protected override void Effect(Entity<DyeableComponent> entity, ref EntityEffectEvent<DyeEntity> args)
    {
        var scale = args.Effect.Strength * args.Scale;
        _dye.ModifyColor(entity, args.Effect.R * scale, args.Effect.G * scale, args.Effect.B * scale);
    }
}