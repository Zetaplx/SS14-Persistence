using Content.Shared._Persistence14.Dependencies;
using Content.Shared.Popups;

namespace Content.Shared._Persistence14.Allergy.Reactions;

public sealed partial class AllergicReactionPopupSelector : AllergicReactionSelector
{
    [DataField("message", required: true)]
    private string _message = "";

    [DataField("popupType")]
    private PopupType _popupType = PopupType.Small;

    [DataField("private")]
    private bool _privatePopup = true;

    public override void React(ContextDependencies dependencies, AllergyContext ctx)
    {
        var popup = dependencies.Ensure<SharedPopupSystem>();
        var loc = dependencies.Ensure<ILocalizationManager>();

        if (!loc.TryGetString(_message, out var msg))
            msg = _message;

        if (_privatePopup)
            popup.PopupEntity(msg, ctx.AllergicEntityUid, ctx.AllergicEntityUid, _popupType);
        else
            popup.PopupEntity(msg, ctx.AllergicEntityUid, _popupType);
    }
}