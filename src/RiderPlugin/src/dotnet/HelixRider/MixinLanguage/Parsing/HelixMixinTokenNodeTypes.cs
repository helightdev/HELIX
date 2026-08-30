#if RIDER
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using JetBrains.Util;

namespace HelixRider.MixinLanguage.Parsing;

public static class HelixMixinTokenNodeTypes
{
    public static readonly TokenNodeType Directive = new HelixMixinTokenNodeType("DIRECTIVE", 1000, keyword: true);
    public static readonly TokenNodeType Value = new HelixMixinTokenNodeType("VALUE", 1001, keyword: true);
    public static readonly TokenNodeType Path = new HelixMixinTokenNodeType("PATH", 1002, identifier: true);
    public static readonly TokenNodeType Function = new HelixMixinTokenNodeType("FUNCTION", 1003, identifier: true);
    public static readonly TokenNodeType ArgumentDelimiter = new HelixMixinTokenNodeType("ARGUMENT_DELIMITER", 1004);
    public static readonly TokenNodeType Argument = new HelixMixinTokenNodeType("ARGUMENT", 1005);
    public static readonly TokenNodeType Operator = new HelixMixinTokenNodeType("OPERATOR", 1006);
    public static readonly TokenNodeType Parenthesis = new HelixMixinTokenNodeType("PARENTHESIS", 1007);
    public static readonly TokenNodeType Escape = new HelixMixinTokenNodeType("ESCAPE", 1008);
    public static readonly TokenNodeType Continuation = new HelixMixinTokenNodeType("CONTINUATION", 1009);
    public static readonly TokenNodeType Comment = new HelixMixinTokenNodeType("COMMENT", 1010, comment: true);
    public static readonly TokenNodeType WhiteSpace = new HelixMixinTokenNodeType("WHITE_SPACE", 1011, whitespace: true);
    public static readonly TokenNodeType NewLine = new HelixMixinTokenNodeType("NEW_LINE", 1012, whitespace: true);
    public static readonly TokenNodeType Text = new HelixMixinTokenNodeType("TEXT", 1013);
    public static readonly TokenNodeType Invalid = new HelixMixinTokenNodeType("INVALID", 1014);
    public static readonly TokenNodeType Eof = new HelixMixinTokenNodeType("EOF", 1015);
    public static readonly TokenNodeType DirectiveArgumentDelimiter = new HelixMixinTokenNodeType("DIRECTIVE_ARGUMENT_DELIMITER", 1016);
    public static readonly TokenNodeType ExpressionArgumentDelimiter = new HelixMixinTokenNodeType("EXPRESSION_ARGUMENT_DELIMITER", 1017);
    public static readonly TokenNodeType EnclosedReferenceParenthesis = new HelixMixinTokenNodeType("ENCLOSED_REFERENCE_PARENTHESIS", 1018);
    public static readonly TokenNodeType Expression = new HelixMixinTokenNodeType("EXPRESSION", 1019);

    public static readonly TokenNodeType DIRECTIVE = Directive;
    public static readonly TokenNodeType VALUE = Value;
    public static readonly TokenNodeType PATH = Path;
    public static readonly TokenNodeType FUNCTION = Function;
    public static readonly TokenNodeType ARGUMENT_DELIMITER = ArgumentDelimiter;
    public static readonly TokenNodeType ARGUMENT = Argument;
    public static readonly TokenNodeType OPERATOR = Operator;
    public static readonly TokenNodeType PARENTHESIS = Parenthesis;
    public static readonly TokenNodeType ESCAPE = Escape;
    public static readonly TokenNodeType CONTINUATION = Continuation;
    public static readonly TokenNodeType COMMENT = Comment;
    public static readonly TokenNodeType WHITE_SPACE = WhiteSpace;
    public static readonly TokenNodeType NEW_LINE = NewLine;
    public static readonly TokenNodeType TEXT = Text;
    public static readonly TokenNodeType INVALID = Invalid;
    public static readonly TokenNodeType DIRECTIVE_ARGUMENT_DELIMITER = DirectiveArgumentDelimiter;
    public static readonly TokenNodeType EXPRESSION_ARGUMENT_DELIMITER = ExpressionArgumentDelimiter;
    public static readonly TokenNodeType ENCLOSED_REFERENCE_PARENTHESIS = EnclosedReferenceParenthesis;
    public static readonly TokenNodeType EXPRESSION = Expression;
}

internal sealed class HelixMixinTokenNodeType : TokenNodeType
{
    private readonly bool _keyword;
    private readonly bool _identifier;
    private readonly bool _comment;
    private readonly bool _whitespace;

    internal HelixMixinTokenNodeType(string name, int index, bool keyword = false,
        bool identifier = false, bool comment = false, bool whitespace = false) : base(name, index)
    {
        _keyword = keyword;
        _identifier = identifier;
        _comment = comment;
        _whitespace = whitespace;
    }

    public override LeafElementBase Create(IBuffer buffer, TreeOffset startOffset, TreeOffset endOffset) =>
        new HelixMixinTokenNode(this, buffer.GetText(new TextRange(startOffset.Offset, endOffset.Offset)));

    public override bool IsWhitespace => _whitespace;
    public override bool IsComment => _comment;
    public override bool IsStringLiteral => false;
    public override bool IsConstantLiteral => false;
    public override bool IsIdentifier => _identifier;
    public override bool IsKeyword => _keyword;
    public override string TokenRepresentation => string.Empty;
}
#endif
