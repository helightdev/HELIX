#if RIDER
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Resources;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using JetBrains.UI.Icons;

namespace HelixRider.MixinLanguage;

[ProjectFileType(typeof(HelixMixinProjectFileType))]
public sealed class HelixMixinProjectFileLanguageService : ProjectFileLanguageService
{
    public HelixMixinProjectFileLanguageService() : base(HelixMixinProjectFileType.Instance) { }
    public override ILexerFactory GetMixedLexerFactory(ISolution solution, IBuffer buffer,
        IPsiSourceFile sourceFile = null) => HelixMixinLanguage.Instance.LanguageService()?.GetPrimaryLexerFactory();
    protected override PsiLanguageType PsiLanguageType =>
        (PsiLanguageType)HelixMixinLanguage.Instance ?? UnknownLanguage.Instance;
    public override IconId Icon => ServicesNavigationThemedIcons.UsageOther.Id;
}
#endif
