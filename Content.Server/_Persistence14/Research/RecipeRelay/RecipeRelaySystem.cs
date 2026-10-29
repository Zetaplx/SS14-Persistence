using Content.Server._Persistence14.GameTicking;
using Content.Shared._Persistence14.Research.RecipeRelay;

namespace Content.Server._Persistence14.Research.RecipeRelay;

public sealed partial class RecipeRelaySystem : SharedRecipeRelaySystem
{
    [SubscribeLocalEvent]
    private void OnPostInit(ref PostInitEvent args)
    {
        var sourceQuery = EntityQueryEnumerator<RecipeRelaySourceComponent>();
        while (sourceQuery.MoveNext(out var uid, out var source))
        {
            EnsureSourceIntegrity((uid, source), false);
        }

        var receiverQuery = EntityQueryEnumerator<RecipeRelayReceiverComponent>();
        while (receiverQuery.MoveNext(out var uid, out var receiver))
        {
            EnsureReceiverIntegrity((uid, receiver));
        }
    }
}