#if RIDER
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi;
using HelixRider.MixinLanguage.Parsing.Gen;

namespace HelixRider.MixinLanguage.Parsing;

internal sealed class HelixMixinParser : HelixMixinParserGenerated, IParser
{
    internal HelixMixinParser(ILexer<int> lexer) { SetLexer(lexer); }

    public IFile ParseFile() => (IFile)ParseHelixMixinFile();

    public override TreeElement ParseHelixMixinTokens()
    {
        var result = TreeElementFactory.CreateCompositeElement(ElementType.HELIX_MIXIN_TOKENS);
        while (myLexer.TokenType != null)
            result.AppendNewChild(CreateToken());
        return result;
    }

    protected override TreeElement CreateToken()
    {
        var type = myLexer.TokenType;
        var element = type.Create(myLexer.Buffer, new TreeOffset(myLexer.TokenStart), new TreeOffset(myLexer.TokenEnd));
        myLexer.Advance();
        return element;
    }
}
#endif
