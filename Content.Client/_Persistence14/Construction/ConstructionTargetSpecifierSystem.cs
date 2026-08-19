using Content.Shared._Persistence14.Construction;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Verbs;

namespace Content.Client._Persistence14.Construction;

public sealed partial class ConstructionTargetSpecifierSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ConstructionTargetSpecifierComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<ConstructionTargetSpecifierComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<ConstructionTargetSpecifierComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<ConstructionTargetSpecifierComponent, ExaminedEvent>(OnExamine);
    }

    private void OnUseInHand(Entity<ConstructionTargetSpecifierComponent> entity, ref UseInHandEvent args)
    {
        if (args.Handled || entity.Comp.ValidTargets.Length <= 1)
            return;

        CycleTarget(entity, 1);
        args.Handled = true;
    }

    private void OnGetVerbs(Entity<ConstructionTargetSpecifierComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        for (var i = 0; i < entity.Comp.ValidTargets.Length; i++)
        {
            var (targetNode, loc) = entity.Comp.ValidTargets[i];

            var verb = new Verb
            {
                Text = Loc.GetString(loc),
                Act = () => SetTarget(entity, i),
            };

            args.Verbs.Add(verb);
        }
    }

    private void OnGetAltVerbs(Entity<ConstructionTargetSpecifierComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract!)
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Category = VerbCategory.SetTarget,
            Text = Loc.GetString("construction-target-specifier-reverse"),
            Priority = 10,
            Act = () => CycleTarget(entity, -1)
        });
    }

    private void OnExamine(Entity<ConstructionTargetSpecifierComponent> entity, ref ExaminedEvent args)
    {
        var (id, loc) = entity.Comp.ValidTargets[entity.Comp.CurrentTargetIndex];

        args.PushMarkup(Loc.GetString("construction-target-specifier-current-target", ("target", Loc.GetString(loc))));
    }

    private void SetTarget(Entity<ConstructionTargetSpecifierComponent> entity, int index)
    {
        var newIndex = index;
        var length = entity.Comp.ValidTargets.Length;
        newIndex = ((newIndex % length) + length) % length; // Simple wrapparound modulus.
        entity.Comp.CurrentTargetIndex = newIndex;
        Dirty(entity);
    }

    private void CycleTarget(Entity<ConstructionTargetSpecifierComponent> entity, int count = 1) => SetTarget(entity, entity.Comp.CurrentTargetIndex + count);
}