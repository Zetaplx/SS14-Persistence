using Content.Shared._Persistence14.Research.TechDisk;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;

namespace Content.Server._Persistence14.Research.TechDisk;

public sealed partial class TechDiskSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnInteractWith(EntityUid uid, TechnologyDiskComponent diskComponent, ref AfterInteractEvent args)
    {
        
    }

    [SubscribeLocalEvent]
    private void OnDoAfterComplete(EntityUid uid, TechnologyDiskComponent diskComponent, ref DoAfterArgs args)
    {
        
    }
}