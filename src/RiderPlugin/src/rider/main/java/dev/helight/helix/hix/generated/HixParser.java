// Generated from HixParser.g4 by ANTLR 4.13.2
package dev.helight.helix.hix.generated;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast", "CheckReturnValue", "this-escape"})
public class HixParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.13.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		ERROR_TOKEN=1, TERMINATOR=2, ESCAPE=3, BEGIN_ARGUMENT=4, BEGIN_VALUE=5,
		BEGIN_VALUE_ESCAPED=6, BEGIN_CONTENT=7, BEGIN_VALUE_INTERPOLATE=8, BEGIN_VALUE_INLINE=9,
		IDENTIFIER=10, NAMESPACE_IDENTIFIER=11, VALUE_END=12, ROOT_IDENTIFIER=13,
		BEGIN_TABLE=14, BEGIN_TUPLE=15, BEGIN_LAMBDA_BLOCK=16, BEGIN_LAMBDA_ARROW=17,
		BEGIN_METADATA_VALUE=18, METADATA_PREFIX=19, FAT_ARROW=20, NUMBER=21,
		BOOLEAN=22, NULL=23, ESCAPED_AT=24, ESCAPED_START=25, OUTER_WHITESPACE=26,
		NEWLINE=27, COMMENT=28, SLASH_COMMENT=29, SECTION_DELIMITER=30, LC=31,
		RC=32, SEMICOLON=33, COMMA=34, KEYWORD_FUNC=35, KEYWORD_DO=36, KEYWORD_EXPRESSION=37,
		KEYWORD_MIXIN=38, KEYWORD_PRELUDE=39, KEYWORD_DERIVATION=40, KEYWORD_ELSE=41,
		KEYWORD_RETURN=42, KEYWORD_GOTO=43, KEYWORD_BREAK=44, KEYWORD_CONTINUE=45,
		KEYWORD_TARGET=46, KEYWORD_VAR=47, KEYWORD_LOCAL=48, KEYWORD_CARRY=49,
		KEYWORD_SIG=50, KEYWORD_WHEN=51, KEYWORD_PURE=52, KEYWORD_INLINE=53, KEYWORD_NOINLINE=54,
		KEYWORD_STRICT=55, LABEL_PREFIX=56, ASSIGN=57, ARROW=58, TOPLEVEL_VALUE_EXPRESSION=59,
		EMPTY_PARAMETERS=60, BEGIN_PARAMETERS=61, METADATA_WHITESPACE=62, ARGUMENT_TEXT=63,
		ARGUMENT_END=64, CONTENT_WRAP=65, CONTENT_LINEBREAK=66, CONTENT_TEXT=67,
		VALUE_FUNCTION=68, VALUE_PREDICATE=69, VALUE_MEMBER=70, VALUE_WRAP=71,
		VALUE_DELIMITER=72, END_PARAMETERS=73, VALUE_END_INLINE=74, VALUE_SMART_ROOT=75,
		NOT_VALUE=76, VALUE_CHECK=77, VALUE_ELVIS=78, VALUE_ASSIGN=79, VALUE_EXPAND=80,
		VALUE_WHITESPACE=81, ESCAPE_MACRO=82, ESCAPE_LITERAL=83, ESCAPE_HEX=84,
		END_CONTENT_INTERPOLATE=85, FUNCTION_IDENTIFIER=86, MEMBER_IDENTIFIER=87,
		LABEL_IDENTIFIER=88, TOPLEVEL_NULL=89;
	public static final int
		RULE_compilationUnit = 0, RULE_fileMetadataSection = 1, RULE_topLevelDeclaration = 2,
		RULE_metadataList = 3, RULE_metadata = 4, RULE_metadataValue = 5, RULE_mixinDeclaration = 6,
		RULE_mixinBody = 7, RULE_expressionDeclaration = 8, RULE_funcDeclaration = 9,
		RULE_functionBody = 10, RULE_functionMetadata = 11, RULE_functionSignatureVariant = 12,
		RULE_signature = 13, RULE_tableSignature = 14, RULE_tableSignatureEntry = 15,
		RULE_statementBlock = 16, RULE_statement = 17, RULE_invocationStatement = 18,
		RULE_whenConditionStatement = 19, RULE_whenElseBranch = 20, RULE_whenResult = 21,
		RULE_whenChainCondition = 22, RULE_whenChainStatement = 23, RULE_whenChainBody = 24,
		RULE_whenChainBranch = 25, RULE_whenValueStatement = 26, RULE_whenValueBody = 27,
		RULE_whenValueBranch = 28, RULE_whenValueCondition = 29, RULE_assignmentStatement = 30,
		RULE_assignedValue = 31, RULE_controlflowStatement = 32, RULE_contentBlock = 33,
		RULE_contentBody = 34, RULE_contentInterpolate = 35, RULE_value = 36,
		RULE_valueExpression = 37, RULE_nonArgumentValue = 38, RULE_primaryValue = 39,
		RULE_lambdaValue = 40, RULE_prefixOperators = 41, RULE_postfixOperators = 42,
		RULE_valueStatement = 43, RULE_tailValue = 44, RULE_tupleValue = 45, RULE_tableValue = 46,
		RULE_tableKeyedEntry = 47, RULE_valueList = 48, RULE_argumentValue = 49,
		RULE_argumentBody = 50, RULE_inlineValue = 51, RULE_inlineTransformation = 52,
		RULE_derivation = 53, RULE_derivationRoot = 54, RULE_elvisValue = 55,
		RULE_transformationPart = 56, RULE_functionChainType = 57, RULE_labelIdentifier = 58,
		RULE_memberIdentifier = 59, RULE_mixinIdentifier = 60, RULE_functionDeclarationIdentifier = 61,
		RULE_variableIdentifier = 62, RULE_functionIdentifier = 63, RULE_kindIdentifier = 64,
		RULE_expressionModifier = 65, RULE_mixinModifier = 66, RULE_variableSpecifiers = 67,
		RULE_funcModifier = 68, RULE_trivia = 69, RULE_comment = 70, RULE_escaped = 71;
	private static String[] makeRuleNames() {
		return new String[] {
			"compilationUnit", "fileMetadataSection", "topLevelDeclaration", "metadataList",
			"metadata", "metadataValue", "mixinDeclaration", "mixinBody", "expressionDeclaration",
			"funcDeclaration", "functionBody", "functionMetadata", "functionSignatureVariant",
			"signature", "tableSignature", "tableSignatureEntry", "statementBlock",
			"statement", "invocationStatement", "whenConditionStatement", "whenElseBranch",
			"whenResult", "whenChainCondition", "whenChainStatement", "whenChainBody",
			"whenChainBranch", "whenValueStatement", "whenValueBody", "whenValueBranch",
			"whenValueCondition", "assignmentStatement", "assignedValue", "controlflowStatement",
			"contentBlock", "contentBody", "contentInterpolate", "value", "valueExpression",
			"nonArgumentValue", "primaryValue", "lambdaValue", "prefixOperators",
			"postfixOperators", "valueStatement", "tailValue", "tupleValue", "tableValue",
			"tableKeyedEntry", "valueList", "argumentValue", "argumentBody", "inlineValue",
			"inlineTransformation", "derivation", "derivationRoot", "elvisValue",
			"transformationPart", "functionChainType", "labelIdentifier", "memberIdentifier",
			"mixinIdentifier", "functionDeclarationIdentifier", "variableIdentifier",
			"functionIdentifier", "kindIdentifier", "expressionModifier", "mixinModifier",
			"variableSpecifiers", "funcModifier", "trivia", "comment", "escaped"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null, null, null,
			"'@@'", "'\\'", null, null, null, null, "'---'", "'{'", null, null, null,
			"'func'", "'do'", "'expression'", "'mixin'", "'prelude'", "'derivation'",
			"'else'", "'return'", "'goto'", "'break'", "'continue'", "'target'",
			"'var'", "'local'", "'carry'", "'sig'", "'when'", "'pure'", "'inline'",
			"'noinline'", "'strict'", null, null, "'->'", null, null, "'('", null,
			null, "'>'", null, null, null, null, "':?'", "'#'", null, null, "')'",
			"']'", "'$'", "'!'", "'?'", "'?:'", null, "'...'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "ERROR_TOKEN", "TERMINATOR", "ESCAPE", "BEGIN_ARGUMENT", "BEGIN_VALUE",
			"BEGIN_VALUE_ESCAPED", "BEGIN_CONTENT", "BEGIN_VALUE_INTERPOLATE", "BEGIN_VALUE_INLINE",
			"IDENTIFIER", "NAMESPACE_IDENTIFIER", "VALUE_END", "ROOT_IDENTIFIER",
			"BEGIN_TABLE", "BEGIN_TUPLE", "BEGIN_LAMBDA_BLOCK", "BEGIN_LAMBDA_ARROW",
			"BEGIN_METADATA_VALUE", "METADATA_PREFIX", "FAT_ARROW", "NUMBER", "BOOLEAN",
			"NULL", "ESCAPED_AT", "ESCAPED_START", "OUTER_WHITESPACE", "NEWLINE",
			"COMMENT", "SLASH_COMMENT", "SECTION_DELIMITER", "LC", "RC", "SEMICOLON",
			"COMMA", "KEYWORD_FUNC", "KEYWORD_DO", "KEYWORD_EXPRESSION", "KEYWORD_MIXIN",
			"KEYWORD_PRELUDE", "KEYWORD_DERIVATION", "KEYWORD_ELSE", "KEYWORD_RETURN",
			"KEYWORD_GOTO", "KEYWORD_BREAK", "KEYWORD_CONTINUE", "KEYWORD_TARGET",
			"KEYWORD_VAR", "KEYWORD_LOCAL", "KEYWORD_CARRY", "KEYWORD_SIG", "KEYWORD_WHEN",
			"KEYWORD_PURE", "KEYWORD_INLINE", "KEYWORD_NOINLINE", "KEYWORD_STRICT",
			"LABEL_PREFIX", "ASSIGN", "ARROW", "TOPLEVEL_VALUE_EXPRESSION", "EMPTY_PARAMETERS",
			"BEGIN_PARAMETERS", "METADATA_WHITESPACE", "ARGUMENT_TEXT", "ARGUMENT_END",
			"CONTENT_WRAP", "CONTENT_LINEBREAK", "CONTENT_TEXT", "VALUE_FUNCTION",
			"VALUE_PREDICATE", "VALUE_MEMBER", "VALUE_WRAP", "VALUE_DELIMITER", "END_PARAMETERS",
			"VALUE_END_INLINE", "VALUE_SMART_ROOT", "NOT_VALUE", "VALUE_CHECK", "VALUE_ELVIS",
			"VALUE_ASSIGN", "VALUE_EXPAND", "VALUE_WHITESPACE", "ESCAPE_MACRO", "ESCAPE_LITERAL",
			"ESCAPE_HEX", "END_CONTENT_INTERPOLATE", "FUNCTION_IDENTIFIER", "MEMBER_IDENTIFIER",
			"LABEL_IDENTIFIER", "TOPLEVEL_NULL"
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
	public String getGrammarFileName() { return "HixParser.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public HixParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@SuppressWarnings("CheckReturnValue")
	public static class CompilationUnitContext extends ParserRuleContext {
		public TerminalNode EOF() { return getToken(HixParser.EOF, 0); }
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
		}
		public FileMetadataSectionContext fileMetadataSection() {
			return getRuleContext(FileMetadataSectionContext.class,0);
		}
		public List<TopLevelDeclarationContext> topLevelDeclaration() {
			return getRuleContexts(TopLevelDeclarationContext.class);
		}
		public TopLevelDeclarationContext topLevelDeclaration(int i) {
			return getRuleContext(TopLevelDeclarationContext.class,i);
		}
		public CompilationUnitContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_compilationUnit; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitCompilationUnit(this);
			else return visitor.visitChildren(this);
		}
	}

	public final CompilationUnitContext compilationUnit() throws RecognitionException {
		CompilationUnitContext _localctx = new CompilationUnitContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_compilationUnit);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(147);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(144);
				trivia();
				}
				}
				setState(149);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(151);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,1,_ctx) ) {
			case 1:
				{
				setState(150);
				fileMetadataSection();
				}
				break;
			}
			setState(162);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 31526606141652992L) != 0)) {
				{
				{
				setState(153);
				topLevelDeclaration();
				setState(157);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
					{
					{
					setState(154);
					trivia();
					}
					}
					setState(159);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
				}
				setState(164);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(165);
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
	public static class FileMetadataSectionContext extends ParserRuleContext {
		public MetadataListContext metadataList() {
			return getRuleContext(MetadataListContext.class,0);
		}
		public TerminalNode SECTION_DELIMITER() { return getToken(HixParser.SECTION_DELIMITER, 0); }
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
		}
		public FileMetadataSectionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_fileMetadataSection; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitFileMetadataSection(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FileMetadataSectionContext fileMetadataSection() throws RecognitionException {
		FileMetadataSectionContext _localctx = new FileMetadataSectionContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_fileMetadataSection);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(167);
			metadataList();
			setState(171);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(168);
				trivia();
				}
				}
				setState(173);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(174);
			match(SECTION_DELIMITER);
			setState(178);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(175);
				trivia();
				}
				}
				setState(180);
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

	@SuppressWarnings("CheckReturnValue")
	public static class TopLevelDeclarationContext extends ParserRuleContext {
		public MetadataListContext metadataList() {
			return getRuleContext(MetadataListContext.class,0);
		}
		public MixinDeclarationContext mixinDeclaration() {
			return getRuleContext(MixinDeclarationContext.class,0);
		}
		public FuncDeclarationContext funcDeclaration() {
			return getRuleContext(FuncDeclarationContext.class,0);
		}
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
		}
		public TopLevelDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_topLevelDeclaration; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTopLevelDeclaration(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TopLevelDeclarationContext topLevelDeclaration() throws RecognitionException {
		TopLevelDeclarationContext _localctx = new TopLevelDeclarationContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_topLevelDeclaration);
		int _la;
		try {
			setState(194);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(181);
				metadataList();
				setState(185);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
					{
					{
					setState(182);
					trivia();
					}
					}
					setState(187);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(190);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_MIXIN:
				case KEYWORD_DERIVATION:
					{
					setState(188);
					mixinDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(189);
					funcDeclaration();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				break;
			case KEYWORD_MIXIN:
			case KEYWORD_DERIVATION:
				enterOuterAlt(_localctx, 2);
				{
				setState(192);
				mixinDeclaration();
				}
				break;
			case KEYWORD_FUNC:
			case KEYWORD_PURE:
			case KEYWORD_INLINE:
			case KEYWORD_NOINLINE:
				enterOuterAlt(_localctx, 3);
				{
				setState(193);
				funcDeclaration();
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
	public static class MetadataListContext extends ParserRuleContext {
		public List<MetadataContext> metadata() {
			return getRuleContexts(MetadataContext.class);
		}
		public MetadataContext metadata(int i) {
			return getRuleContext(MetadataContext.class,i);
		}
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
		}
		public MetadataListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_metadataList; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitMetadataList(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MetadataListContext metadataList() throws RecognitionException {
		MetadataListContext _localctx = new MetadataListContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_metadataList);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(196);
			metadata();
			setState(206);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,10,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(200);
					_errHandler.sync(this);
					_la = _input.LA(1);
					while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
						{
						{
						setState(197);
						trivia();
						}
						}
						setState(202);
						_errHandler.sync(this);
						_la = _input.LA(1);
					}
					setState(203);
					metadata();
					}
					}
				}
				setState(208);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,10,_ctx);
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
	public static class MetadataContext extends ParserRuleContext {
		public TerminalNode METADATA_PREFIX() { return getToken(HixParser.METADATA_PREFIX, 0); }
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public ValueListContext valueList() {
			return getRuleContext(ValueListContext.class,0);
		}
		public MetadataValueContext metadataValue() {
			return getRuleContext(MetadataValueContext.class,0);
		}
		public MetadataContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_metadata; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitMetadata(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MetadataContext metadata() throws RecognitionException {
		MetadataContext _localctx = new MetadataContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_metadata);
		int _la;
		try {
			setState(215);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(209);
				match(METADATA_PREFIX);
				setState(210);
				match(IDENTIFIER);
				setState(212);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3458764513820540944L) != 0)) {
					{
					setState(211);
					valueList();
					}
				}

				}
				break;
			case BEGIN_METADATA_VALUE:
				enterOuterAlt(_localctx, 2);
				{
				setState(214);
				metadataValue();
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
	public static class MetadataValueContext extends ParserRuleContext {
		public TerminalNode BEGIN_METADATA_VALUE() { return getToken(HixParser.BEGIN_METADATA_VALUE, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public TerminalNode VALUE_END_INLINE() { return getToken(HixParser.VALUE_END_INLINE, 0); }
		public MetadataValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_metadataValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitMetadataValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MetadataValueContext metadataValue() throws RecognitionException {
		MetadataValueContext _localctx = new MetadataValueContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_metadataValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(217);
			match(BEGIN_METADATA_VALUE);
			setState(218);
			value();
			setState(219);
			match(VALUE_END_INLINE);
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
	public static class MixinDeclarationContext extends ParserRuleContext {
		public TerminalNode KEYWORD_MIXIN() { return getToken(HixParser.KEYWORD_MIXIN, 0); }
		public MixinIdentifierContext mixinIdentifier() {
			return getRuleContext(MixinIdentifierContext.class,0);
		}
		public MixinBodyContext mixinBody() {
			return getRuleContext(MixinBodyContext.class,0);
		}
		public List<MixinModifierContext> mixinModifier() {
			return getRuleContexts(MixinModifierContext.class);
		}
		public MixinModifierContext mixinModifier(int i) {
			return getRuleContext(MixinModifierContext.class,i);
		}
		public MixinDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_mixinDeclaration; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitMixinDeclaration(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MixinDeclarationContext mixinDeclaration() throws RecognitionException {
		MixinDeclarationContext _localctx = new MixinDeclarationContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_mixinDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(224);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_DERIVATION) {
				{
				{
				setState(221);
				mixinModifier();
				}
				}
				setState(226);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(227);
			match(KEYWORD_MIXIN);
			setState(228);
			mixinIdentifier();
			setState(229);
			mixinBody();
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
	public static class MixinBodyContext extends ParserRuleContext {
		public TerminalNode LC() { return getToken(HixParser.LC, 0); }
		public TerminalNode RC() { return getToken(HixParser.RC, 0); }
		public List<ExpressionDeclarationContext> expressionDeclaration() {
			return getRuleContexts(ExpressionDeclarationContext.class);
		}
		public ExpressionDeclarationContext expressionDeclaration(int i) {
			return getRuleContext(ExpressionDeclarationContext.class,i);
		}
		public List<FuncDeclarationContext> funcDeclaration() {
			return getRuleContexts(FuncDeclarationContext.class);
		}
		public FuncDeclarationContext funcDeclaration(int i) {
			return getRuleContext(FuncDeclarationContext.class,i);
		}
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
		}
		public MixinBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_mixinBody; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitMixinBody(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MixinBodyContext mixinBody() throws RecognitionException {
		MixinBodyContext _localctx = new MixinBodyContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_mixinBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(231);
			match(LC);
			setState(237);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 67554716904587264L) != 0)) {
				{
				setState(235);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_EXPRESSION:
				case KEYWORD_PRELUDE:
				case KEYWORD_STRICT:
					{
					setState(232);
					expressionDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(233);
					funcDeclaration();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(234);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(239);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(240);
			match(RC);
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
	public static class ExpressionDeclarationContext extends ParserRuleContext {
		public TerminalNode KEYWORD_EXPRESSION() { return getToken(HixParser.KEYWORD_EXPRESSION, 0); }
		public StatementBlockContext statementBlock() {
			return getRuleContext(StatementBlockContext.class,0);
		}
		public List<ExpressionModifierContext> expressionModifier() {
			return getRuleContexts(ExpressionModifierContext.class);
		}
		public ExpressionModifierContext expressionModifier(int i) {
			return getRuleContext(ExpressionModifierContext.class,i);
		}
		public ExpressionDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_expressionDeclaration; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitExpressionDeclaration(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ExpressionDeclarationContext expressionDeclaration() throws RecognitionException {
		ExpressionDeclarationContext _localctx = new ExpressionDeclarationContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_expressionDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(245);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_PRELUDE || _la==KEYWORD_STRICT) {
				{
				{
				setState(242);
				expressionModifier();
				}
				}
				setState(247);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(248);
			match(KEYWORD_EXPRESSION);
			setState(249);
			statementBlock();
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
	public static class FuncDeclarationContext extends ParserRuleContext {
		public TerminalNode KEYWORD_FUNC() { return getToken(HixParser.KEYWORD_FUNC, 0); }
		public FunctionDeclarationIdentifierContext functionDeclarationIdentifier() {
			return getRuleContext(FunctionDeclarationIdentifierContext.class,0);
		}
		public FunctionMetadataContext functionMetadata() {
			return getRuleContext(FunctionMetadataContext.class,0);
		}
		public FunctionBodyContext functionBody() {
			return getRuleContext(FunctionBodyContext.class,0);
		}
		public List<FuncModifierContext> funcModifier() {
			return getRuleContexts(FuncModifierContext.class);
		}
		public FuncModifierContext funcModifier(int i) {
			return getRuleContext(FuncModifierContext.class,i);
		}
		public FuncDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_funcDeclaration; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitFuncDeclaration(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FuncDeclarationContext funcDeclaration() throws RecognitionException {
		FuncDeclarationContext _localctx = new FuncDeclarationContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_funcDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(254);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 31525197391593472L) != 0)) {
				{
				{
				setState(251);
				funcModifier();
				}
				}
				setState(256);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(257);
			match(KEYWORD_FUNC);
			setState(258);
			functionDeclarationIdentifier();
			setState(259);
			functionMetadata();
			setState(260);
			functionBody();
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
	public static class FunctionBodyContext extends ParserRuleContext {
		public StatementBlockContext statementBlock() {
			return getRuleContext(StatementBlockContext.class,0);
		}
		public TerminalNode KEYWORD_DO() { return getToken(HixParser.KEYWORD_DO, 0); }
		public TerminalNode FAT_ARROW() { return getToken(HixParser.FAT_ARROW, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public TerminalNode VALUE_END() { return getToken(HixParser.VALUE_END, 0); }
		public FunctionBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_functionBody; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitFunctionBody(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FunctionBodyContext functionBody() throws RecognitionException {
		FunctionBodyContext _localctx = new FunctionBodyContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_functionBody);
		int _la;
		try {
			setState(271);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case LC:
			case KEYWORD_DO:
				enterOuterAlt(_localctx, 1);
				{
				setState(263);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_DO) {
					{
					setState(262);
					match(KEYWORD_DO);
					}
				}

				setState(265);
				statementBlock();
				}
				break;
			case FAT_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(266);
				match(FAT_ARROW);
				setState(267);
				value();
				setState(269);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_END) {
					{
					setState(268);
					match(VALUE_END);
					}
				}

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
	public static class FunctionMetadataContext extends ParserRuleContext {
		public List<FunctionSignatureVariantContext> functionSignatureVariant() {
			return getRuleContexts(FunctionSignatureVariantContext.class);
		}
		public FunctionSignatureVariantContext functionSignatureVariant(int i) {
			return getRuleContext(FunctionSignatureVariantContext.class,i);
		}
		public List<TerminalNode> NEWLINE() { return getTokens(HixParser.NEWLINE); }
		public TerminalNode NEWLINE(int i) {
			return getToken(HixParser.NEWLINE, i);
		}
		public FunctionMetadataContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_functionMetadata; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitFunctionMetadata(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FunctionMetadataContext functionMetadata() throws RecognitionException {
		FunctionMetadataContext _localctx = new FunctionMetadataContext(_ctx, getState());
		enterRule(_localctx, 22, RULE_functionMetadata);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(277);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==NEWLINE || _la==KEYWORD_SIG) {
				{
				setState(275);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_SIG:
					{
					setState(273);
					functionSignatureVariant();
					}
					break;
				case NEWLINE:
					{
					setState(274);
					match(NEWLINE);
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(279);
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

	@SuppressWarnings("CheckReturnValue")
	public static class FunctionSignatureVariantContext extends ParserRuleContext {
		public TerminalNode KEYWORD_SIG() { return getToken(HixParser.KEYWORD_SIG, 0); }
		public List<SignatureContext> signature() {
			return getRuleContexts(SignatureContext.class);
		}
		public SignatureContext signature(int i) {
			return getRuleContext(SignatureContext.class,i);
		}
		public TerminalNode ARROW() { return getToken(HixParser.ARROW, 0); }
		public FunctionSignatureVariantContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_functionSignatureVariant; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitFunctionSignatureVariant(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FunctionSignatureVariantContext functionSignatureVariant() throws RecognitionException {
		FunctionSignatureVariantContext _localctx = new FunctionSignatureVariantContext(_ctx, getState());
		enterRule(_localctx, 24, RULE_functionSignatureVariant);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(280);
			match(KEYWORD_SIG);
			setState(281);
			signature();
			setState(282);
			match(ARROW);
			setState(283);
			signature();
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
	public static class SignatureContext extends ParserRuleContext {
		public TableSignatureContext tableSignature() {
			return getRuleContext(TableSignatureContext.class,0);
		}
		public KindIdentifierContext kindIdentifier() {
			return getRuleContext(KindIdentifierContext.class,0);
		}
		public SignatureContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_signature; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitSignature(this);
			else return visitor.visitChildren(this);
		}
	}

	public final SignatureContext signature() throws RecognitionException {
		SignatureContext _localctx = new SignatureContext(_ctx, getState());
		enterRule(_localctx, 26, RULE_signature);
		try {
			setState(287);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_TABLE:
				enterOuterAlt(_localctx, 1);
				{
				setState(285);
				tableSignature();
				}
				break;
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case NULL:
				enterOuterAlt(_localctx, 2);
				{
				setState(286);
				kindIdentifier();
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
	public static class TableSignatureContext extends ParserRuleContext {
		public TerminalNode BEGIN_TABLE() { return getToken(HixParser.BEGIN_TABLE, 0); }
		public TerminalNode RC() { return getToken(HixParser.RC, 0); }
		public List<TableSignatureEntryContext> tableSignatureEntry() {
			return getRuleContexts(TableSignatureEntryContext.class);
		}
		public TableSignatureEntryContext tableSignatureEntry(int i) {
			return getRuleContext(TableSignatureEntryContext.class,i);
		}
		public List<TerminalNode> VALUE_DELIMITER() { return getTokens(HixParser.VALUE_DELIMITER); }
		public TerminalNode VALUE_DELIMITER(int i) {
			return getToken(HixParser.VALUE_DELIMITER, i);
		}
		public TableSignatureContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tableSignature; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTableSignature(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TableSignatureContext tableSignature() throws RecognitionException {
		TableSignatureContext _localctx = new TableSignatureContext(_ctx, getState());
		enterRule(_localctx, 28, RULE_tableSignature);
		int _la;
		try {
			int _alt;
			setState(305);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,26,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(289);
				match(BEGIN_TABLE);
				setState(290);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(291);
				match(BEGIN_TABLE);
				setState(292);
				tableSignatureEntry();
				setState(297);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,24,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(293);
						match(VALUE_DELIMITER);
						setState(294);
						tableSignatureEntry();
						}
						}
					}
					setState(299);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,24,_ctx);
				}
				setState(301);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(300);
					match(VALUE_DELIMITER);
					}
				}

				setState(303);
				match(RC);
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
	public static class TableSignatureEntryContext extends ParserRuleContext {
		public TerminalNode ROOT_IDENTIFIER() { return getToken(HixParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode VALUE_ASSIGN() { return getToken(HixParser.VALUE_ASSIGN, 0); }
		public KindIdentifierContext kindIdentifier() {
			return getRuleContext(KindIdentifierContext.class,0);
		}
		public List<MetadataContext> metadata() {
			return getRuleContexts(MetadataContext.class);
		}
		public MetadataContext metadata(int i) {
			return getRuleContext(MetadataContext.class,i);
		}
		public TerminalNode VALUE_EXPAND() { return getToken(HixParser.VALUE_EXPAND, 0); }
		public TableSignatureEntryContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tableSignatureEntry; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTableSignatureEntry(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TableSignatureEntryContext tableSignatureEntry() throws RecognitionException {
		TableSignatureEntryContext _localctx = new TableSignatureEntryContext(_ctx, getState());
		enterRule(_localctx, 30, RULE_tableSignatureEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(310);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				{
				setState(307);
				metadata();
				}
				}
				setState(312);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(314);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==VALUE_EXPAND) {
				{
				setState(313);
				match(VALUE_EXPAND);
				}
			}

			setState(316);
			match(ROOT_IDENTIFIER);
			setState(317);
			match(VALUE_ASSIGN);
			setState(318);
			kindIdentifier();
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
	public static class StatementBlockContext extends ParserRuleContext {
		public TerminalNode LC() { return getToken(HixParser.LC, 0); }
		public TerminalNode RC() { return getToken(HixParser.RC, 0); }
		public List<StatementContext> statement() {
			return getRuleContexts(StatementContext.class);
		}
		public StatementContext statement(int i) {
			return getRuleContext(StatementContext.class,i);
		}
		public List<TerminalNode> SEMICOLON() { return getTokens(HixParser.SEMICOLON); }
		public TerminalNode SEMICOLON(int i) {
			return getToken(HixParser.SEMICOLON, i);
		}
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
		}
		public StatementBlockContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_statementBlock; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitStatementBlock(this);
			else return visitor.visitChildren(this);
		}
	}

	public final StatementBlockContext statementBlock() throws RecognitionException {
		StatementBlockContext _localctx = new StatementBlockContext(_ctx, getState());
		enterRule(_localctx, 32, RULE_statementBlock);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(320);
			match(LC);
			setState(326);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 75430907388888064L) != 0)) {
				{
				setState(324);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case IDENTIFIER:
				case LC:
				case KEYWORD_RETURN:
				case KEYWORD_GOTO:
				case KEYWORD_BREAK:
				case KEYWORD_CONTINUE:
				case KEYWORD_TARGET:
				case KEYWORD_VAR:
				case KEYWORD_LOCAL:
				case KEYWORD_CARRY:
				case KEYWORD_WHEN:
				case LABEL_PREFIX:
					{
					setState(321);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(322);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(323);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(328);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(329);
			match(RC);
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
	public static class StatementContext extends ParserRuleContext {
		public LabelIdentifierContext labelIdentifier() {
			return getRuleContext(LabelIdentifierContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(HixParser.NEWLINE, 0); }
		public StatementBlockContext statementBlock() {
			return getRuleContext(StatementBlockContext.class,0);
		}
		public InvocationStatementContext invocationStatement() {
			return getRuleContext(InvocationStatementContext.class,0);
		}
		public AssignmentStatementContext assignmentStatement() {
			return getRuleContext(AssignmentStatementContext.class,0);
		}
		public ControlflowStatementContext controlflowStatement() {
			return getRuleContext(ControlflowStatementContext.class,0);
		}
		public WhenValueStatementContext whenValueStatement() {
			return getRuleContext(WhenValueStatementContext.class,0);
		}
		public WhenConditionStatementContext whenConditionStatement() {
			return getRuleContext(WhenConditionStatementContext.class,0);
		}
		public WhenChainStatementContext whenChainStatement() {
			return getRuleContext(WhenChainStatementContext.class,0);
		}
		public StatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_statement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final StatementContext statement() throws RecognitionException {
		StatementContext _localctx = new StatementContext(_ctx, getState());
		enterRule(_localctx, 34, RULE_statement);
		int _la;
		try {
			setState(344);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,32,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(331);
				labelIdentifier();
				setState(332);
				match(NEWLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(335);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LABEL_PREFIX) {
					{
					setState(334);
					labelIdentifier();
					}
				}

				setState(337);
				statementBlock();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(338);
				invocationStatement();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(339);
				assignmentStatement();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(340);
				controlflowStatement();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(341);
				whenValueStatement();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(342);
				whenConditionStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(343);
				whenChainStatement();
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
	public static class InvocationStatementContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public ValueListContext valueList() {
			return getRuleContext(ValueListContext.class,0);
		}
		public TailValueContext tailValue() {
			return getRuleContext(TailValueContext.class,0);
		}
		public InvocationStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_invocationStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitInvocationStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final InvocationStatementContext invocationStatement() throws RecognitionException {
		InvocationStatementContext _localctx = new InvocationStatementContext(_ctx, getState());
		enterRule(_localctx, 36, RULE_invocationStatement);
		try {
			setState(353);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,34,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(346);
				match(IDENTIFIER);
				setState(347);
				valueList();
				setState(349);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,33,_ctx) ) {
				case 1:
					{
					setState(348);
					tailValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(351);
				match(IDENTIFIER);
				setState(352);
				tailValue();
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
	public static class WhenConditionStatementContext extends ParserRuleContext {
		public TerminalNode KEYWORD_WHEN() { return getToken(HixParser.KEYWORD_WHEN, 0); }
		public WhenChainConditionContext whenChainCondition() {
			return getRuleContext(WhenChainConditionContext.class,0);
		}
		public StatementBlockContext statementBlock() {
			return getRuleContext(StatementBlockContext.class,0);
		}
		public TerminalNode KEYWORD_ELSE() { return getToken(HixParser.KEYWORD_ELSE, 0); }
		public WhenResultContext whenResult() {
			return getRuleContext(WhenResultContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(HixParser.NEWLINE, 0); }
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
		}
		public WhenConditionStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenConditionStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitWhenConditionStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenConditionStatementContext whenConditionStatement() throws RecognitionException {
		WhenConditionStatementContext _localctx = new WhenConditionStatementContext(_ctx, getState());
		enterRule(_localctx, 38, RULE_whenConditionStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(355);
			match(KEYWORD_WHEN);
			setState(356);
			whenChainCondition();
			setState(357);
			statementBlock();
			setState(368);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,36,_ctx) ) {
			case 1:
				{
				setState(361);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
					{
					{
					setState(358);
					trivia();
					}
					}
					setState(363);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(364);
				match(KEYWORD_ELSE);
				setState(365);
				whenResult();
				setState(366);
				match(NEWLINE);
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
	public static class WhenElseBranchContext extends ParserRuleContext {
		public TerminalNode KEYWORD_ELSE() { return getToken(HixParser.KEYWORD_ELSE, 0); }
		public TerminalNode ARROW() { return getToken(HixParser.ARROW, 0); }
		public WhenResultContext whenResult() {
			return getRuleContext(WhenResultContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(HixParser.NEWLINE, 0); }
		public WhenElseBranchContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenElseBranch; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitWhenElseBranch(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenElseBranchContext whenElseBranch() throws RecognitionException {
		WhenElseBranchContext _localctx = new WhenElseBranchContext(_ctx, getState());
		enterRule(_localctx, 40, RULE_whenElseBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(370);
			match(KEYWORD_ELSE);
			setState(371);
			match(ARROW);
			setState(372);
			whenResult();
			setState(373);
			match(NEWLINE);
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
	public static class WhenResultContext extends ParserRuleContext {
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public StatementContext statement() {
			return getRuleContext(StatementContext.class,0);
		}
		public WhenResultContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenResult; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitWhenResult(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenResultContext whenResult() throws RecognitionException {
		WhenResultContext _localctx = new WhenResultContext(_ctx, getState());
		enterRule(_localctx, 42, RULE_whenResult);
		try {
			setState(377);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_ARGUMENT:
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case BEGIN_LAMBDA_BLOCK:
			case BEGIN_LAMBDA_ARROW:
			case NUMBER:
			case BOOLEAN:
			case NULL:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(375);
				value();
				}
				break;
			case IDENTIFIER:
			case LC:
			case KEYWORD_RETURN:
			case KEYWORD_GOTO:
			case KEYWORD_BREAK:
			case KEYWORD_CONTINUE:
			case KEYWORD_TARGET:
			case KEYWORD_VAR:
			case KEYWORD_LOCAL:
			case KEYWORD_CARRY:
			case KEYWORD_WHEN:
			case LABEL_PREFIX:
				enterOuterAlt(_localctx, 2);
				{
				setState(376);
				statement();
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
	public static class WhenChainConditionContext extends ParserRuleContext {
		public ValueListContext valueList() {
			return getRuleContext(ValueListContext.class,0);
		}
		public WhenChainConditionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenChainCondition; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitWhenChainCondition(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenChainConditionContext whenChainCondition() throws RecognitionException {
		WhenChainConditionContext _localctx = new WhenChainConditionContext(_ctx, getState());
		enterRule(_localctx, 44, RULE_whenChainCondition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(379);
			valueList();
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
	public static class WhenChainStatementContext extends ParserRuleContext {
		public TerminalNode KEYWORD_WHEN() { return getToken(HixParser.KEYWORD_WHEN, 0); }
		public TerminalNode LC() { return getToken(HixParser.LC, 0); }
		public TerminalNode NEWLINE() { return getToken(HixParser.NEWLINE, 0); }
		public WhenChainBodyContext whenChainBody() {
			return getRuleContext(WhenChainBodyContext.class,0);
		}
		public TerminalNode RC() { return getToken(HixParser.RC, 0); }
		public WhenChainStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenChainStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitWhenChainStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenChainStatementContext whenChainStatement() throws RecognitionException {
		WhenChainStatementContext _localctx = new WhenChainStatementContext(_ctx, getState());
		enterRule(_localctx, 46, RULE_whenChainStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(381);
			match(KEYWORD_WHEN);
			setState(382);
			match(LC);
			setState(383);
			match(NEWLINE);
			setState(384);
			whenChainBody();
			setState(385);
			match(RC);
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
	public static class WhenChainBodyContext extends ParserRuleContext {
		public List<WhenChainBranchContext> whenChainBranch() {
			return getRuleContexts(WhenChainBranchContext.class);
		}
		public WhenChainBranchContext whenChainBranch(int i) {
			return getRuleContext(WhenChainBranchContext.class,i);
		}
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
		}
		public WhenElseBranchContext whenElseBranch() {
			return getRuleContext(WhenElseBranchContext.class,0);
		}
		public WhenChainBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenChainBody; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitWhenChainBody(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenChainBodyContext whenChainBody() throws RecognitionException {
		WhenChainBodyContext _localctx = new WhenChainBodyContext(_ctx, getState());
		enterRule(_localctx, 48, RULE_whenChainBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(387);
			whenChainBranch();
			setState(392);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3458764514760065040L) != 0)) {
				{
				setState(390);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case EMPTY_PARAMETERS:
				case BEGIN_PARAMETERS:
					{
					setState(388);
					whenChainBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(389);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(394);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(396);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(395);
				whenElseBranch();
				}
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
	public static class WhenChainBranchContext extends ParserRuleContext {
		public WhenChainConditionContext whenChainCondition() {
			return getRuleContext(WhenChainConditionContext.class,0);
		}
		public TerminalNode ARROW() { return getToken(HixParser.ARROW, 0); }
		public WhenResultContext whenResult() {
			return getRuleContext(WhenResultContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(HixParser.NEWLINE, 0); }
		public WhenChainBranchContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenChainBranch; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitWhenChainBranch(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenChainBranchContext whenChainBranch() throws RecognitionException {
		WhenChainBranchContext _localctx = new WhenChainBranchContext(_ctx, getState());
		enterRule(_localctx, 50, RULE_whenChainBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(398);
			whenChainCondition();
			setState(399);
			match(ARROW);
			setState(400);
			whenResult();
			setState(401);
			match(NEWLINE);
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
	public static class WhenValueStatementContext extends ParserRuleContext {
		public TerminalNode KEYWORD_WHEN() { return getToken(HixParser.KEYWORD_WHEN, 0); }
		public TerminalNode LC() { return getToken(HixParser.LC, 0); }
		public TerminalNode NEWLINE() { return getToken(HixParser.NEWLINE, 0); }
		public WhenValueBodyContext whenValueBody() {
			return getRuleContext(WhenValueBodyContext.class,0);
		}
		public TerminalNode RC() { return getToken(HixParser.RC, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public WhenValueStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenValueStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitWhenValueStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenValueStatementContext whenValueStatement() throws RecognitionException {
		WhenValueStatementContext _localctx = new WhenValueStatementContext(_ctx, getState());
		enterRule(_localctx, 52, RULE_whenValueStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(403);
			match(KEYWORD_WHEN);
			setState(405);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 576460752318358032L) != 0) || ((((_la - 75)) & ~0x3f) == 0 && ((1L << (_la - 75)) & 2051L) != 0)) {
				{
				setState(404);
				value();
				}
			}

			setState(407);
			match(LC);
			setState(408);
			match(NEWLINE);
			setState(409);
			whenValueBody();
			setState(410);
			match(RC);
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
	public static class WhenValueBodyContext extends ParserRuleContext {
		public List<WhenValueBranchContext> whenValueBranch() {
			return getRuleContexts(WhenValueBranchContext.class);
		}
		public WhenValueBranchContext whenValueBranch(int i) {
			return getRuleContext(WhenValueBranchContext.class,i);
		}
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
		}
		public WhenElseBranchContext whenElseBranch() {
			return getRuleContext(WhenElseBranchContext.class,0);
		}
		public WhenValueBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenValueBody; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitWhenValueBody(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenValueBodyContext whenValueBody() throws RecognitionException {
		WhenValueBodyContext _localctx = new WhenValueBodyContext(_ctx, getState());
		enterRule(_localctx, 54, RULE_whenValueBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(415);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(412);
				trivia();
				}
				}
				setState(417);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(418);
			whenValueBranch();
			setState(423);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 576460753257882128L) != 0) || ((((_la - 75)) & ~0x3f) == 0 && ((1L << (_la - 75)) & 2051L) != 0)) {
				{
				setState(421);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case BEGIN_VALUE_INLINE:
				case ROOT_IDENTIFIER:
				case BEGIN_TABLE:
				case BEGIN_TUPLE:
				case BEGIN_LAMBDA_BLOCK:
				case BEGIN_LAMBDA_ARROW:
				case NUMBER:
				case BOOLEAN:
				case NULL:
				case TOPLEVEL_VALUE_EXPRESSION:
				case VALUE_SMART_ROOT:
				case NOT_VALUE:
				case FUNCTION_IDENTIFIER:
					{
					setState(419);
					whenValueBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(420);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(425);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(427);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(426);
				whenElseBranch();
				}
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
	public static class WhenValueBranchContext extends ParserRuleContext {
		public WhenValueConditionContext whenValueCondition() {
			return getRuleContext(WhenValueConditionContext.class,0);
		}
		public TerminalNode ARROW() { return getToken(HixParser.ARROW, 0); }
		public WhenResultContext whenResult() {
			return getRuleContext(WhenResultContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(HixParser.NEWLINE, 0); }
		public WhenValueBranchContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenValueBranch; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitWhenValueBranch(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenValueBranchContext whenValueBranch() throws RecognitionException {
		WhenValueBranchContext _localctx = new WhenValueBranchContext(_ctx, getState());
		enterRule(_localctx, 56, RULE_whenValueBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(429);
			whenValueCondition();
			setState(430);
			match(ARROW);
			setState(431);
			whenResult();
			setState(432);
			match(NEWLINE);
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
	public static class WhenValueConditionContext extends ParserRuleContext {
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public InlineTransformationContext inlineTransformation() {
			return getRuleContext(InlineTransformationContext.class,0);
		}
		public WhenValueConditionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenValueCondition; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitWhenValueCondition(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenValueConditionContext whenValueCondition() throws RecognitionException {
		WhenValueConditionContext _localctx = new WhenValueConditionContext(_ctx, getState());
		enterRule(_localctx, 58, RULE_whenValueCondition);
		try {
			setState(436);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,46,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(434);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(435);
				inlineTransformation();
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
	public static class AssignmentStatementContext extends ParserRuleContext {
		public VariableSpecifiersContext variableSpecifiers() {
			return getRuleContext(VariableSpecifiersContext.class,0);
		}
		public VariableIdentifierContext variableIdentifier() {
			return getRuleContext(VariableIdentifierContext.class,0);
		}
		public AssignedValueContext assignedValue() {
			return getRuleContext(AssignedValueContext.class,0);
		}
		public AssignmentStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_assignmentStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitAssignmentStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final AssignmentStatementContext assignmentStatement() throws RecognitionException {
		AssignmentStatementContext _localctx = new AssignmentStatementContext(_ctx, getState());
		enterRule(_localctx, 60, RULE_assignmentStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(438);
			variableSpecifiers();
			setState(439);
			variableIdentifier();
			setState(440);
			assignedValue();
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
	public static class AssignedValueContext extends ParserRuleContext {
		public TerminalNode ASSIGN() { return getToken(HixParser.ASSIGN, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public WhenValueStatementContext whenValueStatement() {
			return getRuleContext(WhenValueStatementContext.class,0);
		}
		public WhenChainStatementContext whenChainStatement() {
			return getRuleContext(WhenChainStatementContext.class,0);
		}
		public InvocationStatementContext invocationStatement() {
			return getRuleContext(InvocationStatementContext.class,0);
		}
		public TailValueContext tailValue() {
			return getRuleContext(TailValueContext.class,0);
		}
		public AssignedValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_assignedValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitAssignedValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final AssignedValueContext assignedValue() throws RecognitionException {
		AssignedValueContext _localctx = new AssignedValueContext(_ctx, getState());
		enterRule(_localctx, 62, RULE_assignedValue);
		try {
			setState(450);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ASSIGN:
				enterOuterAlt(_localctx, 1);
				{
				setState(442);
				match(ASSIGN);
				setState(447);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,47,_ctx) ) {
				case 1:
					{
					setState(443);
					value();
					}
					break;
				case 2:
					{
					setState(444);
					whenValueStatement();
					}
					break;
				case 3:
					{
					setState(445);
					whenChainStatement();
					}
					break;
				case 4:
					{
					setState(446);
					invocationStatement();
					}
					break;
				}
				}
				break;
			case BEGIN_CONTENT:
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case BEGIN_LAMBDA_BLOCK:
			case BEGIN_LAMBDA_ARROW:
			case NUMBER:
			case BOOLEAN:
			case NULL:
			case NEWLINE:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(449);
				tailValue();
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
	public static class ControlflowStatementContext extends ParserRuleContext {
		public TerminalNode KEYWORD_RETURN() { return getToken(HixParser.KEYWORD_RETURN, 0); }
		public ValueListContext valueList() {
			return getRuleContext(ValueListContext.class,0);
		}
		public TerminalNode KEYWORD_GOTO() { return getToken(HixParser.KEYWORD_GOTO, 0); }
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public TerminalNode KEYWORD_CONTINUE() { return getToken(HixParser.KEYWORD_CONTINUE, 0); }
		public TerminalNode KEYWORD_BREAK() { return getToken(HixParser.KEYWORD_BREAK, 0); }
		public ControlflowStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_controlflowStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitControlflowStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ControlflowStatementContext controlflowStatement() throws RecognitionException {
		ControlflowStatementContext _localctx = new ControlflowStatementContext(_ctx, getState());
		enterRule(_localctx, 64, RULE_controlflowStatement);
		int _la;
		try {
			setState(460);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_RETURN:
				enterOuterAlt(_localctx, 1);
				{
				setState(452);
				match(KEYWORD_RETURN);
				setState(454);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3458764513820540944L) != 0)) {
					{
					setState(453);
					valueList();
					}
				}

				}
				break;
			case KEYWORD_GOTO:
				enterOuterAlt(_localctx, 2);
				{
				setState(456);
				match(KEYWORD_GOTO);
				setState(457);
				match(IDENTIFIER);
				}
				break;
			case KEYWORD_CONTINUE:
				enterOuterAlt(_localctx, 3);
				{
				setState(458);
				match(KEYWORD_CONTINUE);
				}
				break;
			case KEYWORD_BREAK:
				enterOuterAlt(_localctx, 4);
				{
				setState(459);
				match(KEYWORD_BREAK);
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
	public static class ContentBlockContext extends ParserRuleContext {
		public TerminalNode BEGIN_CONTENT() { return getToken(HixParser.BEGIN_CONTENT, 0); }
		public ContentBodyContext contentBody() {
			return getRuleContext(ContentBodyContext.class,0);
		}
		public TerminalNode TERMINATOR() { return getToken(HixParser.TERMINATOR, 0); }
		public ContentBlockContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_contentBlock; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitContentBlock(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ContentBlockContext contentBlock() throws RecognitionException {
		ContentBlockContext _localctx = new ContentBlockContext(_ctx, getState());
		enterRule(_localctx, 66, RULE_contentBlock);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(462);
			match(BEGIN_CONTENT);
			setState(463);
			contentBody();
			setState(464);
			match(TERMINATOR);
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
	public static class ContentBodyContext extends ParserRuleContext {
		public List<TerminalNode> CONTENT_WRAP() { return getTokens(HixParser.CONTENT_WRAP); }
		public TerminalNode CONTENT_WRAP(int i) {
			return getToken(HixParser.CONTENT_WRAP, i);
		}
		public List<TerminalNode> CONTENT_LINEBREAK() { return getTokens(HixParser.CONTENT_LINEBREAK); }
		public TerminalNode CONTENT_LINEBREAK(int i) {
			return getToken(HixParser.CONTENT_LINEBREAK, i);
		}
		public List<TerminalNode> CONTENT_TEXT() { return getTokens(HixParser.CONTENT_TEXT); }
		public TerminalNode CONTENT_TEXT(int i) {
			return getToken(HixParser.CONTENT_TEXT, i);
		}
		public List<ContentInterpolateContext> contentInterpolate() {
			return getRuleContexts(ContentInterpolateContext.class);
		}
		public ContentInterpolateContext contentInterpolate(int i) {
			return getRuleContext(ContentInterpolateContext.class,i);
		}
		public ContentBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_contentBody; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitContentBody(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ContentBodyContext contentBody() throws RecognitionException {
		ContentBodyContext _localctx = new ContentBodyContext(_ctx, getState());
		enterRule(_localctx, 68, RULE_contentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(470);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				setState(470);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case CONTENT_WRAP:
					{
					setState(466);
					match(CONTENT_WRAP);
					}
					break;
				case CONTENT_LINEBREAK:
					{
					setState(467);
					match(CONTENT_LINEBREAK);
					}
					break;
				case CONTENT_TEXT:
					{
					setState(468);
					match(CONTENT_TEXT);
					}
					break;
				case BEGIN_VALUE_INTERPOLATE:
					{
					setState(469);
					contentInterpolate();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(472);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 8)) & ~0x3f) == 0 && ((1L << (_la - 8)) & 1008806316530991105L) != 0) );
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
	public static class ContentInterpolateContext extends ParserRuleContext {
		public TerminalNode BEGIN_VALUE_INTERPOLATE() { return getToken(HixParser.BEGIN_VALUE_INTERPOLATE, 0); }
		public DerivationContext derivation() {
			return getRuleContext(DerivationContext.class,0);
		}
		public TerminalNode RC() { return getToken(HixParser.RC, 0); }
		public TerminalNode END_CONTENT_INTERPOLATE() { return getToken(HixParser.END_CONTENT_INTERPOLATE, 0); }
		public ContentInterpolateContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_contentInterpolate; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitContentInterpolate(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ContentInterpolateContext contentInterpolate() throws RecognitionException {
		ContentInterpolateContext _localctx = new ContentInterpolateContext(_ctx, getState());
		enterRule(_localctx, 70, RULE_contentInterpolate);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(474);
			match(BEGIN_VALUE_INTERPOLATE);
			setState(475);
			derivation();
			setState(476);
			match(RC);
			setState(478);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==END_CONTENT_INTERPOLATE) {
				{
				setState(477);
				match(END_CONTENT_INTERPOLATE);
				}
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
		public NonArgumentValueContext nonArgumentValue() {
			return getRuleContext(NonArgumentValueContext.class,0);
		}
		public ArgumentValueContext argumentValue() {
			return getRuleContext(ArgumentValueContext.class,0);
		}
		public ValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_value; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ValueContext value() throws RecognitionException {
		ValueContext _localctx = new ValueContext(_ctx, getState());
		enterRule(_localctx, 72, RULE_value);
		try {
			setState(482);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case BEGIN_LAMBDA_BLOCK:
			case BEGIN_LAMBDA_ARROW:
			case NUMBER:
			case BOOLEAN:
			case NULL:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(480);
				nonArgumentValue(0);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(481);
				argumentValue();
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
	public static class ValueExpressionContext extends ParserRuleContext {
		public TerminalNode TOPLEVEL_VALUE_EXPRESSION() { return getToken(HixParser.TOPLEVEL_VALUE_EXPRESSION, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public TerminalNode VALUE_END() { return getToken(HixParser.VALUE_END, 0); }
		public ValueExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_valueExpression; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitValueExpression(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ValueExpressionContext valueExpression() throws RecognitionException {
		ValueExpressionContext _localctx = new ValueExpressionContext(_ctx, getState());
		enterRule(_localctx, 74, RULE_valueExpression);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(484);
			match(TOPLEVEL_VALUE_EXPRESSION);
			setState(485);
			value();
			setState(486);
			match(VALUE_END);
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
	public static class NonArgumentValueContext extends ParserRuleContext {
		public PrimaryValueContext primaryValue() {
			return getRuleContext(PrimaryValueContext.class,0);
		}
		public ElvisValueContext elvisValue() {
			return getRuleContext(ElvisValueContext.class,0);
		}
		public PrefixOperatorsContext prefixOperators() {
			return getRuleContext(PrefixOperatorsContext.class,0);
		}
		public NonArgumentValueContext nonArgumentValue() {
			return getRuleContext(NonArgumentValueContext.class,0);
		}
		public PostfixOperatorsContext postfixOperators() {
			return getRuleContext(PostfixOperatorsContext.class,0);
		}
		public NonArgumentValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_nonArgumentValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitNonArgumentValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final NonArgumentValueContext nonArgumentValue() throws RecognitionException {
		return nonArgumentValue(0);
	}

	private NonArgumentValueContext nonArgumentValue(int _p) throws RecognitionException {
		ParserRuleContext _parentctx = _ctx;
		int _parentState = getState();
		NonArgumentValueContext _localctx = new NonArgumentValueContext(_ctx, _parentState);
		NonArgumentValueContext _prevctx = _localctx;
		int _startState = 76;
		enterRecursionRule(_localctx, 76, RULE_nonArgumentValue, _p);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(496);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case BEGIN_LAMBDA_BLOCK:
			case BEGIN_LAMBDA_ARROW:
			case NUMBER:
			case BOOLEAN:
			case NULL:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case FUNCTION_IDENTIFIER:
				{
				setState(489);
				primaryValue();
				setState(491);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,55,_ctx) ) {
				case 1:
					{
					setState(490);
					elvisValue();
					}
					break;
				}
				}
				break;
			case NOT_VALUE:
				{
				setState(493);
				prefixOperators();
				setState(494);
				nonArgumentValue(2);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(502);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,57,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					{
					_localctx = new NonArgumentValueContext(_parentctx, _parentState);
					pushNewRecursionContext(_localctx, _startState, RULE_nonArgumentValue);
					setState(498);
					if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
					setState(499);
					postfixOperators();
					}
					}
				}
				setState(504);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,57,_ctx);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			unrollRecursionContexts(_parentctx);
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class PrimaryValueContext extends ParserRuleContext {
		public InlineValueContext inlineValue() {
			return getRuleContext(InlineValueContext.class,0);
		}
		public LambdaValueContext lambdaValue() {
			return getRuleContext(LambdaValueContext.class,0);
		}
		public DerivationContext derivation() {
			return getRuleContext(DerivationContext.class,0);
		}
		public TableValueContext tableValue() {
			return getRuleContext(TableValueContext.class,0);
		}
		public TupleValueContext tupleValue() {
			return getRuleContext(TupleValueContext.class,0);
		}
		public ValueExpressionContext valueExpression() {
			return getRuleContext(ValueExpressionContext.class,0);
		}
		public ValueStatementContext valueStatement() {
			return getRuleContext(ValueStatementContext.class,0);
		}
		public TerminalNode NUMBER() { return getToken(HixParser.NUMBER, 0); }
		public TerminalNode BOOLEAN() { return getToken(HixParser.BOOLEAN, 0); }
		public TerminalNode NULL() { return getToken(HixParser.NULL, 0); }
		public PrimaryValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_primaryValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitPrimaryValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PrimaryValueContext primaryValue() throws RecognitionException {
		PrimaryValueContext _localctx = new PrimaryValueContext(_ctx, getState());
		enterRule(_localctx, 78, RULE_primaryValue);
		try {
			setState(515);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,58,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(505);
				inlineValue();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(506);
				lambdaValue();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(507);
				derivation();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(508);
				tableValue();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(509);
				tupleValue();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(510);
				valueExpression();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(511);
				valueStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(512);
				match(NUMBER);
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(513);
				match(BOOLEAN);
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(514);
				match(NULL);
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
	public static class LambdaValueContext extends ParserRuleContext {
		public TerminalNode BEGIN_LAMBDA_BLOCK() { return getToken(HixParser.BEGIN_LAMBDA_BLOCK, 0); }
		public TerminalNode RC() { return getToken(HixParser.RC, 0); }
		public List<StatementContext> statement() {
			return getRuleContexts(StatementContext.class);
		}
		public StatementContext statement(int i) {
			return getRuleContext(StatementContext.class,i);
		}
		public List<TerminalNode> SEMICOLON() { return getTokens(HixParser.SEMICOLON); }
		public TerminalNode SEMICOLON(int i) {
			return getToken(HixParser.SEMICOLON, i);
		}
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
		}
		public TerminalNode BEGIN_LAMBDA_ARROW() { return getToken(HixParser.BEGIN_LAMBDA_ARROW, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public TerminalNode VALUE_END() { return getToken(HixParser.VALUE_END, 0); }
		public LambdaValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_lambdaValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitLambdaValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final LambdaValueContext lambdaValue() throws RecognitionException {
		LambdaValueContext _localctx = new LambdaValueContext(_ctx, getState());
		enterRule(_localctx, 80, RULE_lambdaValue);
		int _la;
		try {
			setState(532);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_LAMBDA_BLOCK:
				enterOuterAlt(_localctx, 1);
				{
				setState(517);
				match(BEGIN_LAMBDA_BLOCK);
				setState(523);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 75430907388888064L) != 0)) {
					{
					setState(521);
					_errHandler.sync(this);
					switch (_input.LA(1)) {
					case IDENTIFIER:
					case LC:
					case KEYWORD_RETURN:
					case KEYWORD_GOTO:
					case KEYWORD_BREAK:
					case KEYWORD_CONTINUE:
					case KEYWORD_TARGET:
					case KEYWORD_VAR:
					case KEYWORD_LOCAL:
					case KEYWORD_CARRY:
					case KEYWORD_WHEN:
					case LABEL_PREFIX:
						{
						setState(518);
						statement();
						}
						break;
					case SEMICOLON:
						{
						setState(519);
						match(SEMICOLON);
						}
						break;
					case NEWLINE:
					case COMMENT:
					case SLASH_COMMENT:
						{
						setState(520);
						trivia();
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					}
					setState(525);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(526);
				match(RC);
				}
				break;
			case BEGIN_LAMBDA_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(527);
				match(BEGIN_LAMBDA_ARROW);
				setState(528);
				value();
				setState(530);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,61,_ctx) ) {
				case 1:
					{
					setState(529);
					match(VALUE_END);
					}
					break;
				}
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
	public static class PrefixOperatorsContext extends ParserRuleContext {
		public TerminalNode NOT_VALUE() { return getToken(HixParser.NOT_VALUE, 0); }
		public PrefixOperatorsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_prefixOperators; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitPrefixOperators(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PrefixOperatorsContext prefixOperators() throws RecognitionException {
		PrefixOperatorsContext _localctx = new PrefixOperatorsContext(_ctx, getState());
		enterRule(_localctx, 82, RULE_prefixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(534);
			match(NOT_VALUE);
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
	public static class PostfixOperatorsContext extends ParserRuleContext {
		public TerminalNode VALUE_CHECK() { return getToken(HixParser.VALUE_CHECK, 0); }
		public PostfixOperatorsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_postfixOperators; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitPostfixOperators(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PostfixOperatorsContext postfixOperators() throws RecognitionException {
		PostfixOperatorsContext _localctx = new PostfixOperatorsContext(_ctx, getState());
		enterRule(_localctx, 84, RULE_postfixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(536);
			match(VALUE_CHECK);
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
	public static class ValueStatementContext extends ParserRuleContext {
		public FunctionIdentifierContext functionIdentifier() {
			return getRuleContext(FunctionIdentifierContext.class,0);
		}
		public ValueListContext valueList() {
			return getRuleContext(ValueListContext.class,0);
		}
		public ValueStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_valueStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitValueStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ValueStatementContext valueStatement() throws RecognitionException {
		ValueStatementContext _localctx = new ValueStatementContext(_ctx, getState());
		enterRule(_localctx, 86, RULE_valueStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(538);
			functionIdentifier();
			setState(539);
			valueList();
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
	public static class TailValueContext extends ParserRuleContext {
		public ContentBlockContext contentBlock() {
			return getRuleContext(ContentBlockContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(HixParser.NEWLINE, 0); }
		public NonArgumentValueContext nonArgumentValue() {
			return getRuleContext(NonArgumentValueContext.class,0);
		}
		public TailValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tailValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTailValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TailValueContext tailValue() throws RecognitionException {
		TailValueContext _localctx = new TailValueContext(_ctx, getState());
		enterRule(_localctx, 88, RULE_tailValue);
		int _la;
		try {
			setState(546);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_CONTENT:
			case NEWLINE:
				enterOuterAlt(_localctx, 1);
				{
				setState(542);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==NEWLINE) {
					{
					setState(541);
					match(NEWLINE);
					}
				}

				setState(544);
				contentBlock();
				}
				break;
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case BEGIN_LAMBDA_BLOCK:
			case BEGIN_LAMBDA_ARROW:
			case NUMBER:
			case BOOLEAN:
			case NULL:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(545);
				nonArgumentValue(0);
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
	public static class TupleValueContext extends ParserRuleContext {
		public TerminalNode BEGIN_TUPLE() { return getToken(HixParser.BEGIN_TUPLE, 0); }
		public TerminalNode VALUE_END_INLINE() { return getToken(HixParser.VALUE_END_INLINE, 0); }
		public List<ValueContext> value() {
			return getRuleContexts(ValueContext.class);
		}
		public ValueContext value(int i) {
			return getRuleContext(ValueContext.class,i);
		}
		public List<TerminalNode> VALUE_DELIMITER() { return getTokens(HixParser.VALUE_DELIMITER); }
		public TerminalNode VALUE_DELIMITER(int i) {
			return getToken(HixParser.VALUE_DELIMITER, i);
		}
		public TupleValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tupleValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTupleValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TupleValueContext tupleValue() throws RecognitionException {
		TupleValueContext _localctx = new TupleValueContext(_ctx, getState());
		enterRule(_localctx, 90, RULE_tupleValue);
		int _la;
		try {
			int _alt;
			setState(564);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,67,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(548);
				match(BEGIN_TUPLE);
				setState(549);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(550);
				match(BEGIN_TUPLE);
				setState(551);
				value();
				setState(556);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,65,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(552);
						match(VALUE_DELIMITER);
						setState(553);
						value();
						}
						}
					}
					setState(558);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,65,_ctx);
				}
				setState(560);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(559);
					match(VALUE_DELIMITER);
					}
				}

				setState(562);
				match(VALUE_END_INLINE);
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
	public static class TableValueContext extends ParserRuleContext {
		public TerminalNode BEGIN_TABLE() { return getToken(HixParser.BEGIN_TABLE, 0); }
		public TerminalNode RC() { return getToken(HixParser.RC, 0); }
		public List<TableKeyedEntryContext> tableKeyedEntry() {
			return getRuleContexts(TableKeyedEntryContext.class);
		}
		public TableKeyedEntryContext tableKeyedEntry(int i) {
			return getRuleContext(TableKeyedEntryContext.class,i);
		}
		public List<TerminalNode> VALUE_DELIMITER() { return getTokens(HixParser.VALUE_DELIMITER); }
		public TerminalNode VALUE_DELIMITER(int i) {
			return getToken(HixParser.VALUE_DELIMITER, i);
		}
		public TableValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tableValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTableValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TableValueContext tableValue() throws RecognitionException {
		TableValueContext _localctx = new TableValueContext(_ctx, getState());
		enterRule(_localctx, 92, RULE_tableValue);
		int _la;
		try {
			int _alt;
			setState(582);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,70,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(566);
				match(BEGIN_TABLE);
				setState(567);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(568);
				match(BEGIN_TABLE);
				setState(569);
				tableKeyedEntry();
				setState(574);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,68,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(570);
						match(VALUE_DELIMITER);
						setState(571);
						tableKeyedEntry();
						}
						}
					}
					setState(576);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,68,_ctx);
				}
				setState(578);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(577);
					match(VALUE_DELIMITER);
					}
				}

				setState(580);
				match(RC);
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
	public static class TableKeyedEntryContext extends ParserRuleContext {
		public TerminalNode ROOT_IDENTIFIER() { return getToken(HixParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode VALUE_ASSIGN() { return getToken(HixParser.VALUE_ASSIGN, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public List<MetadataContext> metadata() {
			return getRuleContexts(MetadataContext.class);
		}
		public MetadataContext metadata(int i) {
			return getRuleContext(MetadataContext.class,i);
		}
		public TableKeyedEntryContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tableKeyedEntry; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTableKeyedEntry(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TableKeyedEntryContext tableKeyedEntry() throws RecognitionException {
		TableKeyedEntryContext _localctx = new TableKeyedEntryContext(_ctx, getState());
		enterRule(_localctx, 94, RULE_tableKeyedEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(587);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				{
				setState(584);
				metadata();
				}
				}
				setState(589);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(590);
			match(ROOT_IDENTIFIER);
			setState(591);
			match(VALUE_ASSIGN);
			setState(592);
			value();
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
	public static class ValueListContext extends ParserRuleContext {
		public TerminalNode BEGIN_PARAMETERS() { return getToken(HixParser.BEGIN_PARAMETERS, 0); }
		public List<ValueContext> value() {
			return getRuleContexts(ValueContext.class);
		}
		public ValueContext value(int i) {
			return getRuleContext(ValueContext.class,i);
		}
		public TerminalNode END_PARAMETERS() { return getToken(HixParser.END_PARAMETERS, 0); }
		public List<TerminalNode> VALUE_DELIMITER() { return getTokens(HixParser.VALUE_DELIMITER); }
		public TerminalNode VALUE_DELIMITER(int i) {
			return getToken(HixParser.VALUE_DELIMITER, i);
		}
		public TerminalNode EMPTY_PARAMETERS() { return getToken(HixParser.EMPTY_PARAMETERS, 0); }
		public List<ArgumentValueContext> argumentValue() {
			return getRuleContexts(ArgumentValueContext.class);
		}
		public ArgumentValueContext argumentValue(int i) {
			return getRuleContext(ArgumentValueContext.class,i);
		}
		public ValueListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_valueList; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitValueList(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ValueListContext valueList() throws RecognitionException {
		ValueListContext _localctx = new ValueListContext(_ctx, getState());
		enterRule(_localctx, 96, RULE_valueList);
		int _la;
		try {
			int _alt;
			setState(616);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,75,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(594);
				match(BEGIN_PARAMETERS);
				setState(595);
				value();
				setState(600);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,72,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(596);
						match(VALUE_DELIMITER);
						setState(597);
						value();
						}
						}
					}
					setState(602);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,72,_ctx);
				}
				setState(604);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(603);
					match(VALUE_DELIMITER);
					}
				}

				setState(606);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(608);
				match(BEGIN_PARAMETERS);
				setState(609);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(610);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(612);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(611);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(614);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,74,_ctx);
				} while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER );
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
	public static class ArgumentValueContext extends ParserRuleContext {
		public TerminalNode BEGIN_ARGUMENT() { return getToken(HixParser.BEGIN_ARGUMENT, 0); }
		public ArgumentBodyContext argumentBody() {
			return getRuleContext(ArgumentBodyContext.class,0);
		}
		public TerminalNode ARGUMENT_END() { return getToken(HixParser.ARGUMENT_END, 0); }
		public ArgumentValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_argumentValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitArgumentValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ArgumentValueContext argumentValue() throws RecognitionException {
		ArgumentValueContext _localctx = new ArgumentValueContext(_ctx, getState());
		enterRule(_localctx, 98, RULE_argumentValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(618);
			match(BEGIN_ARGUMENT);
			setState(619);
			argumentBody();
			setState(620);
			match(ARGUMENT_END);
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
	public static class ArgumentBodyContext extends ParserRuleContext {
		public List<TerminalNode> ARGUMENT_TEXT() { return getTokens(HixParser.ARGUMENT_TEXT); }
		public TerminalNode ARGUMENT_TEXT(int i) {
			return getToken(HixParser.ARGUMENT_TEXT, i);
		}
		public List<EscapedContext> escaped() {
			return getRuleContexts(EscapedContext.class);
		}
		public EscapedContext escaped(int i) {
			return getRuleContext(EscapedContext.class,i);
		}
		public List<InlineValueContext> inlineValue() {
			return getRuleContexts(InlineValueContext.class);
		}
		public InlineValueContext inlineValue(int i) {
			return getRuleContext(InlineValueContext.class,i);
		}
		public ArgumentBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_argumentBody; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitArgumentBody(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ArgumentBodyContext argumentBody() throws RecognitionException {
		ArgumentBodyContext _localctx = new ArgumentBodyContext(_ctx, getState());
		enterRule(_localctx, 100, RULE_argumentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(627);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & -9223372036854775288L) != 0)) {
				{
				setState(625);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case ARGUMENT_TEXT:
					{
					setState(622);
					match(ARGUMENT_TEXT);
					}
					break;
				case ESCAPE:
					{
					setState(623);
					escaped();
					}
					break;
				case BEGIN_VALUE_INLINE:
					{
					setState(624);
					inlineValue();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(629);
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

	@SuppressWarnings("CheckReturnValue")
	public static class InlineValueContext extends ParserRuleContext {
		public TerminalNode BEGIN_VALUE_INLINE() { return getToken(HixParser.BEGIN_VALUE_INLINE, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public TerminalNode VALUE_END_INLINE() { return getToken(HixParser.VALUE_END_INLINE, 0); }
		public InlineValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_inlineValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitInlineValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final InlineValueContext inlineValue() throws RecognitionException {
		InlineValueContext _localctx = new InlineValueContext(_ctx, getState());
		enterRule(_localctx, 102, RULE_inlineValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(630);
			match(BEGIN_VALUE_INLINE);
			setState(631);
			value();
			setState(632);
			match(VALUE_END_INLINE);
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
	public static class InlineTransformationContext extends ParserRuleContext {
		public TerminalNode BEGIN_VALUE_INLINE() { return getToken(HixParser.BEGIN_VALUE_INLINE, 0); }
		public TerminalNode VALUE_END_INLINE() { return getToken(HixParser.VALUE_END_INLINE, 0); }
		public List<TransformationPartContext> transformationPart() {
			return getRuleContexts(TransformationPartContext.class);
		}
		public TransformationPartContext transformationPart(int i) {
			return getRuleContext(TransformationPartContext.class,i);
		}
		public InlineTransformationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_inlineTransformation; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitInlineTransformation(this);
			else return visitor.visitChildren(this);
		}
	}

	public final InlineTransformationContext inlineTransformation() throws RecognitionException {
		InlineTransformationContext _localctx = new InlineTransformationContext(_ctx, getState());
		enterRule(_localctx, 104, RULE_inlineTransformation);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(634);
			match(BEGIN_VALUE_INLINE);
			setState(636);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(635);
				transformationPart();
				}
				}
				setState(638);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 68)) & ~0x3f) == 0 && ((1L << (_la - 68)) & 15L) != 0) );
			setState(640);
			match(VALUE_END_INLINE);
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
	public static class DerivationContext extends ParserRuleContext {
		public DerivationRootContext derivationRoot() {
			return getRuleContext(DerivationRootContext.class,0);
		}
		public List<TransformationPartContext> transformationPart() {
			return getRuleContexts(TransformationPartContext.class);
		}
		public TransformationPartContext transformationPart(int i) {
			return getRuleContext(TransformationPartContext.class,i);
		}
		public DerivationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_derivation; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitDerivation(this);
			else return visitor.visitChildren(this);
		}
	}

	public final DerivationContext derivation() throws RecognitionException {
		DerivationContext _localctx = new DerivationContext(_ctx, getState());
		enterRule(_localctx, 106, RULE_derivation);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(642);
			derivationRoot();
			setState(646);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,79,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(643);
					transformationPart();
					}
					}
				}
				setState(648);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,79,_ctx);
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
	public static class DerivationRootContext extends ParserRuleContext {
		public TerminalNode ROOT_IDENTIFIER() { return getToken(HixParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode VALUE_SMART_ROOT() { return getToken(HixParser.VALUE_SMART_ROOT, 0); }
		public TerminalNode NUMBER() { return getToken(HixParser.NUMBER, 0); }
		public DerivationRootContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_derivationRoot; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitDerivationRoot(this);
			else return visitor.visitChildren(this);
		}
	}

	public final DerivationRootContext derivationRoot() throws RecognitionException {
		DerivationRootContext _localctx = new DerivationRootContext(_ctx, getState());
		enterRule(_localctx, 108, RULE_derivationRoot);
		int _la;
		try {
			setState(652);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ROOT_IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(649);
				match(ROOT_IDENTIFIER);
				}
				break;
			case VALUE_SMART_ROOT:
				enterOuterAlt(_localctx, 2);
				{
				setState(650);
				match(VALUE_SMART_ROOT);
				setState(651);
				_la = _input.LA(1);
				if ( !(_la==ROOT_IDENTIFIER || _la==NUMBER) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
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
	public static class ElvisValueContext extends ParserRuleContext {
		public TerminalNode VALUE_ELVIS() { return getToken(HixParser.VALUE_ELVIS, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public ElvisValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_elvisValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitElvisValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ElvisValueContext elvisValue() throws RecognitionException {
		ElvisValueContext _localctx = new ElvisValueContext(_ctx, getState());
		enterRule(_localctx, 110, RULE_elvisValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(654);
			match(VALUE_ELVIS);
			setState(655);
			value();
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
	public static class TransformationPartContext extends ParserRuleContext {
		public FunctionChainTypeContext functionChainType() {
			return getRuleContext(FunctionChainTypeContext.class,0);
		}
		public FunctionIdentifierContext functionIdentifier() {
			return getRuleContext(FunctionIdentifierContext.class,0);
		}
		public ValueListContext valueList() {
			return getRuleContext(ValueListContext.class,0);
		}
		public MemberIdentifierContext memberIdentifier() {
			return getRuleContext(MemberIdentifierContext.class,0);
		}
		public TerminalNode VALUE_WRAP() { return getToken(HixParser.VALUE_WRAP, 0); }
		public TransformationPartContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_transformationPart; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTransformationPart(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TransformationPartContext transformationPart() throws RecognitionException {
		TransformationPartContext _localctx = new TransformationPartContext(_ctx, getState());
		enterRule(_localctx, 112, RULE_transformationPart);
		try {
			setState(664);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case VALUE_FUNCTION:
			case VALUE_PREDICATE:
				enterOuterAlt(_localctx, 1);
				{
				setState(657);
				functionChainType();
				setState(658);
				functionIdentifier();
				setState(660);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,81,_ctx) ) {
				case 1:
					{
					setState(659);
					valueList();
					}
					break;
				}
				}
				break;
			case VALUE_MEMBER:
				enterOuterAlt(_localctx, 2);
				{
				setState(662);
				memberIdentifier();
				}
				break;
			case VALUE_WRAP:
				enterOuterAlt(_localctx, 3);
				{
				setState(663);
				match(VALUE_WRAP);
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
	public static class FunctionChainTypeContext extends ParserRuleContext {
		public TerminalNode VALUE_FUNCTION() { return getToken(HixParser.VALUE_FUNCTION, 0); }
		public TerminalNode VALUE_PREDICATE() { return getToken(HixParser.VALUE_PREDICATE, 0); }
		public FunctionChainTypeContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_functionChainType; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitFunctionChainType(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FunctionChainTypeContext functionChainType() throws RecognitionException {
		FunctionChainTypeContext _localctx = new FunctionChainTypeContext(_ctx, getState());
		enterRule(_localctx, 114, RULE_functionChainType);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(666);
			_la = _input.LA(1);
			if ( !(_la==VALUE_FUNCTION || _la==VALUE_PREDICATE) ) {
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
	public static class LabelIdentifierContext extends ParserRuleContext {
		public TerminalNode LABEL_PREFIX() { return getToken(HixParser.LABEL_PREFIX, 0); }
		public TerminalNode LABEL_IDENTIFIER() { return getToken(HixParser.LABEL_IDENTIFIER, 0); }
		public LabelIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_labelIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitLabelIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final LabelIdentifierContext labelIdentifier() throws RecognitionException {
		LabelIdentifierContext _localctx = new LabelIdentifierContext(_ctx, getState());
		enterRule(_localctx, 116, RULE_labelIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(668);
			match(LABEL_PREFIX);
			setState(669);
			match(LABEL_IDENTIFIER);
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
	public static class MemberIdentifierContext extends ParserRuleContext {
		public TerminalNode VALUE_MEMBER() { return getToken(HixParser.VALUE_MEMBER, 0); }
		public TerminalNode MEMBER_IDENTIFIER() { return getToken(HixParser.MEMBER_IDENTIFIER, 0); }
		public MemberIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_memberIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitMemberIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MemberIdentifierContext memberIdentifier() throws RecognitionException {
		MemberIdentifierContext _localctx = new MemberIdentifierContext(_ctx, getState());
		enterRule(_localctx, 118, RULE_memberIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(671);
			match(VALUE_MEMBER);
			setState(672);
			match(MEMBER_IDENTIFIER);
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
	public static class MixinIdentifierContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public TerminalNode NAMESPACE_IDENTIFIER() { return getToken(HixParser.NAMESPACE_IDENTIFIER, 0); }
		public ArgumentValueContext argumentValue() {
			return getRuleContext(ArgumentValueContext.class,0);
		}
		public MixinIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_mixinIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitMixinIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MixinIdentifierContext mixinIdentifier() throws RecognitionException {
		MixinIdentifierContext _localctx = new MixinIdentifierContext(_ctx, getState());
		enterRule(_localctx, 120, RULE_mixinIdentifier);
		try {
			setState(677);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(674);
				match(IDENTIFIER);
				}
				break;
			case NAMESPACE_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(675);
				match(NAMESPACE_IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 3);
				{
				setState(676);
				argumentValue();
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
	public static class FunctionDeclarationIdentifierContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public ArgumentValueContext argumentValue() {
			return getRuleContext(ArgumentValueContext.class,0);
		}
		public FunctionDeclarationIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_functionDeclarationIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitFunctionDeclarationIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FunctionDeclarationIdentifierContext functionDeclarationIdentifier() throws RecognitionException {
		FunctionDeclarationIdentifierContext _localctx = new FunctionDeclarationIdentifierContext(_ctx, getState());
		enterRule(_localctx, 122, RULE_functionDeclarationIdentifier);
		try {
			setState(681);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(679);
				match(IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(680);
				argumentValue();
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
	public static class VariableIdentifierContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public VariableIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_variableIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitVariableIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final VariableIdentifierContext variableIdentifier() throws RecognitionException {
		VariableIdentifierContext _localctx = new VariableIdentifierContext(_ctx, getState());
		enterRule(_localctx, 124, RULE_variableIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(683);
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
	public static class FunctionIdentifierContext extends ParserRuleContext {
		public TerminalNode FUNCTION_IDENTIFIER() { return getToken(HixParser.FUNCTION_IDENTIFIER, 0); }
		public TerminalNode ROOT_IDENTIFIER() { return getToken(HixParser.ROOT_IDENTIFIER, 0); }
		public FunctionIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_functionIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitFunctionIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FunctionIdentifierContext functionIdentifier() throws RecognitionException {
		FunctionIdentifierContext _localctx = new FunctionIdentifierContext(_ctx, getState());
		enterRule(_localctx, 126, RULE_functionIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(685);
			_la = _input.LA(1);
			if ( !(_la==ROOT_IDENTIFIER || _la==FUNCTION_IDENTIFIER) ) {
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
	public static class KindIdentifierContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public TerminalNode ROOT_IDENTIFIER() { return getToken(HixParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode NULL() { return getToken(HixParser.NULL, 0); }
		public KindIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_kindIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitKindIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final KindIdentifierContext kindIdentifier() throws RecognitionException {
		KindIdentifierContext _localctx = new KindIdentifierContext(_ctx, getState());
		enterRule(_localctx, 128, RULE_kindIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(687);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 8397824L) != 0)) ) {
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
	public static class ExpressionModifierContext extends ParserRuleContext {
		public TerminalNode KEYWORD_PRELUDE() { return getToken(HixParser.KEYWORD_PRELUDE, 0); }
		public TerminalNode KEYWORD_STRICT() { return getToken(HixParser.KEYWORD_STRICT, 0); }
		public ExpressionModifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_expressionModifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitExpressionModifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ExpressionModifierContext expressionModifier() throws RecognitionException {
		ExpressionModifierContext _localctx = new ExpressionModifierContext(_ctx, getState());
		enterRule(_localctx, 130, RULE_expressionModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(689);
			_la = _input.LA(1);
			if ( !(_la==KEYWORD_PRELUDE || _la==KEYWORD_STRICT) ) {
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
	public static class MixinModifierContext extends ParserRuleContext {
		public TerminalNode KEYWORD_DERIVATION() { return getToken(HixParser.KEYWORD_DERIVATION, 0); }
		public MixinModifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_mixinModifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitMixinModifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MixinModifierContext mixinModifier() throws RecognitionException {
		MixinModifierContext _localctx = new MixinModifierContext(_ctx, getState());
		enterRule(_localctx, 132, RULE_mixinModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(691);
			match(KEYWORD_DERIVATION);
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
	public static class VariableSpecifiersContext extends ParserRuleContext {
		public TerminalNode KEYWORD_LOCAL() { return getToken(HixParser.KEYWORD_LOCAL, 0); }
		public TerminalNode KEYWORD_CARRY() { return getToken(HixParser.KEYWORD_CARRY, 0); }
		public TerminalNode KEYWORD_VAR() { return getToken(HixParser.KEYWORD_VAR, 0); }
		public TerminalNode KEYWORD_TARGET() { return getToken(HixParser.KEYWORD_TARGET, 0); }
		public VariableSpecifiersContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_variableSpecifiers; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitVariableSpecifiers(this);
			else return visitor.visitChildren(this);
		}
	}

	public final VariableSpecifiersContext variableSpecifiers() throws RecognitionException {
		VariableSpecifiersContext _localctx = new VariableSpecifiersContext(_ctx, getState());
		enterRule(_localctx, 134, RULE_variableSpecifiers);
		int _la;
		try {
			setState(701);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_LOCAL:
			case KEYWORD_CARRY:
				enterOuterAlt(_localctx, 1);
				{
				setState(694);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(693);
					match(KEYWORD_CARRY);
					}
				}

				setState(696);
				match(KEYWORD_LOCAL);
				}
				break;
			case KEYWORD_TARGET:
			case KEYWORD_VAR:
				enterOuterAlt(_localctx, 2);
				{
				{
				setState(698);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_TARGET) {
					{
					setState(697);
					match(KEYWORD_TARGET);
					}
				}

				setState(700);
				match(KEYWORD_VAR);
				}
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
	public static class FuncModifierContext extends ParserRuleContext {
		public TerminalNode KEYWORD_PURE() { return getToken(HixParser.KEYWORD_PURE, 0); }
		public TerminalNode KEYWORD_NOINLINE() { return getToken(HixParser.KEYWORD_NOINLINE, 0); }
		public TerminalNode KEYWORD_INLINE() { return getToken(HixParser.KEYWORD_INLINE, 0); }
		public FuncModifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_funcModifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitFuncModifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FuncModifierContext funcModifier() throws RecognitionException {
		FuncModifierContext _localctx = new FuncModifierContext(_ctx, getState());
		enterRule(_localctx, 136, RULE_funcModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(703);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 31525197391593472L) != 0)) ) {
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
	public static class TriviaContext extends ParserRuleContext {
		public CommentContext comment() {
			return getRuleContext(CommentContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(HixParser.NEWLINE, 0); }
		public TriviaContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_trivia; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTrivia(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TriviaContext trivia() throws RecognitionException {
		TriviaContext _localctx = new TriviaContext(_ctx, getState());
		enterRule(_localctx, 138, RULE_trivia);
		try {
			setState(707);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case COMMENT:
			case SLASH_COMMENT:
				enterOuterAlt(_localctx, 1);
				{
				setState(705);
				comment();
				}
				break;
			case NEWLINE:
				enterOuterAlt(_localctx, 2);
				{
				setState(706);
				match(NEWLINE);
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
	public static class CommentContext extends ParserRuleContext {
		public TerminalNode COMMENT() { return getToken(HixParser.COMMENT, 0); }
		public TerminalNode SLASH_COMMENT() { return getToken(HixParser.SLASH_COMMENT, 0); }
		public CommentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_comment; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitComment(this);
			else return visitor.visitChildren(this);
		}
	}

	public final CommentContext comment() throws RecognitionException {
		CommentContext _localctx = new CommentContext(_ctx, getState());
		enterRule(_localctx, 140, RULE_comment);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(709);
			_la = _input.LA(1);
			if ( !(_la==COMMENT || _la==SLASH_COMMENT) ) {
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
	public static class EscapedContext extends ParserRuleContext {
		public TerminalNode ESCAPE() { return getToken(HixParser.ESCAPE, 0); }
		public TerminalNode ESCAPE_MACRO() { return getToken(HixParser.ESCAPE_MACRO, 0); }
		public TerminalNode ESCAPE_LITERAL() { return getToken(HixParser.ESCAPE_LITERAL, 0); }
		public TerminalNode ESCAPE_HEX() { return getToken(HixParser.ESCAPE_HEX, 0); }
		public EscapedContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_escaped; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitEscaped(this);
			else return visitor.visitChildren(this);
		}
	}

	public final EscapedContext escaped() throws RecognitionException {
		EscapedContext _localctx = new EscapedContext(_ctx, getState());
		enterRule(_localctx, 142, RULE_escaped);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(711);
			match(ESCAPE);
			setState(712);
			_la = _input.LA(1);
			if ( !(((((_la - 82)) & ~0x3f) == 0 && ((1L << (_la - 82)) & 7L) != 0)) ) {
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

	public boolean sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 38:
			return nonArgumentValue_sempred((NonArgumentValueContext)_localctx, predIndex);
		}
		return true;
	}
	private boolean nonArgumentValue_sempred(NonArgumentValueContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0:
			return precpred(_ctx, 1);
		}
		return true;
	}

	public static final String _serializedATN =
		"\u0004\u0001Y\u02cb\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
		"\u0002\u0007\u0002\u0002\u0003\u0007\u0003\u0002\u0004\u0007\u0004\u0002"+
		"\u0005\u0007\u0005\u0002\u0006\u0007\u0006\u0002\u0007\u0007\u0007\u0002"+
		"\b\u0007\b\u0002\t\u0007\t\u0002\n\u0007\n\u0002\u000b\u0007\u000b\u0002"+
		"\f\u0007\f\u0002\r\u0007\r\u0002\u000e\u0007\u000e\u0002\u000f\u0007\u000f"+
		"\u0002\u0010\u0007\u0010\u0002\u0011\u0007\u0011\u0002\u0012\u0007\u0012"+
		"\u0002\u0013\u0007\u0013\u0002\u0014\u0007\u0014\u0002\u0015\u0007\u0015"+
		"\u0002\u0016\u0007\u0016\u0002\u0017\u0007\u0017\u0002\u0018\u0007\u0018"+
		"\u0002\u0019\u0007\u0019\u0002\u001a\u0007\u001a\u0002\u001b\u0007\u001b"+
		"\u0002\u001c\u0007\u001c\u0002\u001d\u0007\u001d\u0002\u001e\u0007\u001e"+
		"\u0002\u001f\u0007\u001f\u0002 \u0007 \u0002!\u0007!\u0002\"\u0007\"\u0002"+
		"#\u0007#\u0002$\u0007$\u0002%\u0007%\u0002&\u0007&\u0002\'\u0007\'\u0002"+
		"(\u0007(\u0002)\u0007)\u0002*\u0007*\u0002+\u0007+\u0002,\u0007,\u0002"+
		"-\u0007-\u0002.\u0007.\u0002/\u0007/\u00020\u00070\u00021\u00071\u0002"+
		"2\u00072\u00023\u00073\u00024\u00074\u00025\u00075\u00026\u00076\u0002"+
		"7\u00077\u00028\u00078\u00029\u00079\u0002:\u0007:\u0002;\u0007;\u0002"+
		"<\u0007<\u0002=\u0007=\u0002>\u0007>\u0002?\u0007?\u0002@\u0007@\u0002"+
		"A\u0007A\u0002B\u0007B\u0002C\u0007C\u0002D\u0007D\u0002E\u0007E\u0002"+
		"F\u0007F\u0002G\u0007G\u0001\u0000\u0005\u0000\u0092\b\u0000\n\u0000\f"+
		"\u0000\u0095\t\u0000\u0001\u0000\u0003\u0000\u0098\b\u0000\u0001\u0000"+
		"\u0001\u0000\u0005\u0000\u009c\b\u0000\n\u0000\f\u0000\u009f\t\u0000\u0005"+
		"\u0000\u00a1\b\u0000\n\u0000\f\u0000\u00a4\t\u0000\u0001\u0000\u0001\u0000"+
		"\u0001\u0001\u0001\u0001\u0005\u0001\u00aa\b\u0001\n\u0001\f\u0001\u00ad"+
		"\t\u0001\u0001\u0001\u0001\u0001\u0005\u0001\u00b1\b\u0001\n\u0001\f\u0001"+
		"\u00b4\t\u0001\u0001\u0002\u0001\u0002\u0005\u0002\u00b8\b\u0002\n\u0002"+
		"\f\u0002\u00bb\t\u0002\u0001\u0002\u0001\u0002\u0003\u0002\u00bf\b\u0002"+
		"\u0001\u0002\u0001\u0002\u0003\u0002\u00c3\b\u0002\u0001\u0003\u0001\u0003"+
		"\u0005\u0003\u00c7\b\u0003\n\u0003\f\u0003\u00ca\t\u0003\u0001\u0003\u0005"+
		"\u0003\u00cd\b\u0003\n\u0003\f\u0003\u00d0\t\u0003\u0001\u0004\u0001\u0004"+
		"\u0001\u0004\u0003\u0004\u00d5\b\u0004\u0001\u0004\u0003\u0004\u00d8\b"+
		"\u0004\u0001\u0005\u0001\u0005\u0001\u0005\u0001\u0005\u0001\u0006\u0005"+
		"\u0006\u00df\b\u0006\n\u0006\f\u0006\u00e2\t\u0006\u0001\u0006\u0001\u0006"+
		"\u0001\u0006\u0001\u0006\u0001\u0007\u0001\u0007\u0001\u0007\u0001\u0007"+
		"\u0005\u0007\u00ec\b\u0007\n\u0007\f\u0007\u00ef\t\u0007\u0001\u0007\u0001"+
		"\u0007\u0001\b\u0005\b\u00f4\b\b\n\b\f\b\u00f7\t\b\u0001\b\u0001\b\u0001"+
		"\b\u0001\t\u0005\t\u00fd\b\t\n\t\f\t\u0100\t\t\u0001\t\u0001\t\u0001\t"+
		"\u0001\t\u0001\t\u0001\n\u0003\n\u0108\b\n\u0001\n\u0001\n\u0001\n\u0001"+
		"\n\u0003\n\u010e\b\n\u0003\n\u0110\b\n\u0001\u000b\u0001\u000b\u0005\u000b"+
		"\u0114\b\u000b\n\u000b\f\u000b\u0117\t\u000b\u0001\f\u0001\f\u0001\f\u0001"+
		"\f\u0001\f\u0001\r\u0001\r\u0003\r\u0120\b\r\u0001\u000e\u0001\u000e\u0001"+
		"\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0005\u000e\u0128\b\u000e\n"+
		"\u000e\f\u000e\u012b\t\u000e\u0001\u000e\u0003\u000e\u012e\b\u000e\u0001"+
		"\u000e\u0001\u000e\u0003\u000e\u0132\b\u000e\u0001\u000f\u0005\u000f\u0135"+
		"\b\u000f\n\u000f\f\u000f\u0138\t\u000f\u0001\u000f\u0003\u000f\u013b\b"+
		"\u000f\u0001\u000f\u0001\u000f\u0001\u000f\u0001\u000f\u0001\u0010\u0001"+
		"\u0010\u0001\u0010\u0001\u0010\u0005\u0010\u0145\b\u0010\n\u0010\f\u0010"+
		"\u0148\t\u0010\u0001\u0010\u0001\u0010\u0001\u0011\u0001\u0011\u0001\u0011"+
		"\u0001\u0011\u0003\u0011\u0150\b\u0011\u0001\u0011\u0001\u0011\u0001\u0011"+
		"\u0001\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0003\u0011\u0159\b\u0011"+
		"\u0001\u0012\u0001\u0012\u0001\u0012\u0003\u0012\u015e\b\u0012\u0001\u0012"+
		"\u0001\u0012\u0003\u0012\u0162\b\u0012\u0001\u0013\u0001\u0013\u0001\u0013"+
		"\u0001\u0013\u0005\u0013\u0168\b\u0013\n\u0013\f\u0013\u016b\t\u0013\u0001"+
		"\u0013\u0001\u0013\u0001\u0013\u0001\u0013\u0003\u0013\u0171\b\u0013\u0001"+
		"\u0014\u0001\u0014\u0001\u0014\u0001\u0014\u0001\u0014\u0001\u0015\u0001"+
		"\u0015\u0003\u0015\u017a\b\u0015\u0001\u0016\u0001\u0016\u0001\u0017\u0001"+
		"\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0018\u0001"+
		"\u0018\u0001\u0018\u0005\u0018\u0187\b\u0018\n\u0018\f\u0018\u018a\t\u0018"+
		"\u0001\u0018\u0003\u0018\u018d\b\u0018\u0001\u0019\u0001\u0019\u0001\u0019"+
		"\u0001\u0019\u0001\u0019\u0001\u001a\u0001\u001a\u0003\u001a\u0196\b\u001a"+
		"\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001b"+
		"\u0005\u001b\u019e\b\u001b\n\u001b\f\u001b\u01a1\t\u001b\u0001\u001b\u0001"+
		"\u001b\u0001\u001b\u0005\u001b\u01a6\b\u001b\n\u001b\f\u001b\u01a9\t\u001b"+
		"\u0001\u001b\u0003\u001b\u01ac\b\u001b\u0001\u001c\u0001\u001c\u0001\u001c"+
		"\u0001\u001c\u0001\u001c\u0001\u001d\u0001\u001d\u0003\u001d\u01b5\b\u001d"+
		"\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001f\u0001\u001f"+
		"\u0001\u001f\u0001\u001f\u0001\u001f\u0003\u001f\u01c0\b\u001f\u0001\u001f"+
		"\u0003\u001f\u01c3\b\u001f\u0001 \u0001 \u0003 \u01c7\b \u0001 \u0001"+
		" \u0001 \u0001 \u0003 \u01cd\b \u0001!\u0001!\u0001!\u0001!\u0001\"\u0001"+
		"\"\u0001\"\u0001\"\u0004\"\u01d7\b\"\u000b\"\f\"\u01d8\u0001#\u0001#\u0001"+
		"#\u0001#\u0003#\u01df\b#\u0001$\u0001$\u0003$\u01e3\b$\u0001%\u0001%\u0001"+
		"%\u0001%\u0001&\u0001&\u0001&\u0003&\u01ec\b&\u0001&\u0001&\u0001&\u0003"+
		"&\u01f1\b&\u0001&\u0001&\u0005&\u01f5\b&\n&\f&\u01f8\t&\u0001\'\u0001"+
		"\'\u0001\'\u0001\'\u0001\'\u0001\'\u0001\'\u0001\'\u0001\'\u0001\'\u0003"+
		"\'\u0204\b\'\u0001(\u0001(\u0001(\u0001(\u0005(\u020a\b(\n(\f(\u020d\t"+
		"(\u0001(\u0001(\u0001(\u0001(\u0003(\u0213\b(\u0003(\u0215\b(\u0001)\u0001"+
		")\u0001*\u0001*\u0001+\u0001+\u0001+\u0001,\u0003,\u021f\b,\u0001,\u0001"+
		",\u0003,\u0223\b,\u0001-\u0001-\u0001-\u0001-\u0001-\u0001-\u0005-\u022b"+
		"\b-\n-\f-\u022e\t-\u0001-\u0003-\u0231\b-\u0001-\u0001-\u0003-\u0235\b"+
		"-\u0001.\u0001.\u0001.\u0001.\u0001.\u0001.\u0005.\u023d\b.\n.\f.\u0240"+
		"\t.\u0001.\u0003.\u0243\b.\u0001.\u0001.\u0003.\u0247\b.\u0001/\u0005"+
		"/\u024a\b/\n/\f/\u024d\t/\u0001/\u0001/\u0001/\u0001/\u00010\u00010\u0001"+
		"0\u00010\u00050\u0257\b0\n0\f0\u025a\t0\u00010\u00030\u025d\b0\u00010"+
		"\u00010\u00010\u00010\u00010\u00010\u00040\u0265\b0\u000b0\f0\u0266\u0003"+
		"0\u0269\b0\u00011\u00011\u00011\u00011\u00012\u00012\u00012\u00052\u0272"+
		"\b2\n2\f2\u0275\t2\u00013\u00013\u00013\u00013\u00014\u00014\u00044\u027d"+
		"\b4\u000b4\f4\u027e\u00014\u00014\u00015\u00015\u00055\u0285\b5\n5\f5"+
		"\u0288\t5\u00016\u00016\u00016\u00036\u028d\b6\u00017\u00017\u00017\u0001"+
		"8\u00018\u00018\u00038\u0295\b8\u00018\u00018\u00038\u0299\b8\u00019\u0001"+
		"9\u0001:\u0001:\u0001:\u0001;\u0001;\u0001;\u0001<\u0001<\u0001<\u0003"+
		"<\u02a6\b<\u0001=\u0001=\u0003=\u02aa\b=\u0001>\u0001>\u0001?\u0001?\u0001"+
		"@\u0001@\u0001A\u0001A\u0001B\u0001B\u0001C\u0003C\u02b7\bC\u0001C\u0001"+
		"C\u0003C\u02bb\bC\u0001C\u0003C\u02be\bC\u0001D\u0001D\u0001E\u0001E\u0003"+
		"E\u02c4\bE\u0001F\u0001F\u0001G\u0001G\u0001G\u0001G\u0000\u0001LH\u0000"+
		"\u0002\u0004\u0006\b\n\f\u000e\u0010\u0012\u0014\u0016\u0018\u001a\u001c"+
		"\u001e \"$&(*,.02468:<>@BDFHJLNPRTVXZ\\^`bdfhjlnprtvxz|~\u0080\u0082\u0084"+
		"\u0086\u0088\u008a\u008c\u008e\u0000\b\u0002\u0000\r\r\u0015\u0015\u0001"+
		"\u0000DE\u0002\u0000\r\rVV\u0003\u0000\n\n\r\r\u0017\u0017\u0002\u0000"+
		"\'\'77\u0001\u000046\u0001\u0000\u001c\u001d\u0001\u0000RT\u02f8\u0000"+
		"\u0093\u0001\u0000\u0000\u0000\u0002\u00a7\u0001\u0000\u0000\u0000\u0004"+
		"\u00c2\u0001\u0000\u0000\u0000\u0006\u00c4\u0001\u0000\u0000\u0000\b\u00d7"+
		"\u0001\u0000\u0000\u0000\n\u00d9\u0001\u0000\u0000\u0000\f\u00e0\u0001"+
		"\u0000\u0000\u0000\u000e\u00e7\u0001\u0000\u0000\u0000\u0010\u00f5\u0001"+
		"\u0000\u0000\u0000\u0012\u00fe\u0001\u0000\u0000\u0000\u0014\u010f\u0001"+
		"\u0000\u0000\u0000\u0016\u0115\u0001\u0000\u0000\u0000\u0018\u0118\u0001"+
		"\u0000\u0000\u0000\u001a\u011f\u0001\u0000\u0000\u0000\u001c\u0131\u0001"+
		"\u0000\u0000\u0000\u001e\u0136\u0001\u0000\u0000\u0000 \u0140\u0001\u0000"+
		"\u0000\u0000\"\u0158\u0001\u0000\u0000\u0000$\u0161\u0001\u0000\u0000"+
		"\u0000&\u0163\u0001\u0000\u0000\u0000(\u0172\u0001\u0000\u0000\u0000*"+
		"\u0179\u0001\u0000\u0000\u0000,\u017b\u0001\u0000\u0000\u0000.\u017d\u0001"+
		"\u0000\u0000\u00000\u0183\u0001\u0000\u0000\u00002\u018e\u0001\u0000\u0000"+
		"\u00004\u0193\u0001\u0000\u0000\u00006\u019f\u0001\u0000\u0000\u00008"+
		"\u01ad\u0001\u0000\u0000\u0000:\u01b4\u0001\u0000\u0000\u0000<\u01b6\u0001"+
		"\u0000\u0000\u0000>\u01c2\u0001\u0000\u0000\u0000@\u01cc\u0001\u0000\u0000"+
		"\u0000B\u01ce\u0001\u0000\u0000\u0000D\u01d6\u0001\u0000\u0000\u0000F"+
		"\u01da\u0001\u0000\u0000\u0000H\u01e2\u0001\u0000\u0000\u0000J\u01e4\u0001"+
		"\u0000\u0000\u0000L\u01f0\u0001\u0000\u0000\u0000N\u0203\u0001\u0000\u0000"+
		"\u0000P\u0214\u0001\u0000\u0000\u0000R\u0216\u0001\u0000\u0000\u0000T"+
		"\u0218\u0001\u0000\u0000\u0000V\u021a\u0001\u0000\u0000\u0000X\u0222\u0001"+
		"\u0000\u0000\u0000Z\u0234\u0001\u0000\u0000\u0000\\\u0246\u0001\u0000"+
		"\u0000\u0000^\u024b\u0001\u0000\u0000\u0000`\u0268\u0001\u0000\u0000\u0000"+
		"b\u026a\u0001\u0000\u0000\u0000d\u0273\u0001\u0000\u0000\u0000f\u0276"+
		"\u0001\u0000\u0000\u0000h\u027a\u0001\u0000\u0000\u0000j\u0282\u0001\u0000"+
		"\u0000\u0000l\u028c\u0001\u0000\u0000\u0000n\u028e\u0001\u0000\u0000\u0000"+
		"p\u0298\u0001\u0000\u0000\u0000r\u029a\u0001\u0000\u0000\u0000t\u029c"+
		"\u0001\u0000\u0000\u0000v\u029f\u0001\u0000\u0000\u0000x\u02a5\u0001\u0000"+
		"\u0000\u0000z\u02a9\u0001\u0000\u0000\u0000|\u02ab\u0001\u0000\u0000\u0000"+
		"~\u02ad\u0001\u0000\u0000\u0000\u0080\u02af\u0001\u0000\u0000\u0000\u0082"+
		"\u02b1\u0001\u0000\u0000\u0000\u0084\u02b3\u0001\u0000\u0000\u0000\u0086"+
		"\u02bd\u0001\u0000\u0000\u0000\u0088\u02bf\u0001\u0000\u0000\u0000\u008a"+
		"\u02c3\u0001\u0000\u0000\u0000\u008c\u02c5\u0001\u0000\u0000\u0000\u008e"+
		"\u02c7\u0001\u0000\u0000\u0000\u0090\u0092\u0003\u008aE\u0000\u0091\u0090"+
		"\u0001\u0000\u0000\u0000\u0092\u0095\u0001\u0000\u0000\u0000\u0093\u0091"+
		"\u0001\u0000\u0000\u0000\u0093\u0094\u0001\u0000\u0000\u0000\u0094\u0097"+
		"\u0001\u0000\u0000\u0000\u0095\u0093\u0001\u0000\u0000\u0000\u0096\u0098"+
		"\u0003\u0002\u0001\u0000\u0097\u0096\u0001\u0000\u0000\u0000\u0097\u0098"+
		"\u0001\u0000\u0000\u0000\u0098\u00a2\u0001\u0000\u0000\u0000\u0099\u009d"+
		"\u0003\u0004\u0002\u0000\u009a\u009c\u0003\u008aE\u0000\u009b\u009a\u0001"+
		"\u0000\u0000\u0000\u009c\u009f\u0001\u0000\u0000\u0000\u009d\u009b\u0001"+
		"\u0000\u0000\u0000\u009d\u009e\u0001\u0000\u0000\u0000\u009e\u00a1\u0001"+
		"\u0000\u0000\u0000\u009f\u009d\u0001\u0000\u0000\u0000\u00a0\u0099\u0001"+
		"\u0000\u0000\u0000\u00a1\u00a4\u0001\u0000\u0000\u0000\u00a2\u00a0\u0001"+
		"\u0000\u0000\u0000\u00a2\u00a3\u0001\u0000\u0000\u0000\u00a3\u00a5\u0001"+
		"\u0000\u0000\u0000\u00a4\u00a2\u0001\u0000\u0000\u0000\u00a5\u00a6\u0005"+
		"\u0000\u0000\u0001\u00a6\u0001\u0001\u0000\u0000\u0000\u00a7\u00ab\u0003"+
		"\u0006\u0003\u0000\u00a8\u00aa\u0003\u008aE\u0000\u00a9\u00a8\u0001\u0000"+
		"\u0000\u0000\u00aa\u00ad\u0001\u0000\u0000\u0000\u00ab\u00a9\u0001\u0000"+
		"\u0000\u0000\u00ab\u00ac\u0001\u0000\u0000\u0000\u00ac\u00ae\u0001\u0000"+
		"\u0000\u0000\u00ad\u00ab\u0001\u0000\u0000\u0000\u00ae\u00b2\u0005\u001e"+
		"\u0000\u0000\u00af\u00b1\u0003\u008aE\u0000\u00b0\u00af\u0001\u0000\u0000"+
		"\u0000\u00b1\u00b4\u0001\u0000\u0000\u0000\u00b2\u00b0\u0001\u0000\u0000"+
		"\u0000\u00b2\u00b3\u0001\u0000\u0000\u0000\u00b3\u0003\u0001\u0000\u0000"+
		"\u0000\u00b4\u00b2\u0001\u0000\u0000\u0000\u00b5\u00b9\u0003\u0006\u0003"+
		"\u0000\u00b6\u00b8\u0003\u008aE\u0000\u00b7\u00b6\u0001\u0000\u0000\u0000"+
		"\u00b8\u00bb\u0001\u0000\u0000\u0000\u00b9\u00b7\u0001\u0000\u0000\u0000"+
		"\u00b9\u00ba\u0001\u0000\u0000\u0000\u00ba\u00be\u0001\u0000\u0000\u0000"+
		"\u00bb\u00b9\u0001\u0000\u0000\u0000\u00bc\u00bf\u0003\f\u0006\u0000\u00bd"+
		"\u00bf\u0003\u0012\t\u0000\u00be\u00bc\u0001\u0000\u0000\u0000\u00be\u00bd"+
		"\u0001\u0000\u0000\u0000\u00bf\u00c3\u0001\u0000\u0000\u0000\u00c0\u00c3"+
		"\u0003\f\u0006\u0000\u00c1\u00c3\u0003\u0012\t\u0000\u00c2\u00b5\u0001"+
		"\u0000\u0000\u0000\u00c2\u00c0\u0001\u0000\u0000\u0000\u00c2\u00c1\u0001"+
		"\u0000\u0000\u0000\u00c3\u0005\u0001\u0000\u0000\u0000\u00c4\u00ce\u0003"+
		"\b\u0004\u0000\u00c5\u00c7\u0003\u008aE\u0000\u00c6\u00c5\u0001\u0000"+
		"\u0000\u0000\u00c7\u00ca\u0001\u0000\u0000\u0000\u00c8\u00c6\u0001\u0000"+
		"\u0000\u0000\u00c8\u00c9\u0001\u0000\u0000\u0000\u00c9\u00cb\u0001\u0000"+
		"\u0000\u0000\u00ca\u00c8\u0001\u0000\u0000\u0000\u00cb\u00cd\u0003\b\u0004"+
		"\u0000\u00cc\u00c8\u0001\u0000\u0000\u0000\u00cd\u00d0\u0001\u0000\u0000"+
		"\u0000\u00ce\u00cc\u0001\u0000\u0000\u0000\u00ce\u00cf\u0001\u0000\u0000"+
		"\u0000\u00cf\u0007\u0001\u0000\u0000\u0000\u00d0\u00ce\u0001\u0000\u0000"+
		"\u0000\u00d1\u00d2\u0005\u0013\u0000\u0000\u00d2\u00d4\u0005\n\u0000\u0000"+
		"\u00d3\u00d5\u0003`0\u0000\u00d4\u00d3\u0001\u0000\u0000\u0000\u00d4\u00d5"+
		"\u0001\u0000\u0000\u0000\u00d5\u00d8\u0001\u0000\u0000\u0000\u00d6\u00d8"+
		"\u0003\n\u0005\u0000\u00d7\u00d1\u0001\u0000\u0000\u0000\u00d7\u00d6\u0001"+
		"\u0000\u0000\u0000\u00d8\t\u0001\u0000\u0000\u0000\u00d9\u00da\u0005\u0012"+
		"\u0000\u0000\u00da\u00db\u0003H$\u0000\u00db\u00dc\u0005J\u0000\u0000"+
		"\u00dc\u000b\u0001\u0000\u0000\u0000\u00dd\u00df\u0003\u0084B\u0000\u00de"+
		"\u00dd\u0001\u0000\u0000\u0000\u00df\u00e2\u0001\u0000\u0000\u0000\u00e0"+
		"\u00de\u0001\u0000\u0000\u0000\u00e0\u00e1\u0001\u0000\u0000\u0000\u00e1"+
		"\u00e3\u0001\u0000\u0000\u0000\u00e2\u00e0\u0001\u0000\u0000\u0000\u00e3"+
		"\u00e4\u0005&\u0000\u0000\u00e4\u00e5\u0003x<\u0000\u00e5\u00e6\u0003"+
		"\u000e\u0007\u0000\u00e6\r\u0001\u0000\u0000\u0000\u00e7\u00ed\u0005\u001f"+
		"\u0000\u0000\u00e8\u00ec\u0003\u0010\b\u0000\u00e9\u00ec\u0003\u0012\t"+
		"\u0000\u00ea\u00ec\u0003\u008aE\u0000\u00eb\u00e8\u0001\u0000\u0000\u0000"+
		"\u00eb\u00e9\u0001\u0000\u0000\u0000\u00eb\u00ea\u0001\u0000\u0000\u0000"+
		"\u00ec\u00ef\u0001\u0000\u0000\u0000\u00ed\u00eb\u0001\u0000\u0000\u0000"+
		"\u00ed\u00ee\u0001\u0000\u0000\u0000\u00ee\u00f0\u0001\u0000\u0000\u0000"+
		"\u00ef\u00ed\u0001\u0000\u0000\u0000\u00f0\u00f1\u0005 \u0000\u0000\u00f1"+
		"\u000f\u0001\u0000\u0000\u0000\u00f2\u00f4\u0003\u0082A\u0000\u00f3\u00f2"+
		"\u0001\u0000\u0000\u0000\u00f4\u00f7\u0001\u0000\u0000\u0000\u00f5\u00f3"+
		"\u0001\u0000\u0000\u0000\u00f5\u00f6\u0001\u0000\u0000\u0000\u00f6\u00f8"+
		"\u0001\u0000\u0000\u0000\u00f7\u00f5\u0001\u0000\u0000\u0000\u00f8\u00f9"+
		"\u0005%\u0000\u0000\u00f9\u00fa\u0003 \u0010\u0000\u00fa\u0011\u0001\u0000"+
		"\u0000\u0000\u00fb\u00fd\u0003\u0088D\u0000\u00fc\u00fb\u0001\u0000\u0000"+
		"\u0000\u00fd\u0100\u0001\u0000\u0000\u0000\u00fe\u00fc\u0001\u0000\u0000"+
		"\u0000\u00fe\u00ff\u0001\u0000\u0000\u0000\u00ff\u0101\u0001\u0000\u0000"+
		"\u0000\u0100\u00fe\u0001\u0000\u0000\u0000\u0101\u0102\u0005#\u0000\u0000"+
		"\u0102\u0103\u0003z=\u0000\u0103\u0104\u0003\u0016\u000b\u0000\u0104\u0105"+
		"\u0003\u0014\n\u0000\u0105\u0013\u0001\u0000\u0000\u0000\u0106\u0108\u0005"+
		"$\u0000\u0000\u0107\u0106\u0001\u0000\u0000\u0000\u0107\u0108\u0001\u0000"+
		"\u0000\u0000\u0108\u0109\u0001\u0000\u0000\u0000\u0109\u0110\u0003 \u0010"+
		"\u0000\u010a\u010b\u0005\u0014\u0000\u0000\u010b\u010d\u0003H$\u0000\u010c"+
		"\u010e\u0005\f\u0000\u0000\u010d\u010c\u0001\u0000\u0000\u0000\u010d\u010e"+
		"\u0001\u0000\u0000\u0000\u010e\u0110\u0001\u0000\u0000\u0000\u010f\u0107"+
		"\u0001\u0000\u0000\u0000\u010f\u010a\u0001\u0000\u0000\u0000\u0110\u0015"+
		"\u0001\u0000\u0000\u0000\u0111\u0114\u0003\u0018\f\u0000\u0112\u0114\u0005"+
		"\u001b\u0000\u0000\u0113\u0111\u0001\u0000\u0000\u0000\u0113\u0112\u0001"+
		"\u0000\u0000\u0000\u0114\u0117\u0001\u0000\u0000\u0000\u0115\u0113\u0001"+
		"\u0000\u0000\u0000\u0115\u0116\u0001\u0000\u0000\u0000\u0116\u0017\u0001"+
		"\u0000\u0000\u0000\u0117\u0115\u0001\u0000\u0000\u0000\u0118\u0119\u0005"+
		"2\u0000\u0000\u0119\u011a\u0003\u001a\r\u0000\u011a\u011b\u0005:\u0000"+
		"\u0000\u011b\u011c\u0003\u001a\r\u0000\u011c\u0019\u0001\u0000\u0000\u0000"+
		"\u011d\u0120\u0003\u001c\u000e\u0000\u011e\u0120\u0003\u0080@\u0000\u011f"+
		"\u011d\u0001\u0000\u0000\u0000\u011f\u011e\u0001\u0000\u0000\u0000\u0120"+
		"\u001b\u0001\u0000\u0000\u0000\u0121\u0122\u0005\u000e\u0000\u0000\u0122"+
		"\u0132\u0005 \u0000\u0000\u0123\u0124\u0005\u000e\u0000\u0000\u0124\u0129"+
		"\u0003\u001e\u000f\u0000\u0125\u0126\u0005H\u0000\u0000\u0126\u0128\u0003"+
		"\u001e\u000f\u0000\u0127\u0125\u0001\u0000\u0000\u0000\u0128\u012b\u0001"+
		"\u0000\u0000\u0000\u0129\u0127\u0001\u0000\u0000\u0000\u0129\u012a\u0001"+
		"\u0000\u0000\u0000\u012a\u012d\u0001\u0000\u0000\u0000\u012b\u0129\u0001"+
		"\u0000\u0000\u0000\u012c\u012e\u0005H\u0000\u0000\u012d\u012c\u0001\u0000"+
		"\u0000\u0000\u012d\u012e\u0001\u0000\u0000\u0000\u012e\u012f\u0001\u0000"+
		"\u0000\u0000\u012f\u0130\u0005 \u0000\u0000\u0130\u0132\u0001\u0000\u0000"+
		"\u0000\u0131\u0121\u0001\u0000\u0000\u0000\u0131\u0123\u0001\u0000\u0000"+
		"\u0000\u0132\u001d\u0001\u0000\u0000\u0000\u0133\u0135\u0003\b\u0004\u0000"+
		"\u0134\u0133\u0001\u0000\u0000\u0000\u0135\u0138\u0001\u0000\u0000\u0000"+
		"\u0136\u0134\u0001\u0000\u0000\u0000\u0136\u0137\u0001\u0000\u0000\u0000"+
		"\u0137\u013a\u0001\u0000\u0000\u0000\u0138\u0136\u0001\u0000\u0000\u0000"+
		"\u0139\u013b\u0005P\u0000\u0000\u013a\u0139\u0001\u0000\u0000\u0000\u013a"+
		"\u013b\u0001\u0000\u0000\u0000\u013b\u013c\u0001\u0000\u0000\u0000\u013c"+
		"\u013d\u0005\r\u0000\u0000\u013d\u013e\u0005O\u0000\u0000\u013e\u013f"+
		"\u0003\u0080@\u0000\u013f\u001f\u0001\u0000\u0000\u0000\u0140\u0146\u0005"+
		"\u001f\u0000\u0000\u0141\u0145\u0003\"\u0011\u0000\u0142\u0145\u0005!"+
		"\u0000\u0000\u0143\u0145\u0003\u008aE\u0000\u0144\u0141\u0001\u0000\u0000"+
		"\u0000\u0144\u0142\u0001\u0000\u0000\u0000\u0144\u0143\u0001\u0000\u0000"+
		"\u0000\u0145\u0148\u0001\u0000\u0000\u0000\u0146\u0144\u0001\u0000\u0000"+
		"\u0000\u0146\u0147\u0001\u0000\u0000\u0000\u0147\u0149\u0001\u0000\u0000"+
		"\u0000\u0148\u0146\u0001\u0000\u0000\u0000\u0149\u014a\u0005 \u0000\u0000"+
		"\u014a!\u0001\u0000\u0000\u0000\u014b\u014c\u0003t:\u0000\u014c\u014d"+
		"\u0005\u001b\u0000\u0000\u014d\u0159\u0001\u0000\u0000\u0000\u014e\u0150"+
		"\u0003t:\u0000\u014f\u014e\u0001\u0000\u0000\u0000\u014f\u0150\u0001\u0000"+
		"\u0000\u0000\u0150\u0151\u0001\u0000\u0000\u0000\u0151\u0159\u0003 \u0010"+
		"\u0000\u0152\u0159\u0003$\u0012\u0000\u0153\u0159\u0003<\u001e\u0000\u0154"+
		"\u0159\u0003@ \u0000\u0155\u0159\u00034\u001a\u0000\u0156\u0159\u0003"+
		"&\u0013\u0000\u0157\u0159\u0003.\u0017\u0000\u0158\u014b\u0001\u0000\u0000"+
		"\u0000\u0158\u014f\u0001\u0000\u0000\u0000\u0158\u0152\u0001\u0000\u0000"+
		"\u0000\u0158\u0153\u0001\u0000\u0000\u0000\u0158\u0154\u0001\u0000\u0000"+
		"\u0000\u0158\u0155\u0001\u0000\u0000\u0000\u0158\u0156\u0001\u0000\u0000"+
		"\u0000\u0158\u0157\u0001\u0000\u0000\u0000\u0159#\u0001\u0000\u0000\u0000"+
		"\u015a\u015b\u0005\n\u0000\u0000\u015b\u015d\u0003`0\u0000\u015c\u015e"+
		"\u0003X,\u0000\u015d\u015c\u0001\u0000\u0000\u0000\u015d\u015e\u0001\u0000"+
		"\u0000\u0000\u015e\u0162\u0001\u0000\u0000\u0000\u015f\u0160\u0005\n\u0000"+
		"\u0000\u0160\u0162\u0003X,\u0000\u0161\u015a\u0001\u0000\u0000\u0000\u0161"+
		"\u015f\u0001\u0000\u0000\u0000\u0162%\u0001\u0000\u0000\u0000\u0163\u0164"+
		"\u00053\u0000\u0000\u0164\u0165\u0003,\u0016\u0000\u0165\u0170\u0003 "+
		"\u0010\u0000\u0166\u0168\u0003\u008aE\u0000\u0167\u0166\u0001\u0000\u0000"+
		"\u0000\u0168\u016b\u0001\u0000\u0000\u0000\u0169\u0167\u0001\u0000\u0000"+
		"\u0000\u0169\u016a\u0001\u0000\u0000\u0000\u016a\u016c\u0001\u0000\u0000"+
		"\u0000\u016b\u0169\u0001\u0000\u0000\u0000\u016c\u016d\u0005)\u0000\u0000"+
		"\u016d\u016e\u0003*\u0015\u0000\u016e\u016f\u0005\u001b\u0000\u0000\u016f"+
		"\u0171\u0001\u0000\u0000\u0000\u0170\u0169\u0001\u0000\u0000\u0000\u0170"+
		"\u0171\u0001\u0000\u0000\u0000\u0171\'\u0001\u0000\u0000\u0000\u0172\u0173"+
		"\u0005)\u0000\u0000\u0173\u0174\u0005:\u0000\u0000\u0174\u0175\u0003*"+
		"\u0015\u0000\u0175\u0176\u0005\u001b\u0000\u0000\u0176)\u0001\u0000\u0000"+
		"\u0000\u0177\u017a\u0003H$\u0000\u0178\u017a\u0003\"\u0011\u0000\u0179"+
		"\u0177\u0001\u0000\u0000\u0000\u0179\u0178\u0001\u0000\u0000\u0000\u017a"+
		"+\u0001\u0000\u0000\u0000\u017b\u017c\u0003`0\u0000\u017c-\u0001\u0000"+
		"\u0000\u0000\u017d\u017e\u00053\u0000\u0000\u017e\u017f\u0005\u001f\u0000"+
		"\u0000\u017f\u0180\u0005\u001b\u0000\u0000\u0180\u0181\u00030\u0018\u0000"+
		"\u0181\u0182\u0005 \u0000\u0000\u0182/\u0001\u0000\u0000\u0000\u0183\u0188"+
		"\u00032\u0019\u0000\u0184\u0187\u00032\u0019\u0000\u0185\u0187\u0003\u008a"+
		"E\u0000\u0186\u0184\u0001\u0000\u0000\u0000\u0186\u0185\u0001\u0000\u0000"+
		"\u0000\u0187\u018a\u0001\u0000\u0000\u0000\u0188\u0186\u0001\u0000\u0000"+
		"\u0000\u0188\u0189\u0001\u0000\u0000\u0000\u0189\u018c\u0001\u0000\u0000"+
		"\u0000\u018a\u0188\u0001\u0000\u0000\u0000\u018b\u018d\u0003(\u0014\u0000"+
		"\u018c\u018b\u0001\u0000\u0000\u0000\u018c\u018d\u0001\u0000\u0000\u0000"+
		"\u018d1\u0001\u0000\u0000\u0000\u018e\u018f\u0003,\u0016\u0000\u018f\u0190"+
		"\u0005:\u0000\u0000\u0190\u0191\u0003*\u0015\u0000\u0191\u0192\u0005\u001b"+
		"\u0000\u0000\u01923\u0001\u0000\u0000\u0000\u0193\u0195\u00053\u0000\u0000"+
		"\u0194\u0196\u0003H$\u0000\u0195\u0194\u0001\u0000\u0000\u0000\u0195\u0196"+
		"\u0001\u0000\u0000\u0000\u0196\u0197\u0001\u0000\u0000\u0000\u0197\u0198"+
		"\u0005\u001f\u0000\u0000\u0198\u0199\u0005\u001b\u0000\u0000\u0199\u019a"+
		"\u00036\u001b\u0000\u019a\u019b\u0005 \u0000\u0000\u019b5\u0001\u0000"+
		"\u0000\u0000\u019c\u019e\u0003\u008aE\u0000\u019d\u019c\u0001\u0000\u0000"+
		"\u0000\u019e\u01a1\u0001\u0000\u0000\u0000\u019f\u019d\u0001\u0000\u0000"+
		"\u0000\u019f\u01a0\u0001\u0000\u0000\u0000\u01a0\u01a2\u0001\u0000\u0000"+
		"\u0000\u01a1\u019f\u0001\u0000\u0000\u0000\u01a2\u01a7\u00038\u001c\u0000"+
		"\u01a3\u01a6\u00038\u001c\u0000\u01a4\u01a6\u0003\u008aE\u0000\u01a5\u01a3"+
		"\u0001\u0000\u0000\u0000\u01a5\u01a4\u0001\u0000\u0000\u0000\u01a6\u01a9"+
		"\u0001\u0000\u0000\u0000\u01a7\u01a5\u0001\u0000\u0000\u0000\u01a7\u01a8"+
		"\u0001\u0000\u0000\u0000\u01a8\u01ab\u0001\u0000\u0000\u0000\u01a9\u01a7"+
		"\u0001\u0000\u0000\u0000\u01aa\u01ac\u0003(\u0014\u0000\u01ab\u01aa\u0001"+
		"\u0000\u0000\u0000\u01ab\u01ac\u0001\u0000\u0000\u0000\u01ac7\u0001\u0000"+
		"\u0000\u0000\u01ad\u01ae\u0003:\u001d\u0000\u01ae\u01af\u0005:\u0000\u0000"+
		"\u01af\u01b0\u0003*\u0015\u0000\u01b0\u01b1\u0005\u001b\u0000\u0000\u01b1"+
		"9\u0001\u0000\u0000\u0000\u01b2\u01b5\u0003H$\u0000\u01b3\u01b5\u0003"+
		"h4\u0000\u01b4\u01b2\u0001\u0000\u0000\u0000\u01b4\u01b3\u0001\u0000\u0000"+
		"\u0000\u01b5;\u0001\u0000\u0000\u0000\u01b6\u01b7\u0003\u0086C\u0000\u01b7"+
		"\u01b8\u0003|>\u0000\u01b8\u01b9\u0003>\u001f\u0000\u01b9=\u0001\u0000"+
		"\u0000\u0000\u01ba\u01bf\u00059\u0000\u0000\u01bb\u01c0\u0003H$\u0000"+
		"\u01bc\u01c0\u00034\u001a\u0000\u01bd\u01c0\u0003.\u0017\u0000\u01be\u01c0"+
		"\u0003$\u0012\u0000\u01bf\u01bb\u0001\u0000\u0000\u0000\u01bf\u01bc\u0001"+
		"\u0000\u0000\u0000\u01bf\u01bd\u0001\u0000\u0000\u0000\u01bf\u01be\u0001"+
		"\u0000\u0000\u0000\u01c0\u01c3\u0001\u0000\u0000\u0000\u01c1\u01c3\u0003"+
		"X,\u0000\u01c2\u01ba\u0001\u0000\u0000\u0000\u01c2\u01c1\u0001\u0000\u0000"+
		"\u0000\u01c3?\u0001\u0000\u0000\u0000\u01c4\u01c6\u0005*\u0000\u0000\u01c5"+
		"\u01c7\u0003`0\u0000\u01c6\u01c5\u0001\u0000\u0000\u0000\u01c6\u01c7\u0001"+
		"\u0000\u0000\u0000\u01c7\u01cd\u0001\u0000\u0000\u0000\u01c8\u01c9\u0005"+
		"+\u0000\u0000\u01c9\u01cd\u0005\n\u0000\u0000\u01ca\u01cd\u0005-\u0000"+
		"\u0000\u01cb\u01cd\u0005,\u0000\u0000\u01cc\u01c4\u0001\u0000\u0000\u0000"+
		"\u01cc\u01c8\u0001\u0000\u0000\u0000\u01cc\u01ca\u0001\u0000\u0000\u0000"+
		"\u01cc\u01cb\u0001\u0000\u0000\u0000\u01cdA\u0001\u0000\u0000\u0000\u01ce"+
		"\u01cf\u0005\u0007\u0000\u0000\u01cf\u01d0\u0003D\"\u0000\u01d0\u01d1"+
		"\u0005\u0002\u0000\u0000\u01d1C\u0001\u0000\u0000\u0000\u01d2\u01d7\u0005"+
		"A\u0000\u0000\u01d3\u01d7\u0005B\u0000\u0000\u01d4\u01d7\u0005C\u0000"+
		"\u0000\u01d5\u01d7\u0003F#\u0000\u01d6\u01d2\u0001\u0000\u0000\u0000\u01d6"+
		"\u01d3\u0001\u0000\u0000\u0000\u01d6\u01d4\u0001\u0000\u0000\u0000\u01d6"+
		"\u01d5\u0001\u0000\u0000\u0000\u01d7\u01d8\u0001\u0000\u0000\u0000\u01d8"+
		"\u01d6\u0001\u0000\u0000\u0000\u01d8\u01d9\u0001\u0000\u0000\u0000\u01d9"+
		"E\u0001\u0000\u0000\u0000\u01da\u01db\u0005\b\u0000\u0000\u01db\u01dc"+
		"\u0003j5\u0000\u01dc\u01de\u0005 \u0000\u0000\u01dd\u01df\u0005U\u0000"+
		"\u0000\u01de\u01dd\u0001\u0000\u0000\u0000\u01de\u01df\u0001\u0000\u0000"+
		"\u0000\u01dfG\u0001\u0000\u0000\u0000\u01e0\u01e3\u0003L&\u0000\u01e1"+
		"\u01e3\u0003b1\u0000\u01e2\u01e0\u0001\u0000\u0000\u0000\u01e2\u01e1\u0001"+
		"\u0000\u0000\u0000\u01e3I\u0001\u0000\u0000\u0000\u01e4\u01e5\u0005;\u0000"+
		"\u0000\u01e5\u01e6\u0003H$\u0000\u01e6\u01e7\u0005\f\u0000\u0000\u01e7"+
		"K\u0001\u0000\u0000\u0000\u01e8\u01e9\u0006&\uffff\uffff\u0000\u01e9\u01eb"+
		"\u0003N\'\u0000\u01ea\u01ec\u0003n7\u0000\u01eb\u01ea\u0001\u0000\u0000"+
		"\u0000\u01eb\u01ec\u0001\u0000\u0000\u0000\u01ec\u01f1\u0001\u0000\u0000"+
		"\u0000\u01ed\u01ee\u0003R)\u0000\u01ee\u01ef\u0003L&\u0002\u01ef\u01f1"+
		"\u0001\u0000\u0000\u0000\u01f0\u01e8\u0001\u0000\u0000\u0000\u01f0\u01ed"+
		"\u0001\u0000\u0000\u0000\u01f1\u01f6\u0001\u0000\u0000\u0000\u01f2\u01f3"+
		"\n\u0001\u0000\u0000\u01f3\u01f5\u0003T*\u0000\u01f4\u01f2\u0001\u0000"+
		"\u0000\u0000\u01f5\u01f8\u0001\u0000\u0000\u0000\u01f6\u01f4\u0001\u0000"+
		"\u0000\u0000\u01f6\u01f7\u0001\u0000\u0000\u0000\u01f7M\u0001\u0000\u0000"+
		"\u0000\u01f8\u01f6\u0001\u0000\u0000\u0000\u01f9\u0204\u0003f3\u0000\u01fa"+
		"\u0204\u0003P(\u0000\u01fb\u0204\u0003j5\u0000\u01fc\u0204\u0003\\.\u0000"+
		"\u01fd\u0204\u0003Z-\u0000\u01fe\u0204\u0003J%\u0000\u01ff\u0204\u0003"+
		"V+\u0000\u0200\u0204\u0005\u0015\u0000\u0000\u0201\u0204\u0005\u0016\u0000"+
		"\u0000\u0202\u0204\u0005\u0017\u0000\u0000\u0203\u01f9\u0001\u0000\u0000"+
		"\u0000\u0203\u01fa\u0001\u0000\u0000\u0000\u0203\u01fb\u0001\u0000\u0000"+
		"\u0000\u0203\u01fc\u0001\u0000\u0000\u0000\u0203\u01fd\u0001\u0000\u0000"+
		"\u0000\u0203\u01fe\u0001\u0000\u0000\u0000\u0203\u01ff\u0001\u0000\u0000"+
		"\u0000\u0203\u0200\u0001\u0000\u0000\u0000\u0203\u0201\u0001\u0000\u0000"+
		"\u0000\u0203\u0202\u0001\u0000\u0000\u0000\u0204O\u0001\u0000\u0000\u0000"+
		"\u0205\u020b\u0005\u0010\u0000\u0000\u0206\u020a\u0003\"\u0011\u0000\u0207"+
		"\u020a\u0005!\u0000\u0000\u0208\u020a\u0003\u008aE\u0000\u0209\u0206\u0001"+
		"\u0000\u0000\u0000\u0209\u0207\u0001\u0000\u0000\u0000\u0209\u0208\u0001"+
		"\u0000\u0000\u0000\u020a\u020d\u0001\u0000\u0000\u0000\u020b\u0209\u0001"+
		"\u0000\u0000\u0000\u020b\u020c\u0001\u0000\u0000\u0000\u020c\u020e\u0001"+
		"\u0000\u0000\u0000\u020d\u020b\u0001\u0000\u0000\u0000\u020e\u0215\u0005"+
		" \u0000\u0000\u020f\u0210\u0005\u0011\u0000\u0000\u0210\u0212\u0003H$"+
		"\u0000\u0211\u0213\u0005\f\u0000\u0000\u0212\u0211\u0001\u0000\u0000\u0000"+
		"\u0212\u0213\u0001\u0000\u0000\u0000\u0213\u0215\u0001\u0000\u0000\u0000"+
		"\u0214\u0205\u0001\u0000\u0000\u0000\u0214\u020f\u0001\u0000\u0000\u0000"+
		"\u0215Q\u0001\u0000\u0000\u0000\u0216\u0217\u0005L\u0000\u0000\u0217S"+
		"\u0001\u0000\u0000\u0000\u0218\u0219\u0005M\u0000\u0000\u0219U\u0001\u0000"+
		"\u0000\u0000\u021a\u021b\u0003~?\u0000\u021b\u021c\u0003`0\u0000\u021c"+
		"W\u0001\u0000\u0000\u0000\u021d\u021f\u0005\u001b\u0000\u0000\u021e\u021d"+
		"\u0001\u0000\u0000\u0000\u021e\u021f\u0001\u0000\u0000\u0000\u021f\u0220"+
		"\u0001\u0000\u0000\u0000\u0220\u0223\u0003B!\u0000\u0221\u0223\u0003L"+
		"&\u0000\u0222\u021e\u0001\u0000\u0000\u0000\u0222\u0221\u0001\u0000\u0000"+
		"\u0000\u0223Y\u0001\u0000\u0000\u0000\u0224\u0225\u0005\u000f\u0000\u0000"+
		"\u0225\u0235\u0005J\u0000\u0000\u0226\u0227\u0005\u000f\u0000\u0000\u0227"+
		"\u022c\u0003H$\u0000\u0228\u0229\u0005H\u0000\u0000\u0229\u022b\u0003"+
		"H$\u0000\u022a\u0228\u0001\u0000\u0000\u0000\u022b\u022e\u0001\u0000\u0000"+
		"\u0000\u022c\u022a\u0001\u0000\u0000\u0000\u022c\u022d\u0001\u0000\u0000"+
		"\u0000\u022d\u0230\u0001\u0000\u0000\u0000\u022e\u022c\u0001\u0000\u0000"+
		"\u0000\u022f\u0231\u0005H\u0000\u0000\u0230\u022f\u0001\u0000\u0000\u0000"+
		"\u0230\u0231\u0001\u0000\u0000\u0000\u0231\u0232\u0001\u0000\u0000\u0000"+
		"\u0232\u0233\u0005J\u0000\u0000\u0233\u0235\u0001\u0000\u0000\u0000\u0234"+
		"\u0224\u0001\u0000\u0000\u0000\u0234\u0226\u0001\u0000\u0000\u0000\u0235"+
		"[\u0001\u0000\u0000\u0000\u0236\u0237\u0005\u000e\u0000\u0000\u0237\u0247"+
		"\u0005 \u0000\u0000\u0238\u0239\u0005\u000e\u0000\u0000\u0239\u023e\u0003"+
		"^/\u0000\u023a\u023b\u0005H\u0000\u0000\u023b\u023d\u0003^/\u0000\u023c"+
		"\u023a\u0001\u0000\u0000\u0000\u023d\u0240\u0001\u0000\u0000\u0000\u023e"+
		"\u023c\u0001\u0000\u0000\u0000\u023e\u023f\u0001\u0000\u0000\u0000\u023f"+
		"\u0242\u0001\u0000\u0000\u0000\u0240\u023e\u0001\u0000\u0000\u0000\u0241"+
		"\u0243\u0005H\u0000\u0000\u0242\u0241\u0001\u0000\u0000\u0000\u0242\u0243"+
		"\u0001\u0000\u0000\u0000\u0243\u0244\u0001\u0000\u0000\u0000\u0244\u0245"+
		"\u0005 \u0000\u0000\u0245\u0247\u0001\u0000\u0000\u0000\u0246\u0236\u0001"+
		"\u0000\u0000\u0000\u0246\u0238\u0001\u0000\u0000\u0000\u0247]\u0001\u0000"+
		"\u0000\u0000\u0248\u024a\u0003\b\u0004\u0000\u0249\u0248\u0001\u0000\u0000"+
		"\u0000\u024a\u024d\u0001\u0000\u0000\u0000\u024b\u0249\u0001\u0000\u0000"+
		"\u0000\u024b\u024c\u0001\u0000\u0000\u0000\u024c\u024e\u0001\u0000\u0000"+
		"\u0000\u024d\u024b\u0001\u0000\u0000\u0000\u024e\u024f\u0005\r\u0000\u0000"+
		"\u024f\u0250\u0005O\u0000\u0000\u0250\u0251\u0003H$\u0000\u0251_\u0001"+
		"\u0000\u0000\u0000\u0252\u0253\u0005=\u0000\u0000\u0253\u0258\u0003H$"+
		"\u0000\u0254\u0255\u0005H\u0000\u0000\u0255\u0257\u0003H$\u0000\u0256"+
		"\u0254\u0001\u0000\u0000\u0000\u0257\u025a\u0001\u0000\u0000\u0000\u0258"+
		"\u0256\u0001\u0000\u0000\u0000\u0258\u0259\u0001\u0000\u0000\u0000\u0259"+
		"\u025c\u0001\u0000\u0000\u0000\u025a\u0258\u0001\u0000\u0000\u0000\u025b"+
		"\u025d\u0005H\u0000\u0000\u025c\u025b\u0001\u0000\u0000\u0000\u025c\u025d"+
		"\u0001\u0000\u0000\u0000\u025d\u025e\u0001\u0000\u0000\u0000\u025e\u025f"+
		"\u0005I\u0000\u0000\u025f\u0269\u0001\u0000\u0000\u0000\u0260\u0261\u0005"+
		"=\u0000\u0000\u0261\u0269\u0005I\u0000\u0000\u0262\u0269\u0005<\u0000"+
		"\u0000\u0263\u0265\u0003b1\u0000\u0264\u0263\u0001\u0000\u0000\u0000\u0265"+
		"\u0266\u0001\u0000\u0000\u0000\u0266\u0264\u0001\u0000\u0000\u0000\u0266"+
		"\u0267\u0001\u0000\u0000\u0000\u0267\u0269\u0001\u0000\u0000\u0000\u0268"+
		"\u0252\u0001\u0000\u0000\u0000\u0268\u0260\u0001\u0000\u0000\u0000\u0268"+
		"\u0262\u0001\u0000\u0000\u0000\u0268\u0264\u0001\u0000\u0000\u0000\u0269"+
		"a\u0001\u0000\u0000\u0000\u026a\u026b\u0005\u0004\u0000\u0000\u026b\u026c"+
		"\u0003d2\u0000\u026c\u026d\u0005@\u0000\u0000\u026dc\u0001\u0000\u0000"+
		"\u0000\u026e\u0272\u0005?\u0000\u0000\u026f\u0272\u0003\u008eG\u0000\u0270"+
		"\u0272\u0003f3\u0000\u0271\u026e\u0001\u0000\u0000\u0000\u0271\u026f\u0001"+
		"\u0000\u0000\u0000\u0271\u0270\u0001\u0000\u0000\u0000\u0272\u0275\u0001"+
		"\u0000\u0000\u0000\u0273\u0271\u0001\u0000\u0000\u0000\u0273\u0274\u0001"+
		"\u0000\u0000\u0000\u0274e\u0001\u0000\u0000\u0000\u0275\u0273\u0001\u0000"+
		"\u0000\u0000\u0276\u0277\u0005\t\u0000\u0000\u0277\u0278\u0003H$\u0000"+
		"\u0278\u0279\u0005J\u0000\u0000\u0279g\u0001\u0000\u0000\u0000\u027a\u027c"+
		"\u0005\t\u0000\u0000\u027b\u027d\u0003p8\u0000\u027c\u027b\u0001\u0000"+
		"\u0000\u0000\u027d\u027e\u0001\u0000\u0000\u0000\u027e\u027c\u0001\u0000"+
		"\u0000\u0000\u027e\u027f\u0001\u0000\u0000\u0000\u027f\u0280\u0001\u0000"+
		"\u0000\u0000\u0280\u0281\u0005J\u0000\u0000\u0281i\u0001\u0000\u0000\u0000"+
		"\u0282\u0286\u0003l6\u0000\u0283\u0285\u0003p8\u0000\u0284\u0283\u0001"+
		"\u0000\u0000\u0000\u0285\u0288\u0001\u0000\u0000\u0000\u0286\u0284\u0001"+
		"\u0000\u0000\u0000\u0286\u0287\u0001\u0000\u0000\u0000\u0287k\u0001\u0000"+
		"\u0000\u0000\u0288\u0286\u0001\u0000\u0000\u0000\u0289\u028d\u0005\r\u0000"+
		"\u0000\u028a\u028b\u0005K\u0000\u0000\u028b\u028d\u0007\u0000\u0000\u0000"+
		"\u028c\u0289\u0001\u0000\u0000\u0000\u028c\u028a\u0001\u0000\u0000\u0000"+
		"\u028dm\u0001\u0000\u0000\u0000\u028e\u028f\u0005N\u0000\u0000\u028f\u0290"+
		"\u0003H$\u0000\u0290o\u0001\u0000\u0000\u0000\u0291\u0292\u0003r9\u0000"+
		"\u0292\u0294\u0003~?\u0000\u0293\u0295\u0003`0\u0000\u0294\u0293\u0001"+
		"\u0000\u0000\u0000\u0294\u0295\u0001\u0000\u0000\u0000\u0295\u0299\u0001"+
		"\u0000\u0000\u0000\u0296\u0299\u0003v;\u0000\u0297\u0299\u0005G\u0000"+
		"\u0000\u0298\u0291\u0001\u0000\u0000\u0000\u0298\u0296\u0001\u0000\u0000"+
		"\u0000\u0298\u0297\u0001\u0000\u0000\u0000\u0299q\u0001\u0000\u0000\u0000"+
		"\u029a\u029b\u0007\u0001\u0000\u0000\u029bs\u0001\u0000\u0000\u0000\u029c"+
		"\u029d\u00058\u0000\u0000\u029d\u029e\u0005X\u0000\u0000\u029eu\u0001"+
		"\u0000\u0000\u0000\u029f\u02a0\u0005F\u0000\u0000\u02a0\u02a1\u0005W\u0000"+
		"\u0000\u02a1w\u0001\u0000\u0000\u0000\u02a2\u02a6\u0005\n\u0000\u0000"+
		"\u02a3\u02a6\u0005\u000b\u0000\u0000\u02a4\u02a6\u0003b1\u0000\u02a5\u02a2"+
		"\u0001\u0000\u0000\u0000\u02a5\u02a3\u0001\u0000\u0000\u0000\u02a5\u02a4"+
		"\u0001\u0000\u0000\u0000\u02a6y\u0001\u0000\u0000\u0000\u02a7\u02aa\u0005"+
		"\n\u0000\u0000\u02a8\u02aa\u0003b1\u0000\u02a9\u02a7\u0001\u0000\u0000"+
		"\u0000\u02a9\u02a8\u0001\u0000\u0000\u0000\u02aa{\u0001\u0000\u0000\u0000"+
		"\u02ab\u02ac\u0005\n\u0000\u0000\u02ac}\u0001\u0000\u0000\u0000\u02ad"+
		"\u02ae\u0007\u0002\u0000\u0000\u02ae\u007f\u0001\u0000\u0000\u0000\u02af"+
		"\u02b0\u0007\u0003\u0000\u0000\u02b0\u0081\u0001\u0000\u0000\u0000\u02b1"+
		"\u02b2\u0007\u0004\u0000\u0000\u02b2\u0083\u0001\u0000\u0000\u0000\u02b3"+
		"\u02b4\u0005(\u0000\u0000\u02b4\u0085\u0001\u0000\u0000\u0000\u02b5\u02b7"+
		"\u00051\u0000\u0000\u02b6\u02b5\u0001\u0000\u0000\u0000\u02b6\u02b7\u0001"+
		"\u0000\u0000\u0000\u02b7\u02b8\u0001\u0000\u0000\u0000\u02b8\u02be\u0005"+
		"0\u0000\u0000\u02b9\u02bb\u0005.\u0000\u0000\u02ba\u02b9\u0001\u0000\u0000"+
		"\u0000\u02ba\u02bb\u0001\u0000\u0000\u0000\u02bb\u02bc\u0001\u0000\u0000"+
		"\u0000\u02bc\u02be\u0005/\u0000\u0000\u02bd\u02b6\u0001\u0000\u0000\u0000"+
		"\u02bd\u02ba\u0001\u0000\u0000\u0000\u02be\u0087\u0001\u0000\u0000\u0000"+
		"\u02bf\u02c0\u0007\u0005\u0000\u0000\u02c0\u0089\u0001\u0000\u0000\u0000"+
		"\u02c1\u02c4\u0003\u008cF\u0000\u02c2\u02c4\u0005\u001b\u0000\u0000\u02c3"+
		"\u02c1\u0001\u0000\u0000\u0000\u02c3\u02c2\u0001\u0000\u0000\u0000\u02c4"+
		"\u008b\u0001\u0000\u0000\u0000\u02c5\u02c6\u0007\u0006\u0000\u0000\u02c6"+
		"\u008d\u0001\u0000\u0000\u0000\u02c7\u02c8\u0005\u0003\u0000\u0000\u02c8"+
		"\u02c9\u0007\u0007\u0000\u0000\u02c9\u008f\u0001\u0000\u0000\u0000Y\u0093"+
		"\u0097\u009d\u00a2\u00ab\u00b2\u00b9\u00be\u00c2\u00c8\u00ce\u00d4\u00d7"+
		"\u00e0\u00eb\u00ed\u00f5\u00fe\u0107\u010d\u010f\u0113\u0115\u011f\u0129"+
		"\u012d\u0131\u0136\u013a\u0144\u0146\u014f\u0158\u015d\u0161\u0169\u0170"+
		"\u0179\u0186\u0188\u018c\u0195\u019f\u01a5\u01a7\u01ab\u01b4\u01bf\u01c2"+
		"\u01c6\u01cc\u01d6\u01d8\u01de\u01e2\u01eb\u01f0\u01f6\u0203\u0209\u020b"+
		"\u0212\u0214\u021e\u0222\u022c\u0230\u0234\u023e\u0242\u0246\u024b\u0258"+
		"\u025c\u0266\u0268\u0271\u0273\u027e\u0286\u028c\u0294\u0298\u02a5\u02a9"+
		"\u02b6\u02ba\u02bd\u02c3";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}