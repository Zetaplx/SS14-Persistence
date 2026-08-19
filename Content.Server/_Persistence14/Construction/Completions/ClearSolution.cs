using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Construction;
using JetBrains.Annotations;

namespace Content.Server._Persistence14.Construction.Completions;

[UsedImplicitly]
[DataDefinition]
public sealed partial class ClearSolution : IGraphAction
{
    [DataField(required: true)]
    public string Solution { get; set; } = default!;

    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        var solutionSystem = entityManager.System<SharedSolutionContainerSystem>();

        if (!solutionSystem.TryGetSolution(uid, Solution, out var solutionEnt, out _))
            return;

        solutionSystem.RemoveAllSolution(solutionEnt.Value);
    }
}