namespace Content.Server._Persistence14.Virology;

public sealed partial class VirologySystem : EntitySystem
{
    private static VirologyManagerComponent? _virologyManager = null;

    public VirologyManagerComponent GetManager()
    {
        if (_virologyManager is { } manager)
            return manager;
    }
}