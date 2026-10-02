using Robust.Shared.Random;

namespace Content.Shared._Persistence14.ValueSelector;

public sealed partial class IntConstSelector : ValueSelector<int>
{
    [DataField("value", required: true)]
    public int SetValue;
    protected override int Resolve() => SetValue;
}

public sealed partial class IntRangeSelector : ValueSelector<int>
{
    [DataField]
    public int Min = 0;

    [DataField]
    public bool MinInclusive = true;

    [DataField(required: true)]
    public int Max;
    [DataField]
    public bool MaxInclusive = true;

    protected override int Resolve()
    {
        var rand = IoCManager.Resolve<IRobustRandom>();

        return rand.Next(Min + (MinInclusive ? 0 : 1), Max + (MaxInclusive ? 1 : 0));
    }
}