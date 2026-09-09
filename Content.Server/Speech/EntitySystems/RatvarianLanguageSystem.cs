using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.Speech.EntitySystems;
using System.Text;
using System.Text.RegularExpressions;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class RatvarianLanguageSystem : SharedRatvarianLanguageSystem
{
    public override void DoRatvarian(EntityUid uid, TimeSpan time, bool refresh)
    {
        if (refresh)
            Status.TryUpdateStatusEffectDuration(uid, Ratvarian, time);
        else
            Status.TryAddStatusEffectDuration(uid, Ratvarian, time);
    }
}
