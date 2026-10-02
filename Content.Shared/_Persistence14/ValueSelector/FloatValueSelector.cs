using Robust.Shared.Random;

namespace Content.Shared._Persistence14.ValueSelector;

public sealed partial class FloatConstSelector : ValueSelector<float>
{
    [DataField("value", required: true)]
    public float SetValue;
    protected override float Resolve() => SetValue;
}

public sealed partial class FloatRangeSelector : ValueSelector<float>
{
    [DataField]
    public int Min = 0;

    [DataField]
    public bool MinInclusive = true;

    [DataField(required: true)]
    public int Max;
    [DataField]
    public bool MaxInclusive = true;

    protected override float Resolve()
    {
        var rand = IoCManager.Resolve<IRobustRandom>();

        return rand.NextFloat(Min + (MinInclusive ? 0 : 1), Max + (MaxInclusive ? 1 : 0));
    }
}