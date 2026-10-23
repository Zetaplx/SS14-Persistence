using System.Linq;
using Content.Server.GameTicking;
using Robust.Server.GameObjects;

namespace Content.Server._Persistence14.Singleton;

public sealed partial class SingletonManager : EntitySystem
{
    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private MapSystem _map = default!;

    /// <summary>
    ///     Registers an instance of the component as a singleton
    /// </summary>
    public TComp Register<TComp>(TComp? instance = null) where TComp : Component, new()
    {
        var singletonEnt = GetSingletonEntity();

        if (HasComp<TComp>(singletonEnt))
            RemComp<TComp>(singletonEnt);

        if (instance is null)
            return AddComp<TComp>(singletonEnt);

        AddComp(singletonEnt, instance);
        return instance;
    }

    /// <summary>
    ///     Attempts to retrieve a singleton component.
    /// </summary>
    public new bool TryGet<TComp>(out TComp component) where TComp : Component
    {
        var singletonEnt = GetSingletonEntity();

        return TryComp(singletonEnt, out component!);
    }

    /// <summary>
    ///     Retrieves a singleton component, creating a new component if none exists.
    /// </summary>
    public TComp Ensure<TComp>() where TComp : Component, new()
    {
        if (TryGet<TComp>(out var singleton))
            return singleton;

        var singletonEnt = GetSingletonEntity();
        var comp = AddComp<TComp>(singletonEnt);

        return comp;
    }

    /// <summary>
    ///     Retrieves the entity on which all singleton components exist.
    /// </summary>
    public EntityUid GetSingletonEntity()
    {
        var singletonEnt = _map.GetMap(_gameTicker.DefaultMap);

        if (!Exists(singletonEnt))
            throw new InvalidOperationException($"Cannot create singleton: default map does not exist!");

        return singletonEnt;
    }

    /// <summary>
    ///     Discovers all singleton components marked with the SingletonComponentAttribute within the Content.Server assembly.
    /// </summary>
    public IEnumerable<Type> DiscoverSingletons()
    {
        return typeof(SingletonManager).Assembly.GetTypes().Where(
            type =>
                !type.IsAbstract && // Abstract singletons make no sense...
                typeof(Component).IsAssignableFrom(type) && // Singletons must contain component data
                type.IsDefined(typeof(SingletonComponentAttribute), false)  // Singeltons marked with the SingletonComponentAttribute.
        );
    }
}

/// <summary>
///     Identifies a component as intended to be used with the SingletonManager and be accessible through singleton registration.
///     Such components are automatically added to _gameTicker.DefaultMap on StartRound.<br/><br/>
/// 
///     These components must exist in Content.Server. Shared component singletons must be manually registered using Ensure or Register.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed partial class SingletonComponentAttribute : Attribute;