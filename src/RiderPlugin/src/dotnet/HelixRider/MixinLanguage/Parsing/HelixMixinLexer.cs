#if RIDER
using System.Collections.Generic;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using JetBrains.Util;
using MixinLanguage.Compiler;

namespace HelixRider.MixinLanguage.Parsing;

public sealed class HelixMixinLexer : ILexer<int>
{
    private readonly IBuffer _buffer;
    private IReadOnlyList<MixinEditorToken> _tokens;
    private int _index;
    private int _end;

    public HelixMixinLexer(IBuffer buffer) { _buffer = buffer; Start(); }
    public void Start() => Start(0, _buffer.Length, 0);
    public void Start(int startOffset, int endOffset, uint state)
    {
        var text = _buffer.GetText(new TextRange(startOffset, endOffset));
        var relative = MixinEditorLexer.Lex(text);
        var tokens = new List<MixinEditorToken>(relative.Count);
        foreach (var token in relative)
            tokens.Add(new MixinEditorToken(token.Kind, token.Start + startOffset, token.End + startOffset));
        _tokens = tokens;
        _index = 0;
        _end = endOffset;
    }

    public void Advance() { if (_index < _tokens.Count) _index++; }
    public TokenNodeType TokenType => _index >= _tokens.Count ? null : Map(_tokens[_index].Kind);
    public int TokenStart => _index >= _tokens.Count ? _end : _tokens[_index].Start;
    public int TokenEnd => _index >= _tokens.Count ? _end : _tokens[_index].End;
    public IBuffer Buffer => _buffer;
    public int EOFPos => _end;
    public int LexemIndent => 0;
    public uint LexerStateEx => 0;
    public object CurrentPosition { get => _index; set => _index = (int)value; }
    int ILexer<int>.CurrentPosition { get => _index; set => _index = value; }

    private static TokenNodeType Map(MixinEditorTokenKind kind) => kind switch
    {
        MixinEditorTokenKind.Directive => HelixMixinTokenNodeTypes.Directive,
        MixinEditorTokenKind.Value => HelixMixinTokenNodeTypes.Value,
        MixinEditorTokenKind.Path => HelixMixinTokenNodeTypes.Path,
        MixinEditorTokenKind.Function => HelixMixinTokenNodeTypes.Function,
        MixinEditorTokenKind.ArgumentDelimiter => HelixMixinTokenNodeTypes.ArgumentDelimiter,
        MixinEditorTokenKind.DirectiveArgumentDelimiter => HelixMixinTokenNodeTypes.DirectiveArgumentDelimiter,
        MixinEditorTokenKind.ExpressionArgumentDelimiter => HelixMixinTokenNodeTypes.ExpressionArgumentDelimiter,
        MixinEditorTokenKind.Argument => HelixMixinTokenNodeTypes.Argument,
        MixinEditorTokenKind.Operator => HelixMixinTokenNodeTypes.Operator,
        MixinEditorTokenKind.Parenthesis => HelixMixinTokenNodeTypes.Parenthesis,
        MixinEditorTokenKind.EnclosedReferenceParenthesis => HelixMixinTokenNodeTypes.EnclosedReferenceParenthesis,
        MixinEditorTokenKind.Escape => HelixMixinTokenNodeTypes.Escape,
        MixinEditorTokenKind.Continuation => HelixMixinTokenNodeTypes.Continuation,
        MixinEditorTokenKind.Comment => HelixMixinTokenNodeTypes.Comment,
        MixinEditorTokenKind.Whitespace => HelixMixinTokenNodeTypes.WhiteSpace,
        MixinEditorTokenKind.NewLine => HelixMixinTokenNodeTypes.NewLine,
        MixinEditorTokenKind.Invalid => HelixMixinTokenNodeTypes.Invalid,
        _ => HelixMixinTokenNodeTypes.Text
    };
}

internal sealed class HelixMixinLexerFactory : ILexerFactory
{
    public ILexer CreateLexer(IBuffer buffer) => new HelixMixinLexer(buffer);
}
#endif
