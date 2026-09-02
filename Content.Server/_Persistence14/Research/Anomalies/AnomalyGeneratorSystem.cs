using System.Linq;
using Content.Server.Anomaly.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Audio;
using Content.Server.Materials;
using Content.Server.Radio.EntitySystems;
using Content.Shared._Persistence14.RandomTable;
using Content.Shared._Persistence14.Research.Anomalies;
using Content.Shared._Persistence14.Research.Anomalies.Modules;
using Content.Shared.Anomaly;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Materials;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Radio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Persistence14.Research.Anomalies;

public sealed partial class AnomalyGeneratorSystem : SharedAnomalyGeneratorSystem
{
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedAnomalyCapsuleSystem _capsules = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly MaterialStorageSystem _material = default!;
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly RandomTableSystem _randomTable = default!;
    private const int RandomCoordinateAttempts = 25;
    private const string Sawmill = "anomaly-generator";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnomalyGeneratorComponent, GenerateAnomalyEvent>(OnGenerateAnomaly);
        SubscribeLocalEvent<AnomalyGeneratorComponent, UpdateAnomalyGeneratorUIEvent>(OnUpdateUIEvent);
        SubscribeLocalEvent<AnomalyGeneratorComponent, BoundUIOpenedEvent>(OnBUIOpen);
        SubscribeLocalEvent<AnomalyGeneratorComponent, MaterialAmountChangedEvent>(OnMaterialQtyChange);
        SubscribeLocalEvent<AnomalyGeneratorComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<AnomalyGeneratorComponent, ItemSlotInsertAttemptEvent>(OnCapsuleInsertAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GeneratingAnomalyGeneratorComponent, AnomalyGeneratorComponent>();
        while (query.MoveNext(out var ent, out var active, out var gen))
        {
            if (_time.CurTime < active.EndTime)
                continue;

            active.AudioStream = _audio.Stop(active.AudioStream);
            FinishAnomalyGenerator((ent, gen));
        }
    }

    #region Event Hooks
    private void OnGenerateAnomaly(Entity<AnomalyGeneratorComponent> generator, ref GenerateAnomalyEvent args)
    {
        StartAnomalyGenerator(generator);
    }

    private void OnBUIOpen(Entity<AnomalyGeneratorComponent> generator, ref BoundUIOpenedEvent args)
    {
        UpdateGeneratorUi(generator);
    }

    private void OnMaterialQtyChange(Entity<AnomalyGeneratorComponent> generator, ref MaterialAmountChangedEvent args)
    {
        UpdateGeneratorUi(generator);
    }

    private void OnPowerChanged(Entity<AnomalyGeneratorComponent> generator, ref PowerChangedEvent args)
    {
        _ambient.SetAmbience(generator.Owner, args.Powered);
        if (args.Powered)
            return;

        CancelAnomalyGenerator(generator);
    }

    private void OnUpdateUIEvent(Entity<AnomalyGeneratorComponent> generator, ref UpdateAnomalyGeneratorUIEvent args) => UpdateGeneratorUi(generator);

    private void OnCapsuleInsertAttempt(Entity<AnomalyGeneratorComponent> generator, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Slot.ID != generator.Comp.CapsuleContainer)
            return;

        if (!TryComp<AnomalyCapsuleComponent>(args.Item, out var capsuleComp))
            return; // Should be caught by the whitelist...

        Entity<AnomalyCapsuleComponent> capsule = (args.Item, capsuleComp);

        if (_capsules.HasCore(capsule))
            return; // Insert as normal

        args.Cancelled = true;

        if (args.User is { } user)
        {
            _popup.PopupEntity(Loc.GetString("anomaly-generator-capsule-missing-core"), generator.Owner, user);
        }
    }
    #endregion

    #region Lifecycle
    /// <summary>
    /// Starts up the anomaly generator applied necessary components and playing sound effects.
    /// </summary>
    private void StartAnomalyGenerator(Entity<AnomalyGeneratorComponent> generator)
    {
        if (!CanGenerateAnomaly(generator) || !CanStartAnomaly(generator)) // Already generating
            return;

        var generatingComp = EnsureComp<GeneratingAnomalyGeneratorComponent>(generator.Owner);
        generatingComp.EndTime = _time.CurTime + generator.Comp.GenerationLength;
        generatingComp.AudioStream = _audio.PlayPvs(generator.Comp.GeneratingSound, generator.Owner, AudioParams.Default.WithLoop(true))?.Entity;
        generator.Comp.CooldownEndTime = _time.CurTime + generator.Comp.CooldownLength;
        _appearance.SetData(generator.Owner, AnomalyGeneratorVisuals.Generating, true);
        UpdateGeneratorUi(generator);
    }

    /// <summary>
    /// Actually runs all the generation code and effects. Taken pretty much wholesale from AnomalySystem.Generator.
    /// </summary>
    private void FinishAnomalyGenerator(Entity<AnomalyGeneratorComponent> generator)
    {
        RemComp<GeneratingAnomalyGeneratorComponent>(generator.Owner);
        _appearance.SetData(generator.Owner, AnomalyGeneratorVisuals.Generating, false);

        if (!TryGenerateAnomaly(generator))
        {
            return; // Should probably do *something* if it fails to generate...
        }

        _audio.PlayPvs(generator.Comp.GeneratingFinishedSound, generator.Owner);
        var message = Loc.GetString("anomaly-generator-announcement");
        _radio.SendRadioMessage(generator.Owner, message, _prototype.Index<RadioChannelPrototype>(generator.Comp.ScienceChannel), generator.Owner);
        UpdateGeneratorUi(generator);
    }

    private void CancelAnomalyGenerator(Entity<AnomalyGeneratorComponent> generator)
    {
        RemComp<GeneratingAnomalyGeneratorComponent>(generator.Owner);

        _appearance.SetData(generator.Owner, AnomalyGeneratorVisuals.Generating, false);
        UpdateGeneratorUi(generator);
    }

    #endregion

    /// <summary>
    /// Attempts to retrieve the anomaly capsule from the item slot.
    /// </summary>
    private bool TryGetAnomalyCapsule(Entity<AnomalyGeneratorComponent> generator, out Entity<AnomalyCapsuleComponent> capsule)
    {
        capsule = default!;

        if (!_slots.TryGetSlot(generator.Owner, generator.Comp.CapsuleContainer, out var slot) ||
            slot.Item is not { } capsuleUid ||
            !TryComp<AnomalyCapsuleComponent>(capsuleUid, out var capsuleComp))
            return false;

        capsule = (capsuleUid, capsuleComp);
        return true;
    }
}