using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Tools;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Construction.Graph.Steps;

public sealed partial class ToolUse : ConstructionGraphStep
{
    [DataField(required: true)] private ProtoId<ToolQualityPrototype> _tool;
    [DataField] private float _fuel = 0f;


    public override bool CanBeginStep(Entity<ConstructionComponent> construction, in ConstructionStepContext ctx, out EntityUid user)
    {
        user = default!;
        if (ctx.TriggerArgs is not InteractUsingEvent interact)
            return false;
        user = interact.User;

        if (!ctx.EntityManager.TryGetComponent<ToolComponent>(interact.Used, out var tool))
            return false;

        var toolSystem = ctx.EntityManager.System<SharedToolSystem>();
        if (!toolSystem.CanStartToolUse(interact.Used, interact.User, construction.Owner, _fuel, [_tool], tool))
            return false;

        // Scale doafter by tool speed
        DoAfterMultiplier = 1f / tool.SpeedModifier;
        return true;
    }

    public override void OnCompleteStep(Entity<ConstructionComponent> construction, in ConstructionStepContext ctx)
    {
        if (ctx.TriggerArgs is not InteractUsingEvent interact)
            return;

        if (!ctx.EntityManager.TryGetComponent<ToolComponent>(interact.Used, out var tool))
            return;

        // Play sound:
        var toolSystem = ctx.EntityManager.System<SharedToolSystem>();
        toolSystem.PlayToolSound(interact.Used, tool, interact.User);

        // Deplete welder solution
        if (!ctx.EntityManager.TryGetComponent<WelderComponent>(interact.Used, out var welder))
            return;

        var solutionContainerSystem = ctx.EntityManager.System<SharedSolutionContainerSystem>();
        if (!solutionContainerSystem.TryGetSolution(interact.Used, welder.FuelSolutionName, out var solution))
            return;

        solutionContainerSystem.RemoveReagent(solution.Value, welder.FuelReagent, _fuel);
    }
}