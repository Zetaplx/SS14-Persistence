using System.Linq;
using Content.Server.Construction;
using Content.Shared._Persistence14.Construction;
using Content.Shared.Construction;
using JetBrains.Annotations;
using Robust.Server.Containers;

namespace Content.Server._Persistence14.Construction.Completions;

/// <summary>
/// Sets the pathfinding target for the construction node based on an entity stored within a container with the <see cref="ConstructionTargetSpecifierComponent"/>
/// </summary>
[UsedImplicitly, DataDefinition]
public sealed partial class SpecifyTarget : IGraphAction
{
    /// <summary>
    /// The container id holding the required <see cref="ConstructionTargetSpecifierComponent"/>.
    /// </summary>
    [DataField(required: true)]
    public string Store;

    /// <inheritdoc/>
    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        var containerSystem = entityManager.System<ContainerSystem>();
        var constructionSystem = entityManager.System<ConstructionSystem>();

        if (!containerSystem.TryGetContainer(uid, Store, out var container))
            return;

        var item = container.ContainedEntities.First();
        if (!entityManager.TryGetComponent(item, out ConstructionTargetSpecifierComponent? specifierComp))
            return;

        constructionSystem.SetPathfindingTarget(uid, specifierComp.CurrentTarget.TargetNode);
    }
}