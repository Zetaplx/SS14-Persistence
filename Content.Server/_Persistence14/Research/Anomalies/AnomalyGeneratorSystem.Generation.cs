using Content.Server.Anomaly.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._Persistence14.RandomTable.State;
using Content.Shared._Persistence14.Research.Anomalies;
using Content.Shared.CCVar;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Persistence14.Research.Anomalies;

public sealed partial class AnomalyGeneratorSystem
{
    private bool CanGenerateAnomaly(Entity<AnomalyGeneratorComponent> generator) => CanGenerateAnomaly(generator, out _);
    private bool CanGenerateAnomaly(Entity<AnomalyGeneratorComponent> generator, out Entity<AnomalyCapsuleComponent> capsule)
    {
        capsule = default!;

        if (!this.IsPowered(generator.Owner, EntityManager))
            return false; // Generator is unpowered

        if (_material.GetMaterialAmount(generator.Owner, generator.Comp.RequiredMaterial) < generator.Comp.MaterialPerAnomaly)
            return false; // Not enough fuel

        if (!TryGetAnomalyCapsule(generator, out capsule))
            return false; // No capsule

        return true;
    }

    private bool CanStartAnomaly(Entity<AnomalyGeneratorComponent> generator)
    {
        if (_time.CurTime < generator.Comp.CooldownEndTime)
            return false; // Still on cooldown.

        if (HasComp<GeneratingAnomalyGeneratorComponent>(generator.Owner))
            return false; // Already started

        return true;
    }

    /// <summary>
    /// Attempts to generate an anomaly using the capsule contained within the generator. The type of anomaly and its location depend on the capsule used.
    /// </summary>
    private bool TryGenerateAnomaly(Entity<AnomalyGeneratorComponent> generator)
    {
        if (!CanGenerateAnomaly(generator, out var capsule))
            return false;
        var tableState = EnsureComp<RandomTableStateComponent>(generator.Owner);
        var ev = new AnomalyGeneratorAttemptEvent
        {
            Context = new AnomalyGenerationContext
            {
                GeneratorUid = generator.Owner,
                Capsule = capsule,
            }
        };
        _capsules.RelayEventToModules(capsule, ref ev);
        RaiseLocalEvent(ref ev);
        if (ev.Cancelled)
            return false;

        if (!_capsules.TryGetAnomalyPrototype(capsule, out var anomalyPrototype, tableState))
            return false;

        if (ev.Context.ForceEnvironmental && ev.Context.ForceInfectious)
            return false;

        var spawnType = AnomalySpawnType.Random;
        if (ev.Context.ForceEnvironmental)
            spawnType = AnomalySpawnType.Environmental;
        if (ev.Context.ForceInfectious)
            spawnType = AnomalySpawnType.Infectious;

        if (!anomalyPrototype.TryGetSpawnableProtoId(_random, out var spawnable, spawnType))
            return false;

        if (ev.Context.TargetCoordinates is not { } coordinates && !TryGetCoordinatesOnEntitysGrid(generator.Owner, out coordinates))
            return false;

        if (!_material.TryChangeMaterialAmount(generator.Owner, generator.Comp.RequiredMaterial, -generator.Comp.MaterialPerAnomaly))
            return false;

        QueueDel(capsule.Owner); // Delete the used capsule
        var spawn = Spawn(spawnable.Id, coordinates); // spawnable is assigned, I promise.
        LogManager.GetSawmill(Sawmill).Info($"An anomaly ({ToPrettyString(spawn)}) was generated at these coordinates: {coordinates}");
        return true;
    }

    /// <summary>
    /// Spawns an anomaly at a random point on a target grid.
    /// </summary>
    public void SpawnAnomalyOnGrid(EntityUid gridUid, EntProtoId anomalyEntityId)
    {
        if (!TryGetCoordinatesOnGrid(gridUid, out var coordinates))
        {
            LogManager.GetSawmill(Sawmill).Warning($"Attempted to manually spawn anomaly but failed to find valid coordinates on grid {ToPrettyString(gridUid)}.");
            return;
        }

        var spawn = Spawn(anomalyEntityId, coordinates);
        LogManager.GetSawmill(Sawmill).Info($"An anomaly ({ToPrettyString(spawn)}) was generated at these coordinates: {coordinates}");
    }
    /// <summary>
    /// Spawns an anomaly at a random point on a target grid.
    /// </summary>
    public void SpawnAnomalyOnGrid(EntityUid gridUid, ProtoId<AnomalyPrototype> anomalyProtoId, AnomalySpawnType type)
    {
        var anomalyProto = _prototype.Index(anomalyProtoId);
        if (!anomalyProto.TryGetSpawnableProtoId(_random, out var spawnable, type))
            return; // No spawnable type on proto. TODO: Send log with error.

        SpawnAnomalyOnGrid(gridUid, spawnable);
    }

    /// <summary>
    /// Spawns an anomaly at a random point on the same grid as the target entity.
    /// </summary>
    public void SpawnAnomalyOnEntityGrid(EntityUid targetEntityUid, EntProtoId anomalyEntityId)
    {
        if (!TryGetCoordinatesOnEntitysGrid(targetEntityUid, out var coordinates))
        {
            LogManager.GetSawmill(Sawmill).Warning($"Attempted to manually spawn anomaly but failed to find valid coordinates on entity {ToPrettyString(targetEntityUid)}'s grid.");
            return;
        }

        var spawn = Spawn(anomalyEntityId, coordinates);
        LogManager.GetSawmill(Sawmill).Info($"An anomaly ({ToPrettyString(spawn)}) was generated at these coordinates: {coordinates}");
    }
    /// <summary>
    /// Spawns an anomaly at a random point on the same grid as the target entity.
    /// </summary>
    public void SpawnAnomalyOnEntityGrid(EntityUid targetEntityUid, ProtoId<AnomalyPrototype> anomalyProtoId, AnomalySpawnType type)
    {
        var anomalyProto = _prototype.Index(anomalyProtoId);
        if (!anomalyProto.TryGetSpawnableProtoId(_random, out var spawnable, type))
            return; // No spawnable type on proto. TODO: Send log with error.

        SpawnAnomalyOnEntityGrid(targetEntityUid, spawnable);
    }

    /// <summary>
    /// Spawns an anomaly at a specific set of coordinates.
    /// </summary>
    public void SpawnAnomalyAtCoordinates(EntityCoordinates coordinates, EntProtoId anomalyEntityid)
    {
        var spawn = Spawn(anomalyEntityid, coordinates);
        LogManager.GetSawmill(Sawmill).Info($"An anomaly ({ToPrettyString(spawn)}) was generated at these coordinates: {coordinates}");
    }
    /// <summary>
    /// Spawns an anomaly at a specific set of coordinates.
    /// </summary>
    public void SpawnAnomalyAtCoordinates(EntityCoordinates coordinates, ProtoId<AnomalyPrototype> anomalyProtoId, AnomalySpawnType type)
    {
        var anomalyProto = _prototype.Index(anomalyProtoId);
        if (!anomalyProto.TryGetSpawnableProtoId(_random, out var spawnable, type))
            return; // No spawnable type on proto. TODO: Send log with error.

        SpawnAnomalyAtCoordinates(coordinates, spawnable);
    }

    /// <summary>
    /// Attempts to get a random set of coordinates from the grid containing the target entity.
    /// </summary>
    private bool TryGetCoordinatesOnEntitysGrid(EntityUid targetUid, out EntityCoordinates coordinates)
    {
        coordinates = default!;
        var xform = Transform(targetUid);

        if (xform.GridUid is not { } gridUid) // Generator isn't on a grid. For some reason.
            return false;

        return TryGetCoordinatesOnGrid(gridUid, out coordinates);
    }

    /// <summary>
    /// Attempts to get a random set of coordinates from a specific grid entity.
    /// </summary>
    private bool TryGetCoordinatesOnGrid(EntityUid gridUid, out EntityCoordinates coordinates)
    {
        coordinates = default!;
        if (!TryComp<MapGridComponent>(gridUid, out var gridComp))
            return false;

        Entity<MapGridComponent> grid = (gridUid, gridComp);
        var gridBounds = gridComp.LocalAABB.Scale(_configuration.GetCVar(CCVars.AnomalyGenerationGridBoundsScale));
        var xform = Transform(grid.Owner);

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
}