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

        if (!EntityManager.TryGetComponent<LinkedEntityComponent>(uids[0], out var comp1) ||
            !EntityManager.TryGetComponent<LinkedEntityComponent>(uids[1], out var comp2))
        {
            shell.WriteError($"Unable to link entities without the LinkedEntityComponent.");
            return;
        }
    }
}