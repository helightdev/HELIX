#if RIDER
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;

namespace HelixRider.MixinLanguage;

[LanguageDefinition(Name)]
public sealed class HelixMixinLanguage : KnownLanguage
{
    public new const string Name = "HELIX_MIXIN";

    [CanBeNull, UsedImplicitly]
    public static HelixMixinLanguage Instance { get; private set; }

    public HelixMixinLanguage() : base(Name, "HELIX Mixin") { }
}
#endif
