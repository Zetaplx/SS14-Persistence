using Content.Shared.Administration;
using Content.Shared.SSDIndicator;
using Robust.Shared.Console;

namespace Content.Server._Persistence14.Commands;

[AnyCommand]
public sealed partial class SSDCommand : LocalizedEntityCommands
{
#pragma warning disable IDE1006 // Naming Styles
    [Dependency] private SSDIndicatorSystem _SSDSystem = default!;
#pragma warning restore IDE1006 // Naming Styles

    public override string Command => "ssd";
    public override string Help => "ssd";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;
        if (player != null && player.AttachedEntity.HasValue)
            _SSDSystem.ToggleManualSSD(player.AttachedEntity.Value);
        else
            shell.WriteError(LocalizationManager.GetString("shell-target-player-does-not-exist"));
    }
}
