using Content.Server.Administration;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;

namespace Content.Server._Persistence14.Toolshed;

[ToolshedCommand(Name = "entityeffect"), AdminCommand(Shared.Administration.AdminFlags.Debug)]
public sealed partial class EntityEffectCommand : ToolshedCommand
{
    private SharedEntityEffectsSystem? _effects;

    [CommandImplementation("prototype")]
    public EntityUid ApplyPrototype(
        [PipedArgument] EntityUid target,
        [CommandArgument] ProtoId<EntityEffectPrototype> effect,
        float scale = 1f)
    {
        _effects ??= GetSys<SharedEntityEffectsSystem>();

        _effects.TryApplyEffect(target, effect, scale);
        return target;
    }

    [CommandImplementation("prototype")]
    public IEnumerable<EntityUid> ApplyPrototype(
        [PipedArgument] IEnumerable<EntityUid> targets,
        [CommandArgument] ProtoId<EntityEffectPrototype> effect,
        float scale = 1f)
    {
        _effects ??= GetSys<SharedEntityEffectsSystem>();

        foreach (var target in targets)
        {
            _effects.TryApplyEffect(target, effect, scale);
            yield return target;
        }
    }
}