using Content.Shared._Persistence14.PersistentIdentifier;
using Content.Shared._Persistence14.PersistentIdentifier.Reference;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Persistence14.Research.TechDisk;

[RegisterComponent]
public sealed partial class TechDiskTerminalPrintingComponent : Component
{
    [DataField(readOnly: true, customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan PrintStartTime = TimeSpan.Zero;

    [DataField(readOnly: true, customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan PrintEndTime = TimeSpan.Zero;
}