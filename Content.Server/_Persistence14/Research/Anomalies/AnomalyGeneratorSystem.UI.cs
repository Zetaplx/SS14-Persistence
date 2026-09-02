using System.Linq;
using Content.Server.Anomaly.Components;
using Content.Shared._Persistence14.Research.Anomalies;
using Content.Shared._Persistence14.Research.Anomalies.Modules;
using Content.Shared.Anomaly;
using Robust.Shared.Prototypes;


namespace Content.Server._Persistence14.Research.Anomalies;

public sealed partial class AnomalyGeneratorSystem
{
    private void UpdateGeneratorUi(Entity<AnomalyGeneratorComponent> generator)
    {
        var isGenerating = TryComp<GeneratingAnomalyGeneratorComponent>(generator.Owner, out var generatingComp);
        var isOnCooldown = _time.CurTime < generator.Comp.CooldownEndTime;

        var canGenerate = CanGenerateAnomaly(generator, out _);
        var hasCapsule = TryGetAnomalyCapsule(generator, out var capsule);

        var material = _material.GetMaterialAmount(generator.Owner, generator.Comp.RequiredMaterial);


        var list = new Dictionary<ProtoId<AnomalyPrototype>, float>();
        if (hasCapsule && _capsules.TryGetCore(capsule, out var core))
        {
            list = _randomTable.ListPrototype<AnomalyPrototype>(core.Comp.AnomalyPool).ToDictionary(
                pair => new ProtoId<AnomalyPrototype>(pair.prototype.ID),
                pair => pair.prob
            );
        }

        var forcedEnvironmental = false;
        var forcedInfectious = false;
        if (hasCapsule)
        {
            foreach (var module in _capsules.GetModules(capsule))
            {
                if (TryComp<CategorySpecifierCapsuleModuleComponent>(module.Owner, out var comp))
                {
                    if (comp.Category == AnomalyCategory.Environmental) forcedEnvironmental = true;
                    if (comp.Category == AnomalyCategory.Infectious) forcedInfectious = true;
                }
            }
        }


        var state = new AnomalyGeneratorBUIState
        {
            GenerateDuration = generator.Comp.GenerationLength,
            GenerateEndTime = isGenerating ? generatingComp?.EndTime : null,
            CooldownDuration = generator.Comp.CooldownLength,
            CooldownEndTime = isOnCooldown ? generator.Comp.CooldownEndTime : null,

            CanGenerateAnomaly = canGenerate,

            MaterialAmount = material / 100f,
            MaterialRequired = generator.Comp.MaterialPerAnomaly / 100f,
            Capsule = hasCapsule ? GetNetEntity(capsule.Owner) : null,

            AnomalyProbabilities = list,
            ForcedEnvironmental = forcedEnvironmental,
            ForcedInfectious = forcedInfectious
        };
        _ui.SetUiState(generator.Owner, AnomalyGeneratorUiKey.Key, state);
    }
}