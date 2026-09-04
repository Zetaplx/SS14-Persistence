using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Persistence14.Construction;

[NetSerializable, Serializable]
public sealed partial class ConstructionDoAfterEvent : SimpleDoAfterEvent
{
    public required EntityEventArgs? TriggerArgs;
    public required int EdgeIndex;
    public required int StepIndex;
}

[ByRefEvent, NetSerializable, Serializable]
public sealed partial class ConstructionStartEvent : EntityEventArgs
{
    public required EntityUid User;
}