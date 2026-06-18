using Content.Shared._Persistence14.Dependencies;
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Allergy.Reactions;

public sealed partial class AllergicReactionEmoteSelector : AllergicReactionSelector
{
    [DataField("emote", required: true)]
    private ProtoId<EmotePrototype> _emote = default!;

    [DataField("showEmoteMessage")]
    private bool _showEmoteMessage = true;

    public override void React(ContextDependencies dependencies, AllergyContext ctx)
    {
        var chat = dependencies.Ensure<SharedChatSystem>();

        if (_showEmoteMessage)
            chat.TryEmoteWithChat(ctx.AllergicEntityUid, _emote);
        else
            chat.TryEmoteWithoutChat(ctx.AllergicEntityUid, _emote);
    }
}