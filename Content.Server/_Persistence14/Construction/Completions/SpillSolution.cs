using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Construction;
using Content.Shared.Fluids;
using JetBrains.Annotations;

namespace Content.Server._Persistence14.Construction.Completions;

[UsedImplicitly]
[DataDefinition]
public sealed partial class SpillSolution : IGraphAction
{
    [DataField(required: true)]
    public string Solution { get; set; } = default!;

    [DataField]
    public bool UseSound { get; set; } = true;

    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        var puddleSystem = entityManager.System<SharedPuddleSystem>();
        var solutionSystem = entityManager.System<SharedSolutionContainerSystem>();

        if (!solutionSystem.TryGetSolution(uid, Solution, out _, out var solution) ||
            !entityManager.TryGetComponent(uid, out TransformComponent? transform))
            return;

        puddleSystem.TrySpillAt(transform.Coordinates, solution, out _, UseSound);
    }
}