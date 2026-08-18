using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

[Serializable, NetSerializable]
public sealed partial class ConstructionInteractDoAfterEvent : DoAfterEvent
{
    [DataField("clickLocation")]
    public NetCoordinates ClickLocation;

    private ConstructionInteractDoAfterEvent()
    {
    }

    public ConstructionInteractDoAfterEvent(IEntityManager entManager, InteractUsingEvent ev)
    {
        ClickLocation = entManager.GetNetCoordinates(ev.ClickLocation);
    }

    public override DoAfterEvent Clone() => this;
}
