namespace Content.Shared._Persistence14.Dyes;

public sealed partial class SharedDyeableSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DyeableComponent, ComponentInit>(OnComponentInit);
    }

    public void OnComponentInit(Entity<DyeableComponent> entity, ref ComponentInit args)
    {
        if (entity.Comp.Dyed)
        {
            _appearanceSystem.SetData(entity.Owner, DyeableVisuals.Color, entity.Comp.Color!);
        }
    }
    public void SetColor(Entity<DyeableComponent> entity, Color color)
    {
        if (!entity.Comp.Dyed && entity.Comp.DefaultColor is null && _appearanceSystem.TryGetData(entity.Owner, DyeableVisuals.Color, out var currentColor))
        {
            entity.Comp.DefaultColor = (Color)currentColor;
        }

        entity.Comp.Color = color;
        Dirty(entity);

        _appearanceSystem.SetData(entity.Owner, DyeableVisuals.Color, color);
    }

    public void ModifyColor(Entity<DyeableComponent> entity, float r, float g, float b)
    {
        var color = entity.Comp.Color ?? Color.White;
        color.R = color.R + r;
        color.G = color.G + g;
        color.B = color.B + b;

        SetColor(entity, color);
    }

    public void ClearColor(Entity<DyeableComponent> entity)
    {
        entity.Comp.Color = null;

        if (entity.Comp.DefaultColor is { } defaultColor)
        {
            _appearanceSystem.SetData(entity.Owner, DyeableVisuals.Color, defaultColor);
        }
        else
        {
            _appearanceSystem.SetData(entity.Owner, DyeableVisuals.Color, Color.White);
        }
        entity.Comp.DefaultColor = null;
        Dirty(entity);

    }
}