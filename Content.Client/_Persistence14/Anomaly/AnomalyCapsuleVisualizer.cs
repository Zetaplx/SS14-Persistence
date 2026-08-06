using Content.Shared._Persistence14.Research.Anomalies;
using Content.Shared.Containers.ItemSlots;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Client._Persistence14.Anomaly;

public sealed partial class AnomalyCapsuleVisualizerSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnomalyCapsuleVisualizerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<AnomalyCapsuleVisualizerComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<AnomalyCapsuleVisualizerComponent, EntRemovedFromContainerMessage>(OnItemRemoved);
    }

    public void OnStartup(Entity<AnomalyCapsuleVisualizerComponent> visualizer, ref ComponentStartup args)
    {
        UpdateVisuals(visualizer);
    }

    public void OnItemInserted(Entity<AnomalyCapsuleVisualizerComponent> visualizer, ref EntInsertedIntoContainerMessage args)
    {
        UpdateVisuals(visualizer);
    }

    public void OnItemRemoved(Entity<AnomalyCapsuleVisualizerComponent> visualizer, ref EntRemovedFromContainerMessage args)
    {
        UpdateVisuals(visualizer);
    }

    private void UpdateVisuals(Entity<AnomalyCapsuleVisualizerComponent> visualizer)
    {
        if (!TryComp<AnomalyCapsuleComponent>(visualizer, out var capsuleComp))
            return; // Visualizers should only be on capsules. But some capsules may use other/no visualizer.
        Entity<AnomalyCapsuleComponent> capsule = (visualizer.Owner, capsuleComp);

        if (!_slots.TryGetSlot(capsule.Owner, capsule.Comp.CoreSlot, out var slot) ||
            !TryComp<AnomalyCapsuleCoreVisualizerComponent>(slot.Item, out var coreVisualizer) ||
            !TryComp<SpriteComponent>(visualizer.Owner, out var spriteComp) ||
            !_sprite.TryGetLayer((visualizer.Owner, spriteComp), AnomalyCapsuleVisualLayers.Core, out var layer, false))
        {
            _appearance.SetData(capsule.Owner, AnomalyCapsuleVisualData.HasCore, false);
            return;
        }

        _sprite.LayerSetSprite(layer, coreVisualizer.Core);
        _appearance.SetData(capsule.Owner, AnomalyCapsuleVisualData.HasCore, true);
    }
}

public enum AnomalyCapsuleVisualData
{
    HasCore
}

public enum AnomalyCapsuleVisualLayers
{
    Base,
    Core
}