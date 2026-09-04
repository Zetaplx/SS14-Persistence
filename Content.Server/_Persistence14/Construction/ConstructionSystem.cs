using Content.Shared._Persistence14.Construction;
using Content.Shared._Persistence14.Construction.Graph;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Server._Persistence14.Construction;

public sealed partial class ConstructionSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _protoMan = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ConstructionComponent, ConstructionDoAfterEvent>(OnStepDoAfterReceived);

        InitializeSteps();
    }

    public bool TryConstructionStep(Entity<ConstructionComponent?> construction, EntityEventArgs triggerArgs)
    {
        if (!Resolve(construction, ref construction.Comp) ||
            !TryGetNode(construction, out var node))
            return false;
        var ent = (construction, construction.Comp);

        var stepctx = new ConstructionStepContext
        {
            TriggerArgs = triggerArgs,

            EntityManager = EntityManager,
            PrototypeManager = _protoMan,
        };

        var conditionctx = new ConstructionConditionContext
        {
            EntityManager = EntityManager,
            PrototypeManager = _protoMan
        };

        int edgeIndex = 0;
        foreach (var edge in node.Edges)
        {
            if (!edge.VerifyConditions(ent, conditionctx))
            {
                edgeIndex++;
                continue;
            }

            var stepIndex = 0;
            if (construction.Comp.CurrentEdgeSteps.TryGetValue(edgeIndex, out var storedStepIndex))
                stepIndex = storedStepIndex;

            var step = edge.Steps[stepIndex];
            if (step.CanBeginStep(ent, stepctx, out var user))
            {
                DoStep(ent, step, edgeIndex, stepIndex, stepctx, user);
                return true;
            }

            edgeIndex++;
        }

        return false;
    }

    private void DoStep(Entity<ConstructionComponent> construction, ConstructionGraphStep step, int edgeIndex, int stepIndex, ConstructionStepContext ctx, EntityUid user)
    {
        if (step.DoAfter <= TimeSpan.Zero)
            EnactStep(construction, step, edgeIndex, stepIndex, ctx);
        else
            StartStepDoAfter(construction, step, edgeIndex, stepIndex, ctx, user);
    }

    private void OnStepDoAfterReceived(Entity<ConstructionComponent> construction, ref ConstructionDoAfterEvent args)
    {
        if (!TryGetNode(construction.AsNullable(), out var node))
            return;

        var ctx = new ConstructionStepContext
        {
            TriggerArgs = args.TriggerArgs,
            EntityManager = EntityManager,
            PrototypeManager = _protoMan
        };

        var step = node.Edges[args.EdgeIndex].Steps[args.StepIndex];

        EnactStep(construction, step, args.EdgeIndex, args.StepIndex, ctx);
    }

    private void EnactStep(Entity<ConstructionComponent> construction, ConstructionGraphStep step, int edgeindex, int stepIndex, ConstructionStepContext ctx)
    {
        if (!TryGetNode(construction.AsNullable(), out var node))
            return;

        var edge = node.Edges[edgeindex];
        var conditionctx = new ConstructionConditionContext
        {
            EntityManager = EntityManager,
            PrototypeManager = _protoMan
        };

        if (!edge.VerifyConditions(construction, conditionctx) ||
            !step.CanBeginStep(construction, ctx, out var user))
            return;

        step.OnCompleteStep(construction, ctx);


        construction.Comp.CurrentEdgeSteps.Clear();
        if (stepIndex + 1 >= node.Edges[edgeindex].Steps.Length)
        {
            // TODO: Edge completion logic
        }
        else
        {
            construction.Comp.CurrentEdgeSteps[edgeindex] = stepIndex + 1;
        }
    }

    private void StartStepDoAfter(Entity<ConstructionComponent> construction, ConstructionGraphStep step, int edgeIndex, int stepIndex, ConstructionStepContext ctx, EntityUid user)
    {
        var constructionDoAfterArgs = new ConstructionDoAfterEvent
        {
            TriggerArgs = ctx.TriggerArgs,
            EdgeIndex = edgeIndex,
            StepIndex = stepIndex,
        };

        var doafter = new DoAfterArgs(
            EntityManager,
            user,
            TimeSpan.FromSeconds(step.DoAfter.TotalSeconds * step.DoAfterMultiplier + step.DoAfterAddifier),
            constructionDoAfterArgs,
            construction.Owner);

        _doAfter.TryStartDoAfter(doafter);
    }
}