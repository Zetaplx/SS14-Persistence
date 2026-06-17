using Content.Shared._Persistence14.Dependencies;
using JetBrains.Annotations;

namespace Content.Shared._Persistence14.Allergy;

[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class AllergenSelector
{
    [DataField]
    public float ExposureMultiplier = 1f;

    public abstract bool Exposed(ContextDependencies dependencies, AllergyContext ctx);
}