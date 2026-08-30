#if RIDER
using JetBrains.Annotations;
using JetBrains.ProjectModel;

namespace HelixRider.MixinLanguage;

[ProjectFileTypeDefinition(Name)]
public sealed class HelixMixinProjectFileType : KnownProjectFileType
{
    public new const string Name = "HELIX_MIXIN";
    public const string AdditionalFileExtension = ".HelixSourceGenerator.additionalfile";
    public const string ProjectModelExtension = ".additionalfile";

    [CanBeNull, UsedImplicitly]
    public new static HelixMixinProjectFileType Instance { get; private set; }

    public HelixMixinProjectFileType()
        // ReSharper's project-model map is keyed by the final extension. The language itself
        // still treats AdditionalFileExtension as the canonical filename suffix.
        : base(Name, "HELIX Mixin", new[] { ProjectModelExtension }) { }
}
#endif
