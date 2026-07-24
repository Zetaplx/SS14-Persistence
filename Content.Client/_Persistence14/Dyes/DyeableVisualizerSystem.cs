using Content.Client.Clothing;
using Content.Shared._Persistence14.Dyes;
using Content.Shared.Clothing;
using Robust.Client.GameObjects;


namespace Content.Client._Persistence14.Dyes;

public sealed class DyeableVisualizerSystem : VisualizerSystem<DyeableComponent>
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DyeableComponent, GetEquipmentVisualsEvent>(OnGetEquipmentVisuals, after: [typeof(ClientClothingSystem)]);
    }

    /// <summary>
    /// Updates basic sprite appearance to account for dyed colors.
    /// </summary>
    protected override void OnAppearanceChange(EntityUid uid, DyeableComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<Color>(uid, DyeableVisuals.Color, out var color, args.Component))
            return;

        var sprite = (uid, args.Sprite);

        if (component.Layers.Count <= 0)
        {
            _sprite.SetColor(sprite, color);
            return;
        }

        foreach (var layerKey in component.Layers)
        {
            if (!_sprite.LayerMapTryGet(sprite, layerKey, out var layer, false))
                continue;

            _sprite.LayerSetColor((uid, args.Sprite), layer, color);
        }
    }

    /// <summary>
    /// Updates the equipment visuals of the entity to use dyed colors.
    /// </summary>
    public void OnGetEquipmentVisuals(Entity<DyeableComponent> entity, ref GetEquipmentVisualsEvent args)
    {
        if (entity.Comp.Color is not { } color)
            return;

        for (int i = 0; i < args.Layers.Count; i++)
        {
            var (key, layer) = args.Layers[i];
            if (!ShouldDyeLayer(entity.Comp, key))
                continue;

            layer.Color = color;
            args.Layers[i] = (key, layer);
        }
    }

    /// <summary>
    /// Determines if a given layer should be dyed based on the settings in <see cref="DyeableComponent"/>
    /// </summary>
    private bool ShouldDyeLayer(DyeableComponent component, object layerKey)
    {
        if (component.Layers.Count == 0)
            return true;

        return component.Layers.Contains(layerKey.ToString()!);
    }
}