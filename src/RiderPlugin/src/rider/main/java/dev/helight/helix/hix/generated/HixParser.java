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
		STATIC_CALL=22, DYNAMIC_CALL=23, BEGIN_INLINE_BLOCK=24, FAT_ARROW=25,
		NUMBER=26, STRING=27, BOOLEAN=28, NULL=29, MISSING=30, QUESTION=31, PIPE=32,
		ESCAPED_AT=33, ESCAPED_START=34, OUTER_WHITESPACE=35, NEWLINE=36, COMMENT=37,
		SLASH_COMMENT=38, SECTION_DELIMITER=39, LC=40, RC=41, SEMICOLON=42, COMMA=43,
		KEYWORD_FUNC=44, KEYWORD_DELEGATE=45, KEYWORD_TYPE=46, KEYWORD_DO=47,
		KEYWORD_EXTERNAL=48, KEYWORD_EXPRESSION=49, KEYWORD_MIXIN=50, KEYWORD_PRELUDE=51,
		KEYWORD_DERIVATION=52, KEYWORD_ELSE=53, KEYWORD_RETURN=54, KEYWORD_GOTO=55,
		KEYWORD_BREAK=56, KEYWORD_CONTINUE=57, KEYWORD_LOCAL=58, KEYWORD_CARRY=59,
		KEYWORD_WHEN=60, KEYWORD_PURE=61, KEYWORD_META=62, KEYWORD_FOR=63, KEYWORD_INLINE=64,
		KEYWORD_NOINLINE=65, KEYWORD_STRICT=66, LABEL_PREFIX=67, TOPLEVEL_DERIVATION=68,
		ASSIGN=69, ARROW=70, TOPLEVEL_VALUE_EXPRESSION=71, EMPTY_PARAMETERS=72,
		BEGIN_PARAMETERS=73, METADATA_WHITESPACE=74, DERIVATION_WHITESPACE=75,
		ARGUMENT_TEXT=76, ARGUMENT_END=77, CONTENT_WRAP=78, CONTENT_LINEBREAK=79,
		CONTENT_TEXT=80, VALUE_FUNCTION=81, VALUE_PREDICATE=82, VALUE_MEMBER=83,
		VALUE_WRAP=84, VALUE_DELIMITER=85, END_PARAMETERS=86, VALUE_END_INLINE=87,
		VALUE_SMART_ROOT=88, NOT_VALUE=89, VALUE_ELVIS=90, VALUE_ASSIGN=91, VALUE_EXPAND=92,
		VALUE_WHITESPACE=93, ESCAPE_MACRO=94, ESCAPE_LITERAL=95, ESCAPE_HEX=96,
		END_CONTENT_INTERPOLATE=97, FUNCTION_IDENTIFIER=98, MEMBER_IDENTIFIER=99,
		LABEL_IDENTIFIER=100, TOPLEVEL_QUESTION=101, TOPLEVEL_PIPE=102, TOPLEVEL_SMART_ROOT=103,
		TOPLEVEL_NULL=104, VALUE_CAST_BOOLEAN=105, VALUE_EXPLICIT_ROOT=106, VALUE_STATIC_CALL=107,
		VALUE_DYNAMIC_CALL=108, VALUE_BLOCK_RESULT=109, VALUE_MISSING=110;
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
		RULE_funcModifier = 88, RULE_trivia = 89, RULE_comment = 90, RULE_escaped = 91;
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
			"expressionModifier", "mixinModifier", "funcModifier", "trivia", "comment",
			"escaped"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, "'@@'", null, null,
			null, null, null, "'---'", "'{'", null, null, null, "'func'", null, "'type'",
			"'do'", "'external'", "'expression'", "'mixin'", "'prelude'", "'derivation'",
			"'else'", "'return'", "'goto'", "'break'", "'continue'", "'local'", "'carry'",
			"'when'", "'pure'", "'meta'", "'for'", "'inline'", "'noinline'", "'strict'",
			null, null, null, null, null, null, "'('", null, null, null, "'>'", null,
			null, null, null, "':?'", "'#'", null, null, "')'", "']'", null, "'!'",
			"'?:'", null, "'...'", null, null, null, null, null, null, null, null,
			null, null, null, null, "'@bool'", "'@root'", "'@static'", "'@dynamic'",
			"'=>'", "'missing'"
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
			"STATIC_CALL", "DYNAMIC_CALL", "BEGIN_INLINE_BLOCK", "FAT_ARROW", "NUMBER",
			"STRING", "BOOLEAN", "NULL", "MISSING", "QUESTION", "PIPE", "ESCAPED_AT",
			"ESCAPED_START", "OUTER_WHITESPACE", "NEWLINE", "COMMENT", "SLASH_COMMENT",
			"SECTION_DELIMITER", "LC", "RC", "SEMICOLON", "COMMA", "KEYWORD_FUNC",
			"KEYWORD_DELEGATE", "KEYWORD_TYPE", "KEYWORD_DO", "KEYWORD_EXTERNAL",
			"KEYWORD_EXPRESSION", "KEYWORD_MIXIN", "KEYWORD_PRELUDE", "KEYWORD_DERIVATION",
			"KEYWORD_ELSE", "KEYWORD_RETURN", "KEYWORD_GOTO", "KEYWORD_BREAK", "KEYWORD_CONTINUE",
			"KEYWORD_LOCAL", "KEYWORD_CARRY", "KEYWORD_WHEN", "KEYWORD_PURE", "KEYWORD_META",
			"KEYWORD_FOR", "KEYWORD_INLINE", "KEYWORD_NOINLINE", "KEYWORD_STRICT",
			"LABEL_PREFIX", "TOPLEVEL_DERIVATION", "ASSIGN", "ARROW", "TOPLEVEL_VALUE_EXPRESSION",
			"EMPTY_PARAMETERS", "BEGIN_PARAMETERS", "METADATA_WHITESPACE", "DERIVATION_WHITESPACE",
			"ARGUMENT_TEXT", "ARGUMENT_END", "CONTENT_WRAP", "CONTENT_LINEBREAK",
			"CONTENT_TEXT", "VALUE_FUNCTION", "VALUE_PREDICATE", "VALUE_MEMBER",
			"VALUE_WRAP", "VALUE_DELIMITER", "END_PARAMETERS", "VALUE_END_INLINE",
			"VALUE_SMART_ROOT", "NOT_VALUE", "VALUE_ELVIS", "VALUE_ASSIGN", "VALUE_EXPAND",
			"VALUE_WHITESPACE", "ESCAPE_MACRO", "ESCAPE_LITERAL", "ESCAPE_HEX", "END_CONTENT_INTERPOLATE",
			"FUNCTION_IDENTIFIER", "MEMBER_IDENTIFIER", "LABEL_IDENTIFIER", "TOPLEVEL_QUESTION",
			"TOPLEVEL_PIPE", "TOPLEVEL_SMART_ROOT", "TOPLEVEL_NULL", "VALUE_CAST_BOOLEAN",
			"VALUE_EXPLICIT_ROOT", "VALUE_STATIC_CALL", "VALUE_DYNAMIC_CALL", "VALUE_BLOCK_RESULT",
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
			setState(187);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
				{
				{
				setState(184);
				trivia();
				}
				}
				setState(189);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(191);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,1,_ctx) ) {
			case 1:
				{
				setState(190);
				fileMetadataSection();
				}
				break;
			}
			setState(202);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 18)) & ~0x3f) == 0 && ((1L << (_la - 18)) & 237516321980419L) != 0)) {
				{
				{
				setState(193);
				topLevelDeclaration();
				setState(197);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
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
				}
				}
				setState(204);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(205);
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
			setState(207);
			metadataList();
			setState(211);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
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
			setState(214);
			match(SECTION_DELIMITER);
			setState(218);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
				{
				{
				setState(215);
				trivia();
				}
				}
				setState(220);
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
			setState(238);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(221);
				metadataList();
				setState(225);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
					{
					{
					setState(222);
					trivia();
					}
					}
					setState(227);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(232);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_MIXIN:
				case KEYWORD_DERIVATION:
					{
					setState(228);
					mixinDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(229);
					funcDeclaration();
					}
					break;
				case KEYWORD_TYPE:
					{
					setState(230);
					typeDeclaration();
					}
					break;
				case KEYWORD_META:
					{
					setState(231);
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
				setState(234);
				mixinDeclaration();
				}
				break;
			case KEYWORD_FUNC:
			case KEYWORD_PURE:
			case KEYWORD_INLINE:
			case KEYWORD_NOINLINE:
				enterOuterAlt(_localctx, 3);
				{
				setState(235);
				funcDeclaration();
				}
				break;
			case KEYWORD_TYPE:
				enterOuterAlt(_localctx, 4);
				{
				setState(236);
				typeDeclaration();
				}
				break;
			case KEYWORD_META:
				enterOuterAlt(_localctx, 5);
				{
				setState(237);
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
			setState(240);
			metadata();
			setState(250);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,10,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(244);
					_errHandler.sync(this);
					_la = _input.LA(1);
					while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
						{
						{
						setState(241);
						trivia();
						}
						}
						setState(246);
						_errHandler.sync(this);
						_la = _input.LA(1);
					}
					setState(247);
					metadata();
					}
					}
				}
				setState(252);
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
			setState(254);
			_errHandler.sync(this);
			_alt = 1;
			do {
				switch (_alt) {
				case 1:
					{
					{
					setState(253);
					metadata();
					}
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				setState(256);
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
			setState(270);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(258);
				match(METADATA_PREFIX);
				setState(259);
				match(IDENTIFIER);
				setState(263);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,12,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(260);
						argumentValue();
						}
						}
					}
					setState(265);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,12,_ctx);
				}
				setState(267);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==BEGIN_ARGUMENT || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
					{
					setState(266);
					valueList();
					}
				}

				}
				break;
			case BEGIN_METADATA_VALUE:
				enterOuterAlt(_localctx, 2);
				{
				setState(269);
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
			setState(272);
			match(BEGIN_METADATA_VALUE);
			setState(273);
			value();
			setState(274);
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
			setState(279);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_DERIVATION) {
				{
				{
				setState(276);
				mixinModifier();
				}
				}
				setState(281);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(282);
			match(KEYWORD_MIXIN);
			setState(283);
			mixinIdentifier();
			setState(285);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(284);
				patternParameterList();
				}
			}

			setState(287);
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
			setState(289);
			match(KEYWORD_TYPE);
			setState(290);
			match(IDENTIFIER);
			setState(291);
			match(ASSIGN);
			setState(292);
			patternExpression();
			setState(294);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==SEMICOLON) {
				{
				setState(293);
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
		public TerminalNode IDENTIFIER() { return getToken(HixParser.IDENTIFIER, 0); }
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
			setState(296);
			match(KEYWORD_META);
			setState(297);
			match(IDENTIFIER);
			setState(298);
			patternParameterList();
			setState(299);
			match(KEYWORD_FOR);
			setState(300);
			metaTarget();
			setState(307);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1768014697464832L) != 0)) {
				{
				{
				setState(302);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==COMMA) {
					{
					setState(301);
					match(COMMA);
					}
				}

				setState(304);
				metaTarget();
				}
				}
				setState(309);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(310);
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
			setState(312);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 1759218604442624L) != 0)) ) {
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
			setState(314);
			match(LC);
			setState(320);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 36)) & ~0x3f) == 0 && ((1L << (_la - 36)) & 1912643847L) != 0)) {
				{
				setState(318);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_EXPRESSION:
				case KEYWORD_PRELUDE:
				case KEYWORD_STRICT:
					{
					setState(315);
					expressionDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(316);
					funcDeclaration();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(317);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(322);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(323);
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
			setState(328);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_PRELUDE || _la==KEYWORD_STRICT) {
				{
				{
				setState(325);
				expressionModifier();
				}
				}
				setState(330);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(331);
			match(KEYWORD_EXPRESSION);
			setState(332);
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
			setState(337);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 61)) & ~0x3f) == 0 && ((1L << (_la - 61)) & 25L) != 0)) {
				{
				{
				setState(334);
				funcModifier();
				}
				}
				setState(339);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(340);
			match(KEYWORD_FUNC);
			setState(341);
			functionDeclarationIdentifier();
			setState(343);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BEGIN_VALUE_INLINE || ((((_la - 70)) & ~0x3f) == 0 && ((1L << (_la - 70)) & 13L) != 0)) {
				{
				setState(342);
				directFunctionSignature();
				}
			}

			setState(348);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
				{
				{
				setState(345);
				trivia();
				}
				}
				setState(350);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(351);
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
			setState(355);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case EMPTY_PARAMETERS:
			case BEGIN_PARAMETERS:
				{
				setState(353);
				patternParameterList();
				}
				break;
			case BEGIN_VALUE_INLINE:
				{
				setState(354);
				collectiveInputPattern();
				}
				break;
			case ARROW:
				break;
			default:
				break;
			}
			setState(357);
			match(ARROW);
			setState(360);
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
				setState(358);
				patternExpression();
				}
				break;
			case EMPTY_PARAMETERS:
			case BEGIN_PARAMETERS:
				{
				setState(359);
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
			setState(362);
			match(BEGIN_VALUE_INLINE);
			setState(363);
			patternExpression();
			setState(364);
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
			setState(366);
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
			setState(384);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case LC:
			case KEYWORD_DO:
				enterOuterAlt(_localctx, 1);
				{
				setState(369);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_DO) {
					{
					setState(368);
					match(KEYWORD_DO);
					}
				}

				setState(371);
				statementBlock();
				}
				break;
			case FAT_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(372);
				match(FAT_ARROW);
				setState(373);
				value();
				setState(375);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_END) {
					{
					setState(374);
					match(VALUE_END);
					}
				}

				}
				break;
			case KEYWORD_EXTERNAL:
				enterOuterAlt(_localctx, 3);
				{
				setState(377);
				match(KEYWORD_EXTERNAL);
				setState(379);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==IDENTIFIER) {
					{
					setState(378);
					match(IDENTIFIER);
					}
				}

				setState(382);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==SEMICOLON) {
					{
					setState(381);
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
			setState(391);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(386);
				inlineMetadataList();
				setState(388);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,33,_ctx) ) {
				case 1:
					{
					setState(387);
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
				setState(390);
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
			setState(393);
			patternTerm();
			setState(398);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,35,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(394);
					match(PIPE);
					setState(395);
					patternTerm();
					}
					}
				}
				setState(400);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,35,_ctx);
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
			setState(401);
			patternPrimary();
			setState(403);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,36,_ctx) ) {
			case 1:
				{
				setState(402);
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
			setState(409);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case NULL:
			case MISSING:
				enterOuterAlt(_localctx, 1);
				{
				setState(405);
				patternIdentifier();
				}
				break;
			case BEGIN_TABLE:
				enterOuterAlt(_localctx, 2);
				{
				setState(406);
				tablePattern();
				}
				break;
			case BEGIN_TUPLE:
				enterOuterAlt(_localctx, 3);
				{
				setState(407);
				tuplePattern();
				}
				break;
			case KEYWORD_DELEGATE:
				enterOuterAlt(_localctx, 4);
				{
				setState(408);
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
			setState(427);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,40,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(411);
				match(BEGIN_TABLE);
				setState(412);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(413);
				match(BEGIN_TABLE);
				setState(414);
				patternField();
				setState(419);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,38,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(415);
						match(VALUE_DELIMITER);
						setState(416);
						patternField();
						}
						}
					}
					setState(421);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,38,_ctx);
				}
				setState(423);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(422);
					match(VALUE_DELIMITER);
					}
				}

				setState(425);
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
			setState(445);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,43,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(429);
				match(BEGIN_TUPLE);
				setState(430);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(431);
				match(BEGIN_TUPLE);
				setState(432);
				patternField();
				setState(437);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,41,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(433);
						match(VALUE_DELIMITER);
						setState(434);
						patternField();
						}
						}
					}
					setState(439);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,41,_ctx);
				}
				setState(441);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(440);
					match(VALUE_DELIMITER);
					}
				}

				setState(443);
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
			setState(447);
			match(KEYWORD_DELEGATE);
			setState(448);
			patternParameterList();
			setState(449);
			match(ARROW);
			setState(450);
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
			setState(469);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,46,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(452);
				match(BEGIN_PARAMETERS);
				setState(453);
				patternField();
				setState(458);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,44,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(454);
						match(VALUE_DELIMITER);
						setState(455);
						patternField();
						}
						}
					}
					setState(460);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,44,_ctx);
				}
				setState(462);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(461);
					match(VALUE_DELIMITER);
					}
				}

				setState(464);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(466);
				match(BEGIN_PARAMETERS);
				setState(467);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(468);
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
		enterRule(_localctx, 52, RULE_patternField);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(472);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				setState(471);
				metadataList();
				}
			}

			setState(474);
			patternUnion();
			setState(476);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ROOT_IDENTIFIER) {
				{
				setState(475);
				match(ROOT_IDENTIFIER);
				}
			}

			setState(480);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==VALUE_ASSIGN) {
				{
				setState(478);
				match(VALUE_ASSIGN);
				setState(479);
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
			setState(482);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 1610621952L) != 0)) ) {
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
			setState(484);
			match(LC);
			setState(490);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 2287834589299475456L) != 0) || ((((_la - 67)) & ~0x3f) == 0 && ((1L << (_la - 67)) & 2097171L) != 0)) {
				{
				setState(488);
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
				case TOPLEVEL_VALUE_EXPRESSION:
				case VALUE_SMART_ROOT:
					{
					setState(485);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(486);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(487);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(492);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(493);
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
			setState(505);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(495);
				metadataList();
				setState(499);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
					{
					{
					setState(496);
					trivia();
					}
					}
					setState(501);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(502);
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
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
				enterOuterAlt(_localctx, 2);
				{
				setState(504);
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
			setState(523);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,55,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(507);
				labelIdentifier();
				setState(508);
				match(NEWLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(511);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LABEL_PREFIX) {
					{
					setState(510);
					labelIdentifier();
					}
				}

				setState(513);
				statementBlock();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(514);
				invocationStatement();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(515);
				localDeclarationStatement();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(516);
				localAssignmentStatement();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(517);
				toplevelDerivationStatement();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(518);
				controlflowStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(519);
				whenValueStatement();
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(520);
				whenConditionStatement();
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(521);
				whenChainStatement();
				}
				break;
			case 11:
				enterOuterAlt(_localctx, 11);
				{
				setState(522);
				valueExpression();
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
			setState(533);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,57,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(525);
				invocationIdentifier();
				setState(526);
				callValueList();
				setState(528);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,56,_ctx) ) {
				case 1:
					{
					setState(527);
					tailValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(530);
				invocationIdentifier();
				setState(531);
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
			setState(535);
			match(KEYWORD_WHEN);
			setState(536);
			whenChainCondition();
			setState(537);
			statementBlock();
			setState(548);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,59,_ctx) ) {
			case 1:
				{
				setState(541);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
					{
					{
					setState(538);
					trivia();
					}
					}
					setState(543);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(544);
				match(KEYWORD_ELSE);
				setState(545);
				whenResult();
				setState(546);
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
			setState(550);
			match(KEYWORD_ELSE);
			setState(551);
			match(ARROW);
			setState(552);
			whenResult();
			setState(553);
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
			setState(557);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,60,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(555);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(556);
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
			setState(559);
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
			setState(561);
			match(KEYWORD_WHEN);
			setState(562);
			match(LC);
			setState(563);
			match(NEWLINE);
			setState(564);
			whenChainBody();
			setState(565);
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
			setState(567);
			whenChainBranch();
			setState(572);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337168L) != 0) || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(570);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case EMPTY_PARAMETERS:
				case BEGIN_PARAMETERS:
					{
					setState(568);
					whenChainBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(569);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(574);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(576);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(575);
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
			setState(578);
			whenChainCondition();
			setState(579);
			match(ARROW);
			setState(580);
			whenResult();
			setState(581);
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
			setState(583);
			match(KEYWORD_WHEN);
			setState(585);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 2099503632L) != 0) || ((((_la - 71)) & ~0x3f) == 0 && ((1L << (_la - 71)) & 134610945L) != 0)) {
				{
				setState(584);
				value();
				}
			}

			setState(587);
			match(LC);
			setState(588);
			match(NEWLINE);
			setState(589);
			whenValueBody();
			setState(590);
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
			setState(595);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
				{
				{
				setState(592);
				trivia();
				}
				}
				setState(597);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(598);
			whenValueBranch();
			setState(603);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 483135840784L) != 0) || ((((_la - 71)) & ~0x3f) == 0 && ((1L << (_la - 71)) & 134610945L) != 0)) {
				{
				setState(601);
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
					{
					setState(599);
					whenValueBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(600);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(605);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(607);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(606);
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
			setState(609);
			whenValueCondition();
			setState(610);
			match(ARROW);
			setState(611);
			whenResult();
			setState(612);
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
			setState(616);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,69,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(614);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(615);
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
			setState(635);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,74,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(619);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(618);
					match(KEYWORD_CARRY);
					}
				}

				setState(621);
				match(KEYWORD_LOCAL);
				setState(622);
				variableIdentifier();
				setState(624);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,71,_ctx) ) {
				case 1:
					{
					setState(623);
					assignedValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(627);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(626);
					match(KEYWORD_CARRY);
					}
				}

				setState(629);
				match(KEYWORD_LOCAL);
				setState(630);
				patternExpression();
				setState(631);
				variableIdentifier();
				setState(633);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,73,_ctx) ) {
				case 1:
					{
					setState(632);
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
			setState(637);
			match(VALUE_SMART_ROOT);
			setState(638);
			variableIdentifier();
			setState(639);
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
			enterOuterAlt(_localctx, 1);
			{
			setState(641);
			match(TOPLEVEL_DERIVATION);
			setState(642);
			functionIdentifier();
			setState(644);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BEGIN_ARGUMENT || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(643);
				callValueList();
				}
			}

			setState(649);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 81)) & ~0x3f) == 0 && ((1L << (_la - 81)) & 15L) != 0)) {
				{
				{
				setState(646);
				transformationPart();
				}
				}
				setState(651);
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
		enterRule(_localctx, 92, RULE_assignedValue);
		try {
			setState(660);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ASSIGN:
				enterOuterAlt(_localctx, 1);
				{
				setState(652);
				match(ASSIGN);
				setState(657);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,77,_ctx) ) {
				case 1:
					{
					setState(653);
					value();
					}
					break;
				case 2:
					{
					setState(654);
					whenValueStatement();
					}
					break;
				case 3:
					{
					setState(655);
					whenChainStatement();
					}
					break;
				case 4:
					{
					setState(656);
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
				enterOuterAlt(_localctx, 2);
				{
				setState(659);
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
			setState(670);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_RETURN:
				enterOuterAlt(_localctx, 1);
				{
				setState(662);
				match(KEYWORD_RETURN);
				setState(664);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==BEGIN_ARGUMENT || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
					{
					setState(663);
					valueList();
					}
				}

				}
				break;
			case KEYWORD_GOTO:
				enterOuterAlt(_localctx, 2);
				{
				setState(666);
				match(KEYWORD_GOTO);
				setState(667);
				match(IDENTIFIER);
				}
				break;
			case KEYWORD_CONTINUE:
				enterOuterAlt(_localctx, 3);
				{
				setState(668);
				match(KEYWORD_CONTINUE);
				}
				break;
			case KEYWORD_BREAK:
				enterOuterAlt(_localctx, 4);
				{
				setState(669);
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
			setState(672);
			match(BEGIN_CONTENT);
			setState(673);
			contentBody();
			setState(674);
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
			setState(680);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				setState(680);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case CONTENT_WRAP:
					{
					setState(676);
					match(CONTENT_WRAP);
					}
					break;
				case CONTENT_LINEBREAK:
					{
					setState(677);
					match(CONTENT_LINEBREAK);
					}
					break;
				case CONTENT_TEXT:
					{
					setState(678);
					match(CONTENT_TEXT);
					}
					break;
				case BEGIN_VALUE_INTERPOLATE:
					{
					setState(679);
					contentInterpolate();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(682);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( _la==BEGIN_VALUE_INTERPOLATE || ((((_la - 78)) & ~0x3f) == 0 && ((1L << (_la - 78)) & 7L) != 0) );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
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
			setState(684);
			match(BEGIN_VALUE_INTERPOLATE);
			setState(685);
			derivation();
			setState(686);
			match(RC);
			setState(688);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==END_CONTENT_INTERPOLATE) {
				{
				setState(687);
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
			setState(692);
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
				enterOuterAlt(_localctx, 1);
				{
				setState(690);
				nonArgumentValue(0);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(691);
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
			setState(694);
			match(TOPLEVEL_VALUE_EXPRESSION);
			setState(695);
			value();
			setState(696);
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
			setState(712);
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
				{
				setState(699);
				primaryValue();
				setState(703);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,85,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(700);
						transformationPart();
						}
						}
					}
					setState(705);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,85,_ctx);
				}
				setState(707);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,86,_ctx) ) {
				case 1:
					{
					setState(706);
					elvisValue();
					}
					break;
				}
				}
				break;
			case NOT_VALUE:
				{
				setState(709);
				prefixOperators();
				setState(710);
				nonArgumentValue(2);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(718);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,88,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					{
					_localctx = new NonArgumentValueContext(_parentctx, _parentState);
					pushNewRecursionContext(_localctx, _startState, RULE_nonArgumentValue);
					setState(714);
					if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
					setState(715);
					postfixOperators();
					}
					}
				}
				setState(720);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,88,_ctx);
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
			setState(734);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,89,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(721);
				inlineValue();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(722);
				lambdaValue();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(723);
				inlineBlockValue();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(724);
				derivation();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(725);
				tableValue();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(726);
				tupleValue();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(727);
				valueExpression();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(728);
				valueStatement();
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(729);
				match(NUMBER);
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(730);
				match(STRING);
				}
				break;
			case 11:
				enterOuterAlt(_localctx, 11);
				{
				setState(731);
				match(BOOLEAN);
				}
				break;
			case 12:
				enterOuterAlt(_localctx, 12);
				{
				setState(732);
				match(NULL);
				}
				break;
			case 13:
				enterOuterAlt(_localctx, 13);
				{
				setState(733);
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
			setState(736);
			match(BEGIN_INLINE_BLOCK);
			setState(742);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 2287834589299475456L) != 0) || ((((_la - 67)) & ~0x3f) == 0 && ((1L << (_la - 67)) & 2097171L) != 0)) {
				{
				setState(740);
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
				case TOPLEVEL_VALUE_EXPRESSION:
				case VALUE_SMART_ROOT:
					{
					setState(737);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(738);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(739);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(744);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(745);
			match(RC);
			setState(746);
			match(FAT_ARROW);
			setState(747);
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
			setState(764);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_LAMBDA_BLOCK:
				enterOuterAlt(_localctx, 1);
				{
				setState(749);
				match(BEGIN_LAMBDA_BLOCK);
				setState(755);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 2287834589299475456L) != 0) || ((((_la - 67)) & ~0x3f) == 0 && ((1L << (_la - 67)) & 2097171L) != 0)) {
					{
					setState(753);
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
					case TOPLEVEL_VALUE_EXPRESSION:
					case VALUE_SMART_ROOT:
						{
						setState(750);
						statement();
						}
						break;
					case SEMICOLON:
						{
						setState(751);
						match(SEMICOLON);
						}
						break;
					case NEWLINE:
					case COMMENT:
					case SLASH_COMMENT:
						{
						setState(752);
						trivia();
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					}
					setState(757);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(758);
				match(RC);
				}
				break;
			case BEGIN_LAMBDA_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(759);
				match(BEGIN_LAMBDA_ARROW);
				setState(760);
				value();
				setState(762);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,94,_ctx) ) {
				case 1:
					{
					setState(761);
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
			setState(766);
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
			setState(768);
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
			setState(770);
			functionIdentifier();
			setState(771);
			callValueList();
			setState(773);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,96,_ctx) ) {
			case 1:
				{
				setState(772);
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
		public TerminalNode DYNAMIC_CALL() { return getToken(HixParser.DYNAMIC_CALL, 0); }
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
			setState(778);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case STATIC_CALL:
				enterOuterAlt(_localctx, 1);
				{
				setState(775);
				match(STATIC_CALL);
				setState(776);
				valueList();
				}
				break;
			case DYNAMIC_CALL:
				enterOuterAlt(_localctx, 2);
				{
				setState(777);
				match(DYNAMIC_CALL);
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
			setState(785);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_CONTENT:
			case NEWLINE:
				enterOuterAlt(_localctx, 1);
				{
				setState(781);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==NEWLINE) {
					{
					setState(780);
					match(NEWLINE);
					}
				}

				setState(783);
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
				enterOuterAlt(_localctx, 2);
				{
				setState(784);
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
			setState(803);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,102,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(787);
				match(BEGIN_TUPLE);
				setState(788);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(789);
				match(BEGIN_TUPLE);
				setState(790);
				value();
				setState(795);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,100,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(791);
						match(VALUE_DELIMITER);
						setState(792);
						value();
						}
						}
					}
					setState(797);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,100,_ctx);
				}
				setState(799);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(798);
					match(VALUE_DELIMITER);
					}
				}

				setState(801);
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
			setState(821);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,105,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(805);
				match(BEGIN_TABLE);
				setState(806);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(807);
				match(BEGIN_TABLE);
				setState(808);
				tableKeyedEntry();
				setState(813);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,103,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(809);
						match(VALUE_DELIMITER);
						setState(810);
						tableKeyedEntry();
						}
						}
					}
					setState(815);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,103,_ctx);
				}
				setState(817);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(816);
					match(VALUE_DELIMITER);
					}
				}

				setState(819);
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
		enterRule(_localctx, 128, RULE_tableKeyedEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(826);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				{
				setState(823);
				metadata();
				}
				}
				setState(828);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(829);
			_la = _input.LA(1);
			if ( !(_la==ROOT_IDENTIFIER || _la==MISSING) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			setState(830);
			match(VALUE_ASSIGN);
			setState(831);
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
			setState(855);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,110,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(833);
				match(BEGIN_PARAMETERS);
				setState(834);
				value();
				setState(839);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,107,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(835);
						match(VALUE_DELIMITER);
						setState(836);
						value();
						}
						}
					}
					setState(841);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,107,_ctx);
				}
				setState(843);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(842);
					match(VALUE_DELIMITER);
					}
				}

				setState(845);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(847);
				match(BEGIN_PARAMETERS);
				setState(848);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(849);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(851);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(850);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(853);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,109,_ctx);
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
			setState(879);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,114,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(857);
				match(BEGIN_PARAMETERS);
				setState(858);
				callArgument();
				setState(863);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,111,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(859);
						match(VALUE_DELIMITER);
						setState(860);
						callArgument();
						}
						}
					}
					setState(865);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,111,_ctx);
				}
				setState(867);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(866);
					match(VALUE_DELIMITER);
					}
				}

				setState(869);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(871);
				match(BEGIN_PARAMETERS);
				setState(872);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(873);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(875);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(874);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(877);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,113,_ctx);
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
		enterRule(_localctx, 134, RULE_callArgument);
		int _la;
		try {
			setState(885);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,115,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(881);
				_la = _input.LA(1);
				if ( !(_la==ROOT_IDENTIFIER || _la==MISSING) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(882);
				match(VALUE_ASSIGN);
				setState(883);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(884);
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
			setState(887);
			match(BEGIN_ARGUMENT);
			setState(888);
			argumentBody();
			setState(889);
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
			setState(896);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==ESCAPE || _la==BEGIN_VALUE_INLINE || _la==ARGUMENT_TEXT) {
				{
				setState(894);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case ARGUMENT_TEXT:
					{
					setState(891);
					match(ARGUMENT_TEXT);
					}
					break;
				case ESCAPE:
					{
					setState(892);
					escaped();
					}
					break;
				case BEGIN_VALUE_INLINE:
					{
					setState(893);
					inlineValue();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(898);
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
		enterRule(_localctx, 140, RULE_inlineValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(899);
			match(BEGIN_VALUE_INLINE);
			setState(900);
			value();
			setState(901);
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
			setState(903);
			match(BEGIN_VALUE_INLINE);
			setState(905);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(904);
				transformationPart();
				}
				}
				setState(907);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 81)) & ~0x3f) == 0 && ((1L << (_la - 81)) & 15L) != 0) );
			setState(909);
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
			setState(911);
			derivationRoot();
			setState(915);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,119,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(912);
					transformationPart();
					}
					}
				}
				setState(917);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,119,_ctx);
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
			setState(918);
			match(EXPLICIT_ROOT);
			setState(919);
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
			setState(925);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case EXPLICIT_ROOT:
				enterOuterAlt(_localctx, 1);
				{
				setState(921);
				explicitRoot();
				}
				break;
			case ROOT_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(922);
				match(ROOT_IDENTIFIER);
				}
				break;
			case VALUE_SMART_ROOT:
				enterOuterAlt(_localctx, 3);
				{
				setState(923);
				match(VALUE_SMART_ROOT);
				setState(924);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 1140858880L) != 0)) ) {
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
			setState(927);
			match(VALUE_ELVIS);
			setState(928);
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
			setState(940);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case VALUE_FUNCTION:
			case VALUE_PREDICATE:
				enterOuterAlt(_localctx, 1);
				{
				setState(930);
				functionChainType();
				setState(931);
				functionIdentifier();
				setState(933);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,121,_ctx) ) {
				case 1:
					{
					setState(932);
					callValueList();
					}
					break;
				}
				setState(936);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,122,_ctx) ) {
				case 1:
					{
					setState(935);
					callBinding();
					}
					break;
				}
				}
				break;
			case VALUE_MEMBER:
				enterOuterAlt(_localctx, 2);
				{
				setState(938);
				memberIdentifier();
				}
				break;
			case VALUE_WRAP:
				enterOuterAlt(_localctx, 3);
				{
				setState(939);
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
		enterRule(_localctx, 154, RULE_functionChainType);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(942);
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
		enterRule(_localctx, 156, RULE_labelIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(944);
			match(LABEL_PREFIX);
			setState(945);
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
			setState(947);
			match(VALUE_MEMBER);
			setState(948);
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
			setState(953);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(950);
				match(IDENTIFIER);
				}
				break;
			case NAMESPACE_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(951);
				match(NAMESPACE_IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 3);
				{
				setState(952);
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
			setState(957);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(955);
				match(IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(956);
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
			setState(959);
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
		enterRule(_localctx, 166, RULE_functionIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(961);
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
		enterRule(_localctx, 168, RULE_invocationIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(963);
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
			setState(965);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 1610621952L) != 0)) ) {
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
			setState(967);
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
			setState(969);
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
		enterRule(_localctx, 176, RULE_funcModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(971);
			_la = _input.LA(1);
			if ( !(((((_la - 61)) & ~0x3f) == 0 && ((1L << (_la - 61)) & 25L) != 0)) ) {
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
		enterRule(_localctx, 178, RULE_trivia);
		try {
			setState(975);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case COMMENT:
			case SLASH_COMMENT:
				enterOuterAlt(_localctx, 1);
				{
				setState(973);
				comment();
				}
				break;
			case NEWLINE:
				enterOuterAlt(_localctx, 2);
				{
				setState(974);
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
		enterRule(_localctx, 180, RULE_comment);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(977);
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
		enterRule(_localctx, 182, RULE_escaped);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(979);
			match(ESCAPE);
			setState(980);
			_la = _input.LA(1);
			if ( !(((((_la - 94)) & ~0x3f) == 0 && ((1L << (_la - 94)) & 7L) != 0)) ) {
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
		"\u0004\u0001n\u03d7\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
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
		"Z\u0007Z\u0002[\u0007[\u0001\u0000\u0005\u0000\u00ba\b\u0000\n\u0000\f"+
		"\u0000\u00bd\t\u0000\u0001\u0000\u0003\u0000\u00c0\b\u0000\u0001\u0000"+
		"\u0001\u0000\u0005\u0000\u00c4\b\u0000\n\u0000\f\u0000\u00c7\t\u0000\u0005"+
		"\u0000\u00c9\b\u0000\n\u0000\f\u0000\u00cc\t\u0000\u0001\u0000\u0001\u0000"+
		"\u0001\u0001\u0001\u0001\u0005\u0001\u00d2\b\u0001\n\u0001\f\u0001\u00d5"+
		"\t\u0001\u0001\u0001\u0001\u0001\u0005\u0001\u00d9\b\u0001\n\u0001\f\u0001"+
		"\u00dc\t\u0001\u0001\u0002\u0001\u0002\u0005\u0002\u00e0\b\u0002\n\u0002"+
		"\f\u0002\u00e3\t\u0002\u0001\u0002\u0001\u0002\u0001\u0002\u0001\u0002"+
		"\u0003\u0002\u00e9\b\u0002\u0001\u0002\u0001\u0002\u0001\u0002\u0001\u0002"+
		"\u0003\u0002\u00ef\b\u0002\u0001\u0003\u0001\u0003\u0005\u0003\u00f3\b"+
		"\u0003\n\u0003\f\u0003\u00f6\t\u0003\u0001\u0003\u0005\u0003\u00f9\b\u0003"+
		"\n\u0003\f\u0003\u00fc\t\u0003\u0001\u0004\u0004\u0004\u00ff\b\u0004\u000b"+
		"\u0004\f\u0004\u0100\u0001\u0005\u0001\u0005\u0001\u0005\u0005\u0005\u0106"+
		"\b\u0005\n\u0005\f\u0005\u0109\t\u0005\u0001\u0005\u0003\u0005\u010c\b"+
		"\u0005\u0001\u0005\u0003\u0005\u010f\b\u0005\u0001\u0006\u0001\u0006\u0001"+
		"\u0006\u0001\u0006\u0001\u0007\u0005\u0007\u0116\b\u0007\n\u0007\f\u0007"+
		"\u0119\t\u0007\u0001\u0007\u0001\u0007\u0001\u0007\u0003\u0007\u011e\b"+
		"\u0007\u0001\u0007\u0001\u0007\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b"+
		"\u0003\b\u0127\b\b\u0001\t\u0001\t\u0001\t\u0001\t\u0001\t\u0001\t\u0003"+
		"\t\u012f\b\t\u0001\t\u0005\t\u0132\b\t\n\t\f\t\u0135\t\t\u0001\t\u0001"+
		"\t\u0001\n\u0001\n\u0001\u000b\u0001\u000b\u0001\u000b\u0001\u000b\u0005"+
		"\u000b\u013f\b\u000b\n\u000b\f\u000b\u0142\t\u000b\u0001\u000b\u0001\u000b"+
		"\u0001\f\u0005\f\u0147\b\f\n\f\f\f\u014a\t\f\u0001\f\u0001\f\u0001\f\u0001"+
		"\r\u0005\r\u0150\b\r\n\r\f\r\u0153\t\r\u0001\r\u0001\r\u0001\r\u0003\r"+
		"\u0158\b\r\u0001\r\u0005\r\u015b\b\r\n\r\f\r\u015e\t\r\u0001\r\u0001\r"+
		"\u0001\u000e\u0001\u000e\u0003\u000e\u0164\b\u000e\u0001\u000e\u0001\u000e"+
		"\u0001\u000e\u0003\u000e\u0169\b\u000e\u0001\u000f\u0001\u000f\u0001\u000f"+
		"\u0001\u000f\u0001\u0010\u0001\u0010\u0001\u0011\u0003\u0011\u0172\b\u0011"+
		"\u0001\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0003\u0011\u0178\b\u0011"+
		"\u0001\u0011\u0001\u0011\u0003\u0011\u017c\b\u0011\u0001\u0011\u0003\u0011"+
		"\u017f\b\u0011\u0003\u0011\u0181\b\u0011\u0001\u0012\u0001\u0012\u0003"+
		"\u0012\u0185\b\u0012\u0001\u0012\u0003\u0012\u0188\b\u0012\u0001\u0013"+
		"\u0001\u0013\u0001\u0013\u0005\u0013\u018d\b\u0013\n\u0013\f\u0013\u0190"+
		"\t\u0013\u0001\u0014\u0001\u0014\u0003\u0014\u0194\b\u0014\u0001\u0015"+
		"\u0001\u0015\u0001\u0015\u0001\u0015\u0003\u0015\u019a\b\u0015\u0001\u0016"+
		"\u0001\u0016\u0001\u0016\u0001\u0016\u0001\u0016\u0001\u0016\u0005\u0016"+
		"\u01a2\b\u0016\n\u0016\f\u0016\u01a5\t\u0016\u0001\u0016\u0003\u0016\u01a8"+
		"\b\u0016\u0001\u0016\u0001\u0016\u0003\u0016\u01ac\b\u0016\u0001\u0017"+
		"\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0005\u0017"+
		"\u01b4\b\u0017\n\u0017\f\u0017\u01b7\t\u0017\u0001\u0017\u0003\u0017\u01ba"+
		"\b\u0017\u0001\u0017\u0001\u0017\u0003\u0017\u01be\b\u0017\u0001\u0018"+
		"\u0001\u0018\u0001\u0018\u0001\u0018\u0001\u0018\u0001\u0019\u0001\u0019"+
		"\u0001\u0019\u0001\u0019\u0005\u0019\u01c9\b\u0019\n\u0019\f\u0019\u01cc"+
		"\t\u0019\u0001\u0019\u0003\u0019\u01cf\b\u0019\u0001\u0019\u0001\u0019"+
		"\u0001\u0019\u0001\u0019\u0001\u0019\u0003\u0019\u01d6\b\u0019\u0001\u001a"+
		"\u0003\u001a\u01d9\b\u001a\u0001\u001a\u0001\u001a\u0003\u001a\u01dd\b"+
		"\u001a\u0001\u001a\u0001\u001a\u0003\u001a\u01e1\b\u001a\u0001\u001b\u0001"+
		"\u001b\u0001\u001c\u0001\u001c\u0001\u001c\u0001\u001c\u0005\u001c\u01e9"+
		"\b\u001c\n\u001c\f\u001c\u01ec\t\u001c\u0001\u001c\u0001\u001c\u0001\u001d"+
		"\u0001\u001d\u0005\u001d\u01f2\b\u001d\n\u001d\f\u001d\u01f5\t\u001d\u0001"+
		"\u001d\u0001\u001d\u0001\u001d\u0003\u001d\u01fa\b\u001d\u0001\u001e\u0001"+
		"\u001e\u0001\u001e\u0001\u001e\u0003\u001e\u0200\b\u001e\u0001\u001e\u0001"+
		"\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001"+
		"\u001e\u0001\u001e\u0001\u001e\u0003\u001e\u020c\b\u001e\u0001\u001f\u0001"+
		"\u001f\u0001\u001f\u0003\u001f\u0211\b\u001f\u0001\u001f\u0001\u001f\u0001"+
		"\u001f\u0003\u001f\u0216\b\u001f\u0001 \u0001 \u0001 \u0001 \u0005 \u021c"+
		"\b \n \f \u021f\t \u0001 \u0001 \u0001 \u0001 \u0003 \u0225\b \u0001!"+
		"\u0001!\u0001!\u0001!\u0001!\u0001\"\u0001\"\u0003\"\u022e\b\"\u0001#"+
		"\u0001#\u0001$\u0001$\u0001$\u0001$\u0001$\u0001$\u0001%\u0001%\u0001"+
		"%\u0005%\u023b\b%\n%\f%\u023e\t%\u0001%\u0003%\u0241\b%\u0001&\u0001&"+
		"\u0001&\u0001&\u0001&\u0001\'\u0001\'\u0003\'\u024a\b\'\u0001\'\u0001"+
		"\'\u0001\'\u0001\'\u0001\'\u0001(\u0005(\u0252\b(\n(\f(\u0255\t(\u0001"+
		"(\u0001(\u0001(\u0005(\u025a\b(\n(\f(\u025d\t(\u0001(\u0003(\u0260\b("+
		"\u0001)\u0001)\u0001)\u0001)\u0001)\u0001*\u0001*\u0003*\u0269\b*\u0001"+
		"+\u0003+\u026c\b+\u0001+\u0001+\u0001+\u0003+\u0271\b+\u0001+\u0003+\u0274"+
		"\b+\u0001+\u0001+\u0001+\u0001+\u0003+\u027a\b+\u0003+\u027c\b+\u0001"+
		",\u0001,\u0001,\u0001,\u0001-\u0001-\u0001-\u0003-\u0285\b-\u0001-\u0005"+
		"-\u0288\b-\n-\f-\u028b\t-\u0001.\u0001.\u0001.\u0001.\u0001.\u0003.\u0292"+
		"\b.\u0001.\u0003.\u0295\b.\u0001/\u0001/\u0003/\u0299\b/\u0001/\u0001"+
		"/\u0001/\u0001/\u0003/\u029f\b/\u00010\u00010\u00010\u00010\u00011\u0001"+
		"1\u00011\u00011\u00041\u02a9\b1\u000b1\f1\u02aa\u00012\u00012\u00012\u0001"+
		"2\u00032\u02b1\b2\u00013\u00013\u00033\u02b5\b3\u00014\u00014\u00014\u0001"+
		"4\u00015\u00015\u00015\u00055\u02be\b5\n5\f5\u02c1\t5\u00015\u00035\u02c4"+
		"\b5\u00015\u00015\u00015\u00035\u02c9\b5\u00015\u00015\u00055\u02cd\b"+
		"5\n5\f5\u02d0\t5\u00016\u00016\u00016\u00016\u00016\u00016\u00016\u0001"+
		"6\u00016\u00016\u00016\u00016\u00016\u00036\u02df\b6\u00017\u00017\u0001"+
		"7\u00017\u00057\u02e5\b7\n7\f7\u02e8\t7\u00017\u00017\u00017\u00017\u0001"+
		"8\u00018\u00018\u00018\u00058\u02f2\b8\n8\f8\u02f5\t8\u00018\u00018\u0001"+
		"8\u00018\u00038\u02fb\b8\u00038\u02fd\b8\u00019\u00019\u0001:\u0001:\u0001"+
		";\u0001;\u0001;\u0003;\u0306\b;\u0001<\u0001<\u0001<\u0003<\u030b\b<\u0001"+
		"=\u0003=\u030e\b=\u0001=\u0001=\u0003=\u0312\b=\u0001>\u0001>\u0001>\u0001"+
		">\u0001>\u0001>\u0005>\u031a\b>\n>\f>\u031d\t>\u0001>\u0003>\u0320\b>"+
		"\u0001>\u0001>\u0003>\u0324\b>\u0001?\u0001?\u0001?\u0001?\u0001?\u0001"+
		"?\u0005?\u032c\b?\n?\f?\u032f\t?\u0001?\u0003?\u0332\b?\u0001?\u0001?"+
		"\u0003?\u0336\b?\u0001@\u0005@\u0339\b@\n@\f@\u033c\t@\u0001@\u0001@\u0001"+
		"@\u0001@\u0001A\u0001A\u0001A\u0001A\u0005A\u0346\bA\nA\fA\u0349\tA\u0001"+
		"A\u0003A\u034c\bA\u0001A\u0001A\u0001A\u0001A\u0001A\u0001A\u0004A\u0354"+
		"\bA\u000bA\fA\u0355\u0003A\u0358\bA\u0001B\u0001B\u0001B\u0001B\u0005"+
		"B\u035e\bB\nB\fB\u0361\tB\u0001B\u0003B\u0364\bB\u0001B\u0001B\u0001B"+
		"\u0001B\u0001B\u0001B\u0004B\u036c\bB\u000bB\fB\u036d\u0003B\u0370\bB"+
		"\u0001C\u0001C\u0001C\u0001C\u0003C\u0376\bC\u0001D\u0001D\u0001D\u0001"+
		"D\u0001E\u0001E\u0001E\u0005E\u037f\bE\nE\fE\u0382\tE\u0001F\u0001F\u0001"+
		"F\u0001F\u0001G\u0001G\u0004G\u038a\bG\u000bG\fG\u038b\u0001G\u0001G\u0001"+
		"H\u0001H\u0005H\u0392\bH\nH\fH\u0395\tH\u0001I\u0001I\u0001I\u0001J\u0001"+
		"J\u0001J\u0001J\u0003J\u039e\bJ\u0001K\u0001K\u0001K\u0001L\u0001L\u0001"+
		"L\u0003L\u03a6\bL\u0001L\u0003L\u03a9\bL\u0001L\u0001L\u0003L\u03ad\b"+
		"L\u0001M\u0001M\u0001N\u0001N\u0001N\u0001O\u0001O\u0001O\u0001P\u0001"+
		"P\u0001P\u0003P\u03ba\bP\u0001Q\u0001Q\u0003Q\u03be\bQ\u0001R\u0001R\u0001"+
		"S\u0001S\u0001T\u0001T\u0001U\u0001U\u0001V\u0001V\u0001W\u0001W\u0001"+
		"X\u0001X\u0001Y\u0001Y\u0003Y\u03d0\bY\u0001Z\u0001Z\u0001[\u0001[\u0001"+
		"[\u0001[\u0000\u0001j\\\u0000\u0002\u0004\u0006\b\n\f\u000e\u0010\u0012"+
		"\u0014\u0016\u0018\u001a\u001c\u001e \"$&(*,.02468:<>@BDFHJLNPRTVXZ\\"+
		"^`bdfhjlnprtvxz|~\u0080\u0082\u0084\u0086\u0088\u008a\u008c\u008e\u0090"+
		"\u0092\u0094\u0096\u0098\u009a\u009c\u009e\u00a0\u00a2\u00a4\u00a6\u00a8"+
		"\u00aa\u00ac\u00ae\u00b0\u00b2\u00b4\u00b6\u0000\f\u0003\u0000\n\n..1"+
		"2\u0003\u0000\n\n\r\r\u001d\u001e\u0002\u0000\u0014\u0014\u001f\u001f"+
		"\u0002\u0000\r\r\u001e\u001e\u0003\u0000\r\r\u001a\u001a\u001e\u001e\u0001"+
		"\u0000QR\u0002\u0000\r\rbb\u0002\u0000\n\n::\u0002\u000033BB\u0002\u0000"+
		"==@A\u0001\u0000%&\u0001\u0000^`\u0429\u0000\u00bb\u0001\u0000\u0000\u0000"+
		"\u0002\u00cf\u0001\u0000\u0000\u0000\u0004\u00ee\u0001\u0000\u0000\u0000"+
		"\u0006\u00f0\u0001\u0000\u0000\u0000\b\u00fe\u0001\u0000\u0000\u0000\n"+
		"\u010e\u0001\u0000\u0000\u0000\f\u0110\u0001\u0000\u0000\u0000\u000e\u0117"+
		"\u0001\u0000\u0000\u0000\u0010\u0121\u0001\u0000\u0000\u0000\u0012\u0128"+
		"\u0001\u0000\u0000\u0000\u0014\u0138\u0001\u0000\u0000\u0000\u0016\u013a"+
		"\u0001\u0000\u0000\u0000\u0018\u0148\u0001\u0000\u0000\u0000\u001a\u0151"+
		"\u0001\u0000\u0000\u0000\u001c\u0163\u0001\u0000\u0000\u0000\u001e\u016a"+
		"\u0001\u0000\u0000\u0000 \u016e\u0001\u0000\u0000\u0000\"\u0180\u0001"+
		"\u0000\u0000\u0000$\u0187\u0001\u0000\u0000\u0000&\u0189\u0001\u0000\u0000"+
		"\u0000(\u0191\u0001\u0000\u0000\u0000*\u0199\u0001\u0000\u0000\u0000,"+
		"\u01ab\u0001\u0000\u0000\u0000.\u01bd\u0001\u0000\u0000\u00000\u01bf\u0001"+
		"\u0000\u0000\u00002\u01d5\u0001\u0000\u0000\u00004\u01d8\u0001\u0000\u0000"+
		"\u00006\u01e2\u0001\u0000\u0000\u00008\u01e4\u0001\u0000\u0000\u0000:"+
		"\u01f9\u0001\u0000\u0000\u0000<\u020b\u0001\u0000\u0000\u0000>\u0215\u0001"+
		"\u0000\u0000\u0000@\u0217\u0001\u0000\u0000\u0000B\u0226\u0001\u0000\u0000"+
		"\u0000D\u022d\u0001\u0000\u0000\u0000F\u022f\u0001\u0000\u0000\u0000H"+
		"\u0231\u0001\u0000\u0000\u0000J\u0237\u0001\u0000\u0000\u0000L\u0242\u0001"+
		"\u0000\u0000\u0000N\u0247\u0001\u0000\u0000\u0000P\u0253\u0001\u0000\u0000"+
		"\u0000R\u0261\u0001\u0000\u0000\u0000T\u0268\u0001\u0000\u0000\u0000V"+
		"\u027b\u0001\u0000\u0000\u0000X\u027d\u0001\u0000\u0000\u0000Z\u0281\u0001"+
		"\u0000\u0000\u0000\\\u0294\u0001\u0000\u0000\u0000^\u029e\u0001\u0000"+
		"\u0000\u0000`\u02a0\u0001\u0000\u0000\u0000b\u02a8\u0001\u0000\u0000\u0000"+
		"d\u02ac\u0001\u0000\u0000\u0000f\u02b4\u0001\u0000\u0000\u0000h\u02b6"+
		"\u0001\u0000\u0000\u0000j\u02c8\u0001\u0000\u0000\u0000l\u02de\u0001\u0000"+
		"\u0000\u0000n\u02e0\u0001\u0000\u0000\u0000p\u02fc\u0001\u0000\u0000\u0000"+
		"r\u02fe\u0001\u0000\u0000\u0000t\u0300\u0001\u0000\u0000\u0000v\u0302"+
		"\u0001\u0000\u0000\u0000x\u030a\u0001\u0000\u0000\u0000z\u0311\u0001\u0000"+
		"\u0000\u0000|\u0323\u0001\u0000\u0000\u0000~\u0335\u0001\u0000\u0000\u0000"+
		"\u0080\u033a\u0001\u0000\u0000\u0000\u0082\u0357\u0001\u0000\u0000\u0000"+
		"\u0084\u036f\u0001\u0000\u0000\u0000\u0086\u0375\u0001\u0000\u0000\u0000"+
		"\u0088\u0377\u0001\u0000\u0000\u0000\u008a\u0380\u0001\u0000\u0000\u0000"+
		"\u008c\u0383\u0001\u0000\u0000\u0000\u008e\u0387\u0001\u0000\u0000\u0000"+
		"\u0090\u038f\u0001\u0000\u0000\u0000\u0092\u0396\u0001\u0000\u0000\u0000"+
		"\u0094\u039d\u0001\u0000\u0000\u0000\u0096\u039f\u0001\u0000\u0000\u0000"+
		"\u0098\u03ac\u0001\u0000\u0000\u0000\u009a\u03ae\u0001\u0000\u0000\u0000"+
		"\u009c\u03b0\u0001\u0000\u0000\u0000\u009e\u03b3\u0001\u0000\u0000\u0000"+
		"\u00a0\u03b9\u0001\u0000\u0000\u0000\u00a2\u03bd\u0001\u0000\u0000\u0000"+
		"\u00a4\u03bf\u0001\u0000\u0000\u0000\u00a6\u03c1\u0001\u0000\u0000\u0000"+
		"\u00a8\u03c3\u0001\u0000\u0000\u0000\u00aa\u03c5\u0001\u0000\u0000\u0000"+
		"\u00ac\u03c7\u0001\u0000\u0000\u0000\u00ae\u03c9\u0001\u0000\u0000\u0000"+
		"\u00b0\u03cb\u0001\u0000\u0000\u0000\u00b2\u03cf\u0001\u0000\u0000\u0000"+
		"\u00b4\u03d1\u0001\u0000\u0000\u0000\u00b6\u03d3\u0001\u0000\u0000\u0000"+
		"\u00b8\u00ba\u0003\u00b2Y\u0000\u00b9\u00b8\u0001\u0000\u0000\u0000\u00ba"+
		"\u00bd\u0001\u0000\u0000\u0000\u00bb\u00b9\u0001\u0000\u0000\u0000\u00bb"+
		"\u00bc\u0001\u0000\u0000\u0000\u00bc\u00bf\u0001\u0000\u0000\u0000\u00bd"+
		"\u00bb\u0001\u0000\u0000\u0000\u00be\u00c0\u0003\u0002\u0001\u0000\u00bf"+
		"\u00be\u0001\u0000\u0000\u0000\u00bf\u00c0\u0001\u0000\u0000\u0000\u00c0"+
		"\u00ca\u0001\u0000\u0000\u0000\u00c1\u00c5\u0003\u0004\u0002\u0000\u00c2"+
		"\u00c4\u0003\u00b2Y\u0000\u00c3\u00c2\u0001\u0000\u0000\u0000\u00c4\u00c7"+
		"\u0001\u0000\u0000\u0000\u00c5\u00c3\u0001\u0000\u0000\u0000\u00c5\u00c6"+
		"\u0001\u0000\u0000\u0000\u00c6\u00c9\u0001\u0000\u0000\u0000\u00c7\u00c5"+
		"\u0001\u0000\u0000\u0000\u00c8\u00c1\u0001\u0000\u0000\u0000\u00c9\u00cc"+
		"\u0001\u0000\u0000\u0000\u00ca\u00c8\u0001\u0000\u0000\u0000\u00ca\u00cb"+
		"\u0001\u0000\u0000\u0000\u00cb\u00cd\u0001\u0000\u0000\u0000\u00cc\u00ca"+
		"\u0001\u0000\u0000\u0000\u00cd\u00ce\u0005\u0000\u0000\u0001\u00ce\u0001"+
		"\u0001\u0000\u0000\u0000\u00cf\u00d3\u0003\u0006\u0003\u0000\u00d0\u00d2"+
		"\u0003\u00b2Y\u0000\u00d1\u00d0\u0001\u0000\u0000\u0000\u00d2\u00d5\u0001"+
		"\u0000\u0000\u0000\u00d3\u00d1\u0001\u0000\u0000\u0000\u00d3\u00d4\u0001"+
		"\u0000\u0000\u0000\u00d4\u00d6\u0001\u0000\u0000\u0000\u00d5\u00d3\u0001"+
		"\u0000\u0000\u0000\u00d6\u00da\u0005\'\u0000\u0000\u00d7\u00d9\u0003\u00b2"+
		"Y\u0000\u00d8\u00d7\u0001\u0000\u0000\u0000\u00d9\u00dc\u0001\u0000\u0000"+
		"\u0000\u00da\u00d8\u0001\u0000\u0000\u0000\u00da\u00db\u0001\u0000\u0000"+
		"\u0000\u00db\u0003\u0001\u0000\u0000\u0000\u00dc\u00da\u0001\u0000\u0000"+
		"\u0000\u00dd\u00e1\u0003\u0006\u0003\u0000\u00de\u00e0\u0003\u00b2Y\u0000"+
		"\u00df\u00de\u0001\u0000\u0000\u0000\u00e0\u00e3\u0001\u0000\u0000\u0000"+
		"\u00e1\u00df\u0001\u0000\u0000\u0000\u00e1\u00e2\u0001\u0000\u0000\u0000"+
		"\u00e2\u00e8\u0001\u0000\u0000\u0000\u00e3\u00e1\u0001\u0000\u0000\u0000"+
		"\u00e4\u00e9\u0003\u000e\u0007\u0000\u00e5\u00e9\u0003\u001a\r\u0000\u00e6"+
		"\u00e9\u0003\u0010\b\u0000\u00e7\u00e9\u0003\u0012\t\u0000\u00e8\u00e4"+
		"\u0001\u0000\u0000\u0000\u00e8\u00e5\u0001\u0000\u0000\u0000\u00e8\u00e6"+
		"\u0001\u0000\u0000\u0000\u00e8\u00e7\u0001\u0000\u0000\u0000\u00e9\u00ef"+
		"\u0001\u0000\u0000\u0000\u00ea\u00ef\u0003\u000e\u0007\u0000\u00eb\u00ef"+
		"\u0003\u001a\r\u0000\u00ec\u00ef\u0003\u0010\b\u0000\u00ed\u00ef\u0003"+
		"\u0012\t\u0000\u00ee\u00dd\u0001\u0000\u0000\u0000\u00ee\u00ea\u0001\u0000"+
		"\u0000\u0000\u00ee\u00eb\u0001\u0000\u0000\u0000\u00ee\u00ec\u0001\u0000"+
		"\u0000\u0000\u00ee\u00ed\u0001\u0000\u0000\u0000\u00ef\u0005\u0001\u0000"+
		"\u0000\u0000\u00f0\u00fa\u0003\n\u0005\u0000\u00f1\u00f3\u0003\u00b2Y"+
		"\u0000\u00f2\u00f1\u0001\u0000\u0000\u0000\u00f3\u00f6\u0001\u0000\u0000"+
		"\u0000\u00f4\u00f2\u0001\u0000\u0000\u0000\u00f4\u00f5\u0001\u0000\u0000"+
		"\u0000\u00f5\u00f7\u0001\u0000\u0000\u0000\u00f6\u00f4\u0001\u0000\u0000"+
		"\u0000\u00f7\u00f9\u0003\n\u0005\u0000\u00f8\u00f4\u0001\u0000\u0000\u0000"+
		"\u00f9\u00fc\u0001\u0000\u0000\u0000\u00fa\u00f8\u0001\u0000\u0000\u0000"+
		"\u00fa\u00fb\u0001\u0000\u0000\u0000\u00fb\u0007\u0001\u0000\u0000\u0000"+
		"\u00fc\u00fa\u0001\u0000\u0000\u0000\u00fd\u00ff\u0003\n\u0005\u0000\u00fe"+
		"\u00fd\u0001\u0000\u0000\u0000\u00ff\u0100\u0001\u0000\u0000\u0000\u0100"+
		"\u00fe\u0001\u0000\u0000\u0000\u0100\u0101\u0001\u0000\u0000\u0000\u0101"+
		"\t\u0001\u0000\u0000\u0000\u0102\u0103\u0005\u0013\u0000\u0000\u0103\u0107"+
		"\u0005\n\u0000\u0000\u0104\u0106\u0003\u0088D\u0000\u0105\u0104\u0001"+
		"\u0000\u0000\u0000\u0106\u0109\u0001\u0000\u0000\u0000\u0107\u0105\u0001"+
		"\u0000\u0000\u0000\u0107\u0108\u0001\u0000\u0000\u0000\u0108\u010b\u0001"+
		"\u0000\u0000\u0000\u0109\u0107\u0001\u0000\u0000\u0000\u010a\u010c\u0003"+
		"\u0082A\u0000\u010b\u010a\u0001\u0000\u0000\u0000\u010b\u010c\u0001\u0000"+
		"\u0000\u0000\u010c\u010f\u0001\u0000\u0000\u0000\u010d\u010f\u0003\f\u0006"+
		"\u0000\u010e\u0102\u0001\u0000\u0000\u0000\u010e\u010d\u0001\u0000\u0000"+
		"\u0000\u010f\u000b\u0001\u0000\u0000\u0000\u0110\u0111\u0005\u0012\u0000"+
		"\u0000\u0111\u0112\u0003f3\u0000\u0112\u0113\u0005W\u0000\u0000\u0113"+
		"\r\u0001\u0000\u0000\u0000\u0114\u0116\u0003\u00aeW\u0000\u0115\u0114"+
		"\u0001\u0000\u0000\u0000\u0116\u0119\u0001\u0000\u0000\u0000\u0117\u0115"+
		"\u0001\u0000\u0000\u0000\u0117\u0118\u0001\u0000\u0000\u0000\u0118\u011a"+
		"\u0001\u0000\u0000\u0000\u0119\u0117\u0001\u0000\u0000\u0000\u011a\u011b"+
		"\u00052\u0000\u0000\u011b\u011d\u0003\u00a0P\u0000\u011c\u011e\u00032"+
		"\u0019\u0000\u011d\u011c\u0001\u0000\u0000\u0000\u011d\u011e\u0001\u0000"+
		"\u0000\u0000\u011e\u011f\u0001\u0000\u0000\u0000\u011f\u0120\u0003\u0016"+
		"\u000b\u0000\u0120\u000f\u0001\u0000\u0000\u0000\u0121\u0122\u0005.\u0000"+
		"\u0000\u0122\u0123\u0005\n\u0000\u0000\u0123\u0124\u0005E\u0000\u0000"+
		"\u0124\u0126\u0003$\u0012\u0000\u0125\u0127\u0005*\u0000\u0000\u0126\u0125"+
		"\u0001\u0000\u0000\u0000\u0126\u0127\u0001\u0000\u0000\u0000\u0127\u0011"+
		"\u0001\u0000\u0000\u0000\u0128\u0129\u0005>\u0000\u0000\u0129\u012a\u0005"+
		"\n\u0000\u0000\u012a\u012b\u00032\u0019\u0000\u012b\u012c\u0005?\u0000"+
		"\u0000\u012c\u0133\u0003\u0014\n\u0000\u012d\u012f\u0005+\u0000\u0000"+
		"\u012e\u012d\u0001\u0000\u0000\u0000\u012e\u012f\u0001\u0000\u0000\u0000"+
		"\u012f\u0130\u0001\u0000\u0000\u0000\u0130\u0132\u0003\u0014\n\u0000\u0131"+
		"\u012e\u0001\u0000\u0000\u0000\u0132\u0135\u0001\u0000\u0000\u0000\u0133"+
		"\u0131\u0001\u0000\u0000\u0000\u0133\u0134\u0001\u0000\u0000\u0000\u0134"+
		"\u0136\u0001\u0000\u0000\u0000\u0135\u0133\u0001\u0000\u0000\u0000\u0136"+
		"\u0137\u0005*\u0000\u0000\u0137\u0013\u0001\u0000\u0000\u0000\u0138\u0139"+
		"\u0007\u0000\u0000\u0000\u0139\u0015\u0001\u0000\u0000\u0000\u013a\u0140"+
		"\u0005(\u0000\u0000\u013b\u013f\u0003\u0018\f\u0000\u013c\u013f\u0003"+
		"\u001a\r\u0000\u013d\u013f\u0003\u00b2Y\u0000\u013e\u013b\u0001\u0000"+
		"\u0000\u0000\u013e\u013c\u0001\u0000\u0000\u0000\u013e\u013d\u0001\u0000"+
		"\u0000\u0000\u013f\u0142\u0001\u0000\u0000\u0000\u0140\u013e\u0001\u0000"+
		"\u0000\u0000\u0140\u0141\u0001\u0000\u0000\u0000\u0141\u0143\u0001\u0000"+
		"\u0000\u0000\u0142\u0140\u0001\u0000\u0000\u0000\u0143\u0144\u0005)\u0000"+
		"\u0000\u0144\u0017\u0001\u0000\u0000\u0000\u0145\u0147\u0003\u00acV\u0000"+
		"\u0146\u0145\u0001\u0000\u0000\u0000\u0147\u014a\u0001\u0000\u0000\u0000"+
		"\u0148\u0146\u0001\u0000\u0000\u0000\u0148\u0149\u0001\u0000\u0000\u0000"+
		"\u0149\u014b\u0001\u0000\u0000\u0000\u014a\u0148\u0001\u0000\u0000\u0000"+
		"\u014b\u014c\u00051\u0000\u0000\u014c\u014d\u00038\u001c\u0000\u014d\u0019"+
		"\u0001\u0000\u0000\u0000\u014e\u0150\u0003\u00b0X\u0000\u014f\u014e\u0001"+
		"\u0000\u0000\u0000\u0150\u0153\u0001\u0000\u0000\u0000\u0151\u014f\u0001"+
		"\u0000\u0000\u0000\u0151\u0152\u0001\u0000\u0000\u0000\u0152\u0154\u0001"+
		"\u0000\u0000\u0000\u0153\u0151\u0001\u0000\u0000\u0000\u0154\u0155\u0005"+
		",\u0000\u0000\u0155\u0157\u0003\u00a2Q\u0000\u0156\u0158\u0003\u001c\u000e"+
		"\u0000\u0157\u0156\u0001\u0000\u0000\u0000\u0157\u0158\u0001\u0000\u0000"+
		"\u0000\u0158\u015c\u0001\u0000\u0000\u0000\u0159\u015b\u0003\u00b2Y\u0000"+
		"\u015a\u0159\u0001\u0000\u0000\u0000\u015b\u015e\u0001\u0000\u0000\u0000"+
		"\u015c\u015a\u0001\u0000\u0000\u0000\u015c\u015d\u0001\u0000\u0000\u0000"+
		"\u015d\u015f\u0001\u0000\u0000\u0000\u015e\u015c\u0001\u0000\u0000\u0000"+
		"\u015f\u0160\u0003\"\u0011\u0000\u0160\u001b\u0001\u0000\u0000\u0000\u0161"+
		"\u0164\u00032\u0019\u0000\u0162\u0164\u0003\u001e\u000f\u0000\u0163\u0161"+
		"\u0001\u0000\u0000\u0000\u0163\u0162\u0001\u0000\u0000\u0000\u0163\u0164"+
		"\u0001\u0000\u0000\u0000\u0164\u0165\u0001\u0000\u0000\u0000\u0165\u0168"+
		"\u0005F\u0000\u0000\u0166\u0169\u0003$\u0012\u0000\u0167\u0169\u0003 "+
		"\u0010\u0000\u0168\u0166\u0001\u0000\u0000\u0000\u0168\u0167\u0001\u0000"+
		"\u0000\u0000\u0169\u001d\u0001\u0000\u0000\u0000\u016a\u016b\u0005\t\u0000"+
		"\u0000\u016b\u016c\u0003$\u0012\u0000\u016c\u016d\u0005W\u0000\u0000\u016d"+
		"\u001f\u0001\u0000\u0000\u0000\u016e\u016f\u00032\u0019\u0000\u016f!\u0001"+
		"\u0000\u0000\u0000\u0170\u0172\u0005/\u0000\u0000\u0171\u0170\u0001\u0000"+
		"\u0000\u0000\u0171\u0172\u0001\u0000\u0000\u0000\u0172\u0173\u0001\u0000"+
		"\u0000\u0000\u0173\u0181\u00038\u001c\u0000\u0174\u0175\u0005\u0019\u0000"+
		"\u0000\u0175\u0177\u0003f3\u0000\u0176\u0178\u0005\f\u0000\u0000\u0177"+
		"\u0176\u0001\u0000\u0000\u0000\u0177\u0178\u0001\u0000\u0000\u0000\u0178"+
		"\u0181\u0001\u0000\u0000\u0000\u0179\u017b\u00050\u0000\u0000\u017a\u017c"+
		"\u0005\n\u0000\u0000\u017b\u017a\u0001\u0000\u0000\u0000\u017b\u017c\u0001"+
		"\u0000\u0000\u0000\u017c\u017e\u0001\u0000\u0000\u0000\u017d\u017f\u0005"+
		"*\u0000\u0000\u017e\u017d\u0001\u0000\u0000\u0000\u017e\u017f\u0001\u0000"+
		"\u0000\u0000\u017f\u0181\u0001\u0000\u0000\u0000\u0180\u0171\u0001\u0000"+
		"\u0000\u0000\u0180\u0174\u0001\u0000\u0000\u0000\u0180\u0179\u0001\u0000"+
		"\u0000\u0000\u0181#\u0001\u0000\u0000\u0000\u0182\u0184\u0003\b\u0004"+
		"\u0000\u0183\u0185\u0003&\u0013\u0000\u0184\u0183\u0001\u0000\u0000\u0000"+
		"\u0184\u0185\u0001\u0000\u0000\u0000\u0185\u0188\u0001\u0000\u0000\u0000"+
		"\u0186\u0188\u0003&\u0013\u0000\u0187\u0182\u0001\u0000\u0000\u0000\u0187"+
		"\u0186\u0001\u0000\u0000\u0000\u0188%\u0001\u0000\u0000\u0000\u0189\u018e"+
		"\u0003(\u0014\u0000\u018a\u018b\u0005 \u0000\u0000\u018b\u018d\u0003("+
		"\u0014\u0000\u018c\u018a\u0001\u0000\u0000\u0000\u018d\u0190\u0001\u0000"+
		"\u0000\u0000\u018e\u018c\u0001\u0000\u0000\u0000\u018e\u018f\u0001\u0000"+
		"\u0000\u0000\u018f\'\u0001\u0000\u0000\u0000\u0190\u018e\u0001\u0000\u0000"+
		"\u0000\u0191\u0193\u0003*\u0015\u0000\u0192\u0194\u0005\u001f\u0000\u0000"+
		"\u0193\u0192\u0001\u0000\u0000\u0000\u0193\u0194\u0001\u0000\u0000\u0000"+
		"\u0194)\u0001\u0000\u0000\u0000\u0195\u019a\u00036\u001b\u0000\u0196\u019a"+
		"\u0003,\u0016\u0000\u0197\u019a\u0003.\u0017\u0000\u0198\u019a\u00030"+
		"\u0018\u0000\u0199\u0195\u0001\u0000\u0000\u0000\u0199\u0196\u0001\u0000"+
		"\u0000\u0000\u0199\u0197\u0001\u0000\u0000\u0000\u0199\u0198\u0001\u0000"+
		"\u0000\u0000\u019a+\u0001\u0000\u0000\u0000\u019b\u019c\u0005\u000e\u0000"+
		"\u0000\u019c\u01ac\u0005)\u0000\u0000\u019d\u019e\u0005\u000e\u0000\u0000"+
		"\u019e\u01a3\u00034\u001a\u0000\u019f\u01a0\u0005U\u0000\u0000\u01a0\u01a2"+
		"\u00034\u001a\u0000\u01a1\u019f\u0001\u0000\u0000\u0000\u01a2\u01a5\u0001"+
		"\u0000\u0000\u0000\u01a3\u01a1\u0001\u0000\u0000\u0000\u01a3\u01a4\u0001"+
		"\u0000\u0000\u0000\u01a4\u01a7\u0001\u0000\u0000\u0000\u01a5\u01a3\u0001"+
		"\u0000\u0000\u0000\u01a6\u01a8\u0005U\u0000\u0000\u01a7\u01a6\u0001\u0000"+
		"\u0000\u0000\u01a7\u01a8\u0001\u0000\u0000\u0000\u01a8\u01a9\u0001\u0000"+
		"\u0000\u0000\u01a9\u01aa\u0005)\u0000\u0000\u01aa\u01ac\u0001\u0000\u0000"+
		"\u0000\u01ab\u019b\u0001\u0000\u0000\u0000\u01ab\u019d\u0001\u0000\u0000"+
		"\u0000\u01ac-\u0001\u0000\u0000\u0000\u01ad\u01ae\u0005\u000f\u0000\u0000"+
		"\u01ae\u01be\u0005W\u0000\u0000\u01af\u01b0\u0005\u000f\u0000\u0000\u01b0"+
		"\u01b5\u00034\u001a\u0000\u01b1\u01b2\u0005U\u0000\u0000\u01b2\u01b4\u0003"+
		"4\u001a\u0000\u01b3\u01b1\u0001\u0000\u0000\u0000\u01b4\u01b7\u0001\u0000"+
		"\u0000\u0000\u01b5\u01b3\u0001\u0000\u0000\u0000\u01b5\u01b6\u0001\u0000"+
		"\u0000\u0000\u01b6\u01b9\u0001\u0000\u0000\u0000\u01b7\u01b5\u0001\u0000"+
		"\u0000\u0000\u01b8\u01ba\u0005U\u0000\u0000\u01b9\u01b8\u0001\u0000\u0000"+
		"\u0000\u01b9\u01ba\u0001\u0000\u0000\u0000\u01ba\u01bb\u0001\u0000\u0000"+
		"\u0000\u01bb\u01bc\u0005W\u0000\u0000\u01bc\u01be\u0001\u0000\u0000\u0000"+
		"\u01bd\u01ad\u0001\u0000\u0000\u0000\u01bd\u01af\u0001\u0000\u0000\u0000"+
		"\u01be/\u0001\u0000\u0000\u0000\u01bf\u01c0\u0005-\u0000\u0000\u01c0\u01c1"+
		"\u00032\u0019\u0000\u01c1\u01c2\u0005F\u0000\u0000\u01c2\u01c3\u0003$"+
		"\u0012\u0000\u01c31\u0001\u0000\u0000\u0000\u01c4\u01c5\u0005I\u0000\u0000"+
		"\u01c5\u01ca\u00034\u001a\u0000\u01c6\u01c7\u0005U\u0000\u0000\u01c7\u01c9"+
		"\u00034\u001a\u0000\u01c8\u01c6\u0001\u0000\u0000\u0000\u01c9\u01cc\u0001"+
		"\u0000\u0000\u0000\u01ca\u01c8\u0001\u0000\u0000\u0000\u01ca\u01cb\u0001"+
		"\u0000\u0000\u0000\u01cb\u01ce\u0001\u0000\u0000\u0000\u01cc\u01ca\u0001"+
		"\u0000\u0000\u0000\u01cd\u01cf\u0005U\u0000\u0000\u01ce\u01cd\u0001\u0000"+
		"\u0000\u0000\u01ce\u01cf\u0001\u0000\u0000\u0000\u01cf\u01d0\u0001\u0000"+
		"\u0000\u0000\u01d0\u01d1\u0005V\u0000\u0000\u01d1\u01d6\u0001\u0000\u0000"+
		"\u0000\u01d2\u01d3\u0005I\u0000\u0000\u01d3\u01d6\u0005V\u0000\u0000\u01d4"+
		"\u01d6\u0005H\u0000\u0000\u01d5\u01c4\u0001\u0000\u0000\u0000\u01d5\u01d2"+
		"\u0001\u0000\u0000\u0000\u01d5\u01d4\u0001\u0000\u0000\u0000\u01d63\u0001"+
		"\u0000\u0000\u0000\u01d7\u01d9\u0003\u0006\u0003\u0000\u01d8\u01d7\u0001"+
		"\u0000\u0000\u0000\u01d8\u01d9\u0001\u0000\u0000\u0000\u01d9\u01da\u0001"+
		"\u0000\u0000\u0000\u01da\u01dc\u0003&\u0013\u0000\u01db\u01dd\u0005\r"+
		"\u0000\u0000\u01dc\u01db\u0001\u0000\u0000\u0000\u01dc\u01dd\u0001\u0000"+
		"\u0000\u0000\u01dd\u01e0\u0001\u0000\u0000\u0000\u01de\u01df\u0005[\u0000"+
		"\u0000\u01df\u01e1\u0003f3\u0000\u01e0\u01de\u0001\u0000\u0000\u0000\u01e0"+
		"\u01e1\u0001\u0000\u0000\u0000\u01e15\u0001\u0000\u0000\u0000\u01e2\u01e3"+
		"\u0007\u0001\u0000\u0000\u01e37\u0001\u0000\u0000\u0000\u01e4\u01ea\u0005"+
		"(\u0000\u0000\u01e5\u01e9\u0003:\u001d\u0000\u01e6\u01e9\u0005*\u0000"+
		"\u0000\u01e7\u01e9\u0003\u00b2Y\u0000\u01e8\u01e5\u0001\u0000\u0000\u0000"+
		"\u01e8\u01e6\u0001\u0000\u0000\u0000\u01e8\u01e7\u0001\u0000\u0000\u0000"+
		"\u01e9\u01ec\u0001\u0000\u0000\u0000\u01ea\u01e8\u0001\u0000\u0000\u0000"+
		"\u01ea\u01eb\u0001\u0000\u0000\u0000\u01eb\u01ed\u0001\u0000\u0000\u0000"+
		"\u01ec\u01ea\u0001\u0000\u0000\u0000\u01ed\u01ee\u0005)\u0000\u0000\u01ee"+
		"9\u0001\u0000\u0000\u0000\u01ef\u01f3\u0003\u0006\u0003\u0000\u01f0\u01f2"+
		"\u0003\u00b2Y\u0000\u01f1\u01f0\u0001\u0000\u0000\u0000\u01f2\u01f5\u0001"+
		"\u0000\u0000\u0000\u01f3\u01f1\u0001\u0000\u0000\u0000\u01f3\u01f4\u0001"+
		"\u0000\u0000\u0000\u01f4\u01f6\u0001\u0000\u0000\u0000\u01f5\u01f3\u0001"+
		"\u0000\u0000\u0000\u01f6\u01f7\u0003<\u001e\u0000\u01f7\u01fa\u0001\u0000"+
		"\u0000\u0000\u01f8\u01fa\u0003<\u001e\u0000\u01f9\u01ef\u0001\u0000\u0000"+
		"\u0000\u01f9\u01f8\u0001\u0000\u0000\u0000\u01fa;\u0001\u0000\u0000\u0000"+
		"\u01fb\u01fc\u0003\u009cN\u0000\u01fc\u01fd\u0005$\u0000\u0000\u01fd\u020c"+
		"\u0001\u0000\u0000\u0000\u01fe\u0200\u0003\u009cN\u0000\u01ff\u01fe\u0001"+
		"\u0000\u0000\u0000\u01ff\u0200\u0001\u0000\u0000\u0000\u0200\u0201\u0001"+
		"\u0000\u0000\u0000\u0201\u020c\u00038\u001c\u0000\u0202\u020c\u0003>\u001f"+
		"\u0000\u0203\u020c\u0003V+\u0000\u0204\u020c\u0003X,\u0000\u0205\u020c"+
		"\u0003Z-\u0000\u0206\u020c\u0003^/\u0000\u0207\u020c\u0003N\'\u0000\u0208"+
		"\u020c\u0003@ \u0000\u0209\u020c\u0003H$\u0000\u020a\u020c\u0003h4\u0000"+
		"\u020b\u01fb\u0001\u0000\u0000\u0000\u020b\u01ff\u0001\u0000\u0000\u0000"+
		"\u020b\u0202\u0001\u0000\u0000\u0000\u020b\u0203\u0001\u0000\u0000\u0000"+
		"\u020b\u0204\u0001\u0000\u0000\u0000\u020b\u0205\u0001\u0000\u0000\u0000"+
		"\u020b\u0206\u0001\u0000\u0000\u0000\u020b\u0207\u0001\u0000\u0000\u0000"+
		"\u020b\u0208\u0001\u0000\u0000\u0000\u020b\u0209\u0001\u0000\u0000\u0000"+
		"\u020b\u020a\u0001\u0000\u0000\u0000\u020c=\u0001\u0000\u0000\u0000\u020d"+
		"\u020e\u0003\u00a8T\u0000\u020e\u0210\u0003\u0084B\u0000\u020f\u0211\u0003"+
		"z=\u0000\u0210\u020f\u0001\u0000\u0000\u0000\u0210\u0211\u0001\u0000\u0000"+
		"\u0000\u0211\u0216\u0001\u0000\u0000\u0000\u0212\u0213\u0003\u00a8T\u0000"+
		"\u0213\u0214\u0003z=\u0000\u0214\u0216\u0001\u0000\u0000\u0000\u0215\u020d"+
		"\u0001\u0000\u0000\u0000\u0215\u0212\u0001\u0000\u0000\u0000\u0216?\u0001"+
		"\u0000\u0000\u0000\u0217\u0218\u0005<\u0000\u0000\u0218\u0219\u0003F#"+
		"\u0000\u0219\u0224\u00038\u001c\u0000\u021a\u021c\u0003\u00b2Y\u0000\u021b"+
		"\u021a\u0001\u0000\u0000\u0000\u021c\u021f\u0001\u0000\u0000\u0000\u021d"+
		"\u021b\u0001\u0000\u0000\u0000\u021d\u021e\u0001\u0000\u0000\u0000\u021e"+
		"\u0220\u0001\u0000\u0000\u0000\u021f\u021d\u0001\u0000\u0000\u0000\u0220"+
		"\u0221\u00055\u0000\u0000\u0221\u0222\u0003D\"\u0000\u0222\u0223\u0005"+
		"$\u0000\u0000\u0223\u0225\u0001\u0000\u0000\u0000\u0224\u021d\u0001\u0000"+
		"\u0000\u0000\u0224\u0225\u0001\u0000\u0000\u0000\u0225A\u0001\u0000\u0000"+
		"\u0000\u0226\u0227\u00055\u0000\u0000\u0227\u0228\u0005F\u0000\u0000\u0228"+
		"\u0229\u0003D\"\u0000\u0229\u022a\u0005$\u0000\u0000\u022aC\u0001\u0000"+
		"\u0000\u0000\u022b\u022e\u0003f3\u0000\u022c\u022e\u0003:\u001d\u0000"+
		"\u022d\u022b\u0001\u0000\u0000\u0000\u022d\u022c\u0001\u0000\u0000\u0000"+
		"\u022eE\u0001\u0000\u0000\u0000\u022f\u0230\u0003\u0082A\u0000\u0230G"+
		"\u0001\u0000\u0000\u0000\u0231\u0232\u0005<\u0000\u0000\u0232\u0233\u0005"+
		"(\u0000\u0000\u0233\u0234\u0005$\u0000\u0000\u0234\u0235\u0003J%\u0000"+
		"\u0235\u0236\u0005)\u0000\u0000\u0236I\u0001\u0000\u0000\u0000\u0237\u023c"+
		"\u0003L&\u0000\u0238\u023b\u0003L&\u0000\u0239\u023b\u0003\u00b2Y\u0000"+
		"\u023a\u0238\u0001\u0000\u0000\u0000\u023a\u0239\u0001\u0000\u0000\u0000"+
		"\u023b\u023e\u0001\u0000\u0000\u0000\u023c\u023a\u0001\u0000\u0000\u0000"+
		"\u023c\u023d\u0001\u0000\u0000\u0000\u023d\u0240\u0001\u0000\u0000\u0000"+
		"\u023e\u023c\u0001\u0000\u0000\u0000\u023f\u0241\u0003B!\u0000\u0240\u023f"+
		"\u0001\u0000\u0000\u0000\u0240\u0241\u0001\u0000\u0000\u0000\u0241K\u0001"+
		"\u0000\u0000\u0000\u0242\u0243\u0003F#\u0000\u0243\u0244\u0005F\u0000"+
		"\u0000\u0244\u0245\u0003D\"\u0000\u0245\u0246\u0005$\u0000\u0000\u0246"+
		"M\u0001\u0000\u0000\u0000\u0247\u0249\u0005<\u0000\u0000\u0248\u024a\u0003"+
		"f3\u0000\u0249\u0248\u0001\u0000\u0000\u0000\u0249\u024a\u0001\u0000\u0000"+
		"\u0000\u024a\u024b\u0001\u0000\u0000\u0000\u024b\u024c\u0005(\u0000\u0000"+
		"\u024c\u024d\u0005$\u0000\u0000\u024d\u024e\u0003P(\u0000\u024e\u024f"+
		"\u0005)\u0000\u0000\u024fO\u0001\u0000\u0000\u0000\u0250\u0252\u0003\u00b2"+
		"Y\u0000\u0251\u0250\u0001\u0000\u0000\u0000\u0252\u0255\u0001\u0000\u0000"+
		"\u0000\u0253\u0251\u0001\u0000\u0000\u0000\u0253\u0254\u0001\u0000\u0000"+
		"\u0000\u0254\u0256\u0001\u0000\u0000\u0000\u0255\u0253\u0001\u0000\u0000"+
		"\u0000\u0256\u025b\u0003R)\u0000\u0257\u025a\u0003R)\u0000\u0258\u025a"+
		"\u0003\u00b2Y\u0000\u0259\u0257\u0001\u0000\u0000\u0000\u0259\u0258\u0001"+
		"\u0000\u0000\u0000\u025a\u025d\u0001\u0000\u0000\u0000\u025b\u0259\u0001"+
		"\u0000\u0000\u0000\u025b\u025c\u0001\u0000\u0000\u0000\u025c\u025f\u0001"+
		"\u0000\u0000\u0000\u025d\u025b\u0001\u0000\u0000\u0000\u025e\u0260\u0003"+
		"B!\u0000\u025f\u025e\u0001\u0000\u0000\u0000\u025f\u0260\u0001\u0000\u0000"+
		"\u0000\u0260Q\u0001\u0000\u0000\u0000\u0261\u0262\u0003T*\u0000\u0262"+
		"\u0263\u0005F\u0000\u0000\u0263\u0264\u0003D\"\u0000\u0264\u0265\u0005"+
		"$\u0000\u0000\u0265S\u0001\u0000\u0000\u0000\u0266\u0269\u0003f3\u0000"+
		"\u0267\u0269\u0003\u008eG\u0000\u0268\u0266\u0001\u0000\u0000\u0000\u0268"+
		"\u0267\u0001\u0000\u0000\u0000\u0269U\u0001\u0000\u0000\u0000\u026a\u026c"+
		"\u0005;\u0000\u0000\u026b\u026a\u0001\u0000\u0000\u0000\u026b\u026c\u0001"+
		"\u0000\u0000\u0000\u026c\u026d\u0001\u0000\u0000\u0000\u026d\u026e\u0005"+
		":\u0000\u0000\u026e\u0270\u0003\u00a4R\u0000\u026f\u0271\u0003\\.\u0000"+
		"\u0270\u026f\u0001\u0000\u0000\u0000\u0270\u0271\u0001\u0000\u0000\u0000"+
		"\u0271\u027c\u0001\u0000\u0000\u0000\u0272\u0274\u0005;\u0000\u0000\u0273"+
		"\u0272\u0001\u0000\u0000\u0000\u0273\u0274\u0001\u0000\u0000\u0000\u0274"+
		"\u0275\u0001\u0000\u0000\u0000\u0275\u0276\u0005:\u0000\u0000\u0276\u0277"+
		"\u0003$\u0012\u0000\u0277\u0279\u0003\u00a4R\u0000\u0278\u027a\u0003\\"+
		".\u0000\u0279\u0278\u0001\u0000\u0000\u0000\u0279\u027a\u0001\u0000\u0000"+
		"\u0000\u027a\u027c\u0001\u0000\u0000\u0000\u027b\u026b\u0001\u0000\u0000"+
		"\u0000\u027b\u0273\u0001\u0000\u0000\u0000\u027cW\u0001\u0000\u0000\u0000"+
		"\u027d\u027e\u0005X\u0000\u0000\u027e\u027f\u0003\u00a4R\u0000\u027f\u0280"+
		"\u0003\\.\u0000\u0280Y\u0001\u0000\u0000\u0000\u0281\u0282\u0005D\u0000"+
		"\u0000\u0282\u0284\u0003\u00a6S\u0000\u0283\u0285\u0003\u0084B\u0000\u0284"+
		"\u0283\u0001\u0000\u0000\u0000\u0284\u0285\u0001\u0000\u0000\u0000\u0285"+
		"\u0289\u0001\u0000\u0000\u0000\u0286\u0288\u0003\u0098L\u0000\u0287\u0286"+
		"\u0001\u0000\u0000\u0000\u0288\u028b\u0001\u0000\u0000\u0000\u0289\u0287"+
		"\u0001\u0000\u0000\u0000\u0289\u028a\u0001\u0000\u0000\u0000\u028a[\u0001"+
		"\u0000\u0000\u0000\u028b\u0289\u0001\u0000\u0000\u0000\u028c\u0291\u0005"+
		"E\u0000\u0000\u028d\u0292\u0003f3\u0000\u028e\u0292\u0003N\'\u0000\u028f"+
		"\u0292\u0003H$\u0000\u0290\u0292\u0003>\u001f\u0000\u0291\u028d\u0001"+
		"\u0000\u0000\u0000\u0291\u028e\u0001\u0000\u0000\u0000\u0291\u028f\u0001"+
		"\u0000\u0000\u0000\u0291\u0290\u0001\u0000\u0000\u0000\u0292\u0295\u0001"+
		"\u0000\u0000\u0000\u0293\u0295\u0003z=\u0000\u0294\u028c\u0001\u0000\u0000"+
		"\u0000\u0294\u0293\u0001\u0000\u0000\u0000\u0295]\u0001\u0000\u0000\u0000"+
		"\u0296\u0298\u00056\u0000\u0000\u0297\u0299\u0003\u0082A\u0000\u0298\u0297"+
		"\u0001\u0000\u0000\u0000\u0298\u0299\u0001\u0000\u0000\u0000\u0299\u029f"+
		"\u0001\u0000\u0000\u0000\u029a\u029b\u00057\u0000\u0000\u029b\u029f\u0005"+
		"\n\u0000\u0000\u029c\u029f\u00059\u0000\u0000\u029d\u029f\u00058\u0000"+
		"\u0000\u029e\u0296\u0001\u0000\u0000\u0000\u029e\u029a\u0001\u0000\u0000"+
		"\u0000\u029e\u029c\u0001\u0000\u0000\u0000\u029e\u029d\u0001\u0000\u0000"+
		"\u0000\u029f_\u0001\u0000\u0000\u0000\u02a0\u02a1\u0005\u0007\u0000\u0000"+
		"\u02a1\u02a2\u0003b1\u0000\u02a2\u02a3\u0005\u0002\u0000\u0000\u02a3a"+
		"\u0001\u0000\u0000\u0000\u02a4\u02a9\u0005N\u0000\u0000\u02a5\u02a9\u0005"+
		"O\u0000\u0000\u02a6\u02a9\u0005P\u0000\u0000\u02a7\u02a9\u0003d2\u0000"+
		"\u02a8\u02a4\u0001\u0000\u0000\u0000\u02a8\u02a5\u0001\u0000\u0000\u0000"+
		"\u02a8\u02a6\u0001\u0000\u0000\u0000\u02a8\u02a7\u0001\u0000\u0000\u0000"+
		"\u02a9\u02aa\u0001\u0000\u0000\u0000\u02aa\u02a8\u0001\u0000\u0000\u0000"+
		"\u02aa\u02ab\u0001\u0000\u0000\u0000\u02abc\u0001\u0000\u0000\u0000\u02ac"+
		"\u02ad\u0005\b\u0000\u0000\u02ad\u02ae\u0003\u0090H\u0000\u02ae\u02b0"+
		"\u0005)\u0000\u0000\u02af\u02b1\u0005a\u0000\u0000\u02b0\u02af\u0001\u0000"+
		"\u0000\u0000\u02b0\u02b1\u0001\u0000\u0000\u0000\u02b1e\u0001\u0000\u0000"+
		"\u0000\u02b2\u02b5\u0003j5\u0000\u02b3\u02b5\u0003\u0088D\u0000\u02b4"+
		"\u02b2\u0001\u0000\u0000\u0000\u02b4\u02b3\u0001\u0000\u0000\u0000\u02b5"+
		"g\u0001\u0000\u0000\u0000\u02b6\u02b7\u0005G\u0000\u0000\u02b7\u02b8\u0003"+
		"f3\u0000\u02b8\u02b9\u0005\f\u0000\u0000\u02b9i\u0001\u0000\u0000\u0000"+
		"\u02ba\u02bb\u00065\uffff\uffff\u0000\u02bb\u02bf\u0003l6\u0000\u02bc"+
		"\u02be\u0003\u0098L\u0000\u02bd\u02bc\u0001\u0000\u0000\u0000\u02be\u02c1"+
		"\u0001\u0000\u0000\u0000\u02bf\u02bd\u0001\u0000\u0000\u0000\u02bf\u02c0"+
		"\u0001\u0000\u0000\u0000\u02c0\u02c3\u0001\u0000\u0000\u0000\u02c1\u02bf"+
		"\u0001\u0000\u0000\u0000\u02c2\u02c4\u0003\u0096K\u0000\u02c3\u02c2\u0001"+
		"\u0000\u0000\u0000\u02c3\u02c4\u0001\u0000\u0000\u0000\u02c4\u02c9\u0001"+
		"\u0000\u0000\u0000\u02c5\u02c6\u0003r9\u0000\u02c6\u02c7\u0003j5\u0002"+
		"\u02c7\u02c9\u0001\u0000\u0000\u0000\u02c8\u02ba\u0001\u0000\u0000\u0000"+
		"\u02c8\u02c5\u0001\u0000\u0000\u0000\u02c9\u02ce\u0001\u0000\u0000\u0000"+
		"\u02ca\u02cb\n\u0001\u0000\u0000\u02cb\u02cd\u0003t:\u0000\u02cc\u02ca"+
		"\u0001\u0000\u0000\u0000\u02cd\u02d0\u0001\u0000\u0000\u0000\u02ce\u02cc"+
		"\u0001\u0000\u0000\u0000\u02ce\u02cf\u0001\u0000\u0000\u0000\u02cfk\u0001"+
		"\u0000\u0000\u0000\u02d0\u02ce\u0001\u0000\u0000\u0000\u02d1\u02df\u0003"+
		"\u008cF\u0000\u02d2\u02df\u0003p8\u0000\u02d3\u02df\u0003n7\u0000\u02d4"+
		"\u02df\u0003\u0090H\u0000\u02d5\u02df\u0003~?\u0000\u02d6\u02df\u0003"+
		"|>\u0000\u02d7\u02df\u0003h4\u0000\u02d8\u02df\u0003v;\u0000\u02d9\u02df"+
		"\u0005\u001a\u0000\u0000\u02da\u02df\u0005\u001b\u0000\u0000\u02db\u02df"+
		"\u0005\u001c\u0000\u0000\u02dc\u02df\u0005\u001d\u0000\u0000\u02dd\u02df"+
		"\u0005\u001e\u0000\u0000\u02de\u02d1\u0001\u0000\u0000\u0000\u02de\u02d2"+
		"\u0001\u0000\u0000\u0000\u02de\u02d3\u0001\u0000\u0000\u0000\u02de\u02d4"+
		"\u0001\u0000\u0000\u0000\u02de\u02d5\u0001\u0000\u0000\u0000\u02de\u02d6"+
		"\u0001\u0000\u0000\u0000\u02de\u02d7\u0001\u0000\u0000\u0000\u02de\u02d8"+
		"\u0001\u0000\u0000\u0000\u02de\u02d9\u0001\u0000\u0000\u0000\u02de\u02da"+
		"\u0001\u0000\u0000\u0000\u02de\u02db\u0001\u0000\u0000\u0000\u02de\u02dc"+
		"\u0001\u0000\u0000\u0000\u02de\u02dd\u0001\u0000\u0000\u0000\u02dfm\u0001"+
		"\u0000\u0000\u0000\u02e0\u02e6\u0005\u0018\u0000\u0000\u02e1\u02e5\u0003"+
		":\u001d\u0000\u02e2\u02e5\u0005*\u0000\u0000\u02e3\u02e5\u0003\u00b2Y"+
		"\u0000\u02e4\u02e1\u0001\u0000\u0000\u0000\u02e4\u02e2\u0001\u0000\u0000"+
		"\u0000\u02e4\u02e3\u0001\u0000\u0000\u0000\u02e5\u02e8\u0001\u0000\u0000"+
		"\u0000\u02e6\u02e4\u0001\u0000\u0000\u0000\u02e6\u02e7\u0001\u0000\u0000"+
		"\u0000\u02e7\u02e9\u0001\u0000\u0000\u0000\u02e8\u02e6\u0001\u0000\u0000"+
		"\u0000\u02e9\u02ea\u0005)\u0000\u0000\u02ea\u02eb\u0005\u0019\u0000\u0000"+
		"\u02eb\u02ec\u0003\u0090H\u0000\u02eco\u0001\u0000\u0000\u0000\u02ed\u02f3"+
		"\u0005\u0010\u0000\u0000\u02ee\u02f2\u0003:\u001d\u0000\u02ef\u02f2\u0005"+
		"*\u0000\u0000\u02f0\u02f2\u0003\u00b2Y\u0000\u02f1\u02ee\u0001\u0000\u0000"+
		"\u0000\u02f1\u02ef\u0001\u0000\u0000\u0000\u02f1\u02f0\u0001\u0000\u0000"+
		"\u0000\u02f2\u02f5\u0001\u0000\u0000\u0000\u02f3\u02f1\u0001\u0000\u0000"+
		"\u0000\u02f3\u02f4\u0001\u0000\u0000\u0000\u02f4\u02f6\u0001\u0000\u0000"+
		"\u0000\u02f5\u02f3\u0001\u0000\u0000\u0000\u02f6\u02fd\u0005)\u0000\u0000"+
		"\u02f7\u02f8\u0005\u0011\u0000\u0000\u02f8\u02fa\u0003f3\u0000\u02f9\u02fb"+
		"\u0005\f\u0000\u0000\u02fa\u02f9\u0001\u0000\u0000\u0000\u02fa\u02fb\u0001"+
		"\u0000\u0000\u0000\u02fb\u02fd\u0001\u0000\u0000\u0000\u02fc\u02ed\u0001"+
		"\u0000\u0000\u0000\u02fc\u02f7\u0001\u0000\u0000\u0000\u02fdq\u0001\u0000"+
		"\u0000\u0000\u02fe\u02ff\u0005Y\u0000\u0000\u02ffs\u0001\u0000\u0000\u0000"+
		"\u0300\u0301\u0007\u0002\u0000\u0000\u0301u\u0001\u0000\u0000\u0000\u0302"+
		"\u0303\u0003\u00a6S\u0000\u0303\u0305\u0003\u0084B\u0000\u0304\u0306\u0003"+
		"x<\u0000\u0305\u0304\u0001\u0000\u0000\u0000\u0305\u0306\u0001\u0000\u0000"+
		"\u0000\u0306w\u0001\u0000\u0000\u0000\u0307\u0308\u0005\u0016\u0000\u0000"+
		"\u0308\u030b\u0003\u0082A\u0000\u0309\u030b\u0005\u0017\u0000\u0000\u030a"+
		"\u0307\u0001\u0000\u0000\u0000\u030a\u0309\u0001\u0000\u0000\u0000\u030b"+
		"y\u0001\u0000\u0000\u0000\u030c\u030e\u0005$\u0000\u0000\u030d\u030c\u0001"+
		"\u0000\u0000\u0000\u030d\u030e\u0001\u0000\u0000\u0000\u030e\u030f\u0001"+
		"\u0000\u0000\u0000\u030f\u0312\u0003`0\u0000\u0310\u0312\u0003j5\u0000"+
		"\u0311\u030d\u0001\u0000\u0000\u0000\u0311\u0310\u0001\u0000\u0000\u0000"+
		"\u0312{\u0001\u0000\u0000\u0000\u0313\u0314\u0005\u000f\u0000\u0000\u0314"+
		"\u0324\u0005W\u0000\u0000\u0315\u0316\u0005\u000f\u0000\u0000\u0316\u031b"+
		"\u0003f3\u0000\u0317\u0318\u0005U\u0000\u0000\u0318\u031a\u0003f3\u0000"+
		"\u0319\u0317\u0001\u0000\u0000\u0000\u031a\u031d\u0001\u0000\u0000\u0000"+
		"\u031b\u0319\u0001\u0000\u0000\u0000\u031b\u031c\u0001\u0000\u0000\u0000"+
		"\u031c\u031f\u0001\u0000\u0000\u0000\u031d\u031b\u0001\u0000\u0000\u0000"+
		"\u031e\u0320\u0005U\u0000\u0000\u031f\u031e\u0001\u0000\u0000\u0000\u031f"+
		"\u0320\u0001\u0000\u0000\u0000\u0320\u0321\u0001\u0000\u0000\u0000\u0321"+
		"\u0322\u0005W\u0000\u0000\u0322\u0324\u0001\u0000\u0000\u0000\u0323\u0313"+
		"\u0001\u0000\u0000\u0000\u0323\u0315\u0001\u0000\u0000\u0000\u0324}\u0001"+
		"\u0000\u0000\u0000\u0325\u0326\u0005\u000e\u0000\u0000\u0326\u0336\u0005"+
		")\u0000\u0000\u0327\u0328\u0005\u000e\u0000\u0000\u0328\u032d\u0003\u0080"+
		"@\u0000\u0329\u032a\u0005U\u0000\u0000\u032a\u032c\u0003\u0080@\u0000"+
		"\u032b\u0329\u0001\u0000\u0000\u0000\u032c\u032f\u0001\u0000\u0000\u0000"+
		"\u032d\u032b\u0001\u0000\u0000\u0000\u032d\u032e\u0001\u0000\u0000\u0000"+
		"\u032e\u0331\u0001\u0000\u0000\u0000\u032f\u032d\u0001\u0000\u0000\u0000"+
		"\u0330\u0332\u0005U\u0000\u0000\u0331\u0330\u0001\u0000\u0000\u0000\u0331"+
		"\u0332\u0001\u0000\u0000\u0000\u0332\u0333\u0001\u0000\u0000\u0000\u0333"+
		"\u0334\u0005)\u0000\u0000\u0334\u0336\u0001\u0000\u0000\u0000\u0335\u0325"+
		"\u0001\u0000\u0000\u0000\u0335\u0327\u0001\u0000\u0000\u0000\u0336\u007f"+
		"\u0001\u0000\u0000\u0000\u0337\u0339\u0003\n\u0005\u0000\u0338\u0337\u0001"+
		"\u0000\u0000\u0000\u0339\u033c\u0001\u0000\u0000\u0000\u033a\u0338\u0001"+
		"\u0000\u0000\u0000\u033a\u033b\u0001\u0000\u0000\u0000\u033b\u033d\u0001"+
		"\u0000\u0000\u0000\u033c\u033a\u0001\u0000\u0000\u0000\u033d\u033e\u0007"+
		"\u0003\u0000\u0000\u033e\u033f\u0005[\u0000\u0000\u033f\u0340\u0003f3"+
		"\u0000\u0340\u0081\u0001\u0000\u0000\u0000\u0341\u0342\u0005I\u0000\u0000"+
		"\u0342\u0347\u0003f3\u0000\u0343\u0344\u0005U\u0000\u0000\u0344\u0346"+
		"\u0003f3\u0000\u0345\u0343\u0001\u0000\u0000\u0000\u0346\u0349\u0001\u0000"+
		"\u0000\u0000\u0347\u0345\u0001\u0000\u0000\u0000\u0347\u0348\u0001\u0000"+
		"\u0000\u0000\u0348\u034b\u0001\u0000\u0000\u0000\u0349\u0347\u0001\u0000"+
		"\u0000\u0000\u034a\u034c\u0005U\u0000\u0000\u034b\u034a\u0001\u0000\u0000"+
		"\u0000\u034b\u034c\u0001\u0000\u0000\u0000\u034c\u034d\u0001\u0000\u0000"+
		"\u0000\u034d\u034e\u0005V\u0000\u0000\u034e\u0358\u0001\u0000\u0000\u0000"+
		"\u034f\u0350\u0005I\u0000\u0000\u0350\u0358\u0005V\u0000\u0000\u0351\u0358"+
		"\u0005H\u0000\u0000\u0352\u0354\u0003\u0088D\u0000\u0353\u0352\u0001\u0000"+
		"\u0000\u0000\u0354\u0355\u0001\u0000\u0000\u0000\u0355\u0353\u0001\u0000"+
		"\u0000\u0000\u0355\u0356\u0001\u0000\u0000\u0000\u0356\u0358\u0001\u0000"+
		"\u0000\u0000\u0357\u0341\u0001\u0000\u0000\u0000\u0357\u034f\u0001\u0000"+
		"\u0000\u0000\u0357\u0351\u0001\u0000\u0000\u0000\u0357\u0353\u0001\u0000"+
		"\u0000\u0000\u0358\u0083\u0001\u0000\u0000\u0000\u0359\u035a\u0005I\u0000"+
		"\u0000\u035a\u035f\u0003\u0086C\u0000\u035b\u035c\u0005U\u0000\u0000\u035c"+
		"\u035e\u0003\u0086C\u0000\u035d\u035b\u0001\u0000\u0000\u0000\u035e\u0361"+
		"\u0001\u0000\u0000\u0000\u035f\u035d\u0001\u0000\u0000\u0000\u035f\u0360"+
		"\u0001\u0000\u0000\u0000\u0360\u0363\u0001\u0000\u0000\u0000\u0361\u035f"+
		"\u0001\u0000\u0000\u0000\u0362\u0364\u0005U\u0000\u0000\u0363\u0362\u0001"+
		"\u0000\u0000\u0000\u0363\u0364\u0001\u0000\u0000\u0000\u0364\u0365\u0001"+
		"\u0000\u0000\u0000\u0365\u0366\u0005V\u0000\u0000\u0366\u0370\u0001\u0000"+
		"\u0000\u0000\u0367\u0368\u0005I\u0000\u0000\u0368\u0370\u0005V\u0000\u0000"+
		"\u0369\u0370\u0005H\u0000\u0000\u036a\u036c\u0003\u0088D\u0000\u036b\u036a"+
		"\u0001\u0000\u0000\u0000\u036c\u036d\u0001\u0000\u0000\u0000\u036d\u036b"+
		"\u0001\u0000\u0000\u0000\u036d\u036e\u0001\u0000\u0000\u0000\u036e\u0370"+
		"\u0001\u0000\u0000\u0000\u036f\u0359\u0001\u0000\u0000\u0000\u036f\u0367"+
		"\u0001\u0000\u0000\u0000\u036f\u0369\u0001\u0000\u0000\u0000\u036f\u036b"+
		"\u0001\u0000\u0000\u0000\u0370\u0085\u0001\u0000\u0000\u0000\u0371\u0372"+
		"\u0007\u0003\u0000\u0000\u0372\u0373\u0005[\u0000\u0000\u0373\u0376\u0003"+
		"f3\u0000\u0374\u0376\u0003f3\u0000\u0375\u0371\u0001\u0000\u0000\u0000"+
		"\u0375\u0374\u0001\u0000\u0000\u0000\u0376\u0087\u0001\u0000\u0000\u0000"+
		"\u0377\u0378\u0005\u0004\u0000\u0000\u0378\u0379\u0003\u008aE\u0000\u0379"+
		"\u037a\u0005M\u0000\u0000\u037a\u0089\u0001\u0000\u0000\u0000\u037b\u037f"+
		"\u0005L\u0000\u0000\u037c\u037f\u0003\u00b6[\u0000\u037d\u037f\u0003\u008c"+
		"F\u0000\u037e\u037b\u0001\u0000\u0000\u0000\u037e\u037c\u0001\u0000\u0000"+
		"\u0000\u037e\u037d\u0001\u0000\u0000\u0000\u037f\u0382\u0001\u0000\u0000"+
		"\u0000\u0380\u037e\u0001\u0000\u0000\u0000\u0380\u0381\u0001\u0000\u0000"+
		"\u0000\u0381\u008b\u0001\u0000\u0000\u0000\u0382\u0380\u0001\u0000\u0000"+
		"\u0000\u0383\u0384\u0005\t\u0000\u0000\u0384\u0385\u0003f3\u0000\u0385"+
		"\u0386\u0005W\u0000\u0000\u0386\u008d\u0001\u0000\u0000\u0000\u0387\u0389"+
		"\u0005\t\u0000\u0000\u0388\u038a\u0003\u0098L\u0000\u0389\u0388\u0001"+
		"\u0000\u0000\u0000\u038a\u038b\u0001\u0000\u0000\u0000\u038b\u0389\u0001"+
		"\u0000\u0000\u0000\u038b\u038c\u0001\u0000\u0000\u0000\u038c\u038d\u0001"+
		"\u0000\u0000\u0000\u038d\u038e\u0005W\u0000\u0000\u038e\u008f\u0001\u0000"+
		"\u0000\u0000\u038f\u0393\u0003\u0094J\u0000\u0390\u0392\u0003\u0098L\u0000"+
		"\u0391\u0390\u0001\u0000\u0000\u0000\u0392\u0395\u0001\u0000\u0000\u0000"+
		"\u0393\u0391\u0001\u0000\u0000\u0000\u0393\u0394\u0001\u0000\u0000\u0000"+
		"\u0394\u0091\u0001\u0000\u0000\u0000\u0395\u0393\u0001\u0000\u0000\u0000"+
		"\u0396\u0397\u0005\u0015\u0000\u0000\u0397\u0398\u0003\u0082A\u0000\u0398"+
		"\u0093\u0001\u0000\u0000\u0000\u0399\u039e\u0003\u0092I\u0000\u039a\u039e"+
		"\u0005\r\u0000\u0000\u039b\u039c\u0005X\u0000\u0000\u039c\u039e\u0007"+
		"\u0004\u0000\u0000\u039d\u0399\u0001\u0000\u0000\u0000\u039d\u039a\u0001"+
		"\u0000\u0000\u0000\u039d\u039b\u0001\u0000\u0000\u0000\u039e\u0095\u0001"+
		"\u0000\u0000\u0000\u039f\u03a0\u0005Z\u0000\u0000\u03a0\u03a1\u0003f3"+
		"\u0000\u03a1\u0097\u0001\u0000\u0000\u0000\u03a2\u03a3\u0003\u009aM\u0000"+
		"\u03a3\u03a5\u0003\u00a6S\u0000\u03a4\u03a6\u0003\u0084B\u0000\u03a5\u03a4"+
		"\u0001\u0000\u0000\u0000\u03a5\u03a6\u0001\u0000\u0000\u0000\u03a6\u03a8"+
		"\u0001\u0000\u0000\u0000\u03a7\u03a9\u0003x<\u0000\u03a8\u03a7\u0001\u0000"+
		"\u0000\u0000\u03a8\u03a9\u0001\u0000\u0000\u0000\u03a9\u03ad\u0001\u0000"+
		"\u0000\u0000\u03aa\u03ad\u0003\u009eO\u0000\u03ab\u03ad\u0005T\u0000\u0000"+
		"\u03ac\u03a2\u0001\u0000\u0000\u0000\u03ac\u03aa\u0001\u0000\u0000\u0000"+
		"\u03ac\u03ab\u0001\u0000\u0000\u0000\u03ad\u0099\u0001\u0000\u0000\u0000"+
		"\u03ae\u03af\u0007\u0005\u0000\u0000\u03af\u009b\u0001\u0000\u0000\u0000"+
		"\u03b0\u03b1\u0005C\u0000\u0000\u03b1\u03b2\u0005d\u0000\u0000\u03b2\u009d"+
		"\u0001\u0000\u0000\u0000\u03b3\u03b4\u0005S\u0000\u0000\u03b4\u03b5\u0005"+
		"c\u0000\u0000\u03b5\u009f\u0001\u0000\u0000\u0000\u03b6\u03ba\u0005\n"+
		"\u0000\u0000\u03b7\u03ba\u0005\u000b\u0000\u0000\u03b8\u03ba\u0003\u0088"+
		"D\u0000\u03b9\u03b6\u0001\u0000\u0000\u0000\u03b9\u03b7\u0001\u0000\u0000"+
		"\u0000\u03b9\u03b8\u0001\u0000\u0000\u0000\u03ba\u00a1\u0001\u0000\u0000"+
		"\u0000\u03bb\u03be\u0005\n\u0000\u0000\u03bc\u03be\u0003\u0088D\u0000"+
		"\u03bd\u03bb\u0001\u0000\u0000\u0000\u03bd\u03bc\u0001\u0000\u0000\u0000"+
		"\u03be\u00a3\u0001\u0000\u0000\u0000\u03bf\u03c0\u0005\n\u0000\u0000\u03c0"+
		"\u00a5\u0001\u0000\u0000\u0000\u03c1\u03c2\u0007\u0006\u0000\u0000\u03c2"+
		"\u00a7\u0001\u0000\u0000\u0000\u03c3\u03c4\u0007\u0007\u0000\u0000\u03c4"+
		"\u00a9\u0001\u0000\u0000\u0000\u03c5\u03c6\u0007\u0001\u0000\u0000\u03c6"+
		"\u00ab\u0001\u0000\u0000\u0000\u03c7\u03c8\u0007\b\u0000\u0000\u03c8\u00ad"+
		"\u0001\u0000\u0000\u0000\u03c9\u03ca\u00054\u0000\u0000\u03ca\u00af\u0001"+
		"\u0000\u0000\u0000\u03cb\u03cc\u0007\t\u0000\u0000\u03cc\u00b1\u0001\u0000"+
		"\u0000\u0000\u03cd\u03d0\u0003\u00b4Z\u0000\u03ce\u03d0\u0005$\u0000\u0000"+
		"\u03cf\u03cd\u0001\u0000\u0000\u0000\u03cf\u03ce\u0001\u0000\u0000\u0000"+
		"\u03d0\u00b3\u0001\u0000\u0000\u0000\u03d1\u03d2\u0007\n\u0000\u0000\u03d2"+
		"\u00b5\u0001\u0000\u0000\u0000\u03d3\u03d4\u0005\u0003\u0000\u0000\u03d4"+
		"\u03d5\u0007\u000b\u0000\u0000\u03d5\u00b7\u0001\u0000\u0000\u0000\u007f"+
		"\u00bb\u00bf\u00c5\u00ca\u00d3\u00da\u00e1\u00e8\u00ee\u00f4\u00fa\u0100"+
		"\u0107\u010b\u010e\u0117\u011d\u0126\u012e\u0133\u013e\u0140\u0148\u0151"+
		"\u0157\u015c\u0163\u0168\u0171\u0177\u017b\u017e\u0180\u0184\u0187\u018e"+
		"\u0193\u0199\u01a3\u01a7\u01ab\u01b5\u01b9\u01bd\u01ca\u01ce\u01d5\u01d8"+
		"\u01dc\u01e0\u01e8\u01ea\u01f3\u01f9\u01ff\u020b\u0210\u0215\u021d\u0224"+
		"\u022d\u023a\u023c\u0240\u0249\u0253\u0259\u025b\u025f\u0268\u026b\u0270"+
		"\u0273\u0279\u027b\u0284\u0289\u0291\u0294\u0298\u029e\u02a8\u02aa\u02b0"+
		"\u02b4\u02bf\u02c3\u02c8\u02ce\u02de\u02e4\u02e6\u02f1\u02f3\u02fa\u02fc"+
		"\u0305\u030a\u030d\u0311\u031b\u031f\u0323\u032d\u0331\u0335\u033a\u0347"+
		"\u034b\u0355\u0357\u035f\u0363\u036d\u036f\u0375\u037e\u0380\u038b\u0393"+
		"\u039d\u03a5\u03a8\u03ac\u03b9\u03bd\u03cf";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}