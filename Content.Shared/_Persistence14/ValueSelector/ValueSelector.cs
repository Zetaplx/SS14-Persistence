namespace Content.Shared._Persistence14.ValueSelector;

[ImplicitDataDefinitionForInheritors]
public abstract partial class ValueSelector<T>
{
    [DataField("resolvedValue", readOnly: true)]
    private T _value = default!;

    [DataField(readOnly: true)]
    private bool _resolved = false;

    public T Value
    {
        get
        {
            if (!_resolved)
            {
                _value = Resolve();
                _resolved = true;
            }

            return _value!;
        }
    }

    protected abstract T Resolve();
}