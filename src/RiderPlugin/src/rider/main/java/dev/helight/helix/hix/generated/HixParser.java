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
		BEGIN_METADATA_VALUE=18, METADATA_PREFIX=19, CAST_BOOLEAN=20, EXPLICIT_ROOT=21,
		STATIC_CALL=22, BEGIN_INLINE_BLOCK=23, FAT_ARROW=24, NUMBER=25, STRING=26,
		BOOLEAN=27, NULL=28, MISSING=29, QUESTION=30, PIPE=31, ESCAPED_AT=32,
		ESCAPED_START=33, OUTER_WHITESPACE=34, NEWLINE=35, COMMENT=36, SLASH_COMMENT=37,
		SECTION_DELIMITER=38, LC=39, RC=40, SEMICOLON=41, COMMA=42, KEYWORD_FUNC=43,
		KEYWORD_DELEGATE=44, KEYWORD_TYPE=45, KEYWORD_TRAIT=46, KEYWORD_DO=47,
		KEYWORD_EXTERNAL=48, KEYWORD_EXPRESSION=49, KEYWORD_MIXIN=50, KEYWORD_PRELUDE=51,
		KEYWORD_DERIVATION=52, KEYWORD_ELSE=53, KEYWORD_RETURN=54, KEYWORD_GOTO=55,
		KEYWORD_BREAK=56, KEYWORD_CONTINUE=57, KEYWORD_LOCAL=58, KEYWORD_CARRY=59,
		KEYWORD_WHEN=60, KEYWORD_PURE=61, KEYWORD_META=62, KEYWORD_COMPILER=63,
		KEYWORD_BYTECODE=64, KEYWORD_RUNTIME=65, KEYWORD_FOR=66, KEYWORD_NOINLINE=67,
		KEYWORD_STRICT=68, LABEL_PREFIX=69, TOPLEVEL_DERIVATION=70, ASSIGN=71,
		ARROW=72, TOPLEVEL_VALUE_EXPRESSION=73, EMPTY_PARAMETERS=74, BEGIN_PARAMETERS=75,
		METADATA_WHITESPACE=76, DERIVATION_WHITESPACE=77, ARGUMENT_TEXT=78, ARGUMENT_END=79,
		CONTENT_WRAP=80, CONTENT_LINEBREAK=81, CONTENT_TEXT=82, VALUE_FUNCTION=83,
		VALUE_PREDICATE=84, VALUE_MEMBER=85, VALUE_WRAP=86, VALUE_DELIMITER=87,
		END_PARAMETERS=88, VALUE_END_INLINE=89, VALUE_SMART_ROOT=90, NOT_VALUE=91,
		VALUE_ELVIS=92, VALUE_ASSIGN=93, VALUE_EXPAND=94, VALUE_WHITESPACE=95,
		ESCAPE_MACRO=96, ESCAPE_LITERAL=97, ESCAPE_HEX=98, END_CONTENT_INTERPOLATE=99,
		FUNCTION_IDENTIFIER=100, MEMBER_IDENTIFIER=101, LABEL_IDENTIFIER=102,
		TOPLEVEL_QUESTION=103, TOPLEVEL_PIPE=104, TOPLEVEL_NULL=105, VALUE_CAST_BOOLEAN=106,
		VALUE_EXPLICIT_ROOT=107, VALUE_STATIC_CALL=108, VALUE_BLOCK_RESULT=109,
		VALUE_MISSING=110;
	public static final int
		RULE_compilationUnit = 0, RULE_fileMetadataSection = 1, RULE_topLevelDeclaration = 2,
		RULE_metadataList = 3, RULE_inlineMetadataList = 4, RULE_metadata = 5,
		RULE_metadataValue = 6, RULE_mixinDeclaration = 7, RULE_typeDeclaration = 8,
		RULE_metaDeclaration = 9, RULE_metaTarget = 10, RULE_mixinBody = 11, RULE_expressionDeclaration = 12,
		RULE_funcDeclaration = 13, RULE_directFunctionSignature = 14, RULE_collectiveInputPattern = 15,
		RULE_outputSignatureFields = 16, RULE_functionBody = 17, RULE_patternExpression = 18,
		RULE_patternUnion = 19, RULE_patternTerm = 20, RULE_patternPrimary = 21,
		RULE_tablePattern = 22, RULE_tuplePattern = 23, RULE_delegatePattern = 24,
		RULE_patternParameterList = 25, RULE_patternField = 26, RULE_patternIdentifier = 27,
		RULE_statementBlock = 28, RULE_statement = 29, RULE_statementBody = 30,
		RULE_invocationStatement = 31, RULE_whenConditionStatement = 32, RULE_whenElseBranch = 33,
		RULE_whenResult = 34, RULE_whenChainCondition = 35, RULE_whenChainStatement = 36,
		RULE_whenChainBody = 37, RULE_whenChainBranch = 38, RULE_whenValueStatement = 39,
		RULE_whenValueBody = 40, RULE_whenValueBranch = 41, RULE_whenValueCondition = 42,
		RULE_localDeclarationStatement = 43, RULE_localAssignmentStatement = 44,
		RULE_toplevelDerivationStatement = 45, RULE_assignedValue = 46, RULE_controlflowStatement = 47,
		RULE_contentBlock = 48, RULE_contentBody = 49, RULE_contentInterpolate = 50,
		RULE_value = 51, RULE_valueExpression = 52, RULE_nonArgumentValue = 53,
		RULE_primaryValue = 54, RULE_inlineBlockValue = 55, RULE_lambdaValue = 56,
		RULE_prefixOperators = 57, RULE_postfixOperators = 58, RULE_valueStatement = 59,
		RULE_callBinding = 60, RULE_tailValue = 61, RULE_tupleValue = 62, RULE_tableValue = 63,
		RULE_tableKeyedEntry = 64, RULE_valueList = 65, RULE_callValueList = 66,
		RULE_callArgument = 67, RULE_argumentValue = 68, RULE_argumentBody = 69,
		RULE_inlineValue = 70, RULE_inlineTransformation = 71, RULE_derivation = 72,
		RULE_explicitRoot = 73, RULE_derivationRoot = 74, RULE_elvisValue = 75,
		RULE_transformationPart = 76, RULE_functionChainType = 77, RULE_labelIdentifier = 78,
		RULE_memberIdentifier = 79, RULE_mixinIdentifier = 80, RULE_functionDeclarationIdentifier = 81,
		RULE_variableIdentifier = 82, RULE_functionIdentifier = 83, RULE_invocationIdentifier = 84,
		RULE_kindIdentifier = 85, RULE_expressionModifier = 86, RULE_mixinModifier = 87,
		RULE_metaModifier = 88, RULE_funcModifier = 89, RULE_trivia = 90, RULE_comment = 91,
		RULE_escaped = 92;
	private static String[] makeRuleNames() {
		return new String[] {
			"compilationUnit", "fileMetadataSection", "topLevelDeclaration", "metadataList",
			"inlineMetadataList", "metadata", "metadataValue", "mixinDeclaration",
			"typeDeclaration", "metaDeclaration", "metaTarget", "mixinBody", "expressionDeclaration",
			"funcDeclaration", "directFunctionSignature", "collectiveInputPattern",
			"outputSignatureFields", "functionBody", "patternExpression", "patternUnion",
			"patternTerm", "patternPrimary", "tablePattern", "tuplePattern", "delegatePattern",
			"patternParameterList", "patternField", "patternIdentifier", "statementBlock",
			"statement", "statementBody", "invocationStatement", "whenConditionStatement",
			"whenElseBranch", "whenResult", "whenChainCondition", "whenChainStatement",
			"whenChainBody", "whenChainBranch", "whenValueStatement", "whenValueBody",
			"whenValueBranch", "whenValueCondition", "localDeclarationStatement",
			"localAssignmentStatement", "toplevelDerivationStatement", "assignedValue",
			"controlflowStatement", "contentBlock", "contentBody", "contentInterpolate",
			"value", "valueExpression", "nonArgumentValue", "primaryValue", "inlineBlockValue",
			"lambdaValue", "prefixOperators", "postfixOperators", "valueStatement",
			"callBinding", "tailValue", "tupleValue", "tableValue", "tableKeyedEntry",
			"valueList", "callValueList", "callArgument", "argumentValue", "argumentBody",
			"inlineValue", "inlineTransformation", "derivation", "explicitRoot",
			"derivationRoot", "elvisValue", "transformationPart", "functionChainType",
			"labelIdentifier", "memberIdentifier", "mixinIdentifier", "functionDeclarationIdentifier",
			"variableIdentifier", "functionIdentifier", "invocationIdentifier", "kindIdentifier",
			"expressionModifier", "mixinModifier", "metaModifier", "funcModifier",
			"trivia", "comment", "escaped"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, "'@@'", null, null, null,
			null, null, "'---'", "'{'", null, null, null, "'func'", null, "'type'",
			"'trait'", "'do'", "'external'", "'expression'", "'mixin'", "'prelude'",
			"'derivation'", "'else'", "'return'", "'goto'", "'break'", "'continue'",
			"'local'", "'carry'", "'when'", "'pure'", "'meta'", "'compiler'", "'bytecode'",
			"'runtime'", "'for'", "'noinline'", "'strict'", null, null, null, null,
			null, null, "'('", null, null, null, "'>'", null, null, null, null, "':?'",
			"'#'", null, null, "')'", "']'", "'$'", "'!'", "'?:'", null, "'...'",
			null, null, null, null, null, null, null, null, null, null, null, "'@bool'",
			"'@root'", "'@static'", "'=>'", "'missing'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "ERROR_TOKEN", "TERMINATOR", "ESCAPE", "BEGIN_ARGUMENT", "BEGIN_VALUE",
			"BEGIN_VALUE_ESCAPED", "BEGIN_CONTENT", "BEGIN_VALUE_INTERPOLATE", "BEGIN_VALUE_INLINE",
			"IDENTIFIER", "NAMESPACE_IDENTIFIER", "VALUE_END", "ROOT_IDENTIFIER",
			"BEGIN_TABLE", "BEGIN_TUPLE", "BEGIN_LAMBDA_BLOCK", "BEGIN_LAMBDA_ARROW",
			"BEGIN_METADATA_VALUE", "METADATA_PREFIX", "CAST_BOOLEAN", "EXPLICIT_ROOT",
			"STATIC_CALL", "BEGIN_INLINE_BLOCK", "FAT_ARROW", "NUMBER", "STRING",
			"BOOLEAN", "NULL", "MISSING", "QUESTION", "PIPE", "ESCAPED_AT", "ESCAPED_START",
			"OUTER_WHITESPACE", "NEWLINE", "COMMENT", "SLASH_COMMENT", "SECTION_DELIMITER",
			"LC", "RC", "SEMICOLON", "COMMA", "KEYWORD_FUNC", "KEYWORD_DELEGATE",
			"KEYWORD_TYPE", "KEYWORD_TRAIT", "KEYWORD_DO", "KEYWORD_EXTERNAL", "KEYWORD_EXPRESSION",
			"KEYWORD_MIXIN", "KEYWORD_PRELUDE", "KEYWORD_DERIVATION", "KEYWORD_ELSE",
			"KEYWORD_RETURN", "KEYWORD_GOTO", "KEYWORD_BREAK", "KEYWORD_CONTINUE",
			"KEYWORD_LOCAL", "KEYWORD_CARRY", "KEYWORD_WHEN", "KEYWORD_PURE", "KEYWORD_META",
			"KEYWORD_COMPILER", "KEYWORD_BYTECODE", "KEYWORD_RUNTIME", "KEYWORD_FOR",
			"KEYWORD_NOINLINE", "KEYWORD_STRICT", "LABEL_PREFIX", "TOPLEVEL_DERIVATION",
			"ASSIGN", "ARROW", "TOPLEVEL_VALUE_EXPRESSION", "EMPTY_PARAMETERS", "BEGIN_PARAMETERS",
			"METADATA_WHITESPACE", "DERIVATION_WHITESPACE", "ARGUMENT_TEXT", "ARGUMENT_END",
			"CONTENT_WRAP", "CONTENT_LINEBREAK", "CONTENT_TEXT", "VALUE_FUNCTION",
			"VALUE_PREDICATE", "VALUE_MEMBER", "VALUE_WRAP", "VALUE_DELIMITER", "END_PARAMETERS",
			"VALUE_END_INLINE", "VALUE_SMART_ROOT", "NOT_VALUE", "VALUE_ELVIS", "VALUE_ASSIGN",
			"VALUE_EXPAND", "VALUE_WHITESPACE", "ESCAPE_MACRO", "ESCAPE_LITERAL",
			"ESCAPE_HEX", "END_CONTENT_INTERPOLATE", "FUNCTION_IDENTIFIER", "MEMBER_IDENTIFIER",
			"LABEL_IDENTIFIER", "TOPLEVEL_QUESTION", "TOPLEVEL_PIPE", "TOPLEVEL_NULL",
			"VALUE_CAST_BOOLEAN", "VALUE_EXPLICIT_ROOT", "VALUE_STATIC_CALL", "VALUE_BLOCK_RESULT",
			"VALUE_MISSING"
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
			setState(189);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 240518168576L) != 0)) {
				{
				{
				setState(186);
				trivia();
				}
				}
				setState(191);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(193);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,1,_ctx) ) {
			case 1:
				{
				setState(192);
				fileMetadataSection();
				}
				break;
			}
			setState(204);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 18)) & ~0x3f) == 0 && ((1L << (_la - 18)) & 835650748153859L) != 0)) {
				{
				{
				setState(195);
				topLevelDeclaration();
				setState(199);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 240518168576L) != 0)) {
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
				}
				}
				setState(206);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(207);
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
			setState(209);
			metadataList();
			setState(213);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 240518168576L) != 0)) {
				{
				{
				setState(210);
				trivia();
				}
				}
				setState(215);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(216);
			match(SECTION_DELIMITER);
			setState(220);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 240518168576L) != 0)) {
				{
				{
				setState(217);
				trivia();
				}
				}
				setState(222);
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
		public MetaDeclarationContext metaDeclaration() {
			return getRuleContext(MetaDeclarationContext.class,0);
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
			setState(240);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(223);
				metadataList();
				setState(227);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 240518168576L) != 0)) {
					{
					{
					setState(224);
					trivia();
					}
					}
					setState(229);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(234);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_MIXIN:
				case KEYWORD_DERIVATION:
					{
					setState(230);
					mixinDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_NOINLINE:
					{
					setState(231);
					funcDeclaration();
					}
					break;
				case KEYWORD_TYPE:
				case KEYWORD_TRAIT:
					{
					setState(232);
					typeDeclaration();
					}
					break;
				case KEYWORD_META:
				case KEYWORD_COMPILER:
				case KEYWORD_BYTECODE:
				case KEYWORD_RUNTIME:
					{
					setState(233);
					metaDeclaration();
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
				setState(236);
				mixinDeclaration();
				}
				break;
			case KEYWORD_FUNC:
			case KEYWORD_PURE:
			case KEYWORD_NOINLINE:
				enterOuterAlt(_localctx, 3);
				{
				setState(237);
				funcDeclaration();
				}
				break;
			case KEYWORD_TYPE:
			case KEYWORD_TRAIT:
				enterOuterAlt(_localctx, 4);
				{
				setState(238);
				typeDeclaration();
				}
				break;
			case KEYWORD_META:
			case KEYWORD_COMPILER:
			case KEYWORD_BYTECODE:
			case KEYWORD_RUNTIME:
				enterOuterAlt(_localctx, 5);
				{
				setState(239);
				metaDeclaration();
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
			setState(242);
			metadata();
			setState(252);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,10,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(246);
					_errHandler.sync(this);
					_la = _input.LA(1);
					while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 240518168576L) != 0)) {
						{
						{
						setState(243);
						trivia();
						}
						}
						setState(248);
						_errHandler.sync(this);
						_la = _input.LA(1);
					}
					setState(249);
					metadata();
					}
					}
				}
				setState(254);
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
			setState(256);
			_errHandler.sync(this);
			_alt = 1;
			do {
				switch (_alt) {
				case 1:
					{
					{
					setState(255);
					metadata();
					}
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				setState(258);
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
		public List<ArgumentValueContext> argumentValue() {
			return getRuleContexts(ArgumentValueContext.class);
		}
		public ArgumentValueContext argumentValue(int i) {
			return getRuleContext(ArgumentValueContext.class,i);
		}
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
			int _alt;
			setState(272);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(260);
				match(METADATA_PREFIX);
				setState(261);
				match(IDENTIFIER);
				setState(265);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,12,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(262);
						argumentValue();
						}
						}
					}
					setState(267);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,12,_ctx);
				}
				setState(269);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==BEGIN_ARGUMENT || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
					{
					setState(268);
					valueList();
					}
				}

				}
				break;
			case BEGIN_METADATA_VALUE:
				enterOuterAlt(_localctx, 2);
				{
				setState(271);
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
			setState(274);
			match(BEGIN_METADATA_VALUE);
			setState(275);
			value();
			setState(276);
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
			setState(281);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_DERIVATION) {
				{
				{
				setState(278);
				mixinModifier();
				}
				}
				setState(283);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(284);
			match(KEYWORD_MIXIN);
			setState(285);
			mixinIdentifier();
			setState(287);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(286);
				patternParameterList();
				}
			}

			setState(289);
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
		public TerminalNode KEYWORD_TRAIT() { return getToken(HixParser.KEYWORD_TRAIT, 0); }
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
			setState(292);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_TRAIT) {
				{
				setState(291);
				match(KEYWORD_TRAIT);
				}
			}

			setState(294);
			match(KEYWORD_TYPE);
			setState(295);
			match(IDENTIFIER);
			setState(296);
			match(ASSIGN);
			setState(297);
			patternExpression();
			setState(299);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==SEMICOLON) {
				{
				setState(298);
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
	public static class MetaDeclarationContext extends ParserRuleContext {
		public TerminalNode KEYWORD_META() { return getToken(HixParser.KEYWORD_META, 0); }
		public PatternParameterListContext patternParameterList() {
			return getRuleContext(PatternParameterListContext.class,0);
		}
		public TerminalNode KEYWORD_FOR() { return getToken(HixParser.KEYWORD_FOR, 0); }
		public List<MetaTargetContext> metaTarget() {
			return getRuleContexts(MetaTargetContext.class);
		}
		public MetaTargetContext metaTarget(int i) {
			return getRuleContext(MetaTargetContext.class,i);
		}
		public TerminalNode SEMICOLON() { return getToken(HixParser.SEMICOLON, 0); }
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public TerminalNode KEYWORD_TRAIT() { return getToken(HixParser.KEYWORD_TRAIT, 0); }
		public List<MetaModifierContext> metaModifier() {
			return getRuleContexts(MetaModifierContext.class);
		}
		public MetaModifierContext metaModifier(int i) {
			return getRuleContext(MetaModifierContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(HixParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(HixParser.COMMA, i);
		}
		public MetaDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_metaDeclaration; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitMetaDeclaration(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MetaDeclarationContext metaDeclaration() throws RecognitionException {
		MetaDeclarationContext _localctx = new MetaDeclarationContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_metaDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(304);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 63)) & ~0x3f) == 0 && ((1L << (_la - 63)) & 7L) != 0)) {
				{
				{
				setState(301);
				metaModifier();
				}
				}
				setState(306);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(307);
			match(KEYWORD_META);
			setState(308);
			_la = _input.LA(1);
			if ( !(_la==IDENTIFIER || _la==KEYWORD_TRAIT) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			setState(309);
			patternParameterList();
			setState(310);
			match(KEYWORD_FOR);
			setState(311);
			metaTarget();
			setState(318);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1728432278864896L) != 0)) {
				{
				{
				setState(313);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==COMMA) {
					{
					setState(312);
					match(COMMA);
					}
				}

				setState(315);
				metaTarget();
				}
				}
				setState(320);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(321);
			match(SEMICOLON);
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
	public static class MetaTargetContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public TerminalNode KEYWORD_TYPE() { return getToken(HixParser.KEYWORD_TYPE, 0); }
		public TerminalNode KEYWORD_MIXIN() { return getToken(HixParser.KEYWORD_MIXIN, 0); }
		public TerminalNode KEYWORD_EXPRESSION() { return getToken(HixParser.KEYWORD_EXPRESSION, 0); }
		public MetaTargetContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_metaTarget; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitMetaTarget(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MetaTargetContext metaTarget() throws RecognitionException {
		MetaTargetContext _localctx = new MetaTargetContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_metaTarget);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(323);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 1724034232353792L) != 0)) ) {
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
		enterRule(_localctx, 22, RULE_mixinBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(325);
			match(LC);
			setState(331);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 35)) & ~0x3f) == 0 && ((1L << (_la - 35)) & 12952092935L) != 0)) {
				{
				setState(329);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_EXPRESSION:
				case KEYWORD_PRELUDE:
				case KEYWORD_STRICT:
					{
					setState(326);
					expressionDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_NOINLINE:
					{
					setState(327);
					funcDeclaration();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(328);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(333);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(334);
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
		enterRule(_localctx, 24, RULE_expressionDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(339);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_PRELUDE || _la==KEYWORD_STRICT) {
				{
				{
				setState(336);
				expressionModifier();
				}
				}
				setState(341);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(342);
			match(KEYWORD_EXPRESSION);
			setState(343);
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
		enterRule(_localctx, 26, RULE_funcDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(348);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_PURE || _la==KEYWORD_NOINLINE) {
				{
				{
				setState(345);
				funcModifier();
				}
				}
				setState(350);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(351);
			match(KEYWORD_FUNC);
			setState(352);
			functionDeclarationIdentifier();
			setState(354);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BEGIN_VALUE_INLINE || ((((_la - 72)) & ~0x3f) == 0 && ((1L << (_la - 72)) & 13L) != 0)) {
				{
				setState(353);
				directFunctionSignature();
				}
			}

			setState(359);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 240518168576L) != 0)) {
				{
				{
				setState(356);
				trivia();
				}
				}
				setState(361);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(362);
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
		public OutputSignatureFieldsContext outputSignatureFields() {
			return getRuleContext(OutputSignatureFieldsContext.class,0);
		}
		public PatternParameterListContext patternParameterList() {
			return getRuleContext(PatternParameterListContext.class,0);
		}
		public CollectiveInputPatternContext collectiveInputPattern() {
			return getRuleContext(CollectiveInputPatternContext.class,0);
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
		enterRule(_localctx, 28, RULE_directFunctionSignature);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(366);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case EMPTY_PARAMETERS:
			case BEGIN_PARAMETERS:
				{
				setState(364);
				patternParameterList();
				}
				break;
			case BEGIN_VALUE_INLINE:
				{
				setState(365);
				collectiveInputPattern();
				}
				break;
			case ARROW:
				break;
			default:
				break;
			}
			setState(368);
			match(ARROW);
			setState(371);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
			case NULL:
			case MISSING:
			case KEYWORD_DELEGATE:
				{
				setState(369);
				patternExpression();
				}
				break;
			case EMPTY_PARAMETERS:
			case BEGIN_PARAMETERS:
				{
				setState(370);
				outputSignatureFields();
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
	public static class CollectiveInputPatternContext extends ParserRuleContext {
		public TerminalNode BEGIN_VALUE_INLINE() { return getToken(HixParser.BEGIN_VALUE_INLINE, 0); }
		public PatternExpressionContext patternExpression() {
			return getRuleContext(PatternExpressionContext.class,0);
		}
		public TerminalNode VALUE_END_INLINE() { return getToken(HixParser.VALUE_END_INLINE, 0); }
		public CollectiveInputPatternContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_collectiveInputPattern; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitCollectiveInputPattern(this);
			else return visitor.visitChildren(this);
		}
	}

	public final CollectiveInputPatternContext collectiveInputPattern() throws RecognitionException {
		CollectiveInputPatternContext _localctx = new CollectiveInputPatternContext(_ctx, getState());
		enterRule(_localctx, 30, RULE_collectiveInputPattern);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(373);
			match(BEGIN_VALUE_INLINE);
			setState(374);
			patternExpression();
			setState(375);
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
	public static class OutputSignatureFieldsContext extends ParserRuleContext {
		public PatternParameterListContext patternParameterList() {
			return getRuleContext(PatternParameterListContext.class,0);
		}
		public OutputSignatureFieldsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_outputSignatureFields; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitOutputSignatureFields(this);
			else return visitor.visitChildren(this);
		}
	}

	public final OutputSignatureFieldsContext outputSignatureFields() throws RecognitionException {
		OutputSignatureFieldsContext _localctx = new OutputSignatureFieldsContext(_ctx, getState());
		enterRule(_localctx, 32, RULE_outputSignatureFields);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(377);
			patternParameterList();
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
		public TerminalNode KEYWORD_EXTERNAL() { return getToken(HixParser.KEYWORD_EXTERNAL, 0); }
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
		public TerminalNode SEMICOLON() { return getToken(HixParser.SEMICOLON, 0); }
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
		enterRule(_localctx, 34, RULE_functionBody);
		int _la;
		try {
			setState(395);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case LC:
			case KEYWORD_DO:
				enterOuterAlt(_localctx, 1);
				{
				setState(380);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_DO) {
					{
					setState(379);
					match(KEYWORD_DO);
					}
				}

				setState(382);
				statementBlock();
				}
				break;
			case FAT_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(383);
				match(FAT_ARROW);
				setState(384);
				value();
				setState(386);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_END) {
					{
					setState(385);
					match(VALUE_END);
					}
				}

				}
				break;
			case KEYWORD_EXTERNAL:
				enterOuterAlt(_localctx, 3);
				{
				setState(388);
				match(KEYWORD_EXTERNAL);
				setState(390);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==IDENTIFIER) {
					{
					setState(389);
					match(IDENTIFIER);
					}
				}

				setState(393);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==SEMICOLON) {
					{
					setState(392);
					match(SEMICOLON);
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
		enterRule(_localctx, 36, RULE_patternExpression);
		try {
			setState(402);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(397);
				inlineMetadataList();
				setState(399);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,35,_ctx) ) {
				case 1:
					{
					setState(398);
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
				setState(401);
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
		enterRule(_localctx, 38, RULE_patternUnion);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(404);
			patternTerm();
			setState(409);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,37,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(405);
					match(PIPE);
					setState(406);
					patternTerm();
					}
					}
				}
				setState(411);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,37,_ctx);
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
		enterRule(_localctx, 40, RULE_patternTerm);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(412);
			patternPrimary();
			setState(414);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,38,_ctx) ) {
			case 1:
				{
				setState(413);
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
		enterRule(_localctx, 42, RULE_patternPrimary);
		try {
			setState(420);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case NULL:
			case MISSING:
				enterOuterAlt(_localctx, 1);
				{
				setState(416);
				patternIdentifier();
				}
				break;
			case BEGIN_TABLE:
				enterOuterAlt(_localctx, 2);
				{
				setState(417);
				tablePattern();
				}
				break;
			case BEGIN_TUPLE:
				enterOuterAlt(_localctx, 3);
				{
				setState(418);
				tuplePattern();
				}
				break;
			case KEYWORD_DELEGATE:
				enterOuterAlt(_localctx, 4);
				{
				setState(419);
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
		enterRule(_localctx, 44, RULE_tablePattern);
		int _la;
		try {
			int _alt;
			setState(438);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,42,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(422);
				match(BEGIN_TABLE);
				setState(423);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(424);
				match(BEGIN_TABLE);
				setState(425);
				patternField();
				setState(430);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,40,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(426);
						match(VALUE_DELIMITER);
						setState(427);
						patternField();
						}
						}
					}
					setState(432);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,40,_ctx);
				}
				setState(434);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(433);
					match(VALUE_DELIMITER);
					}
				}

				setState(436);
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
		enterRule(_localctx, 46, RULE_tuplePattern);
		int _la;
		try {
			int _alt;
			setState(456);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,45,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(440);
				match(BEGIN_TUPLE);
				setState(441);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(442);
				match(BEGIN_TUPLE);
				setState(443);
				patternField();
				setState(448);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,43,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(444);
						match(VALUE_DELIMITER);
						setState(445);
						patternField();
						}
						}
					}
					setState(450);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,43,_ctx);
				}
				setState(452);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(451);
					match(VALUE_DELIMITER);
					}
				}

				setState(454);
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
		enterRule(_localctx, 48, RULE_delegatePattern);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(458);
			match(KEYWORD_DELEGATE);
			setState(459);
			patternParameterList();
			setState(460);
			match(ARROW);
			setState(461);
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
		enterRule(_localctx, 50, RULE_patternParameterList);
		int _la;
		try {
			int _alt;
			setState(480);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,48,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(463);
				match(BEGIN_PARAMETERS);
				setState(464);
				patternField();
				setState(469);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,46,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(465);
						match(VALUE_DELIMITER);
						setState(466);
						patternField();
						}
						}
					}
					setState(471);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,46,_ctx);
				}
				setState(473);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(472);
					match(VALUE_DELIMITER);
					}
				}

				setState(475);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(477);
				match(BEGIN_PARAMETERS);
				setState(478);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(479);
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
		public TerminalNode VALUE_ASSIGN() { return getToken(HixParser.VALUE_ASSIGN, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public TerminalNode ROOT_IDENTIFIER() { return getToken(HixParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode STRING() { return getToken(HixParser.STRING, 0); }
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
		enterRule(_localctx, 52, RULE_patternField);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(483);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				setState(482);
				metadataList();
				}
			}

			setState(485);
			patternUnion();
			setState(487);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ROOT_IDENTIFIER || _la==STRING) {
				{
				setState(486);
				_la = _input.LA(1);
				if ( !(_la==ROOT_IDENTIFIER || _la==STRING) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				}
			}

			setState(491);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==VALUE_ASSIGN) {
				{
				setState(489);
				match(VALUE_ASSIGN);
				setState(490);
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
		enterRule(_localctx, 54, RULE_patternIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(493);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 805315584L) != 0)) ) {
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
		enterRule(_localctx, 56, RULE_statementBlock);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(495);
			match(LC);
			setState(501);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 2287831601053165056L) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & 10743709715L) != 0)) {
				{
				setState(499);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_VALUE_INLINE:
				case IDENTIFIER:
				case ROOT_IDENTIFIER:
				case BEGIN_TABLE:
				case BEGIN_TUPLE:
				case BEGIN_LAMBDA_BLOCK:
				case BEGIN_LAMBDA_ARROW:
				case BEGIN_METADATA_VALUE:
				case METADATA_PREFIX:
				case EXPLICIT_ROOT:
				case BEGIN_INLINE_BLOCK:
				case NUMBER:
				case STRING:
				case BOOLEAN:
				case NULL:
				case MISSING:
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
				case TOPLEVEL_VALUE_EXPRESSION:
				case VALUE_SMART_ROOT:
				case NOT_VALUE:
				case FUNCTION_IDENTIFIER:
				case LABEL_IDENTIFIER:
					{
					setState(496);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(497);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(498);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(503);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(504);
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
		enterRule(_localctx, 58, RULE_statement);
		int _la;
		try {
			setState(516);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(506);
				metadataList();
				setState(510);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 240518168576L) != 0)) {
					{
					{
					setState(507);
					trivia();
					}
					}
					setState(512);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(513);
				statementBody();
				}
				break;
			case BEGIN_VALUE_INLINE:
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case BEGIN_LAMBDA_BLOCK:
			case BEGIN_LAMBDA_ARROW:
			case EXPLICIT_ROOT:
			case BEGIN_INLINE_BLOCK:
			case NUMBER:
			case STRING:
			case BOOLEAN:
			case NULL:
			case MISSING:
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
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
			case LABEL_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(515);
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
		public ValueExpressionContext valueExpression() {
			return getRuleContext(ValueExpressionContext.class,0);
		}
		public NonArgumentValueContext nonArgumentValue() {
			return getRuleContext(NonArgumentValueContext.class,0);
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
		enterRule(_localctx, 60, RULE_statementBody);
		int _la;
		try {
			setState(535);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,57,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(518);
				labelIdentifier();
				setState(519);
				match(NEWLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(522);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LABEL_PREFIX) {
					{
					setState(521);
					labelIdentifier();
					}
				}

				setState(524);
				statementBlock();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(525);
				invocationStatement();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(526);
				localDeclarationStatement();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(527);
				localAssignmentStatement();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(528);
				toplevelDerivationStatement();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(529);
				controlflowStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(530);
				whenValueStatement();
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(531);
				whenConditionStatement();
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(532);
				whenChainStatement();
				}
				break;
			case 11:
				enterOuterAlt(_localctx, 11);
				{
				setState(533);
				valueExpression();
				}
				break;
			case 12:
				enterOuterAlt(_localctx, 12);
				{
				setState(534);
				nonArgumentValue(0);
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
		enterRule(_localctx, 62, RULE_invocationStatement);
		try {
			setState(545);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,59,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(537);
				invocationIdentifier();
				setState(538);
				callValueList();
				setState(540);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,58,_ctx) ) {
				case 1:
					{
					setState(539);
					tailValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(542);
				invocationIdentifier();
				setState(543);
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
		enterRule(_localctx, 64, RULE_whenConditionStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(547);
			match(KEYWORD_WHEN);
			setState(548);
			whenChainCondition();
			setState(549);
			statementBlock();
			setState(560);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,61,_ctx) ) {
			case 1:
				{
				setState(553);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 240518168576L) != 0)) {
					{
					{
					setState(550);
					trivia();
					}
					}
					setState(555);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(556);
				match(KEYWORD_ELSE);
				setState(557);
				whenResult();
				setState(558);
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
		enterRule(_localctx, 66, RULE_whenElseBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(562);
			match(KEYWORD_ELSE);
			setState(563);
			match(ARROW);
			setState(564);
			whenResult();
			setState(565);
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
		enterRule(_localctx, 68, RULE_whenResult);
		try {
			setState(569);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,62,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(567);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(568);
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
		enterRule(_localctx, 70, RULE_whenChainCondition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(571);
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
		enterRule(_localctx, 72, RULE_whenChainStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(573);
			match(KEYWORD_WHEN);
			setState(574);
			match(LC);
			setState(575);
			match(NEWLINE);
			setState(576);
			whenChainBody();
			setState(577);
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
		enterRule(_localctx, 74, RULE_whenChainBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(579);
			whenChainBranch();
			setState(584);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 240518168592L) != 0) || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(582);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case EMPTY_PARAMETERS:
				case BEGIN_PARAMETERS:
					{
					setState(580);
					whenChainBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(581);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(586);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(588);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(587);
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
		enterRule(_localctx, 76, RULE_whenChainBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(590);
			whenChainCondition();
			setState(591);
			match(ARROW);
			setState(592);
			whenResult();
			setState(593);
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
		enterRule(_localctx, 78, RULE_whenValueStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(595);
			match(KEYWORD_WHEN);
			setState(597);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1050927632L) != 0) || ((((_la - 73)) & ~0x3f) == 0 && ((1L << (_la - 73)) & 671481857L) != 0)) {
				{
				setState(596);
				value();
				}
			}

			setState(599);
			match(LC);
			setState(600);
			match(NEWLINE);
			setState(601);
			whenValueBody();
			setState(602);
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
		enterRule(_localctx, 80, RULE_whenValueBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(607);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 240518168576L) != 0)) {
				{
				{
				setState(604);
				trivia();
				}
				}
				setState(609);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(610);
			whenValueBranch();
			setState(615);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 241569096208L) != 0) || ((((_la - 73)) & ~0x3f) == 0 && ((1L << (_la - 73)) & 671481857L) != 0)) {
				{
				setState(613);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case BEGIN_VALUE_INLINE:
				case ROOT_IDENTIFIER:
				case BEGIN_TABLE:
				case BEGIN_TUPLE:
				case BEGIN_LAMBDA_BLOCK:
				case BEGIN_LAMBDA_ARROW:
				case EXPLICIT_ROOT:
				case BEGIN_INLINE_BLOCK:
				case NUMBER:
				case STRING:
				case BOOLEAN:
				case NULL:
				case MISSING:
				case TOPLEVEL_VALUE_EXPRESSION:
				case VALUE_SMART_ROOT:
				case NOT_VALUE:
				case FUNCTION_IDENTIFIER:
				case LABEL_IDENTIFIER:
					{
					setState(611);
					whenValueBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(612);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(617);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(619);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(618);
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
		enterRule(_localctx, 82, RULE_whenValueBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(621);
			whenValueCondition();
			setState(622);
			match(ARROW);
			setState(623);
			whenResult();
			setState(624);
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
		enterRule(_localctx, 84, RULE_whenValueCondition);
		try {
			setState(628);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,71,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(626);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(627);
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
		enterRule(_localctx, 86, RULE_localDeclarationStatement);
		int _la;
		try {
			setState(647);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,76,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(631);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(630);
					match(KEYWORD_CARRY);
					}
				}

				setState(633);
				match(KEYWORD_LOCAL);
				setState(634);
				variableIdentifier();
				setState(636);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,73,_ctx) ) {
				case 1:
					{
					setState(635);
					assignedValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(639);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(638);
					match(KEYWORD_CARRY);
					}
				}

				setState(641);
				match(KEYWORD_LOCAL);
				setState(642);
				patternExpression();
				setState(643);
				variableIdentifier();
				setState(645);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,75,_ctx) ) {
				case 1:
					{
					setState(644);
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
		enterRule(_localctx, 88, RULE_localAssignmentStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(649);
			match(VALUE_SMART_ROOT);
			setState(650);
			variableIdentifier();
			setState(651);
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
		enterRule(_localctx, 90, RULE_toplevelDerivationStatement);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(653);
			match(TOPLEVEL_DERIVATION);
			setState(654);
			functionIdentifier();
			setState(656);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BEGIN_ARGUMENT || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(655);
				callValueList();
				}
			}

			setState(661);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,78,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(658);
					transformationPart();
					}
					}
				}
				setState(663);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,78,_ctx);
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
		enterRule(_localctx, 92, RULE_assignedValue);
		try {
			setState(672);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ASSIGN:
				enterOuterAlt(_localctx, 1);
				{
				setState(664);
				match(ASSIGN);
				setState(669);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,79,_ctx) ) {
				case 1:
					{
					setState(665);
					value();
					}
					break;
				case 2:
					{
					setState(666);
					whenValueStatement();
					}
					break;
				case 3:
					{
					setState(667);
					whenChainStatement();
					}
					break;
				case 4:
					{
					setState(668);
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
			case EXPLICIT_ROOT:
			case BEGIN_INLINE_BLOCK:
			case NUMBER:
			case STRING:
			case BOOLEAN:
			case NULL:
			case MISSING:
			case NEWLINE:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
			case LABEL_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(671);
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
		enterRule(_localctx, 94, RULE_controlflowStatement);
		int _la;
		try {
			setState(682);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_RETURN:
				enterOuterAlt(_localctx, 1);
				{
				setState(674);
				match(KEYWORD_RETURN);
				setState(676);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==BEGIN_ARGUMENT || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
					{
					setState(675);
					valueList();
					}
				}

				}
				break;
			case KEYWORD_GOTO:
				enterOuterAlt(_localctx, 2);
				{
				setState(678);
				match(KEYWORD_GOTO);
				setState(679);
				match(IDENTIFIER);
				}
				break;
			case KEYWORD_CONTINUE:
				enterOuterAlt(_localctx, 3);
				{
				setState(680);
				match(KEYWORD_CONTINUE);
				}
				break;
			case KEYWORD_BREAK:
				enterOuterAlt(_localctx, 4);
				{
				setState(681);
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
		enterRule(_localctx, 96, RULE_contentBlock);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(684);
			match(BEGIN_CONTENT);
			setState(685);
			contentBody();
			setState(686);
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
		enterRule(_localctx, 98, RULE_contentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(692);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				setState(692);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case CONTENT_WRAP:
					{
					setState(688);
					match(CONTENT_WRAP);
					}
					break;
				case CONTENT_LINEBREAK:
					{
					setState(689);
					match(CONTENT_LINEBREAK);
					}
					break;
				case CONTENT_TEXT:
					{
					setState(690);
					match(CONTENT_TEXT);
					}
					break;
				case BEGIN_VALUE_INTERPOLATE:
					{
					setState(691);
					contentInterpolate();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(694);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( _la==BEGIN_VALUE_INTERPOLATE || ((((_la - 80)) & ~0x3f) == 0 && ((1L << (_la - 80)) & 7L) != 0) );
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
		enterRule(_localctx, 100, RULE_contentInterpolate);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(696);
			match(BEGIN_VALUE_INTERPOLATE);
			setState(697);
			derivation();
			setState(698);
			match(RC);
			setState(700);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==END_CONTENT_INTERPOLATE) {
				{
				setState(699);
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
		enterRule(_localctx, 102, RULE_value);
		try {
			setState(704);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case BEGIN_LAMBDA_BLOCK:
			case BEGIN_LAMBDA_ARROW:
			case EXPLICIT_ROOT:
			case BEGIN_INLINE_BLOCK:
			case NUMBER:
			case STRING:
			case BOOLEAN:
			case NULL:
			case MISSING:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
			case LABEL_IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(702);
				nonArgumentValue(0);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(703);
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
		enterRule(_localctx, 104, RULE_valueExpression);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(706);
			match(TOPLEVEL_VALUE_EXPRESSION);
			setState(707);
			value();
			setState(708);
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
		public List<TransformationPartContext> transformationPart() {
			return getRuleContexts(TransformationPartContext.class);
		}
		public TransformationPartContext transformationPart(int i) {
			return getRuleContext(TransformationPartContext.class,i);
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
		int _startState = 106;
		enterRecursionRule(_localctx, 106, RULE_nonArgumentValue, _p);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(724);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case BEGIN_LAMBDA_BLOCK:
			case BEGIN_LAMBDA_ARROW:
			case EXPLICIT_ROOT:
			case BEGIN_INLINE_BLOCK:
			case NUMBER:
			case STRING:
			case BOOLEAN:
			case NULL:
			case MISSING:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case FUNCTION_IDENTIFIER:
			case LABEL_IDENTIFIER:
				{
				setState(711);
				primaryValue();
				setState(715);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,87,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(712);
						transformationPart();
						}
						}
					}
					setState(717);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,87,_ctx);
				}
				setState(719);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,88,_ctx) ) {
				case 1:
					{
					setState(718);
					elvisValue();
					}
					break;
				}
				}
				break;
			case NOT_VALUE:
				{
				setState(721);
				prefixOperators();
				setState(722);
				nonArgumentValue(2);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(730);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,90,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					{
					_localctx = new NonArgumentValueContext(_parentctx, _parentState);
					pushNewRecursionContext(_localctx, _startState, RULE_nonArgumentValue);
					setState(726);
					if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
					setState(727);
					postfixOperators();
					}
					}
				}
				setState(732);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,90,_ctx);
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
		public InlineBlockValueContext inlineBlockValue() {
			return getRuleContext(InlineBlockValueContext.class,0);
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
		public TerminalNode STRING() { return getToken(HixParser.STRING, 0); }
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
		enterRule(_localctx, 108, RULE_primaryValue);
		try {
			setState(746);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,91,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(733);
				inlineValue();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(734);
				lambdaValue();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(735);
				inlineBlockValue();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(736);
				derivation();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(737);
				tableValue();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(738);
				tupleValue();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(739);
				valueExpression();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(740);
				valueStatement();
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(741);
				match(NUMBER);
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(742);
				match(STRING);
				}
				break;
			case 11:
				enterOuterAlt(_localctx, 11);
				{
				setState(743);
				match(BOOLEAN);
				}
				break;
			case 12:
				enterOuterAlt(_localctx, 12);
				{
				setState(744);
				match(NULL);
				}
				break;
			case 13:
				enterOuterAlt(_localctx, 13);
				{
				setState(745);
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
	public static class InlineBlockValueContext extends ParserRuleContext {
		public TerminalNode BEGIN_INLINE_BLOCK() { return getToken(HixParser.BEGIN_INLINE_BLOCK, 0); }
		public TerminalNode RC() { return getToken(HixParser.RC, 0); }
		public TerminalNode FAT_ARROW() { return getToken(HixParser.FAT_ARROW, 0); }
		public DerivationContext derivation() {
			return getRuleContext(DerivationContext.class,0);
		}
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
		public InlineBlockValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_inlineBlockValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitInlineBlockValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final InlineBlockValueContext inlineBlockValue() throws RecognitionException {
		InlineBlockValueContext _localctx = new InlineBlockValueContext(_ctx, getState());
		enterRule(_localctx, 110, RULE_inlineBlockValue);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(748);
			match(BEGIN_INLINE_BLOCK);
			setState(754);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 2287831601053165056L) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & 10743709715L) != 0)) {
				{
				setState(752);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_VALUE_INLINE:
				case IDENTIFIER:
				case ROOT_IDENTIFIER:
				case BEGIN_TABLE:
				case BEGIN_TUPLE:
				case BEGIN_LAMBDA_BLOCK:
				case BEGIN_LAMBDA_ARROW:
				case BEGIN_METADATA_VALUE:
				case METADATA_PREFIX:
				case EXPLICIT_ROOT:
				case BEGIN_INLINE_BLOCK:
				case NUMBER:
				case STRING:
				case BOOLEAN:
				case NULL:
				case MISSING:
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
				case TOPLEVEL_VALUE_EXPRESSION:
				case VALUE_SMART_ROOT:
				case NOT_VALUE:
				case FUNCTION_IDENTIFIER:
				case LABEL_IDENTIFIER:
					{
					setState(749);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(750);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(751);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(756);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(757);
			match(RC);
			setState(758);
			match(FAT_ARROW);
			setState(759);
			derivation();
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
		enterRule(_localctx, 112, RULE_lambdaValue);
		int _la;
		try {
			setState(776);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_LAMBDA_BLOCK:
				enterOuterAlt(_localctx, 1);
				{
				setState(761);
				match(BEGIN_LAMBDA_BLOCK);
				setState(767);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 2287831601053165056L) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & 10743709715L) != 0)) {
					{
					setState(765);
					_errHandler.sync(this);
					switch (_input.LA(1)) {
					case BEGIN_VALUE_INLINE:
					case IDENTIFIER:
					case ROOT_IDENTIFIER:
					case BEGIN_TABLE:
					case BEGIN_TUPLE:
					case BEGIN_LAMBDA_BLOCK:
					case BEGIN_LAMBDA_ARROW:
					case BEGIN_METADATA_VALUE:
					case METADATA_PREFIX:
					case EXPLICIT_ROOT:
					case BEGIN_INLINE_BLOCK:
					case NUMBER:
					case STRING:
					case BOOLEAN:
					case NULL:
					case MISSING:
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
					case TOPLEVEL_VALUE_EXPRESSION:
					case VALUE_SMART_ROOT:
					case NOT_VALUE:
					case FUNCTION_IDENTIFIER:
					case LABEL_IDENTIFIER:
						{
						setState(762);
						statement();
						}
						break;
					case SEMICOLON:
						{
						setState(763);
						match(SEMICOLON);
						}
						break;
					case NEWLINE:
					case COMMENT:
					case SLASH_COMMENT:
						{
						setState(764);
						trivia();
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					}
					setState(769);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(770);
				match(RC);
				}
				break;
			case BEGIN_LAMBDA_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(771);
				match(BEGIN_LAMBDA_ARROW);
				setState(772);
				value();
				setState(774);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,96,_ctx) ) {
				case 1:
					{
					setState(773);
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
		enterRule(_localctx, 114, RULE_prefixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(778);
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
		public TerminalNode CAST_BOOLEAN() { return getToken(HixParser.CAST_BOOLEAN, 0); }
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
		enterRule(_localctx, 116, RULE_postfixOperators);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(780);
			_la = _input.LA(1);
			if ( !(_la==CAST_BOOLEAN || _la==QUESTION) ) {
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
	public static class ValueStatementContext extends ParserRuleContext {
		public FunctionIdentifierContext functionIdentifier() {
			return getRuleContext(FunctionIdentifierContext.class,0);
		}
		public CallValueListContext callValueList() {
			return getRuleContext(CallValueListContext.class,0);
		}
		public CallBindingContext callBinding() {
			return getRuleContext(CallBindingContext.class,0);
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
		enterRule(_localctx, 118, RULE_valueStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(782);
			functionIdentifier();
			setState(783);
			callValueList();
			setState(785);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,98,_ctx) ) {
			case 1:
				{
				setState(784);
				callBinding();
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
	public static class CallBindingContext extends ParserRuleContext {
		public TerminalNode STATIC_CALL() { return getToken(HixParser.STATIC_CALL, 0); }
		public ValueListContext valueList() {
			return getRuleContext(ValueListContext.class,0);
		}
		public CallBindingContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_callBinding; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitCallBinding(this);
			else return visitor.visitChildren(this);
		}
	}

	public final CallBindingContext callBinding() throws RecognitionException {
		CallBindingContext _localctx = new CallBindingContext(_ctx, getState());
		enterRule(_localctx, 120, RULE_callBinding);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(787);
			match(STATIC_CALL);
			setState(788);
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
		enterRule(_localctx, 122, RULE_tailValue);
		int _la;
		try {
			setState(795);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_CONTENT:
			case NEWLINE:
				enterOuterAlt(_localctx, 1);
				{
				setState(791);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==NEWLINE) {
					{
					setState(790);
					match(NEWLINE);
					}
				}

				setState(793);
				contentBlock();
				}
				break;
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case BEGIN_LAMBDA_BLOCK:
			case BEGIN_LAMBDA_ARROW:
			case EXPLICIT_ROOT:
			case BEGIN_INLINE_BLOCK:
			case NUMBER:
			case STRING:
			case BOOLEAN:
			case NULL:
			case MISSING:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
			case LABEL_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(794);
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
		enterRule(_localctx, 124, RULE_tupleValue);
		int _la;
		try {
			int _alt;
			setState(813);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,103,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(797);
				match(BEGIN_TUPLE);
				setState(798);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(799);
				match(BEGIN_TUPLE);
				setState(800);
				value();
				setState(805);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,101,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(801);
						match(VALUE_DELIMITER);
						setState(802);
						value();
						}
						}
					}
					setState(807);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,101,_ctx);
				}
				setState(809);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(808);
					match(VALUE_DELIMITER);
					}
				}

				setState(811);
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
		enterRule(_localctx, 126, RULE_tableValue);
		int _la;
		try {
			int _alt;
			setState(831);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,106,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(815);
				match(BEGIN_TABLE);
				setState(816);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(817);
				match(BEGIN_TABLE);
				setState(818);
				tableKeyedEntry();
				setState(823);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,104,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(819);
						match(VALUE_DELIMITER);
						setState(820);
						tableKeyedEntry();
						}
						}
					}
					setState(825);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,104,_ctx);
				}
				setState(827);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(826);
					match(VALUE_DELIMITER);
					}
				}

				setState(829);
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
		public TerminalNode STRING() { return getToken(HixParser.STRING, 0); }
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
		enterRule(_localctx, 128, RULE_tableKeyedEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(836);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				{
				setState(833);
				metadata();
				}
				}
				setState(838);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(839);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 603987968L) != 0)) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			setState(840);
			match(VALUE_ASSIGN);
			setState(841);
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
		enterRule(_localctx, 130, RULE_valueList);
		int _la;
		try {
			int _alt;
			setState(865);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,111,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(843);
				match(BEGIN_PARAMETERS);
				setState(844);
				value();
				setState(849);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,108,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(845);
						match(VALUE_DELIMITER);
						setState(846);
						value();
						}
						}
					}
					setState(851);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,108,_ctx);
				}
				setState(853);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(852);
					match(VALUE_DELIMITER);
					}
				}

				setState(855);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(857);
				match(BEGIN_PARAMETERS);
				setState(858);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(859);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(861);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(860);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(863);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,110,_ctx);
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
		enterRule(_localctx, 132, RULE_callValueList);
		int _la;
		try {
			int _alt;
			setState(889);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,115,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(867);
				match(BEGIN_PARAMETERS);
				setState(868);
				callArgument();
				setState(873);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,112,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(869);
						match(VALUE_DELIMITER);
						setState(870);
						callArgument();
						}
						}
					}
					setState(875);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,112,_ctx);
				}
				setState(877);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(876);
					match(VALUE_DELIMITER);
					}
				}

				setState(879);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(881);
				match(BEGIN_PARAMETERS);
				setState(882);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(883);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(885);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(884);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(887);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,114,_ctx);
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
		public TerminalNode STRING() { return getToken(HixParser.STRING, 0); }
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
		enterRule(_localctx, 134, RULE_callArgument);
		int _la;
		try {
			setState(895);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,116,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(891);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 603987968L) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(892);
				match(VALUE_ASSIGN);
				setState(893);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(894);
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
		enterRule(_localctx, 136, RULE_argumentValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(897);
			match(BEGIN_ARGUMENT);
			setState(898);
			argumentBody();
			setState(899);
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
		enterRule(_localctx, 138, RULE_argumentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(906);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==ESCAPE || _la==BEGIN_VALUE_INLINE || _la==ARGUMENT_TEXT) {
				{
				setState(904);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case ARGUMENT_TEXT:
					{
					setState(901);
					match(ARGUMENT_TEXT);
					}
					break;
				case ESCAPE:
					{
					setState(902);
					escaped();
					}
					break;
				case BEGIN_VALUE_INLINE:
					{
					setState(903);
					inlineValue();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(908);
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
		public TerminalNode VALUE_END_INLINE() { return getToken(HixParser.VALUE_END_INLINE, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
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
		enterRule(_localctx, 140, RULE_inlineValue);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(909);
			match(BEGIN_VALUE_INLINE);
			setState(911);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1050927632L) != 0) || ((((_la - 73)) & ~0x3f) == 0 && ((1L << (_la - 73)) & 671481857L) != 0)) {
				{
				setState(910);
				value();
				}
			}

			setState(913);
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
		enterRule(_localctx, 142, RULE_inlineTransformation);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(915);
			match(BEGIN_VALUE_INLINE);
			setState(917);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(916);
				transformationPart();
				}
				}
				setState(919);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & 245761L) != 0) );
			setState(921);
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
		enterRule(_localctx, 144, RULE_derivation);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(923);
			derivationRoot();
			setState(927);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,121,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(924);
					transformationPart();
					}
					}
				}
				setState(929);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,121,_ctx);
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
	public static class ExplicitRootContext extends ParserRuleContext {
		public TerminalNode EXPLICIT_ROOT() { return getToken(HixParser.EXPLICIT_ROOT, 0); }
		public ValueListContext valueList() {
			return getRuleContext(ValueListContext.class,0);
		}
		public ExplicitRootContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_explicitRoot; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitExplicitRoot(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ExplicitRootContext explicitRoot() throws RecognitionException {
		ExplicitRootContext _localctx = new ExplicitRootContext(_ctx, getState());
		enterRule(_localctx, 146, RULE_explicitRoot);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(930);
			match(EXPLICIT_ROOT);
			setState(931);
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
	public static class DerivationRootContext extends ParserRuleContext {
		public ExplicitRootContext explicitRoot() {
			return getRuleContext(ExplicitRootContext.class,0);
		}
		public TerminalNode ROOT_IDENTIFIER() { return getToken(HixParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode VALUE_SMART_ROOT() { return getToken(HixParser.VALUE_SMART_ROOT, 0); }
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
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
		enterRule(_localctx, 148, RULE_derivationRoot);
		int _la;
		try {
			setState(937);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case EXPLICIT_ROOT:
				enterOuterAlt(_localctx, 1);
				{
				setState(933);
				explicitRoot();
				}
				break;
			case ROOT_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(934);
				match(ROOT_IDENTIFIER);
				}
				break;
			case VALUE_SMART_ROOT:
				enterOuterAlt(_localctx, 3);
				{
				setState(935);
				match(VALUE_SMART_ROOT);
				setState(936);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 570434560L) != 0)) ) {
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
		enterRule(_localctx, 150, RULE_elvisValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(939);
			match(VALUE_ELVIS);
			setState(940);
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
		public CallBindingContext callBinding() {
			return getRuleContext(CallBindingContext.class,0);
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
		enterRule(_localctx, 152, RULE_transformationPart);
		try {
			setState(952);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case LABEL_PREFIX:
			case VALUE_FUNCTION:
			case VALUE_PREDICATE:
				enterOuterAlt(_localctx, 1);
				{
				setState(942);
				functionChainType();
				setState(943);
				functionIdentifier();
				setState(945);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,123,_ctx) ) {
				case 1:
					{
					setState(944);
					callValueList();
					}
					break;
				}
				setState(948);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,124,_ctx) ) {
				case 1:
					{
					setState(947);
					callBinding();
					}
					break;
				}
				}
				break;
			case VALUE_MEMBER:
				enterOuterAlt(_localctx, 2);
				{
				setState(950);
				memberIdentifier();
				}
				break;
			case VALUE_WRAP:
				enterOuterAlt(_localctx, 3);
				{
				setState(951);
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
		public TerminalNode LABEL_PREFIX() { return getToken(HixParser.LABEL_PREFIX, 0); }
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
		enterRule(_localctx, 154, RULE_functionChainType);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(954);
			_la = _input.LA(1);
			if ( !(((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & 49153L) != 0)) ) {
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
		enterRule(_localctx, 156, RULE_labelIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(956);
			match(LABEL_PREFIX);
			setState(957);
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
		enterRule(_localctx, 158, RULE_memberIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(959);
			match(VALUE_MEMBER);
			setState(960);
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
		enterRule(_localctx, 160, RULE_mixinIdentifier);
		try {
			setState(965);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(962);
				match(IDENTIFIER);
				}
				break;
			case NAMESPACE_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(963);
				match(NAMESPACE_IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 3);
				{
				setState(964);
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
		enterRule(_localctx, 162, RULE_functionDeclarationIdentifier);
		try {
			setState(969);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(967);
				match(IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(968);
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
		enterRule(_localctx, 164, RULE_variableIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(971);
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
		public TerminalNode LABEL_IDENTIFIER() { return getToken(HixParser.LABEL_IDENTIFIER, 0); }
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
		enterRule(_localctx, 166, RULE_functionIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(973);
			_la = _input.LA(1);
			if ( !(_la==ROOT_IDENTIFIER || _la==FUNCTION_IDENTIFIER || _la==LABEL_IDENTIFIER) ) {
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
		enterRule(_localctx, 168, RULE_invocationIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(975);
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
		enterRule(_localctx, 170, RULE_kindIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(977);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 805315584L) != 0)) ) {
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
		enterRule(_localctx, 172, RULE_expressionModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(979);
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
		enterRule(_localctx, 174, RULE_mixinModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(981);
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
	public static class MetaModifierContext extends ParserRuleContext {
		public TerminalNode KEYWORD_COMPILER() { return getToken(HixParser.KEYWORD_COMPILER, 0); }
		public TerminalNode KEYWORD_BYTECODE() { return getToken(HixParser.KEYWORD_BYTECODE, 0); }
		public TerminalNode KEYWORD_RUNTIME() { return getToken(HixParser.KEYWORD_RUNTIME, 0); }
		public MetaModifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_metaModifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof HixParserVisitor ) return ((HixParserVisitor<? extends T>)visitor).visitMetaModifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MetaModifierContext metaModifier() throws RecognitionException {
		MetaModifierContext _localctx = new MetaModifierContext(_ctx, getState());
		enterRule(_localctx, 176, RULE_metaModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(983);
			_la = _input.LA(1);
			if ( !(((((_la - 63)) & ~0x3f) == 0 && ((1L << (_la - 63)) & 7L) != 0)) ) {
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
	public static class FuncModifierContext extends ParserRuleContext {
		public TerminalNode KEYWORD_PURE() { return getToken(HixParser.KEYWORD_PURE, 0); }
		public TerminalNode KEYWORD_NOINLINE() { return getToken(HixParser.KEYWORD_NOINLINE, 0); }
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
		enterRule(_localctx, 178, RULE_funcModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(985);
			_la = _input.LA(1);
			if ( !(_la==KEYWORD_PURE || _la==KEYWORD_NOINLINE) ) {
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
		enterRule(_localctx, 180, RULE_trivia);
		try {
			setState(989);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case COMMENT:
			case SLASH_COMMENT:
				enterOuterAlt(_localctx, 1);
				{
				setState(987);
				comment();
				}
				break;
			case NEWLINE:
				enterOuterAlt(_localctx, 2);
				{
				setState(988);
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
		enterRule(_localctx, 182, RULE_comment);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(991);
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
		enterRule(_localctx, 184, RULE_escaped);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(993);
			match(ESCAPE);
			setState(994);
			_la = _input.LA(1);
			if ( !(((((_la - 96)) & ~0x3f) == 0 && ((1L << (_la - 96)) & 7L) != 0)) ) {
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
		case 53:
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
		"\u0004\u0001n\u03e5\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
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
		"P\u0007P\u0002Q\u0007Q\u0002R\u0007R\u0002S\u0007S\u0002T\u0007T\u0002"+
		"U\u0007U\u0002V\u0007V\u0002W\u0007W\u0002X\u0007X\u0002Y\u0007Y\u0002"+
		"Z\u0007Z\u0002[\u0007[\u0002\\\u0007\\\u0001\u0000\u0005\u0000\u00bc\b"+
		"\u0000\n\u0000\f\u0000\u00bf\t\u0000\u0001\u0000\u0003\u0000\u00c2\b\u0000"+
		"\u0001\u0000\u0001\u0000\u0005\u0000\u00c6\b\u0000\n\u0000\f\u0000\u00c9"+
		"\t\u0000\u0005\u0000\u00cb\b\u0000\n\u0000\f\u0000\u00ce\t\u0000\u0001"+
		"\u0000\u0001\u0000\u0001\u0001\u0001\u0001\u0005\u0001\u00d4\b\u0001\n"+
		"\u0001\f\u0001\u00d7\t\u0001\u0001\u0001\u0001\u0001\u0005\u0001\u00db"+
		"\b\u0001\n\u0001\f\u0001\u00de\t\u0001\u0001\u0002\u0001\u0002\u0005\u0002"+
		"\u00e2\b\u0002\n\u0002\f\u0002\u00e5\t\u0002\u0001\u0002\u0001\u0002\u0001"+
		"\u0002\u0001\u0002\u0003\u0002\u00eb\b\u0002\u0001\u0002\u0001\u0002\u0001"+
		"\u0002\u0001\u0002\u0003\u0002\u00f1\b\u0002\u0001\u0003\u0001\u0003\u0005"+
		"\u0003\u00f5\b\u0003\n\u0003\f\u0003\u00f8\t\u0003\u0001\u0003\u0005\u0003"+
		"\u00fb\b\u0003\n\u0003\f\u0003\u00fe\t\u0003\u0001\u0004\u0004\u0004\u0101"+
		"\b\u0004\u000b\u0004\f\u0004\u0102\u0001\u0005\u0001\u0005\u0001\u0005"+
		"\u0005\u0005\u0108\b\u0005\n\u0005\f\u0005\u010b\t\u0005\u0001\u0005\u0003"+
		"\u0005\u010e\b\u0005\u0001\u0005\u0003\u0005\u0111\b\u0005\u0001\u0006"+
		"\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0007\u0005\u0007\u0118\b\u0007"+
		"\n\u0007\f\u0007\u011b\t\u0007\u0001\u0007\u0001\u0007\u0001\u0007\u0003"+
		"\u0007\u0120\b\u0007\u0001\u0007\u0001\u0007\u0001\b\u0003\b\u0125\b\b"+
		"\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0003\b\u012c\b\b\u0001\t\u0005"+
		"\t\u012f\b\t\n\t\f\t\u0132\t\t\u0001\t\u0001\t\u0001\t\u0001\t\u0001\t"+
		"\u0001\t\u0003\t\u013a\b\t\u0001\t\u0005\t\u013d\b\t\n\t\f\t\u0140\t\t"+
		"\u0001\t\u0001\t\u0001\n\u0001\n\u0001\u000b\u0001\u000b\u0001\u000b\u0001"+
		"\u000b\u0005\u000b\u014a\b\u000b\n\u000b\f\u000b\u014d\t\u000b\u0001\u000b"+
		"\u0001\u000b\u0001\f\u0005\f\u0152\b\f\n\f\f\f\u0155\t\f\u0001\f\u0001"+
		"\f\u0001\f\u0001\r\u0005\r\u015b\b\r\n\r\f\r\u015e\t\r\u0001\r\u0001\r"+
		"\u0001\r\u0003\r\u0163\b\r\u0001\r\u0005\r\u0166\b\r\n\r\f\r\u0169\t\r"+
		"\u0001\r\u0001\r\u0001\u000e\u0001\u000e\u0003\u000e\u016f\b\u000e\u0001"+
		"\u000e\u0001\u000e\u0001\u000e\u0003\u000e\u0174\b\u000e\u0001\u000f\u0001"+
		"\u000f\u0001\u000f\u0001\u000f\u0001\u0010\u0001\u0010\u0001\u0011\u0003"+
		"\u0011\u017d\b\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0003"+
		"\u0011\u0183\b\u0011\u0001\u0011\u0001\u0011\u0003\u0011\u0187\b\u0011"+
		"\u0001\u0011\u0003\u0011\u018a\b\u0011\u0003\u0011\u018c\b\u0011\u0001"+
		"\u0012\u0001\u0012\u0003\u0012\u0190\b\u0012\u0001\u0012\u0003\u0012\u0193"+
		"\b\u0012\u0001\u0013\u0001\u0013\u0001\u0013\u0005\u0013\u0198\b\u0013"+
		"\n\u0013\f\u0013\u019b\t\u0013\u0001\u0014\u0001\u0014\u0003\u0014\u019f"+
		"\b\u0014\u0001\u0015\u0001\u0015\u0001\u0015\u0001\u0015\u0003\u0015\u01a5"+
		"\b\u0015\u0001\u0016\u0001\u0016\u0001\u0016\u0001\u0016\u0001\u0016\u0001"+
		"\u0016\u0005\u0016\u01ad\b\u0016\n\u0016\f\u0016\u01b0\t\u0016\u0001\u0016"+
		"\u0003\u0016\u01b3\b\u0016\u0001\u0016\u0001\u0016\u0003\u0016\u01b7\b"+
		"\u0016\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001"+
		"\u0017\u0005\u0017\u01bf\b\u0017\n\u0017\f\u0017\u01c2\t\u0017\u0001\u0017"+
		"\u0003\u0017\u01c5\b\u0017\u0001\u0017\u0001\u0017\u0003\u0017\u01c9\b"+
		"\u0017\u0001\u0018\u0001\u0018\u0001\u0018\u0001\u0018\u0001\u0018\u0001"+
		"\u0019\u0001\u0019\u0001\u0019\u0001\u0019\u0005\u0019\u01d4\b\u0019\n"+
		"\u0019\f\u0019\u01d7\t\u0019\u0001\u0019\u0003\u0019\u01da\b\u0019\u0001"+
		"\u0019\u0001\u0019\u0001\u0019\u0001\u0019\u0001\u0019\u0003\u0019\u01e1"+
		"\b\u0019\u0001\u001a\u0003\u001a\u01e4\b\u001a\u0001\u001a\u0001\u001a"+
		"\u0003\u001a\u01e8\b\u001a\u0001\u001a\u0001\u001a\u0003\u001a\u01ec\b"+
		"\u001a\u0001\u001b\u0001\u001b\u0001\u001c\u0001\u001c\u0001\u001c\u0001"+
		"\u001c\u0005\u001c\u01f4\b\u001c\n\u001c\f\u001c\u01f7\t\u001c\u0001\u001c"+
		"\u0001\u001c\u0001\u001d\u0001\u001d\u0005\u001d\u01fd\b\u001d\n\u001d"+
		"\f\u001d\u0200\t\u001d\u0001\u001d\u0001\u001d\u0001\u001d\u0003\u001d"+
		"\u0205\b\u001d\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0003\u001e"+
		"\u020b\b\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e"+
		"\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e"+
		"\u0003\u001e\u0218\b\u001e\u0001\u001f\u0001\u001f\u0001\u001f\u0003\u001f"+
		"\u021d\b\u001f\u0001\u001f\u0001\u001f\u0001\u001f\u0003\u001f\u0222\b"+
		"\u001f\u0001 \u0001 \u0001 \u0001 \u0005 \u0228\b \n \f \u022b\t \u0001"+
		" \u0001 \u0001 \u0001 \u0003 \u0231\b \u0001!\u0001!\u0001!\u0001!\u0001"+
		"!\u0001\"\u0001\"\u0003\"\u023a\b\"\u0001#\u0001#\u0001$\u0001$\u0001"+
		"$\u0001$\u0001$\u0001$\u0001%\u0001%\u0001%\u0005%\u0247\b%\n%\f%\u024a"+
		"\t%\u0001%\u0003%\u024d\b%\u0001&\u0001&\u0001&\u0001&\u0001&\u0001\'"+
		"\u0001\'\u0003\'\u0256\b\'\u0001\'\u0001\'\u0001\'\u0001\'\u0001\'\u0001"+
		"(\u0005(\u025e\b(\n(\f(\u0261\t(\u0001(\u0001(\u0001(\u0005(\u0266\b("+
		"\n(\f(\u0269\t(\u0001(\u0003(\u026c\b(\u0001)\u0001)\u0001)\u0001)\u0001"+
		")\u0001*\u0001*\u0003*\u0275\b*\u0001+\u0003+\u0278\b+\u0001+\u0001+\u0001"+
		"+\u0003+\u027d\b+\u0001+\u0003+\u0280\b+\u0001+\u0001+\u0001+\u0001+\u0003"+
		"+\u0286\b+\u0003+\u0288\b+\u0001,\u0001,\u0001,\u0001,\u0001-\u0001-\u0001"+
		"-\u0003-\u0291\b-\u0001-\u0005-\u0294\b-\n-\f-\u0297\t-\u0001.\u0001."+
		"\u0001.\u0001.\u0001.\u0003.\u029e\b.\u0001.\u0003.\u02a1\b.\u0001/\u0001"+
		"/\u0003/\u02a5\b/\u0001/\u0001/\u0001/\u0001/\u0003/\u02ab\b/\u00010\u0001"+
		"0\u00010\u00010\u00011\u00011\u00011\u00011\u00041\u02b5\b1\u000b1\f1"+
		"\u02b6\u00012\u00012\u00012\u00012\u00032\u02bd\b2\u00013\u00013\u0003"+
		"3\u02c1\b3\u00014\u00014\u00014\u00014\u00015\u00015\u00015\u00055\u02ca"+
		"\b5\n5\f5\u02cd\t5\u00015\u00035\u02d0\b5\u00015\u00015\u00015\u00035"+
		"\u02d5\b5\u00015\u00015\u00055\u02d9\b5\n5\f5\u02dc\t5\u00016\u00016\u0001"+
		"6\u00016\u00016\u00016\u00016\u00016\u00016\u00016\u00016\u00016\u0001"+
		"6\u00036\u02eb\b6\u00017\u00017\u00017\u00017\u00057\u02f1\b7\n7\f7\u02f4"+
		"\t7\u00017\u00017\u00017\u00017\u00018\u00018\u00018\u00018\u00058\u02fe"+
		"\b8\n8\f8\u0301\t8\u00018\u00018\u00018\u00018\u00038\u0307\b8\u00038"+
		"\u0309\b8\u00019\u00019\u0001:\u0001:\u0001;\u0001;\u0001;\u0003;\u0312"+
		"\b;\u0001<\u0001<\u0001<\u0001=\u0003=\u0318\b=\u0001=\u0001=\u0003=\u031c"+
		"\b=\u0001>\u0001>\u0001>\u0001>\u0001>\u0001>\u0005>\u0324\b>\n>\f>\u0327"+
		"\t>\u0001>\u0003>\u032a\b>\u0001>\u0001>\u0003>\u032e\b>\u0001?\u0001"+
		"?\u0001?\u0001?\u0001?\u0001?\u0005?\u0336\b?\n?\f?\u0339\t?\u0001?\u0003"+
		"?\u033c\b?\u0001?\u0001?\u0003?\u0340\b?\u0001@\u0005@\u0343\b@\n@\f@"+
		"\u0346\t@\u0001@\u0001@\u0001@\u0001@\u0001A\u0001A\u0001A\u0001A\u0005"+
		"A\u0350\bA\nA\fA\u0353\tA\u0001A\u0003A\u0356\bA\u0001A\u0001A\u0001A"+
		"\u0001A\u0001A\u0001A\u0004A\u035e\bA\u000bA\fA\u035f\u0003A\u0362\bA"+
		"\u0001B\u0001B\u0001B\u0001B\u0005B\u0368\bB\nB\fB\u036b\tB\u0001B\u0003"+
		"B\u036e\bB\u0001B\u0001B\u0001B\u0001B\u0001B\u0001B\u0004B\u0376\bB\u000b"+
		"B\fB\u0377\u0003B\u037a\bB\u0001C\u0001C\u0001C\u0001C\u0003C\u0380\b"+
		"C\u0001D\u0001D\u0001D\u0001D\u0001E\u0001E\u0001E\u0005E\u0389\bE\nE"+
		"\fE\u038c\tE\u0001F\u0001F\u0003F\u0390\bF\u0001F\u0001F\u0001G\u0001"+
		"G\u0004G\u0396\bG\u000bG\fG\u0397\u0001G\u0001G\u0001H\u0001H\u0005H\u039e"+
		"\bH\nH\fH\u03a1\tH\u0001I\u0001I\u0001I\u0001J\u0001J\u0001J\u0001J\u0003"+
		"J\u03aa\bJ\u0001K\u0001K\u0001K\u0001L\u0001L\u0001L\u0003L\u03b2\bL\u0001"+
		"L\u0003L\u03b5\bL\u0001L\u0001L\u0003L\u03b9\bL\u0001M\u0001M\u0001N\u0001"+
		"N\u0001N\u0001O\u0001O\u0001O\u0001P\u0001P\u0001P\u0003P\u03c6\bP\u0001"+
		"Q\u0001Q\u0003Q\u03ca\bQ\u0001R\u0001R\u0001S\u0001S\u0001T\u0001T\u0001"+
		"U\u0001U\u0001V\u0001V\u0001W\u0001W\u0001X\u0001X\u0001Y\u0001Y\u0001"+
		"Z\u0001Z\u0003Z\u03de\bZ\u0001[\u0001[\u0001\\\u0001\\\u0001\\\u0001\\"+
		"\u0000\u0001j]\u0000\u0002\u0004\u0006\b\n\f\u000e\u0010\u0012\u0014\u0016"+
		"\u0018\u001a\u001c\u001e \"$&(*,.02468:<>@BDFHJLNPRTVXZ\\^`bdfhjlnprt"+
		"vxz|~\u0080\u0082\u0084\u0086\u0088\u008a\u008c\u008e\u0090\u0092\u0094"+
		"\u0096\u0098\u009a\u009c\u009e\u00a0\u00a2\u00a4\u00a6\u00a8\u00aa\u00ac"+
		"\u00ae\u00b0\u00b2\u00b4\u00b6\u00b8\u0000\u000f\u0002\u0000\n\n..\u0003"+
		"\u0000\n\n--12\u0002\u0000\r\r\u001a\u001a\u0003\u0000\n\n\r\r\u001c\u001d"+
		"\u0002\u0000\u0014\u0014\u001e\u001e\u0003\u0000\r\r\u001a\u001a\u001d"+
		"\u001d\u0004\u0000\n\n\r\r\u0019\u0019\u001d\u001d\u0002\u0000EEST\u0003"+
		"\u0000\r\rddff\u0002\u0000\n\n::\u0002\u000033DD\u0001\u0000?A\u0002\u0000"+
		"==CC\u0001\u0000$%\u0001\u0000`b\u0439\u0000\u00bd\u0001\u0000\u0000\u0000"+
		"\u0002\u00d1\u0001\u0000\u0000\u0000\u0004\u00f0\u0001\u0000\u0000\u0000"+
		"\u0006\u00f2\u0001\u0000\u0000\u0000\b\u0100\u0001\u0000\u0000\u0000\n"+
		"\u0110\u0001\u0000\u0000\u0000\f\u0112\u0001\u0000\u0000\u0000\u000e\u0119"+
		"\u0001\u0000\u0000\u0000\u0010\u0124\u0001\u0000\u0000\u0000\u0012\u0130"+
		"\u0001\u0000\u0000\u0000\u0014\u0143\u0001\u0000\u0000\u0000\u0016\u0145"+
		"\u0001\u0000\u0000\u0000\u0018\u0153\u0001\u0000\u0000\u0000\u001a\u015c"+
		"\u0001\u0000\u0000\u0000\u001c\u016e\u0001\u0000\u0000\u0000\u001e\u0175"+
		"\u0001\u0000\u0000\u0000 \u0179\u0001\u0000\u0000\u0000\"\u018b\u0001"+
		"\u0000\u0000\u0000$\u0192\u0001\u0000\u0000\u0000&\u0194\u0001\u0000\u0000"+
		"\u0000(\u019c\u0001\u0000\u0000\u0000*\u01a4\u0001\u0000\u0000\u0000,"+
		"\u01b6\u0001\u0000\u0000\u0000.\u01c8\u0001\u0000\u0000\u00000\u01ca\u0001"+
		"\u0000\u0000\u00002\u01e0\u0001\u0000\u0000\u00004\u01e3\u0001\u0000\u0000"+
		"\u00006\u01ed\u0001\u0000\u0000\u00008\u01ef\u0001\u0000\u0000\u0000:"+
		"\u0204\u0001\u0000\u0000\u0000<\u0217\u0001\u0000\u0000\u0000>\u0221\u0001"+
		"\u0000\u0000\u0000@\u0223\u0001\u0000\u0000\u0000B\u0232\u0001\u0000\u0000"+
		"\u0000D\u0239\u0001\u0000\u0000\u0000F\u023b\u0001\u0000\u0000\u0000H"+
		"\u023d\u0001\u0000\u0000\u0000J\u0243\u0001\u0000\u0000\u0000L\u024e\u0001"+
		"\u0000\u0000\u0000N\u0253\u0001\u0000\u0000\u0000P\u025f\u0001\u0000\u0000"+
		"\u0000R\u026d\u0001\u0000\u0000\u0000T\u0274\u0001\u0000\u0000\u0000V"+
		"\u0287\u0001\u0000\u0000\u0000X\u0289\u0001\u0000\u0000\u0000Z\u028d\u0001"+
		"\u0000\u0000\u0000\\\u02a0\u0001\u0000\u0000\u0000^\u02aa\u0001\u0000"+
		"\u0000\u0000`\u02ac\u0001\u0000\u0000\u0000b\u02b4\u0001\u0000\u0000\u0000"+
		"d\u02b8\u0001\u0000\u0000\u0000f\u02c0\u0001\u0000\u0000\u0000h\u02c2"+
		"\u0001\u0000\u0000\u0000j\u02d4\u0001\u0000\u0000\u0000l\u02ea\u0001\u0000"+
		"\u0000\u0000n\u02ec\u0001\u0000\u0000\u0000p\u0308\u0001\u0000\u0000\u0000"+
		"r\u030a\u0001\u0000\u0000\u0000t\u030c\u0001\u0000\u0000\u0000v\u030e"+
		"\u0001\u0000\u0000\u0000x\u0313\u0001\u0000\u0000\u0000z\u031b\u0001\u0000"+
		"\u0000\u0000|\u032d\u0001\u0000\u0000\u0000~\u033f\u0001\u0000\u0000\u0000"+
		"\u0080\u0344\u0001\u0000\u0000\u0000\u0082\u0361\u0001\u0000\u0000\u0000"+
		"\u0084\u0379\u0001\u0000\u0000\u0000\u0086\u037f\u0001\u0000\u0000\u0000"+
		"\u0088\u0381\u0001\u0000\u0000\u0000\u008a\u038a\u0001\u0000\u0000\u0000"+
		"\u008c\u038d\u0001\u0000\u0000\u0000\u008e\u0393\u0001\u0000\u0000\u0000"+
		"\u0090\u039b\u0001\u0000\u0000\u0000\u0092\u03a2\u0001\u0000\u0000\u0000"+
		"\u0094\u03a9\u0001\u0000\u0000\u0000\u0096\u03ab\u0001\u0000\u0000\u0000"+
		"\u0098\u03b8\u0001\u0000\u0000\u0000\u009a\u03ba\u0001\u0000\u0000\u0000"+
		"\u009c\u03bc\u0001\u0000\u0000\u0000\u009e\u03bf\u0001\u0000\u0000\u0000"+
		"\u00a0\u03c5\u0001\u0000\u0000\u0000\u00a2\u03c9\u0001\u0000\u0000\u0000"+
		"\u00a4\u03cb\u0001\u0000\u0000\u0000\u00a6\u03cd\u0001\u0000\u0000\u0000"+
		"\u00a8\u03cf\u0001\u0000\u0000\u0000\u00aa\u03d1\u0001\u0000\u0000\u0000"+
		"\u00ac\u03d3\u0001\u0000\u0000\u0000\u00ae\u03d5\u0001\u0000\u0000\u0000"+
		"\u00b0\u03d7\u0001\u0000\u0000\u0000\u00b2\u03d9\u0001\u0000\u0000\u0000"+
		"\u00b4\u03dd\u0001\u0000\u0000\u0000\u00b6\u03df\u0001\u0000\u0000\u0000"+
		"\u00b8\u03e1\u0001\u0000\u0000\u0000\u00ba\u00bc\u0003\u00b4Z\u0000\u00bb"+
		"\u00ba\u0001\u0000\u0000\u0000\u00bc\u00bf\u0001\u0000\u0000\u0000\u00bd"+
		"\u00bb\u0001\u0000\u0000\u0000\u00bd\u00be\u0001\u0000\u0000\u0000\u00be"+
		"\u00c1\u0001\u0000\u0000\u0000\u00bf\u00bd\u0001\u0000\u0000\u0000\u00c0"+
		"\u00c2\u0003\u0002\u0001\u0000\u00c1\u00c0\u0001\u0000\u0000\u0000\u00c1"+
		"\u00c2\u0001\u0000\u0000\u0000\u00c2\u00cc\u0001\u0000\u0000\u0000\u00c3"+
		"\u00c7\u0003\u0004\u0002\u0000\u00c4\u00c6\u0003\u00b4Z\u0000\u00c5\u00c4"+
		"\u0001\u0000\u0000\u0000\u00c6\u00c9\u0001\u0000\u0000\u0000\u00c7\u00c5"+
		"\u0001\u0000\u0000\u0000\u00c7\u00c8\u0001\u0000\u0000\u0000\u00c8\u00cb"+
		"\u0001\u0000\u0000\u0000\u00c9\u00c7\u0001\u0000\u0000\u0000\u00ca\u00c3"+
		"\u0001\u0000\u0000\u0000\u00cb\u00ce\u0001\u0000\u0000\u0000\u00cc\u00ca"+
		"\u0001\u0000\u0000\u0000\u00cc\u00cd\u0001\u0000\u0000\u0000\u00cd\u00cf"+
		"\u0001\u0000\u0000\u0000\u00ce\u00cc\u0001\u0000\u0000\u0000\u00cf\u00d0"+
		"\u0005\u0000\u0000\u0001\u00d0\u0001\u0001\u0000\u0000\u0000\u00d1\u00d5"+
		"\u0003\u0006\u0003\u0000\u00d2\u00d4\u0003\u00b4Z\u0000\u00d3\u00d2\u0001"+
		"\u0000\u0000\u0000\u00d4\u00d7\u0001\u0000\u0000\u0000\u00d5\u00d3\u0001"+
		"\u0000\u0000\u0000\u00d5\u00d6\u0001\u0000\u0000\u0000\u00d6\u00d8\u0001"+
		"\u0000\u0000\u0000\u00d7\u00d5\u0001\u0000\u0000\u0000\u00d8\u00dc\u0005"+
		"&\u0000\u0000\u00d9\u00db\u0003\u00b4Z\u0000\u00da\u00d9\u0001\u0000\u0000"+
		"\u0000\u00db\u00de\u0001\u0000\u0000\u0000\u00dc\u00da\u0001\u0000\u0000"+
		"\u0000\u00dc\u00dd\u0001\u0000\u0000\u0000\u00dd\u0003\u0001\u0000\u0000"+
		"\u0000\u00de\u00dc\u0001\u0000\u0000\u0000\u00df\u00e3\u0003\u0006\u0003"+
		"\u0000\u00e0\u00e2\u0003\u00b4Z\u0000\u00e1\u00e0\u0001\u0000\u0000\u0000"+
		"\u00e2\u00e5\u0001\u0000\u0000\u0000\u00e3\u00e1\u0001\u0000\u0000\u0000"+
		"\u00e3\u00e4\u0001\u0000\u0000\u0000\u00e4\u00ea\u0001\u0000\u0000\u0000"+
		"\u00e5\u00e3\u0001\u0000\u0000\u0000\u00e6\u00eb\u0003\u000e\u0007\u0000"+
		"\u00e7\u00eb\u0003\u001a\r\u0000\u00e8\u00eb\u0003\u0010\b\u0000\u00e9"+
		"\u00eb\u0003\u0012\t\u0000\u00ea\u00e6\u0001\u0000\u0000\u0000\u00ea\u00e7"+
		"\u0001\u0000\u0000\u0000\u00ea\u00e8\u0001\u0000\u0000\u0000\u00ea\u00e9"+
		"\u0001\u0000\u0000\u0000\u00eb\u00f1\u0001\u0000\u0000\u0000\u00ec\u00f1"+
		"\u0003\u000e\u0007\u0000\u00ed\u00f1\u0003\u001a\r\u0000\u00ee\u00f1\u0003"+
		"\u0010\b\u0000\u00ef\u00f1\u0003\u0012\t\u0000\u00f0\u00df\u0001\u0000"+
		"\u0000\u0000\u00f0\u00ec\u0001\u0000\u0000\u0000\u00f0\u00ed\u0001\u0000"+
		"\u0000\u0000\u00f0\u00ee\u0001\u0000\u0000\u0000\u00f0\u00ef\u0001\u0000"+
		"\u0000\u0000\u00f1\u0005\u0001\u0000\u0000\u0000\u00f2\u00fc\u0003\n\u0005"+
		"\u0000\u00f3\u00f5\u0003\u00b4Z\u0000\u00f4\u00f3\u0001\u0000\u0000\u0000"+
		"\u00f5\u00f8\u0001\u0000\u0000\u0000\u00f6\u00f4\u0001\u0000\u0000\u0000"+
		"\u00f6\u00f7\u0001\u0000\u0000\u0000\u00f7\u00f9\u0001\u0000\u0000\u0000"+
		"\u00f8\u00f6\u0001\u0000\u0000\u0000\u00f9\u00fb\u0003\n\u0005\u0000\u00fa"+
		"\u00f6\u0001\u0000\u0000\u0000\u00fb\u00fe\u0001\u0000\u0000\u0000\u00fc"+
		"\u00fa\u0001\u0000\u0000\u0000\u00fc\u00fd\u0001\u0000\u0000\u0000\u00fd"+
		"\u0007\u0001\u0000\u0000\u0000\u00fe\u00fc\u0001\u0000\u0000\u0000\u00ff"+
		"\u0101\u0003\n\u0005\u0000\u0100\u00ff\u0001\u0000\u0000\u0000\u0101\u0102"+
		"\u0001\u0000\u0000\u0000\u0102\u0100\u0001\u0000\u0000\u0000\u0102\u0103"+
		"\u0001\u0000\u0000\u0000\u0103\t\u0001\u0000\u0000\u0000\u0104\u0105\u0005"+
		"\u0013\u0000\u0000\u0105\u0109\u0005\n\u0000\u0000\u0106\u0108\u0003\u0088"+
		"D\u0000\u0107\u0106\u0001\u0000\u0000\u0000\u0108\u010b\u0001\u0000\u0000"+
		"\u0000\u0109\u0107\u0001\u0000\u0000\u0000\u0109\u010a\u0001\u0000\u0000"+
		"\u0000\u010a\u010d\u0001\u0000\u0000\u0000\u010b\u0109\u0001\u0000\u0000"+
		"\u0000\u010c\u010e\u0003\u0082A\u0000\u010d\u010c\u0001\u0000\u0000\u0000"+
		"\u010d\u010e\u0001\u0000\u0000\u0000\u010e\u0111\u0001\u0000\u0000\u0000"+
		"\u010f\u0111\u0003\f\u0006\u0000\u0110\u0104\u0001\u0000\u0000\u0000\u0110"+
		"\u010f\u0001\u0000\u0000\u0000\u0111\u000b\u0001\u0000\u0000\u0000\u0112"+
		"\u0113\u0005\u0012\u0000\u0000\u0113\u0114\u0003f3\u0000\u0114\u0115\u0005"+
		"Y\u0000\u0000\u0115\r\u0001\u0000\u0000\u0000\u0116\u0118\u0003\u00ae"+
		"W\u0000\u0117\u0116\u0001\u0000\u0000\u0000\u0118\u011b\u0001\u0000\u0000"+
		"\u0000\u0119\u0117\u0001\u0000\u0000\u0000\u0119\u011a\u0001\u0000\u0000"+
		"\u0000\u011a\u011c\u0001\u0000\u0000\u0000\u011b\u0119\u0001\u0000\u0000"+
		"\u0000\u011c\u011d\u00052\u0000\u0000\u011d\u011f\u0003\u00a0P\u0000\u011e"+
		"\u0120\u00032\u0019\u0000\u011f\u011e\u0001\u0000\u0000\u0000\u011f\u0120"+
		"\u0001\u0000\u0000\u0000\u0120\u0121\u0001\u0000\u0000\u0000\u0121\u0122"+
		"\u0003\u0016\u000b\u0000\u0122\u000f\u0001\u0000\u0000\u0000\u0123\u0125"+
		"\u0005.\u0000\u0000\u0124\u0123\u0001\u0000\u0000\u0000\u0124\u0125\u0001"+
		"\u0000\u0000\u0000\u0125\u0126\u0001\u0000\u0000\u0000\u0126\u0127\u0005"+
		"-\u0000\u0000\u0127\u0128\u0005\n\u0000\u0000\u0128\u0129\u0005G\u0000"+
		"\u0000\u0129\u012b\u0003$\u0012\u0000\u012a\u012c\u0005)\u0000\u0000\u012b"+
		"\u012a\u0001\u0000\u0000\u0000\u012b\u012c\u0001\u0000\u0000\u0000\u012c"+
		"\u0011\u0001\u0000\u0000\u0000\u012d\u012f\u0003\u00b0X\u0000\u012e\u012d"+
		"\u0001\u0000\u0000\u0000\u012f\u0132\u0001\u0000\u0000\u0000\u0130\u012e"+
		"\u0001\u0000\u0000\u0000\u0130\u0131\u0001\u0000\u0000\u0000\u0131\u0133"+
		"\u0001\u0000\u0000\u0000\u0132\u0130\u0001\u0000\u0000\u0000\u0133\u0134"+
		"\u0005>\u0000\u0000\u0134\u0135\u0007\u0000\u0000\u0000\u0135\u0136\u0003"+
		"2\u0019\u0000\u0136\u0137\u0005B\u0000\u0000\u0137\u013e\u0003\u0014\n"+
		"\u0000\u0138\u013a\u0005*\u0000\u0000\u0139\u0138\u0001\u0000\u0000\u0000"+
		"\u0139\u013a\u0001\u0000\u0000\u0000\u013a\u013b\u0001\u0000\u0000\u0000"+
		"\u013b\u013d\u0003\u0014\n\u0000\u013c\u0139\u0001\u0000\u0000\u0000\u013d"+
		"\u0140\u0001\u0000\u0000\u0000\u013e\u013c\u0001\u0000\u0000\u0000\u013e"+
		"\u013f\u0001\u0000\u0000\u0000\u013f\u0141\u0001\u0000\u0000\u0000\u0140"+
		"\u013e\u0001\u0000\u0000\u0000\u0141\u0142\u0005)\u0000\u0000\u0142\u0013"+
		"\u0001\u0000\u0000\u0000\u0143\u0144\u0007\u0001\u0000\u0000\u0144\u0015"+
		"\u0001\u0000\u0000\u0000\u0145\u014b\u0005\'\u0000\u0000\u0146\u014a\u0003"+
		"\u0018\f\u0000\u0147\u014a\u0003\u001a\r\u0000\u0148\u014a\u0003\u00b4"+
		"Z\u0000\u0149\u0146\u0001\u0000\u0000\u0000\u0149\u0147\u0001\u0000\u0000"+
		"\u0000\u0149\u0148\u0001\u0000\u0000\u0000\u014a\u014d\u0001\u0000\u0000"+
		"\u0000\u014b\u0149\u0001\u0000\u0000\u0000\u014b\u014c\u0001\u0000\u0000"+
		"\u0000\u014c\u014e\u0001\u0000\u0000\u0000\u014d\u014b\u0001\u0000\u0000"+
		"\u0000\u014e\u014f\u0005(\u0000\u0000\u014f\u0017\u0001\u0000\u0000\u0000"+
		"\u0150\u0152\u0003\u00acV\u0000\u0151\u0150\u0001\u0000\u0000\u0000\u0152"+
		"\u0155\u0001\u0000\u0000\u0000\u0153\u0151\u0001\u0000\u0000\u0000\u0153"+
		"\u0154\u0001\u0000\u0000\u0000\u0154\u0156\u0001\u0000\u0000\u0000\u0155"+
		"\u0153\u0001\u0000\u0000\u0000\u0156\u0157\u00051\u0000\u0000\u0157\u0158"+
		"\u00038\u001c\u0000\u0158\u0019\u0001\u0000\u0000\u0000\u0159\u015b\u0003"+
		"\u00b2Y\u0000\u015a\u0159\u0001\u0000\u0000\u0000\u015b\u015e\u0001\u0000"+
		"\u0000\u0000\u015c\u015a\u0001\u0000\u0000\u0000\u015c\u015d\u0001\u0000"+
		"\u0000\u0000\u015d\u015f\u0001\u0000\u0000\u0000\u015e\u015c\u0001\u0000"+
		"\u0000\u0000\u015f\u0160\u0005+\u0000\u0000\u0160\u0162\u0003\u00a2Q\u0000"+
		"\u0161\u0163\u0003\u001c\u000e\u0000\u0162\u0161\u0001\u0000\u0000\u0000"+
		"\u0162\u0163\u0001\u0000\u0000\u0000\u0163\u0167\u0001\u0000\u0000\u0000"+
		"\u0164\u0166\u0003\u00b4Z\u0000\u0165\u0164\u0001\u0000\u0000\u0000\u0166"+
		"\u0169\u0001\u0000\u0000\u0000\u0167\u0165\u0001\u0000\u0000\u0000\u0167"+
		"\u0168\u0001\u0000\u0000\u0000\u0168\u016a\u0001\u0000\u0000\u0000\u0169"+
		"\u0167\u0001\u0000\u0000\u0000\u016a\u016b\u0003\"\u0011\u0000\u016b\u001b"+
		"\u0001\u0000\u0000\u0000\u016c\u016f\u00032\u0019\u0000\u016d\u016f\u0003"+
		"\u001e\u000f\u0000\u016e\u016c\u0001\u0000\u0000\u0000\u016e\u016d\u0001"+
		"\u0000\u0000\u0000\u016e\u016f\u0001\u0000\u0000\u0000\u016f\u0170\u0001"+
		"\u0000\u0000\u0000\u0170\u0173\u0005H\u0000\u0000\u0171\u0174\u0003$\u0012"+
		"\u0000\u0172\u0174\u0003 \u0010\u0000\u0173\u0171\u0001\u0000\u0000\u0000"+
		"\u0173\u0172\u0001\u0000\u0000\u0000\u0174\u001d\u0001\u0000\u0000\u0000"+
		"\u0175\u0176\u0005\t\u0000\u0000\u0176\u0177\u0003$\u0012\u0000\u0177"+
		"\u0178\u0005Y\u0000\u0000\u0178\u001f\u0001\u0000\u0000\u0000\u0179\u017a"+
		"\u00032\u0019\u0000\u017a!\u0001\u0000\u0000\u0000\u017b\u017d\u0005/"+
		"\u0000\u0000\u017c\u017b\u0001\u0000\u0000\u0000\u017c\u017d\u0001\u0000"+
		"\u0000\u0000\u017d\u017e\u0001\u0000\u0000\u0000\u017e\u018c\u00038\u001c"+
		"\u0000\u017f\u0180\u0005\u0018\u0000\u0000\u0180\u0182\u0003f3\u0000\u0181"+
		"\u0183\u0005\f\u0000\u0000\u0182\u0181\u0001\u0000\u0000\u0000\u0182\u0183"+
		"\u0001\u0000\u0000\u0000\u0183\u018c\u0001\u0000\u0000\u0000\u0184\u0186"+
		"\u00050\u0000\u0000\u0185\u0187\u0005\n\u0000\u0000\u0186\u0185\u0001"+
		"\u0000\u0000\u0000\u0186\u0187\u0001\u0000\u0000\u0000\u0187\u0189\u0001"+
		"\u0000\u0000\u0000\u0188\u018a\u0005)\u0000\u0000\u0189\u0188\u0001\u0000"+
		"\u0000\u0000\u0189\u018a\u0001\u0000\u0000\u0000\u018a\u018c\u0001\u0000"+
		"\u0000\u0000\u018b\u017c\u0001\u0000\u0000\u0000\u018b\u017f\u0001\u0000"+
		"\u0000\u0000\u018b\u0184\u0001\u0000\u0000\u0000\u018c#\u0001\u0000\u0000"+
		"\u0000\u018d\u018f\u0003\b\u0004\u0000\u018e\u0190\u0003&\u0013\u0000"+
		"\u018f\u018e\u0001\u0000\u0000\u0000\u018f\u0190\u0001\u0000\u0000\u0000"+
		"\u0190\u0193\u0001\u0000\u0000\u0000\u0191\u0193\u0003&\u0013\u0000\u0192"+
		"\u018d\u0001\u0000\u0000\u0000\u0192\u0191\u0001\u0000\u0000\u0000\u0193"+
		"%\u0001\u0000\u0000\u0000\u0194\u0199\u0003(\u0014\u0000\u0195\u0196\u0005"+
		"\u001f\u0000\u0000\u0196\u0198\u0003(\u0014\u0000\u0197\u0195\u0001\u0000"+
		"\u0000\u0000\u0198\u019b\u0001\u0000\u0000\u0000\u0199\u0197\u0001\u0000"+
		"\u0000\u0000\u0199\u019a\u0001\u0000\u0000\u0000\u019a\'\u0001\u0000\u0000"+
		"\u0000\u019b\u0199\u0001\u0000\u0000\u0000\u019c\u019e\u0003*\u0015\u0000"+
		"\u019d\u019f\u0005\u001e\u0000\u0000\u019e\u019d\u0001\u0000\u0000\u0000"+
		"\u019e\u019f\u0001\u0000\u0000\u0000\u019f)\u0001\u0000\u0000\u0000\u01a0"+
		"\u01a5\u00036\u001b\u0000\u01a1\u01a5\u0003,\u0016\u0000\u01a2\u01a5\u0003"+
		".\u0017\u0000\u01a3\u01a5\u00030\u0018\u0000\u01a4\u01a0\u0001\u0000\u0000"+
		"\u0000\u01a4\u01a1\u0001\u0000\u0000\u0000\u01a4\u01a2\u0001\u0000\u0000"+
		"\u0000\u01a4\u01a3\u0001\u0000\u0000\u0000\u01a5+\u0001\u0000\u0000\u0000"+
		"\u01a6\u01a7\u0005\u000e\u0000\u0000\u01a7\u01b7\u0005(\u0000\u0000\u01a8"+
		"\u01a9\u0005\u000e\u0000\u0000\u01a9\u01ae\u00034\u001a\u0000\u01aa\u01ab"+
		"\u0005W\u0000\u0000\u01ab\u01ad\u00034\u001a\u0000\u01ac\u01aa\u0001\u0000"+
		"\u0000\u0000\u01ad\u01b0\u0001\u0000\u0000\u0000\u01ae\u01ac\u0001\u0000"+
		"\u0000\u0000\u01ae\u01af\u0001\u0000\u0000\u0000\u01af\u01b2\u0001\u0000"+
		"\u0000\u0000\u01b0\u01ae\u0001\u0000\u0000\u0000\u01b1\u01b3\u0005W\u0000"+
		"\u0000\u01b2\u01b1\u0001\u0000\u0000\u0000\u01b2\u01b3\u0001\u0000\u0000"+
		"\u0000\u01b3\u01b4\u0001\u0000\u0000\u0000\u01b4\u01b5\u0005(\u0000\u0000"+
		"\u01b5\u01b7\u0001\u0000\u0000\u0000\u01b6\u01a6\u0001\u0000\u0000\u0000"+
		"\u01b6\u01a8\u0001\u0000\u0000\u0000\u01b7-\u0001\u0000\u0000\u0000\u01b8"+
		"\u01b9\u0005\u000f\u0000\u0000\u01b9\u01c9\u0005Y\u0000\u0000\u01ba\u01bb"+
		"\u0005\u000f\u0000\u0000\u01bb\u01c0\u00034\u001a\u0000\u01bc\u01bd\u0005"+
		"W\u0000\u0000\u01bd\u01bf\u00034\u001a\u0000\u01be\u01bc\u0001\u0000\u0000"+
		"\u0000\u01bf\u01c2\u0001\u0000\u0000\u0000\u01c0\u01be\u0001\u0000\u0000"+
		"\u0000\u01c0\u01c1\u0001\u0000\u0000\u0000\u01c1\u01c4\u0001\u0000\u0000"+
		"\u0000\u01c2\u01c0\u0001\u0000\u0000\u0000\u01c3\u01c5\u0005W\u0000\u0000"+
		"\u01c4\u01c3\u0001\u0000\u0000\u0000\u01c4\u01c5\u0001\u0000\u0000\u0000"+
		"\u01c5\u01c6\u0001\u0000\u0000\u0000\u01c6\u01c7\u0005Y\u0000\u0000\u01c7"+
		"\u01c9\u0001\u0000\u0000\u0000\u01c8\u01b8\u0001\u0000\u0000\u0000\u01c8"+
		"\u01ba\u0001\u0000\u0000\u0000\u01c9/\u0001\u0000\u0000\u0000\u01ca\u01cb"+
		"\u0005,\u0000\u0000\u01cb\u01cc\u00032\u0019\u0000\u01cc\u01cd\u0005H"+
		"\u0000\u0000\u01cd\u01ce\u0003$\u0012\u0000\u01ce1\u0001\u0000\u0000\u0000"+
		"\u01cf\u01d0\u0005K\u0000\u0000\u01d0\u01d5\u00034\u001a\u0000\u01d1\u01d2"+
		"\u0005W\u0000\u0000\u01d2\u01d4\u00034\u001a\u0000\u01d3\u01d1\u0001\u0000"+
		"\u0000\u0000\u01d4\u01d7\u0001\u0000\u0000\u0000\u01d5\u01d3\u0001\u0000"+
		"\u0000\u0000\u01d5\u01d6\u0001\u0000\u0000\u0000\u01d6\u01d9\u0001\u0000"+
		"\u0000\u0000\u01d7\u01d5\u0001\u0000\u0000\u0000\u01d8\u01da\u0005W\u0000"+
		"\u0000\u01d9\u01d8\u0001\u0000\u0000\u0000\u01d9\u01da\u0001\u0000\u0000"+
		"\u0000\u01da\u01db\u0001\u0000\u0000\u0000\u01db\u01dc\u0005X\u0000\u0000"+
		"\u01dc\u01e1\u0001\u0000\u0000\u0000\u01dd\u01de\u0005K\u0000\u0000\u01de"+
		"\u01e1\u0005X\u0000\u0000\u01df\u01e1\u0005J\u0000\u0000\u01e0\u01cf\u0001"+
		"\u0000\u0000\u0000\u01e0\u01dd\u0001\u0000\u0000\u0000\u01e0\u01df\u0001"+
		"\u0000\u0000\u0000\u01e13\u0001\u0000\u0000\u0000\u01e2\u01e4\u0003\u0006"+
		"\u0003\u0000\u01e3\u01e2\u0001\u0000\u0000\u0000\u01e3\u01e4\u0001\u0000"+
		"\u0000\u0000\u01e4\u01e5\u0001\u0000\u0000\u0000\u01e5\u01e7\u0003&\u0013"+
		"\u0000\u01e6\u01e8\u0007\u0002\u0000\u0000\u01e7\u01e6\u0001\u0000\u0000"+
		"\u0000\u01e7\u01e8\u0001\u0000\u0000\u0000\u01e8\u01eb\u0001\u0000\u0000"+
		"\u0000\u01e9\u01ea\u0005]\u0000\u0000\u01ea\u01ec\u0003f3\u0000\u01eb"+
		"\u01e9\u0001\u0000\u0000\u0000\u01eb\u01ec\u0001\u0000\u0000\u0000\u01ec"+
		"5\u0001\u0000\u0000\u0000\u01ed\u01ee\u0007\u0003\u0000\u0000\u01ee7\u0001"+
		"\u0000\u0000\u0000\u01ef\u01f5\u0005\'\u0000\u0000\u01f0\u01f4\u0003:"+
		"\u001d\u0000\u01f1\u01f4\u0005)\u0000\u0000\u01f2\u01f4\u0003\u00b4Z\u0000"+
		"\u01f3\u01f0\u0001\u0000\u0000\u0000\u01f3\u01f1\u0001\u0000\u0000\u0000"+
		"\u01f3\u01f2\u0001\u0000\u0000\u0000\u01f4\u01f7\u0001\u0000\u0000\u0000"+
		"\u01f5\u01f3\u0001\u0000\u0000\u0000\u01f5\u01f6\u0001\u0000\u0000\u0000"+
		"\u01f6\u01f8\u0001\u0000\u0000\u0000\u01f7\u01f5\u0001\u0000\u0000\u0000"+
		"\u01f8\u01f9\u0005(\u0000\u0000\u01f99\u0001\u0000\u0000\u0000\u01fa\u01fe"+
		"\u0003\u0006\u0003\u0000\u01fb\u01fd\u0003\u00b4Z\u0000\u01fc\u01fb\u0001"+
		"\u0000\u0000\u0000\u01fd\u0200\u0001\u0000\u0000\u0000\u01fe\u01fc\u0001"+
		"\u0000\u0000\u0000\u01fe\u01ff\u0001\u0000\u0000\u0000\u01ff\u0201\u0001"+
		"\u0000\u0000\u0000\u0200\u01fe\u0001\u0000\u0000\u0000\u0201\u0202\u0003"+
		"<\u001e\u0000\u0202\u0205\u0001\u0000\u0000\u0000\u0203\u0205\u0003<\u001e"+
		"\u0000\u0204\u01fa\u0001\u0000\u0000\u0000\u0204\u0203\u0001\u0000\u0000"+
		"\u0000\u0205;\u0001\u0000\u0000\u0000\u0206\u0207\u0003\u009cN\u0000\u0207"+
		"\u0208\u0005#\u0000\u0000\u0208\u0218\u0001\u0000\u0000\u0000\u0209\u020b"+
		"\u0003\u009cN\u0000\u020a\u0209\u0001\u0000\u0000\u0000\u020a\u020b\u0001"+
		"\u0000\u0000\u0000\u020b\u020c\u0001\u0000\u0000\u0000\u020c\u0218\u0003"+
		"8\u001c\u0000\u020d\u0218\u0003>\u001f\u0000\u020e\u0218\u0003V+\u0000"+
		"\u020f\u0218\u0003X,\u0000\u0210\u0218\u0003Z-\u0000\u0211\u0218\u0003"+
		"^/\u0000\u0212\u0218\u0003N\'\u0000\u0213\u0218\u0003@ \u0000\u0214\u0218"+
		"\u0003H$\u0000\u0215\u0218\u0003h4\u0000\u0216\u0218\u0003j5\u0000\u0217"+
		"\u0206\u0001\u0000\u0000\u0000\u0217\u020a\u0001\u0000\u0000\u0000\u0217"+
		"\u020d\u0001\u0000\u0000\u0000\u0217\u020e\u0001\u0000\u0000\u0000\u0217"+
		"\u020f\u0001\u0000\u0000\u0000\u0217\u0210\u0001\u0000\u0000\u0000\u0217"+
		"\u0211\u0001\u0000\u0000\u0000\u0217\u0212\u0001\u0000\u0000\u0000\u0217"+
		"\u0213\u0001\u0000\u0000\u0000\u0217\u0214\u0001\u0000\u0000\u0000\u0217"+
		"\u0215\u0001\u0000\u0000\u0000\u0217\u0216\u0001\u0000\u0000\u0000\u0218"+
		"=\u0001\u0000\u0000\u0000\u0219\u021a\u0003\u00a8T\u0000\u021a\u021c\u0003"+
		"\u0084B\u0000\u021b\u021d\u0003z=\u0000\u021c\u021b\u0001\u0000\u0000"+
		"\u0000\u021c\u021d\u0001\u0000\u0000\u0000\u021d\u0222\u0001\u0000\u0000"+
		"\u0000\u021e\u021f\u0003\u00a8T\u0000\u021f\u0220\u0003z=\u0000\u0220"+
		"\u0222\u0001\u0000\u0000\u0000\u0221\u0219\u0001\u0000\u0000\u0000\u0221"+
		"\u021e\u0001\u0000\u0000\u0000\u0222?\u0001\u0000\u0000\u0000\u0223\u0224"+
		"\u0005<\u0000\u0000\u0224\u0225\u0003F#\u0000\u0225\u0230\u00038\u001c"+
		"\u0000\u0226\u0228\u0003\u00b4Z\u0000\u0227\u0226\u0001\u0000\u0000\u0000"+
		"\u0228\u022b\u0001\u0000\u0000\u0000\u0229\u0227\u0001\u0000\u0000\u0000"+
		"\u0229\u022a\u0001\u0000\u0000\u0000\u022a\u022c\u0001\u0000\u0000\u0000"+
		"\u022b\u0229\u0001\u0000\u0000\u0000\u022c\u022d\u00055\u0000\u0000\u022d"+
		"\u022e\u0003D\"\u0000\u022e\u022f\u0005#\u0000\u0000\u022f\u0231\u0001"+
		"\u0000\u0000\u0000\u0230\u0229\u0001\u0000\u0000\u0000\u0230\u0231\u0001"+
		"\u0000\u0000\u0000\u0231A\u0001\u0000\u0000\u0000\u0232\u0233\u00055\u0000"+
		"\u0000\u0233\u0234\u0005H\u0000\u0000\u0234\u0235\u0003D\"\u0000\u0235"+
		"\u0236\u0005#\u0000\u0000\u0236C\u0001\u0000\u0000\u0000\u0237\u023a\u0003"+
		"f3\u0000\u0238\u023a\u0003:\u001d\u0000\u0239\u0237\u0001\u0000\u0000"+
		"\u0000\u0239\u0238\u0001\u0000\u0000\u0000\u023aE\u0001\u0000\u0000\u0000"+
		"\u023b\u023c\u0003\u0082A\u0000\u023cG\u0001\u0000\u0000\u0000\u023d\u023e"+
		"\u0005<\u0000\u0000\u023e\u023f\u0005\'\u0000\u0000\u023f\u0240\u0005"+
		"#\u0000\u0000\u0240\u0241\u0003J%\u0000\u0241\u0242\u0005(\u0000\u0000"+
		"\u0242I\u0001\u0000\u0000\u0000\u0243\u0248\u0003L&\u0000\u0244\u0247"+
		"\u0003L&\u0000\u0245\u0247\u0003\u00b4Z\u0000\u0246\u0244\u0001\u0000"+
		"\u0000\u0000\u0246\u0245\u0001\u0000\u0000\u0000\u0247\u024a\u0001\u0000"+
		"\u0000\u0000\u0248\u0246\u0001\u0000\u0000\u0000\u0248\u0249\u0001\u0000"+
		"\u0000\u0000\u0249\u024c\u0001\u0000\u0000\u0000\u024a\u0248\u0001\u0000"+
		"\u0000\u0000\u024b\u024d\u0003B!\u0000\u024c\u024b\u0001\u0000\u0000\u0000"+
		"\u024c\u024d\u0001\u0000\u0000\u0000\u024dK\u0001\u0000\u0000\u0000\u024e"+
		"\u024f\u0003F#\u0000\u024f\u0250\u0005H\u0000\u0000\u0250\u0251\u0003"+
		"D\"\u0000\u0251\u0252\u0005#\u0000\u0000\u0252M\u0001\u0000\u0000\u0000"+
		"\u0253\u0255\u0005<\u0000\u0000\u0254\u0256\u0003f3\u0000\u0255\u0254"+
		"\u0001\u0000\u0000\u0000\u0255\u0256\u0001\u0000\u0000\u0000\u0256\u0257"+
		"\u0001\u0000\u0000\u0000\u0257\u0258\u0005\'\u0000\u0000\u0258\u0259\u0005"+
		"#\u0000\u0000\u0259\u025a\u0003P(\u0000\u025a\u025b\u0005(\u0000\u0000"+
		"\u025bO\u0001\u0000\u0000\u0000\u025c\u025e\u0003\u00b4Z\u0000\u025d\u025c"+
		"\u0001\u0000\u0000\u0000\u025e\u0261\u0001\u0000\u0000\u0000\u025f\u025d"+
		"\u0001\u0000\u0000\u0000\u025f\u0260\u0001\u0000\u0000\u0000\u0260\u0262"+
		"\u0001\u0000\u0000\u0000\u0261\u025f\u0001\u0000\u0000\u0000\u0262\u0267"+
		"\u0003R)\u0000\u0263\u0266\u0003R)\u0000\u0264\u0266\u0003\u00b4Z\u0000"+
		"\u0265\u0263\u0001\u0000\u0000\u0000\u0265\u0264\u0001\u0000\u0000\u0000"+
		"\u0266\u0269\u0001\u0000\u0000\u0000\u0267\u0265\u0001\u0000\u0000\u0000"+
		"\u0267\u0268\u0001\u0000\u0000\u0000\u0268\u026b\u0001\u0000\u0000\u0000"+
		"\u0269\u0267\u0001\u0000\u0000\u0000\u026a\u026c\u0003B!\u0000\u026b\u026a"+
		"\u0001\u0000\u0000\u0000\u026b\u026c\u0001\u0000\u0000\u0000\u026cQ\u0001"+
		"\u0000\u0000\u0000\u026d\u026e\u0003T*\u0000\u026e\u026f\u0005H\u0000"+
		"\u0000\u026f\u0270\u0003D\"\u0000\u0270\u0271\u0005#\u0000\u0000\u0271"+
		"S\u0001\u0000\u0000\u0000\u0272\u0275\u0003f3\u0000\u0273\u0275\u0003"+
		"\u008eG\u0000\u0274\u0272\u0001\u0000\u0000\u0000\u0274\u0273\u0001\u0000"+
		"\u0000\u0000\u0275U\u0001\u0000\u0000\u0000\u0276\u0278\u0005;\u0000\u0000"+
		"\u0277\u0276\u0001\u0000\u0000\u0000\u0277\u0278\u0001\u0000\u0000\u0000"+
		"\u0278\u0279\u0001\u0000\u0000\u0000\u0279\u027a\u0005:\u0000\u0000\u027a"+
		"\u027c\u0003\u00a4R\u0000\u027b\u027d\u0003\\.\u0000\u027c\u027b\u0001"+
		"\u0000\u0000\u0000\u027c\u027d\u0001\u0000\u0000\u0000\u027d\u0288\u0001"+
		"\u0000\u0000\u0000\u027e\u0280\u0005;\u0000\u0000\u027f\u027e\u0001\u0000"+
		"\u0000\u0000\u027f\u0280\u0001\u0000\u0000\u0000\u0280\u0281\u0001\u0000"+
		"\u0000\u0000\u0281\u0282\u0005:\u0000\u0000\u0282\u0283\u0003$\u0012\u0000"+
		"\u0283\u0285\u0003\u00a4R\u0000\u0284\u0286\u0003\\.\u0000\u0285\u0284"+
		"\u0001\u0000\u0000\u0000\u0285\u0286\u0001\u0000\u0000\u0000\u0286\u0288"+
		"\u0001\u0000\u0000\u0000\u0287\u0277\u0001\u0000\u0000\u0000\u0287\u027f"+
		"\u0001\u0000\u0000\u0000\u0288W\u0001\u0000\u0000\u0000\u0289\u028a\u0005"+
		"Z\u0000\u0000\u028a\u028b\u0003\u00a4R\u0000\u028b\u028c\u0003\\.\u0000"+
		"\u028cY\u0001\u0000\u0000\u0000\u028d\u028e\u0005F\u0000\u0000\u028e\u0290"+
		"\u0003\u00a6S\u0000\u028f\u0291\u0003\u0084B\u0000\u0290\u028f\u0001\u0000"+
		"\u0000\u0000\u0290\u0291\u0001\u0000\u0000\u0000\u0291\u0295\u0001\u0000"+
		"\u0000\u0000\u0292\u0294\u0003\u0098L\u0000\u0293\u0292\u0001\u0000\u0000"+
		"\u0000\u0294\u0297\u0001\u0000\u0000\u0000\u0295\u0293\u0001\u0000\u0000"+
		"\u0000\u0295\u0296\u0001\u0000\u0000\u0000\u0296[\u0001\u0000\u0000\u0000"+
		"\u0297\u0295\u0001\u0000\u0000\u0000\u0298\u029d\u0005G\u0000\u0000\u0299"+
		"\u029e\u0003f3\u0000\u029a\u029e\u0003N\'\u0000\u029b\u029e\u0003H$\u0000"+
		"\u029c\u029e\u0003>\u001f\u0000\u029d\u0299\u0001\u0000\u0000\u0000\u029d"+
		"\u029a\u0001\u0000\u0000\u0000\u029d\u029b\u0001\u0000\u0000\u0000\u029d"+
		"\u029c\u0001\u0000\u0000\u0000\u029e\u02a1\u0001\u0000\u0000\u0000\u029f"+
		"\u02a1\u0003z=\u0000\u02a0\u0298\u0001\u0000\u0000\u0000\u02a0\u029f\u0001"+
		"\u0000\u0000\u0000\u02a1]\u0001\u0000\u0000\u0000\u02a2\u02a4\u00056\u0000"+
		"\u0000\u02a3\u02a5\u0003\u0082A\u0000\u02a4\u02a3\u0001\u0000\u0000\u0000"+
		"\u02a4\u02a5\u0001\u0000\u0000\u0000\u02a5\u02ab\u0001\u0000\u0000\u0000"+
		"\u02a6\u02a7\u00057\u0000\u0000\u02a7\u02ab\u0005\n\u0000\u0000\u02a8"+
		"\u02ab\u00059\u0000\u0000\u02a9\u02ab\u00058\u0000\u0000\u02aa\u02a2\u0001"+
		"\u0000\u0000\u0000\u02aa\u02a6\u0001\u0000\u0000\u0000\u02aa\u02a8\u0001"+
		"\u0000\u0000\u0000\u02aa\u02a9\u0001\u0000\u0000\u0000\u02ab_\u0001\u0000"+
		"\u0000\u0000\u02ac\u02ad\u0005\u0007\u0000\u0000\u02ad\u02ae\u0003b1\u0000"+
		"\u02ae\u02af\u0005\u0002\u0000\u0000\u02afa\u0001\u0000\u0000\u0000\u02b0"+
		"\u02b5\u0005P\u0000\u0000\u02b1\u02b5\u0005Q\u0000\u0000\u02b2\u02b5\u0005"+
		"R\u0000\u0000\u02b3\u02b5\u0003d2\u0000\u02b4\u02b0\u0001\u0000\u0000"+
		"\u0000\u02b4\u02b1\u0001\u0000\u0000\u0000\u02b4\u02b2\u0001\u0000\u0000"+
		"\u0000\u02b4\u02b3\u0001\u0000\u0000\u0000\u02b5\u02b6\u0001\u0000\u0000"+
		"\u0000\u02b6\u02b4\u0001\u0000\u0000\u0000\u02b6\u02b7\u0001\u0000\u0000"+
		"\u0000\u02b7c\u0001\u0000\u0000\u0000\u02b8\u02b9\u0005\b\u0000\u0000"+
		"\u02b9\u02ba\u0003\u0090H\u0000\u02ba\u02bc\u0005(\u0000\u0000\u02bb\u02bd"+
		"\u0005c\u0000\u0000\u02bc\u02bb\u0001\u0000\u0000\u0000\u02bc\u02bd\u0001"+
		"\u0000\u0000\u0000\u02bde\u0001\u0000\u0000\u0000\u02be\u02c1\u0003j5"+
		"\u0000\u02bf\u02c1\u0003\u0088D\u0000\u02c0\u02be\u0001\u0000\u0000\u0000"+
		"\u02c0\u02bf\u0001\u0000\u0000\u0000\u02c1g\u0001\u0000\u0000\u0000\u02c2"+
		"\u02c3\u0005I\u0000\u0000\u02c3\u02c4\u0003f3\u0000\u02c4\u02c5\u0005"+
		"\f\u0000\u0000\u02c5i\u0001\u0000\u0000\u0000\u02c6\u02c7\u00065\uffff"+
		"\uffff\u0000\u02c7\u02cb\u0003l6\u0000\u02c8\u02ca\u0003\u0098L\u0000"+
		"\u02c9\u02c8\u0001\u0000\u0000\u0000\u02ca\u02cd\u0001\u0000\u0000\u0000"+
		"\u02cb\u02c9\u0001\u0000\u0000\u0000\u02cb\u02cc\u0001\u0000\u0000\u0000"+
		"\u02cc\u02cf\u0001\u0000\u0000\u0000\u02cd\u02cb\u0001\u0000\u0000\u0000"+
		"\u02ce\u02d0\u0003\u0096K\u0000\u02cf\u02ce\u0001\u0000\u0000\u0000\u02cf"+
		"\u02d0\u0001\u0000\u0000\u0000\u02d0\u02d5\u0001\u0000\u0000\u0000\u02d1"+
		"\u02d2\u0003r9\u0000\u02d2\u02d3\u0003j5\u0002\u02d3\u02d5\u0001\u0000"+
		"\u0000\u0000\u02d4\u02c6\u0001\u0000\u0000\u0000\u02d4\u02d1\u0001\u0000"+
		"\u0000\u0000\u02d5\u02da\u0001\u0000\u0000\u0000\u02d6\u02d7\n\u0001\u0000"+
		"\u0000\u02d7\u02d9\u0003t:\u0000\u02d8\u02d6\u0001\u0000\u0000\u0000\u02d9"+
		"\u02dc\u0001\u0000\u0000\u0000\u02da\u02d8\u0001\u0000\u0000\u0000\u02da"+
		"\u02db\u0001\u0000\u0000\u0000\u02dbk\u0001\u0000\u0000\u0000\u02dc\u02da"+
		"\u0001\u0000\u0000\u0000\u02dd\u02eb\u0003\u008cF\u0000\u02de\u02eb\u0003"+
		"p8\u0000\u02df\u02eb\u0003n7\u0000\u02e0\u02eb\u0003\u0090H\u0000\u02e1"+
		"\u02eb\u0003~?\u0000\u02e2\u02eb\u0003|>\u0000\u02e3\u02eb\u0003h4\u0000"+
		"\u02e4\u02eb\u0003v;\u0000\u02e5\u02eb\u0005\u0019\u0000\u0000\u02e6\u02eb"+
		"\u0005\u001a\u0000\u0000\u02e7\u02eb\u0005\u001b\u0000\u0000\u02e8\u02eb"+
		"\u0005\u001c\u0000\u0000\u02e9\u02eb\u0005\u001d\u0000\u0000\u02ea\u02dd"+
		"\u0001\u0000\u0000\u0000\u02ea\u02de\u0001\u0000\u0000\u0000\u02ea\u02df"+
		"\u0001\u0000\u0000\u0000\u02ea\u02e0\u0001\u0000\u0000\u0000\u02ea\u02e1"+
		"\u0001\u0000\u0000\u0000\u02ea\u02e2\u0001\u0000\u0000\u0000\u02ea\u02e3"+
		"\u0001\u0000\u0000\u0000\u02ea\u02e4\u0001\u0000\u0000\u0000\u02ea\u02e5"+
		"\u0001\u0000\u0000\u0000\u02ea\u02e6\u0001\u0000\u0000\u0000\u02ea\u02e7"+
		"\u0001\u0000\u0000\u0000\u02ea\u02e8\u0001\u0000\u0000\u0000\u02ea\u02e9"+
		"\u0001\u0000\u0000\u0000\u02ebm\u0001\u0000\u0000\u0000\u02ec\u02f2\u0005"+
		"\u0017\u0000\u0000\u02ed\u02f1\u0003:\u001d\u0000\u02ee\u02f1\u0005)\u0000"+
		"\u0000\u02ef\u02f1\u0003\u00b4Z\u0000\u02f0\u02ed\u0001\u0000\u0000\u0000"+
		"\u02f0\u02ee\u0001\u0000\u0000\u0000\u02f0\u02ef\u0001\u0000\u0000\u0000"+
		"\u02f1\u02f4\u0001\u0000\u0000\u0000\u02f2\u02f0\u0001\u0000\u0000\u0000"+
		"\u02f2\u02f3\u0001\u0000\u0000\u0000\u02f3\u02f5\u0001\u0000\u0000\u0000"+
		"\u02f4\u02f2\u0001\u0000\u0000\u0000\u02f5\u02f6\u0005(\u0000\u0000\u02f6"+
		"\u02f7\u0005\u0018\u0000\u0000\u02f7\u02f8\u0003\u0090H\u0000\u02f8o\u0001"+
		"\u0000\u0000\u0000\u02f9\u02ff\u0005\u0010\u0000\u0000\u02fa\u02fe\u0003"+
		":\u001d\u0000\u02fb\u02fe\u0005)\u0000\u0000\u02fc\u02fe\u0003\u00b4Z"+
		"\u0000\u02fd\u02fa\u0001\u0000\u0000\u0000\u02fd\u02fb\u0001\u0000\u0000"+
		"\u0000\u02fd\u02fc\u0001\u0000\u0000\u0000\u02fe\u0301\u0001\u0000\u0000"+
		"\u0000\u02ff\u02fd\u0001\u0000\u0000\u0000\u02ff\u0300\u0001\u0000\u0000"+
		"\u0000\u0300\u0302\u0001\u0000\u0000\u0000\u0301\u02ff\u0001\u0000\u0000"+
		"\u0000\u0302\u0309\u0005(\u0000\u0000\u0303\u0304\u0005\u0011\u0000\u0000"+
		"\u0304\u0306\u0003f3\u0000\u0305\u0307\u0005\f\u0000\u0000\u0306\u0305"+
		"\u0001\u0000\u0000\u0000\u0306\u0307\u0001\u0000\u0000\u0000\u0307\u0309"+
		"\u0001\u0000\u0000\u0000\u0308\u02f9\u0001\u0000\u0000\u0000\u0308\u0303"+
		"\u0001\u0000\u0000\u0000\u0309q\u0001\u0000\u0000\u0000\u030a\u030b\u0005"+
		"[\u0000\u0000\u030bs\u0001\u0000\u0000\u0000\u030c\u030d\u0007\u0004\u0000"+
		"\u0000\u030du\u0001\u0000\u0000\u0000\u030e\u030f\u0003\u00a6S\u0000\u030f"+
		"\u0311\u0003\u0084B\u0000\u0310\u0312\u0003x<\u0000\u0311\u0310\u0001"+
		"\u0000\u0000\u0000\u0311\u0312\u0001\u0000\u0000\u0000\u0312w\u0001\u0000"+
		"\u0000\u0000\u0313\u0314\u0005\u0016\u0000\u0000\u0314\u0315\u0003\u0082"+
		"A\u0000\u0315y\u0001\u0000\u0000\u0000\u0316\u0318\u0005#\u0000\u0000"+
		"\u0317\u0316\u0001\u0000\u0000\u0000\u0317\u0318\u0001\u0000\u0000\u0000"+
		"\u0318\u0319\u0001\u0000\u0000\u0000\u0319\u031c\u0003`0\u0000\u031a\u031c"+
		"\u0003j5\u0000\u031b\u0317\u0001\u0000\u0000\u0000\u031b\u031a\u0001\u0000"+
		"\u0000\u0000\u031c{\u0001\u0000\u0000\u0000\u031d\u031e\u0005\u000f\u0000"+
		"\u0000\u031e\u032e\u0005Y\u0000\u0000\u031f\u0320\u0005\u000f\u0000\u0000"+
		"\u0320\u0325\u0003f3\u0000\u0321\u0322\u0005W\u0000\u0000\u0322\u0324"+
		"\u0003f3\u0000\u0323\u0321\u0001\u0000\u0000\u0000\u0324\u0327\u0001\u0000"+
		"\u0000\u0000\u0325\u0323\u0001\u0000\u0000\u0000\u0325\u0326\u0001\u0000"+
		"\u0000\u0000\u0326\u0329\u0001\u0000\u0000\u0000\u0327\u0325\u0001\u0000"+
		"\u0000\u0000\u0328\u032a\u0005W\u0000\u0000\u0329\u0328\u0001\u0000\u0000"+
		"\u0000\u0329\u032a\u0001\u0000\u0000\u0000\u032a\u032b\u0001\u0000\u0000"+
		"\u0000\u032b\u032c\u0005Y\u0000\u0000\u032c\u032e\u0001\u0000\u0000\u0000"+
		"\u032d\u031d\u0001\u0000\u0000\u0000\u032d\u031f\u0001\u0000\u0000\u0000"+
		"\u032e}\u0001\u0000\u0000\u0000\u032f\u0330\u0005\u000e\u0000\u0000\u0330"+
		"\u0340\u0005(\u0000\u0000\u0331\u0332\u0005\u000e\u0000\u0000\u0332\u0337"+
		"\u0003\u0080@\u0000\u0333\u0334\u0005W\u0000\u0000\u0334\u0336\u0003\u0080"+
		"@\u0000\u0335\u0333\u0001\u0000\u0000\u0000\u0336\u0339\u0001\u0000\u0000"+
		"\u0000\u0337\u0335\u0001\u0000\u0000\u0000\u0337\u0338\u0001\u0000\u0000"+
		"\u0000\u0338\u033b\u0001\u0000\u0000\u0000\u0339\u0337\u0001\u0000\u0000"+
		"\u0000\u033a\u033c\u0005W\u0000\u0000\u033b\u033a\u0001\u0000\u0000\u0000"+
		"\u033b\u033c\u0001\u0000\u0000\u0000\u033c\u033d\u0001\u0000\u0000\u0000"+
		"\u033d\u033e\u0005(\u0000\u0000\u033e\u0340\u0001\u0000\u0000\u0000\u033f"+
		"\u032f\u0001\u0000\u0000\u0000\u033f\u0331\u0001\u0000\u0000\u0000\u0340"+
		"\u007f\u0001\u0000\u0000\u0000\u0341\u0343\u0003\n\u0005\u0000\u0342\u0341"+
		"\u0001\u0000\u0000\u0000\u0343\u0346\u0001\u0000\u0000\u0000\u0344\u0342"+
		"\u0001\u0000\u0000\u0000\u0344\u0345\u0001\u0000\u0000\u0000\u0345\u0347"+
		"\u0001\u0000\u0000\u0000\u0346\u0344\u0001\u0000\u0000\u0000\u0347\u0348"+
		"\u0007\u0005\u0000\u0000\u0348\u0349\u0005]\u0000\u0000\u0349\u034a\u0003"+
		"f3\u0000\u034a\u0081\u0001\u0000\u0000\u0000\u034b\u034c\u0005K\u0000"+
		"\u0000\u034c\u0351\u0003f3\u0000\u034d\u034e\u0005W\u0000\u0000\u034e"+
		"\u0350\u0003f3\u0000\u034f\u034d\u0001\u0000\u0000\u0000\u0350\u0353\u0001"+
		"\u0000\u0000\u0000\u0351\u034f\u0001\u0000\u0000\u0000\u0351\u0352\u0001"+
		"\u0000\u0000\u0000\u0352\u0355\u0001\u0000\u0000\u0000\u0353\u0351\u0001"+
		"\u0000\u0000\u0000\u0354\u0356\u0005W\u0000\u0000\u0355\u0354\u0001\u0000"+
		"\u0000\u0000\u0355\u0356\u0001\u0000\u0000\u0000\u0356\u0357\u0001\u0000"+
		"\u0000\u0000\u0357\u0358\u0005X\u0000\u0000\u0358\u0362\u0001\u0000\u0000"+
		"\u0000\u0359\u035a\u0005K\u0000\u0000\u035a\u0362\u0005X\u0000\u0000\u035b"+
		"\u0362\u0005J\u0000\u0000\u035c\u035e\u0003\u0088D\u0000\u035d\u035c\u0001"+
		"\u0000\u0000\u0000\u035e\u035f\u0001\u0000\u0000\u0000\u035f\u035d\u0001"+
		"\u0000\u0000\u0000\u035f\u0360\u0001\u0000\u0000\u0000\u0360\u0362\u0001"+
		"\u0000\u0000\u0000\u0361\u034b\u0001\u0000\u0000\u0000\u0361\u0359\u0001"+
		"\u0000\u0000\u0000\u0361\u035b\u0001\u0000\u0000\u0000\u0361\u035d\u0001"+
		"\u0000\u0000\u0000\u0362\u0083\u0001\u0000\u0000\u0000\u0363\u0364\u0005"+
		"K\u0000\u0000\u0364\u0369\u0003\u0086C\u0000\u0365\u0366\u0005W\u0000"+
		"\u0000\u0366\u0368\u0003\u0086C\u0000\u0367\u0365\u0001\u0000\u0000\u0000"+
		"\u0368\u036b\u0001\u0000\u0000\u0000\u0369\u0367\u0001\u0000\u0000\u0000"+
		"\u0369\u036a\u0001\u0000\u0000\u0000\u036a\u036d\u0001\u0000\u0000\u0000"+
		"\u036b\u0369\u0001\u0000\u0000\u0000\u036c\u036e\u0005W\u0000\u0000\u036d"+
		"\u036c\u0001\u0000\u0000\u0000\u036d\u036e\u0001\u0000\u0000\u0000\u036e"+
		"\u036f\u0001\u0000\u0000\u0000\u036f\u0370\u0005X\u0000\u0000\u0370\u037a"+
		"\u0001\u0000\u0000\u0000\u0371\u0372\u0005K\u0000\u0000\u0372\u037a\u0005"+
		"X\u0000\u0000\u0373\u037a\u0005J\u0000\u0000\u0374\u0376\u0003\u0088D"+
		"\u0000\u0375\u0374\u0001\u0000\u0000\u0000\u0376\u0377\u0001\u0000\u0000"+
		"\u0000\u0377\u0375\u0001\u0000\u0000\u0000\u0377\u0378\u0001\u0000\u0000"+
		"\u0000\u0378\u037a\u0001\u0000\u0000\u0000\u0379\u0363\u0001\u0000\u0000"+
		"\u0000\u0379\u0371\u0001\u0000\u0000\u0000\u0379\u0373\u0001\u0000\u0000"+
		"\u0000\u0379\u0375\u0001\u0000\u0000\u0000\u037a\u0085\u0001\u0000\u0000"+
		"\u0000\u037b\u037c\u0007\u0005\u0000\u0000\u037c\u037d\u0005]\u0000\u0000"+
		"\u037d\u0380\u0003f3\u0000\u037e\u0380\u0003f3\u0000\u037f\u037b\u0001"+
		"\u0000\u0000\u0000\u037f\u037e\u0001\u0000\u0000\u0000\u0380\u0087\u0001"+
		"\u0000\u0000\u0000\u0381\u0382\u0005\u0004\u0000\u0000\u0382\u0383\u0003"+
		"\u008aE\u0000\u0383\u0384\u0005O\u0000\u0000\u0384\u0089\u0001\u0000\u0000"+
		"\u0000\u0385\u0389\u0005N\u0000\u0000\u0386\u0389\u0003\u00b8\\\u0000"+
		"\u0387\u0389\u0003\u008cF\u0000\u0388\u0385\u0001\u0000\u0000\u0000\u0388"+
		"\u0386\u0001\u0000\u0000\u0000\u0388\u0387\u0001\u0000\u0000\u0000\u0389"+
		"\u038c\u0001\u0000\u0000\u0000\u038a\u0388\u0001\u0000\u0000\u0000\u038a"+
		"\u038b\u0001\u0000\u0000\u0000\u038b\u008b\u0001\u0000\u0000\u0000\u038c"+
		"\u038a\u0001\u0000\u0000\u0000\u038d\u038f\u0005\t\u0000\u0000\u038e\u0390"+
		"\u0003f3\u0000\u038f\u038e\u0001\u0000\u0000\u0000\u038f\u0390\u0001\u0000"+
		"\u0000\u0000\u0390\u0391\u0001\u0000\u0000\u0000\u0391\u0392\u0005Y\u0000"+
		"\u0000\u0392\u008d\u0001\u0000\u0000\u0000\u0393\u0395\u0005\t\u0000\u0000"+
		"\u0394\u0396\u0003\u0098L\u0000\u0395\u0394\u0001\u0000\u0000\u0000\u0396"+
		"\u0397\u0001\u0000\u0000\u0000\u0397\u0395\u0001\u0000\u0000\u0000\u0397"+
		"\u0398\u0001\u0000\u0000\u0000\u0398\u0399\u0001\u0000\u0000\u0000\u0399"+
		"\u039a\u0005Y\u0000\u0000\u039a\u008f\u0001\u0000\u0000\u0000\u039b\u039f"+
		"\u0003\u0094J\u0000\u039c\u039e\u0003\u0098L\u0000\u039d\u039c\u0001\u0000"+
		"\u0000\u0000\u039e\u03a1\u0001\u0000\u0000\u0000\u039f\u039d\u0001\u0000"+
		"\u0000\u0000\u039f\u03a0\u0001\u0000\u0000\u0000\u03a0\u0091\u0001\u0000"+
		"\u0000\u0000\u03a1\u039f\u0001\u0000\u0000\u0000\u03a2\u03a3\u0005\u0015"+
		"\u0000\u0000\u03a3\u03a4\u0003\u0082A\u0000\u03a4\u0093\u0001\u0000\u0000"+
		"\u0000\u03a5\u03aa\u0003\u0092I\u0000\u03a6\u03aa\u0005\r\u0000\u0000"+
		"\u03a7\u03a8\u0005Z\u0000\u0000\u03a8\u03aa\u0007\u0006\u0000\u0000\u03a9"+
		"\u03a5\u0001\u0000\u0000\u0000\u03a9\u03a6\u0001\u0000\u0000\u0000\u03a9"+
		"\u03a7\u0001\u0000\u0000\u0000\u03aa\u0095\u0001\u0000\u0000\u0000\u03ab"+
		"\u03ac\u0005\\\u0000\u0000\u03ac\u03ad\u0003f3\u0000\u03ad\u0097\u0001"+
		"\u0000\u0000\u0000\u03ae\u03af\u0003\u009aM\u0000\u03af\u03b1\u0003\u00a6"+
		"S\u0000\u03b0\u03b2\u0003\u0084B\u0000\u03b1\u03b0\u0001\u0000\u0000\u0000"+
		"\u03b1\u03b2\u0001\u0000\u0000\u0000\u03b2\u03b4\u0001\u0000\u0000\u0000"+
		"\u03b3\u03b5\u0003x<\u0000\u03b4\u03b3\u0001\u0000\u0000\u0000\u03b4\u03b5"+
		"\u0001\u0000\u0000\u0000\u03b5\u03b9\u0001\u0000\u0000\u0000\u03b6\u03b9"+
		"\u0003\u009eO\u0000\u03b7\u03b9\u0005V\u0000\u0000\u03b8\u03ae\u0001\u0000"+
		"\u0000\u0000\u03b8\u03b6\u0001\u0000\u0000\u0000\u03b8\u03b7\u0001\u0000"+
		"\u0000\u0000\u03b9\u0099\u0001\u0000\u0000\u0000\u03ba\u03bb\u0007\u0007"+
		"\u0000\u0000\u03bb\u009b\u0001\u0000\u0000\u0000\u03bc\u03bd\u0005E\u0000"+
		"\u0000\u03bd\u03be\u0005f\u0000\u0000\u03be\u009d\u0001\u0000\u0000\u0000"+
		"\u03bf\u03c0\u0005U\u0000\u0000\u03c0\u03c1\u0005e\u0000\u0000\u03c1\u009f"+
		"\u0001\u0000\u0000\u0000\u03c2\u03c6\u0005\n\u0000\u0000\u03c3\u03c6\u0005"+
		"\u000b\u0000\u0000\u03c4\u03c6\u0003\u0088D\u0000\u03c5\u03c2\u0001\u0000"+
		"\u0000\u0000\u03c5\u03c3\u0001\u0000\u0000\u0000\u03c5\u03c4\u0001\u0000"+
		"\u0000\u0000\u03c6\u00a1\u0001\u0000\u0000\u0000\u03c7\u03ca\u0005\n\u0000"+
		"\u0000\u03c8\u03ca\u0003\u0088D\u0000\u03c9\u03c7\u0001\u0000\u0000\u0000"+
		"\u03c9\u03c8\u0001\u0000\u0000\u0000\u03ca\u00a3\u0001\u0000\u0000\u0000"+
		"\u03cb\u03cc\u0005\n\u0000\u0000\u03cc\u00a5\u0001\u0000\u0000\u0000\u03cd"+
		"\u03ce\u0007\b\u0000\u0000\u03ce\u00a7\u0001\u0000\u0000\u0000\u03cf\u03d0"+
		"\u0007\t\u0000\u0000\u03d0\u00a9\u0001\u0000\u0000\u0000\u03d1\u03d2\u0007"+
		"\u0003\u0000\u0000\u03d2\u00ab\u0001\u0000\u0000\u0000\u03d3\u03d4\u0007"+
		"\n\u0000\u0000\u03d4\u00ad\u0001\u0000\u0000\u0000\u03d5\u03d6\u00054"+
		"\u0000\u0000\u03d6\u00af\u0001\u0000\u0000\u0000\u03d7\u03d8\u0007\u000b"+
		"\u0000\u0000\u03d8\u00b1\u0001\u0000\u0000\u0000\u03d9\u03da\u0007\f\u0000"+
		"\u0000\u03da\u00b3\u0001\u0000\u0000\u0000\u03db\u03de\u0003\u00b6[\u0000"+
		"\u03dc\u03de\u0005#\u0000\u0000\u03dd\u03db\u0001\u0000\u0000\u0000\u03dd"+
		"\u03dc\u0001\u0000\u0000\u0000\u03de\u00b5\u0001\u0000\u0000\u0000\u03df"+
		"\u03e0\u0007\r\u0000\u0000\u03e0\u00b7\u0001\u0000\u0000\u0000\u03e1\u03e2"+
		"\u0005\u0003\u0000\u0000\u03e2\u03e3\u0007\u000e\u0000\u0000\u03e3\u00b9"+
		"\u0001\u0000\u0000\u0000\u0081\u00bd\u00c1\u00c7\u00cc\u00d5\u00dc\u00e3"+
		"\u00ea\u00f0\u00f6\u00fc\u0102\u0109\u010d\u0110\u0119\u011f\u0124\u012b"+
		"\u0130\u0139\u013e\u0149\u014b\u0153\u015c\u0162\u0167\u016e\u0173\u017c"+
		"\u0182\u0186\u0189\u018b\u018f\u0192\u0199\u019e\u01a4\u01ae\u01b2\u01b6"+
		"\u01c0\u01c4\u01c8\u01d5\u01d9\u01e0\u01e3\u01e7\u01eb\u01f3\u01f5\u01fe"+
		"\u0204\u020a\u0217\u021c\u0221\u0229\u0230\u0239\u0246\u0248\u024c\u0255"+
		"\u025f\u0265\u0267\u026b\u0274\u0277\u027c\u027f\u0285\u0287\u0290\u0295"+
		"\u029d\u02a0\u02a4\u02aa\u02b4\u02b6\u02bc\u02c0\u02cb\u02cf\u02d4\u02da"+
		"\u02ea\u02f0\u02f2\u02fd\u02ff\u0306\u0308\u0311\u0317\u031b\u0325\u0329"+
		"\u032d\u0337\u033b\u033f\u0344\u0351\u0355\u035f\u0361\u0369\u036d\u0377"+
		"\u0379\u037f\u0388\u038a\u038f\u0397\u039f\u03a9\u03b1\u03b4\u03b8\u03c5"+
		"\u03c9\u03dd";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}