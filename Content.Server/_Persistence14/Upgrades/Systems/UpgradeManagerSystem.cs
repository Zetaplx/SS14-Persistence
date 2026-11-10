using Content.Server.Examine;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Components;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Reflection;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class UpgradeManagerSystem : EntitySystem
{
    [Dependency] private HandsSystem _hands = default!;
    [Dependency] private PopupSystem _popupSystem = default!;
    [Dependency] private IPrototypeManager _protoMan = default!;
    [Dependency] private ExamineSystem _examine = default!;
    [Dependency] private IReflectionManager _reflection = default!;
    [Dependency] private IEntitySystemManager _systemManager = default!;

    private Dictionary<string, BaseUpgradeSystem> _upgradeSystems = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UpgradeableComponent, GetVerbsEvent<ActivationVerb>>(AddUpgradeVerb);
        SubscribeLocalEvent<UpgradeableComponent, GetVerbsEvent<ExamineVerb>>(AddExamineVerb);

        CatalogueUpgradeSystems();
    }

    private void CatalogueUpgradeSystems()
    {
        _upgradeSystems.Clear();
        foreach (var type in _reflection.GetAllChildren<BaseUpgradeSystem>())
        {
            if (type.IsAbstract ||
                !_systemManager.TryGetEntitySystem(type, out var system) ||
                system is not BaseUpgradeSystem upgradeSystem)
                continue;

            _upgradeSystems.Add(type.Name, upgradeSystem);
        }
    }

    private void AddExamineVerb(Entity<UpgradeableComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanAccess)
            return;
        if (!TryComp(args.User, out ActorComponent? actor))
            return;
        FormattedMessage msg = new();
        msg.TryAddMarkup(GetUpgradeableExamineText(ent), out _);
        _examine.AddDetailedExamineVerb(args, ent.Comp, msg, "Upgrades", hoverMessage: "Examine the upgrades.");
    }

    private string GetUpgradeableExamineText(Entity<UpgradeableComponent> ent)
    {
        string msg = "";
        foreach (var kv in ent.Comp.Upgrades)
        {
            var upgrade = kv.Key;
            var appliedTimes = kv.Value;
            var upgradeProto = _protoMan.Index(upgrade);
            var max = 1;
            if (ent.Comp.UpgradeProfiles.TryGetValue(upgrade, out var upgradeProfile))
                max = upgradeProfile.MaxUpgradeCount;
            msg += $"\n\n[bold]{upgradeProto.Name}[/bold]:\n[color=yellow]{upgradeProto.Description}[/color] ({appliedTimes}/{max})";
        }

        return msg;
    }

    private void AddUpgradeVerb(EntityUid uid, UpgradeableComponent comp, GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess)
            return;
        if (!TryComp(args.User, out ActorComponent? actor))
            return;
        if (_hands.GetActiveItem(args.User) is not { } item ||
            !HasComp<UpgradeModuleComponent>(item))
            return; // Don't show "UPGRADE" if I'm not holding an upgrade... TODO: Only show if upgrade is *compatible*

        var upgradeVerb = new ActivationVerb
        {
            Text = Loc.GetString(comp.Verb),
            Icon = comp.VerbImage,
            Act = () =>
            {
                OnUpgrade(uid, comp, item, args.User);
            },
            Impact = LogImpact.Low,

        };
        upgradeVerb.Impact = LogImpact.Low;
        args.Verbs.Add(upgradeVerb);
    }

    private void OnUpgrade(EntityUid uid, UpgradeableComponent component, EntityUid upgradeUid, EntityUid user)
    {
        if (!TryComp(user, out ActorComponent? actor))
            return;

        if (!TryComp<UpgradeModuleComponent>(upgradeUid, out var upgradeComp))
        {
            _popupSystem.PopupEntity("You must be holding a valid upgrade module!", uid, actor.PlayerSession, PopupType.Large);
            return;
        }

        if (!TryApplyUpgrade(uid, component, upgradeComp, user))
        {
            _popupSystem.PopupEntity("Upgrade failed! Invalid module or incompatible upgrade.", uid, actor.PlayerSession, PopupType.Large);
            return;
        }

        _popupSystem.PopupEntity("Upgrade applied successfully!", uid, actor.PlayerSession, PopupType.Large);
        QueueDel(upgradeUid);
    }


    private bool TryApplyUpgrade(EntityUid uid, UpgradeableComponent comp, UpgradeModuleComponent upgradeModuleComp, EntityUid user)
    {
        bool any = false;
        foreach (var upgradeProtoId in upgradeModuleComp.Upgrades)
        {
            if (!comp.UpgradeProfiles.TryGetValue(upgradeProtoId, out var upgradeProfile))
                continue; // Unsupported upgrade.

            if (!_upgradeSystems.TryGetValue(upgradeProfile.UpgradeSystem, out var upgradeSystem))
                continue; // Unknown system

            if (!ProtoMan.TryIndex(upgradeProtoId, out var upgradeProto))
                continue; // Invalid proto id

            if (comp.Upgrades.TryGetValue(upgradeProtoId, out var currentCount) &&
                currentCount + 1 > upgradeProfile.MaxUpgradeCount)
                continue; // To many upgrades!

            // Verify compatibility of blacklists and whitelists
            bool whitelistSuccess = true;
            foreach (var whitelist in upgradeProto.Whitelist)
            {
                if (!comp.Upgrades.ContainsKey(whitelist))
                {
                    whitelistSuccess = false;
                    break;
                }
            }
            if (!whitelistSuccess)
                continue; // Whitelist Failure

            bool blacklistSuccess = true;
            foreach (var blacklist in upgradeProto.Blacklist)
            {
                if (comp.Upgrades.ContainsKey(blacklist))
                {
                    blacklistSuccess = false;
                    break;
                }
            }
            if (!blacklistSuccess)
                continue; // Blacklist Failure

            if (!upgradeSystem.TryApplyUpgrade((uid, comp), upgradeProto, user, upgradeProfile.UpgradeMultiplier))
                continue; // Failed to apply upgrade

            any = true;
            comp.Upgrades[upgradeProtoId] = comp.Upgrades.GetValueOrDefault(upgradeProtoId) + 1;
        }

        return any;
    }
}
