using Content.Server._Persistence14.Singleton;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    [Dependency] private SingletonManager _singleton = default!;

    public void LoadSingletons()
    {
        var ent = _singleton.GetSingletonEntity();
        var singletonTypes = _singleton.DiscoverSingletons();

        foreach (var type in singletonTypes)
        {
            if (HasComp(ent, type))
                continue;

            var component = (Component)Activator.CreateInstance(type)!;
            AddComp(ent, component);
        }
    }
}