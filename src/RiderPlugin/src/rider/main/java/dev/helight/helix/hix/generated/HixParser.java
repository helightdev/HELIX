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
		RC=32, SEMICOLON=33, COMMA=34, KEYWORD_FUNC=35, KEYWORD_DELEGATE=36, KEYWORD_TYPE=37,
		KEYWORD_DO=38, KEYWORD_EXPRESSION=39, KEYWORD_MIXIN=40, KEYWORD_PRELUDE=41,
		KEYWORD_DERIVATION=42, KEYWORD_ELSE=43, KEYWORD_RETURN=44, KEYWORD_GOTO=45,
		KEYWORD_BREAK=46, KEYWORD_CONTINUE=47, KEYWORD_LOCAL=48, KEYWORD_CARRY=49,
		KEYWORD_WHEN=50, KEYWORD_PURE=51, KEYWORD_INLINE=52, KEYWORD_NOINLINE=53,
		KEYWORD_STRICT=54, LABEL_PREFIX=55, TOPLEVEL_DERIVATION=56, ASSIGN=57,
		ARROW=58, TOPLEVEL_VALUE_EXPRESSION=59, EMPTY_PARAMETERS=60, BEGIN_PARAMETERS=61,
		METADATA_WHITESPACE=62, DERIVATION_WHITESPACE=63, ARGUMENT_TEXT=64, ARGUMENT_END=65,
		CONTENT_WRAP=66, CONTENT_LINEBREAK=67, CONTENT_TEXT=68, VALUE_FUNCTION=69,
		VALUE_PREDICATE=70, VALUE_MEMBER=71, VALUE_WRAP=72, VALUE_DELIMITER=73,
		END_PARAMETERS=74, VALUE_END_INLINE=75, VALUE_SMART_ROOT=76, NOT_VALUE=77,
		VALUE_CHECK=78, VALUE_ELVIS=79, VALUE_ASSIGN=80, VALUE_EXPAND=81, VALUE_WHITESPACE=82,
		ESCAPE_MACRO=83, ESCAPE_LITERAL=84, ESCAPE_HEX=85, END_CONTENT_INTERPOLATE=86,
		FUNCTION_IDENTIFIER=87, MEMBER_IDENTIFIER=88, LABEL_IDENTIFIER=89, TOPLEVEL_SMART_ROOT=90,
		TOPLEVEL_NULL=91, DERIVATION_CHECK=92;
	public static final int
		RULE_compilationUnit = 0, RULE_fileMetadataSection = 1, RULE_topLevelDeclaration = 2,
		RULE_metadataList = 3, RULE_metadata = 4, RULE_metadataValue = 5, RULE_mixinDeclaration = 6,
		RULE_typeDeclaration = 7, RULE_mixinBody = 8, RULE_expressionDeclaration = 9,
		RULE_funcDeclaration = 10, RULE_directFunctionSignature = 11, RULE_functionBody = 12,
		RULE_patternExpression = 13, RULE_patternPrimary = 14, RULE_tablePattern = 15,
		RULE_tuplePattern = 16, RULE_delegatePattern = 17, RULE_patternParameterList = 18,
		RULE_patternField = 19, RULE_patternIdentifier = 20, RULE_statementBlock = 21,
		RULE_statement = 22, RULE_invocationStatement = 23, RULE_whenConditionStatement = 24,
		RULE_whenElseBranch = 25, RULE_whenResult = 26, RULE_whenChainCondition = 27,
		RULE_whenChainStatement = 28, RULE_whenChainBody = 29, RULE_whenChainBranch = 30,
		RULE_whenValueStatement = 31, RULE_whenValueBody = 32, RULE_whenValueBranch = 33,
		RULE_whenValueCondition = 34, RULE_localDeclarationStatement = 35, RULE_localAssignmentStatement = 36,
		RULE_toplevelDerivationStatement = 37, RULE_assignedValue = 38, RULE_controlflowStatement = 39,
		RULE_contentBlock = 40, RULE_contentBody = 41, RULE_contentInterpolate = 42,
		RULE_value = 43, RULE_valueExpression = 44, RULE_nonArgumentValue = 45,
		RULE_primaryValue = 46, RULE_lambdaValue = 47, RULE_prefixOperators = 48,
		RULE_postfixOperators = 49, RULE_valueStatement = 50, RULE_tailValue = 51,
		RULE_tupleValue = 52, RULE_tableValue = 53, RULE_tableKeyedEntry = 54,
		RULE_valueList = 55, RULE_argumentValue = 56, RULE_argumentBody = 57,
		RULE_inlineValue = 58, RULE_inlineTransformation = 59, RULE_derivation = 60,
		RULE_derivationRoot = 61, RULE_elvisValue = 62, RULE_transformationPart = 63,
		RULE_functionChainType = 64, RULE_labelIdentifier = 65, RULE_memberIdentifier = 66,
		RULE_mixinIdentifier = 67, RULE_functionDeclarationIdentifier = 68, RULE_variableIdentifier = 69,
		RULE_functionIdentifier = 70, RULE_invocationIdentifier = 71, RULE_kindIdentifier = 72,
		RULE_expressionModifier = 73, RULE_mixinModifier = 74, RULE_funcModifier = 75,
		RULE_trivia = 76, RULE_comment = 77, RULE_escaped = 78;
	private static String[] makeRuleNames() {
		return new String[] {
			"compilationUnit", "fileMetadataSection", "topLevelDeclaration", "metadataList",
			"metadata", "metadataValue", "mixinDeclaration", "typeDeclaration", "mixinBody",
			"expressionDeclaration", "funcDeclaration", "directFunctionSignature",
			"functionBody", "patternExpression", "patternPrimary", "tablePattern",
			"tuplePattern", "delegatePattern", "patternParameterList", "patternField",
			"patternIdentifier", "statementBlock", "statement", "invocationStatement",
			"whenConditionStatement", "whenElseBranch", "whenResult", "whenChainCondition",
			"whenChainStatement", "whenChainBody", "whenChainBranch", "whenValueStatement",
			"whenValueBody", "whenValueBranch", "whenValueCondition", "localDeclarationStatement",
			"localAssignmentStatement", "toplevelDerivationStatement", "assignedValue",
			"controlflowStatement", "contentBlock", "contentBody", "contentInterpolate",
			"value", "valueExpression", "nonArgumentValue", "primaryValue", "lambdaValue",
			"prefixOperators", "postfixOperators", "valueStatement", "tailValue",
			"tupleValue", "tableValue", "tableKeyedEntry", "valueList", "argumentValue",
			"argumentBody", "inlineValue", "inlineTransformation", "derivation",
			"derivationRoot", "elvisValue", "transformationPart", "functionChainType",
			"labelIdentifier", "memberIdentifier", "mixinIdentifier", "functionDeclarationIdentifier",
			"variableIdentifier", "functionIdentifier", "invocationIdentifier", "kindIdentifier",
			"expressionModifier", "mixinModifier", "funcModifier", "trivia", "comment",
			"escaped"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null, null, null,
			"'@@'", null, null, null, null, null, "'---'", "'{'", null, null, null,
			"'func'", null, "'type'", "'do'", "'expression'", "'mixin'", "'prelude'",
			"'derivation'", "'else'", "'return'", "'goto'", "'break'", "'continue'",
			"'local'", "'carry'", "'when'", "'pure'", "'inline'", "'noinline'", "'strict'",
			null, null, null, "'->'", null, null, "'('", null, null, null, "'>'",
			null, null, null, null, "':?'", "'#'", null, null, "')'", "']'", null,
			"'!'", null, "'?:'", null, "'...'"
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
			"COMMA", "KEYWORD_FUNC", "KEYWORD_DELEGATE", "KEYWORD_TYPE", "KEYWORD_DO",
			"KEYWORD_EXPRESSION", "KEYWORD_MIXIN", "KEYWORD_PRELUDE", "KEYWORD_DERIVATION",
			"KEYWORD_ELSE", "KEYWORD_RETURN", "KEYWORD_GOTO", "KEYWORD_BREAK", "KEYWORD_CONTINUE",
			"KEYWORD_LOCAL", "KEYWORD_CARRY", "KEYWORD_WHEN", "KEYWORD_PURE", "KEYWORD_INLINE",
			"KEYWORD_NOINLINE", "KEYWORD_STRICT", "LABEL_PREFIX", "TOPLEVEL_DERIVATION",
			"ASSIGN", "ARROW", "TOPLEVEL_VALUE_EXPRESSION", "EMPTY_PARAMETERS", "BEGIN_PARAMETERS",
			"METADATA_WHITESPACE", "DERIVATION_WHITESPACE", "ARGUMENT_TEXT", "ARGUMENT_END",
			"CONTENT_WRAP", "CONTENT_LINEBREAK", "CONTENT_TEXT", "VALUE_FUNCTION",
			"VALUE_PREDICATE", "VALUE_MEMBER", "VALUE_WRAP", "VALUE_DELIMITER", "END_PARAMETERS",
			"VALUE_END_INLINE", "VALUE_SMART_ROOT", "NOT_VALUE", "VALUE_CHECK", "VALUE_ELVIS",
			"VALUE_ASSIGN", "VALUE_EXPAND", "VALUE_WHITESPACE", "ESCAPE_MACRO", "ESCAPE_LITERAL",
			"ESCAPE_HEX", "END_CONTENT_INTERPOLATE", "FUNCTION_IDENTIFIER", "MEMBER_IDENTIFIER",
			"LABEL_IDENTIFIER", "TOPLEVEL_SMART_ROOT", "TOPLEVEL_NULL", "DERIVATION_CHECK"
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
			setState(161);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(158);
				trivia();
				}
				}
				setState(163);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(165);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,1,_ctx) ) {
			case 1:
				{
				setState(164);
				fileMetadataSection();
				}
				break;
			}
			setState(176);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 15768268053413888L) != 0)) {
				{
				{
				setState(167);
				topLevelDeclaration();
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
				}
				}
				setState(178);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(179);
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
			setState(188);
			match(SECTION_DELIMITER);
			setState(192);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(189);
				trivia();
				}
				}
				setState(194);
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
		public TypeDeclarationContext typeDeclaration() {
			return getRuleContext(TypeDeclarationContext.class,0);
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
			setState(210);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(195);
				metadataList();
				setState(199);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
					{
					{
					setState(196);
					trivia();
					}
					}
					setState(201);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(205);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_MIXIN:
				case KEYWORD_DERIVATION:
					{
					setState(202);
					mixinDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(203);
					funcDeclaration();
					}
					break;
				case KEYWORD_TYPE:
					{
					setState(204);
					typeDeclaration();
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
				setState(207);
				mixinDeclaration();
				}
				break;
			case KEYWORD_FUNC:
			case KEYWORD_PURE:
			case KEYWORD_INLINE:
			case KEYWORD_NOINLINE:
				enterOuterAlt(_localctx, 3);
				{
				setState(208);
				funcDeclaration();
				}
				break;
			case KEYWORD_TYPE:
				enterOuterAlt(_localctx, 4);
				{
				setState(209);
				typeDeclaration();
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
			setState(212);
			metadata();
			setState(222);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,10,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(216);
					_errHandler.sync(this);
					_la = _input.LA(1);
					while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
						{
						{
						setState(213);
						trivia();
						}
						}
						setState(218);
						_errHandler.sync(this);
						_la = _input.LA(1);
					}
					setState(219);
					metadata();
					}
					}
				}
				setState(224);
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
			setState(231);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(225);
				match(METADATA_PREFIX);
				setState(226);
				match(IDENTIFIER);
				setState(228);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3458764513820540944L) != 0)) {
					{
					setState(227);
					valueList();
					}
				}

				}
				break;
			case BEGIN_METADATA_VALUE:
				enterOuterAlt(_localctx, 2);
				{
				setState(230);
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
			setState(233);
			match(BEGIN_METADATA_VALUE);
			setState(234);
			value();
			setState(235);
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
			setState(240);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_DERIVATION) {
				{
				{
				setState(237);
				mixinModifier();
				}
				}
				setState(242);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(243);
			match(KEYWORD_MIXIN);
			setState(244);
			mixinIdentifier();
			setState(245);
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
	public static class TypeDeclarationContext extends ParserRuleContext {
		public TerminalNode KEYWORD_TYPE() { return getToken(HixParser.KEYWORD_TYPE, 0); }
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public TerminalNode ASSIGN() { return getToken(HixParser.ASSIGN, 0); }
		public PatternExpressionContext patternExpression() {
			return getRuleContext(PatternExpressionContext.class,0);
		}
		public TypeDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_typeDeclaration; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTypeDeclaration(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TypeDeclarationContext typeDeclaration() throws RecognitionException {
		TypeDeclarationContext _localctx = new TypeDeclarationContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_typeDeclaration);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(247);
			match(KEYWORD_TYPE);
			setState(248);
			match(IDENTIFIER);
			setState(249);
			match(ASSIGN);
			setState(250);
			patternExpression();
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
		enterRule(_localctx, 16, RULE_mixinBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(252);
			match(LC);
			setState(258);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 33779781283610624L) != 0)) {
				{
				setState(256);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_EXPRESSION:
				case KEYWORD_PRELUDE:
				case KEYWORD_STRICT:
					{
					setState(253);
					expressionDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(254);
					funcDeclaration();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(255);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(260);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(261);
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
		enterRule(_localctx, 18, RULE_expressionDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(266);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_PRELUDE || _la==KEYWORD_STRICT) {
				{
				{
				setState(263);
				expressionModifier();
				}
				}
				setState(268);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(269);
			match(KEYWORD_EXPRESSION);
			setState(270);
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
		public FunctionBodyContext functionBody() {
			return getRuleContext(FunctionBodyContext.class,0);
		}
		public List<FuncModifierContext> funcModifier() {
			return getRuleContexts(FuncModifierContext.class);
		}
		public FuncModifierContext funcModifier(int i) {
			return getRuleContext(FuncModifierContext.class,i);
		}
		public DirectFunctionSignatureContext directFunctionSignature() {
			return getRuleContext(DirectFunctionSignatureContext.class,0);
		}
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
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
		enterRule(_localctx, 20, RULE_funcDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(275);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 15762598695796736L) != 0)) {
				{
				{
				setState(272);
				funcModifier();
				}
				}
				setState(277);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(278);
			match(KEYWORD_FUNC);
			setState(279);
			functionDeclarationIdentifier();
			setState(281);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3746994889972252672L) != 0)) {
				{
				setState(280);
				directFunctionSignature();
				}
			}

			setState(286);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(283);
				trivia();
				}
				}
				setState(288);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(289);
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
	public static class DirectFunctionSignatureContext extends ParserRuleContext {
		public TerminalNode ARROW() { return getToken(HixParser.ARROW, 0); }
		public PatternExpressionContext patternExpression() {
			return getRuleContext(PatternExpressionContext.class,0);
		}
		public PatternParameterListContext patternParameterList() {
			return getRuleContext(PatternParameterListContext.class,0);
		}
		public DirectFunctionSignatureContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_directFunctionSignature; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitDirectFunctionSignature(this);
			else return visitor.visitChildren(this);
		}
	}

	public final DirectFunctionSignatureContext directFunctionSignature() throws RecognitionException {
		DirectFunctionSignatureContext _localctx = new DirectFunctionSignatureContext(_ctx, getState());
		enterRule(_localctx, 22, RULE_directFunctionSignature);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(292);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(291);
				patternParameterList();
				}
			}

			setState(294);
			match(ARROW);
			setState(295);
			patternExpression();
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
		enterRule(_localctx, 24, RULE_functionBody);
		int _la;
		try {
			setState(306);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case LC:
			case KEYWORD_DO:
				enterOuterAlt(_localctx, 1);
				{
				setState(298);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_DO) {
					{
					setState(297);
					match(KEYWORD_DO);
					}
				}

				setState(300);
				statementBlock();
				}
				break;
			case FAT_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(301);
				match(FAT_ARROW);
				setState(302);
				value();
				setState(304);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_END) {
					{
					setState(303);
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
	public static class PatternExpressionContext extends ParserRuleContext {
		public MetadataListContext metadataList() {
			return getRuleContext(MetadataListContext.class,0);
		}
		public PatternPrimaryContext patternPrimary() {
			return getRuleContext(PatternPrimaryContext.class,0);
		}
		public PatternExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_patternExpression; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitPatternExpression(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PatternExpressionContext patternExpression() throws RecognitionException {
		PatternExpressionContext _localctx = new PatternExpressionContext(_ctx, getState());
		enterRule(_localctx, 26, RULE_patternExpression);
		try {
			setState(313);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(308);
				metadataList();
				setState(310);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,24,_ctx) ) {
				case 1:
					{
					setState(309);
					patternPrimary();
					}
					break;
				}
				}
				break;
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case NULL:
			case KEYWORD_DELEGATE:
				enterOuterAlt(_localctx, 2);
				{
				setState(312);
				patternPrimary();
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
	public static class PatternPrimaryContext extends ParserRuleContext {
		public PatternIdentifierContext patternIdentifier() {
			return getRuleContext(PatternIdentifierContext.class,0);
		}
		public TablePatternContext tablePattern() {
			return getRuleContext(TablePatternContext.class,0);
		}
		public TuplePatternContext tuplePattern() {
			return getRuleContext(TuplePatternContext.class,0);
		}
		public DelegatePatternContext delegatePattern() {
			return getRuleContext(DelegatePatternContext.class,0);
		}
		public PatternPrimaryContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_patternPrimary; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitPatternPrimary(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PatternPrimaryContext patternPrimary() throws RecognitionException {
		PatternPrimaryContext _localctx = new PatternPrimaryContext(_ctx, getState());
		enterRule(_localctx, 28, RULE_patternPrimary);
		try {
			setState(319);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case NULL:
				enterOuterAlt(_localctx, 1);
				{
				setState(315);
				patternIdentifier();
				}
				break;
			case BEGIN_TABLE:
				enterOuterAlt(_localctx, 2);
				{
				setState(316);
				tablePattern();
				}
				break;
			case BEGIN_TUPLE:
				enterOuterAlt(_localctx, 3);
				{
				setState(317);
				tuplePattern();
				}
				break;
			case KEYWORD_DELEGATE:
				enterOuterAlt(_localctx, 4);
				{
				setState(318);
				delegatePattern();
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
	public static class TablePatternContext extends ParserRuleContext {
		public TerminalNode BEGIN_TABLE() { return getToken(HixParser.BEGIN_TABLE, 0); }
		public TerminalNode RC() { return getToken(HixParser.RC, 0); }
		public List<PatternFieldContext> patternField() {
			return getRuleContexts(PatternFieldContext.class);
		}
		public PatternFieldContext patternField(int i) {
			return getRuleContext(PatternFieldContext.class,i);
		}
		public List<TerminalNode> VALUE_DELIMITER() { return getTokens(HixParser.VALUE_DELIMITER); }
		public TerminalNode VALUE_DELIMITER(int i) {
			return getToken(HixParser.VALUE_DELIMITER, i);
		}
		public TablePatternContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tablePattern; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTablePattern(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TablePatternContext tablePattern() throws RecognitionException {
		TablePatternContext _localctx = new TablePatternContext(_ctx, getState());
		enterRule(_localctx, 30, RULE_tablePattern);
		int _la;
		try {
			int _alt;
			setState(337);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,29,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(321);
				match(BEGIN_TABLE);
				setState(322);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(323);
				match(BEGIN_TABLE);
				setState(324);
				patternField();
				setState(329);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,27,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(325);
						match(VALUE_DELIMITER);
						setState(326);
						patternField();
						}
						}
					}
					setState(331);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,27,_ctx);
				}
				setState(333);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(332);
					match(VALUE_DELIMITER);
					}
				}

				setState(335);
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
	public static class TuplePatternContext extends ParserRuleContext {
		public TerminalNode BEGIN_TUPLE() { return getToken(HixParser.BEGIN_TUPLE, 0); }
		public TerminalNode VALUE_END_INLINE() { return getToken(HixParser.VALUE_END_INLINE, 0); }
		public List<PatternFieldContext> patternField() {
			return getRuleContexts(PatternFieldContext.class);
		}
		public PatternFieldContext patternField(int i) {
			return getRuleContext(PatternFieldContext.class,i);
		}
		public List<TerminalNode> VALUE_DELIMITER() { return getTokens(HixParser.VALUE_DELIMITER); }
		public TerminalNode VALUE_DELIMITER(int i) {
			return getToken(HixParser.VALUE_DELIMITER, i);
		}
		public TuplePatternContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tuplePattern; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitTuplePattern(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TuplePatternContext tuplePattern() throws RecognitionException {
		TuplePatternContext _localctx = new TuplePatternContext(_ctx, getState());
		enterRule(_localctx, 32, RULE_tuplePattern);
		int _la;
		try {
			int _alt;
			setState(355);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,32,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(339);
				match(BEGIN_TUPLE);
				setState(340);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(341);
				match(BEGIN_TUPLE);
				setState(342);
				patternField();
				setState(347);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,30,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(343);
						match(VALUE_DELIMITER);
						setState(344);
						patternField();
						}
						}
					}
					setState(349);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,30,_ctx);
				}
				setState(351);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(350);
					match(VALUE_DELIMITER);
					}
				}

				setState(353);
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
	public static class DelegatePatternContext extends ParserRuleContext {
		public TerminalNode KEYWORD_DELEGATE() { return getToken(HixParser.KEYWORD_DELEGATE, 0); }
		public PatternParameterListContext patternParameterList() {
			return getRuleContext(PatternParameterListContext.class,0);
		}
		public TerminalNode ARROW() { return getToken(HixParser.ARROW, 0); }
		public PatternExpressionContext patternExpression() {
			return getRuleContext(PatternExpressionContext.class,0);
		}
		public DelegatePatternContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_delegatePattern; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitDelegatePattern(this);
			else return visitor.visitChildren(this);
		}
	}

	public final DelegatePatternContext delegatePattern() throws RecognitionException {
		DelegatePatternContext _localctx = new DelegatePatternContext(_ctx, getState());
		enterRule(_localctx, 34, RULE_delegatePattern);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(357);
			match(KEYWORD_DELEGATE);
			setState(358);
			patternParameterList();
			setState(359);
			match(ARROW);
			setState(360);
			patternExpression();
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
	public static class PatternParameterListContext extends ParserRuleContext {
		public TerminalNode BEGIN_PARAMETERS() { return getToken(HixParser.BEGIN_PARAMETERS, 0); }
		public List<PatternFieldContext> patternField() {
			return getRuleContexts(PatternFieldContext.class);
		}
		public PatternFieldContext patternField(int i) {
			return getRuleContext(PatternFieldContext.class,i);
		}
		public TerminalNode END_PARAMETERS() { return getToken(HixParser.END_PARAMETERS, 0); }
		public List<TerminalNode> VALUE_DELIMITER() { return getTokens(HixParser.VALUE_DELIMITER); }
		public TerminalNode VALUE_DELIMITER(int i) {
			return getToken(HixParser.VALUE_DELIMITER, i);
		}
		public TerminalNode EMPTY_PARAMETERS() { return getToken(HixParser.EMPTY_PARAMETERS, 0); }
		public PatternParameterListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_patternParameterList; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitPatternParameterList(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PatternParameterListContext patternParameterList() throws RecognitionException {
		PatternParameterListContext _localctx = new PatternParameterListContext(_ctx, getState());
		enterRule(_localctx, 36, RULE_patternParameterList);
		int _la;
		try {
			int _alt;
			setState(379);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,35,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(362);
				match(BEGIN_PARAMETERS);
				setState(363);
				patternField();
				setState(368);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,33,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(364);
						match(VALUE_DELIMITER);
						setState(365);
						patternField();
						}
						}
					}
					setState(370);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,33,_ctx);
				}
				setState(372);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(371);
					match(VALUE_DELIMITER);
					}
				}

				setState(374);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(376);
				match(BEGIN_PARAMETERS);
				setState(377);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(378);
				match(EMPTY_PARAMETERS);
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
	public static class PatternFieldContext extends ParserRuleContext {
		public PatternPrimaryContext patternPrimary() {
			return getRuleContext(PatternPrimaryContext.class,0);
		}
		public MetadataListContext metadataList() {
			return getRuleContext(MetadataListContext.class,0);
		}
		public TerminalNode ROOT_IDENTIFIER() { return getToken(HixParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode VALUE_ASSIGN() { return getToken(HixParser.VALUE_ASSIGN, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public PatternFieldContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_patternField; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitPatternField(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PatternFieldContext patternField() throws RecognitionException {
		PatternFieldContext _localctx = new PatternFieldContext(_ctx, getState());
		enterRule(_localctx, 38, RULE_patternField);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(382);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				setState(381);
				metadataList();
				}
			}

			setState(384);
			patternPrimary();
			setState(386);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ROOT_IDENTIFIER) {
				{
				setState(385);
				match(ROOT_IDENTIFIER);
				}
			}

			setState(390);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==VALUE_ASSIGN) {
				{
				setState(388);
				match(VALUE_ASSIGN);
				setState(389);
				value();
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
	public static class PatternIdentifierContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public TerminalNode ROOT_IDENTIFIER() { return getToken(HixParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode NULL() { return getToken(HixParser.NULL, 0); }
		public PatternIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_patternIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitPatternIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PatternIdentifierContext patternIdentifier() throws RecognitionException {
		PatternIdentifierContext _localctx = new PatternIdentifierContext(_ctx, getState());
		enterRule(_localctx, 40, RULE_patternIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(392);
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
		enterRule(_localctx, 42, RULE_statementBlock);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(394);
			match(LC);
			setState(400);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 110320610361476096L) != 0) || _la==VALUE_SMART_ROOT) {
				{
				setState(398);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case IDENTIFIER:
				case LC:
				case KEYWORD_RETURN:
				case KEYWORD_GOTO:
				case KEYWORD_BREAK:
				case KEYWORD_CONTINUE:
				case KEYWORD_LOCAL:
				case KEYWORD_CARRY:
				case KEYWORD_WHEN:
				case LABEL_PREFIX:
				case TOPLEVEL_DERIVATION:
				case VALUE_SMART_ROOT:
					{
					setState(395);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(396);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(397);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(402);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(403);
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
		public LocalDeclarationStatementContext localDeclarationStatement() {
			return getRuleContext(LocalDeclarationStatementContext.class,0);
		}
		public LocalAssignmentStatementContext localAssignmentStatement() {
			return getRuleContext(LocalAssignmentStatementContext.class,0);
		}
		public ToplevelDerivationStatementContext toplevelDerivationStatement() {
			return getRuleContext(ToplevelDerivationStatementContext.class,0);
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
		enterRule(_localctx, 44, RULE_statement);
		int _la;
		try {
			setState(420);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,42,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(405);
				labelIdentifier();
				setState(406);
				match(NEWLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(409);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LABEL_PREFIX) {
					{
					setState(408);
					labelIdentifier();
					}
				}

				setState(411);
				statementBlock();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(412);
				invocationStatement();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(413);
				localDeclarationStatement();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(414);
				localAssignmentStatement();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(415);
				toplevelDerivationStatement();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(416);
				controlflowStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(417);
				whenValueStatement();
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(418);
				whenConditionStatement();
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(419);
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
		public InvocationIdentifierContext invocationIdentifier() {
			return getRuleContext(InvocationIdentifierContext.class,0);
		}
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
		enterRule(_localctx, 46, RULE_invocationStatement);
		try {
			setState(430);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,44,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(422);
				invocationIdentifier();
				setState(423);
				valueList();
				setState(425);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,43,_ctx) ) {
				case 1:
					{
					setState(424);
					tailValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(427);
				invocationIdentifier();
				setState(428);
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
		enterRule(_localctx, 48, RULE_whenConditionStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(432);
			match(KEYWORD_WHEN);
			setState(433);
			whenChainCondition();
			setState(434);
			statementBlock();
			setState(445);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,46,_ctx) ) {
			case 1:
				{
				setState(438);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
					{
					{
					setState(435);
					trivia();
					}
					}
					setState(440);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(441);
				match(KEYWORD_ELSE);
				setState(442);
				whenResult();
				setState(443);
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
		enterRule(_localctx, 50, RULE_whenElseBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(447);
			match(KEYWORD_ELSE);
			setState(448);
			match(ARROW);
			setState(449);
			whenResult();
			setState(450);
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
		enterRule(_localctx, 52, RULE_whenResult);
		try {
			setState(454);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,47,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(452);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(453);
				statement();
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
		enterRule(_localctx, 54, RULE_whenChainCondition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(456);
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
		enterRule(_localctx, 56, RULE_whenChainStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(458);
			match(KEYWORD_WHEN);
			setState(459);
			match(LC);
			setState(460);
			match(NEWLINE);
			setState(461);
			whenChainBody();
			setState(462);
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
		enterRule(_localctx, 58, RULE_whenChainBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(464);
			whenChainBranch();
			setState(469);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3458764514760065040L) != 0)) {
				{
				setState(467);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case EMPTY_PARAMETERS:
				case BEGIN_PARAMETERS:
					{
					setState(465);
					whenChainBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(466);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(471);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(473);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(472);
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
		enterRule(_localctx, 60, RULE_whenChainBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(475);
			whenChainCondition();
			setState(476);
			match(ARROW);
			setState(477);
			whenResult();
			setState(478);
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
		enterRule(_localctx, 62, RULE_whenValueStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(480);
			match(KEYWORD_WHEN);
			setState(482);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 576460752318358032L) != 0) || ((((_la - 76)) & ~0x3f) == 0 && ((1L << (_la - 76)) & 2051L) != 0)) {
				{
				setState(481);
				value();
				}
			}

			setState(484);
			match(LC);
			setState(485);
			match(NEWLINE);
			setState(486);
			whenValueBody();
			setState(487);
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
		enterRule(_localctx, 64, RULE_whenValueBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(492);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(489);
				trivia();
				}
				}
				setState(494);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(495);
			whenValueBranch();
			setState(500);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 576460753257882128L) != 0) || ((((_la - 76)) & ~0x3f) == 0 && ((1L << (_la - 76)) & 2051L) != 0)) {
				{
				setState(498);
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
					setState(496);
					whenValueBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(497);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(502);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(504);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(503);
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
		enterRule(_localctx, 66, RULE_whenValueBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(506);
			whenValueCondition();
			setState(507);
			match(ARROW);
			setState(508);
			whenResult();
			setState(509);
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
		enterRule(_localctx, 68, RULE_whenValueCondition);
		try {
			setState(513);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,56,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(511);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(512);
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
	public static class LocalDeclarationStatementContext extends ParserRuleContext {
		public TerminalNode KEYWORD_LOCAL() { return getToken(HixParser.KEYWORD_LOCAL, 0); }
		public VariableIdentifierContext variableIdentifier() {
			return getRuleContext(VariableIdentifierContext.class,0);
		}
		public TerminalNode KEYWORD_CARRY() { return getToken(HixParser.KEYWORD_CARRY, 0); }
		public AssignedValueContext assignedValue() {
			return getRuleContext(AssignedValueContext.class,0);
		}
		public PatternExpressionContext patternExpression() {
			return getRuleContext(PatternExpressionContext.class,0);
		}
		public LocalDeclarationStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_localDeclarationStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitLocalDeclarationStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final LocalDeclarationStatementContext localDeclarationStatement() throws RecognitionException {
		LocalDeclarationStatementContext _localctx = new LocalDeclarationStatementContext(_ctx, getState());
		enterRule(_localctx, 70, RULE_localDeclarationStatement);
		int _la;
		try {
			setState(532);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,61,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(516);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(515);
					match(KEYWORD_CARRY);
					}
				}

				setState(518);
				match(KEYWORD_LOCAL);
				setState(519);
				variableIdentifier();
				setState(521);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,58,_ctx) ) {
				case 1:
					{
					setState(520);
					assignedValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(524);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(523);
					match(KEYWORD_CARRY);
					}
				}

				setState(526);
				match(KEYWORD_LOCAL);
				setState(527);
				patternExpression();
				setState(528);
				variableIdentifier();
				setState(530);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,60,_ctx) ) {
				case 1:
					{
					setState(529);
					assignedValue();
					}
					break;
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
	public static class LocalAssignmentStatementContext extends ParserRuleContext {
		public TerminalNode VALUE_SMART_ROOT() { return getToken(HixParser.VALUE_SMART_ROOT, 0); }
		public VariableIdentifierContext variableIdentifier() {
			return getRuleContext(VariableIdentifierContext.class,0);
		}
		public AssignedValueContext assignedValue() {
			return getRuleContext(AssignedValueContext.class,0);
		}
		public LocalAssignmentStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_localAssignmentStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitLocalAssignmentStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final LocalAssignmentStatementContext localAssignmentStatement() throws RecognitionException {
		LocalAssignmentStatementContext _localctx = new LocalAssignmentStatementContext(_ctx, getState());
		enterRule(_localctx, 72, RULE_localAssignmentStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(534);
			match(VALUE_SMART_ROOT);
			setState(535);
			variableIdentifier();
			setState(536);
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
	public static class ToplevelDerivationStatementContext extends ParserRuleContext {
		public TerminalNode TOPLEVEL_DERIVATION() { return getToken(HixParser.TOPLEVEL_DERIVATION, 0); }
		public FunctionIdentifierContext functionIdentifier() {
			return getRuleContext(FunctionIdentifierContext.class,0);
		}
		public ValueListContext valueList() {
			return getRuleContext(ValueListContext.class,0);
		}
		public List<TransformationPartContext> transformationPart() {
			return getRuleContexts(TransformationPartContext.class);
		}
		public TransformationPartContext transformationPart(int i) {
			return getRuleContext(TransformationPartContext.class,i);
		}
		public ToplevelDerivationStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_toplevelDerivationStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitToplevelDerivationStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ToplevelDerivationStatementContext toplevelDerivationStatement() throws RecognitionException {
		ToplevelDerivationStatementContext _localctx = new ToplevelDerivationStatementContext(_ctx, getState());
		enterRule(_localctx, 74, RULE_toplevelDerivationStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(538);
			match(TOPLEVEL_DERIVATION);
			setState(539);
			functionIdentifier();
			setState(541);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3458764513820540944L) != 0)) {
				{
				setState(540);
				valueList();
				}
			}

			setState(546);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & 15L) != 0)) {
				{
				{
				setState(543);
				transformationPart();
				}
				}
				setState(548);
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
		enterRule(_localctx, 76, RULE_assignedValue);
		try {
			setState(557);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ASSIGN:
				enterOuterAlt(_localctx, 1);
				{
				setState(549);
				match(ASSIGN);
				setState(554);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,64,_ctx) ) {
				case 1:
					{
					setState(550);
					value();
					}
					break;
				case 2:
					{
					setState(551);
					whenValueStatement();
					}
					break;
				case 3:
					{
					setState(552);
					whenChainStatement();
					}
					break;
				case 4:
					{
					setState(553);
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
				setState(556);
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
		enterRule(_localctx, 78, RULE_controlflowStatement);
		int _la;
		try {
			setState(567);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_RETURN:
				enterOuterAlt(_localctx, 1);
				{
				setState(559);
				match(KEYWORD_RETURN);
				setState(561);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3458764513820540944L) != 0)) {
					{
					setState(560);
					valueList();
					}
				}

				}
				break;
			case KEYWORD_GOTO:
				enterOuterAlt(_localctx, 2);
				{
				setState(563);
				match(KEYWORD_GOTO);
				setState(564);
				match(IDENTIFIER);
				}
				break;
			case KEYWORD_CONTINUE:
				enterOuterAlt(_localctx, 3);
				{
				setState(565);
				match(KEYWORD_CONTINUE);
				}
				break;
			case KEYWORD_BREAK:
				enterOuterAlt(_localctx, 4);
				{
				setState(566);
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
		enterRule(_localctx, 80, RULE_contentBlock);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(569);
			match(BEGIN_CONTENT);
			setState(570);
			contentBody();
			setState(571);
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
		enterRule(_localctx, 82, RULE_contentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(577);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				setState(577);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case CONTENT_WRAP:
					{
					setState(573);
					match(CONTENT_WRAP);
					}
					break;
				case CONTENT_LINEBREAK:
					{
					setState(574);
					match(CONTENT_LINEBREAK);
					}
					break;
				case CONTENT_TEXT:
					{
					setState(575);
					match(CONTENT_TEXT);
					}
					break;
				case BEGIN_VALUE_INTERPOLATE:
					{
					setState(576);
					contentInterpolate();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(579);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 8)) & ~0x3f) == 0 && ((1L << (_la - 8)) & 2017612633061982209L) != 0) );
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
		enterRule(_localctx, 84, RULE_contentInterpolate);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(581);
			match(BEGIN_VALUE_INTERPOLATE);
			setState(582);
			derivation();
			setState(583);
			match(RC);
			setState(585);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==END_CONTENT_INTERPOLATE) {
				{
				setState(584);
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
		enterRule(_localctx, 86, RULE_value);
		try {
			setState(589);
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
				setState(587);
				nonArgumentValue(0);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(588);
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
		enterRule(_localctx, 88, RULE_valueExpression);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(591);
			match(TOPLEVEL_VALUE_EXPRESSION);
			setState(592);
			value();
			setState(593);
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
		int _startState = 90;
		enterRecursionRule(_localctx, 90, RULE_nonArgumentValue, _p);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(603);
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
				setState(596);
				primaryValue();
				setState(598);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,72,_ctx) ) {
				case 1:
					{
					setState(597);
					elvisValue();
					}
					break;
				}
				}
				break;
			case NOT_VALUE:
				{
				setState(600);
				prefixOperators();
				setState(601);
				nonArgumentValue(2);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(609);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,74,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					{
					_localctx = new NonArgumentValueContext(_parentctx, _parentState);
					pushNewRecursionContext(_localctx, _startState, RULE_nonArgumentValue);
					setState(605);
					if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
					setState(606);
					postfixOperators();
					}
					}
				}
				setState(611);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,74,_ctx);
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
		enterRule(_localctx, 92, RULE_primaryValue);
		try {
			setState(622);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,75,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(612);
				inlineValue();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(613);
				lambdaValue();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(614);
				derivation();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(615);
				tableValue();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(616);
				tupleValue();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(617);
				valueExpression();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(618);
				valueStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(619);
				match(NUMBER);
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(620);
				match(BOOLEAN);
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(621);
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
		enterRule(_localctx, 94, RULE_lambdaValue);
		int _la;
		try {
			setState(639);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_LAMBDA_BLOCK:
				enterOuterAlt(_localctx, 1);
				{
				setState(624);
				match(BEGIN_LAMBDA_BLOCK);
				setState(630);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 110320610361476096L) != 0) || _la==VALUE_SMART_ROOT) {
					{
					setState(628);
					_errHandler.sync(this);
					switch (_input.LA(1)) {
					case IDENTIFIER:
					case LC:
					case KEYWORD_RETURN:
					case KEYWORD_GOTO:
					case KEYWORD_BREAK:
					case KEYWORD_CONTINUE:
					case KEYWORD_LOCAL:
					case KEYWORD_CARRY:
					case KEYWORD_WHEN:
					case LABEL_PREFIX:
					case TOPLEVEL_DERIVATION:
					case VALUE_SMART_ROOT:
						{
						setState(625);
						statement();
						}
						break;
					case SEMICOLON:
						{
						setState(626);
						match(SEMICOLON);
						}
						break;
					case NEWLINE:
					case COMMENT:
					case SLASH_COMMENT:
						{
						setState(627);
						trivia();
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					}
					setState(632);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(633);
				match(RC);
				}
				break;
			case BEGIN_LAMBDA_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(634);
				match(BEGIN_LAMBDA_ARROW);
				setState(635);
				value();
				setState(637);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,78,_ctx) ) {
				case 1:
					{
					setState(636);
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
		enterRule(_localctx, 96, RULE_prefixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(641);
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
		enterRule(_localctx, 98, RULE_postfixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(643);
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
		enterRule(_localctx, 100, RULE_valueStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(645);
			functionIdentifier();
			setState(646);
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
		enterRule(_localctx, 102, RULE_tailValue);
		int _la;
		try {
			setState(653);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_CONTENT:
			case NEWLINE:
				enterOuterAlt(_localctx, 1);
				{
				setState(649);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==NEWLINE) {
					{
					setState(648);
					match(NEWLINE);
					}
				}

				setState(651);
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
				setState(652);
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
		enterRule(_localctx, 104, RULE_tupleValue);
		int _la;
		try {
			int _alt;
			setState(671);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,84,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(655);
				match(BEGIN_TUPLE);
				setState(656);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(657);
				match(BEGIN_TUPLE);
				setState(658);
				value();
				setState(663);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,82,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(659);
						match(VALUE_DELIMITER);
						setState(660);
						value();
						}
						}
					}
					setState(665);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,82,_ctx);
				}
				setState(667);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(666);
					match(VALUE_DELIMITER);
					}
				}

				setState(669);
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
		enterRule(_localctx, 106, RULE_tableValue);
		int _la;
		try {
			int _alt;
			setState(689);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,87,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(673);
				match(BEGIN_TABLE);
				setState(674);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(675);
				match(BEGIN_TABLE);
				setState(676);
				tableKeyedEntry();
				setState(681);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,85,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(677);
						match(VALUE_DELIMITER);
						setState(678);
						tableKeyedEntry();
						}
						}
					}
					setState(683);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,85,_ctx);
				}
				setState(685);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(684);
					match(VALUE_DELIMITER);
					}
				}

				setState(687);
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
		enterRule(_localctx, 108, RULE_tableKeyedEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(694);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				{
				setState(691);
				metadata();
				}
				}
				setState(696);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(697);
			match(ROOT_IDENTIFIER);
			setState(698);
			match(VALUE_ASSIGN);
			setState(699);
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
		enterRule(_localctx, 110, RULE_valueList);
		int _la;
		try {
			int _alt;
			setState(723);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,92,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(701);
				match(BEGIN_PARAMETERS);
				setState(702);
				value();
				setState(707);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,89,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(703);
						match(VALUE_DELIMITER);
						setState(704);
						value();
						}
						}
					}
					setState(709);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,89,_ctx);
				}
				setState(711);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(710);
					match(VALUE_DELIMITER);
					}
				}

				setState(713);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(715);
				match(BEGIN_PARAMETERS);
				setState(716);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(717);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(719);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(718);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(721);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,91,_ctx);
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
		enterRule(_localctx, 112, RULE_argumentValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(725);
			match(BEGIN_ARGUMENT);
			setState(726);
			argumentBody();
			setState(727);
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
		enterRule(_localctx, 114, RULE_argumentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(734);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 3)) & ~0x3f) == 0 && ((1L << (_la - 3)) & 2305843009213694017L) != 0)) {
				{
				setState(732);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case ARGUMENT_TEXT:
					{
					setState(729);
					match(ARGUMENT_TEXT);
					}
					break;
				case ESCAPE:
					{
					setState(730);
					escaped();
					}
					break;
				case BEGIN_VALUE_INLINE:
					{
					setState(731);
					inlineValue();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(736);
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
		enterRule(_localctx, 116, RULE_inlineValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(737);
			match(BEGIN_VALUE_INLINE);
			setState(738);
			value();
			setState(739);
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
		enterRule(_localctx, 118, RULE_inlineTransformation);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(741);
			match(BEGIN_VALUE_INLINE);
			setState(743);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(742);
				transformationPart();
				}
				}
				setState(745);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & 15L) != 0) );
			setState(747);
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
		enterRule(_localctx, 120, RULE_derivation);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(749);
			derivationRoot();
			setState(753);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,96,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(750);
					transformationPart();
					}
					}
				}
				setState(755);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,96,_ctx);
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
		enterRule(_localctx, 122, RULE_derivationRoot);
		int _la;
		try {
			setState(759);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ROOT_IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(756);
				match(ROOT_IDENTIFIER);
				}
				break;
			case VALUE_SMART_ROOT:
				enterOuterAlt(_localctx, 2);
				{
				setState(757);
				match(VALUE_SMART_ROOT);
				setState(758);
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
		enterRule(_localctx, 124, RULE_elvisValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(761);
			match(VALUE_ELVIS);
			setState(762);
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
		enterRule(_localctx, 126, RULE_transformationPart);
		try {
			setState(771);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case VALUE_FUNCTION:
			case VALUE_PREDICATE:
				enterOuterAlt(_localctx, 1);
				{
				setState(764);
				functionChainType();
				setState(765);
				functionIdentifier();
				setState(767);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,98,_ctx) ) {
				case 1:
					{
					setState(766);
					valueList();
					}
					break;
				}
				}
				break;
			case VALUE_MEMBER:
				enterOuterAlt(_localctx, 2);
				{
				setState(769);
				memberIdentifier();
				}
				break;
			case VALUE_WRAP:
				enterOuterAlt(_localctx, 3);
				{
				setState(770);
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
		enterRule(_localctx, 128, RULE_functionChainType);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(773);
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
		enterRule(_localctx, 130, RULE_labelIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(775);
			match(LABEL_PREFIX);
			setState(776);
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
		enterRule(_localctx, 132, RULE_memberIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(778);
			match(VALUE_MEMBER);
			setState(779);
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
		enterRule(_localctx, 134, RULE_mixinIdentifier);
		try {
			setState(784);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(781);
				match(IDENTIFIER);
				}
				break;
			case NAMESPACE_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(782);
				match(NAMESPACE_IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 3);
				{
				setState(783);
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
		enterRule(_localctx, 136, RULE_functionDeclarationIdentifier);
		try {
			setState(788);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(786);
				match(IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(787);
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
		enterRule(_localctx, 138, RULE_variableIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(790);
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
		enterRule(_localctx, 140, RULE_functionIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(792);
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
	public static class InvocationIdentifierContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public TerminalNode KEYWORD_LOCAL() { return getToken(HixParser.KEYWORD_LOCAL, 0); }
		public InvocationIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_invocationIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitInvocationIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final InvocationIdentifierContext invocationIdentifier() throws RecognitionException {
		InvocationIdentifierContext _localctx = new InvocationIdentifierContext(_ctx, getState());
		enterRule(_localctx, 142, RULE_invocationIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(794);
			_la = _input.LA(1);
			if ( !(_la==IDENTIFIER || _la==KEYWORD_LOCAL) ) {
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
		enterRule(_localctx, 144, RULE_kindIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(796);
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
		enterRule(_localctx, 146, RULE_expressionModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(798);
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
		enterRule(_localctx, 148, RULE_mixinModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(800);
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
		enterRule(_localctx, 150, RULE_funcModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(802);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 15762598695796736L) != 0)) ) {
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
		enterRule(_localctx, 152, RULE_trivia);
		try {
			setState(806);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case COMMENT:
			case SLASH_COMMENT:
				enterOuterAlt(_localctx, 1);
				{
				setState(804);
				comment();
				}
				break;
			case NEWLINE:
				enterOuterAlt(_localctx, 2);
				{
				setState(805);
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
		enterRule(_localctx, 154, RULE_comment);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(808);
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
		enterRule(_localctx, 156, RULE_escaped);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(810);
			match(ESCAPE);
			setState(811);
			_la = _input.LA(1);
			if ( !(((((_la - 83)) & ~0x3f) == 0 && ((1L << (_la - 83)) & 7L) != 0)) ) {
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
		case 45:
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
		"\u0004\u0001\\\u032e\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
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
		"F\u0007F\u0002G\u0007G\u0002H\u0007H\u0002I\u0007I\u0002J\u0007J\u0002"+
		"K\u0007K\u0002L\u0007L\u0002M\u0007M\u0002N\u0007N\u0001\u0000\u0005\u0000"+
		"\u00a0\b\u0000\n\u0000\f\u0000\u00a3\t\u0000\u0001\u0000\u0003\u0000\u00a6"+
		"\b\u0000\u0001\u0000\u0001\u0000\u0005\u0000\u00aa\b\u0000\n\u0000\f\u0000"+
		"\u00ad\t\u0000\u0005\u0000\u00af\b\u0000\n\u0000\f\u0000\u00b2\t\u0000"+
		"\u0001\u0000\u0001\u0000\u0001\u0001\u0001\u0001\u0005\u0001\u00b8\b\u0001"+
		"\n\u0001\f\u0001\u00bb\t\u0001\u0001\u0001\u0001\u0001\u0005\u0001\u00bf"+
		"\b\u0001\n\u0001\f\u0001\u00c2\t\u0001\u0001\u0002\u0001\u0002\u0005\u0002"+
		"\u00c6\b\u0002\n\u0002\f\u0002\u00c9\t\u0002\u0001\u0002\u0001\u0002\u0001"+
		"\u0002\u0003\u0002\u00ce\b\u0002\u0001\u0002\u0001\u0002\u0001\u0002\u0003"+
		"\u0002\u00d3\b\u0002\u0001\u0003\u0001\u0003\u0005\u0003\u00d7\b\u0003"+
		"\n\u0003\f\u0003\u00da\t\u0003\u0001\u0003\u0005\u0003\u00dd\b\u0003\n"+
		"\u0003\f\u0003\u00e0\t\u0003\u0001\u0004\u0001\u0004\u0001\u0004\u0003"+
		"\u0004\u00e5\b\u0004\u0001\u0004\u0003\u0004\u00e8\b\u0004\u0001\u0005"+
		"\u0001\u0005\u0001\u0005\u0001\u0005\u0001\u0006\u0005\u0006\u00ef\b\u0006"+
		"\n\u0006\f\u0006\u00f2\t\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001"+
		"\u0006\u0001\u0007\u0001\u0007\u0001\u0007\u0001\u0007\u0001\u0007\u0001"+
		"\b\u0001\b\u0001\b\u0001\b\u0005\b\u0101\b\b\n\b\f\b\u0104\t\b\u0001\b"+
		"\u0001\b\u0001\t\u0005\t\u0109\b\t\n\t\f\t\u010c\t\t\u0001\t\u0001\t\u0001"+
		"\t\u0001\n\u0005\n\u0112\b\n\n\n\f\n\u0115\t\n\u0001\n\u0001\n\u0001\n"+
		"\u0003\n\u011a\b\n\u0001\n\u0005\n\u011d\b\n\n\n\f\n\u0120\t\n\u0001\n"+
		"\u0001\n\u0001\u000b\u0003\u000b\u0125\b\u000b\u0001\u000b\u0001\u000b"+
		"\u0001\u000b\u0001\f\u0003\f\u012b\b\f\u0001\f\u0001\f\u0001\f\u0001\f"+
		"\u0003\f\u0131\b\f\u0003\f\u0133\b\f\u0001\r\u0001\r\u0003\r\u0137\b\r"+
		"\u0001\r\u0003\r\u013a\b\r\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e"+
		"\u0003\u000e\u0140\b\u000e\u0001\u000f\u0001\u000f\u0001\u000f\u0001\u000f"+
		"\u0001\u000f\u0001\u000f\u0005\u000f\u0148\b\u000f\n\u000f\f\u000f\u014b"+
		"\t\u000f\u0001\u000f\u0003\u000f\u014e\b\u000f\u0001\u000f\u0001\u000f"+
		"\u0003\u000f\u0152\b\u000f\u0001\u0010\u0001\u0010\u0001\u0010\u0001\u0010"+
		"\u0001\u0010\u0001\u0010\u0005\u0010\u015a\b\u0010\n\u0010\f\u0010\u015d"+
		"\t\u0010\u0001\u0010\u0003\u0010\u0160\b\u0010\u0001\u0010\u0001\u0010"+
		"\u0003\u0010\u0164\b\u0010\u0001\u0011\u0001\u0011\u0001\u0011\u0001\u0011"+
		"\u0001\u0011\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0005\u0012"+
		"\u016f\b\u0012\n\u0012\f\u0012\u0172\t\u0012\u0001\u0012\u0003\u0012\u0175"+
		"\b\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0003"+
		"\u0012\u017c\b\u0012\u0001\u0013\u0003\u0013\u017f\b\u0013\u0001\u0013"+
		"\u0001\u0013\u0003\u0013\u0183\b\u0013\u0001\u0013\u0001\u0013\u0003\u0013"+
		"\u0187\b\u0013\u0001\u0014\u0001\u0014\u0001\u0015\u0001\u0015\u0001\u0015"+
		"\u0001\u0015\u0005\u0015\u018f\b\u0015\n\u0015\f\u0015\u0192\t\u0015\u0001"+
		"\u0015\u0001\u0015\u0001\u0016\u0001\u0016\u0001\u0016\u0001\u0016\u0003"+
		"\u0016\u019a\b\u0016\u0001\u0016\u0001\u0016\u0001\u0016\u0001\u0016\u0001"+
		"\u0016\u0001\u0016\u0001\u0016\u0001\u0016\u0001\u0016\u0003\u0016\u01a5"+
		"\b\u0016\u0001\u0017\u0001\u0017\u0001\u0017\u0003\u0017\u01aa\b\u0017"+
		"\u0001\u0017\u0001\u0017\u0001\u0017\u0003\u0017\u01af\b\u0017\u0001\u0018"+
		"\u0001\u0018\u0001\u0018\u0001\u0018\u0005\u0018\u01b5\b\u0018\n\u0018"+
		"\f\u0018\u01b8\t\u0018\u0001\u0018\u0001\u0018\u0001\u0018\u0001\u0018"+
		"\u0003\u0018\u01be\b\u0018\u0001\u0019\u0001\u0019\u0001\u0019\u0001\u0019"+
		"\u0001\u0019\u0001\u001a\u0001\u001a\u0003\u001a\u01c7\b\u001a\u0001\u001b"+
		"\u0001\u001b\u0001\u001c\u0001\u001c\u0001\u001c\u0001\u001c\u0001\u001c"+
		"\u0001\u001c\u0001\u001d\u0001\u001d\u0001\u001d\u0005\u001d\u01d4\b\u001d"+
		"\n\u001d\f\u001d\u01d7\t\u001d\u0001\u001d\u0003\u001d\u01da\b\u001d\u0001"+
		"\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001f\u0001"+
		"\u001f\u0003\u001f\u01e3\b\u001f\u0001\u001f\u0001\u001f\u0001\u001f\u0001"+
		"\u001f\u0001\u001f\u0001 \u0005 \u01eb\b \n \f \u01ee\t \u0001 \u0001"+
		" \u0001 \u0005 \u01f3\b \n \f \u01f6\t \u0001 \u0003 \u01f9\b \u0001!"+
		"\u0001!\u0001!\u0001!\u0001!\u0001\"\u0001\"\u0003\"\u0202\b\"\u0001#"+
		"\u0003#\u0205\b#\u0001#\u0001#\u0001#\u0003#\u020a\b#\u0001#\u0003#\u020d"+
		"\b#\u0001#\u0001#\u0001#\u0001#\u0003#\u0213\b#\u0003#\u0215\b#\u0001"+
		"$\u0001$\u0001$\u0001$\u0001%\u0001%\u0001%\u0003%\u021e\b%\u0001%\u0005"+
		"%\u0221\b%\n%\f%\u0224\t%\u0001&\u0001&\u0001&\u0001&\u0001&\u0003&\u022b"+
		"\b&\u0001&\u0003&\u022e\b&\u0001\'\u0001\'\u0003\'\u0232\b\'\u0001\'\u0001"+
		"\'\u0001\'\u0001\'\u0003\'\u0238\b\'\u0001(\u0001(\u0001(\u0001(\u0001"+
		")\u0001)\u0001)\u0001)\u0004)\u0242\b)\u000b)\f)\u0243\u0001*\u0001*\u0001"+
		"*\u0001*\u0003*\u024a\b*\u0001+\u0001+\u0003+\u024e\b+\u0001,\u0001,\u0001"+
		",\u0001,\u0001-\u0001-\u0001-\u0003-\u0257\b-\u0001-\u0001-\u0001-\u0003"+
		"-\u025c\b-\u0001-\u0001-\u0005-\u0260\b-\n-\f-\u0263\t-\u0001.\u0001."+
		"\u0001.\u0001.\u0001.\u0001.\u0001.\u0001.\u0001.\u0001.\u0003.\u026f"+
		"\b.\u0001/\u0001/\u0001/\u0001/\u0005/\u0275\b/\n/\f/\u0278\t/\u0001/"+
		"\u0001/\u0001/\u0001/\u0003/\u027e\b/\u0003/\u0280\b/\u00010\u00010\u0001"+
		"1\u00011\u00012\u00012\u00012\u00013\u00033\u028a\b3\u00013\u00013\u0003"+
		"3\u028e\b3\u00014\u00014\u00014\u00014\u00014\u00014\u00054\u0296\b4\n"+
		"4\f4\u0299\t4\u00014\u00034\u029c\b4\u00014\u00014\u00034\u02a0\b4\u0001"+
		"5\u00015\u00015\u00015\u00015\u00015\u00055\u02a8\b5\n5\f5\u02ab\t5\u0001"+
		"5\u00035\u02ae\b5\u00015\u00015\u00035\u02b2\b5\u00016\u00056\u02b5\b"+
		"6\n6\f6\u02b8\t6\u00016\u00016\u00016\u00016\u00017\u00017\u00017\u0001"+
		"7\u00057\u02c2\b7\n7\f7\u02c5\t7\u00017\u00037\u02c8\b7\u00017\u00017"+
		"\u00017\u00017\u00017\u00017\u00047\u02d0\b7\u000b7\f7\u02d1\u00037\u02d4"+
		"\b7\u00018\u00018\u00018\u00018\u00019\u00019\u00019\u00059\u02dd\b9\n"+
		"9\f9\u02e0\t9\u0001:\u0001:\u0001:\u0001:\u0001;\u0001;\u0004;\u02e8\b"+
		";\u000b;\f;\u02e9\u0001;\u0001;\u0001<\u0001<\u0005<\u02f0\b<\n<\f<\u02f3"+
		"\t<\u0001=\u0001=\u0001=\u0003=\u02f8\b=\u0001>\u0001>\u0001>\u0001?\u0001"+
		"?\u0001?\u0003?\u0300\b?\u0001?\u0001?\u0003?\u0304\b?\u0001@\u0001@\u0001"+
		"A\u0001A\u0001A\u0001B\u0001B\u0001B\u0001C\u0001C\u0001C\u0003C\u0311"+
		"\bC\u0001D\u0001D\u0003D\u0315\bD\u0001E\u0001E\u0001F\u0001F\u0001G\u0001"+
		"G\u0001H\u0001H\u0001I\u0001I\u0001J\u0001J\u0001K\u0001K\u0001L\u0001"+
		"L\u0003L\u0327\bL\u0001M\u0001M\u0001N\u0001N\u0001N\u0001N\u0000\u0001"+
		"ZO\u0000\u0002\u0004\u0006\b\n\f\u000e\u0010\u0012\u0014\u0016\u0018\u001a"+
		"\u001c\u001e \"$&(*,.02468:<>@BDFHJLNPRTVXZ\\^`bdfhjlnprtvxz|~\u0080\u0082"+
		"\u0084\u0086\u0088\u008a\u008c\u008e\u0090\u0092\u0094\u0096\u0098\u009a"+
		"\u009c\u0000\t\u0003\u0000\n\n\r\r\u0017\u0017\u0002\u0000\r\r\u0015\u0015"+
		"\u0001\u0000EF\u0002\u0000\r\rWW\u0002\u0000\n\n00\u0002\u0000))66\u0001"+
		"\u000035\u0001\u0000\u001c\u001d\u0001\u0000SU\u0369\u0000\u00a1\u0001"+
		"\u0000\u0000\u0000\u0002\u00b5\u0001\u0000\u0000\u0000\u0004\u00d2\u0001"+
		"\u0000\u0000\u0000\u0006\u00d4\u0001\u0000\u0000\u0000\b\u00e7\u0001\u0000"+
		"\u0000\u0000\n\u00e9\u0001\u0000\u0000\u0000\f\u00f0\u0001\u0000\u0000"+
		"\u0000\u000e\u00f7\u0001\u0000\u0000\u0000\u0010\u00fc\u0001\u0000\u0000"+
		"\u0000\u0012\u010a\u0001\u0000\u0000\u0000\u0014\u0113\u0001\u0000\u0000"+
		"\u0000\u0016\u0124\u0001\u0000\u0000\u0000\u0018\u0132\u0001\u0000\u0000"+
		"\u0000\u001a\u0139\u0001\u0000\u0000\u0000\u001c\u013f\u0001\u0000\u0000"+
		"\u0000\u001e\u0151\u0001\u0000\u0000\u0000 \u0163\u0001\u0000\u0000\u0000"+
		"\"\u0165\u0001\u0000\u0000\u0000$\u017b\u0001\u0000\u0000\u0000&\u017e"+
		"\u0001\u0000\u0000\u0000(\u0188\u0001\u0000\u0000\u0000*\u018a\u0001\u0000"+
		"\u0000\u0000,\u01a4\u0001\u0000\u0000\u0000.\u01ae\u0001\u0000\u0000\u0000"+
		"0\u01b0\u0001\u0000\u0000\u00002\u01bf\u0001\u0000\u0000\u00004\u01c6"+
		"\u0001\u0000\u0000\u00006\u01c8\u0001\u0000\u0000\u00008\u01ca\u0001\u0000"+
		"\u0000\u0000:\u01d0\u0001\u0000\u0000\u0000<\u01db\u0001\u0000\u0000\u0000"+
		">\u01e0\u0001\u0000\u0000\u0000@\u01ec\u0001\u0000\u0000\u0000B\u01fa"+
		"\u0001\u0000\u0000\u0000D\u0201\u0001\u0000\u0000\u0000F\u0214\u0001\u0000"+
		"\u0000\u0000H\u0216\u0001\u0000\u0000\u0000J\u021a\u0001\u0000\u0000\u0000"+
		"L\u022d\u0001\u0000\u0000\u0000N\u0237\u0001\u0000\u0000\u0000P\u0239"+
		"\u0001\u0000\u0000\u0000R\u0241\u0001\u0000\u0000\u0000T\u0245\u0001\u0000"+
		"\u0000\u0000V\u024d\u0001\u0000\u0000\u0000X\u024f\u0001\u0000\u0000\u0000"+
		"Z\u025b\u0001\u0000\u0000\u0000\\\u026e\u0001\u0000\u0000\u0000^\u027f"+
		"\u0001\u0000\u0000\u0000`\u0281\u0001\u0000\u0000\u0000b\u0283\u0001\u0000"+
		"\u0000\u0000d\u0285\u0001\u0000\u0000\u0000f\u028d\u0001\u0000\u0000\u0000"+
		"h\u029f\u0001\u0000\u0000\u0000j\u02b1\u0001\u0000\u0000\u0000l\u02b6"+
		"\u0001\u0000\u0000\u0000n\u02d3\u0001\u0000\u0000\u0000p\u02d5\u0001\u0000"+
		"\u0000\u0000r\u02de\u0001\u0000\u0000\u0000t\u02e1\u0001\u0000\u0000\u0000"+
		"v\u02e5\u0001\u0000\u0000\u0000x\u02ed\u0001\u0000\u0000\u0000z\u02f7"+
		"\u0001\u0000\u0000\u0000|\u02f9\u0001\u0000\u0000\u0000~\u0303\u0001\u0000"+
		"\u0000\u0000\u0080\u0305\u0001\u0000\u0000\u0000\u0082\u0307\u0001\u0000"+
		"\u0000\u0000\u0084\u030a\u0001\u0000\u0000\u0000\u0086\u0310\u0001\u0000"+
		"\u0000\u0000\u0088\u0314\u0001\u0000\u0000\u0000\u008a\u0316\u0001\u0000"+
		"\u0000\u0000\u008c\u0318\u0001\u0000\u0000\u0000\u008e\u031a\u0001\u0000"+
		"\u0000\u0000\u0090\u031c\u0001\u0000\u0000\u0000\u0092\u031e\u0001\u0000"+
		"\u0000\u0000\u0094\u0320\u0001\u0000\u0000\u0000\u0096\u0322\u0001\u0000"+
		"\u0000\u0000\u0098\u0326\u0001\u0000\u0000\u0000\u009a\u0328\u0001\u0000"+
		"\u0000\u0000\u009c\u032a\u0001\u0000\u0000\u0000\u009e\u00a0\u0003\u0098"+
		"L\u0000\u009f\u009e\u0001\u0000\u0000\u0000\u00a0\u00a3\u0001\u0000\u0000"+
		"\u0000\u00a1\u009f\u0001\u0000\u0000\u0000\u00a1\u00a2\u0001\u0000\u0000"+
		"\u0000\u00a2\u00a5\u0001\u0000\u0000\u0000\u00a3\u00a1\u0001\u0000\u0000"+
		"\u0000\u00a4\u00a6\u0003\u0002\u0001\u0000\u00a5\u00a4\u0001\u0000\u0000"+
		"\u0000\u00a5\u00a6\u0001\u0000\u0000\u0000\u00a6\u00b0\u0001\u0000\u0000"+
		"\u0000\u00a7\u00ab\u0003\u0004\u0002\u0000\u00a8\u00aa\u0003\u0098L\u0000"+
		"\u00a9\u00a8\u0001\u0000\u0000\u0000\u00aa\u00ad\u0001\u0000\u0000\u0000"+
		"\u00ab\u00a9\u0001\u0000\u0000\u0000\u00ab\u00ac\u0001\u0000\u0000\u0000"+
		"\u00ac\u00af\u0001\u0000\u0000\u0000\u00ad\u00ab\u0001\u0000\u0000\u0000"+
		"\u00ae\u00a7\u0001\u0000\u0000\u0000\u00af\u00b2\u0001\u0000\u0000\u0000"+
		"\u00b0\u00ae\u0001\u0000\u0000\u0000\u00b0\u00b1\u0001\u0000\u0000\u0000"+
		"\u00b1\u00b3\u0001\u0000\u0000\u0000\u00b2\u00b0\u0001\u0000\u0000\u0000"+
		"\u00b3\u00b4\u0005\u0000\u0000\u0001\u00b4\u0001\u0001\u0000\u0000\u0000"+
		"\u00b5\u00b9\u0003\u0006\u0003\u0000\u00b6\u00b8\u0003\u0098L\u0000\u00b7"+
		"\u00b6\u0001\u0000\u0000\u0000\u00b8\u00bb\u0001\u0000\u0000\u0000\u00b9"+
		"\u00b7\u0001\u0000\u0000\u0000\u00b9\u00ba\u0001\u0000\u0000\u0000\u00ba"+
		"\u00bc\u0001\u0000\u0000\u0000\u00bb\u00b9\u0001\u0000\u0000\u0000\u00bc"+
		"\u00c0\u0005\u001e\u0000\u0000\u00bd\u00bf\u0003\u0098L\u0000\u00be\u00bd"+
		"\u0001\u0000\u0000\u0000\u00bf\u00c2\u0001\u0000\u0000\u0000\u00c0\u00be"+
		"\u0001\u0000\u0000\u0000\u00c0\u00c1\u0001\u0000\u0000\u0000\u00c1\u0003"+
		"\u0001\u0000\u0000\u0000\u00c2\u00c0\u0001\u0000\u0000\u0000\u00c3\u00c7"+
		"\u0003\u0006\u0003\u0000\u00c4\u00c6\u0003\u0098L\u0000\u00c5\u00c4\u0001"+
		"\u0000\u0000\u0000\u00c6\u00c9\u0001\u0000\u0000\u0000\u00c7\u00c5\u0001"+
		"\u0000\u0000\u0000\u00c7\u00c8\u0001\u0000\u0000\u0000\u00c8\u00cd\u0001"+
		"\u0000\u0000\u0000\u00c9\u00c7\u0001\u0000\u0000\u0000\u00ca\u00ce\u0003"+
		"\f\u0006\u0000\u00cb\u00ce\u0003\u0014\n\u0000\u00cc\u00ce\u0003\u000e"+
		"\u0007\u0000\u00cd\u00ca\u0001\u0000\u0000\u0000\u00cd\u00cb\u0001\u0000"+
		"\u0000\u0000\u00cd\u00cc\u0001\u0000\u0000\u0000\u00ce\u00d3\u0001\u0000"+
		"\u0000\u0000\u00cf\u00d3\u0003\f\u0006\u0000\u00d0\u00d3\u0003\u0014\n"+
		"\u0000\u00d1\u00d3\u0003\u000e\u0007\u0000\u00d2\u00c3\u0001\u0000\u0000"+
		"\u0000\u00d2\u00cf\u0001\u0000\u0000\u0000\u00d2\u00d0\u0001\u0000\u0000"+
		"\u0000\u00d2\u00d1\u0001\u0000\u0000\u0000\u00d3\u0005\u0001\u0000\u0000"+
		"\u0000\u00d4\u00de\u0003\b\u0004\u0000\u00d5\u00d7\u0003\u0098L\u0000"+
		"\u00d6\u00d5\u0001\u0000\u0000\u0000\u00d7\u00da\u0001\u0000\u0000\u0000"+
		"\u00d8\u00d6\u0001\u0000\u0000\u0000\u00d8\u00d9\u0001\u0000\u0000\u0000"+
		"\u00d9\u00db\u0001\u0000\u0000\u0000\u00da\u00d8\u0001\u0000\u0000\u0000"+
		"\u00db\u00dd\u0003\b\u0004\u0000\u00dc\u00d8\u0001\u0000\u0000\u0000\u00dd"+
		"\u00e0\u0001\u0000\u0000\u0000\u00de\u00dc\u0001\u0000\u0000\u0000\u00de"+
		"\u00df\u0001\u0000\u0000\u0000\u00df\u0007\u0001\u0000\u0000\u0000\u00e0"+
		"\u00de\u0001\u0000\u0000\u0000\u00e1\u00e2\u0005\u0013\u0000\u0000\u00e2"+
		"\u00e4\u0005\n\u0000\u0000\u00e3\u00e5\u0003n7\u0000\u00e4\u00e3\u0001"+
		"\u0000\u0000\u0000\u00e4\u00e5\u0001\u0000\u0000\u0000\u00e5\u00e8\u0001"+
		"\u0000\u0000\u0000\u00e6\u00e8\u0003\n\u0005\u0000\u00e7\u00e1\u0001\u0000"+
		"\u0000\u0000\u00e7\u00e6\u0001\u0000\u0000\u0000\u00e8\t\u0001\u0000\u0000"+
		"\u0000\u00e9\u00ea\u0005\u0012\u0000\u0000\u00ea\u00eb\u0003V+\u0000\u00eb"+
		"\u00ec\u0005K\u0000\u0000\u00ec\u000b\u0001\u0000\u0000\u0000\u00ed\u00ef"+
		"\u0003\u0094J\u0000\u00ee\u00ed\u0001\u0000\u0000\u0000\u00ef\u00f2\u0001"+
		"\u0000\u0000\u0000\u00f0\u00ee\u0001\u0000\u0000\u0000\u00f0\u00f1\u0001"+
		"\u0000\u0000\u0000\u00f1\u00f3\u0001\u0000\u0000\u0000\u00f2\u00f0\u0001"+
		"\u0000\u0000\u0000\u00f3\u00f4\u0005(\u0000\u0000\u00f4\u00f5\u0003\u0086"+
		"C\u0000\u00f5\u00f6\u0003\u0010\b\u0000\u00f6\r\u0001\u0000\u0000\u0000"+
		"\u00f7\u00f8\u0005%\u0000\u0000\u00f8\u00f9\u0005\n\u0000\u0000\u00f9"+
		"\u00fa\u00059\u0000\u0000\u00fa\u00fb\u0003\u001a\r\u0000\u00fb\u000f"+
		"\u0001\u0000\u0000\u0000\u00fc\u0102\u0005\u001f\u0000\u0000\u00fd\u0101"+
		"\u0003\u0012\t\u0000\u00fe\u0101\u0003\u0014\n\u0000\u00ff\u0101\u0003"+
		"\u0098L\u0000\u0100\u00fd\u0001\u0000\u0000\u0000\u0100\u00fe\u0001\u0000"+
		"\u0000\u0000\u0100\u00ff\u0001\u0000\u0000\u0000\u0101\u0104\u0001\u0000"+
		"\u0000\u0000\u0102\u0100\u0001\u0000\u0000\u0000\u0102\u0103\u0001\u0000"+
		"\u0000\u0000\u0103\u0105\u0001\u0000\u0000\u0000\u0104\u0102\u0001\u0000"+
		"\u0000\u0000\u0105\u0106\u0005 \u0000\u0000\u0106\u0011\u0001\u0000\u0000"+
		"\u0000\u0107\u0109\u0003\u0092I\u0000\u0108\u0107\u0001\u0000\u0000\u0000"+
		"\u0109\u010c\u0001\u0000\u0000\u0000\u010a\u0108\u0001\u0000\u0000\u0000"+
		"\u010a\u010b\u0001\u0000\u0000\u0000\u010b\u010d\u0001\u0000\u0000\u0000"+
		"\u010c\u010a\u0001\u0000\u0000\u0000\u010d\u010e\u0005\'\u0000\u0000\u010e"+
		"\u010f\u0003*\u0015\u0000\u010f\u0013\u0001\u0000\u0000\u0000\u0110\u0112"+
		"\u0003\u0096K\u0000\u0111\u0110\u0001\u0000\u0000\u0000\u0112\u0115\u0001"+
		"\u0000\u0000\u0000\u0113\u0111\u0001\u0000\u0000\u0000\u0113\u0114\u0001"+
		"\u0000\u0000\u0000\u0114\u0116\u0001\u0000\u0000\u0000\u0115\u0113\u0001"+
		"\u0000\u0000\u0000\u0116\u0117\u0005#\u0000\u0000\u0117\u0119\u0003\u0088"+
		"D\u0000\u0118\u011a\u0003\u0016\u000b\u0000\u0119\u0118\u0001\u0000\u0000"+
		"\u0000\u0119\u011a\u0001\u0000\u0000\u0000\u011a\u011e\u0001\u0000\u0000"+
		"\u0000\u011b\u011d\u0003\u0098L\u0000\u011c\u011b\u0001\u0000\u0000\u0000"+
		"\u011d\u0120\u0001\u0000\u0000\u0000\u011e\u011c\u0001\u0000\u0000\u0000"+
		"\u011e\u011f\u0001\u0000\u0000\u0000\u011f\u0121\u0001\u0000\u0000\u0000"+
		"\u0120\u011e\u0001\u0000\u0000\u0000\u0121\u0122\u0003\u0018\f\u0000\u0122"+
		"\u0015\u0001\u0000\u0000\u0000\u0123\u0125\u0003$\u0012\u0000\u0124\u0123"+
		"\u0001\u0000\u0000\u0000\u0124\u0125\u0001\u0000\u0000\u0000\u0125\u0126"+
		"\u0001\u0000\u0000\u0000\u0126\u0127\u0005:\u0000\u0000\u0127\u0128\u0003"+
		"\u001a\r\u0000\u0128\u0017\u0001\u0000\u0000\u0000\u0129\u012b\u0005&"+
		"\u0000\u0000\u012a\u0129\u0001\u0000\u0000\u0000\u012a\u012b\u0001\u0000"+
		"\u0000\u0000\u012b\u012c\u0001\u0000\u0000\u0000\u012c\u0133\u0003*\u0015"+
		"\u0000\u012d\u012e\u0005\u0014\u0000\u0000\u012e\u0130\u0003V+\u0000\u012f"+
		"\u0131\u0005\f\u0000\u0000\u0130\u012f\u0001\u0000\u0000\u0000\u0130\u0131"+
		"\u0001\u0000\u0000\u0000\u0131\u0133\u0001\u0000\u0000\u0000\u0132\u012a"+
		"\u0001\u0000\u0000\u0000\u0132\u012d\u0001\u0000\u0000\u0000\u0133\u0019"+
		"\u0001\u0000\u0000\u0000\u0134\u0136\u0003\u0006\u0003\u0000\u0135\u0137"+
		"\u0003\u001c\u000e\u0000\u0136\u0135\u0001\u0000\u0000\u0000\u0136\u0137"+
		"\u0001\u0000\u0000\u0000\u0137\u013a\u0001\u0000\u0000\u0000\u0138\u013a"+
		"\u0003\u001c\u000e\u0000\u0139\u0134\u0001\u0000\u0000\u0000\u0139\u0138"+
		"\u0001\u0000\u0000\u0000\u013a\u001b\u0001\u0000\u0000\u0000\u013b\u0140"+
		"\u0003(\u0014\u0000\u013c\u0140\u0003\u001e\u000f\u0000\u013d\u0140\u0003"+
		" \u0010\u0000\u013e\u0140\u0003\"\u0011\u0000\u013f\u013b\u0001\u0000"+
		"\u0000\u0000\u013f\u013c\u0001\u0000\u0000\u0000\u013f\u013d\u0001\u0000"+
		"\u0000\u0000\u013f\u013e\u0001\u0000\u0000\u0000\u0140\u001d\u0001\u0000"+
		"\u0000\u0000\u0141\u0142\u0005\u000e\u0000\u0000\u0142\u0152\u0005 \u0000"+
		"\u0000\u0143\u0144\u0005\u000e\u0000\u0000\u0144\u0149\u0003&\u0013\u0000"+
		"\u0145\u0146\u0005I\u0000\u0000\u0146\u0148\u0003&\u0013\u0000\u0147\u0145"+
		"\u0001\u0000\u0000\u0000\u0148\u014b\u0001\u0000\u0000\u0000\u0149\u0147"+
		"\u0001\u0000\u0000\u0000\u0149\u014a\u0001\u0000\u0000\u0000\u014a\u014d"+
		"\u0001\u0000\u0000\u0000\u014b\u0149\u0001\u0000\u0000\u0000\u014c\u014e"+
		"\u0005I\u0000\u0000\u014d\u014c\u0001\u0000\u0000\u0000\u014d\u014e\u0001"+
		"\u0000\u0000\u0000\u014e\u014f\u0001\u0000\u0000\u0000\u014f\u0150\u0005"+
		" \u0000\u0000\u0150\u0152\u0001\u0000\u0000\u0000\u0151\u0141\u0001\u0000"+
		"\u0000\u0000\u0151\u0143\u0001\u0000\u0000\u0000\u0152\u001f\u0001\u0000"+
		"\u0000\u0000\u0153\u0154\u0005\u000f\u0000\u0000\u0154\u0164\u0005K\u0000"+
		"\u0000\u0155\u0156\u0005\u000f\u0000\u0000\u0156\u015b\u0003&\u0013\u0000"+
		"\u0157\u0158\u0005I\u0000\u0000\u0158\u015a\u0003&\u0013\u0000\u0159\u0157"+
		"\u0001\u0000\u0000\u0000\u015a\u015d\u0001\u0000\u0000\u0000\u015b\u0159"+
		"\u0001\u0000\u0000\u0000\u015b\u015c\u0001\u0000\u0000\u0000\u015c\u015f"+
		"\u0001\u0000\u0000\u0000\u015d\u015b\u0001\u0000\u0000\u0000\u015e\u0160"+
		"\u0005I\u0000\u0000\u015f\u015e\u0001\u0000\u0000\u0000\u015f\u0160\u0001"+
		"\u0000\u0000\u0000\u0160\u0161\u0001\u0000\u0000\u0000\u0161\u0162\u0005"+
		"K\u0000\u0000\u0162\u0164\u0001\u0000\u0000\u0000\u0163\u0153\u0001\u0000"+
		"\u0000\u0000\u0163\u0155\u0001\u0000\u0000\u0000\u0164!\u0001\u0000\u0000"+
		"\u0000\u0165\u0166\u0005$\u0000\u0000\u0166\u0167\u0003$\u0012\u0000\u0167"+
		"\u0168\u0005:\u0000\u0000\u0168\u0169\u0003\u001a\r\u0000\u0169#\u0001"+
		"\u0000\u0000\u0000\u016a\u016b\u0005=\u0000\u0000\u016b\u0170\u0003&\u0013"+
		"\u0000\u016c\u016d\u0005I\u0000\u0000\u016d\u016f\u0003&\u0013\u0000\u016e"+
		"\u016c\u0001\u0000\u0000\u0000\u016f\u0172\u0001\u0000\u0000\u0000\u0170"+
		"\u016e\u0001\u0000\u0000\u0000\u0170\u0171\u0001\u0000\u0000\u0000\u0171"+
		"\u0174\u0001\u0000\u0000\u0000\u0172\u0170\u0001\u0000\u0000\u0000\u0173"+
		"\u0175\u0005I\u0000\u0000\u0174\u0173\u0001\u0000\u0000\u0000\u0174\u0175"+
		"\u0001\u0000\u0000\u0000\u0175\u0176\u0001\u0000\u0000\u0000\u0176\u0177"+
		"\u0005J\u0000\u0000\u0177\u017c\u0001\u0000\u0000\u0000\u0178\u0179\u0005"+
		"=\u0000\u0000\u0179\u017c\u0005J\u0000\u0000\u017a\u017c\u0005<\u0000"+
		"\u0000\u017b\u016a\u0001\u0000\u0000\u0000\u017b\u0178\u0001\u0000\u0000"+
		"\u0000\u017b\u017a\u0001\u0000\u0000\u0000\u017c%\u0001\u0000\u0000\u0000"+
		"\u017d\u017f\u0003\u0006\u0003\u0000\u017e\u017d\u0001\u0000\u0000\u0000"+
		"\u017e\u017f\u0001\u0000\u0000\u0000\u017f\u0180\u0001\u0000\u0000\u0000"+
		"\u0180\u0182\u0003\u001c\u000e\u0000\u0181\u0183\u0005\r\u0000\u0000\u0182"+
		"\u0181\u0001\u0000\u0000\u0000\u0182\u0183\u0001\u0000\u0000\u0000\u0183"+
		"\u0186\u0001\u0000\u0000\u0000\u0184\u0185\u0005P\u0000\u0000\u0185\u0187"+
		"\u0003V+\u0000\u0186\u0184\u0001\u0000\u0000\u0000\u0186\u0187\u0001\u0000"+
		"\u0000\u0000\u0187\'\u0001\u0000\u0000\u0000\u0188\u0189\u0007\u0000\u0000"+
		"\u0000\u0189)\u0001\u0000\u0000\u0000\u018a\u0190\u0005\u001f\u0000\u0000"+
		"\u018b\u018f\u0003,\u0016\u0000\u018c\u018f\u0005!\u0000\u0000\u018d\u018f"+
		"\u0003\u0098L\u0000\u018e\u018b\u0001\u0000\u0000\u0000\u018e\u018c\u0001"+
		"\u0000\u0000\u0000\u018e\u018d\u0001\u0000\u0000\u0000\u018f\u0192\u0001"+
		"\u0000\u0000\u0000\u0190\u018e\u0001\u0000\u0000\u0000\u0190\u0191\u0001"+
		"\u0000\u0000\u0000\u0191\u0193\u0001\u0000\u0000\u0000\u0192\u0190\u0001"+
		"\u0000\u0000\u0000\u0193\u0194\u0005 \u0000\u0000\u0194+\u0001\u0000\u0000"+
		"\u0000\u0195\u0196\u0003\u0082A\u0000\u0196\u0197\u0005\u001b\u0000\u0000"+
		"\u0197\u01a5\u0001\u0000\u0000\u0000\u0198\u019a\u0003\u0082A\u0000\u0199"+
		"\u0198\u0001\u0000\u0000\u0000\u0199\u019a\u0001\u0000\u0000\u0000\u019a"+
		"\u019b\u0001\u0000\u0000\u0000\u019b\u01a5\u0003*\u0015\u0000\u019c\u01a5"+
		"\u0003.\u0017\u0000\u019d\u01a5\u0003F#\u0000\u019e\u01a5\u0003H$\u0000"+
		"\u019f\u01a5\u0003J%\u0000\u01a0\u01a5\u0003N\'\u0000\u01a1\u01a5\u0003"+
		">\u001f\u0000\u01a2\u01a5\u00030\u0018\u0000\u01a3\u01a5\u00038\u001c"+
		"\u0000\u01a4\u0195\u0001\u0000\u0000\u0000\u01a4\u0199\u0001\u0000\u0000"+
		"\u0000\u01a4\u019c\u0001\u0000\u0000\u0000\u01a4\u019d\u0001\u0000\u0000"+
		"\u0000\u01a4\u019e\u0001\u0000\u0000\u0000\u01a4\u019f\u0001\u0000\u0000"+
		"\u0000\u01a4\u01a0\u0001\u0000\u0000\u0000\u01a4\u01a1\u0001\u0000\u0000"+
		"\u0000\u01a4\u01a2\u0001\u0000\u0000\u0000\u01a4\u01a3\u0001\u0000\u0000"+
		"\u0000\u01a5-\u0001\u0000\u0000\u0000\u01a6\u01a7\u0003\u008eG\u0000\u01a7"+
		"\u01a9\u0003n7\u0000\u01a8\u01aa\u0003f3\u0000\u01a9\u01a8\u0001\u0000"+
		"\u0000\u0000\u01a9\u01aa\u0001\u0000\u0000\u0000\u01aa\u01af\u0001\u0000"+
		"\u0000\u0000\u01ab\u01ac\u0003\u008eG\u0000\u01ac\u01ad\u0003f3\u0000"+
		"\u01ad\u01af\u0001\u0000\u0000\u0000\u01ae\u01a6\u0001\u0000\u0000\u0000"+
		"\u01ae\u01ab\u0001\u0000\u0000\u0000\u01af/\u0001\u0000\u0000\u0000\u01b0"+
		"\u01b1\u00052\u0000\u0000\u01b1\u01b2\u00036\u001b\u0000\u01b2\u01bd\u0003"+
		"*\u0015\u0000\u01b3\u01b5\u0003\u0098L\u0000\u01b4\u01b3\u0001\u0000\u0000"+
		"\u0000\u01b5\u01b8\u0001\u0000\u0000\u0000\u01b6\u01b4\u0001\u0000\u0000"+
		"\u0000\u01b6\u01b7\u0001\u0000\u0000\u0000\u01b7\u01b9\u0001\u0000\u0000"+
		"\u0000\u01b8\u01b6\u0001\u0000\u0000\u0000\u01b9\u01ba\u0005+\u0000\u0000"+
		"\u01ba\u01bb\u00034\u001a\u0000\u01bb\u01bc\u0005\u001b\u0000\u0000\u01bc"+
		"\u01be\u0001\u0000\u0000\u0000\u01bd\u01b6\u0001\u0000\u0000\u0000\u01bd"+
		"\u01be\u0001\u0000\u0000\u0000\u01be1\u0001\u0000\u0000\u0000\u01bf\u01c0"+
		"\u0005+\u0000\u0000\u01c0\u01c1\u0005:\u0000\u0000\u01c1\u01c2\u00034"+
		"\u001a\u0000\u01c2\u01c3\u0005\u001b\u0000\u0000\u01c33\u0001\u0000\u0000"+
		"\u0000\u01c4\u01c7\u0003V+\u0000\u01c5\u01c7\u0003,\u0016\u0000\u01c6"+
		"\u01c4\u0001\u0000\u0000\u0000\u01c6\u01c5\u0001\u0000\u0000\u0000\u01c7"+
		"5\u0001\u0000\u0000\u0000\u01c8\u01c9\u0003n7\u0000\u01c97\u0001\u0000"+
		"\u0000\u0000\u01ca\u01cb\u00052\u0000\u0000\u01cb\u01cc\u0005\u001f\u0000"+
		"\u0000\u01cc\u01cd\u0005\u001b\u0000\u0000\u01cd\u01ce\u0003:\u001d\u0000"+
		"\u01ce\u01cf\u0005 \u0000\u0000\u01cf9\u0001\u0000\u0000\u0000\u01d0\u01d5"+
		"\u0003<\u001e\u0000\u01d1\u01d4\u0003<\u001e\u0000\u01d2\u01d4\u0003\u0098"+
		"L\u0000\u01d3\u01d1\u0001\u0000\u0000\u0000\u01d3\u01d2\u0001\u0000\u0000"+
		"\u0000\u01d4\u01d7\u0001\u0000\u0000\u0000\u01d5\u01d3\u0001\u0000\u0000"+
		"\u0000\u01d5\u01d6\u0001\u0000\u0000\u0000\u01d6\u01d9\u0001\u0000\u0000"+
		"\u0000\u01d7\u01d5\u0001\u0000\u0000\u0000\u01d8\u01da\u00032\u0019\u0000"+
		"\u01d9\u01d8\u0001\u0000\u0000\u0000\u01d9\u01da\u0001\u0000\u0000\u0000"+
		"\u01da;\u0001\u0000\u0000\u0000\u01db\u01dc\u00036\u001b\u0000\u01dc\u01dd"+
		"\u0005:\u0000\u0000\u01dd\u01de\u00034\u001a\u0000\u01de\u01df\u0005\u001b"+
		"\u0000\u0000\u01df=\u0001\u0000\u0000\u0000\u01e0\u01e2\u00052\u0000\u0000"+
		"\u01e1\u01e3\u0003V+\u0000\u01e2\u01e1\u0001\u0000\u0000\u0000\u01e2\u01e3"+
		"\u0001\u0000\u0000\u0000\u01e3\u01e4\u0001\u0000\u0000\u0000\u01e4\u01e5"+
		"\u0005\u001f\u0000\u0000\u01e5\u01e6\u0005\u001b\u0000\u0000\u01e6\u01e7"+
		"\u0003@ \u0000\u01e7\u01e8\u0005 \u0000\u0000\u01e8?\u0001\u0000\u0000"+
		"\u0000\u01e9\u01eb\u0003\u0098L\u0000\u01ea\u01e9\u0001\u0000\u0000\u0000"+
		"\u01eb\u01ee\u0001\u0000\u0000\u0000\u01ec\u01ea\u0001\u0000\u0000\u0000"+
		"\u01ec\u01ed\u0001\u0000\u0000\u0000\u01ed\u01ef\u0001\u0000\u0000\u0000"+
		"\u01ee\u01ec\u0001\u0000\u0000\u0000\u01ef\u01f4\u0003B!\u0000\u01f0\u01f3"+
		"\u0003B!\u0000\u01f1\u01f3\u0003\u0098L\u0000\u01f2\u01f0\u0001\u0000"+
		"\u0000\u0000\u01f2\u01f1\u0001\u0000\u0000\u0000\u01f3\u01f6\u0001\u0000"+
		"\u0000\u0000\u01f4\u01f2\u0001\u0000\u0000\u0000\u01f4\u01f5\u0001\u0000"+
		"\u0000\u0000\u01f5\u01f8\u0001\u0000\u0000\u0000\u01f6\u01f4\u0001\u0000"+
		"\u0000\u0000\u01f7\u01f9\u00032\u0019\u0000\u01f8\u01f7\u0001\u0000\u0000"+
		"\u0000\u01f8\u01f9\u0001\u0000\u0000\u0000\u01f9A\u0001\u0000\u0000\u0000"+
		"\u01fa\u01fb\u0003D\"\u0000\u01fb\u01fc\u0005:\u0000\u0000\u01fc\u01fd"+
		"\u00034\u001a\u0000\u01fd\u01fe\u0005\u001b\u0000\u0000\u01feC\u0001\u0000"+
		"\u0000\u0000\u01ff\u0202\u0003V+\u0000\u0200\u0202\u0003v;\u0000\u0201"+
		"\u01ff\u0001\u0000\u0000\u0000\u0201\u0200\u0001\u0000\u0000\u0000\u0202"+
		"E\u0001\u0000\u0000\u0000\u0203\u0205\u00051\u0000\u0000\u0204\u0203\u0001"+
		"\u0000\u0000\u0000\u0204\u0205\u0001\u0000\u0000\u0000\u0205\u0206\u0001"+
		"\u0000\u0000\u0000\u0206\u0207\u00050\u0000\u0000\u0207\u0209\u0003\u008a"+
		"E\u0000\u0208\u020a\u0003L&\u0000\u0209\u0208\u0001\u0000\u0000\u0000"+
		"\u0209\u020a\u0001\u0000\u0000\u0000\u020a\u0215\u0001\u0000\u0000\u0000"+
		"\u020b\u020d\u00051\u0000\u0000\u020c\u020b\u0001\u0000\u0000\u0000\u020c"+
		"\u020d\u0001\u0000\u0000\u0000\u020d\u020e\u0001\u0000\u0000\u0000\u020e"+
		"\u020f\u00050\u0000\u0000\u020f\u0210\u0003\u001a\r\u0000\u0210\u0212"+
		"\u0003\u008aE\u0000\u0211\u0213\u0003L&\u0000\u0212\u0211\u0001\u0000"+
		"\u0000\u0000\u0212\u0213\u0001\u0000\u0000\u0000\u0213\u0215\u0001\u0000"+
		"\u0000\u0000\u0214\u0204\u0001\u0000\u0000\u0000\u0214\u020c\u0001\u0000"+
		"\u0000\u0000\u0215G\u0001\u0000\u0000\u0000\u0216\u0217\u0005L\u0000\u0000"+
		"\u0217\u0218\u0003\u008aE\u0000\u0218\u0219\u0003L&\u0000\u0219I\u0001"+
		"\u0000\u0000\u0000\u021a\u021b\u00058\u0000\u0000\u021b\u021d\u0003\u008c"+
		"F\u0000\u021c\u021e\u0003n7\u0000\u021d\u021c\u0001\u0000\u0000\u0000"+
		"\u021d\u021e\u0001\u0000\u0000\u0000\u021e\u0222\u0001\u0000\u0000\u0000"+
		"\u021f\u0221\u0003~?\u0000\u0220\u021f\u0001\u0000\u0000\u0000\u0221\u0224"+
		"\u0001\u0000\u0000\u0000\u0222\u0220\u0001\u0000\u0000\u0000\u0222\u0223"+
		"\u0001\u0000\u0000\u0000\u0223K\u0001\u0000\u0000\u0000\u0224\u0222\u0001"+
		"\u0000\u0000\u0000\u0225\u022a\u00059\u0000\u0000\u0226\u022b\u0003V+"+
		"\u0000\u0227\u022b\u0003>\u001f\u0000\u0228\u022b\u00038\u001c\u0000\u0229"+
		"\u022b\u0003.\u0017\u0000\u022a\u0226\u0001\u0000\u0000\u0000\u022a\u0227"+
		"\u0001\u0000\u0000\u0000\u022a\u0228\u0001\u0000\u0000\u0000\u022a\u0229"+
		"\u0001\u0000\u0000\u0000\u022b\u022e\u0001\u0000\u0000\u0000\u022c\u022e"+
		"\u0003f3\u0000\u022d\u0225\u0001\u0000\u0000\u0000\u022d\u022c\u0001\u0000"+
		"\u0000\u0000\u022eM\u0001\u0000\u0000\u0000\u022f\u0231\u0005,\u0000\u0000"+
		"\u0230\u0232\u0003n7\u0000\u0231\u0230\u0001\u0000\u0000\u0000\u0231\u0232"+
		"\u0001\u0000\u0000\u0000\u0232\u0238\u0001\u0000\u0000\u0000\u0233\u0234"+
		"\u0005-\u0000\u0000\u0234\u0238\u0005\n\u0000\u0000\u0235\u0238\u0005"+
		"/\u0000\u0000\u0236\u0238\u0005.\u0000\u0000\u0237\u022f\u0001\u0000\u0000"+
		"\u0000\u0237\u0233\u0001\u0000\u0000\u0000\u0237\u0235\u0001\u0000\u0000"+
		"\u0000\u0237\u0236\u0001\u0000\u0000\u0000\u0238O\u0001\u0000\u0000\u0000"+
		"\u0239\u023a\u0005\u0007\u0000\u0000\u023a\u023b\u0003R)\u0000\u023b\u023c"+
		"\u0005\u0002\u0000\u0000\u023cQ\u0001\u0000\u0000\u0000\u023d\u0242\u0005"+
		"B\u0000\u0000\u023e\u0242\u0005C\u0000\u0000\u023f\u0242\u0005D\u0000"+
		"\u0000\u0240\u0242\u0003T*\u0000\u0241\u023d\u0001\u0000\u0000\u0000\u0241"+
		"\u023e\u0001\u0000\u0000\u0000\u0241\u023f\u0001\u0000\u0000\u0000\u0241"+
		"\u0240\u0001\u0000\u0000\u0000\u0242\u0243\u0001\u0000\u0000\u0000\u0243"+
		"\u0241\u0001\u0000\u0000\u0000\u0243\u0244\u0001\u0000\u0000\u0000\u0244"+
		"S\u0001\u0000\u0000\u0000\u0245\u0246\u0005\b\u0000\u0000\u0246\u0247"+
		"\u0003x<\u0000\u0247\u0249\u0005 \u0000\u0000\u0248\u024a\u0005V\u0000"+
		"\u0000\u0249\u0248\u0001\u0000\u0000\u0000\u0249\u024a\u0001\u0000\u0000"+
		"\u0000\u024aU\u0001\u0000\u0000\u0000\u024b\u024e\u0003Z-\u0000\u024c"+
		"\u024e\u0003p8\u0000\u024d\u024b\u0001\u0000\u0000\u0000\u024d\u024c\u0001"+
		"\u0000\u0000\u0000\u024eW\u0001\u0000\u0000\u0000\u024f\u0250\u0005;\u0000"+
		"\u0000\u0250\u0251\u0003V+\u0000\u0251\u0252\u0005\f\u0000\u0000\u0252"+
		"Y\u0001\u0000\u0000\u0000\u0253\u0254\u0006-\uffff\uffff\u0000\u0254\u0256"+
		"\u0003\\.\u0000\u0255\u0257\u0003|>\u0000\u0256\u0255\u0001\u0000\u0000"+
		"\u0000\u0256\u0257\u0001\u0000\u0000\u0000\u0257\u025c\u0001\u0000\u0000"+
		"\u0000\u0258\u0259\u0003`0\u0000\u0259\u025a\u0003Z-\u0002\u025a\u025c"+
		"\u0001\u0000\u0000\u0000\u025b\u0253\u0001\u0000\u0000\u0000\u025b\u0258"+
		"\u0001\u0000\u0000\u0000\u025c\u0261\u0001\u0000\u0000\u0000\u025d\u025e"+
		"\n\u0001\u0000\u0000\u025e\u0260\u0003b1\u0000\u025f\u025d\u0001\u0000"+
		"\u0000\u0000\u0260\u0263\u0001\u0000\u0000\u0000\u0261\u025f\u0001\u0000"+
		"\u0000\u0000\u0261\u0262\u0001\u0000\u0000\u0000\u0262[\u0001\u0000\u0000"+
		"\u0000\u0263\u0261\u0001\u0000\u0000\u0000\u0264\u026f\u0003t:\u0000\u0265"+
		"\u026f\u0003^/\u0000\u0266\u026f\u0003x<\u0000\u0267\u026f\u0003j5\u0000"+
		"\u0268\u026f\u0003h4\u0000\u0269\u026f\u0003X,\u0000\u026a\u026f\u0003"+
		"d2\u0000\u026b\u026f\u0005\u0015\u0000\u0000\u026c\u026f\u0005\u0016\u0000"+
		"\u0000\u026d\u026f\u0005\u0017\u0000\u0000\u026e\u0264\u0001\u0000\u0000"+
		"\u0000\u026e\u0265\u0001\u0000\u0000\u0000\u026e\u0266\u0001\u0000\u0000"+
		"\u0000\u026e\u0267\u0001\u0000\u0000\u0000\u026e\u0268\u0001\u0000\u0000"+
		"\u0000\u026e\u0269\u0001\u0000\u0000\u0000\u026e\u026a\u0001\u0000\u0000"+
		"\u0000\u026e\u026b\u0001\u0000\u0000\u0000\u026e\u026c\u0001\u0000\u0000"+
		"\u0000\u026e\u026d\u0001\u0000\u0000\u0000\u026f]\u0001\u0000\u0000\u0000"+
		"\u0270\u0276\u0005\u0010\u0000\u0000\u0271\u0275\u0003,\u0016\u0000\u0272"+
		"\u0275\u0005!\u0000\u0000\u0273\u0275\u0003\u0098L\u0000\u0274\u0271\u0001"+
		"\u0000\u0000\u0000\u0274\u0272\u0001\u0000\u0000\u0000\u0274\u0273\u0001"+
		"\u0000\u0000\u0000\u0275\u0278\u0001\u0000\u0000\u0000\u0276\u0274\u0001"+
		"\u0000\u0000\u0000\u0276\u0277\u0001\u0000\u0000\u0000\u0277\u0279\u0001"+
		"\u0000\u0000\u0000\u0278\u0276\u0001\u0000\u0000\u0000\u0279\u0280\u0005"+
		" \u0000\u0000\u027a\u027b\u0005\u0011\u0000\u0000\u027b\u027d\u0003V+"+
		"\u0000\u027c\u027e\u0005\f\u0000\u0000\u027d\u027c\u0001\u0000\u0000\u0000"+
		"\u027d\u027e\u0001\u0000\u0000\u0000\u027e\u0280\u0001\u0000\u0000\u0000"+
		"\u027f\u0270\u0001\u0000\u0000\u0000\u027f\u027a\u0001\u0000\u0000\u0000"+
		"\u0280_\u0001\u0000\u0000\u0000\u0281\u0282\u0005M\u0000\u0000\u0282a"+
		"\u0001\u0000\u0000\u0000\u0283\u0284\u0005N\u0000\u0000\u0284c\u0001\u0000"+
		"\u0000\u0000\u0285\u0286\u0003\u008cF\u0000\u0286\u0287\u0003n7\u0000"+
		"\u0287e\u0001\u0000\u0000\u0000\u0288\u028a\u0005\u001b\u0000\u0000\u0289"+
		"\u0288\u0001\u0000\u0000\u0000\u0289\u028a\u0001\u0000\u0000\u0000\u028a"+
		"\u028b\u0001\u0000\u0000\u0000\u028b\u028e\u0003P(\u0000\u028c\u028e\u0003"+
		"Z-\u0000\u028d\u0289\u0001\u0000\u0000\u0000\u028d\u028c\u0001\u0000\u0000"+
		"\u0000\u028eg\u0001\u0000\u0000\u0000\u028f\u0290\u0005\u000f\u0000\u0000"+
		"\u0290\u02a0\u0005K\u0000\u0000\u0291\u0292\u0005\u000f\u0000\u0000\u0292"+
		"\u0297\u0003V+\u0000\u0293\u0294\u0005I\u0000\u0000\u0294\u0296\u0003"+
		"V+\u0000\u0295\u0293\u0001\u0000\u0000\u0000\u0296\u0299\u0001\u0000\u0000"+
		"\u0000\u0297\u0295\u0001\u0000\u0000\u0000\u0297\u0298\u0001\u0000\u0000"+
		"\u0000\u0298\u029b\u0001\u0000\u0000\u0000\u0299\u0297\u0001\u0000\u0000"+
		"\u0000\u029a\u029c\u0005I\u0000\u0000\u029b\u029a\u0001\u0000\u0000\u0000"+
		"\u029b\u029c\u0001\u0000\u0000\u0000\u029c\u029d\u0001\u0000\u0000\u0000"+
		"\u029d\u029e\u0005K\u0000\u0000\u029e\u02a0\u0001\u0000\u0000\u0000\u029f"+
		"\u028f\u0001\u0000\u0000\u0000\u029f\u0291\u0001\u0000\u0000\u0000\u02a0"+
		"i\u0001\u0000\u0000\u0000\u02a1\u02a2\u0005\u000e\u0000\u0000\u02a2\u02b2"+
		"\u0005 \u0000\u0000\u02a3\u02a4\u0005\u000e\u0000\u0000\u02a4\u02a9\u0003"+
		"l6\u0000\u02a5\u02a6\u0005I\u0000\u0000\u02a6\u02a8\u0003l6\u0000\u02a7"+
		"\u02a5\u0001\u0000\u0000\u0000\u02a8\u02ab\u0001\u0000\u0000\u0000\u02a9"+
		"\u02a7\u0001\u0000\u0000\u0000\u02a9\u02aa\u0001\u0000\u0000\u0000\u02aa"+
		"\u02ad\u0001\u0000\u0000\u0000\u02ab\u02a9\u0001\u0000\u0000\u0000\u02ac"+
		"\u02ae\u0005I\u0000\u0000\u02ad\u02ac\u0001\u0000\u0000\u0000\u02ad\u02ae"+
		"\u0001\u0000\u0000\u0000\u02ae\u02af\u0001\u0000\u0000\u0000\u02af\u02b0"+
		"\u0005 \u0000\u0000\u02b0\u02b2\u0001\u0000\u0000\u0000\u02b1\u02a1\u0001"+
		"\u0000\u0000\u0000\u02b1\u02a3\u0001\u0000\u0000\u0000\u02b2k\u0001\u0000"+
		"\u0000\u0000\u02b3\u02b5\u0003\b\u0004\u0000\u02b4\u02b3\u0001\u0000\u0000"+
		"\u0000\u02b5\u02b8\u0001\u0000\u0000\u0000\u02b6\u02b4\u0001\u0000\u0000"+
		"\u0000\u02b6\u02b7\u0001\u0000\u0000\u0000\u02b7\u02b9\u0001\u0000\u0000"+
		"\u0000\u02b8\u02b6\u0001\u0000\u0000\u0000\u02b9\u02ba\u0005\r\u0000\u0000"+
		"\u02ba\u02bb\u0005P\u0000\u0000\u02bb\u02bc\u0003V+\u0000\u02bcm\u0001"+
		"\u0000\u0000\u0000\u02bd\u02be\u0005=\u0000\u0000\u02be\u02c3\u0003V+"+
		"\u0000\u02bf\u02c0\u0005I\u0000\u0000\u02c0\u02c2\u0003V+\u0000\u02c1"+
		"\u02bf\u0001\u0000\u0000\u0000\u02c2\u02c5\u0001\u0000\u0000\u0000\u02c3"+
		"\u02c1\u0001\u0000\u0000\u0000\u02c3\u02c4\u0001\u0000\u0000\u0000\u02c4"+
		"\u02c7\u0001\u0000\u0000\u0000\u02c5\u02c3\u0001\u0000\u0000\u0000\u02c6"+
		"\u02c8\u0005I\u0000\u0000\u02c7\u02c6\u0001\u0000\u0000\u0000\u02c7\u02c8"+
		"\u0001\u0000\u0000\u0000\u02c8\u02c9\u0001\u0000\u0000\u0000\u02c9\u02ca"+
		"\u0005J\u0000\u0000\u02ca\u02d4\u0001\u0000\u0000\u0000\u02cb\u02cc\u0005"+
		"=\u0000\u0000\u02cc\u02d4\u0005J\u0000\u0000\u02cd\u02d4\u0005<\u0000"+
		"\u0000\u02ce\u02d0\u0003p8\u0000\u02cf\u02ce\u0001\u0000\u0000\u0000\u02d0"+
		"\u02d1\u0001\u0000\u0000\u0000\u02d1\u02cf\u0001\u0000\u0000\u0000\u02d1"+
		"\u02d2\u0001\u0000\u0000\u0000\u02d2\u02d4\u0001\u0000\u0000\u0000\u02d3"+
		"\u02bd\u0001\u0000\u0000\u0000\u02d3\u02cb\u0001\u0000\u0000\u0000\u02d3"+
		"\u02cd\u0001\u0000\u0000\u0000\u02d3\u02cf\u0001\u0000\u0000\u0000\u02d4"+
		"o\u0001\u0000\u0000\u0000\u02d5\u02d6\u0005\u0004\u0000\u0000\u02d6\u02d7"+
		"\u0003r9\u0000\u02d7\u02d8\u0005A\u0000\u0000\u02d8q\u0001\u0000\u0000"+
		"\u0000\u02d9\u02dd\u0005@\u0000\u0000\u02da\u02dd\u0003\u009cN\u0000\u02db"+
		"\u02dd\u0003t:\u0000\u02dc\u02d9\u0001\u0000\u0000\u0000\u02dc\u02da\u0001"+
		"\u0000\u0000\u0000\u02dc\u02db\u0001\u0000\u0000\u0000\u02dd\u02e0\u0001"+
		"\u0000\u0000\u0000\u02de\u02dc\u0001\u0000\u0000\u0000\u02de\u02df\u0001"+
		"\u0000\u0000\u0000\u02dfs\u0001\u0000\u0000\u0000\u02e0\u02de\u0001\u0000"+
		"\u0000\u0000\u02e1\u02e2\u0005\t\u0000\u0000\u02e2\u02e3\u0003V+\u0000"+
		"\u02e3\u02e4\u0005K\u0000\u0000\u02e4u\u0001\u0000\u0000\u0000\u02e5\u02e7"+
		"\u0005\t\u0000\u0000\u02e6\u02e8\u0003~?\u0000\u02e7\u02e6\u0001\u0000"+
		"\u0000\u0000\u02e8\u02e9\u0001\u0000\u0000\u0000\u02e9\u02e7\u0001\u0000"+
		"\u0000\u0000\u02e9\u02ea\u0001\u0000\u0000\u0000\u02ea\u02eb\u0001\u0000"+
		"\u0000\u0000\u02eb\u02ec\u0005K\u0000\u0000\u02ecw\u0001\u0000\u0000\u0000"+
		"\u02ed\u02f1\u0003z=\u0000\u02ee\u02f0\u0003~?\u0000\u02ef\u02ee\u0001"+
		"\u0000\u0000\u0000\u02f0\u02f3\u0001\u0000\u0000\u0000\u02f1\u02ef\u0001"+
		"\u0000\u0000\u0000\u02f1\u02f2\u0001\u0000\u0000\u0000\u02f2y\u0001\u0000"+
		"\u0000\u0000\u02f3\u02f1\u0001\u0000\u0000\u0000\u02f4\u02f8\u0005\r\u0000"+
		"\u0000\u02f5\u02f6\u0005L\u0000\u0000\u02f6\u02f8\u0007\u0001\u0000\u0000"+
		"\u02f7\u02f4\u0001\u0000\u0000\u0000\u02f7\u02f5\u0001\u0000\u0000\u0000"+
		"\u02f8{\u0001\u0000\u0000\u0000\u02f9\u02fa\u0005O\u0000\u0000\u02fa\u02fb"+
		"\u0003V+\u0000\u02fb}\u0001\u0000\u0000\u0000\u02fc\u02fd\u0003\u0080"+
		"@\u0000\u02fd\u02ff\u0003\u008cF\u0000\u02fe\u0300\u0003n7\u0000\u02ff"+
		"\u02fe\u0001\u0000\u0000\u0000\u02ff\u0300\u0001\u0000\u0000\u0000\u0300"+
		"\u0304\u0001\u0000\u0000\u0000\u0301\u0304\u0003\u0084B\u0000\u0302\u0304"+
		"\u0005H\u0000\u0000\u0303\u02fc\u0001\u0000\u0000\u0000\u0303\u0301\u0001"+
		"\u0000\u0000\u0000\u0303\u0302\u0001\u0000\u0000\u0000\u0304\u007f\u0001"+
		"\u0000\u0000\u0000\u0305\u0306\u0007\u0002\u0000\u0000\u0306\u0081\u0001"+
		"\u0000\u0000\u0000\u0307\u0308\u00057\u0000\u0000\u0308\u0309\u0005Y\u0000"+
		"\u0000\u0309\u0083\u0001\u0000\u0000\u0000\u030a\u030b\u0005G\u0000\u0000"+
		"\u030b\u030c\u0005X\u0000\u0000\u030c\u0085\u0001\u0000\u0000\u0000\u030d"+
		"\u0311\u0005\n\u0000\u0000\u030e\u0311\u0005\u000b\u0000\u0000\u030f\u0311"+
		"\u0003p8\u0000\u0310\u030d\u0001\u0000\u0000\u0000\u0310\u030e\u0001\u0000"+
		"\u0000\u0000\u0310\u030f\u0001\u0000\u0000\u0000\u0311\u0087\u0001\u0000"+
		"\u0000\u0000\u0312\u0315\u0005\n\u0000\u0000\u0313\u0315\u0003p8\u0000"+
		"\u0314\u0312\u0001\u0000\u0000\u0000\u0314\u0313\u0001\u0000\u0000\u0000"+
		"\u0315\u0089\u0001\u0000\u0000\u0000\u0316\u0317\u0005\n\u0000\u0000\u0317"+
		"\u008b\u0001\u0000\u0000\u0000\u0318\u0319\u0007\u0003\u0000\u0000\u0319"+
		"\u008d\u0001\u0000\u0000\u0000\u031a\u031b\u0007\u0004\u0000\u0000\u031b"+
		"\u008f\u0001\u0000\u0000\u0000\u031c\u031d\u0007\u0000\u0000\u0000\u031d"+
		"\u0091\u0001\u0000\u0000\u0000\u031e\u031f\u0007\u0005\u0000\u0000\u031f"+
		"\u0093\u0001\u0000\u0000\u0000\u0320\u0321\u0005*\u0000\u0000\u0321\u0095"+
		"\u0001\u0000\u0000\u0000\u0322\u0323\u0007\u0006\u0000\u0000\u0323\u0097"+
		"\u0001\u0000\u0000\u0000\u0324\u0327\u0003\u009aM\u0000\u0325\u0327\u0005"+
		"\u001b\u0000\u0000\u0326\u0324\u0001\u0000\u0000\u0000\u0326\u0325\u0001"+
		"\u0000\u0000\u0000\u0327\u0099\u0001\u0000\u0000\u0000\u0328\u0329\u0007"+
		"\u0007\u0000\u0000\u0329\u009b\u0001\u0000\u0000\u0000\u032a\u032b\u0005"+
		"\u0003\u0000\u0000\u032b\u032c\u0007\b\u0000\u0000\u032c\u009d\u0001\u0000"+
		"\u0000\u0000g\u00a1\u00a5\u00ab\u00b0\u00b9\u00c0\u00c7\u00cd\u00d2\u00d8"+
		"\u00de\u00e4\u00e7\u00f0\u0100\u0102\u010a\u0113\u0119\u011e\u0124\u012a"+
		"\u0130\u0132\u0136\u0139\u013f\u0149\u014d\u0151\u015b\u015f\u0163\u0170"+
		"\u0174\u017b\u017e\u0182\u0186\u018e\u0190\u0199\u01a4\u01a9\u01ae\u01b6"+
		"\u01bd\u01c6\u01d3\u01d5\u01d9\u01e2\u01ec\u01f2\u01f4\u01f8\u0201\u0204"+
		"\u0209\u020c\u0212\u0214\u021d\u0222\u022a\u022d\u0231\u0237\u0241\u0243"+
		"\u0249\u024d\u0256\u025b\u0261\u026e\u0274\u0276\u027d\u027f\u0289\u028d"+
		"\u0297\u029b\u029f\u02a9\u02ad\u02b1\u02b6\u02c3\u02c7\u02d1\u02d3\u02dc"+
		"\u02de\u02e9\u02f1\u02f7\u02ff\u0303\u0310\u0314\u0326";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}