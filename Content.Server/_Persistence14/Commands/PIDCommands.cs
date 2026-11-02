using Content.Shared._Persistence14.PersistentIdentifier;
using Robust.Shared.Console;

namespace Content.Server._Persistence14.Commands;

public abstract partial class PIDCommand : LocalizedEntityCommands
{
    [Dependency] protected PersistentIdentifierSystem Pid = default!;
}


/// <summary>
///     A simple command for getting the PID of a target entity, if it has one.
/// </summary>
public sealed partial class GetPIDCommand : PIDCommand
{
    public override string Command => "pidget";
    public override string Description => "Gets the PID of a given entity (by uid or NetUid).";
    public override string Help => "pidget <EntityUid>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Help);
            return;
        }

        if (TryParseEntity(args[0], out var uid))
        {
            shell.WriteLine($"PID of Entity {uid}: \"{GetEntPID(uid)}\"");
            return;
        }
        else
        {
            shell.WriteError($"Argument must be a valid entity");
            return;
        }
    }

    private string GetEntPID(EntityUid uid)
    {
        if (!EntityManager.TryGetComponent<PersistentIdentifierComponent>(uid, out var comp))
            return "[None]";

        if (comp.IdInit)
            return comp.Id;
        return "[None]";
    }
}

public sealed partial class FindPIDCommand : PIDCommand
{
    public override string Command => "pidfind";
    public override string Description => "Provides the entity with a given PID.";
    public override string Help => "pidfind <PID>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Help);
            return;
        }

        if (!Pid.TryResolveId(args[0], out var ent))
        {
            shell.WriteError($"No entity found with PID \"{args[0]}\".");
            return;
        }

        shell.WriteLine($"Entity with PID \"{args[0]}\": {ent.Owner}");
    }
}

public sealed partial class InitPIDCommand : PIDCommand
{
    public override string Command => "pidinit";
    public override string Description => "Initializes a target entity's PID if not already initialized.";
    public override string Help => "pidinit <Entity>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Help);
            return;
        }

        if (!TryParseEntity(args[0], out var uid))
        {
            shell.WriteError($"Argument must be a valid entity");
            return;
        }

        var id = Pid.EnsureId(uid);
        shell.WriteLine($"Entity \"{uid}\" initialized with PID: \"{id}\".");
    }
}