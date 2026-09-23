using Content.Shared._Persistence14.Research.TechDisk;
using Robust.Client.UserInterface;

namespace Content.Client._Persistence14.Research.TechDisk;

public sealed partial class TechDiskTerminalBoundUserInterface : BoundUserInterface
{
    private TechDiskTerminalWindow? _window = null;

    public TechDiskTerminalBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<TechDiskTerminalWindow>();
        _window.OnAdd += (t) => SendMessage(new TechDiskAddTechMessage(t));
        _window.OnRemove += (t) => SendMessage(new TechDiskRemoveTechMessage(t));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window is not { } window ||
            state is not TechDiskTerminalBUIState buiState)
            return;

        _window.UpdateUIState(buiState);
    }
}