using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Construction;
using Content.Shared.Fluids;
using JetBrains.Annotations;

namespace Content.Server._Persistence14.Construction.Completions;

[UsedImplicitly]
[DataDefinition]
public sealed partial class SpillAllSolutions : IGraphAction
{
    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        var puddleSystem = entityManager.System<SharedPuddleSystem>();
        var solutionSystem = entityManager.System<SharedSolutionContainerSystem>();

        if (!entityManager.TryGetComponent(uid, out TransformComponent? transform))
            return;

        bool first = true;
        foreach (var (_, solution) in solutionSystem.EnumerateSolutions(uid, true))
        {
            puddleSystem.TrySpillAt(transform.Coordinates, solution.Comp.Solution, out _, first);
            first = false;
        }
    }
}