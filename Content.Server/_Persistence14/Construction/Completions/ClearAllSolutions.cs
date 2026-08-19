using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Construction;
using JetBrains.Annotations;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;

namespace Content.Server._Persistence14.Construction.Completions;

[UsedImplicitly]
[DataDefinition]
public sealed partial class ClearAllSolutions : IGraphAction
{
    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        var solutionSystem = entityManager.System<SharedSolutionContainerSystem>();

        foreach (var (_, solution) in solutionSystem.EnumerateSolutions(uid, true))
        {
            solutionSystem.RemoveAllSolution(solution);
        }
    }
}