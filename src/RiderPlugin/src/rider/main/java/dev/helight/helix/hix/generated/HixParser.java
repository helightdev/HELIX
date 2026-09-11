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
		BOOLEAN=22, NULL=23, MISSING=24, QUESTION=25, PIPE=26, ESCAPED_AT=27,
		ESCAPED_START=28, OUTER_WHITESPACE=29, NEWLINE=30, COMMENT=31, SLASH_COMMENT=32,
		SECTION_DELIMITER=33, LC=34, RC=35, SEMICOLON=36, COMMA=37, KEYWORD_FUNC=38,
		KEYWORD_DELEGATE=39, KEYWORD_TYPE=40, KEYWORD_DO=41, KEYWORD_EXPRESSION=42,
		KEYWORD_MIXIN=43, KEYWORD_PRELUDE=44, KEYWORD_DERIVATION=45, KEYWORD_ELSE=46,
		KEYWORD_RETURN=47, KEYWORD_GOTO=48, KEYWORD_BREAK=49, KEYWORD_CONTINUE=50,
		KEYWORD_LOCAL=51, KEYWORD_CARRY=52, KEYWORD_WHEN=53, KEYWORD_PURE=54,
		KEYWORD_INLINE=55, KEYWORD_NOINLINE=56, KEYWORD_STRICT=57, LABEL_PREFIX=58,
		TOPLEVEL_DERIVATION=59, ASSIGN=60, ARROW=61, TOPLEVEL_VALUE_EXPRESSION=62,
		EMPTY_PARAMETERS=63, BEGIN_PARAMETERS=64, METADATA_WHITESPACE=65, DERIVATION_WHITESPACE=66,
		ARGUMENT_TEXT=67, ARGUMENT_END=68, CONTENT_WRAP=69, CONTENT_LINEBREAK=70,
		CONTENT_TEXT=71, VALUE_FUNCTION=72, VALUE_PREDICATE=73, VALUE_MEMBER=74,
		VALUE_WRAP=75, VALUE_DELIMITER=76, END_PARAMETERS=77, VALUE_END_INLINE=78,
		VALUE_SMART_ROOT=79, NOT_VALUE=80, VALUE_ELVIS=81, VALUE_ASSIGN=82, VALUE_EXPAND=83,
		VALUE_WHITESPACE=84, ESCAPE_MACRO=85, ESCAPE_LITERAL=86, ESCAPE_HEX=87,
		END_CONTENT_INTERPOLATE=88, FUNCTION_IDENTIFIER=89, MEMBER_IDENTIFIER=90,
		LABEL_IDENTIFIER=91, TOPLEVEL_QUESTION=92, TOPLEVEL_PIPE=93, TOPLEVEL_SMART_ROOT=94,
		TOPLEVEL_NULL=95, VALUE_MISSING=96;
	public static final int
		RULE_compilationUnit = 0, RULE_fileMetadataSection = 1, RULE_topLevelDeclaration = 2,
		RULE_metadataList = 3, RULE_inlineMetadataList = 4, RULE_metadata = 5,
		RULE_metadataValue = 6, RULE_mixinDeclaration = 7, RULE_typeDeclaration = 8,
		RULE_mixinBody = 9, RULE_expressionDeclaration = 10, RULE_funcDeclaration = 11,
		RULE_directFunctionSignature = 12, RULE_functionBody = 13, RULE_patternExpression = 14,
		RULE_patternUnion = 15, RULE_patternTerm = 16, RULE_patternPrimary = 17,
		RULE_tablePattern = 18, RULE_tuplePattern = 19, RULE_delegatePattern = 20,
		RULE_patternParameterList = 21, RULE_patternField = 22, RULE_patternIdentifier = 23,
		RULE_statementBlock = 24, RULE_statement = 25, RULE_statementBody = 26,
		RULE_invocationStatement = 27, RULE_whenConditionStatement = 28, RULE_whenElseBranch = 29,
		RULE_whenResult = 30, RULE_whenChainCondition = 31, RULE_whenChainStatement = 32,
		RULE_whenChainBody = 33, RULE_whenChainBranch = 34, RULE_whenValueStatement = 35,
		RULE_whenValueBody = 36, RULE_whenValueBranch = 37, RULE_whenValueCondition = 38,
		RULE_localDeclarationStatement = 39, RULE_localAssignmentStatement = 40,
		RULE_toplevelDerivationStatement = 41, RULE_assignedValue = 42, RULE_controlflowStatement = 43,
		RULE_contentBlock = 44, RULE_contentBody = 45, RULE_contentInterpolate = 46,
		RULE_value = 47, RULE_valueExpression = 48, RULE_nonArgumentValue = 49,
		RULE_primaryValue = 50, RULE_lambdaValue = 51, RULE_prefixOperators = 52,
		RULE_postfixOperators = 53, RULE_valueStatement = 54, RULE_tailValue = 55,
		RULE_tupleValue = 56, RULE_tableValue = 57, RULE_tableKeyedEntry = 58,
		RULE_valueList = 59, RULE_callValueList = 60, RULE_callArgument = 61,
		RULE_argumentValue = 62, RULE_argumentBody = 63, RULE_inlineValue = 64,
		RULE_inlineTransformation = 65, RULE_derivation = 66, RULE_derivationRoot = 67,
		RULE_elvisValue = 68, RULE_transformationPart = 69, RULE_functionChainType = 70,
		RULE_labelIdentifier = 71, RULE_memberIdentifier = 72, RULE_mixinIdentifier = 73,
		RULE_functionDeclarationIdentifier = 74, RULE_variableIdentifier = 75,
		RULE_functionIdentifier = 76, RULE_invocationIdentifier = 77, RULE_kindIdentifier = 78,
		RULE_expressionModifier = 79, RULE_mixinModifier = 80, RULE_funcModifier = 81,
		RULE_trivia = 82, RULE_comment = 83, RULE_escaped = 84;
	private static String[] makeRuleNames() {
		return new String[] {
			"compilationUnit", "fileMetadataSection", "topLevelDeclaration", "metadataList",
			"inlineMetadataList", "metadata", "metadataValue", "mixinDeclaration",
			"typeDeclaration", "mixinBody", "expressionDeclaration", "funcDeclaration",
			"directFunctionSignature", "functionBody", "patternExpression", "patternUnion",
			"patternTerm", "patternPrimary", "tablePattern", "tuplePattern", "delegatePattern",
			"patternParameterList", "patternField", "patternIdentifier", "statementBlock",
			"statement", "statementBody", "invocationStatement", "whenConditionStatement",
			"whenElseBranch", "whenResult", "whenChainCondition", "whenChainStatement",
			"whenChainBody", "whenChainBranch", "whenValueStatement", "whenValueBody",
			"whenValueBranch", "whenValueCondition", "localDeclarationStatement",
			"localAssignmentStatement", "toplevelDerivationStatement", "assignedValue",
			"controlflowStatement", "contentBlock", "contentBody", "contentInterpolate",
			"value", "valueExpression", "nonArgumentValue", "primaryValue", "lambdaValue",
			"prefixOperators", "postfixOperators", "valueStatement", "tailValue",
			"tupleValue", "tableValue", "tableKeyedEntry", "valueList", "callValueList",
			"callArgument", "argumentValue", "argumentBody", "inlineValue", "inlineTransformation",
			"derivation", "derivationRoot", "elvisValue", "transformationPart", "functionChainType",
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
			null, null, null, "'@@'", null, null, null, null, null, "'---'", "'{'",
			null, null, null, "'func'", null, "'type'", "'do'", "'expression'", "'mixin'",
			"'prelude'", "'derivation'", "'else'", "'return'", "'goto'", "'break'",
			"'continue'", "'local'", "'carry'", "'when'", "'pure'", "'inline'", "'noinline'",
			"'strict'", null, null, null, "'->'", null, null, "'('", null, null,
			null, "'>'", null, null, null, null, "':?'", "'#'", null, null, "')'",
			"']'", null, "'!'", "'?:'", null, "'...'", null, null, null, null, null,
			null, null, null, null, null, null, null, "'missing'"
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
			"NULL", "MISSING", "QUESTION", "PIPE", "ESCAPED_AT", "ESCAPED_START",
			"OUTER_WHITESPACE", "NEWLINE", "COMMENT", "SLASH_COMMENT", "SECTION_DELIMITER",
			"LC", "RC", "SEMICOLON", "COMMA", "KEYWORD_FUNC", "KEYWORD_DELEGATE",
			"KEYWORD_TYPE", "KEYWORD_DO", "KEYWORD_EXPRESSION", "KEYWORD_MIXIN",
			"KEYWORD_PRELUDE", "KEYWORD_DERIVATION", "KEYWORD_ELSE", "KEYWORD_RETURN",
			"KEYWORD_GOTO", "KEYWORD_BREAK", "KEYWORD_CONTINUE", "KEYWORD_LOCAL",
			"KEYWORD_CARRY", "KEYWORD_WHEN", "KEYWORD_PURE", "KEYWORD_INLINE", "KEYWORD_NOINLINE",
			"KEYWORD_STRICT", "LABEL_PREFIX", "TOPLEVEL_DERIVATION", "ASSIGN", "ARROW",
			"TOPLEVEL_VALUE_EXPRESSION", "EMPTY_PARAMETERS", "BEGIN_PARAMETERS",
			"METADATA_WHITESPACE", "DERIVATION_WHITESPACE", "ARGUMENT_TEXT", "ARGUMENT_END",
			"CONTENT_WRAP", "CONTENT_LINEBREAK", "CONTENT_TEXT", "VALUE_FUNCTION",
			"VALUE_PREDICATE", "VALUE_MEMBER", "VALUE_WRAP", "VALUE_DELIMITER", "END_PARAMETERS",
			"VALUE_END_INLINE", "VALUE_SMART_ROOT", "NOT_VALUE", "VALUE_ELVIS", "VALUE_ASSIGN",
			"VALUE_EXPAND", "VALUE_WHITESPACE", "ESCAPE_MACRO", "ESCAPE_LITERAL",
			"ESCAPE_HEX", "END_CONTENT_INTERPOLATE", "FUNCTION_IDENTIFIER", "MEMBER_IDENTIFIER",
			"LABEL_IDENTIFIER", "TOPLEVEL_QUESTION", "TOPLEVEL_PIPE", "TOPLEVEL_SMART_ROOT",
			"TOPLEVEL_NULL", "VALUE_MISSING"
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
			setState(173);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 7516192768L) != 0)) {
				{
				{
				setState(170);
				trivia();
				}
				}
				setState(175);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(177);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,1,_ctx) ) {
			case 1:
				{
				setState(176);
				fileMetadataSection();
				}
				break;
			}
			setState(188);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 126146144421806080L) != 0)) {
				{
				{
				setState(179);
				topLevelDeclaration();
				setState(183);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 7516192768L) != 0)) {
					{
					{
					setState(180);
					trivia();
					}
					}
					setState(185);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
				}
				setState(190);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(191);
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
			setState(193);
			metadataList();
			setState(197);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 7516192768L) != 0)) {
				{
				{
				setState(194);
				trivia();
				}
				}
				setState(199);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(200);
			match(SECTION_DELIMITER);
			setState(204);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 7516192768L) != 0)) {
				{
				{
				setState(201);
				trivia();
				}
				}
				setState(206);
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
			setState(222);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(207);
				metadataList();
				setState(211);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 7516192768L) != 0)) {
					{
					{
					setState(208);
					trivia();
					}
					}
					setState(213);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(217);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_MIXIN:
				case KEYWORD_DERIVATION:
					{
					setState(214);
					mixinDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(215);
					funcDeclaration();
					}
					break;
				case KEYWORD_TYPE:
					{
					setState(216);
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
				setState(219);
				mixinDeclaration();
				}
				break;
			case KEYWORD_FUNC:
			case KEYWORD_PURE:
			case KEYWORD_INLINE:
			case KEYWORD_NOINLINE:
				enterOuterAlt(_localctx, 3);
				{
				setState(220);
				funcDeclaration();
				}
				break;
			case KEYWORD_TYPE:
				enterOuterAlt(_localctx, 4);
				{
				setState(221);
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
			setState(224);
			metadata();
			setState(234);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,10,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(228);
					_errHandler.sync(this);
					_la = _input.LA(1);
					while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 7516192768L) != 0)) {
						{
						{
						setState(225);
						trivia();
						}
						}
						setState(230);
						_errHandler.sync(this);
						_la = _input.LA(1);
					}
					setState(231);
					metadata();
					}
					}
				}
				setState(236);
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
	public static class InlineMetadataListContext extends ParserRuleContext {
		public List<MetadataContext> metadata() {
			return getRuleContexts(MetadataContext.class);
		}
		public MetadataContext metadata(int i) {
			return getRuleContext(MetadataContext.class,i);
		}
		public InlineMetadataListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_inlineMetadataList; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitInlineMetadataList(this);
			else return visitor.visitChildren(this);
		}
	}

	public final InlineMetadataListContext inlineMetadataList() throws RecognitionException {
		InlineMetadataListContext _localctx = new InlineMetadataListContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_inlineMetadataList);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(238);
			_errHandler.sync(this);
			_alt = 1;
			do {
				switch (_alt) {
				case 1:
					{
					{
					setState(237);
					metadata();
					}
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				setState(240);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,11,_ctx);
			} while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER );
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
		enterRule(_localctx, 10, RULE_metadata);
		int _la;
		try {
			setState(248);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(242);
				match(METADATA_PREFIX);
				setState(243);
				match(IDENTIFIER);
				setState(245);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (((((_la - 4)) & ~0x3f) == 0 && ((1L << (_la - 4)) & 1729382256910270465L) != 0)) {
					{
					setState(244);
					valueList();
					}
				}

				}
				break;
			case BEGIN_METADATA_VALUE:
				enterOuterAlt(_localctx, 2);
				{
				setState(247);
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
		enterRule(_localctx, 12, RULE_metadataValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(250);
			match(BEGIN_METADATA_VALUE);
			setState(251);
			value();
			setState(252);
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
		public PatternParameterListContext patternParameterList() {
			return getRuleContext(PatternParameterListContext.class,0);
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
		enterRule(_localctx, 14, RULE_mixinDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(257);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_DERIVATION) {
				{
				{
				setState(254);
				mixinModifier();
				}
				}
				setState(259);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(260);
			match(KEYWORD_MIXIN);
			setState(261);
			mixinIdentifier();
			setState(263);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(262);
				patternParameterList();
				}
			}

			setState(265);
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
		public TerminalNode SEMICOLON() { return getToken(HixParser.SEMICOLON, 0); }
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
		enterRule(_localctx, 16, RULE_typeDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(267);
			match(KEYWORD_TYPE);
			setState(268);
			match(IDENTIFIER);
			setState(269);
			match(ASSIGN);
			setState(270);
			patternExpression();
			setState(272);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==SEMICOLON) {
				{
				setState(271);
				match(SEMICOLON);
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
		enterRule(_localctx, 18, RULE_mixinBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(274);
			match(LC);
			setState(280);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 270238250268884992L) != 0)) {
				{
				setState(278);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_EXPRESSION:
				case KEYWORD_PRELUDE:
				case KEYWORD_STRICT:
					{
					setState(275);
					expressionDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(276);
					funcDeclaration();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(277);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(282);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(283);
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
		enterRule(_localctx, 20, RULE_expressionDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(288);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_PRELUDE || _la==KEYWORD_STRICT) {
				{
				{
				setState(285);
				expressionModifier();
				}
				}
				setState(290);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(291);
			match(KEYWORD_EXPRESSION);
			setState(292);
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
		enterRule(_localctx, 22, RULE_funcDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(297);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 126100789566373888L) != 0)) {
				{
				{
				setState(294);
				funcModifier();
				}
				}
				setState(299);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(300);
			match(KEYWORD_FUNC);
			setState(301);
			functionDeclarationIdentifier();
			setState(303);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (((((_la - 61)) & ~0x3f) == 0 && ((1L << (_la - 61)) & 13L) != 0)) {
				{
				setState(302);
				directFunctionSignature();
				}
			}

			setState(308);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 7516192768L) != 0)) {
				{
				{
				setState(305);
				trivia();
				}
				}
				setState(310);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(311);
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
		enterRule(_localctx, 24, RULE_directFunctionSignature);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(314);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(313);
				patternParameterList();
				}
			}

			setState(316);
			match(ARROW);
			setState(317);
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
		enterRule(_localctx, 26, RULE_functionBody);
		int _la;
		try {
			setState(328);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case LC:
			case KEYWORD_DO:
				enterOuterAlt(_localctx, 1);
				{
				setState(320);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_DO) {
					{
					setState(319);
					match(KEYWORD_DO);
					}
				}

				setState(322);
				statementBlock();
				}
				break;
			case FAT_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(323);
				match(FAT_ARROW);
				setState(324);
				value();
				setState(326);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_END) {
					{
					setState(325);
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
		public InlineMetadataListContext inlineMetadataList() {
			return getRuleContext(InlineMetadataListContext.class,0);
		}
		public PatternUnionContext patternUnion() {
			return getRuleContext(PatternUnionContext.class,0);
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
		enterRule(_localctx, 28, RULE_patternExpression);
		try {
			setState(335);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(330);
				inlineMetadataList();
				setState(332);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,27,_ctx) ) {
				case 1:
					{
					setState(331);
					patternUnion();
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
			case MISSING:
			case KEYWORD_DELEGATE:
				enterOuterAlt(_localctx, 2);
				{
				setState(334);
				patternUnion();
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
	public static class PatternUnionContext extends ParserRuleContext {
		public List<PatternTermContext> patternTerm() {
			return getRuleContexts(PatternTermContext.class);
		}
		public PatternTermContext patternTerm(int i) {
			return getRuleContext(PatternTermContext.class,i);
		}
		public List<TerminalNode> PIPE() { return getTokens(HixParser.PIPE); }
		public TerminalNode PIPE(int i) {
			return getToken(HixParser.PIPE, i);
		}
		public PatternUnionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_patternUnion; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitPatternUnion(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PatternUnionContext patternUnion() throws RecognitionException {
		PatternUnionContext _localctx = new PatternUnionContext(_ctx, getState());
		enterRule(_localctx, 30, RULE_patternUnion);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(337);
			patternTerm();
			setState(342);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,29,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(338);
					match(PIPE);
					setState(339);
					patternTerm();
					}
					}
				}
				setState(344);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,29,_ctx);
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
	public static class PatternTermContext extends ParserRuleContext {
		public PatternPrimaryContext patternPrimary() {
			return getRuleContext(PatternPrimaryContext.class,0);
		}
		public TerminalNode QUESTION() { return getToken(HixParser.QUESTION, 0); }
		public PatternTermContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_patternTerm; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitPatternTerm(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PatternTermContext patternTerm() throws RecognitionException {
		PatternTermContext _localctx = new PatternTermContext(_ctx, getState());
		enterRule(_localctx, 32, RULE_patternTerm);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(345);
			patternPrimary();
			setState(347);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,30,_ctx) ) {
			case 1:
				{
				setState(346);
				match(QUESTION);
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
		enterRule(_localctx, 34, RULE_patternPrimary);
		try {
			setState(353);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case NULL:
			case MISSING:
				enterOuterAlt(_localctx, 1);
				{
				setState(349);
				patternIdentifier();
				}
				break;
			case BEGIN_TABLE:
				enterOuterAlt(_localctx, 2);
				{
				setState(350);
				tablePattern();
				}
				break;
			case BEGIN_TUPLE:
				enterOuterAlt(_localctx, 3);
				{
				setState(351);
				tuplePattern();
				}
				break;
			case KEYWORD_DELEGATE:
				enterOuterAlt(_localctx, 4);
				{
				setState(352);
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
		enterRule(_localctx, 36, RULE_tablePattern);
		int _la;
		try {
			int _alt;
			setState(371);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,34,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(355);
				match(BEGIN_TABLE);
				setState(356);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(357);
				match(BEGIN_TABLE);
				setState(358);
				patternField();
				setState(363);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,32,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(359);
						match(VALUE_DELIMITER);
						setState(360);
						patternField();
						}
						}
					}
					setState(365);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,32,_ctx);
				}
				setState(367);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(366);
					match(VALUE_DELIMITER);
					}
				}

				setState(369);
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
		enterRule(_localctx, 38, RULE_tuplePattern);
		int _la;
		try {
			int _alt;
			setState(389);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,37,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(373);
				match(BEGIN_TUPLE);
				setState(374);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(375);
				match(BEGIN_TUPLE);
				setState(376);
				patternField();
				setState(381);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,35,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(377);
						match(VALUE_DELIMITER);
						setState(378);
						patternField();
						}
						}
					}
					setState(383);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,35,_ctx);
				}
				setState(385);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(384);
					match(VALUE_DELIMITER);
					}
				}

				setState(387);
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
		enterRule(_localctx, 40, RULE_delegatePattern);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(391);
			match(KEYWORD_DELEGATE);
			setState(392);
			patternParameterList();
			setState(393);
			match(ARROW);
			setState(394);
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
		enterRule(_localctx, 42, RULE_patternParameterList);
		int _la;
		try {
			int _alt;
			setState(413);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,40,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(396);
				match(BEGIN_PARAMETERS);
				setState(397);
				patternField();
				setState(402);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,38,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(398);
						match(VALUE_DELIMITER);
						setState(399);
						patternField();
						}
						}
					}
					setState(404);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,38,_ctx);
				}
				setState(406);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(405);
					match(VALUE_DELIMITER);
					}
				}

				setState(408);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(410);
				match(BEGIN_PARAMETERS);
				setState(411);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(412);
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
		public PatternUnionContext patternUnion() {
			return getRuleContext(PatternUnionContext.class,0);
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
		enterRule(_localctx, 44, RULE_patternField);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(416);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				setState(415);
				metadataList();
				}
			}

			setState(418);
			patternUnion();
			setState(420);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ROOT_IDENTIFIER) {
				{
				setState(419);
				match(ROOT_IDENTIFIER);
				}
			}

			setState(424);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==VALUE_ASSIGN) {
				{
				setState(422);
				match(VALUE_ASSIGN);
				setState(423);
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
		public TerminalNode MISSING() { return getToken(HixParser.MISSING, 0); }
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
		enterRule(_localctx, 46, RULE_patternIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(426);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 25175040L) != 0)) ) {
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
		enterRule(_localctx, 48, RULE_statementBlock);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(428);
			match(LC);
			setState(434);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 882564882892588032L) != 0) || _la==VALUE_SMART_ROOT) {
				{
				setState(432);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case IDENTIFIER:
				case BEGIN_METADATA_VALUE:
				case METADATA_PREFIX:
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
					setState(429);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(430);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(431);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(436);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(437);
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
		public MetadataListContext metadataList() {
			return getRuleContext(MetadataListContext.class,0);
		}
		public StatementBodyContext statementBody() {
			return getRuleContext(StatementBodyContext.class,0);
		}
		public List<TriviaContext> trivia() {
			return getRuleContexts(TriviaContext.class);
		}
		public TriviaContext trivia(int i) {
			return getRuleContext(TriviaContext.class,i);
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
		enterRule(_localctx, 50, RULE_statement);
		int _la;
		try {
			setState(449);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(439);
				metadataList();
				setState(443);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 7516192768L) != 0)) {
					{
					{
					setState(440);
					trivia();
					}
					}
					setState(445);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(446);
				statementBody();
				}
				break;
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
				enterOuterAlt(_localctx, 2);
				{
				setState(448);
				statementBody();
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
	public static class StatementBodyContext extends ParserRuleContext {
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
		public StatementBodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_statementBody; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitStatementBody(this);
			else return visitor.visitChildren(this);
		}
	}

	public final StatementBodyContext statementBody() throws RecognitionException {
		StatementBodyContext _localctx = new StatementBodyContext(_ctx, getState());
		enterRule(_localctx, 52, RULE_statementBody);
		int _la;
		try {
			setState(466);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,49,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(451);
				labelIdentifier();
				setState(452);
				match(NEWLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(455);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LABEL_PREFIX) {
					{
					setState(454);
					labelIdentifier();
					}
				}

				setState(457);
				statementBlock();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(458);
				invocationStatement();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(459);
				localDeclarationStatement();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(460);
				localAssignmentStatement();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(461);
				toplevelDerivationStatement();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(462);
				controlflowStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(463);
				whenValueStatement();
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(464);
				whenConditionStatement();
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(465);
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
		public CallValueListContext callValueList() {
			return getRuleContext(CallValueListContext.class,0);
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
		enterRule(_localctx, 54, RULE_invocationStatement);
		try {
			setState(476);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,51,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(468);
				invocationIdentifier();
				setState(469);
				callValueList();
				setState(471);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,50,_ctx) ) {
				case 1:
					{
					setState(470);
					tailValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(473);
				invocationIdentifier();
				setState(474);
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
		enterRule(_localctx, 56, RULE_whenConditionStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(478);
			match(KEYWORD_WHEN);
			setState(479);
			whenChainCondition();
			setState(480);
			statementBlock();
			setState(491);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,53,_ctx) ) {
			case 1:
				{
				setState(484);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 7516192768L) != 0)) {
					{
					{
					setState(481);
					trivia();
					}
					}
					setState(486);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(487);
				match(KEYWORD_ELSE);
				setState(488);
				whenResult();
				setState(489);
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
		enterRule(_localctx, 58, RULE_whenElseBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(493);
			match(KEYWORD_ELSE);
			setState(494);
			match(ARROW);
			setState(495);
			whenResult();
			setState(496);
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
		enterRule(_localctx, 60, RULE_whenResult);
		try {
			setState(500);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,54,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(498);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(499);
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
		enterRule(_localctx, 62, RULE_whenChainCondition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(502);
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
		enterRule(_localctx, 64, RULE_whenChainStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(504);
			match(KEYWORD_WHEN);
			setState(505);
			match(LC);
			setState(506);
			match(NEWLINE);
			setState(507);
			whenChainBody();
			setState(508);
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
		enterRule(_localctx, 66, RULE_whenChainBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(510);
			whenChainBranch();
			setState(515);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 4)) & ~0x3f) == 0 && ((1L << (_la - 4)) & 1729382257380032513L) != 0)) {
				{
				setState(513);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case EMPTY_PARAMETERS:
				case BEGIN_PARAMETERS:
					{
					setState(511);
					whenChainBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(512);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(517);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(519);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(518);
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
		enterRule(_localctx, 68, RULE_whenChainBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(521);
			whenChainCondition();
			setState(522);
			match(ARROW);
			setState(523);
			whenResult();
			setState(524);
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
		enterRule(_localctx, 70, RULE_whenValueStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(526);
			match(KEYWORD_WHEN);
			setState(528);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 4611686018459099664L) != 0) || ((((_la - 79)) & ~0x3f) == 0 && ((1L << (_la - 79)) & 1027L) != 0)) {
				{
				setState(527);
				value();
				}
			}

			setState(530);
			match(LC);
			setState(531);
			match(NEWLINE);
			setState(532);
			whenValueBody();
			setState(533);
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
		enterRule(_localctx, 72, RULE_whenValueBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(538);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 7516192768L) != 0)) {
				{
				{
				setState(535);
				trivia();
				}
				}
				setState(540);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(541);
			whenValueBranch();
			setState(546);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 4611686025975292432L) != 0) || ((((_la - 79)) & ~0x3f) == 0 && ((1L << (_la - 79)) & 1027L) != 0)) {
				{
				setState(544);
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
				case MISSING:
				case TOPLEVEL_VALUE_EXPRESSION:
				case VALUE_SMART_ROOT:
				case NOT_VALUE:
				case FUNCTION_IDENTIFIER:
					{
					setState(542);
					whenValueBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(543);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(548);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(550);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(549);
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
		enterRule(_localctx, 74, RULE_whenValueBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(552);
			whenValueCondition();
			setState(553);
			match(ARROW);
			setState(554);
			whenResult();
			setState(555);
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
		enterRule(_localctx, 76, RULE_whenValueCondition);
		try {
			setState(559);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,63,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(557);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(558);
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
		enterRule(_localctx, 78, RULE_localDeclarationStatement);
		int _la;
		try {
			setState(578);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,68,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(562);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(561);
					match(KEYWORD_CARRY);
					}
				}

				setState(564);
				match(KEYWORD_LOCAL);
				setState(565);
				variableIdentifier();
				setState(567);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,65,_ctx) ) {
				case 1:
					{
					setState(566);
					assignedValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(570);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(569);
					match(KEYWORD_CARRY);
					}
				}

				setState(572);
				match(KEYWORD_LOCAL);
				setState(573);
				patternExpression();
				setState(574);
				variableIdentifier();
				setState(576);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,67,_ctx) ) {
				case 1:
					{
					setState(575);
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
		enterRule(_localctx, 80, RULE_localAssignmentStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(580);
			match(VALUE_SMART_ROOT);
			setState(581);
			variableIdentifier();
			setState(582);
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
		public CallValueListContext callValueList() {
			return getRuleContext(CallValueListContext.class,0);
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
		enterRule(_localctx, 82, RULE_toplevelDerivationStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(584);
			match(TOPLEVEL_DERIVATION);
			setState(585);
			functionIdentifier();
			setState(587);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (((((_la - 4)) & ~0x3f) == 0 && ((1L << (_la - 4)) & 1729382256910270465L) != 0)) {
				{
				setState(586);
				callValueList();
				}
			}

			setState(592);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 72)) & ~0x3f) == 0 && ((1L << (_la - 72)) & 15L) != 0)) {
				{
				{
				setState(589);
				transformationPart();
				}
				}
				setState(594);
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
		enterRule(_localctx, 84, RULE_assignedValue);
		try {
			setState(603);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ASSIGN:
				enterOuterAlt(_localctx, 1);
				{
				setState(595);
				match(ASSIGN);
				setState(600);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,71,_ctx) ) {
				case 1:
					{
					setState(596);
					value();
					}
					break;
				case 2:
					{
					setState(597);
					whenValueStatement();
					}
					break;
				case 3:
					{
					setState(598);
					whenChainStatement();
					}
					break;
				case 4:
					{
					setState(599);
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
			case MISSING:
			case NEWLINE:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(602);
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
		enterRule(_localctx, 86, RULE_controlflowStatement);
		int _la;
		try {
			setState(613);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_RETURN:
				enterOuterAlt(_localctx, 1);
				{
				setState(605);
				match(KEYWORD_RETURN);
				setState(607);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (((((_la - 4)) & ~0x3f) == 0 && ((1L << (_la - 4)) & 1729382256910270465L) != 0)) {
					{
					setState(606);
					valueList();
					}
				}

				}
				break;
			case KEYWORD_GOTO:
				enterOuterAlt(_localctx, 2);
				{
				setState(609);
				match(KEYWORD_GOTO);
				setState(610);
				match(IDENTIFIER);
				}
				break;
			case KEYWORD_CONTINUE:
				enterOuterAlt(_localctx, 3);
				{
				setState(611);
				match(KEYWORD_CONTINUE);
				}
				break;
			case KEYWORD_BREAK:
				enterOuterAlt(_localctx, 4);
				{
				setState(612);
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
		enterRule(_localctx, 88, RULE_contentBlock);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(615);
			match(BEGIN_CONTENT);
			setState(616);
			contentBody();
			setState(617);
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
		enterRule(_localctx, 90, RULE_contentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(623);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				setState(623);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case CONTENT_WRAP:
					{
					setState(619);
					match(CONTENT_WRAP);
					}
					break;
				case CONTENT_LINEBREAK:
					{
					setState(620);
					match(CONTENT_LINEBREAK);
					}
					break;
				case CONTENT_TEXT:
					{
					setState(621);
					match(CONTENT_TEXT);
					}
					break;
				case BEGIN_VALUE_INTERPOLATE:
					{
					setState(622);
					contentInterpolate();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(625);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 8)) & ~0x3f) == 0 && ((1L << (_la - 8)) & -2305843009213693951L) != 0) );
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
		enterRule(_localctx, 92, RULE_contentInterpolate);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(627);
			match(BEGIN_VALUE_INTERPOLATE);
			setState(628);
			derivation();
			setState(629);
			match(RC);
			setState(631);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==END_CONTENT_INTERPOLATE) {
				{
				setState(630);
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
		enterRule(_localctx, 94, RULE_value);
		try {
			setState(635);
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
			case MISSING:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(633);
				nonArgumentValue(0);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(634);
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
		enterRule(_localctx, 96, RULE_valueExpression);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(637);
			match(TOPLEVEL_VALUE_EXPRESSION);
			setState(638);
			value();
			setState(639);
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
		int _startState = 98;
		enterRecursionRule(_localctx, 98, RULE_nonArgumentValue, _p);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(649);
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
			case MISSING:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case FUNCTION_IDENTIFIER:
				{
				setState(642);
				primaryValue();
				setState(644);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,79,_ctx) ) {
				case 1:
					{
					setState(643);
					elvisValue();
					}
					break;
				}
				}
				break;
			case NOT_VALUE:
				{
				setState(646);
				prefixOperators();
				setState(647);
				nonArgumentValue(2);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(655);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,81,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					{
					_localctx = new NonArgumentValueContext(_parentctx, _parentState);
					pushNewRecursionContext(_localctx, _startState, RULE_nonArgumentValue);
					setState(651);
					if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
					setState(652);
					postfixOperators();
					}
					}
				}
				setState(657);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,81,_ctx);
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
		public TerminalNode MISSING() { return getToken(HixParser.MISSING, 0); }
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
		enterRule(_localctx, 100, RULE_primaryValue);
		try {
			setState(669);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,82,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(658);
				inlineValue();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(659);
				lambdaValue();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(660);
				derivation();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(661);
				tableValue();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(662);
				tupleValue();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(663);
				valueExpression();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(664);
				valueStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(665);
				match(NUMBER);
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(666);
				match(BOOLEAN);
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(667);
				match(NULL);
				}
				break;
			case 11:
				enterOuterAlt(_localctx, 11);
				{
				setState(668);
				match(MISSING);
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
		enterRule(_localctx, 102, RULE_lambdaValue);
		int _la;
		try {
			setState(686);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_LAMBDA_BLOCK:
				enterOuterAlt(_localctx, 1);
				{
				setState(671);
				match(BEGIN_LAMBDA_BLOCK);
				setState(677);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 882564882892588032L) != 0) || _la==VALUE_SMART_ROOT) {
					{
					setState(675);
					_errHandler.sync(this);
					switch (_input.LA(1)) {
					case IDENTIFIER:
					case BEGIN_METADATA_VALUE:
					case METADATA_PREFIX:
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
						setState(672);
						statement();
						}
						break;
					case SEMICOLON:
						{
						setState(673);
						match(SEMICOLON);
						}
						break;
					case NEWLINE:
					case COMMENT:
					case SLASH_COMMENT:
						{
						setState(674);
						trivia();
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					}
					setState(679);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(680);
				match(RC);
				}
				break;
			case BEGIN_LAMBDA_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(681);
				match(BEGIN_LAMBDA_ARROW);
				setState(682);
				value();
				setState(684);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,85,_ctx) ) {
				case 1:
					{
					setState(683);
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
		enterRule(_localctx, 104, RULE_prefixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(688);
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
		public TerminalNode QUESTION() { return getToken(HixParser.QUESTION, 0); }
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
		enterRule(_localctx, 106, RULE_postfixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(690);
			match(QUESTION);
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
		public CallValueListContext callValueList() {
			return getRuleContext(CallValueListContext.class,0);
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
		enterRule(_localctx, 108, RULE_valueStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(692);
			functionIdentifier();
			setState(693);
			callValueList();
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
		enterRule(_localctx, 110, RULE_tailValue);
		int _la;
		try {
			setState(700);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_CONTENT:
			case NEWLINE:
				enterOuterAlt(_localctx, 1);
				{
				setState(696);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==NEWLINE) {
					{
					setState(695);
					match(NEWLINE);
					}
				}

				setState(698);
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
			case MISSING:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(699);
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
		enterRule(_localctx, 112, RULE_tupleValue);
		int _la;
		try {
			int _alt;
			setState(718);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,91,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(702);
				match(BEGIN_TUPLE);
				setState(703);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(704);
				match(BEGIN_TUPLE);
				setState(705);
				value();
				setState(710);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,89,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(706);
						match(VALUE_DELIMITER);
						setState(707);
						value();
						}
						}
					}
					setState(712);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,89,_ctx);
				}
				setState(714);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(713);
					match(VALUE_DELIMITER);
					}
				}

				setState(716);
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
		enterRule(_localctx, 114, RULE_tableValue);
		int _la;
		try {
			int _alt;
			setState(736);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,94,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(720);
				match(BEGIN_TABLE);
				setState(721);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(722);
				match(BEGIN_TABLE);
				setState(723);
				tableKeyedEntry();
				setState(728);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,92,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(724);
						match(VALUE_DELIMITER);
						setState(725);
						tableKeyedEntry();
						}
						}
					}
					setState(730);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,92,_ctx);
				}
				setState(732);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(731);
					match(VALUE_DELIMITER);
					}
				}

				setState(734);
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
		public TerminalNode VALUE_ASSIGN() { return getToken(HixParser.VALUE_ASSIGN, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public TerminalNode ROOT_IDENTIFIER() { return getToken(HixParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode MISSING() { return getToken(HixParser.MISSING, 0); }
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
		enterRule(_localctx, 116, RULE_tableKeyedEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(741);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				{
				setState(738);
				metadata();
				}
				}
				setState(743);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(744);
			_la = _input.LA(1);
			if ( !(_la==ROOT_IDENTIFIER || _la==MISSING) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			setState(745);
			match(VALUE_ASSIGN);
			setState(746);
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
		enterRule(_localctx, 118, RULE_valueList);
		int _la;
		try {
			int _alt;
			setState(770);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,99,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(748);
				match(BEGIN_PARAMETERS);
				setState(749);
				value();
				setState(754);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,96,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(750);
						match(VALUE_DELIMITER);
						setState(751);
						value();
						}
						}
					}
					setState(756);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,96,_ctx);
				}
				setState(758);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(757);
					match(VALUE_DELIMITER);
					}
				}

				setState(760);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(762);
				match(BEGIN_PARAMETERS);
				setState(763);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(764);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(766);
				_errHandler.sync(this);
				_la = _input.LA(1);
				do {
					{
					{
					setState(765);
					argumentValue();
					}
					}
					setState(768);
					_errHandler.sync(this);
					_la = _input.LA(1);
				} while ( _la==BEGIN_ARGUMENT );
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
	public static class CallValueListContext extends ParserRuleContext {
		public TerminalNode BEGIN_PARAMETERS() { return getToken(HixParser.BEGIN_PARAMETERS, 0); }
		public List<CallArgumentContext> callArgument() {
			return getRuleContexts(CallArgumentContext.class);
		}
		public CallArgumentContext callArgument(int i) {
			return getRuleContext(CallArgumentContext.class,i);
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
		public CallValueListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_callValueList; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitCallValueList(this);
			else return visitor.visitChildren(this);
		}
	}

	public final CallValueListContext callValueList() throws RecognitionException {
		CallValueListContext _localctx = new CallValueListContext(_ctx, getState());
		enterRule(_localctx, 120, RULE_callValueList);
		int _la;
		try {
			int _alt;
			setState(794);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,103,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(772);
				match(BEGIN_PARAMETERS);
				setState(773);
				callArgument();
				setState(778);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,100,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(774);
						match(VALUE_DELIMITER);
						setState(775);
						callArgument();
						}
						}
					}
					setState(780);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,100,_ctx);
				}
				setState(782);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(781);
					match(VALUE_DELIMITER);
					}
				}

				setState(784);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(786);
				match(BEGIN_PARAMETERS);
				setState(787);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(788);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(790);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(789);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(792);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,102,_ctx);
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
	public static class CallArgumentContext extends ParserRuleContext {
		public TerminalNode VALUE_ASSIGN() { return getToken(HixParser.VALUE_ASSIGN, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public TerminalNode ROOT_IDENTIFIER() { return getToken(HixParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode MISSING() { return getToken(HixParser.MISSING, 0); }
		public CallArgumentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_callArgument; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitCallArgument(this);
			else return visitor.visitChildren(this);
		}
	}

	public final CallArgumentContext callArgument() throws RecognitionException {
		CallArgumentContext _localctx = new CallArgumentContext(_ctx, getState());
		enterRule(_localctx, 122, RULE_callArgument);
		int _la;
		try {
			setState(800);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,104,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(796);
				_la = _input.LA(1);
				if ( !(_la==ROOT_IDENTIFIER || _la==MISSING) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(797);
				match(VALUE_ASSIGN);
				setState(798);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(799);
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
		enterRule(_localctx, 124, RULE_argumentValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(802);
			match(BEGIN_ARGUMENT);
			setState(803);
			argumentBody();
			setState(804);
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
		enterRule(_localctx, 126, RULE_argumentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(811);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==ESCAPE || _la==BEGIN_VALUE_INLINE || _la==ARGUMENT_TEXT) {
				{
				setState(809);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case ARGUMENT_TEXT:
					{
					setState(806);
					match(ARGUMENT_TEXT);
					}
					break;
				case ESCAPE:
					{
					setState(807);
					escaped();
					}
					break;
				case BEGIN_VALUE_INLINE:
					{
					setState(808);
					inlineValue();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(813);
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
		enterRule(_localctx, 128, RULE_inlineValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(814);
			match(BEGIN_VALUE_INLINE);
			setState(815);
			value();
			setState(816);
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
		enterRule(_localctx, 130, RULE_inlineTransformation);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(818);
			match(BEGIN_VALUE_INLINE);
			setState(820);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(819);
				transformationPart();
				}
				}
				setState(822);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 72)) & ~0x3f) == 0 && ((1L << (_la - 72)) & 15L) != 0) );
			setState(824);
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
		enterRule(_localctx, 132, RULE_derivation);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(826);
			derivationRoot();
			setState(830);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,108,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(827);
					transformationPart();
					}
					}
				}
				setState(832);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,108,_ctx);
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
		public TerminalNode MISSING() { return getToken(HixParser.MISSING, 0); }
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
		enterRule(_localctx, 134, RULE_derivationRoot);
		int _la;
		try {
			setState(836);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ROOT_IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(833);
				match(ROOT_IDENTIFIER);
				}
				break;
			case VALUE_SMART_ROOT:
				enterOuterAlt(_localctx, 2);
				{
				setState(834);
				match(VALUE_SMART_ROOT);
				setState(835);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 18882560L) != 0)) ) {
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
		enterRule(_localctx, 136, RULE_elvisValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(838);
			match(VALUE_ELVIS);
			setState(839);
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
		public CallValueListContext callValueList() {
			return getRuleContext(CallValueListContext.class,0);
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
		enterRule(_localctx, 138, RULE_transformationPart);
		try {
			setState(848);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case VALUE_FUNCTION:
			case VALUE_PREDICATE:
				enterOuterAlt(_localctx, 1);
				{
				setState(841);
				functionChainType();
				setState(842);
				functionIdentifier();
				setState(844);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,110,_ctx) ) {
				case 1:
					{
					setState(843);
					callValueList();
					}
					break;
				}
				}
				break;
			case VALUE_MEMBER:
				enterOuterAlt(_localctx, 2);
				{
				setState(846);
				memberIdentifier();
				}
				break;
			case VALUE_WRAP:
				enterOuterAlt(_localctx, 3);
				{
				setState(847);
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
		enterRule(_localctx, 140, RULE_functionChainType);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(850);
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
		enterRule(_localctx, 142, RULE_labelIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(852);
			match(LABEL_PREFIX);
			setState(853);
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
		enterRule(_localctx, 144, RULE_memberIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(855);
			match(VALUE_MEMBER);
			setState(856);
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
		enterRule(_localctx, 146, RULE_mixinIdentifier);
		try {
			setState(861);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(858);
				match(IDENTIFIER);
				}
				break;
			case NAMESPACE_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(859);
				match(NAMESPACE_IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 3);
				{
				setState(860);
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
		enterRule(_localctx, 148, RULE_functionDeclarationIdentifier);
		try {
			setState(865);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(863);
				match(IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(864);
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
		enterRule(_localctx, 150, RULE_variableIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(867);
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
		enterRule(_localctx, 152, RULE_functionIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(869);
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
		enterRule(_localctx, 154, RULE_invocationIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(871);
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
		public TerminalNode MISSING() { return getToken(HixParser.MISSING, 0); }
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
		enterRule(_localctx, 156, RULE_kindIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(873);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 25175040L) != 0)) ) {
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
		enterRule(_localctx, 158, RULE_expressionModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(875);
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
		enterRule(_localctx, 160, RULE_mixinModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(877);
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
		enterRule(_localctx, 162, RULE_funcModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(879);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 126100789566373888L) != 0)) ) {
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
		enterRule(_localctx, 164, RULE_trivia);
		try {
			setState(883);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case COMMENT:
			case SLASH_COMMENT:
				enterOuterAlt(_localctx, 1);
				{
				setState(881);
				comment();
				}
				break;
			case NEWLINE:
				enterOuterAlt(_localctx, 2);
				{
				setState(882);
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
		enterRule(_localctx, 166, RULE_comment);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(885);
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
		enterRule(_localctx, 168, RULE_escaped);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(887);
			match(ESCAPE);
			setState(888);
			_la = _input.LA(1);
			if ( !(((((_la - 85)) & ~0x3f) == 0 && ((1L << (_la - 85)) & 7L) != 0)) ) {
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
		case 49:
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
		"\u0004\u0001`\u037b\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
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
		"K\u0007K\u0002L\u0007L\u0002M\u0007M\u0002N\u0007N\u0002O\u0007O\u0002"+
		"P\u0007P\u0002Q\u0007Q\u0002R\u0007R\u0002S\u0007S\u0002T\u0007T\u0001"+
		"\u0000\u0005\u0000\u00ac\b\u0000\n\u0000\f\u0000\u00af\t\u0000\u0001\u0000"+
		"\u0003\u0000\u00b2\b\u0000\u0001\u0000\u0001\u0000\u0005\u0000\u00b6\b"+
		"\u0000\n\u0000\f\u0000\u00b9\t\u0000\u0005\u0000\u00bb\b\u0000\n\u0000"+
		"\f\u0000\u00be\t\u0000\u0001\u0000\u0001\u0000\u0001\u0001\u0001\u0001"+
		"\u0005\u0001\u00c4\b\u0001\n\u0001\f\u0001\u00c7\t\u0001\u0001\u0001\u0001"+
		"\u0001\u0005\u0001\u00cb\b\u0001\n\u0001\f\u0001\u00ce\t\u0001\u0001\u0002"+
		"\u0001\u0002\u0005\u0002\u00d2\b\u0002\n\u0002\f\u0002\u00d5\t\u0002\u0001"+
		"\u0002\u0001\u0002\u0001\u0002\u0003\u0002\u00da\b\u0002\u0001\u0002\u0001"+
		"\u0002\u0001\u0002\u0003\u0002\u00df\b\u0002\u0001\u0003\u0001\u0003\u0005"+
		"\u0003\u00e3\b\u0003\n\u0003\f\u0003\u00e6\t\u0003\u0001\u0003\u0005\u0003"+
		"\u00e9\b\u0003\n\u0003\f\u0003\u00ec\t\u0003\u0001\u0004\u0004\u0004\u00ef"+
		"\b\u0004\u000b\u0004\f\u0004\u00f0\u0001\u0005\u0001\u0005\u0001\u0005"+
		"\u0003\u0005\u00f6\b\u0005\u0001\u0005\u0003\u0005\u00f9\b\u0005\u0001"+
		"\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0007\u0005\u0007\u0100"+
		"\b\u0007\n\u0007\f\u0007\u0103\t\u0007\u0001\u0007\u0001\u0007\u0001\u0007"+
		"\u0003\u0007\u0108\b\u0007\u0001\u0007\u0001\u0007\u0001\b\u0001\b\u0001"+
		"\b\u0001\b\u0001\b\u0003\b\u0111\b\b\u0001\t\u0001\t\u0001\t\u0001\t\u0005"+
		"\t\u0117\b\t\n\t\f\t\u011a\t\t\u0001\t\u0001\t\u0001\n\u0005\n\u011f\b"+
		"\n\n\n\f\n\u0122\t\n\u0001\n\u0001\n\u0001\n\u0001\u000b\u0005\u000b\u0128"+
		"\b\u000b\n\u000b\f\u000b\u012b\t\u000b\u0001\u000b\u0001\u000b\u0001\u000b"+
		"\u0003\u000b\u0130\b\u000b\u0001\u000b\u0005\u000b\u0133\b\u000b\n\u000b"+
		"\f\u000b\u0136\t\u000b\u0001\u000b\u0001\u000b\u0001\f\u0003\f\u013b\b"+
		"\f\u0001\f\u0001\f\u0001\f\u0001\r\u0003\r\u0141\b\r\u0001\r\u0001\r\u0001"+
		"\r\u0001\r\u0003\r\u0147\b\r\u0003\r\u0149\b\r\u0001\u000e\u0001\u000e"+
		"\u0003\u000e\u014d\b\u000e\u0001\u000e\u0003\u000e\u0150\b\u000e\u0001"+
		"\u000f\u0001\u000f\u0001\u000f\u0005\u000f\u0155\b\u000f\n\u000f\f\u000f"+
		"\u0158\t\u000f\u0001\u0010\u0001\u0010\u0003\u0010\u015c\b\u0010\u0001"+
		"\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0003\u0011\u0162\b\u0011\u0001"+
		"\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0005"+
		"\u0012\u016a\b\u0012\n\u0012\f\u0012\u016d\t\u0012\u0001\u0012\u0003\u0012"+
		"\u0170\b\u0012\u0001\u0012\u0001\u0012\u0003\u0012\u0174\b\u0012\u0001"+
		"\u0013\u0001\u0013\u0001\u0013\u0001\u0013\u0001\u0013\u0001\u0013\u0005"+
		"\u0013\u017c\b\u0013\n\u0013\f\u0013\u017f\t\u0013\u0001\u0013\u0003\u0013"+
		"\u0182\b\u0013\u0001\u0013\u0001\u0013\u0003\u0013\u0186\b\u0013\u0001"+
		"\u0014\u0001\u0014\u0001\u0014\u0001\u0014\u0001\u0014\u0001\u0015\u0001"+
		"\u0015\u0001\u0015\u0001\u0015\u0005\u0015\u0191\b\u0015\n\u0015\f\u0015"+
		"\u0194\t\u0015\u0001\u0015\u0003\u0015\u0197\b\u0015\u0001\u0015\u0001"+
		"\u0015\u0001\u0015\u0001\u0015\u0001\u0015\u0003\u0015\u019e\b\u0015\u0001"+
		"\u0016\u0003\u0016\u01a1\b\u0016\u0001\u0016\u0001\u0016\u0003\u0016\u01a5"+
		"\b\u0016\u0001\u0016\u0001\u0016\u0003\u0016\u01a9\b\u0016\u0001\u0017"+
		"\u0001\u0017\u0001\u0018\u0001\u0018\u0001\u0018\u0001\u0018\u0005\u0018"+
		"\u01b1\b\u0018\n\u0018\f\u0018\u01b4\t\u0018\u0001\u0018\u0001\u0018\u0001"+
		"\u0019\u0001\u0019\u0005\u0019\u01ba\b\u0019\n\u0019\f\u0019\u01bd\t\u0019"+
		"\u0001\u0019\u0001\u0019\u0001\u0019\u0003\u0019\u01c2\b\u0019\u0001\u001a"+
		"\u0001\u001a\u0001\u001a\u0001\u001a\u0003\u001a\u01c8\b\u001a\u0001\u001a"+
		"\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a"+
		"\u0001\u001a\u0001\u001a\u0003\u001a\u01d3\b\u001a\u0001\u001b\u0001\u001b"+
		"\u0001\u001b\u0003\u001b\u01d8\b\u001b\u0001\u001b\u0001\u001b\u0001\u001b"+
		"\u0003\u001b\u01dd\b\u001b\u0001\u001c\u0001\u001c\u0001\u001c\u0001\u001c"+
		"\u0005\u001c\u01e3\b\u001c\n\u001c\f\u001c\u01e6\t\u001c\u0001\u001c\u0001"+
		"\u001c\u0001\u001c\u0001\u001c\u0003\u001c\u01ec\b\u001c\u0001\u001d\u0001"+
		"\u001d\u0001\u001d\u0001\u001d\u0001\u001d\u0001\u001e\u0001\u001e\u0003"+
		"\u001e\u01f5\b\u001e\u0001\u001f\u0001\u001f\u0001 \u0001 \u0001 \u0001"+
		" \u0001 \u0001 \u0001!\u0001!\u0001!\u0005!\u0202\b!\n!\f!\u0205\t!\u0001"+
		"!\u0003!\u0208\b!\u0001\"\u0001\"\u0001\"\u0001\"\u0001\"\u0001#\u0001"+
		"#\u0003#\u0211\b#\u0001#\u0001#\u0001#\u0001#\u0001#\u0001$\u0005$\u0219"+
		"\b$\n$\f$\u021c\t$\u0001$\u0001$\u0001$\u0005$\u0221\b$\n$\f$\u0224\t"+
		"$\u0001$\u0003$\u0227\b$\u0001%\u0001%\u0001%\u0001%\u0001%\u0001&\u0001"+
		"&\u0003&\u0230\b&\u0001\'\u0003\'\u0233\b\'\u0001\'\u0001\'\u0001\'\u0003"+
		"\'\u0238\b\'\u0001\'\u0003\'\u023b\b\'\u0001\'\u0001\'\u0001\'\u0001\'"+
		"\u0003\'\u0241\b\'\u0003\'\u0243\b\'\u0001(\u0001(\u0001(\u0001(\u0001"+
		")\u0001)\u0001)\u0003)\u024c\b)\u0001)\u0005)\u024f\b)\n)\f)\u0252\t)"+
		"\u0001*\u0001*\u0001*\u0001*\u0001*\u0003*\u0259\b*\u0001*\u0003*\u025c"+
		"\b*\u0001+\u0001+\u0003+\u0260\b+\u0001+\u0001+\u0001+\u0001+\u0003+\u0266"+
		"\b+\u0001,\u0001,\u0001,\u0001,\u0001-\u0001-\u0001-\u0001-\u0004-\u0270"+
		"\b-\u000b-\f-\u0271\u0001.\u0001.\u0001.\u0001.\u0003.\u0278\b.\u0001"+
		"/\u0001/\u0003/\u027c\b/\u00010\u00010\u00010\u00010\u00011\u00011\u0001"+
		"1\u00031\u0285\b1\u00011\u00011\u00011\u00031\u028a\b1\u00011\u00011\u0005"+
		"1\u028e\b1\n1\f1\u0291\t1\u00012\u00012\u00012\u00012\u00012\u00012\u0001"+
		"2\u00012\u00012\u00012\u00012\u00032\u029e\b2\u00013\u00013\u00013\u0001"+
		"3\u00053\u02a4\b3\n3\f3\u02a7\t3\u00013\u00013\u00013\u00013\u00033\u02ad"+
		"\b3\u00033\u02af\b3\u00014\u00014\u00015\u00015\u00016\u00016\u00016\u0001"+
		"7\u00037\u02b9\b7\u00017\u00017\u00037\u02bd\b7\u00018\u00018\u00018\u0001"+
		"8\u00018\u00018\u00058\u02c5\b8\n8\f8\u02c8\t8\u00018\u00038\u02cb\b8"+
		"\u00018\u00018\u00038\u02cf\b8\u00019\u00019\u00019\u00019\u00019\u0001"+
		"9\u00059\u02d7\b9\n9\f9\u02da\t9\u00019\u00039\u02dd\b9\u00019\u00019"+
		"\u00039\u02e1\b9\u0001:\u0005:\u02e4\b:\n:\f:\u02e7\t:\u0001:\u0001:\u0001"+
		":\u0001:\u0001;\u0001;\u0001;\u0001;\u0005;\u02f1\b;\n;\f;\u02f4\t;\u0001"+
		";\u0003;\u02f7\b;\u0001;\u0001;\u0001;\u0001;\u0001;\u0001;\u0004;\u02ff"+
		"\b;\u000b;\f;\u0300\u0003;\u0303\b;\u0001<\u0001<\u0001<\u0001<\u0005"+
		"<\u0309\b<\n<\f<\u030c\t<\u0001<\u0003<\u030f\b<\u0001<\u0001<\u0001<"+
		"\u0001<\u0001<\u0001<\u0004<\u0317\b<\u000b<\f<\u0318\u0003<\u031b\b<"+
		"\u0001=\u0001=\u0001=\u0001=\u0003=\u0321\b=\u0001>\u0001>\u0001>\u0001"+
		">\u0001?\u0001?\u0001?\u0005?\u032a\b?\n?\f?\u032d\t?\u0001@\u0001@\u0001"+
		"@\u0001@\u0001A\u0001A\u0004A\u0335\bA\u000bA\fA\u0336\u0001A\u0001A\u0001"+
		"B\u0001B\u0005B\u033d\bB\nB\fB\u0340\tB\u0001C\u0001C\u0001C\u0003C\u0345"+
		"\bC\u0001D\u0001D\u0001D\u0001E\u0001E\u0001E\u0003E\u034d\bE\u0001E\u0001"+
		"E\u0003E\u0351\bE\u0001F\u0001F\u0001G\u0001G\u0001G\u0001H\u0001H\u0001"+
		"H\u0001I\u0001I\u0001I\u0003I\u035e\bI\u0001J\u0001J\u0003J\u0362\bJ\u0001"+
		"K\u0001K\u0001L\u0001L\u0001M\u0001M\u0001N\u0001N\u0001O\u0001O\u0001"+
		"P\u0001P\u0001Q\u0001Q\u0001R\u0001R\u0003R\u0374\bR\u0001S\u0001S\u0001"+
		"T\u0001T\u0001T\u0001T\u0000\u0001bU\u0000\u0002\u0004\u0006\b\n\f\u000e"+
		"\u0010\u0012\u0014\u0016\u0018\u001a\u001c\u001e \"$&(*,.02468:<>@BDF"+
		"HJLNPRTVXZ\\^`bdfhjlnprtvxz|~\u0080\u0082\u0084\u0086\u0088\u008a\u008c"+
		"\u008e\u0090\u0092\u0094\u0096\u0098\u009a\u009c\u009e\u00a0\u00a2\u00a4"+
		"\u00a6\u00a8\u0000\n\u0003\u0000\n\n\r\r\u0017\u0018\u0002\u0000\r\r\u0018"+
		"\u0018\u0003\u0000\r\r\u0015\u0015\u0018\u0018\u0001\u0000HI\u0002\u0000"+
		"\r\rYY\u0002\u0000\n\n33\u0002\u0000,,99\u0001\u000068\u0001\u0000\u001f"+
		" \u0001\u0000UW\u03bf\u0000\u00ad\u0001\u0000\u0000\u0000\u0002\u00c1"+
		"\u0001\u0000\u0000\u0000\u0004\u00de\u0001\u0000\u0000\u0000\u0006\u00e0"+
		"\u0001\u0000\u0000\u0000\b\u00ee\u0001\u0000\u0000\u0000\n\u00f8\u0001"+
		"\u0000\u0000\u0000\f\u00fa\u0001\u0000\u0000\u0000\u000e\u0101\u0001\u0000"+
		"\u0000\u0000\u0010\u010b\u0001\u0000\u0000\u0000\u0012\u0112\u0001\u0000"+
		"\u0000\u0000\u0014\u0120\u0001\u0000\u0000\u0000\u0016\u0129\u0001\u0000"+
		"\u0000\u0000\u0018\u013a\u0001\u0000\u0000\u0000\u001a\u0148\u0001\u0000"+
		"\u0000\u0000\u001c\u014f\u0001\u0000\u0000\u0000\u001e\u0151\u0001\u0000"+
		"\u0000\u0000 \u0159\u0001\u0000\u0000\u0000\"\u0161\u0001\u0000\u0000"+
		"\u0000$\u0173\u0001\u0000\u0000\u0000&\u0185\u0001\u0000\u0000\u0000("+
		"\u0187\u0001\u0000\u0000\u0000*\u019d\u0001\u0000\u0000\u0000,\u01a0\u0001"+
		"\u0000\u0000\u0000.\u01aa\u0001\u0000\u0000\u00000\u01ac\u0001\u0000\u0000"+
		"\u00002\u01c1\u0001\u0000\u0000\u00004\u01d2\u0001\u0000\u0000\u00006"+
		"\u01dc\u0001\u0000\u0000\u00008\u01de\u0001\u0000\u0000\u0000:\u01ed\u0001"+
		"\u0000\u0000\u0000<\u01f4\u0001\u0000\u0000\u0000>\u01f6\u0001\u0000\u0000"+
		"\u0000@\u01f8\u0001\u0000\u0000\u0000B\u01fe\u0001\u0000\u0000\u0000D"+
		"\u0209\u0001\u0000\u0000\u0000F\u020e\u0001\u0000\u0000\u0000H\u021a\u0001"+
		"\u0000\u0000\u0000J\u0228\u0001\u0000\u0000\u0000L\u022f\u0001\u0000\u0000"+
		"\u0000N\u0242\u0001\u0000\u0000\u0000P\u0244\u0001\u0000\u0000\u0000R"+
		"\u0248\u0001\u0000\u0000\u0000T\u025b\u0001\u0000\u0000\u0000V\u0265\u0001"+
		"\u0000\u0000\u0000X\u0267\u0001\u0000\u0000\u0000Z\u026f\u0001\u0000\u0000"+
		"\u0000\\\u0273\u0001\u0000\u0000\u0000^\u027b\u0001\u0000\u0000\u0000"+
		"`\u027d\u0001\u0000\u0000\u0000b\u0289\u0001\u0000\u0000\u0000d\u029d"+
		"\u0001\u0000\u0000\u0000f\u02ae\u0001\u0000\u0000\u0000h\u02b0\u0001\u0000"+
		"\u0000\u0000j\u02b2\u0001\u0000\u0000\u0000l\u02b4\u0001\u0000\u0000\u0000"+
		"n\u02bc\u0001\u0000\u0000\u0000p\u02ce\u0001\u0000\u0000\u0000r\u02e0"+
		"\u0001\u0000\u0000\u0000t\u02e5\u0001\u0000\u0000\u0000v\u0302\u0001\u0000"+
		"\u0000\u0000x\u031a\u0001\u0000\u0000\u0000z\u0320\u0001\u0000\u0000\u0000"+
		"|\u0322\u0001\u0000\u0000\u0000~\u032b\u0001\u0000\u0000\u0000\u0080\u032e"+
		"\u0001\u0000\u0000\u0000\u0082\u0332\u0001\u0000\u0000\u0000\u0084\u033a"+
		"\u0001\u0000\u0000\u0000\u0086\u0344\u0001\u0000\u0000\u0000\u0088\u0346"+
		"\u0001\u0000\u0000\u0000\u008a\u0350\u0001\u0000\u0000\u0000\u008c\u0352"+
		"\u0001\u0000\u0000\u0000\u008e\u0354\u0001\u0000\u0000\u0000\u0090\u0357"+
		"\u0001\u0000\u0000\u0000\u0092\u035d\u0001\u0000\u0000\u0000\u0094\u0361"+
		"\u0001\u0000\u0000\u0000\u0096\u0363\u0001\u0000\u0000\u0000\u0098\u0365"+
		"\u0001\u0000\u0000\u0000\u009a\u0367\u0001\u0000\u0000\u0000\u009c\u0369"+
		"\u0001\u0000\u0000\u0000\u009e\u036b\u0001\u0000\u0000\u0000\u00a0\u036d"+
		"\u0001\u0000\u0000\u0000\u00a2\u036f\u0001\u0000\u0000\u0000\u00a4\u0373"+
		"\u0001\u0000\u0000\u0000\u00a6\u0375\u0001\u0000\u0000\u0000\u00a8\u0377"+
		"\u0001\u0000\u0000\u0000\u00aa\u00ac\u0003\u00a4R\u0000\u00ab\u00aa\u0001"+
		"\u0000\u0000\u0000\u00ac\u00af\u0001\u0000\u0000\u0000\u00ad\u00ab\u0001"+
		"\u0000\u0000\u0000\u00ad\u00ae\u0001\u0000\u0000\u0000\u00ae\u00b1\u0001"+
		"\u0000\u0000\u0000\u00af\u00ad\u0001\u0000\u0000\u0000\u00b0\u00b2\u0003"+
		"\u0002\u0001\u0000\u00b1\u00b0\u0001\u0000\u0000\u0000\u00b1\u00b2\u0001"+
		"\u0000\u0000\u0000\u00b2\u00bc\u0001\u0000\u0000\u0000\u00b3\u00b7\u0003"+
		"\u0004\u0002\u0000\u00b4\u00b6\u0003\u00a4R\u0000\u00b5\u00b4\u0001\u0000"+
		"\u0000\u0000\u00b6\u00b9\u0001\u0000\u0000\u0000\u00b7\u00b5\u0001\u0000"+
		"\u0000\u0000\u00b7\u00b8\u0001\u0000\u0000\u0000\u00b8\u00bb\u0001\u0000"+
		"\u0000\u0000\u00b9\u00b7\u0001\u0000\u0000\u0000\u00ba\u00b3\u0001\u0000"+
		"\u0000\u0000\u00bb\u00be\u0001\u0000\u0000\u0000\u00bc\u00ba\u0001\u0000"+
		"\u0000\u0000\u00bc\u00bd\u0001\u0000\u0000\u0000\u00bd\u00bf\u0001\u0000"+
		"\u0000\u0000\u00be\u00bc\u0001\u0000\u0000\u0000\u00bf\u00c0\u0005\u0000"+
		"\u0000\u0001\u00c0\u0001\u0001\u0000\u0000\u0000\u00c1\u00c5\u0003\u0006"+
		"\u0003\u0000\u00c2\u00c4\u0003\u00a4R\u0000\u00c3\u00c2\u0001\u0000\u0000"+
		"\u0000\u00c4\u00c7\u0001\u0000\u0000\u0000\u00c5\u00c3\u0001\u0000\u0000"+
		"\u0000\u00c5\u00c6\u0001\u0000\u0000\u0000\u00c6\u00c8\u0001\u0000\u0000"+
		"\u0000\u00c7\u00c5\u0001\u0000\u0000\u0000\u00c8\u00cc\u0005!\u0000\u0000"+
		"\u00c9\u00cb\u0003\u00a4R\u0000\u00ca\u00c9\u0001\u0000\u0000\u0000\u00cb"+
		"\u00ce\u0001\u0000\u0000\u0000\u00cc\u00ca\u0001\u0000\u0000\u0000\u00cc"+
		"\u00cd\u0001\u0000\u0000\u0000\u00cd\u0003\u0001\u0000\u0000\u0000\u00ce"+
		"\u00cc\u0001\u0000\u0000\u0000\u00cf\u00d3\u0003\u0006\u0003\u0000\u00d0"+
		"\u00d2\u0003\u00a4R\u0000\u00d1\u00d0\u0001\u0000\u0000\u0000\u00d2\u00d5"+
		"\u0001\u0000\u0000\u0000\u00d3\u00d1\u0001\u0000\u0000\u0000\u00d3\u00d4"+
		"\u0001\u0000\u0000\u0000\u00d4\u00d9\u0001\u0000\u0000\u0000\u00d5\u00d3"+
		"\u0001\u0000\u0000\u0000\u00d6\u00da\u0003\u000e\u0007\u0000\u00d7\u00da"+
		"\u0003\u0016\u000b\u0000\u00d8\u00da\u0003\u0010\b\u0000\u00d9\u00d6\u0001"+
		"\u0000\u0000\u0000\u00d9\u00d7\u0001\u0000\u0000\u0000\u00d9\u00d8\u0001"+
		"\u0000\u0000\u0000\u00da\u00df\u0001\u0000\u0000\u0000\u00db\u00df\u0003"+
		"\u000e\u0007\u0000\u00dc\u00df\u0003\u0016\u000b\u0000\u00dd\u00df\u0003"+
		"\u0010\b\u0000\u00de\u00cf\u0001\u0000\u0000\u0000\u00de\u00db\u0001\u0000"+
		"\u0000\u0000\u00de\u00dc\u0001\u0000\u0000\u0000\u00de\u00dd\u0001\u0000"+
		"\u0000\u0000\u00df\u0005\u0001\u0000\u0000\u0000\u00e0\u00ea\u0003\n\u0005"+
		"\u0000\u00e1\u00e3\u0003\u00a4R\u0000\u00e2\u00e1\u0001\u0000\u0000\u0000"+
		"\u00e3\u00e6\u0001\u0000\u0000\u0000\u00e4\u00e2\u0001\u0000\u0000\u0000"+
		"\u00e4\u00e5\u0001\u0000\u0000\u0000\u00e5\u00e7\u0001\u0000\u0000\u0000"+
		"\u00e6\u00e4\u0001\u0000\u0000\u0000\u00e7\u00e9\u0003\n\u0005\u0000\u00e8"+
		"\u00e4\u0001\u0000\u0000\u0000\u00e9\u00ec\u0001\u0000\u0000\u0000\u00ea"+
		"\u00e8\u0001\u0000\u0000\u0000\u00ea\u00eb\u0001\u0000\u0000\u0000\u00eb"+
		"\u0007\u0001\u0000\u0000\u0000\u00ec\u00ea\u0001\u0000\u0000\u0000\u00ed"+
		"\u00ef\u0003\n\u0005\u0000\u00ee\u00ed\u0001\u0000\u0000\u0000\u00ef\u00f0"+
		"\u0001\u0000\u0000\u0000\u00f0\u00ee\u0001\u0000\u0000\u0000\u00f0\u00f1"+
		"\u0001\u0000\u0000\u0000\u00f1\t\u0001\u0000\u0000\u0000\u00f2\u00f3\u0005"+
		"\u0013\u0000\u0000\u00f3\u00f5\u0005\n\u0000\u0000\u00f4\u00f6\u0003v"+
		";\u0000\u00f5\u00f4\u0001\u0000\u0000\u0000\u00f5\u00f6\u0001\u0000\u0000"+
		"\u0000\u00f6\u00f9\u0001\u0000\u0000\u0000\u00f7\u00f9\u0003\f\u0006\u0000"+
		"\u00f8\u00f2\u0001\u0000\u0000\u0000\u00f8\u00f7\u0001\u0000\u0000\u0000"+
		"\u00f9\u000b\u0001\u0000\u0000\u0000\u00fa\u00fb\u0005\u0012\u0000\u0000"+
		"\u00fb\u00fc\u0003^/\u0000\u00fc\u00fd\u0005N\u0000\u0000\u00fd\r\u0001"+
		"\u0000\u0000\u0000\u00fe\u0100\u0003\u00a0P\u0000\u00ff\u00fe\u0001\u0000"+
		"\u0000\u0000\u0100\u0103\u0001\u0000\u0000\u0000\u0101\u00ff\u0001\u0000"+
		"\u0000\u0000\u0101\u0102\u0001\u0000\u0000\u0000\u0102\u0104\u0001\u0000"+
		"\u0000\u0000\u0103\u0101\u0001\u0000\u0000\u0000\u0104\u0105\u0005+\u0000"+
		"\u0000\u0105\u0107\u0003\u0092I\u0000\u0106\u0108\u0003*\u0015\u0000\u0107"+
		"\u0106\u0001\u0000\u0000\u0000\u0107\u0108\u0001\u0000\u0000\u0000\u0108"+
		"\u0109\u0001\u0000\u0000\u0000\u0109\u010a\u0003\u0012\t\u0000\u010a\u000f"+
		"\u0001\u0000\u0000\u0000\u010b\u010c\u0005(\u0000\u0000\u010c\u010d\u0005"+
		"\n\u0000\u0000\u010d\u010e\u0005<\u0000\u0000\u010e\u0110\u0003\u001c"+
		"\u000e\u0000\u010f\u0111\u0005$\u0000\u0000\u0110\u010f\u0001\u0000\u0000"+
		"\u0000\u0110\u0111\u0001\u0000\u0000\u0000\u0111\u0011\u0001\u0000\u0000"+
		"\u0000\u0112\u0118\u0005\"\u0000\u0000\u0113\u0117\u0003\u0014\n\u0000"+
		"\u0114\u0117\u0003\u0016\u000b\u0000\u0115\u0117\u0003\u00a4R\u0000\u0116"+
		"\u0113\u0001\u0000\u0000\u0000\u0116\u0114\u0001\u0000\u0000\u0000\u0116"+
		"\u0115\u0001\u0000\u0000\u0000\u0117\u011a\u0001\u0000\u0000\u0000\u0118"+
		"\u0116\u0001\u0000\u0000\u0000\u0118\u0119\u0001\u0000\u0000\u0000\u0119"+
		"\u011b\u0001\u0000\u0000\u0000\u011a\u0118\u0001\u0000\u0000\u0000\u011b"+
		"\u011c\u0005#\u0000\u0000\u011c\u0013\u0001\u0000\u0000\u0000\u011d\u011f"+
		"\u0003\u009eO\u0000\u011e\u011d\u0001\u0000\u0000\u0000\u011f\u0122\u0001"+
		"\u0000\u0000\u0000\u0120\u011e\u0001\u0000\u0000\u0000\u0120\u0121\u0001"+
		"\u0000\u0000\u0000\u0121\u0123\u0001\u0000\u0000\u0000\u0122\u0120\u0001"+
		"\u0000\u0000\u0000\u0123\u0124\u0005*\u0000\u0000\u0124\u0125\u00030\u0018"+
		"\u0000\u0125\u0015\u0001\u0000\u0000\u0000\u0126\u0128\u0003\u00a2Q\u0000"+
		"\u0127\u0126\u0001\u0000\u0000\u0000\u0128\u012b\u0001\u0000\u0000\u0000"+
		"\u0129\u0127\u0001\u0000\u0000\u0000\u0129\u012a\u0001\u0000\u0000\u0000"+
		"\u012a\u012c\u0001\u0000\u0000\u0000\u012b\u0129\u0001\u0000\u0000\u0000"+
		"\u012c\u012d\u0005&\u0000\u0000\u012d\u012f\u0003\u0094J\u0000\u012e\u0130"+
		"\u0003\u0018\f\u0000\u012f\u012e\u0001\u0000\u0000\u0000\u012f\u0130\u0001"+
		"\u0000\u0000\u0000\u0130\u0134\u0001\u0000\u0000\u0000\u0131\u0133\u0003"+
		"\u00a4R\u0000\u0132\u0131\u0001\u0000\u0000\u0000\u0133\u0136\u0001\u0000"+
		"\u0000\u0000\u0134\u0132\u0001\u0000\u0000\u0000\u0134\u0135\u0001\u0000"+
		"\u0000\u0000\u0135\u0137\u0001\u0000\u0000\u0000\u0136\u0134\u0001\u0000"+
		"\u0000\u0000\u0137\u0138\u0003\u001a\r\u0000\u0138\u0017\u0001\u0000\u0000"+
		"\u0000\u0139\u013b\u0003*\u0015\u0000\u013a\u0139\u0001\u0000\u0000\u0000"+
		"\u013a\u013b\u0001\u0000\u0000\u0000\u013b\u013c\u0001\u0000\u0000\u0000"+
		"\u013c\u013d\u0005=\u0000\u0000\u013d\u013e\u0003\u001c\u000e\u0000\u013e"+
		"\u0019\u0001\u0000\u0000\u0000\u013f\u0141\u0005)\u0000\u0000\u0140\u013f"+
		"\u0001\u0000\u0000\u0000\u0140\u0141\u0001\u0000\u0000\u0000\u0141\u0142"+
		"\u0001\u0000\u0000\u0000\u0142\u0149\u00030\u0018\u0000\u0143\u0144\u0005"+
		"\u0014\u0000\u0000\u0144\u0146\u0003^/\u0000\u0145\u0147\u0005\f\u0000"+
		"\u0000\u0146\u0145\u0001\u0000\u0000\u0000\u0146\u0147\u0001\u0000\u0000"+
		"\u0000\u0147\u0149\u0001\u0000\u0000\u0000\u0148\u0140\u0001\u0000\u0000"+
		"\u0000\u0148\u0143\u0001\u0000\u0000\u0000\u0149\u001b\u0001\u0000\u0000"+
		"\u0000\u014a\u014c\u0003\b\u0004\u0000\u014b\u014d\u0003\u001e\u000f\u0000"+
		"\u014c\u014b\u0001\u0000\u0000\u0000\u014c\u014d\u0001\u0000\u0000\u0000"+
		"\u014d\u0150\u0001\u0000\u0000\u0000\u014e\u0150\u0003\u001e\u000f\u0000"+
		"\u014f\u014a\u0001\u0000\u0000\u0000\u014f\u014e\u0001\u0000\u0000\u0000"+
		"\u0150\u001d\u0001\u0000\u0000\u0000\u0151\u0156\u0003 \u0010\u0000\u0152"+
		"\u0153\u0005\u001a\u0000\u0000\u0153\u0155\u0003 \u0010\u0000\u0154\u0152"+
		"\u0001\u0000\u0000\u0000\u0155\u0158\u0001\u0000\u0000\u0000\u0156\u0154"+
		"\u0001\u0000\u0000\u0000\u0156\u0157\u0001\u0000\u0000\u0000\u0157\u001f"+
		"\u0001\u0000\u0000\u0000\u0158\u0156\u0001\u0000\u0000\u0000\u0159\u015b"+
		"\u0003\"\u0011\u0000\u015a\u015c\u0005\u0019\u0000\u0000\u015b\u015a\u0001"+
		"\u0000\u0000\u0000\u015b\u015c\u0001\u0000\u0000\u0000\u015c!\u0001\u0000"+
		"\u0000\u0000\u015d\u0162\u0003.\u0017\u0000\u015e\u0162\u0003$\u0012\u0000"+
		"\u015f\u0162\u0003&\u0013\u0000\u0160\u0162\u0003(\u0014\u0000\u0161\u015d"+
		"\u0001\u0000\u0000\u0000\u0161\u015e\u0001\u0000\u0000\u0000\u0161\u015f"+
		"\u0001\u0000\u0000\u0000\u0161\u0160\u0001\u0000\u0000\u0000\u0162#\u0001"+
		"\u0000\u0000\u0000\u0163\u0164\u0005\u000e\u0000\u0000\u0164\u0174\u0005"+
		"#\u0000\u0000\u0165\u0166\u0005\u000e\u0000\u0000\u0166\u016b\u0003,\u0016"+
		"\u0000\u0167\u0168\u0005L\u0000\u0000\u0168\u016a\u0003,\u0016\u0000\u0169"+
		"\u0167\u0001\u0000\u0000\u0000\u016a\u016d\u0001\u0000\u0000\u0000\u016b"+
		"\u0169\u0001\u0000\u0000\u0000\u016b\u016c\u0001\u0000\u0000\u0000\u016c"+
		"\u016f\u0001\u0000\u0000\u0000\u016d\u016b\u0001\u0000\u0000\u0000\u016e"+
		"\u0170\u0005L\u0000\u0000\u016f\u016e\u0001\u0000\u0000\u0000\u016f\u0170"+
		"\u0001\u0000\u0000\u0000\u0170\u0171\u0001\u0000\u0000\u0000\u0171\u0172"+
		"\u0005#\u0000\u0000\u0172\u0174\u0001\u0000\u0000\u0000\u0173\u0163\u0001"+
		"\u0000\u0000\u0000\u0173\u0165\u0001\u0000\u0000\u0000\u0174%\u0001\u0000"+
		"\u0000\u0000\u0175\u0176\u0005\u000f\u0000\u0000\u0176\u0186\u0005N\u0000"+
		"\u0000\u0177\u0178\u0005\u000f\u0000\u0000\u0178\u017d\u0003,\u0016\u0000"+
		"\u0179\u017a\u0005L\u0000\u0000\u017a\u017c\u0003,\u0016\u0000\u017b\u0179"+
		"\u0001\u0000\u0000\u0000\u017c\u017f\u0001\u0000\u0000\u0000\u017d\u017b"+
		"\u0001\u0000\u0000\u0000\u017d\u017e\u0001\u0000\u0000\u0000\u017e\u0181"+
		"\u0001\u0000\u0000\u0000\u017f\u017d\u0001\u0000\u0000\u0000\u0180\u0182"+
		"\u0005L\u0000\u0000\u0181\u0180\u0001\u0000\u0000\u0000\u0181\u0182\u0001"+
		"\u0000\u0000\u0000\u0182\u0183\u0001\u0000\u0000\u0000\u0183\u0184\u0005"+
		"N\u0000\u0000\u0184\u0186\u0001\u0000\u0000\u0000\u0185\u0175\u0001\u0000"+
		"\u0000\u0000\u0185\u0177\u0001\u0000\u0000\u0000\u0186\'\u0001\u0000\u0000"+
		"\u0000\u0187\u0188\u0005\'\u0000\u0000\u0188\u0189\u0003*\u0015\u0000"+
		"\u0189\u018a\u0005=\u0000\u0000\u018a\u018b\u0003\u001c\u000e\u0000\u018b"+
		")\u0001\u0000\u0000\u0000\u018c\u018d\u0005@\u0000\u0000\u018d\u0192\u0003"+
		",\u0016\u0000\u018e\u018f\u0005L\u0000\u0000\u018f\u0191\u0003,\u0016"+
		"\u0000\u0190\u018e\u0001\u0000\u0000\u0000\u0191\u0194\u0001\u0000\u0000"+
		"\u0000\u0192\u0190\u0001\u0000\u0000\u0000\u0192\u0193\u0001\u0000\u0000"+
		"\u0000\u0193\u0196\u0001\u0000\u0000\u0000\u0194\u0192\u0001\u0000\u0000"+
		"\u0000\u0195\u0197\u0005L\u0000\u0000\u0196\u0195\u0001\u0000\u0000\u0000"+
		"\u0196\u0197\u0001\u0000\u0000\u0000\u0197\u0198\u0001\u0000\u0000\u0000"+
		"\u0198\u0199\u0005M\u0000\u0000\u0199\u019e\u0001\u0000\u0000\u0000\u019a"+
		"\u019b\u0005@\u0000\u0000\u019b\u019e\u0005M\u0000\u0000\u019c\u019e\u0005"+
		"?\u0000\u0000\u019d\u018c\u0001\u0000\u0000\u0000\u019d\u019a\u0001\u0000"+
		"\u0000\u0000\u019d\u019c\u0001\u0000\u0000\u0000\u019e+\u0001\u0000\u0000"+
		"\u0000\u019f\u01a1\u0003\u0006\u0003\u0000\u01a0\u019f\u0001\u0000\u0000"+
		"\u0000\u01a0\u01a1\u0001\u0000\u0000\u0000\u01a1\u01a2\u0001\u0000\u0000"+
		"\u0000\u01a2\u01a4\u0003\u001e\u000f\u0000\u01a3\u01a5\u0005\r\u0000\u0000"+
		"\u01a4\u01a3\u0001\u0000\u0000\u0000\u01a4\u01a5\u0001\u0000\u0000\u0000"+
		"\u01a5\u01a8\u0001\u0000\u0000\u0000\u01a6\u01a7\u0005R\u0000\u0000\u01a7"+
		"\u01a9\u0003^/\u0000\u01a8\u01a6\u0001\u0000\u0000\u0000\u01a8\u01a9\u0001"+
		"\u0000\u0000\u0000\u01a9-\u0001\u0000\u0000\u0000\u01aa\u01ab\u0007\u0000"+
		"\u0000\u0000\u01ab/\u0001\u0000\u0000\u0000\u01ac\u01b2\u0005\"\u0000"+
		"\u0000\u01ad\u01b1\u00032\u0019\u0000\u01ae\u01b1\u0005$\u0000\u0000\u01af"+
		"\u01b1\u0003\u00a4R\u0000\u01b0\u01ad\u0001\u0000\u0000\u0000\u01b0\u01ae"+
		"\u0001\u0000\u0000\u0000\u01b0\u01af\u0001\u0000\u0000\u0000\u01b1\u01b4"+
		"\u0001\u0000\u0000\u0000\u01b2\u01b0\u0001\u0000\u0000\u0000\u01b2\u01b3"+
		"\u0001\u0000\u0000\u0000\u01b3\u01b5\u0001\u0000\u0000\u0000\u01b4\u01b2"+
		"\u0001\u0000\u0000\u0000\u01b5\u01b6\u0005#\u0000\u0000\u01b61\u0001\u0000"+
		"\u0000\u0000\u01b7\u01bb\u0003\u0006\u0003\u0000\u01b8\u01ba\u0003\u00a4"+
		"R\u0000\u01b9\u01b8\u0001\u0000\u0000\u0000\u01ba\u01bd\u0001\u0000\u0000"+
		"\u0000\u01bb\u01b9\u0001\u0000\u0000\u0000\u01bb\u01bc\u0001\u0000\u0000"+
		"\u0000\u01bc\u01be\u0001\u0000\u0000\u0000\u01bd\u01bb\u0001\u0000\u0000"+
		"\u0000\u01be\u01bf\u00034\u001a\u0000\u01bf\u01c2\u0001\u0000\u0000\u0000"+
		"\u01c0\u01c2\u00034\u001a\u0000\u01c1\u01b7\u0001\u0000\u0000\u0000\u01c1"+
		"\u01c0\u0001\u0000\u0000\u0000\u01c23\u0001\u0000\u0000\u0000\u01c3\u01c4"+
		"\u0003\u008eG\u0000\u01c4\u01c5\u0005\u001e\u0000\u0000\u01c5\u01d3\u0001"+
		"\u0000\u0000\u0000\u01c6\u01c8\u0003\u008eG\u0000\u01c7\u01c6\u0001\u0000"+
		"\u0000\u0000\u01c7\u01c8\u0001\u0000\u0000\u0000\u01c8\u01c9\u0001\u0000"+
		"\u0000\u0000\u01c9\u01d3\u00030\u0018\u0000\u01ca\u01d3\u00036\u001b\u0000"+
		"\u01cb\u01d3\u0003N\'\u0000\u01cc\u01d3\u0003P(\u0000\u01cd\u01d3\u0003"+
		"R)\u0000\u01ce\u01d3\u0003V+\u0000\u01cf\u01d3\u0003F#\u0000\u01d0\u01d3"+
		"\u00038\u001c\u0000\u01d1\u01d3\u0003@ \u0000\u01d2\u01c3\u0001\u0000"+
		"\u0000\u0000\u01d2\u01c7\u0001\u0000\u0000\u0000\u01d2\u01ca\u0001\u0000"+
		"\u0000\u0000\u01d2\u01cb\u0001\u0000\u0000\u0000\u01d2\u01cc\u0001\u0000"+
		"\u0000\u0000\u01d2\u01cd\u0001\u0000\u0000\u0000\u01d2\u01ce\u0001\u0000"+
		"\u0000\u0000\u01d2\u01cf\u0001\u0000\u0000\u0000\u01d2\u01d0\u0001\u0000"+
		"\u0000\u0000\u01d2\u01d1\u0001\u0000\u0000\u0000\u01d35\u0001\u0000\u0000"+
		"\u0000\u01d4\u01d5\u0003\u009aM\u0000\u01d5\u01d7\u0003x<\u0000\u01d6"+
		"\u01d8\u0003n7\u0000\u01d7\u01d6\u0001\u0000\u0000\u0000\u01d7\u01d8\u0001"+
		"\u0000\u0000\u0000\u01d8\u01dd\u0001\u0000\u0000\u0000\u01d9\u01da\u0003"+
		"\u009aM\u0000\u01da\u01db\u0003n7\u0000\u01db\u01dd\u0001\u0000\u0000"+
		"\u0000\u01dc\u01d4\u0001\u0000\u0000\u0000\u01dc\u01d9\u0001\u0000\u0000"+
		"\u0000\u01dd7\u0001\u0000\u0000\u0000\u01de\u01df\u00055\u0000\u0000\u01df"+
		"\u01e0\u0003>\u001f\u0000\u01e0\u01eb\u00030\u0018\u0000\u01e1\u01e3\u0003"+
		"\u00a4R\u0000\u01e2\u01e1\u0001\u0000\u0000\u0000\u01e3\u01e6\u0001\u0000"+
		"\u0000\u0000\u01e4\u01e2\u0001\u0000\u0000\u0000\u01e4\u01e5\u0001\u0000"+
		"\u0000\u0000\u01e5\u01e7\u0001\u0000\u0000\u0000\u01e6\u01e4\u0001\u0000"+
		"\u0000\u0000\u01e7\u01e8\u0005.\u0000\u0000\u01e8\u01e9\u0003<\u001e\u0000"+
		"\u01e9\u01ea\u0005\u001e\u0000\u0000\u01ea\u01ec\u0001\u0000\u0000\u0000"+
		"\u01eb\u01e4\u0001\u0000\u0000\u0000\u01eb\u01ec\u0001\u0000\u0000\u0000"+
		"\u01ec9\u0001\u0000\u0000\u0000\u01ed\u01ee\u0005.\u0000\u0000\u01ee\u01ef"+
		"\u0005=\u0000\u0000\u01ef\u01f0\u0003<\u001e\u0000\u01f0\u01f1\u0005\u001e"+
		"\u0000\u0000\u01f1;\u0001\u0000\u0000\u0000\u01f2\u01f5\u0003^/\u0000"+
		"\u01f3\u01f5\u00032\u0019\u0000\u01f4\u01f2\u0001\u0000\u0000\u0000\u01f4"+
		"\u01f3\u0001\u0000\u0000\u0000\u01f5=\u0001\u0000\u0000\u0000\u01f6\u01f7"+
		"\u0003v;\u0000\u01f7?\u0001\u0000\u0000\u0000\u01f8\u01f9\u00055\u0000"+
		"\u0000\u01f9\u01fa\u0005\"\u0000\u0000\u01fa\u01fb\u0005\u001e\u0000\u0000"+
		"\u01fb\u01fc\u0003B!\u0000\u01fc\u01fd\u0005#\u0000\u0000\u01fdA\u0001"+
		"\u0000\u0000\u0000\u01fe\u0203\u0003D\"\u0000\u01ff\u0202\u0003D\"\u0000"+
		"\u0200\u0202\u0003\u00a4R\u0000\u0201\u01ff\u0001\u0000\u0000\u0000\u0201"+
		"\u0200\u0001\u0000\u0000\u0000\u0202\u0205\u0001\u0000\u0000\u0000\u0203"+
		"\u0201\u0001\u0000\u0000\u0000\u0203\u0204\u0001\u0000\u0000\u0000\u0204"+
		"\u0207\u0001\u0000\u0000\u0000\u0205\u0203\u0001\u0000\u0000\u0000\u0206"+
		"\u0208\u0003:\u001d\u0000\u0207\u0206\u0001\u0000\u0000\u0000\u0207\u0208"+
		"\u0001\u0000\u0000\u0000\u0208C\u0001\u0000\u0000\u0000\u0209\u020a\u0003"+
		">\u001f\u0000\u020a\u020b\u0005=\u0000\u0000\u020b\u020c\u0003<\u001e"+
		"\u0000\u020c\u020d\u0005\u001e\u0000\u0000\u020dE\u0001\u0000\u0000\u0000"+
		"\u020e\u0210\u00055\u0000\u0000\u020f\u0211\u0003^/\u0000\u0210\u020f"+
		"\u0001\u0000\u0000\u0000\u0210\u0211\u0001\u0000\u0000\u0000\u0211\u0212"+
		"\u0001\u0000\u0000\u0000\u0212\u0213\u0005\"\u0000\u0000\u0213\u0214\u0005"+
		"\u001e\u0000\u0000\u0214\u0215\u0003H$\u0000\u0215\u0216\u0005#\u0000"+
		"\u0000\u0216G\u0001\u0000\u0000\u0000\u0217\u0219\u0003\u00a4R\u0000\u0218"+
		"\u0217\u0001\u0000\u0000\u0000\u0219\u021c\u0001\u0000\u0000\u0000\u021a"+
		"\u0218\u0001\u0000\u0000\u0000\u021a\u021b\u0001\u0000\u0000\u0000\u021b"+
		"\u021d\u0001\u0000\u0000\u0000\u021c\u021a\u0001\u0000\u0000\u0000\u021d"+
		"\u0222\u0003J%\u0000\u021e\u0221\u0003J%\u0000\u021f\u0221\u0003\u00a4"+
		"R\u0000\u0220\u021e\u0001\u0000\u0000\u0000\u0220\u021f\u0001\u0000\u0000"+
		"\u0000\u0221\u0224\u0001\u0000\u0000\u0000\u0222\u0220\u0001\u0000\u0000"+
		"\u0000\u0222\u0223\u0001\u0000\u0000\u0000\u0223\u0226\u0001\u0000\u0000"+
		"\u0000\u0224\u0222\u0001\u0000\u0000\u0000\u0225\u0227\u0003:\u001d\u0000"+
		"\u0226\u0225\u0001\u0000\u0000\u0000\u0226\u0227\u0001\u0000\u0000\u0000"+
		"\u0227I\u0001\u0000\u0000\u0000\u0228\u0229\u0003L&\u0000\u0229\u022a"+
		"\u0005=\u0000\u0000\u022a\u022b\u0003<\u001e\u0000\u022b\u022c\u0005\u001e"+
		"\u0000\u0000\u022cK\u0001\u0000\u0000\u0000\u022d\u0230\u0003^/\u0000"+
		"\u022e\u0230\u0003\u0082A\u0000\u022f\u022d\u0001\u0000\u0000\u0000\u022f"+
		"\u022e\u0001\u0000\u0000\u0000\u0230M\u0001\u0000\u0000\u0000\u0231\u0233"+
		"\u00054\u0000\u0000\u0232\u0231\u0001\u0000\u0000\u0000\u0232\u0233\u0001"+
		"\u0000\u0000\u0000\u0233\u0234\u0001\u0000\u0000\u0000\u0234\u0235\u0005"+
		"3\u0000\u0000\u0235\u0237\u0003\u0096K\u0000\u0236\u0238\u0003T*\u0000"+
		"\u0237\u0236\u0001\u0000\u0000\u0000\u0237\u0238\u0001\u0000\u0000\u0000"+
		"\u0238\u0243\u0001\u0000\u0000\u0000\u0239\u023b\u00054\u0000\u0000\u023a"+
		"\u0239\u0001\u0000\u0000\u0000\u023a\u023b\u0001\u0000\u0000\u0000\u023b"+
		"\u023c\u0001\u0000\u0000\u0000\u023c\u023d\u00053\u0000\u0000\u023d\u023e"+
		"\u0003\u001c\u000e\u0000\u023e\u0240\u0003\u0096K\u0000\u023f\u0241\u0003"+
		"T*\u0000\u0240\u023f\u0001\u0000\u0000\u0000\u0240\u0241\u0001\u0000\u0000"+
		"\u0000\u0241\u0243\u0001\u0000\u0000\u0000\u0242\u0232\u0001\u0000\u0000"+
		"\u0000\u0242\u023a\u0001\u0000\u0000\u0000\u0243O\u0001\u0000\u0000\u0000"+
		"\u0244\u0245\u0005O\u0000\u0000\u0245\u0246\u0003\u0096K\u0000\u0246\u0247"+
		"\u0003T*\u0000\u0247Q\u0001\u0000\u0000\u0000\u0248\u0249\u0005;\u0000"+
		"\u0000\u0249\u024b\u0003\u0098L\u0000\u024a\u024c\u0003x<\u0000\u024b"+
		"\u024a\u0001\u0000\u0000\u0000\u024b\u024c\u0001\u0000\u0000\u0000\u024c"+
		"\u0250\u0001\u0000\u0000\u0000\u024d\u024f\u0003\u008aE\u0000\u024e\u024d"+
		"\u0001\u0000\u0000\u0000\u024f\u0252\u0001\u0000\u0000\u0000\u0250\u024e"+
		"\u0001\u0000\u0000\u0000\u0250\u0251\u0001\u0000\u0000\u0000\u0251S\u0001"+
		"\u0000\u0000\u0000\u0252\u0250\u0001\u0000\u0000\u0000\u0253\u0258\u0005"+
		"<\u0000\u0000\u0254\u0259\u0003^/\u0000\u0255\u0259\u0003F#\u0000\u0256"+
		"\u0259\u0003@ \u0000\u0257\u0259\u00036\u001b\u0000\u0258\u0254\u0001"+
		"\u0000\u0000\u0000\u0258\u0255\u0001\u0000\u0000\u0000\u0258\u0256\u0001"+
		"\u0000\u0000\u0000\u0258\u0257\u0001\u0000\u0000\u0000\u0259\u025c\u0001"+
		"\u0000\u0000\u0000\u025a\u025c\u0003n7\u0000\u025b\u0253\u0001\u0000\u0000"+
		"\u0000\u025b\u025a\u0001\u0000\u0000\u0000\u025cU\u0001\u0000\u0000\u0000"+
		"\u025d\u025f\u0005/\u0000\u0000\u025e\u0260\u0003v;\u0000\u025f\u025e"+
		"\u0001\u0000\u0000\u0000\u025f\u0260\u0001\u0000\u0000\u0000\u0260\u0266"+
		"\u0001\u0000\u0000\u0000\u0261\u0262\u00050\u0000\u0000\u0262\u0266\u0005"+
		"\n\u0000\u0000\u0263\u0266\u00052\u0000\u0000\u0264\u0266\u00051\u0000"+
		"\u0000\u0265\u025d\u0001\u0000\u0000\u0000\u0265\u0261\u0001\u0000\u0000"+
		"\u0000\u0265\u0263\u0001\u0000\u0000\u0000\u0265\u0264\u0001\u0000\u0000"+
		"\u0000\u0266W\u0001\u0000\u0000\u0000\u0267\u0268\u0005\u0007\u0000\u0000"+
		"\u0268\u0269\u0003Z-\u0000\u0269\u026a\u0005\u0002\u0000\u0000\u026aY"+
		"\u0001\u0000\u0000\u0000\u026b\u0270\u0005E\u0000\u0000\u026c\u0270\u0005"+
		"F\u0000\u0000\u026d\u0270\u0005G\u0000\u0000\u026e\u0270\u0003\\.\u0000"+
		"\u026f\u026b\u0001\u0000\u0000\u0000\u026f\u026c\u0001\u0000\u0000\u0000"+
		"\u026f\u026d\u0001\u0000\u0000\u0000\u026f\u026e\u0001\u0000\u0000\u0000"+
		"\u0270\u0271\u0001\u0000\u0000\u0000\u0271\u026f\u0001\u0000\u0000\u0000"+
		"\u0271\u0272\u0001\u0000\u0000\u0000\u0272[\u0001\u0000\u0000\u0000\u0273"+
		"\u0274\u0005\b\u0000\u0000\u0274\u0275\u0003\u0084B\u0000\u0275\u0277"+
		"\u0005#\u0000\u0000\u0276\u0278\u0005X\u0000\u0000\u0277\u0276\u0001\u0000"+
		"\u0000\u0000\u0277\u0278\u0001\u0000\u0000\u0000\u0278]\u0001\u0000\u0000"+
		"\u0000\u0279\u027c\u0003b1\u0000\u027a\u027c\u0003|>\u0000\u027b\u0279"+
		"\u0001\u0000\u0000\u0000\u027b\u027a\u0001\u0000\u0000\u0000\u027c_\u0001"+
		"\u0000\u0000\u0000\u027d\u027e\u0005>\u0000\u0000\u027e\u027f\u0003^/"+
		"\u0000\u027f\u0280\u0005\f\u0000\u0000\u0280a\u0001\u0000\u0000\u0000"+
		"\u0281\u0282\u00061\uffff\uffff\u0000\u0282\u0284\u0003d2\u0000\u0283"+
		"\u0285\u0003\u0088D\u0000\u0284\u0283\u0001\u0000\u0000\u0000\u0284\u0285"+
		"\u0001\u0000\u0000\u0000\u0285\u028a\u0001\u0000\u0000\u0000\u0286\u0287"+
		"\u0003h4\u0000\u0287\u0288\u0003b1\u0002\u0288\u028a\u0001\u0000\u0000"+
		"\u0000\u0289\u0281\u0001\u0000\u0000\u0000\u0289\u0286\u0001\u0000\u0000"+
		"\u0000\u028a\u028f\u0001\u0000\u0000\u0000\u028b\u028c\n\u0001\u0000\u0000"+
		"\u028c\u028e\u0003j5\u0000\u028d\u028b\u0001\u0000\u0000\u0000\u028e\u0291"+
		"\u0001\u0000\u0000\u0000\u028f\u028d\u0001\u0000\u0000\u0000\u028f\u0290"+
		"\u0001\u0000\u0000\u0000\u0290c\u0001\u0000\u0000\u0000\u0291\u028f\u0001"+
		"\u0000\u0000\u0000\u0292\u029e\u0003\u0080@\u0000\u0293\u029e\u0003f3"+
		"\u0000\u0294\u029e\u0003\u0084B\u0000\u0295\u029e\u0003r9\u0000\u0296"+
		"\u029e\u0003p8\u0000\u0297\u029e\u0003`0\u0000\u0298\u029e\u0003l6\u0000"+
		"\u0299\u029e\u0005\u0015\u0000\u0000\u029a\u029e\u0005\u0016\u0000\u0000"+
		"\u029b\u029e\u0005\u0017\u0000\u0000\u029c\u029e\u0005\u0018\u0000\u0000"+
		"\u029d\u0292\u0001\u0000\u0000\u0000\u029d\u0293\u0001\u0000\u0000\u0000"+
		"\u029d\u0294\u0001\u0000\u0000\u0000\u029d\u0295\u0001\u0000\u0000\u0000"+
		"\u029d\u0296\u0001\u0000\u0000\u0000\u029d\u0297\u0001\u0000\u0000\u0000"+
		"\u029d\u0298\u0001\u0000\u0000\u0000\u029d\u0299\u0001\u0000\u0000\u0000"+
		"\u029d\u029a\u0001\u0000\u0000\u0000\u029d\u029b\u0001\u0000\u0000\u0000"+
		"\u029d\u029c\u0001\u0000\u0000\u0000\u029ee\u0001\u0000\u0000\u0000\u029f"+
		"\u02a5\u0005\u0010\u0000\u0000\u02a0\u02a4\u00032\u0019\u0000\u02a1\u02a4"+
		"\u0005$\u0000\u0000\u02a2\u02a4\u0003\u00a4R\u0000\u02a3\u02a0\u0001\u0000"+
		"\u0000\u0000\u02a3\u02a1\u0001\u0000\u0000\u0000\u02a3\u02a2\u0001\u0000"+
		"\u0000\u0000\u02a4\u02a7\u0001\u0000\u0000\u0000\u02a5\u02a3\u0001\u0000"+
		"\u0000\u0000\u02a5\u02a6\u0001\u0000\u0000\u0000\u02a6\u02a8\u0001\u0000"+
		"\u0000\u0000\u02a7\u02a5\u0001\u0000\u0000\u0000\u02a8\u02af\u0005#\u0000"+
		"\u0000\u02a9\u02aa\u0005\u0011\u0000\u0000\u02aa\u02ac\u0003^/\u0000\u02ab"+
		"\u02ad\u0005\f\u0000\u0000\u02ac\u02ab\u0001\u0000\u0000\u0000\u02ac\u02ad"+
		"\u0001\u0000\u0000\u0000\u02ad\u02af\u0001\u0000\u0000\u0000\u02ae\u029f"+
		"\u0001\u0000\u0000\u0000\u02ae\u02a9\u0001\u0000\u0000\u0000\u02afg\u0001"+
		"\u0000\u0000\u0000\u02b0\u02b1\u0005P\u0000\u0000\u02b1i\u0001\u0000\u0000"+
		"\u0000\u02b2\u02b3\u0005\u0019\u0000\u0000\u02b3k\u0001\u0000\u0000\u0000"+
		"\u02b4\u02b5\u0003\u0098L\u0000\u02b5\u02b6\u0003x<\u0000\u02b6m\u0001"+
		"\u0000\u0000\u0000\u02b7\u02b9\u0005\u001e\u0000\u0000\u02b8\u02b7\u0001"+
		"\u0000\u0000\u0000\u02b8\u02b9\u0001\u0000\u0000\u0000\u02b9\u02ba\u0001"+
		"\u0000\u0000\u0000\u02ba\u02bd\u0003X,\u0000\u02bb\u02bd\u0003b1\u0000"+
		"\u02bc\u02b8\u0001\u0000\u0000\u0000\u02bc\u02bb\u0001\u0000\u0000\u0000"+
		"\u02bdo\u0001\u0000\u0000\u0000\u02be\u02bf\u0005\u000f\u0000\u0000\u02bf"+
		"\u02cf\u0005N\u0000\u0000\u02c0\u02c1\u0005\u000f\u0000\u0000\u02c1\u02c6"+
		"\u0003^/\u0000\u02c2\u02c3\u0005L\u0000\u0000\u02c3\u02c5\u0003^/\u0000"+
		"\u02c4\u02c2\u0001\u0000\u0000\u0000\u02c5\u02c8\u0001\u0000\u0000\u0000"+
		"\u02c6\u02c4\u0001\u0000\u0000\u0000\u02c6\u02c7\u0001\u0000\u0000\u0000"+
		"\u02c7\u02ca\u0001\u0000\u0000\u0000\u02c8\u02c6\u0001\u0000\u0000\u0000"+
		"\u02c9\u02cb\u0005L\u0000\u0000\u02ca\u02c9\u0001\u0000\u0000\u0000\u02ca"+
		"\u02cb\u0001\u0000\u0000\u0000\u02cb\u02cc\u0001\u0000\u0000\u0000\u02cc"+
		"\u02cd\u0005N\u0000\u0000\u02cd\u02cf\u0001\u0000\u0000\u0000\u02ce\u02be"+
		"\u0001\u0000\u0000\u0000\u02ce\u02c0\u0001\u0000\u0000\u0000\u02cfq\u0001"+
		"\u0000\u0000\u0000\u02d0\u02d1\u0005\u000e\u0000\u0000\u02d1\u02e1\u0005"+
		"#\u0000\u0000\u02d2\u02d3\u0005\u000e\u0000\u0000\u02d3\u02d8\u0003t:"+
		"\u0000\u02d4\u02d5\u0005L\u0000\u0000\u02d5\u02d7\u0003t:\u0000\u02d6"+
		"\u02d4\u0001\u0000\u0000\u0000\u02d7\u02da\u0001\u0000\u0000\u0000\u02d8"+
		"\u02d6\u0001\u0000\u0000\u0000\u02d8\u02d9\u0001\u0000\u0000\u0000\u02d9"+
		"\u02dc\u0001\u0000\u0000\u0000\u02da\u02d8\u0001\u0000\u0000\u0000\u02db"+
		"\u02dd\u0005L\u0000\u0000\u02dc\u02db\u0001\u0000\u0000\u0000\u02dc\u02dd"+
		"\u0001\u0000\u0000\u0000\u02dd\u02de\u0001\u0000\u0000\u0000\u02de\u02df"+
		"\u0005#\u0000\u0000\u02df\u02e1\u0001\u0000\u0000\u0000\u02e0\u02d0\u0001"+
		"\u0000\u0000\u0000\u02e0\u02d2\u0001\u0000\u0000\u0000\u02e1s\u0001\u0000"+
		"\u0000\u0000\u02e2\u02e4\u0003\n\u0005\u0000\u02e3\u02e2\u0001\u0000\u0000"+
		"\u0000\u02e4\u02e7\u0001\u0000\u0000\u0000\u02e5\u02e3\u0001\u0000\u0000"+
		"\u0000\u02e5\u02e6\u0001\u0000\u0000\u0000\u02e6\u02e8\u0001\u0000\u0000"+
		"\u0000\u02e7\u02e5\u0001\u0000\u0000\u0000\u02e8\u02e9\u0007\u0001\u0000"+
		"\u0000\u02e9\u02ea\u0005R\u0000\u0000\u02ea\u02eb\u0003^/\u0000\u02eb"+
		"u\u0001\u0000\u0000\u0000\u02ec\u02ed\u0005@\u0000\u0000\u02ed\u02f2\u0003"+
		"^/\u0000\u02ee\u02ef\u0005L\u0000\u0000\u02ef\u02f1\u0003^/\u0000\u02f0"+
		"\u02ee\u0001\u0000\u0000\u0000\u02f1\u02f4\u0001\u0000\u0000\u0000\u02f2"+
		"\u02f0\u0001\u0000\u0000\u0000\u02f2\u02f3\u0001\u0000\u0000\u0000\u02f3"+
		"\u02f6\u0001\u0000\u0000\u0000\u02f4\u02f2\u0001\u0000\u0000\u0000\u02f5"+
		"\u02f7\u0005L\u0000\u0000\u02f6\u02f5\u0001\u0000\u0000\u0000\u02f6\u02f7"+
		"\u0001\u0000\u0000\u0000\u02f7\u02f8\u0001\u0000\u0000\u0000\u02f8\u02f9"+
		"\u0005M\u0000\u0000\u02f9\u0303\u0001\u0000\u0000\u0000\u02fa\u02fb\u0005"+
		"@\u0000\u0000\u02fb\u0303\u0005M\u0000\u0000\u02fc\u0303\u0005?\u0000"+
		"\u0000\u02fd\u02ff\u0003|>\u0000\u02fe\u02fd\u0001\u0000\u0000\u0000\u02ff"+
		"\u0300\u0001\u0000\u0000\u0000\u0300\u02fe\u0001\u0000\u0000\u0000\u0300"+
		"\u0301\u0001\u0000\u0000\u0000\u0301\u0303\u0001\u0000\u0000\u0000\u0302"+
		"\u02ec\u0001\u0000\u0000\u0000\u0302\u02fa\u0001\u0000\u0000\u0000\u0302"+
		"\u02fc\u0001\u0000\u0000\u0000\u0302\u02fe\u0001\u0000\u0000\u0000\u0303"+
		"w\u0001\u0000\u0000\u0000\u0304\u0305\u0005@\u0000\u0000\u0305\u030a\u0003"+
		"z=\u0000\u0306\u0307\u0005L\u0000\u0000\u0307\u0309\u0003z=\u0000\u0308"+
		"\u0306\u0001\u0000\u0000\u0000\u0309\u030c\u0001\u0000\u0000\u0000\u030a"+
		"\u0308\u0001\u0000\u0000\u0000\u030a\u030b\u0001\u0000\u0000\u0000\u030b"+
		"\u030e\u0001\u0000\u0000\u0000\u030c\u030a\u0001\u0000\u0000\u0000\u030d"+
		"\u030f\u0005L\u0000\u0000\u030e\u030d\u0001\u0000\u0000\u0000\u030e\u030f"+
		"\u0001\u0000\u0000\u0000\u030f\u0310\u0001\u0000\u0000\u0000\u0310\u0311"+
		"\u0005M\u0000\u0000\u0311\u031b\u0001\u0000\u0000\u0000\u0312\u0313\u0005"+
		"@\u0000\u0000\u0313\u031b\u0005M\u0000\u0000\u0314\u031b\u0005?\u0000"+
		"\u0000\u0315\u0317\u0003|>\u0000\u0316\u0315\u0001\u0000\u0000\u0000\u0317"+
		"\u0318\u0001\u0000\u0000\u0000\u0318\u0316\u0001\u0000\u0000\u0000\u0318"+
		"\u0319\u0001\u0000\u0000\u0000\u0319\u031b\u0001\u0000\u0000\u0000\u031a"+
		"\u0304\u0001\u0000\u0000\u0000\u031a\u0312\u0001\u0000\u0000\u0000\u031a"+
		"\u0314\u0001\u0000\u0000\u0000\u031a\u0316\u0001\u0000\u0000\u0000\u031b"+
		"y\u0001\u0000\u0000\u0000\u031c\u031d\u0007\u0001\u0000\u0000\u031d\u031e"+
		"\u0005R\u0000\u0000\u031e\u0321\u0003^/\u0000\u031f\u0321\u0003^/\u0000"+
		"\u0320\u031c\u0001\u0000\u0000\u0000\u0320\u031f\u0001\u0000\u0000\u0000"+
		"\u0321{\u0001\u0000\u0000\u0000\u0322\u0323\u0005\u0004\u0000\u0000\u0323"+
		"\u0324\u0003~?\u0000\u0324\u0325\u0005D\u0000\u0000\u0325}\u0001\u0000"+
		"\u0000\u0000\u0326\u032a\u0005C\u0000\u0000\u0327\u032a\u0003\u00a8T\u0000"+
		"\u0328\u032a\u0003\u0080@\u0000\u0329\u0326\u0001\u0000\u0000\u0000\u0329"+
		"\u0327\u0001\u0000\u0000\u0000\u0329\u0328\u0001\u0000\u0000\u0000\u032a"+
		"\u032d\u0001\u0000\u0000\u0000\u032b\u0329\u0001\u0000\u0000\u0000\u032b"+
		"\u032c\u0001\u0000\u0000\u0000\u032c\u007f\u0001\u0000\u0000\u0000\u032d"+
		"\u032b\u0001\u0000\u0000\u0000\u032e\u032f\u0005\t\u0000\u0000\u032f\u0330"+
		"\u0003^/\u0000\u0330\u0331\u0005N\u0000\u0000\u0331\u0081\u0001\u0000"+
		"\u0000\u0000\u0332\u0334\u0005\t\u0000\u0000\u0333\u0335\u0003\u008aE"+
		"\u0000\u0334\u0333\u0001\u0000\u0000\u0000\u0335\u0336\u0001\u0000\u0000"+
		"\u0000\u0336\u0334\u0001\u0000\u0000\u0000\u0336\u0337\u0001\u0000\u0000"+
		"\u0000\u0337\u0338\u0001\u0000\u0000\u0000\u0338\u0339\u0005N\u0000\u0000"+
		"\u0339\u0083\u0001\u0000\u0000\u0000\u033a\u033e\u0003\u0086C\u0000\u033b"+
		"\u033d\u0003\u008aE\u0000\u033c\u033b\u0001\u0000\u0000\u0000\u033d\u0340"+
		"\u0001\u0000\u0000\u0000\u033e\u033c\u0001\u0000\u0000\u0000\u033e\u033f"+
		"\u0001\u0000\u0000\u0000\u033f\u0085\u0001\u0000\u0000\u0000\u0340\u033e"+
		"\u0001\u0000\u0000\u0000\u0341\u0345\u0005\r\u0000\u0000\u0342\u0343\u0005"+
		"O\u0000\u0000\u0343\u0345\u0007\u0002\u0000\u0000\u0344\u0341\u0001\u0000"+
		"\u0000\u0000\u0344\u0342\u0001\u0000\u0000\u0000\u0345\u0087\u0001\u0000"+
		"\u0000\u0000\u0346\u0347\u0005Q\u0000\u0000\u0347\u0348\u0003^/\u0000"+
		"\u0348\u0089\u0001\u0000\u0000\u0000\u0349\u034a\u0003\u008cF\u0000\u034a"+
		"\u034c\u0003\u0098L\u0000\u034b\u034d\u0003x<\u0000\u034c\u034b\u0001"+
		"\u0000\u0000\u0000\u034c\u034d\u0001\u0000\u0000\u0000\u034d\u0351\u0001"+
		"\u0000\u0000\u0000\u034e\u0351\u0003\u0090H\u0000\u034f\u0351\u0005K\u0000"+
		"\u0000\u0350\u0349\u0001\u0000\u0000\u0000\u0350\u034e\u0001\u0000\u0000"+
		"\u0000\u0350\u034f\u0001\u0000\u0000\u0000\u0351\u008b\u0001\u0000\u0000"+
		"\u0000\u0352\u0353\u0007\u0003\u0000\u0000\u0353\u008d\u0001\u0000\u0000"+
		"\u0000\u0354\u0355\u0005:\u0000\u0000\u0355\u0356\u0005[\u0000\u0000\u0356"+
		"\u008f\u0001\u0000\u0000\u0000\u0357\u0358\u0005J\u0000\u0000\u0358\u0359"+
		"\u0005Z\u0000\u0000\u0359\u0091\u0001\u0000\u0000\u0000\u035a\u035e\u0005"+
		"\n\u0000\u0000\u035b\u035e\u0005\u000b\u0000\u0000\u035c\u035e\u0003|"+
		">\u0000\u035d\u035a\u0001\u0000\u0000\u0000\u035d\u035b\u0001\u0000\u0000"+
		"\u0000\u035d\u035c\u0001\u0000\u0000\u0000\u035e\u0093\u0001\u0000\u0000"+
		"\u0000\u035f\u0362\u0005\n\u0000\u0000\u0360\u0362\u0003|>\u0000\u0361"+
		"\u035f\u0001\u0000\u0000\u0000\u0361\u0360\u0001\u0000\u0000\u0000\u0362"+
		"\u0095\u0001\u0000\u0000\u0000\u0363\u0364\u0005\n\u0000\u0000\u0364\u0097"+
		"\u0001\u0000\u0000\u0000\u0365\u0366\u0007\u0004\u0000\u0000\u0366\u0099"+
		"\u0001\u0000\u0000\u0000\u0367\u0368\u0007\u0005\u0000\u0000\u0368\u009b"+
		"\u0001\u0000\u0000\u0000\u0369\u036a\u0007\u0000\u0000\u0000\u036a\u009d"+
		"\u0001\u0000\u0000\u0000\u036b\u036c\u0007\u0006\u0000\u0000\u036c\u009f"+
		"\u0001\u0000\u0000\u0000\u036d\u036e\u0005-\u0000\u0000\u036e\u00a1\u0001"+
		"\u0000\u0000\u0000\u036f\u0370\u0007\u0007\u0000\u0000\u0370\u00a3\u0001"+
		"\u0000\u0000\u0000\u0371\u0374\u0003\u00a6S\u0000\u0372\u0374\u0005\u001e"+
		"\u0000\u0000\u0373\u0371\u0001\u0000\u0000\u0000\u0373\u0372\u0001\u0000"+
		"\u0000\u0000\u0374\u00a5\u0001\u0000\u0000\u0000\u0375\u0376\u0007\b\u0000"+
		"\u0000\u0376\u00a7\u0001\u0000\u0000\u0000\u0377\u0378\u0005\u0003\u0000"+
		"\u0000\u0378\u0379\u0007\t\u0000\u0000\u0379\u00a9\u0001\u0000\u0000\u0000"+
		"s\u00ad\u00b1\u00b7\u00bc\u00c5\u00cc\u00d3\u00d9\u00de\u00e4\u00ea\u00f0"+
		"\u00f5\u00f8\u0101\u0107\u0110\u0116\u0118\u0120\u0129\u012f\u0134\u013a"+
		"\u0140\u0146\u0148\u014c\u014f\u0156\u015b\u0161\u016b\u016f\u0173\u017d"+
		"\u0181\u0185\u0192\u0196\u019d\u01a0\u01a4\u01a8\u01b0\u01b2\u01bb\u01c1"+
		"\u01c7\u01d2\u01d7\u01dc\u01e4\u01eb\u01f4\u0201\u0203\u0207\u0210\u021a"+
		"\u0220\u0222\u0226\u022f\u0232\u0237\u023a\u0240\u0242\u024b\u0250\u0258"+
		"\u025b\u025f\u0265\u026f\u0271\u0277\u027b\u0284\u0289\u028f\u029d\u02a3"+
		"\u02a5\u02ac\u02ae\u02b8\u02bc\u02c6\u02ca\u02ce\u02d8\u02dc\u02e0\u02e5"+
		"\u02f2\u02f6\u0300\u0302\u030a\u030e\u0318\u031a\u0320\u0329\u032b\u0336"+
		"\u033e\u0344\u034c\u0350\u035d\u0361\u0373";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}