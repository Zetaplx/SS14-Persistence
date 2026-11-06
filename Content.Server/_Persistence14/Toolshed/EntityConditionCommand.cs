using Content.Server.Administration;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;

namespace Content.Server._Persistence14.Toolshed;

[ToolshedCommand(Name = "entitycondition"), AdminCommand(Shared.Administration.AdminFlags.Debug)]
public sealed partial class EntityConditionCommands : ToolshedCommand
{
    private SharedEntityConditionsSystem? _conditions;

    [CommandImplementation("where")]
    public IEnumerable<EntityUid> Where(
        [PipedArgument] IEnumerable<EntityUid> targets,
        [CommandArgument] ProtoId<EntityConditionPrototype> condition)
    {
        _conditions ??= GetSys<SharedEntityConditionsSystem>();

        foreach (var target in targets)
        {
            if (_conditions.TryCondition(target, condition))
                yield return target;
        }
    }

    [CommandImplementation("wherenot")]
    public IEnumerable<EntityUid> WhereNot(
        [PipedArgument] IEnumerable<EntityUid> targets,
        [CommandArgument] ProtoId<EntityConditionPrototype> condition)
    {
        _conditions ??= GetSys<SharedEntityConditionsSystem>();

        foreach (var target in targets)
        {
            if (!_conditions.TryCondition(target, condition))
                yield return target;
        }
    }
}