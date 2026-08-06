using Content.Server.Anomaly.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Radio.EntitySystems;
using Content.Shared._Persistence14.Research.Anomalies;
using Content.Shared.Anomaly;
using Content.Shared.CCVar;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Physics;
using Content.Shared.Radio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Persistence14.Research.Anomalies;

public sealed partial class AnomalyGeneratorSystem : SharedAnomalyGeneratorSystem
{
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedAnomalyCapsuleSystem _capsules = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    private const int RandomCoordinateAttempts = 25;
    private const string Sawmill = "anomaly-generator";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnomalyGeneratorComponent, GenerateAnomalyEvent>(OnGenerateAnomaly);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GeneratingAnomalyGeneratorComponent, AnomalyGeneratorComponent>();
        while (query.MoveNext(out var ent, out var active, out var gen))
        {
            if (_time.CurTime < active.EndTime)
                continue;

            active.AudioStream = _audio.Stop(active.AudioStream);
            FinishAnomalyGenerator((ent, gen));
        }
    }

    #region Signal Hooks
    private void OnGenerateAnomaly(Entity<AnomalyGeneratorComponent> generator, ref GenerateAnomalyEvent args)
    {
        StartAnomalyGenerator(generator);
    }
    #endregion

    #region UI

    private void UpdateGeneratorUi(Entity<AnomalyGeneratorComponent> generator)
    {
        var isGenerating = TryComp<GeneratingAnomalyGeneratorComponent>(generator.Owner, out var generatingComp);
        var isOnCooldown = _time.CurTime < generator.Comp.CooldownEndTime;

        var canGenerate = CanGenerateAnomaly(generator, out var capsule, out var anomalyPrototype);

        var state = new AnomalyGeneratorBUIState
        {
            GenerateDuration = generator.Comp.GenerationLength,
            GenerateEndTime = isGenerating ? generatingComp?.EndTime : null,
            CooldownDuration = generator.Comp.CooldownLength,
            CooldownEndTime = isOnCooldown ? generator.Comp.CooldownEndTime : null,

            CanGenerateAnomaly = canGenerate
        };
        _ui.SetUiState(generator.Owner, AnomalyGeneratorUiKey.Key, state);
    }

    #endregion

    #region Generation
    private bool CanGenerateAnomaly(Entity<AnomalyGeneratorComponent> generator) => CanGenerateAnomaly(generator, out _, out _);
    private bool CanGenerateAnomaly(Entity<AnomalyGeneratorComponent> generator, out Entity<AnomalyCapsuleComponent> capsule, out EntityPrototype anomalyPrototype)
    {
        capsule = default!;
        anomalyPrototype = default!;
        if (!TryGetAnomalyCapsule(generator, out capsule))
            return false;

        if (!_capsules.TryGetAnomalyPrototype(capsule, out anomalyPrototype))
            return false;

        return true;
    }

    /// <summary>
    /// Attempts to generate an anomaly using the capsule contained within the generator. The type of anomaly and its location depend on the capsule used.
    /// </summary>
    private bool TryGenerateAnomaly(Entity<AnomalyGeneratorComponent> generator)
    {
        if (!CanGenerateAnomaly(generator, out var capsule, out var anomalyPrototype))
            return false;

        var ev = new AnomalyGeneratorAttemptEvent
        {
            Context = new AnomalyGenerationContext
            {
                GeneratorUid = generator.Owner,
                Capsule = capsule
            }
        };
        _capsules.RelayEventToModule(capsule, ref ev);
        RaiseLocalEvent(ref ev);
        if (ev.Cancelled)
            return false;

        if (ev.Context.TargetCoordinates is not { } coordinates && !TryGetCoordinatesOnGrid(generator.Owner, out coordinates))
            return false;

        Spawn(anomalyPrototype.ID, coordinates);
        return true;
    }

    public void GenerateAnomalyOnGrid(EntityUid grid, EntProtoId anomalyProtoId)
    {
        if (!TryGetCoordinatesOnGrid(grid, out var coordinates))
        {
            LogManager.GetSawmill(Sawmill).Warning($"Attempted to manually spawn anomaly but failed to find valid coordinates on grid {ToPrettyString(grid)}");
            return;
        }

        Spawn(anomalyProtoId, coordinates);
    }

    /// <summary>
    /// Attempts to get a random set of coordinates from the grid containing the anomaly generator.
    /// Taken basically whole sale from the old anomaly generation logic in AnomalySystem.Generator.cs.
    /// </summary>
    private bool TryGetCoordinatesOnGrid(EntityUid targetUid, out EntityCoordinates coordinates)
    {
        coordinates = default!;
        var xform = Transform(targetUid);

        if (xform.GridUid is null ||
            !TryComp<MapGridComponent>(xform.GridUid, out var gridComp)) // Generator isn't on a grid. For some reason.
            return false;
        Entity<MapGridComponent> grid = (xform.GridUid.Value, gridComp);
        var gridBounds = gridComp.LocalAABB.Scale(_configuration.GetCVar(CCVars.AnomalyGenerationGridBoundsScale));

        for (int i = 0; i < RandomCoordinateAttempts; i++)
        {
            var randomX = _random.Next((int)gridBounds.Left, (int)gridBounds.Right);
            var randomY = _random.Next((int)gridBounds.Bottom, (int)gridBounds.Top);

            var tile = new Vector2i(randomX, randomY);

            // No Air-Blocked Areas
            if (_atmos.IsTileSpace(grid.Owner, xform.MapUid, tile) ||
                _atmos.IsTileAirBlocked(grid, tile))
                continue;

            // Don't spawn inside solid things
            var physQuery = GetEntityQuery<PhysicsComponent>();
            var valid = true;
            foreach (var ent in _map.GetAnchoredEntities(grid, gridComp, tile))
            {
                if (!physQuery.TryGetComponent(ent, out var body))
                    continue;
                if (body.BodyType != BodyType.Static ||
                    !body.Hard ||
                    (body.CollisionLayer & (int)CollisionGroup.Impassable) == 0)
                    continue;

                valid = false;
                break;
            }
            if (!valid)
                continue;

            var pos = _map.GridTileToLocal(grid, gridComp, tile);
            var mapPos = _transform.ToMapCoordinates(pos);

            // Don't spawn in Anti-Anomaly Zones
            var antiAnomalyZonesQueue = AllEntityQuery<AntiAnomalyZoneComponent, TransformComponent>();
            while (antiAnomalyZonesQueue.MoveNext(out _, out var zone, out var anitXform))
            {
                if (anitXform.MapID != mapPos.MapId)
                    continue; // Not the same map.

                var antiCoordinates = _transform.GetWorldPosition(anitXform);
                var delta = antiCoordinates - mapPos.Position;
                if (delta.LengthSquared() < zone.ZoneRadius * zone.ZoneRadius)
                {
                    valid = false;
                    break;
                }
            }
            if (!valid)
                continue;

            coordinates = pos;
            return true;
        }
        LogManager.GetSawmill(Sawmill).Warning($"Anomaly generator ({ToPrettyString(grid.Owner)}) was unable to find a valid spawn location in {RandomCoordinateAttempts} attempts.");
        return false; // No valid point found.
    }
    #endregion

    #region Lifecycle
    /// <summary>
    /// Starts up the anomaly generator applied necessary components and playing sound effects.
    /// </summary>
    private void StartAnomalyGenerator(Entity<AnomalyGeneratorComponent> generator)
    {
        if (!CanGenerateAnomaly(generator))
            return;

        var generatingComp = EnsureComp<GeneratingAnomalyGeneratorComponent>(generator.Owner);
        generatingComp.EndTime = _time.CurTime + generator.Comp.GenerationLength;
        generatingComp.AudioStream = _audio.PlayPvs(generator.Comp.GeneratingSound, generator.Owner, AudioParams.Default.WithLoop(true))?.Entity;
        generator.Comp.CooldownEndTime = _time.CurTime + generator.Comp.CooldownLength;
        _appearance.SetData(generator.Owner, AnomalyGeneratorVisuals.Generating, true);
    }

    /// <summary>
    /// Actually runs all the generation code and effects. Taken pretty much wholesale from AnomalySystem.Generator.
    /// </summary>
    private void FinishAnomalyGenerator(Entity<AnomalyGeneratorComponent> generator)
    {
        if (!TryGenerateAnomaly(generator))
            return; // Should probably do *something* if it fails to generate...

        RemComp<GeneratingAnomalyGeneratorComponent>(generator.Owner);
        _appearance.SetData(generator.Owner, AnomalyGeneratorVisuals.Generating, false);
        _audio.PlayPvs(generator.Comp.GeneratingFinishedSound, generator.Owner);

        var message = Loc.GetString("anomaly-generator-announcement");
        _radio.SendRadioMessage(generator.Owner, message, _prototype.Index<RadioChannelPrototype>(generator.Comp.ScienceChannel), generator.Owner);
    }

    #endregion

    /// <summary>
    /// Attempts to retrieve the anomaly capsule from the item slot.
    /// </summary>
    private bool TryGetAnomalyCapsule(Entity<AnomalyGeneratorComponent> generator, out Entity<AnomalyCapsuleComponent> capsule)
    {
        capsule = default!;

        if (!_slots.TryGetSlot(generator.Owner, generator.Comp.CapsuleContainer, out var slot) ||
            slot.Item is not { } capsuleUid ||
            !TryComp<AnomalyCapsuleComponent>(capsuleUid, out var capsuleComp))
            return false;

        capsule = (capsuleUid, capsuleComp);
        return true;
    }
}