using Content.Shared._Persistence14.Research.ResearchTree;
using Robust.Client.UserInterface;

namespace Content.Client._Persistence14.Research.ResearchTree;

public sealed partial class ResearchTreeClientBoundUserInterface : BoundUserInterface
{
    private ResearchTreeWindow? _window = null;

    public ResearchTreeClientBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<ResearchTreeWindow>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not ResearchTreeClientBoundUserInterfaceState cast ||
            _window is not { })
            return;

        _window.UpdateState(cast);
    }
}