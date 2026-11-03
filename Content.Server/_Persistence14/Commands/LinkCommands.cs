using Content.Server.Administration;
using Content.Shared._Persistence14.PersistentIdentifier;
using Content.Shared.Teleportation.Components;
using Content.Shared.Teleportation.Systems;
using Robust.Shared.Console;

namespace Content.Server._Persistence14.Commands;

public abstract partial class LinkCommand : LocalizedEntityCommands
{
    [Dependency] protected PersistentIdentifierSystem Pid = default!;
    [Dependency] protected LinkedEntitySystem Link = default!;
}

/// <summary>
///   Links two entities with the LinkedEntityComponent  
/// </summary>
[AdminCommand(Shared.Administration.AdminFlags.Debug)]
public sealed partial class LinkEntsCommand : LinkCommand
{
    public override string Command => "link";
    public override string Help => "link <EntityUid 1> <EntityUid 2>";
    public override string Description => "Links two entities with the LinkedEntityComponent.";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Help);
            return;
        }

        if (!TryParseArgEnts(args, out var uids, out var errs, 0, 1))
        {
            foreach (var err in errs)
                shell.WriteError(err);

            return;
        }

        if (!EntityManager.HasComponent<LinkedEntityComponent>(uids[0]) ||
            !EntityManager.HasComponent<LinkedEntityComponent>(uids[1]))
        {
            shell.WriteError($"Unable to link entities without the LinkedEntityComponent.");
            return;
        }

        if (!Link.TryLink(uids[0], uids[1]))
        {
            shell.WriteError($"Failed to link entities.");
            return;
        }
    }
}

/// <summary>
///     Links two entities with a LinkedEntityComponent. Adds the component if it does not exist.
/// </summary>
[AdminCommand(Shared.Administration.AdminFlags.Debug)]
public sealed partial class ForceLinkCommand : LinkCommand
{
    public override string Command => "forcelink";
    public override string Help => "forcelink <EntityUid 1> <EntityUid 2>";
    public override string Description => "Links two entities with a LinkedEntityComponent. Adds the component if it does not exist.";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Help);
            return;
        }

        if (!TryParseArgEnts(args, out var uids, out var errs, 0, 1))
        {
            foreach (var err in errs)
                shell.WriteError(err);

            return;
        }

        EntityManager.EnsureComponent<LinkedEntityComponent>(uids[0]);
        EntityManager.EnsureComponent<LinkedEntityComponent>(uids[1]);

        if (!Link.TryLink(uids[0], uids[1]))
        {
            shell.WriteError($"Failed to link entities.");
            return;
        }

        shell.WriteLine($"Entities \"{uids[0]}\" and \"{uids[1]}\" have been linked.");
    }
}

/// <summary>
///     Removes the link between two entities, if it exists.
/// </summary>
[AdminCommand(Shared.Administration.AdminFlags.Debug)]
public sealed partial class UnlinkCommand : LinkCommand
{
    public override string Command => "unlink";
    public override string Help => "unlink <Entity 1> <Entity 2>";
    public override string Description => "Removes the link between two entities, if it exists.";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Help);
            return;
        }

        if (!TryParseArgEnts(args, out var uids, out var errs, 0, 1))
        {
            foreach (var err in errs)
                shell.WriteError(err);

            return;
        }

        if (!Link.TryUnlink(uids[0], uids[1]))
        {
            shell.WriteError($"Unable to unlink entities \"{uids[0]}\" and \"{uids[1]}\"");
            return;
        }

        shell.WriteLine($"Entities \"{uids[0]}\" and \"{uids[1]}\" unlinked.");
    }
}

/// <summary>
///     Severs all links on a particular entity.
/// </summary>
[AdminCommand(Shared.Administration.AdminFlags.Debug)]
public sealed partial class SeverLinkCommand : LinkCommand
{
    public override string Command => "severlink";
    public override string Help => "severlink <Entity>";
    public override string Description => "Severs all links on a particular entity.";


    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Help);
            return;
        }

        if (!TryParseEntity(args[0], out var uid))
        {
            shell.WriteError($"Unable to parse argument: {args[0]}");

            return;
        }

        if (!Link.TrySever(uid))
        {
            shell.WriteError($"Unable to sever entity's links: {args[0]}");
            return;
        }

        shell.WriteLine($"Severed links for entity \"{args[0]}\"");
    }
}