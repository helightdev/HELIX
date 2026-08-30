#if RIDER
using System.Collections.Generic;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using JetBrains.Util;

namespace HelixRider.MixinLanguage.Parsing;

public sealed class HelixMixinLexer : ILexer<int>
{
    private readonly IBuffer _buffer;
    private IReadOnlyList<TokenSpan> _tokens;
    private int _index;
    private int _end;

    public HelixMixinLexer(IBuffer buffer) { _buffer = buffer; Start(); }
    public void Start() => Start(0, _buffer.Length, 0);
    public void Start(int startOffset, int endOffset, uint state)
    {
        var text = _buffer.GetText(new TextRange(startOffset, endOffset));
        var tokens = new List<TokenSpan>();
        for (var position = 0; position < text.Length;)
        {
            var tokenStart = position;
            if (text[position] is '\r' or '\n')
            {
                if (text[position] == '\r' && position + 1 < text.Length && text[position + 1] == '\n') position++;
                position++;
                tokens.Add(new TokenSpan(HelixMixinTokenNodeTypes.NewLine, tokenStart + startOffset, position + startOffset));
                continue;
            }
            if (text[position] is ' ' or '\t')
            {
                while (position < text.Length && text[position] is ' ' or '\t') position++;
                tokens.Add(new TokenSpan(HelixMixinTokenNodeTypes.WhiteSpace, tokenStart + startOffset, position + startOffset));
                continue;
            }
            // Punctuation is kept as an individual leaf so every shared-parser source boundary
            // can be represented without the lexer deciding what that punctuation means.
            if (!char.IsLetterOrDigit(text[position]) && text[position] != '_') position++;
            else while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
            tokens.Add(new TokenSpan(HelixMixinTokenNodeTypes.Text, tokenStart + startOffset, position + startOffset));
        }
        _tokens = tokens;
        _index = 0;
        _end = endOffset;
    }

    public void Advance() { if (_index < _tokens.Count) _index++; }
    public TokenNodeType TokenType => _index >= _tokens.Count ? null : _tokens[_index].Type;
    public int TokenStart => _index >= _tokens.Count ? _end : _tokens[_index].Start;
    public int TokenEnd => _index >= _tokens.Count ? _end : _tokens[_index].End;
    public IBuffer Buffer => _buffer;
    public int EOFPos => _end;
    public int LexemIndent => 0;
    public uint LexerStateEx => 0;
    public object CurrentPosition { get => _index; set => _index = (int)value; }
    int ILexer<int>.CurrentPosition { get => _index; set => _index = value; }

    private readonly record struct TokenSpan(TokenNodeType Type, int Start, int End);
}

internal sealed class HelixMixinLexerFactory : ILexerFactory
{
    public ILexer CreateLexer(IBuffer buffer) => new HelixMixinLexer(buffer);
}
#endif
