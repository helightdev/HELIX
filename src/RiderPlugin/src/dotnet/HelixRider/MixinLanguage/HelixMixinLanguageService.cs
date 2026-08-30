#if RIDER
using System.Collections.Generic;
using JetBrains.Application.Components;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.Impl;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using HelixRider.MixinLanguage.Parsing;

namespace HelixRider.MixinLanguage;

[Language(typeof(HelixMixinLanguage))]
public sealed class HelixMixinLanguageService : LanguageService
{
    public HelixMixinLanguageService(PsiLanguageType language, ILazy<IConstantValueService> constants)
        : base(language, constants) { }

    public override ILexerFactory GetPrimaryLexerFactory() => new HelixMixinLexerFactory();
    public override ILexer CreateFilteringLexer(ILexer lexer) => lexer;
    public override IParser CreateParser(ILexer lexer, IPsiModule module, IPsiSourceFile sourceFile) =>
        new HelixMixinParser(lexer as ILexer<int> ?? lexer.ToCachingLexer());
    public override IEnumerable<ITypeDeclaration> FindTypeDeclarations(IFile file) =>
        EmptyList<ITypeDeclaration>.Enumerable;
    public override ILanguageCacheProvider CacheProvider => null;
    public override bool IsCaseSensitive => true;
    public override bool SupportTypeMemberCache => false;
    public override ITypePresenter TypePresenter => DefaultTypePresenter.Instance;
}
#endif
