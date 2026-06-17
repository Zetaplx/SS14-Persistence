using System.Diagnostics.CodeAnalysis;

namespace Content.Shared._Persistence14.Dependencies;

public sealed partial class ContextDependencies
{
    private readonly Dictionary<Type, object> _dependencies = new();

    public T Ensure<T>() where T : notnull
    {
        var type = typeof(T);
        if (_dependencies.TryGetValue(type, out var value) && value is T dependency)
            return dependency;

        dependency = IoCManager.Resolve<T>();
        _dependencies[type] = dependency;
        return dependency;
    }

    public bool TryGet<T>([NotNullWhen(true)] out T? dependency) where T : notnull
    {
        var type = typeof(T);
        dependency = default!;
        if (_dependencies.TryGetValue(type, out var value) && value is T t)
        {
            dependency = t;
            return true;
        }

        return false;
    }
}