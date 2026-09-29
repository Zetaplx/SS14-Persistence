using Content.Shared._Persistence14.Research.TechDisk;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client._Persistence14.Research.TechDisk;

public sealed partial class TechDiskTerminalBoundUserInterface : BoundUserInterface
{
    [Dependency] private SpriteSystem _sprite = default!;

    private TechDiskTerminalWindow? _window = null;

    public TechDiskTerminalBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<TechDiskTerminalWindow>();
        _window.OnAdd += (t) => SendMessage(new TechDiskAddTechMessage(t));
        _window.OnRemove += (t) => SendMessage(new TechDiskRemoveTechMessage(t));
        _window.OnPrint += () => SendMessage(new TechDiskPrintMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window is not { } window ||
            state is not TechDiskTerminalBUIState buiState)
            return;

        _window.UpdateUIState(buiState);
    }

    public override void Update()
    {
        base.Update();

        if (_window is not { } window)
            return;

        _window.Update();
    }
    
}