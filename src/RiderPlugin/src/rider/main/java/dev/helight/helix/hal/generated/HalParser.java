// Generated from HalParser.g4 by ANTLR 4.13.2
package dev.helight.helix.hal.generated;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast", "CheckReturnValue", "this-escape"})
public class HalParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.13.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		SECTION=1, PERCENT=2, AMP=3, HASH=4, EQUALS=5, COMMA=6, LBRACE=7, RBRACE=8,
		LBRACKET=9, RBRACKET=10, LPAREN=11, RPAREN=12, LANGLE=13, RANGLE=14, TRUE=15,
		FALSE=16, NULL=17, NUMBER=18, STRING=19, IDENTIFIER=20, BARE=21, LINE_COMMENT=22,
		NEWLINE=23, SPACE=24, ERROR_TOKEN=25;
	public static final int
		RULE_document = 0, RULE_sectionBlock = 1, RULE_metadata = 2, RULE_metadataArguments = 3,
		RULE_metadataArgument = 4, RULE_section = 5, RULE_sectionEntry = 6, RULE_field = 7,
		RULE_fieldKey = 8, RULE_value = 9, RULE_typedContainer = 10, RULE_table = 11,
		RULE_tableEntry = 12, RULE_list = 13, RULE_collectionValue = 14, RULE_call = 15,
		RULE_argumentList = 16, RULE_argument = 17, RULE_selection = 18, RULE_looseScalar = 19,
		RULE_scalarAtom = 20, RULE_lineEnd = 21, RULE_newlines = 22;
	private static String[] makeRuleNames() {
		return new String[] {
			"document", "sectionBlock", "metadata", "metadataArguments", "metadataArgument",
			"section", "sectionEntry", "field", "fieldKey", "value", "typedContainer",
			"table", "tableEntry", "list", "collectionValue", "call", "argumentList",
			"argument", "selection", "looseScalar", "scalarAtom", "lineEnd", "newlines"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'---'", "'%'", "'&'", "'#'", "'='", "','", "'{'", "'}'", "'['",
			"']'", "'('", "')'", "'<'", "'>'", "'true'", "'false'", "'null'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "SECTION", "PERCENT", "AMP", "HASH", "EQUALS", "COMMA", "LBRACE",
			"RBRACE", "LBRACKET", "RBRACKET", "LPAREN", "RPAREN", "LANGLE", "RANGLE",
			"TRUE", "FALSE", "NULL", "NUMBER", "STRING", "IDENTIFIER", "BARE", "LINE_COMMENT",
			"NEWLINE", "SPACE", "ERROR_TOKEN"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}

	@Override
	public String getGrammarFileName() { return "HalParser.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public HalParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@SuppressWarnings("CheckReturnValue")
	public static class DocumentContext extends ParserRuleContext {
		public List<NewlinesContext> newlines() {
			return getRuleContexts(NewlinesContext.class);
		}
		public NewlinesContext newlines(int i) {
			return getRuleContext(NewlinesContext.class,i);
		}
		public List<SectionBlockContext> sectionBlock() {
			return getRuleContexts(SectionBlockContext.class);
		}
		public SectionBlockContext sectionBlock(int i) {
			return getRuleContext(SectionBlockContext.class,i);
		}
		public TerminalNode EOF() { return getToken(HalParser.EOF, 0); }
		public DocumentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_document; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitDocument(this);
			else return visitor.visitChildren(this);
		}
	}

	public final DocumentContext document() throws RecognitionException {
		DocumentContext _localctx = new DocumentContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_document);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(46);
			newlines();
			setState(47);
			sectionBlock();
			setState(53);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,0,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(48);
					newlines();
					setState(49);
					sectionBlock();
					}
					}
				}
				setState(55);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,0,_ctx);
			}
			setState(56);
			newlines();
			setState(57);
			match(EOF);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class SectionBlockContext extends ParserRuleContext {
		public SectionContext section() {
			return getRuleContext(SectionContext.class,0);
		}
		public List<MetadataContext> metadata() {
			return getRuleContexts(MetadataContext.class);
		}
		public MetadataContext metadata(int i) {
			return getRuleContext(MetadataContext.class,i);
		}
		public SectionBlockContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_sectionBlock; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitSectionBlock(this);
			else return visitor.visitChildren(this);
		}
	}

	public final SectionBlockContext sectionBlock() throws RecognitionException {
		SectionBlockContext _localctx = new SectionBlockContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_sectionBlock);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(62);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==PERCENT) {
				{
				{
				setState(59);
				metadata();
				}
				}
				setState(64);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(65);
			section();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class MetadataContext extends ParserRuleContext {
		public TerminalNode PERCENT() { return getToken(HalParser.PERCENT, 0); }
		public TerminalNode IDENTIFIER() { return getToken(HalParser.IDENTIFIER, 0); }
		public NewlinesContext newlines() {
			return getRuleContext(NewlinesContext.class,0);
		}
		public MetadataArgumentsContext metadataArguments() {
			return getRuleContext(MetadataArgumentsContext.class,0);
		}
		public MetadataContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_metadata; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitMetadata(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MetadataContext metadata() throws RecognitionException {
		MetadataContext _localctx = new MetadataContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_metadata);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(67);
			match(PERCENT);
			setState(68);
			match(IDENTIFIER);
			setState(70);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==LPAREN || _la==LANGLE) {
				{
				setState(69);
				metadataArguments();
				}
			}

			setState(72);
			newlines();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class MetadataArgumentsContext extends ParserRuleContext {
		public TerminalNode LANGLE() { return getToken(HalParser.LANGLE, 0); }
		public TerminalNode RANGLE() { return getToken(HalParser.RANGLE, 0); }
		public List<MetadataArgumentContext> metadataArgument() {
			return getRuleContexts(MetadataArgumentContext.class);
		}
		public MetadataArgumentContext metadataArgument(int i) {
			return getRuleContext(MetadataArgumentContext.class,i);
		}
		public TerminalNode LPAREN() { return getToken(HalParser.LPAREN, 0); }
		public TerminalNode RPAREN() { return getToken(HalParser.RPAREN, 0); }
		public ArgumentListContext argumentList() {
			return getRuleContext(ArgumentListContext.class,0);
		}
		public MetadataArgumentsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_metadataArguments; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitMetadataArguments(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MetadataArgumentsContext metadataArguments() throws RecognitionException {
		MetadataArgumentsContext _localctx = new MetadataArgumentsContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_metadataArguments);
		int _la;
		try {
			setState(87);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case LANGLE:
				enterOuterAlt(_localctx, 1);
				{
				setState(74);
				match(LANGLE);
				setState(78);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 4161660L) != 0)) {
					{
					{
					setState(75);
					metadataArgument();
					}
					}
					setState(80);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(81);
				match(RANGLE);
				}
				break;
			case LPAREN:
				enterOuterAlt(_localctx, 2);
				{
				setState(82);
				match(LPAREN);
				setState(84);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 4162176L) != 0)) {
					{
					setState(83);
					argumentList();
					}
				}

				setState(86);
				match(RPAREN);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class MetadataArgumentContext extends ParserRuleContext {
		public ScalarAtomContext scalarAtom() {
			return getRuleContext(ScalarAtomContext.class,0);
		}
		public TerminalNode COMMA() { return getToken(HalParser.COMMA, 0); }
		public TerminalNode EQUALS() { return getToken(HalParser.EQUALS, 0); }
		public TerminalNode HASH() { return getToken(HalParser.HASH, 0); }
		public TerminalNode AMP() { return getToken(HalParser.AMP, 0); }
		public TerminalNode PERCENT() { return getToken(HalParser.PERCENT, 0); }
		public MetadataArgumentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_metadataArgument; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitMetadataArgument(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MetadataArgumentContext metadataArgument() throws RecognitionException {
		MetadataArgumentContext _localctx = new MetadataArgumentContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_metadataArgument);
		try {
			setState(95);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case TRUE:
			case FALSE:
			case NULL:
			case NUMBER:
			case STRING:
			case IDENTIFIER:
			case BARE:
				enterOuterAlt(_localctx, 1);
				{
				setState(89);
				scalarAtom();
				}
				break;
			case COMMA:
				enterOuterAlt(_localctx, 2);
				{
				setState(90);
				match(COMMA);
				}
				break;
			case EQUALS:
				enterOuterAlt(_localctx, 3);
				{
				setState(91);
				match(EQUALS);
				}
				break;
			case HASH:
				enterOuterAlt(_localctx, 4);
				{
				setState(92);
				match(HASH);
				}
				break;
			case AMP:
				enterOuterAlt(_localctx, 5);
				{
				setState(93);
				match(AMP);
				}
				break;
			case PERCENT:
				enterOuterAlt(_localctx, 6);
				{
				setState(94);
				match(PERCENT);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class SectionContext extends ParserRuleContext {
		public TerminalNode SECTION() { return getToken(HalParser.SECTION, 0); }
		public TerminalNode IDENTIFIER() { return getToken(HalParser.IDENTIFIER, 0); }
		public LineEndContext lineEnd() {
			return getRuleContext(LineEndContext.class,0);
		}
		public TerminalNode AMP() { return getToken(HalParser.AMP, 0); }
		public ScalarAtomContext scalarAtom() {
			return getRuleContext(ScalarAtomContext.class,0);
		}
		public List<SectionEntryContext> sectionEntry() {
			return getRuleContexts(SectionEntryContext.class);
		}
		public SectionEntryContext sectionEntry(int i) {
			return getRuleContext(SectionEntryContext.class,i);
		}
		public SectionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_section; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitSection(this);
			else return visitor.visitChildren(this);
		}
	}

	public final SectionContext section() throws RecognitionException {
		SectionContext _localctx = new SectionContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_section);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(97);
			match(SECTION);
			setState(98);
			match(IDENTIFIER);
			setState(101);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==AMP) {
				{
				setState(99);
				match(AMP);
				setState(100);
				scalarAtom();
				}
			}

			setState(103);
			lineEnd();
			setState(107);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,8,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(104);
					sectionEntry();
					}
					}
				}
				setState(109);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,8,_ctx);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class SectionEntryContext extends ParserRuleContext {
		public FieldContext field() {
			return getRuleContext(FieldContext.class,0);
		}
		public List<MetadataContext> metadata() {
			return getRuleContexts(MetadataContext.class);
		}
		public MetadataContext metadata(int i) {
			return getRuleContext(MetadataContext.class,i);
		}
		public SectionEntryContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_sectionEntry; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitSectionEntry(this);
			else return visitor.visitChildren(this);
		}
	}

	public final SectionEntryContext sectionEntry() throws RecognitionException {
		SectionEntryContext _localctx = new SectionEntryContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_sectionEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(113);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==PERCENT) {
				{
				{
				setState(110);
				metadata();
				}
				}
				setState(115);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(116);
			field();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class FieldContext extends ParserRuleContext {
		public FieldKeyContext fieldKey() {
			return getRuleContext(FieldKeyContext.class,0);
		}
		public LineEndContext lineEnd() {
			return getRuleContext(LineEndContext.class,0);
		}
		public TerminalNode EQUALS() { return getToken(HalParser.EQUALS, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public CollectionValueContext collectionValue() {
			return getRuleContext(CollectionValueContext.class,0);
		}
		public FieldContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_field; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitField(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FieldContext field() throws RecognitionException {
		FieldContext _localctx = new FieldContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_field);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(118);
			fieldKey();
			setState(122);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case EQUALS:
				{
				setState(119);
				match(EQUALS);
				setState(120);
				value();
				}
				break;
			case LBRACE:
			case LBRACKET:
			case IDENTIFIER:
				{
				setState(121);
				collectionValue();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			setState(124);
			lineEnd();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class FieldKeyContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HalParser.IDENTIFIER, 0); }
		public TerminalNode STRING() { return getToken(HalParser.STRING, 0); }
		public FieldKeyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_fieldKey; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitFieldKey(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FieldKeyContext fieldKey() throws RecognitionException {
		FieldKeyContext _localctx = new FieldKeyContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_fieldKey);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(126);
			_la = _input.LA(1);
			if ( !(_la==STRING || _la==IDENTIFIER) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ValueContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HalParser.IDENTIFIER, 0); }
		public TypedContainerContext typedContainer() {
			return getRuleContext(TypedContainerContext.class,0);
		}
		public List<SelectionContext> selection() {
			return getRuleContexts(SelectionContext.class);
		}
		public SelectionContext selection(int i) {
			return getRuleContext(SelectionContext.class,i);
		}
		public CallContext call() {
			return getRuleContext(CallContext.class,0);
		}
		public TableContext table() {
			return getRuleContext(TableContext.class,0);
		}
		public ListContext list() {
			return getRuleContext(ListContext.class,0);
		}
		public LooseScalarContext looseScalar() {
			return getRuleContext(LooseScalarContext.class,0);
		}
		public ValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_value; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ValueContext value() throws RecognitionException {
		ValueContext _localctx = new ValueContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_value);
		int _la;
		try {
			setState(164);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,16,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(128);
				match(IDENTIFIER);
				setState(129);
				typedContainer();
				setState(133);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==HASH) {
					{
					{
					setState(130);
					selection();
					}
					}
					setState(135);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(136);
				call();
				setState(140);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==HASH) {
					{
					{
					setState(137);
					selection();
					}
					}
					setState(142);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(143);
				table();
				setState(147);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==HASH) {
					{
					{
					setState(144);
					selection();
					}
					}
					setState(149);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(150);
				list();
				setState(154);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==HASH) {
					{
					{
					setState(151);
					selection();
					}
					}
					setState(156);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(157);
				looseScalar();
				setState(161);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==HASH) {
					{
					{
					setState(158);
					selection();
					}
					}
					setState(163);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class TypedContainerContext extends ParserRuleContext {
		public TableContext table() {
			return getRuleContext(TableContext.class,0);
		}
		public ListContext list() {
			return getRuleContext(ListContext.class,0);
		}
		public TypedContainerContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_typedContainer; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitTypedContainer(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TypedContainerContext typedContainer() throws RecognitionException {
		TypedContainerContext _localctx = new TypedContainerContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_typedContainer);
		try {
			setState(168);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case LBRACE:
				enterOuterAlt(_localctx, 1);
				{
				setState(166);
				table();
				}
				break;
			case LBRACKET:
				enterOuterAlt(_localctx, 2);
				{
				setState(167);
				list();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class TableContext extends ParserRuleContext {
		public TerminalNode LBRACE() { return getToken(HalParser.LBRACE, 0); }
		public List<NewlinesContext> newlines() {
			return getRuleContexts(NewlinesContext.class);
		}
		public NewlinesContext newlines(int i) {
			return getRuleContext(NewlinesContext.class,i);
		}
		public TerminalNode RBRACE() { return getToken(HalParser.RBRACE, 0); }
		public List<TableEntryContext> tableEntry() {
			return getRuleContexts(TableEntryContext.class);
		}
		public TableEntryContext tableEntry(int i) {
			return getRuleContext(TableEntryContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(HalParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(HalParser.COMMA, i);
		}
		public List<TerminalNode> NEWLINE() { return getTokens(HalParser.NEWLINE); }
		public TerminalNode NEWLINE(int i) {
			return getToken(HalParser.NEWLINE, i);
		}
		public TableContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_table; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitTable(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TableContext table() throws RecognitionException {
		TableContext _localctx = new TableContext(_ctx, getState());
		enterRule(_localctx, 22, RULE_table);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(170);
			match(LBRACE);
			setState(171);
			newlines();
			setState(193);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1572868L) != 0)) {
				{
				setState(172);
				tableEntry();
				setState(185);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,20,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(180);
						_errHandler.sync(this);
						switch (_input.LA(1)) {
						case COMMA:
							{
							setState(173);
							match(COMMA);
							setState(174);
							newlines();
							}
							break;
						case NEWLINE:
							{
							setState(176);
							_errHandler.sync(this);
							_la = _input.LA(1);
							do {
								{
								{
								setState(175);
								match(NEWLINE);
								}
								}
								setState(178);
								_errHandler.sync(this);
								_la = _input.LA(1);
							} while ( _la==NEWLINE );
							}
							break;
						default:
							throw new NoViableAltException(this);
						}
						setState(182);
						tableEntry();
						}
						}
					}
					setState(187);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,20,_ctx);
				}
				setState(189);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==COMMA) {
					{
					setState(188);
					match(COMMA);
					}
				}

				setState(191);
				newlines();
				}
			}

			setState(195);
			match(RBRACE);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class TableEntryContext extends ParserRuleContext {
		public FieldKeyContext fieldKey() {
			return getRuleContext(FieldKeyContext.class,0);
		}
		public TerminalNode EQUALS() { return getToken(HalParser.EQUALS, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public CollectionValueContext collectionValue() {
			return getRuleContext(CollectionValueContext.class,0);
		}
		public List<MetadataContext> metadata() {
			return getRuleContexts(MetadataContext.class);
		}
		public MetadataContext metadata(int i) {
			return getRuleContext(MetadataContext.class,i);
		}
		public TableEntryContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tableEntry; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitTableEntry(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TableEntryContext tableEntry() throws RecognitionException {
		TableEntryContext _localctx = new TableEntryContext(_ctx, getState());
		enterRule(_localctx, 24, RULE_tableEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(200);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==PERCENT) {
				{
				{
				setState(197);
				metadata();
				}
				}
				setState(202);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(203);
			fieldKey();
			setState(207);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case EQUALS:
				{
				setState(204);
				match(EQUALS);
				setState(205);
				value();
				}
				break;
			case LBRACE:
			case LBRACKET:
			case IDENTIFIER:
				{
				setState(206);
				collectionValue();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ListContext extends ParserRuleContext {
		public TerminalNode LBRACKET() { return getToken(HalParser.LBRACKET, 0); }
		public List<NewlinesContext> newlines() {
			return getRuleContexts(NewlinesContext.class);
		}
		public NewlinesContext newlines(int i) {
			return getRuleContext(NewlinesContext.class,i);
		}
		public TerminalNode RBRACKET() { return getToken(HalParser.RBRACKET, 0); }
		public List<ValueContext> value() {
			return getRuleContexts(ValueContext.class);
		}
		public ValueContext value(int i) {
			return getRuleContext(ValueContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(HalParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(HalParser.COMMA, i);
		}
		public List<TerminalNode> NEWLINE() { return getTokens(HalParser.NEWLINE); }
		public TerminalNode NEWLINE(int i) {
			return getToken(HalParser.NEWLINE, i);
		}
		public ListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_list; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitList(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ListContext list() throws RecognitionException {
		ListContext _localctx = new ListContext(_ctx, getState());
		enterRule(_localctx, 26, RULE_list);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(209);
			match(LBRACKET);
			setState(210);
			newlines();
			setState(232);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 4162176L) != 0)) {
				{
				setState(211);
				value();
				setState(224);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,27,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(219);
						_errHandler.sync(this);
						switch (_input.LA(1)) {
						case COMMA:
							{
							setState(212);
							match(COMMA);
							setState(213);
							newlines();
							}
							break;
						case NEWLINE:
							{
							setState(215);
							_errHandler.sync(this);
							_la = _input.LA(1);
							do {
								{
								{
								setState(214);
								match(NEWLINE);
								}
								}
								setState(217);
								_errHandler.sync(this);
								_la = _input.LA(1);
							} while ( _la==NEWLINE );
							}
							break;
						default:
							throw new NoViableAltException(this);
						}
						setState(221);
						value();
						}
						}
					}
					setState(226);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,27,_ctx);
				}
				setState(228);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==COMMA) {
					{
					setState(227);
					match(COMMA);
					}
				}

				setState(230);
				newlines();
				}
			}

			setState(234);
			match(RBRACKET);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class CollectionValueContext extends ParserRuleContext {
		public TypedContainerContext typedContainer() {
			return getRuleContext(TypedContainerContext.class,0);
		}
		public TerminalNode IDENTIFIER() { return getToken(HalParser.IDENTIFIER, 0); }
		public CollectionValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_collectionValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitCollectionValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final CollectionValueContext collectionValue() throws RecognitionException {
		CollectionValueContext _localctx = new CollectionValueContext(_ctx, getState());
		enterRule(_localctx, 28, RULE_collectionValue);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(237);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==IDENTIFIER) {
				{
				setState(236);
				match(IDENTIFIER);
				}
			}

			setState(239);
			typedContainer();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class CallContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HalParser.IDENTIFIER, 0); }
		public TerminalNode LPAREN() { return getToken(HalParser.LPAREN, 0); }
		public NewlinesContext newlines() {
			return getRuleContext(NewlinesContext.class,0);
		}
		public TerminalNode RPAREN() { return getToken(HalParser.RPAREN, 0); }
		public ArgumentListContext argumentList() {
			return getRuleContext(ArgumentListContext.class,0);
		}
		public CallContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_call; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitCall(this);
			else return visitor.visitChildren(this);
		}
	}

	public final CallContext call() throws RecognitionException {
		CallContext _localctx = new CallContext(_ctx, getState());
		enterRule(_localctx, 30, RULE_call);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(241);
			match(IDENTIFIER);
			setState(242);
			match(LPAREN);
			setState(243);
			newlines();
			setState(245);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 4162176L) != 0)) {
				{
				setState(244);
				argumentList();
				}
			}

			setState(247);
			match(RPAREN);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ArgumentListContext extends ParserRuleContext {
		public List<ArgumentContext> argument() {
			return getRuleContexts(ArgumentContext.class);
		}
		public ArgumentContext argument(int i) {
			return getRuleContext(ArgumentContext.class,i);
		}
		public List<NewlinesContext> newlines() {
			return getRuleContexts(NewlinesContext.class);
		}
		public NewlinesContext newlines(int i) {
			return getRuleContext(NewlinesContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(HalParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(HalParser.COMMA, i);
		}
		public ArgumentListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_argumentList; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitArgumentList(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ArgumentListContext argumentList() throws RecognitionException {
		ArgumentListContext _localctx = new ArgumentListContext(_ctx, getState());
		enterRule(_localctx, 32, RULE_argumentList);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(249);
			argument();
			setState(256);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,32,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(250);
					match(COMMA);
					setState(251);
					newlines();
					setState(252);
					argument();
					}
					}
				}
				setState(258);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,32,_ctx);
			}
			setState(260);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==COMMA) {
				{
				setState(259);
				match(COMMA);
				}
			}

			setState(262);
			newlines();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ArgumentContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HalParser.IDENTIFIER, 0); }
		public TerminalNode EQUALS() { return getToken(HalParser.EQUALS, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public ArgumentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_argument; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitArgument(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ArgumentContext argument() throws RecognitionException {
		ArgumentContext _localctx = new ArgumentContext(_ctx, getState());
		enterRule(_localctx, 34, RULE_argument);
		try {
			setState(268);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,34,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(264);
				match(IDENTIFIER);
				setState(265);
				match(EQUALS);
				setState(266);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(267);
				value();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class SelectionContext extends ParserRuleContext {
		public TerminalNode HASH() { return getToken(HalParser.HASH, 0); }
		public TerminalNode IDENTIFIER() { return getToken(HalParser.IDENTIFIER, 0); }
		public SelectionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_selection; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitSelection(this);
			else return visitor.visitChildren(this);
		}
	}

	public final SelectionContext selection() throws RecognitionException {
		SelectionContext _localctx = new SelectionContext(_ctx, getState());
		enterRule(_localctx, 36, RULE_selection);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(270);
			match(HASH);
			setState(271);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class LooseScalarContext extends ParserRuleContext {
		public List<ScalarAtomContext> scalarAtom() {
			return getRuleContexts(ScalarAtomContext.class);
		}
		public ScalarAtomContext scalarAtom(int i) {
			return getRuleContext(ScalarAtomContext.class,i);
		}
		public LooseScalarContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_looseScalar; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitLooseScalar(this);
			else return visitor.visitChildren(this);
		}
	}

	public final LooseScalarContext looseScalar() throws RecognitionException {
		LooseScalarContext _localctx = new LooseScalarContext(_ctx, getState());
		enterRule(_localctx, 38, RULE_looseScalar);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(274);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(273);
				scalarAtom();
				}
				}
				setState(276);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( (((_la) & ~0x3f) == 0 && ((1L << _la) & 4161536L) != 0) );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ScalarAtomContext extends ParserRuleContext {
		public TerminalNode STRING() { return getToken(HalParser.STRING, 0); }
		public TerminalNode NUMBER() { return getToken(HalParser.NUMBER, 0); }
		public TerminalNode TRUE() { return getToken(HalParser.TRUE, 0); }
		public TerminalNode FALSE() { return getToken(HalParser.FALSE, 0); }
		public TerminalNode NULL() { return getToken(HalParser.NULL, 0); }
		public TerminalNode IDENTIFIER() { return getToken(HalParser.IDENTIFIER, 0); }
		public TerminalNode BARE() { return getToken(HalParser.BARE, 0); }
		public ScalarAtomContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_scalarAtom; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitScalarAtom(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ScalarAtomContext scalarAtom() throws RecognitionException {
		ScalarAtomContext _localctx = new ScalarAtomContext(_ctx, getState());
		enterRule(_localctx, 40, RULE_scalarAtom);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(278);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 4161536L) != 0)) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class LineEndContext extends ParserRuleContext {
		public TerminalNode COMMA() { return getToken(HalParser.COMMA, 0); }
		public TerminalNode EOF() { return getToken(HalParser.EOF, 0); }
		public List<TerminalNode> NEWLINE() { return getTokens(HalParser.NEWLINE); }
		public TerminalNode NEWLINE(int i) {
			return getToken(HalParser.NEWLINE, i);
		}
		public LineEndContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_lineEnd; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitLineEnd(this);
			else return visitor.visitChildren(this);
		}
	}

	public final LineEndContext lineEnd() throws RecognitionException {
		LineEndContext _localctx = new LineEndContext(_ctx, getState());
		enterRule(_localctx, 42, RULE_lineEnd);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(290);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,38,_ctx) ) {
			case 1:
				{
				setState(281);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==COMMA) {
					{
					setState(280);
					match(COMMA);
					}
				}

				setState(284);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(283);
						match(NEWLINE);
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(286);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,37,_ctx);
				} while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER );
				}
				break;
			case 2:
				{
				setState(288);
				match(COMMA);
				}
				break;
			case 3:
				{
				setState(289);
				match(EOF);
				}
				break;
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class NewlinesContext extends ParserRuleContext {
		public List<TerminalNode> NEWLINE() { return getTokens(HalParser.NEWLINE); }
		public TerminalNode NEWLINE(int i) {
			return getToken(HalParser.NEWLINE, i);
		}
		public NewlinesContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_newlines; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HalParserVisitor ) return ((HalParserVisitor<? extends T>)visitor).visitNewlines(this);
			else return visitor.visitChildren(this);
		}
	}

	public final NewlinesContext newlines() throws RecognitionException {
		NewlinesContext _localctx = new NewlinesContext(_ctx, getState());
		enterRule(_localctx, 44, RULE_newlines);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(295);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==NEWLINE) {
				{
				{
				setState(292);
				match(NEWLINE);
				}
				}
				setState(297);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static final String _serializedATN =
		"\u0004\u0001\u0019\u012b\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001"+
		"\u0002\u0002\u0007\u0002\u0002\u0003\u0007\u0003\u0002\u0004\u0007\u0004"+
		"\u0002\u0005\u0007\u0005\u0002\u0006\u0007\u0006\u0002\u0007\u0007\u0007"+
		"\u0002\b\u0007\b\u0002\t\u0007\t\u0002\n\u0007\n\u0002\u000b\u0007\u000b"+
		"\u0002\f\u0007\f\u0002\r\u0007\r\u0002\u000e\u0007\u000e\u0002\u000f\u0007"+
		"\u000f\u0002\u0010\u0007\u0010\u0002\u0011\u0007\u0011\u0002\u0012\u0007"+
		"\u0012\u0002\u0013\u0007\u0013\u0002\u0014\u0007\u0014\u0002\u0015\u0007"+
		"\u0015\u0002\u0016\u0007\u0016\u0001\u0000\u0001\u0000\u0001\u0000\u0001"+
		"\u0000\u0001\u0000\u0005\u00004\b\u0000\n\u0000\f\u00007\t\u0000\u0001"+
		"\u0000\u0001\u0000\u0001\u0000\u0001\u0001\u0005\u0001=\b\u0001\n\u0001"+
		"\f\u0001@\t\u0001\u0001\u0001\u0001\u0001\u0001\u0002\u0001\u0002\u0001"+
		"\u0002\u0003\u0002G\b\u0002\u0001\u0002\u0001\u0002\u0001\u0003\u0001"+
		"\u0003\u0005\u0003M\b\u0003\n\u0003\f\u0003P\t\u0003\u0001\u0003\u0001"+
		"\u0003\u0001\u0003\u0003\u0003U\b\u0003\u0001\u0003\u0003\u0003X\b\u0003"+
		"\u0001\u0004\u0001\u0004\u0001\u0004\u0001\u0004\u0001\u0004\u0001\u0004"+
		"\u0003\u0004`\b\u0004\u0001\u0005\u0001\u0005\u0001\u0005\u0001\u0005"+
		"\u0003\u0005f\b\u0005\u0001\u0005\u0001\u0005\u0005\u0005j\b\u0005\n\u0005"+
		"\f\u0005m\t\u0005\u0001\u0006\u0005\u0006p\b\u0006\n\u0006\f\u0006s\t"+
		"\u0006\u0001\u0006\u0001\u0006\u0001\u0007\u0001\u0007\u0001\u0007\u0001"+
		"\u0007\u0003\u0007{\b\u0007\u0001\u0007\u0001\u0007\u0001\b\u0001\b\u0001"+
		"\t\u0001\t\u0001\t\u0005\t\u0084\b\t\n\t\f\t\u0087\t\t\u0001\t\u0001\t"+
		"\u0005\t\u008b\b\t\n\t\f\t\u008e\t\t\u0001\t\u0001\t\u0005\t\u0092\b\t"+
		"\n\t\f\t\u0095\t\t\u0001\t\u0001\t\u0005\t\u0099\b\t\n\t\f\t\u009c\t\t"+
		"\u0001\t\u0001\t\u0005\t\u00a0\b\t\n\t\f\t\u00a3\t\t\u0003\t\u00a5\b\t"+
		"\u0001\n\u0001\n\u0003\n\u00a9\b\n\u0001\u000b\u0001\u000b\u0001\u000b"+
		"\u0001\u000b\u0001\u000b\u0001\u000b\u0004\u000b\u00b1\b\u000b\u000b\u000b"+
		"\f\u000b\u00b2\u0003\u000b\u00b5\b\u000b\u0001\u000b\u0005\u000b\u00b8"+
		"\b\u000b\n\u000b\f\u000b\u00bb\t\u000b\u0001\u000b\u0003\u000b\u00be\b"+
		"\u000b\u0001\u000b\u0001\u000b\u0003\u000b\u00c2\b\u000b\u0001\u000b\u0001"+
		"\u000b\u0001\f\u0005\f\u00c7\b\f\n\f\f\f\u00ca\t\f\u0001\f\u0001\f\u0001"+
		"\f\u0001\f\u0003\f\u00d0\b\f\u0001\r\u0001\r\u0001\r\u0001\r\u0001\r\u0001"+
		"\r\u0004\r\u00d8\b\r\u000b\r\f\r\u00d9\u0003\r\u00dc\b\r\u0001\r\u0005"+
		"\r\u00df\b\r\n\r\f\r\u00e2\t\r\u0001\r\u0003\r\u00e5\b\r\u0001\r\u0001"+
		"\r\u0003\r\u00e9\b\r\u0001\r\u0001\r\u0001\u000e\u0003\u000e\u00ee\b\u000e"+
		"\u0001\u000e\u0001\u000e\u0001\u000f\u0001\u000f\u0001\u000f\u0001\u000f"+
		"\u0003\u000f\u00f6\b\u000f\u0001\u000f\u0001\u000f\u0001\u0010\u0001\u0010"+
		"\u0001\u0010\u0001\u0010\u0001\u0010\u0005\u0010\u00ff\b\u0010\n\u0010"+
		"\f\u0010\u0102\t\u0010\u0001\u0010\u0003\u0010\u0105\b\u0010\u0001\u0010"+
		"\u0001\u0010\u0001\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0003\u0011"+
		"\u010d\b\u0011\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0013\u0004\u0013"+
		"\u0113\b\u0013\u000b\u0013\f\u0013\u0114\u0001\u0014\u0001\u0014\u0001"+
		"\u0015\u0003\u0015\u011a\b\u0015\u0001\u0015\u0004\u0015\u011d\b\u0015"+
		"\u000b\u0015\f\u0015\u011e\u0001\u0015\u0001\u0015\u0003\u0015\u0123\b"+
		"\u0015\u0001\u0016\u0005\u0016\u0126\b\u0016\n\u0016\f\u0016\u0129\t\u0016"+
		"\u0001\u0016\u0000\u0000\u0017\u0000\u0002\u0004\u0006\b\n\f\u000e\u0010"+
		"\u0012\u0014\u0016\u0018\u001a\u001c\u001e \"$&(*,\u0000\u0002\u0001\u0000"+
		"\u0013\u0014\u0001\u0000\u000f\u0015\u0143\u0000.\u0001\u0000\u0000\u0000"+
		"\u0002>\u0001\u0000\u0000\u0000\u0004C\u0001\u0000\u0000\u0000\u0006W"+
		"\u0001\u0000\u0000\u0000\b_\u0001\u0000\u0000\u0000\na\u0001\u0000\u0000"+
		"\u0000\fq\u0001\u0000\u0000\u0000\u000ev\u0001\u0000\u0000\u0000\u0010"+
		"~\u0001\u0000\u0000\u0000\u0012\u00a4\u0001\u0000\u0000\u0000\u0014\u00a8"+
		"\u0001\u0000\u0000\u0000\u0016\u00aa\u0001\u0000\u0000\u0000\u0018\u00c8"+
		"\u0001\u0000\u0000\u0000\u001a\u00d1\u0001\u0000\u0000\u0000\u001c\u00ed"+
		"\u0001\u0000\u0000\u0000\u001e\u00f1\u0001\u0000\u0000\u0000 \u00f9\u0001"+
		"\u0000\u0000\u0000\"\u010c\u0001\u0000\u0000\u0000$\u010e\u0001\u0000"+
		"\u0000\u0000&\u0112\u0001\u0000\u0000\u0000(\u0116\u0001\u0000\u0000\u0000"+
		"*\u0122\u0001\u0000\u0000\u0000,\u0127\u0001\u0000\u0000\u0000./\u0003"+
		",\u0016\u0000/5\u0003\u0002\u0001\u000001\u0003,\u0016\u000012\u0003\u0002"+
		"\u0001\u000024\u0001\u0000\u0000\u000030\u0001\u0000\u0000\u000047\u0001"+
		"\u0000\u0000\u000053\u0001\u0000\u0000\u000056\u0001\u0000\u0000\u0000"+
		"68\u0001\u0000\u0000\u000075\u0001\u0000\u0000\u000089\u0003,\u0016\u0000"+
		"9:\u0005\u0000\u0000\u0001:\u0001\u0001\u0000\u0000\u0000;=\u0003\u0004"+
		"\u0002\u0000<;\u0001\u0000\u0000\u0000=@\u0001\u0000\u0000\u0000><\u0001"+
		"\u0000\u0000\u0000>?\u0001\u0000\u0000\u0000?A\u0001\u0000\u0000\u0000"+
		"@>\u0001\u0000\u0000\u0000AB\u0003\n\u0005\u0000B\u0003\u0001\u0000\u0000"+
		"\u0000CD\u0005\u0002\u0000\u0000DF\u0005\u0014\u0000\u0000EG\u0003\u0006"+
		"\u0003\u0000FE\u0001\u0000\u0000\u0000FG\u0001\u0000\u0000\u0000GH\u0001"+
		"\u0000\u0000\u0000HI\u0003,\u0016\u0000I\u0005\u0001\u0000\u0000\u0000"+
		"JN\u0005\r\u0000\u0000KM\u0003\b\u0004\u0000LK\u0001\u0000\u0000\u0000"+
		"MP\u0001\u0000\u0000\u0000NL\u0001\u0000\u0000\u0000NO\u0001\u0000\u0000"+
		"\u0000OQ\u0001\u0000\u0000\u0000PN\u0001\u0000\u0000\u0000QX\u0005\u000e"+
		"\u0000\u0000RT\u0005\u000b\u0000\u0000SU\u0003 \u0010\u0000TS\u0001\u0000"+
		"\u0000\u0000TU\u0001\u0000\u0000\u0000UV\u0001\u0000\u0000\u0000VX\u0005"+
		"\f\u0000\u0000WJ\u0001\u0000\u0000\u0000WR\u0001\u0000\u0000\u0000X\u0007"+
		"\u0001\u0000\u0000\u0000Y`\u0003(\u0014\u0000Z`\u0005\u0006\u0000\u0000"+
		"[`\u0005\u0005\u0000\u0000\\`\u0005\u0004\u0000\u0000]`\u0005\u0003\u0000"+
		"\u0000^`\u0005\u0002\u0000\u0000_Y\u0001\u0000\u0000\u0000_Z\u0001\u0000"+
		"\u0000\u0000_[\u0001\u0000\u0000\u0000_\\\u0001\u0000\u0000\u0000_]\u0001"+
		"\u0000\u0000\u0000_^\u0001\u0000\u0000\u0000`\t\u0001\u0000\u0000\u0000"+
		"ab\u0005\u0001\u0000\u0000be\u0005\u0014\u0000\u0000cd\u0005\u0003\u0000"+
		"\u0000df\u0003(\u0014\u0000ec\u0001\u0000\u0000\u0000ef\u0001\u0000\u0000"+
		"\u0000fg\u0001\u0000\u0000\u0000gk\u0003*\u0015\u0000hj\u0003\f\u0006"+
		"\u0000ih\u0001\u0000\u0000\u0000jm\u0001\u0000\u0000\u0000ki\u0001\u0000"+
		"\u0000\u0000kl\u0001\u0000\u0000\u0000l\u000b\u0001\u0000\u0000\u0000"+
		"mk\u0001\u0000\u0000\u0000np\u0003\u0004\u0002\u0000on\u0001\u0000\u0000"+
		"\u0000ps\u0001\u0000\u0000\u0000qo\u0001\u0000\u0000\u0000qr\u0001\u0000"+
		"\u0000\u0000rt\u0001\u0000\u0000\u0000sq\u0001\u0000\u0000\u0000tu\u0003"+
		"\u000e\u0007\u0000u\r\u0001\u0000\u0000\u0000vz\u0003\u0010\b\u0000wx"+
		"\u0005\u0005\u0000\u0000x{\u0003\u0012\t\u0000y{\u0003\u001c\u000e\u0000"+
		"zw\u0001\u0000\u0000\u0000zy\u0001\u0000\u0000\u0000{|\u0001\u0000\u0000"+
		"\u0000|}\u0003*\u0015\u0000}\u000f\u0001\u0000\u0000\u0000~\u007f\u0007"+
		"\u0000\u0000\u0000\u007f\u0011\u0001\u0000\u0000\u0000\u0080\u0081\u0005"+
		"\u0014\u0000\u0000\u0081\u0085\u0003\u0014\n\u0000\u0082\u0084\u0003$"+
		"\u0012\u0000\u0083\u0082\u0001\u0000\u0000\u0000\u0084\u0087\u0001\u0000"+
		"\u0000\u0000\u0085\u0083\u0001\u0000\u0000\u0000\u0085\u0086\u0001\u0000"+
		"\u0000\u0000\u0086\u00a5\u0001\u0000\u0000\u0000\u0087\u0085\u0001\u0000"+
		"\u0000\u0000\u0088\u008c\u0003\u001e\u000f\u0000\u0089\u008b\u0003$\u0012"+
		"\u0000\u008a\u0089\u0001\u0000\u0000\u0000\u008b\u008e\u0001\u0000\u0000"+
		"\u0000\u008c\u008a\u0001\u0000\u0000\u0000\u008c\u008d\u0001\u0000\u0000"+
		"\u0000\u008d\u00a5\u0001\u0000\u0000\u0000\u008e\u008c\u0001\u0000\u0000"+
		"\u0000\u008f\u0093\u0003\u0016\u000b\u0000\u0090\u0092\u0003$\u0012\u0000"+
		"\u0091\u0090\u0001\u0000\u0000\u0000\u0092\u0095\u0001\u0000\u0000\u0000"+
		"\u0093\u0091\u0001\u0000\u0000\u0000\u0093\u0094\u0001\u0000\u0000\u0000"+
		"\u0094\u00a5\u0001\u0000\u0000\u0000\u0095\u0093\u0001\u0000\u0000\u0000"+
		"\u0096\u009a\u0003\u001a\r\u0000\u0097\u0099\u0003$\u0012\u0000\u0098"+
		"\u0097\u0001\u0000\u0000\u0000\u0099\u009c\u0001\u0000\u0000\u0000\u009a"+
		"\u0098\u0001\u0000\u0000\u0000\u009a\u009b\u0001\u0000\u0000\u0000\u009b"+
		"\u00a5\u0001\u0000\u0000\u0000\u009c\u009a\u0001\u0000\u0000\u0000\u009d"+
		"\u00a1\u0003&\u0013\u0000\u009e\u00a0\u0003$\u0012\u0000\u009f\u009e\u0001"+
		"\u0000\u0000\u0000\u00a0\u00a3\u0001\u0000\u0000\u0000\u00a1\u009f\u0001"+
		"\u0000\u0000\u0000\u00a1\u00a2\u0001\u0000\u0000\u0000\u00a2\u00a5\u0001"+
		"\u0000\u0000\u0000\u00a3\u00a1\u0001\u0000\u0000\u0000\u00a4\u0080\u0001"+
		"\u0000\u0000\u0000\u00a4\u0088\u0001\u0000\u0000\u0000\u00a4\u008f\u0001"+
		"\u0000\u0000\u0000\u00a4\u0096\u0001\u0000\u0000\u0000\u00a4\u009d\u0001"+
		"\u0000\u0000\u0000\u00a5\u0013\u0001\u0000\u0000\u0000\u00a6\u00a9\u0003"+
		"\u0016\u000b\u0000\u00a7\u00a9\u0003\u001a\r\u0000\u00a8\u00a6\u0001\u0000"+
		"\u0000\u0000\u00a8\u00a7\u0001\u0000\u0000\u0000\u00a9\u0015\u0001\u0000"+
		"\u0000\u0000\u00aa\u00ab\u0005\u0007\u0000\u0000\u00ab\u00c1\u0003,\u0016"+
		"\u0000\u00ac\u00b9\u0003\u0018\f\u0000\u00ad\u00ae\u0005\u0006\u0000\u0000"+
		"\u00ae\u00b5\u0003,\u0016\u0000\u00af\u00b1\u0005\u0017\u0000\u0000\u00b0"+
		"\u00af\u0001\u0000\u0000\u0000\u00b1\u00b2\u0001\u0000\u0000\u0000\u00b2"+
		"\u00b0\u0001\u0000\u0000\u0000\u00b2\u00b3\u0001\u0000\u0000\u0000\u00b3"+
		"\u00b5\u0001\u0000\u0000\u0000\u00b4\u00ad\u0001\u0000\u0000\u0000\u00b4"+
		"\u00b0\u0001\u0000\u0000\u0000\u00b5\u00b6\u0001\u0000\u0000\u0000\u00b6"+
		"\u00b8\u0003\u0018\f\u0000\u00b7\u00b4\u0001\u0000\u0000\u0000\u00b8\u00bb"+
		"\u0001\u0000\u0000\u0000\u00b9\u00b7\u0001\u0000\u0000\u0000\u00b9\u00ba"+
		"\u0001\u0000\u0000\u0000\u00ba\u00bd\u0001\u0000\u0000\u0000\u00bb\u00b9"+
		"\u0001\u0000\u0000\u0000\u00bc\u00be\u0005\u0006\u0000\u0000\u00bd\u00bc"+
		"\u0001\u0000\u0000\u0000\u00bd\u00be\u0001\u0000\u0000\u0000\u00be\u00bf"+
		"\u0001\u0000\u0000\u0000\u00bf\u00c0\u0003,\u0016\u0000\u00c0\u00c2\u0001"+
		"\u0000\u0000\u0000\u00c1\u00ac\u0001\u0000\u0000\u0000\u00c1\u00c2\u0001"+
		"\u0000\u0000\u0000\u00c2\u00c3\u0001\u0000\u0000\u0000\u00c3\u00c4\u0005"+
		"\b\u0000\u0000\u00c4\u0017\u0001\u0000\u0000\u0000\u00c5\u00c7\u0003\u0004"+
		"\u0002\u0000\u00c6\u00c5\u0001\u0000\u0000\u0000\u00c7\u00ca\u0001\u0000"+
		"\u0000\u0000\u00c8\u00c6\u0001\u0000\u0000\u0000\u00c8\u00c9\u0001\u0000"+
		"\u0000\u0000\u00c9\u00cb\u0001\u0000\u0000\u0000\u00ca\u00c8\u0001\u0000"+
		"\u0000\u0000\u00cb\u00cf\u0003\u0010\b\u0000\u00cc\u00cd\u0005\u0005\u0000"+
		"\u0000\u00cd\u00d0\u0003\u0012\t\u0000\u00ce\u00d0\u0003\u001c\u000e\u0000"+
		"\u00cf\u00cc\u0001\u0000\u0000\u0000\u00cf\u00ce\u0001\u0000\u0000\u0000"+
		"\u00d0\u0019\u0001\u0000\u0000\u0000\u00d1\u00d2\u0005\t\u0000\u0000\u00d2"+
		"\u00e8\u0003,\u0016\u0000\u00d3\u00e0\u0003\u0012\t\u0000\u00d4\u00d5"+
		"\u0005\u0006\u0000\u0000\u00d5\u00dc\u0003,\u0016\u0000\u00d6\u00d8\u0005"+
		"\u0017\u0000\u0000\u00d7\u00d6\u0001\u0000\u0000\u0000\u00d8\u00d9\u0001"+
		"\u0000\u0000\u0000\u00d9\u00d7\u0001\u0000\u0000\u0000\u00d9\u00da\u0001"+
		"\u0000\u0000\u0000\u00da\u00dc\u0001\u0000\u0000\u0000\u00db\u00d4\u0001"+
		"\u0000\u0000\u0000\u00db\u00d7\u0001\u0000\u0000\u0000\u00dc\u00dd\u0001"+
		"\u0000\u0000\u0000\u00dd\u00df\u0003\u0012\t\u0000\u00de\u00db\u0001\u0000"+
		"\u0000\u0000\u00df\u00e2\u0001\u0000\u0000\u0000\u00e0\u00de\u0001\u0000"+
		"\u0000\u0000\u00e0\u00e1\u0001\u0000\u0000\u0000\u00e1\u00e4\u0001\u0000"+
		"\u0000\u0000\u00e2\u00e0\u0001\u0000\u0000\u0000\u00e3\u00e5\u0005\u0006"+
		"\u0000\u0000\u00e4\u00e3\u0001\u0000\u0000\u0000\u00e4\u00e5\u0001\u0000"+
		"\u0000\u0000\u00e5\u00e6\u0001\u0000\u0000\u0000\u00e6\u00e7\u0003,\u0016"+
		"\u0000\u00e7\u00e9\u0001\u0000\u0000\u0000\u00e8\u00d3\u0001\u0000\u0000"+
		"\u0000\u00e8\u00e9\u0001\u0000\u0000\u0000\u00e9\u00ea\u0001\u0000\u0000"+
		"\u0000\u00ea\u00eb\u0005\n\u0000\u0000\u00eb\u001b\u0001\u0000\u0000\u0000"+
		"\u00ec\u00ee\u0005\u0014\u0000\u0000\u00ed\u00ec\u0001\u0000\u0000\u0000"+
		"\u00ed\u00ee\u0001\u0000\u0000\u0000\u00ee\u00ef\u0001\u0000\u0000\u0000"+
		"\u00ef\u00f0\u0003\u0014\n\u0000\u00f0\u001d\u0001\u0000\u0000\u0000\u00f1"+
		"\u00f2\u0005\u0014\u0000\u0000\u00f2\u00f3\u0005\u000b\u0000\u0000\u00f3"+
		"\u00f5\u0003,\u0016\u0000\u00f4\u00f6\u0003 \u0010\u0000\u00f5\u00f4\u0001"+
		"\u0000\u0000\u0000\u00f5\u00f6\u0001\u0000\u0000\u0000\u00f6\u00f7\u0001"+
		"\u0000\u0000\u0000\u00f7\u00f8\u0005\f\u0000\u0000\u00f8\u001f\u0001\u0000"+
		"\u0000\u0000\u00f9\u0100\u0003\"\u0011\u0000\u00fa\u00fb\u0005\u0006\u0000"+
		"\u0000\u00fb\u00fc\u0003,\u0016\u0000\u00fc\u00fd\u0003\"\u0011\u0000"+
		"\u00fd\u00ff\u0001\u0000\u0000\u0000\u00fe\u00fa\u0001\u0000\u0000\u0000"+
		"\u00ff\u0102\u0001\u0000\u0000\u0000\u0100\u00fe\u0001\u0000\u0000\u0000"+
		"\u0100\u0101\u0001\u0000\u0000\u0000\u0101\u0104\u0001\u0000\u0000\u0000"+
		"\u0102\u0100\u0001\u0000\u0000\u0000\u0103\u0105\u0005\u0006\u0000\u0000"+
		"\u0104\u0103\u0001\u0000\u0000\u0000\u0104\u0105\u0001\u0000\u0000\u0000"+
		"\u0105\u0106\u0001\u0000\u0000\u0000\u0106\u0107\u0003,\u0016\u0000\u0107"+
		"!\u0001\u0000\u0000\u0000\u0108\u0109\u0005\u0014\u0000\u0000\u0109\u010a"+
		"\u0005\u0005\u0000\u0000\u010a\u010d\u0003\u0012\t\u0000\u010b\u010d\u0003"+
		"\u0012\t\u0000\u010c\u0108\u0001\u0000\u0000\u0000\u010c\u010b\u0001\u0000"+
		"\u0000\u0000\u010d#\u0001\u0000\u0000\u0000\u010e\u010f\u0005\u0004\u0000"+
		"\u0000\u010f\u0110\u0005\u0014\u0000\u0000\u0110%\u0001\u0000\u0000\u0000"+
		"\u0111\u0113\u0003(\u0014\u0000\u0112\u0111\u0001\u0000\u0000\u0000\u0113"+
		"\u0114\u0001\u0000\u0000\u0000\u0114\u0112\u0001\u0000\u0000\u0000\u0114"+
		"\u0115\u0001\u0000\u0000\u0000\u0115\'\u0001\u0000\u0000\u0000\u0116\u0117"+
		"\u0007\u0001\u0000\u0000\u0117)\u0001\u0000\u0000\u0000\u0118\u011a\u0005"+
		"\u0006\u0000\u0000\u0119\u0118\u0001\u0000\u0000\u0000\u0119\u011a\u0001"+
		"\u0000\u0000\u0000\u011a\u011c\u0001\u0000\u0000\u0000\u011b\u011d\u0005"+
		"\u0017\u0000\u0000\u011c\u011b\u0001\u0000\u0000\u0000\u011d\u011e\u0001"+
		"\u0000\u0000\u0000\u011e\u011c\u0001\u0000\u0000\u0000\u011e\u011f\u0001"+
		"\u0000\u0000\u0000\u011f\u0123\u0001\u0000\u0000\u0000\u0120\u0123\u0005"+
		"\u0006\u0000\u0000\u0121\u0123\u0005\u0000\u0000\u0001\u0122\u0119\u0001"+
		"\u0000\u0000\u0000\u0122\u0120\u0001\u0000\u0000\u0000\u0122\u0121\u0001"+
		"\u0000\u0000\u0000\u0123+\u0001\u0000\u0000\u0000\u0124\u0126\u0005\u0017"+
		"\u0000\u0000\u0125\u0124\u0001\u0000\u0000\u0000\u0126\u0129\u0001\u0000"+
		"\u0000\u0000\u0127\u0125\u0001\u0000\u0000\u0000\u0127\u0128\u0001\u0000"+
		"\u0000\u0000\u0128-\u0001\u0000\u0000\u0000\u0129\u0127\u0001\u0000\u0000"+
		"\u0000(5>FNTW_ekqz\u0085\u008c\u0093\u009a\u00a1\u00a4\u00a8\u00b2\u00b4"+
		"\u00b9\u00bd\u00c1\u00c8\u00cf\u00d9\u00db\u00e0\u00e4\u00e8\u00ed\u00f5"+
		"\u0100\u0104\u010c\u0114\u0119\u011e\u0122\u0127";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}