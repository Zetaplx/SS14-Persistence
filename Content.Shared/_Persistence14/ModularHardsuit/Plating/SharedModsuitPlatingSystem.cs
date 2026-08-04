using Content.Shared._Persistence14.ModuleHardsuit.Plating;
using Robust.Shared.Containers;

namespace Content.Shared._Persistence14.ModuleHardsuit;

public sealed partial class SharedModsuitPlatingSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    public List<Entity<ModsuitPlatingComponent>> GetPlating(Entity<ModsuitPlatingManagerComponent> plateManager)
    {
        var plating = new List<Entity<ModsuitPlatingComponent>>();

        foreach (var item in _containerSystem.GetContainer(plateManager.Owner, plateManager.Comp.PlatingContainer).ContainedEntities)
        {
            if (!TryComp<ModsuitPlatingComponent>(item, out var platingComp)) 
                LogManager.GetSawmill("modsuit-plating").Error("Item found in container that is not a Modsuit Plating.");
        }
    }
}