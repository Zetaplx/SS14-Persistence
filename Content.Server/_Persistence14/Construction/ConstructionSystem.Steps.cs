using Content.Shared._Persistence14.Construction;
using Content.Shared.Interaction;

namespace Content.Server._Persistence14.Construction;

public sealed partial class ConstructionSystem
{
    private void InitializeSteps()
    {
        RelayConstructionEvent<InteractUsingEvent>();
    }

    private void RelayConstructionEvent<TEvent>() where TEvent : EntityEventArgs
    {
        SubscribeLocalEvent<ConstructionComponent, TEvent>((uid, comp, args) => TryConstructionStep((uid, comp), args));
    }
}