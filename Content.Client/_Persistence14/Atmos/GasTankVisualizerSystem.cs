using Content.Client._Persistence14.Items;
using Content.Shared._Persistence14.Atmos;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Robust.Client.GameObjects;

namespace Content.Client._Persistence14.Atmos;

public sealed partial class GasTankVisualizerSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GasTankVisualizerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GasTankVisualizerComponent, GetInhandVisualsEvent>(OnGetInhandVisuals);
        SubscribeLocalEvent<GasTankVisualizerComponent, GetEquipmentVisualsEvent>(OnGetEquipmentVisuals);
        SubscribeLocalEvent<GasTankVisualizerComponent, GetStorageVisualsEvent>(OnGetStorageVisuals);
    }

    private void OnStartup(Entity<GasTankVisualizerComponent> entity, ref ComponentStartup args)
    {
        if (TryComp<SpriteComponent>(entity.Owner, out var sprite))
            UpdateEntitySprite(entity.Owner, entity.Comp, sprite);
    }

    private void UpdateEntitySprite(EntityUid uid, GasTankVisualizerComponent component, SpriteComponent sprite)
    {
        var layers = BuildLayers(component, GasTankVisualVariants.Icon);

        foreach (var (layerKey, layer) in layers)
        {
            var index = _sprite.LayerMapReserve((uid, sprite), layerKey);

            _sprite.LayerSetData((uid, sprite), index, layer);
        }
    }

    private void OnGetStorageVisuals(Entity<GasTankVisualizerComponent> entity, ref GetStorageVisualsEvent args)
    {
        var layers = new Dictionary<GasTankVisualLayers, PrototypeLayerData>();
        if (entity.Comp.Variants.ContainsKey(GasTankVisualVariants.Storage))
            layers = BuildLayers(entity.Comp, GasTankVisualVariants.Storage);
        else
            layers = BuildLayers(entity.Comp, GasTankVisualVariants.Icon); // If no storage available, use icon.

        foreach (var (layerKey, layer) in layers)
        {
            args.Layers.Add(($"gas-tank-{layerKey}", layer));
        }
    }

    private void OnGetInhandVisuals(Entity<GasTankVisualizerComponent> entity, ref GetInhandVisualsEvent args)
    {
        if (args.Location != HandLocation.Right &&
            args.Location != HandLocation.Left)
            return; // No visuals for middle hands.

        var variant = args.Location switch
        {
            HandLocation.Left => GasTankVisualVariants.InhandLeft,
            HandLocation.Right => GasTankVisualVariants.InhandRight,
            _ => throw new InvalidOperationException()
        };
        var layers = BuildLayers(entity.Comp, variant);

        foreach (var (layerKey, layer) in layers)
        {
            args.Layers.Add(($"gas-tank-{layerKey}", layer));
        }
    }

    private void OnGetEquipmentVisuals(Entity<GasTankVisualizerComponent> entity, ref GetEquipmentVisualsEvent args)
    {
        var variant = GetEquipmentVariant(entity, args);
        if (variant is not { } var)
            return;

        var layers = BuildLayers(entity.Comp, var);

        foreach (var (layerKey, layer) in layers)
        {
            args.Layers.Add(($"gas-tank-{args.Slot}-{layerKey}", layer));
        }
    }

    private GasTankVisualVariants? GetEquipmentVariant(Entity<GasTankVisualizerComponent> entity, GetEquipmentVisualsEvent args)
    {
        switch (args.Slot)
        {
            case "back":
                return GasTankVisualVariants.EquippedBackpack;
            case "belt":
                return GasTankVisualVariants.EquippedBelt;
            case "suitstorage":
                return GetEquipmentVariant(args.Equipee);
            default:
                return null;
        }
    }

    private GasTankVisualVariants? GetEquipmentVariant(EntityUid equipee)
    {
        if (!TryComp<InventoryComponent>(equipee, out var inventory))
            return GasTankVisualVariants.EquippedSuitStorage;

        return inventory.SpeciesId?.ToLowerInvariant() switch
        {
            "cat" => GasTankVisualVariants.EquippedSuitStorage_Cat,
            "dog" => GasTankVisualVariants.EquippedSuitStorage_Dog,
            "fox" => GasTankVisualVariants.EquippedSuitStorage_Fox,
            "hamster" => GasTankVisualVariants.EquippedSuitStorage_Hamster,
            "kangaroo" => GasTankVisualVariants.EquippedSuitStorage_Kangaroo,
            "pig" => GasTankVisualVariants.EquippedSuitStorage_Pig,
            "possum" => GasTankVisualVariants.EquippedSuitStorage_Possum,
            "puppy" => GasTankVisualVariants.EquippedSuitStorage_Puppy,
            "sloth" => GasTankVisualVariants.EquippedSuitStorage_Sloth,

            _ => GasTankVisualVariants.EquippedSuitStorage
        };
    }

    private Dictionary<GasTankVisualLayers, PrototypeLayerData> BuildLayers(GasTankVisualizerComponent component, GasTankVisualVariants variant)
    {
        var layers = new Dictionary<GasTankVisualLayers, PrototypeLayerData>();

        if (!component.Variants.TryGetValue(variant, out var rsi))
            return layers;

        var rsiPath = rsi.ToString();

        // Border
        var border = new PrototypeLayerData
        {
            RsiPath = rsiPath,
            State = "border"
        };
        if (component.LayerColors.TryGetValue(GasTankVisualLayers.Border, out var borderColor))
        {
            border.Color = borderColor;
        }
        layers[GasTankVisualLayers.Border] = border;

        // Base
        var baseLayer = new PrototypeLayerData
        {
            RsiPath = rsiPath,
            State = GetBaseState(component)
        };
        if (component.LayerColors.TryGetValue(GasTankVisualLayers.Base, out var baseColor))
        {
            baseLayer.Color = baseColor;
        }
        layers[GasTankVisualLayers.Base] = baseLayer;

        // Lower Stripe
        if (component.LayerColors.TryGetValue(GasTankVisualLayers.StripeLower, out var lowerStripeColor))
        {
            layers[GasTankVisualLayers.StripeLower] = new PrototypeLayerData
            {
                RsiPath = rsiPath,
                State = "stripe-lower",
                Color = lowerStripeColor
            };
        }

        // Upper Stripe
        if (component.LayerColors.TryGetValue(GasTankVisualLayers.StripeUpper, out var upperStripeColor))
        {
            layers[GasTankVisualLayers.StripeUpper] = new PrototypeLayerData
            {
                RsiPath = rsiPath,
                State = "stripe-upper",
                Color = upperStripeColor
            };
        }

        return layers;
    }

    private static string GetBaseState(GasTankVisualizerComponent component)
    {
        var lower = component.LayerColors.ContainsKey(GasTankVisualLayers.StripeLower);
        var upper = component.LayerColors.ContainsKey(GasTankVisualLayers.StripeUpper);

        return (lower, upper) switch
        {
            (false, false) => "base",
            (true, false) => "base-striped-lower",
            (false, true) => "base-striped-upper",
            (true, true) => "base-striped-double"
        };
    }
}