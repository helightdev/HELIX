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
		KEYWORD_EXPRESSION=48, KEYWORD_MIXIN=49, KEYWORD_PRELUDE=50, KEYWORD_DERIVATION=51,
		KEYWORD_ELSE=52, KEYWORD_RETURN=53, KEYWORD_GOTO=54, KEYWORD_BREAK=55,
		KEYWORD_CONTINUE=56, KEYWORD_LOCAL=57, KEYWORD_CARRY=58, KEYWORD_WHEN=59,
		KEYWORD_PURE=60, KEYWORD_INLINE=61, KEYWORD_NOINLINE=62, KEYWORD_STRICT=63,
		LABEL_PREFIX=64, TOPLEVEL_DERIVATION=65, ASSIGN=66, ARROW=67, TOPLEVEL_VALUE_EXPRESSION=68,
		EMPTY_PARAMETERS=69, BEGIN_PARAMETERS=70, METADATA_WHITESPACE=71, DERIVATION_WHITESPACE=72,
		ARGUMENT_TEXT=73, ARGUMENT_END=74, CONTENT_WRAP=75, CONTENT_LINEBREAK=76,
		CONTENT_TEXT=77, VALUE_FUNCTION=78, VALUE_PREDICATE=79, VALUE_MEMBER=80,
		VALUE_WRAP=81, VALUE_DELIMITER=82, END_PARAMETERS=83, VALUE_END_INLINE=84,
		VALUE_SMART_ROOT=85, NOT_VALUE=86, VALUE_ELVIS=87, VALUE_ASSIGN=88, VALUE_EXPAND=89,
		VALUE_WHITESPACE=90, ESCAPE_MACRO=91, ESCAPE_LITERAL=92, ESCAPE_HEX=93,
		END_CONTENT_INTERPOLATE=94, FUNCTION_IDENTIFIER=95, MEMBER_IDENTIFIER=96,
		LABEL_IDENTIFIER=97, TOPLEVEL_QUESTION=98, TOPLEVEL_PIPE=99, TOPLEVEL_SMART_ROOT=100,
		TOPLEVEL_NULL=101, VALUE_CAST_BOOLEAN=102, VALUE_EXPLICIT_ROOT=103, VALUE_STATIC_CALL=104,
		VALUE_DYNAMIC_CALL=105, VALUE_BLOCK_RESULT=106, VALUE_MISSING=107;
	public static final int
		RULE_compilationUnit = 0, RULE_fileMetadataSection = 1, RULE_topLevelDeclaration = 2,
		RULE_metadataList = 3, RULE_inlineMetadataList = 4, RULE_metadata = 5,
		RULE_metadataValue = 6, RULE_mixinDeclaration = 7, RULE_typeDeclaration = 8,
		RULE_mixinBody = 9, RULE_expressionDeclaration = 10, RULE_funcDeclaration = 11,
		RULE_directFunctionSignature = 12, RULE_collectiveInputPattern = 13, RULE_outputSignatureFields = 14,
		RULE_functionBody = 15, RULE_patternExpression = 16, RULE_patternUnion = 17,
		RULE_patternTerm = 18, RULE_patternPrimary = 19, RULE_tablePattern = 20,
		RULE_tuplePattern = 21, RULE_delegatePattern = 22, RULE_patternParameterList = 23,
		RULE_patternField = 24, RULE_patternIdentifier = 25, RULE_statementBlock = 26,
		RULE_statement = 27, RULE_statementBody = 28, RULE_invocationStatement = 29,
		RULE_whenConditionStatement = 30, RULE_whenElseBranch = 31, RULE_whenResult = 32,
		RULE_whenChainCondition = 33, RULE_whenChainStatement = 34, RULE_whenChainBody = 35,
		RULE_whenChainBranch = 36, RULE_whenValueStatement = 37, RULE_whenValueBody = 38,
		RULE_whenValueBranch = 39, RULE_whenValueCondition = 40, RULE_localDeclarationStatement = 41,
		RULE_localAssignmentStatement = 42, RULE_toplevelDerivationStatement = 43,
		RULE_assignedValue = 44, RULE_controlflowStatement = 45, RULE_contentBlock = 46,
		RULE_contentBody = 47, RULE_contentInterpolate = 48, RULE_value = 49,
		RULE_valueExpression = 50, RULE_nonArgumentValue = 51, RULE_primaryValue = 52,
		RULE_inlineBlockValue = 53, RULE_lambdaValue = 54, RULE_prefixOperators = 55,
		RULE_postfixOperators = 56, RULE_valueStatement = 57, RULE_callBinding = 58,
		RULE_tailValue = 59, RULE_tupleValue = 60, RULE_tableValue = 61, RULE_tableKeyedEntry = 62,
		RULE_valueList = 63, RULE_callValueList = 64, RULE_callArgument = 65,
		RULE_argumentValue = 66, RULE_argumentBody = 67, RULE_inlineValue = 68,
		RULE_inlineTransformation = 69, RULE_derivation = 70, RULE_explicitRoot = 71,
		RULE_derivationRoot = 72, RULE_elvisValue = 73, RULE_transformationPart = 74,
		RULE_functionChainType = 75, RULE_labelIdentifier = 76, RULE_memberIdentifier = 77,
		RULE_mixinIdentifier = 78, RULE_functionDeclarationIdentifier = 79, RULE_variableIdentifier = 80,
		RULE_functionIdentifier = 81, RULE_invocationIdentifier = 82, RULE_kindIdentifier = 83,
		RULE_expressionModifier = 84, RULE_mixinModifier = 85, RULE_funcModifier = 86,
		RULE_trivia = 87, RULE_comment = 88, RULE_escaped = 89;
	private static String[] makeRuleNames() {
		return new String[] {
			"compilationUnit", "fileMetadataSection", "topLevelDeclaration", "metadataList",
			"inlineMetadataList", "metadata", "metadataValue", "mixinDeclaration",
			"typeDeclaration", "mixinBody", "expressionDeclaration", "funcDeclaration",
			"directFunctionSignature", "collectiveInputPattern", "outputSignatureFields",
			"functionBody", "patternExpression", "patternUnion", "patternTerm", "patternPrimary",
			"tablePattern", "tuplePattern", "delegatePattern", "patternParameterList",
			"patternField", "patternIdentifier", "statementBlock", "statement", "statementBody",
			"invocationStatement", "whenConditionStatement", "whenElseBranch", "whenResult",
			"whenChainCondition", "whenChainStatement", "whenChainBody", "whenChainBranch",
			"whenValueStatement", "whenValueBody", "whenValueBranch", "whenValueCondition",
			"localDeclarationStatement", "localAssignmentStatement", "toplevelDerivationStatement",
			"assignedValue", "controlflowStatement", "contentBlock", "contentBody",
			"contentInterpolate", "value", "valueExpression", "nonArgumentValue",
			"primaryValue", "inlineBlockValue", "lambdaValue", "prefixOperators",
			"postfixOperators", "valueStatement", "callBinding", "tailValue", "tupleValue",
			"tableValue", "tableKeyedEntry", "valueList", "callValueList", "callArgument",
			"argumentValue", "argumentBody", "inlineValue", "inlineTransformation",
			"derivation", "explicitRoot", "derivationRoot", "elvisValue", "transformationPart",
			"functionChainType", "labelIdentifier", "memberIdentifier", "mixinIdentifier",
			"functionDeclarationIdentifier", "variableIdentifier", "functionIdentifier",
			"invocationIdentifier", "kindIdentifier", "expressionModifier", "mixinModifier",
			"funcModifier", "trivia", "comment", "escaped"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, "'@@'", null, null,
			null, null, null, "'---'", "'{'", null, null, null, "'func'", null, "'type'",
			"'do'", "'expression'", "'mixin'", "'prelude'", "'derivation'", "'else'",
			"'return'", "'goto'", "'break'", "'continue'", "'local'", "'carry'",
			"'when'", "'pure'", "'inline'", "'noinline'", "'strict'", null, null,
			null, "'->'", null, null, "'('", null, null, null, "'>'", null, null,
			null, null, "':?'", "'#'", null, null, "')'", "']'", null, "'!'", "'?:'",
			null, "'...'", null, null, null, null, null, null, null, null, null,
			null, null, null, "'@bool'", "'@root'", "'@static'", "'@dynamic'", "'=>'",
			"'missing'"
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
			"KEYWORD_DELEGATE", "KEYWORD_TYPE", "KEYWORD_DO", "KEYWORD_EXPRESSION",
			"KEYWORD_MIXIN", "KEYWORD_PRELUDE", "KEYWORD_DERIVATION", "KEYWORD_ELSE",
			"KEYWORD_RETURN", "KEYWORD_GOTO", "KEYWORD_BREAK", "KEYWORD_CONTINUE",
			"KEYWORD_LOCAL", "KEYWORD_CARRY", "KEYWORD_WHEN", "KEYWORD_PURE", "KEYWORD_INLINE",
			"KEYWORD_NOINLINE", "KEYWORD_STRICT", "LABEL_PREFIX", "TOPLEVEL_DERIVATION",
			"ASSIGN", "ARROW", "TOPLEVEL_VALUE_EXPRESSION", "EMPTY_PARAMETERS", "BEGIN_PARAMETERS",
			"METADATA_WHITESPACE", "DERIVATION_WHITESPACE", "ARGUMENT_TEXT", "ARGUMENT_END",
			"CONTENT_WRAP", "CONTENT_LINEBREAK", "CONTENT_TEXT", "VALUE_FUNCTION",
			"VALUE_PREDICATE", "VALUE_MEMBER", "VALUE_WRAP", "VALUE_DELIMITER", "END_PARAMETERS",
			"VALUE_END_INLINE", "VALUE_SMART_ROOT", "NOT_VALUE", "VALUE_ELVIS", "VALUE_ASSIGN",
			"VALUE_EXPAND", "VALUE_WHITESPACE", "ESCAPE_MACRO", "ESCAPE_LITERAL",
			"ESCAPE_HEX", "END_CONTENT_INTERPOLATE", "FUNCTION_IDENTIFIER", "MEMBER_IDENTIFIER",
			"LABEL_IDENTIFIER", "TOPLEVEL_QUESTION", "TOPLEVEL_PIPE", "TOPLEVEL_SMART_ROOT",
			"TOPLEVEL_NULL", "VALUE_CAST_BOOLEAN", "VALUE_EXPLICIT_ROOT", "VALUE_STATIC_CALL",
			"VALUE_DYNAMIC_CALL", "VALUE_BLOCK_RESULT", "VALUE_MISSING"
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
			setState(183);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
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
			setState(187);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,1,_ctx) ) {
			case 1:
				{
				setState(186);
				fileMetadataSection();
				}
				break;
			}
			setState(198);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 8073353242946043904L) != 0)) {
				{
				{
				setState(189);
				topLevelDeclaration();
				setState(193);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
					{
					{
					setState(190);
					trivia();
					}
					}
					setState(195);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
				}
				setState(200);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(201);
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
			setState(203);
			metadataList();
			setState(207);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
				{
				{
				setState(204);
				trivia();
				}
				}
				setState(209);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(210);
			match(SECTION_DELIMITER);
			setState(214);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
				{
				{
				setState(211);
				trivia();
				}
				}
				setState(216);
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
			setState(232);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(217);
				metadataList();
				setState(221);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
					{
					{
					setState(218);
					trivia();
					}
					}
					setState(223);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(227);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_MIXIN:
				case KEYWORD_DERIVATION:
					{
					setState(224);
					mixinDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(225);
					funcDeclaration();
					}
					break;
				case KEYWORD_TYPE:
					{
					setState(226);
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
				setState(229);
				mixinDeclaration();
				}
				break;
			case KEYWORD_FUNC:
			case KEYWORD_PURE:
			case KEYWORD_INLINE:
			case KEYWORD_NOINLINE:
				enterOuterAlt(_localctx, 3);
				{
				setState(230);
				funcDeclaration();
				}
				break;
			case KEYWORD_TYPE:
				enterOuterAlt(_localctx, 4);
				{
				setState(231);
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
			setState(234);
			metadata();
			setState(244);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,10,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(238);
					_errHandler.sync(this);
					_la = _input.LA(1);
					while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
						{
						{
						setState(235);
						trivia();
						}
						}
						setState(240);
						_errHandler.sync(this);
						_la = _input.LA(1);
					}
					setState(241);
					metadata();
					}
					}
				}
				setState(246);
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
			setState(248);
			_errHandler.sync(this);
			_alt = 1;
			do {
				switch (_alt) {
				case 1:
					{
					{
					setState(247);
					metadata();
					}
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				setState(250);
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
			setState(258);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(252);
				match(METADATA_PREFIX);
				setState(253);
				match(IDENTIFIER);
				setState(255);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==BEGIN_ARGUMENT || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
					{
					setState(254);
					valueList();
					}
				}

				}
				break;
			case BEGIN_METADATA_VALUE:
				enterOuterAlt(_localctx, 2);
				{
				setState(257);
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
			setState(260);
			match(BEGIN_METADATA_VALUE);
			setState(261);
			value();
			setState(262);
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
			setState(267);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_DERIVATION) {
				{
				{
				setState(264);
				mixinModifier();
				}
				}
				setState(269);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(270);
			match(KEYWORD_MIXIN);
			setState(271);
			mixinIdentifier();
			setState(273);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(272);
				patternParameterList();
				}
			}

			setState(275);
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
			setState(277);
			match(KEYWORD_TYPE);
			setState(278);
			match(IDENTIFIER);
			setState(279);
			match(ASSIGN);
			setState(280);
			patternExpression();
			setState(282);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==SEMICOLON) {
				{
				setState(281);
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
			setState(284);
			match(LC);
			setState(290);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & -1151496056500912128L) != 0)) {
				{
				setState(288);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_EXPRESSION:
				case KEYWORD_PRELUDE:
				case KEYWORD_STRICT:
					{
					setState(285);
					expressionDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(286);
					funcDeclaration();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(287);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(292);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(293);
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
			setState(298);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_PRELUDE || _la==KEYWORD_STRICT) {
				{
				{
				setState(295);
				expressionModifier();
				}
				}
				setState(300);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(301);
			match(KEYWORD_EXPRESSION);
			setState(302);
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
			setState(307);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 8070450532247928832L) != 0)) {
				{
				{
				setState(304);
				funcModifier();
				}
				}
				setState(309);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(310);
			match(KEYWORD_FUNC);
			setState(311);
			functionDeclarationIdentifier();
			setState(313);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (((((_la - 9)) & ~0x3f) == 0 && ((1L << (_la - 9)) & 3746994889972252673L) != 0)) {
				{
				setState(312);
				directFunctionSignature();
				}
			}

			setState(318);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
				{
				{
				setState(315);
				trivia();
				}
				}
				setState(320);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(321);
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
		enterRule(_localctx, 24, RULE_directFunctionSignature);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(325);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case EMPTY_PARAMETERS:
			case BEGIN_PARAMETERS:
				{
				setState(323);
				patternParameterList();
				}
				break;
			case BEGIN_VALUE_INLINE:
				{
				setState(324);
				collectiveInputPattern();
				}
				break;
			case ARROW:
				break;
			default:
				break;
			}
			setState(327);
			match(ARROW);
			setState(330);
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
				setState(328);
				patternExpression();
				}
				break;
			case EMPTY_PARAMETERS:
			case BEGIN_PARAMETERS:
				{
				setState(329);
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
		enterRule(_localctx, 26, RULE_collectiveInputPattern);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(332);
			match(BEGIN_VALUE_INLINE);
			setState(333);
			patternExpression();
			setState(334);
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
		enterRule(_localctx, 28, RULE_outputSignatureFields);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(336);
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
		enterRule(_localctx, 30, RULE_functionBody);
		int _la;
		try {
			setState(347);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case LC:
			case KEYWORD_DO:
				enterOuterAlt(_localctx, 1);
				{
				setState(339);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_DO) {
					{
					setState(338);
					match(KEYWORD_DO);
					}
				}

				setState(341);
				statementBlock();
				}
				break;
			case FAT_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(342);
				match(FAT_ARROW);
				setState(343);
				value();
				setState(345);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_END) {
					{
					setState(344);
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
		enterRule(_localctx, 32, RULE_patternExpression);
		try {
			setState(354);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(349);
				inlineMetadataList();
				setState(351);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,28,_ctx) ) {
				case 1:
					{
					setState(350);
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
				setState(353);
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
		enterRule(_localctx, 34, RULE_patternUnion);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(356);
			patternTerm();
			setState(361);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,30,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(357);
					match(PIPE);
					setState(358);
					patternTerm();
					}
					}
				}
				setState(363);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,30,_ctx);
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
		enterRule(_localctx, 36, RULE_patternTerm);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(364);
			patternPrimary();
			setState(366);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,31,_ctx) ) {
			case 1:
				{
				setState(365);
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
		enterRule(_localctx, 38, RULE_patternPrimary);
		try {
			setState(372);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case NULL:
			case MISSING:
				enterOuterAlt(_localctx, 1);
				{
				setState(368);
				patternIdentifier();
				}
				break;
			case BEGIN_TABLE:
				enterOuterAlt(_localctx, 2);
				{
				setState(369);
				tablePattern();
				}
				break;
			case BEGIN_TUPLE:
				enterOuterAlt(_localctx, 3);
				{
				setState(370);
				tuplePattern();
				}
				break;
			case KEYWORD_DELEGATE:
				enterOuterAlt(_localctx, 4);
				{
				setState(371);
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
		enterRule(_localctx, 40, RULE_tablePattern);
		int _la;
		try {
			int _alt;
			setState(390);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,35,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(374);
				match(BEGIN_TABLE);
				setState(375);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(376);
				match(BEGIN_TABLE);
				setState(377);
				patternField();
				setState(382);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,33,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(378);
						match(VALUE_DELIMITER);
						setState(379);
						patternField();
						}
						}
					}
					setState(384);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,33,_ctx);
				}
				setState(386);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(385);
					match(VALUE_DELIMITER);
					}
				}

				setState(388);
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
		enterRule(_localctx, 42, RULE_tuplePattern);
		int _la;
		try {
			int _alt;
			setState(408);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,38,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(392);
				match(BEGIN_TUPLE);
				setState(393);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(394);
				match(BEGIN_TUPLE);
				setState(395);
				patternField();
				setState(400);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,36,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(396);
						match(VALUE_DELIMITER);
						setState(397);
						patternField();
						}
						}
					}
					setState(402);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,36,_ctx);
				}
				setState(404);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(403);
					match(VALUE_DELIMITER);
					}
				}

				setState(406);
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
		enterRule(_localctx, 44, RULE_delegatePattern);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(410);
			match(KEYWORD_DELEGATE);
			setState(411);
			patternParameterList();
			setState(412);
			match(ARROW);
			setState(413);
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
		enterRule(_localctx, 46, RULE_patternParameterList);
		int _la;
		try {
			int _alt;
			setState(432);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,41,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(415);
				match(BEGIN_PARAMETERS);
				setState(416);
				patternField();
				setState(421);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,39,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(417);
						match(VALUE_DELIMITER);
						setState(418);
						patternField();
						}
						}
					}
					setState(423);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,39,_ctx);
				}
				setState(425);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(424);
					match(VALUE_DELIMITER);
					}
				}

				setState(427);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(429);
				match(BEGIN_PARAMETERS);
				setState(430);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(431);
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
		enterRule(_localctx, 48, RULE_patternField);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(435);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				setState(434);
				metadataList();
				}
			}

			setState(437);
			patternUnion();
			setState(439);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ROOT_IDENTIFIER) {
				{
				setState(438);
				match(ROOT_IDENTIFIER);
				}
			}

			setState(443);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==VALUE_ASSIGN) {
				{
				setState(441);
				match(VALUE_ASSIGN);
				setState(442);
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
		enterRule(_localctx, 50, RULE_patternIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(445);
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
		enterRule(_localctx, 52, RULE_statementBlock);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(447);
			match(LC);
			setState(453);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1143920283947369472L) != 0) || ((((_la - 64)) & ~0x3f) == 0 && ((1L << (_la - 64)) & 2097171L) != 0)) {
				{
				setState(451);
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
					setState(448);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(449);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(450);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(455);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(456);
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
		enterRule(_localctx, 54, RULE_statement);
		int _la;
		try {
			setState(468);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(458);
				metadataList();
				setState(462);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
					{
					{
					setState(459);
					trivia();
					}
					}
					setState(464);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(465);
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
				setState(467);
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
		enterRule(_localctx, 56, RULE_statementBody);
		int _la;
		try {
			setState(486);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,50,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(470);
				labelIdentifier();
				setState(471);
				match(NEWLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(474);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LABEL_PREFIX) {
					{
					setState(473);
					labelIdentifier();
					}
				}

				setState(476);
				statementBlock();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(477);
				invocationStatement();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(478);
				localDeclarationStatement();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(479);
				localAssignmentStatement();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(480);
				toplevelDerivationStatement();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(481);
				controlflowStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(482);
				whenValueStatement();
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(483);
				whenConditionStatement();
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(484);
				whenChainStatement();
				}
				break;
			case 11:
				enterOuterAlt(_localctx, 11);
				{
				setState(485);
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
		enterRule(_localctx, 58, RULE_invocationStatement);
		try {
			setState(496);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,52,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(488);
				invocationIdentifier();
				setState(489);
				callValueList();
				setState(491);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,51,_ctx) ) {
				case 1:
					{
					setState(490);
					tailValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(493);
				invocationIdentifier();
				setState(494);
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
		enterRule(_localctx, 60, RULE_whenConditionStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(498);
			match(KEYWORD_WHEN);
			setState(499);
			whenChainCondition();
			setState(500);
			statementBlock();
			setState(511);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,54,_ctx) ) {
			case 1:
				{
				setState(504);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
					{
					{
					setState(501);
					trivia();
					}
					}
					setState(506);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(507);
				match(KEYWORD_ELSE);
				setState(508);
				whenResult();
				setState(509);
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
		enterRule(_localctx, 62, RULE_whenElseBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(513);
			match(KEYWORD_ELSE);
			setState(514);
			match(ARROW);
			setState(515);
			whenResult();
			setState(516);
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
		enterRule(_localctx, 64, RULE_whenResult);
		try {
			setState(520);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,55,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(518);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(519);
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
		enterRule(_localctx, 66, RULE_whenChainCondition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(522);
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
		enterRule(_localctx, 68, RULE_whenChainStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(524);
			match(KEYWORD_WHEN);
			setState(525);
			match(LC);
			setState(526);
			match(NEWLINE);
			setState(527);
			whenChainBody();
			setState(528);
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
		enterRule(_localctx, 70, RULE_whenChainBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(530);
			whenChainBranch();
			setState(535);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337168L) != 0) || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(533);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case EMPTY_PARAMETERS:
				case BEGIN_PARAMETERS:
					{
					setState(531);
					whenChainBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(532);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(537);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(539);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(538);
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
		enterRule(_localctx, 72, RULE_whenChainBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(541);
			whenChainCondition();
			setState(542);
			match(ARROW);
			setState(543);
			whenResult();
			setState(544);
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
		enterRule(_localctx, 74, RULE_whenValueStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(546);
			match(KEYWORD_WHEN);
			setState(548);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 2099503632L) != 0) || ((((_la - 68)) & ~0x3f) == 0 && ((1L << (_la - 68)) & 134610945L) != 0)) {
				{
				setState(547);
				value();
				}
			}

			setState(550);
			match(LC);
			setState(551);
			match(NEWLINE);
			setState(552);
			whenValueBody();
			setState(553);
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
		enterRule(_localctx, 76, RULE_whenValueBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(558);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 481036337152L) != 0)) {
				{
				{
				setState(555);
				trivia();
				}
				}
				setState(560);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(561);
			whenValueBranch();
			setState(566);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 483135840784L) != 0) || ((((_la - 68)) & ~0x3f) == 0 && ((1L << (_la - 68)) & 134610945L) != 0)) {
				{
				setState(564);
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
					setState(562);
					whenValueBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(563);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(568);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(570);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(569);
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
		enterRule(_localctx, 78, RULE_whenValueBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(572);
			whenValueCondition();
			setState(573);
			match(ARROW);
			setState(574);
			whenResult();
			setState(575);
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
		enterRule(_localctx, 80, RULE_whenValueCondition);
		try {
			setState(579);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,64,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(577);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(578);
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
		enterRule(_localctx, 82, RULE_localDeclarationStatement);
		int _la;
		try {
			setState(598);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,69,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(582);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(581);
					match(KEYWORD_CARRY);
					}
				}

				setState(584);
				match(KEYWORD_LOCAL);
				setState(585);
				variableIdentifier();
				setState(587);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,66,_ctx) ) {
				case 1:
					{
					setState(586);
					assignedValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(590);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(589);
					match(KEYWORD_CARRY);
					}
				}

				setState(592);
				match(KEYWORD_LOCAL);
				setState(593);
				patternExpression();
				setState(594);
				variableIdentifier();
				setState(596);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,68,_ctx) ) {
				case 1:
					{
					setState(595);
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
		enterRule(_localctx, 84, RULE_localAssignmentStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(600);
			match(VALUE_SMART_ROOT);
			setState(601);
			variableIdentifier();
			setState(602);
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
		enterRule(_localctx, 86, RULE_toplevelDerivationStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(604);
			match(TOPLEVEL_DERIVATION);
			setState(605);
			functionIdentifier();
			setState(607);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BEGIN_ARGUMENT || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(606);
				callValueList();
				}
			}

			setState(612);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 78)) & ~0x3f) == 0 && ((1L << (_la - 78)) & 15L) != 0)) {
				{
				{
				setState(609);
				transformationPart();
				}
				}
				setState(614);
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
		enterRule(_localctx, 88, RULE_assignedValue);
		try {
			setState(623);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ASSIGN:
				enterOuterAlt(_localctx, 1);
				{
				setState(615);
				match(ASSIGN);
				setState(620);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,72,_ctx) ) {
				case 1:
					{
					setState(616);
					value();
					}
					break;
				case 2:
					{
					setState(617);
					whenValueStatement();
					}
					break;
				case 3:
					{
					setState(618);
					whenChainStatement();
					}
					break;
				case 4:
					{
					setState(619);
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
				setState(622);
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
		enterRule(_localctx, 90, RULE_controlflowStatement);
		int _la;
		try {
			setState(633);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_RETURN:
				enterOuterAlt(_localctx, 1);
				{
				setState(625);
				match(KEYWORD_RETURN);
				setState(627);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==BEGIN_ARGUMENT || _la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
					{
					setState(626);
					valueList();
					}
				}

				}
				break;
			case KEYWORD_GOTO:
				enterOuterAlt(_localctx, 2);
				{
				setState(629);
				match(KEYWORD_GOTO);
				setState(630);
				match(IDENTIFIER);
				}
				break;
			case KEYWORD_CONTINUE:
				enterOuterAlt(_localctx, 3);
				{
				setState(631);
				match(KEYWORD_CONTINUE);
				}
				break;
			case KEYWORD_BREAK:
				enterOuterAlt(_localctx, 4);
				{
				setState(632);
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
		enterRule(_localctx, 92, RULE_contentBlock);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(635);
			match(BEGIN_CONTENT);
			setState(636);
			contentBody();
			setState(637);
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
		enterRule(_localctx, 94, RULE_contentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(643);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				setState(643);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case CONTENT_WRAP:
					{
					setState(639);
					match(CONTENT_WRAP);
					}
					break;
				case CONTENT_LINEBREAK:
					{
					setState(640);
					match(CONTENT_LINEBREAK);
					}
					break;
				case CONTENT_TEXT:
					{
					setState(641);
					match(CONTENT_TEXT);
					}
					break;
				case BEGIN_VALUE_INTERPOLATE:
					{
					setState(642);
					contentInterpolate();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(645);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( _la==BEGIN_VALUE_INTERPOLATE || ((((_la - 75)) & ~0x3f) == 0 && ((1L << (_la - 75)) & 7L) != 0) );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
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
		enterRule(_localctx, 96, RULE_contentInterpolate);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(647);
			match(BEGIN_VALUE_INTERPOLATE);
			setState(648);
			derivation();
			setState(649);
			match(RC);
			setState(651);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==END_CONTENT_INTERPOLATE) {
				{
				setState(650);
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
		enterRule(_localctx, 98, RULE_value);
		try {
			setState(655);
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
				setState(653);
				nonArgumentValue(0);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(654);
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
		enterRule(_localctx, 100, RULE_valueExpression);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(657);
			match(TOPLEVEL_VALUE_EXPRESSION);
			setState(658);
			value();
			setState(659);
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
		int _startState = 102;
		enterRecursionRule(_localctx, 102, RULE_nonArgumentValue, _p);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(675);
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
				setState(662);
				primaryValue();
				setState(666);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,80,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(663);
						transformationPart();
						}
						}
					}
					setState(668);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,80,_ctx);
				}
				setState(670);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,81,_ctx) ) {
				case 1:
					{
					setState(669);
					elvisValue();
					}
					break;
				}
				}
				break;
			case NOT_VALUE:
				{
				setState(672);
				prefixOperators();
				setState(673);
				nonArgumentValue(2);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(681);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,83,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					{
					_localctx = new NonArgumentValueContext(_parentctx, _parentState);
					pushNewRecursionContext(_localctx, _startState, RULE_nonArgumentValue);
					setState(677);
					if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
					setState(678);
					postfixOperators();
					}
					}
				}
				setState(683);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,83,_ctx);
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
		enterRule(_localctx, 104, RULE_primaryValue);
		try {
			setState(697);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,84,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(684);
				inlineValue();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(685);
				lambdaValue();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(686);
				inlineBlockValue();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(687);
				derivation();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(688);
				tableValue();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(689);
				tupleValue();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(690);
				valueExpression();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(691);
				valueStatement();
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(692);
				match(NUMBER);
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(693);
				match(STRING);
				}
				break;
			case 11:
				enterOuterAlt(_localctx, 11);
				{
				setState(694);
				match(BOOLEAN);
				}
				break;
			case 12:
				enterOuterAlt(_localctx, 12);
				{
				setState(695);
				match(NULL);
				}
				break;
			case 13:
				enterOuterAlt(_localctx, 13);
				{
				setState(696);
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
		enterRule(_localctx, 106, RULE_inlineBlockValue);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(699);
			match(BEGIN_INLINE_BLOCK);
			setState(705);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1143920283947369472L) != 0) || ((((_la - 64)) & ~0x3f) == 0 && ((1L << (_la - 64)) & 2097171L) != 0)) {
				{
				setState(703);
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
					setState(700);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(701);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(702);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(707);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(708);
			match(RC);
			setState(709);
			match(FAT_ARROW);
			setState(710);
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
		enterRule(_localctx, 108, RULE_lambdaValue);
		int _la;
		try {
			setState(727);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_LAMBDA_BLOCK:
				enterOuterAlt(_localctx, 1);
				{
				setState(712);
				match(BEGIN_LAMBDA_BLOCK);
				setState(718);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1143920283947369472L) != 0) || ((((_la - 64)) & ~0x3f) == 0 && ((1L << (_la - 64)) & 2097171L) != 0)) {
					{
					setState(716);
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
						setState(713);
						statement();
						}
						break;
					case SEMICOLON:
						{
						setState(714);
						match(SEMICOLON);
						}
						break;
					case NEWLINE:
					case COMMENT:
					case SLASH_COMMENT:
						{
						setState(715);
						trivia();
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					}
					setState(720);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(721);
				match(RC);
				}
				break;
			case BEGIN_LAMBDA_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(722);
				match(BEGIN_LAMBDA_ARROW);
				setState(723);
				value();
				setState(725);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,89,_ctx) ) {
				case 1:
					{
					setState(724);
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
		enterRule(_localctx, 110, RULE_prefixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(729);
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
		enterRule(_localctx, 112, RULE_postfixOperators);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(731);
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
		enterRule(_localctx, 114, RULE_valueStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(733);
			functionIdentifier();
			setState(734);
			callValueList();
			setState(736);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,91,_ctx) ) {
			case 1:
				{
				setState(735);
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
		enterRule(_localctx, 116, RULE_callBinding);
		try {
			setState(741);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case STATIC_CALL:
				enterOuterAlt(_localctx, 1);
				{
				setState(738);
				match(STATIC_CALL);
				setState(739);
				valueList();
				}
				break;
			case DYNAMIC_CALL:
				enterOuterAlt(_localctx, 2);
				{
				setState(740);
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
		enterRule(_localctx, 118, RULE_tailValue);
		int _la;
		try {
			setState(748);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_CONTENT:
			case NEWLINE:
				enterOuterAlt(_localctx, 1);
				{
				setState(744);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==NEWLINE) {
					{
					setState(743);
					match(NEWLINE);
					}
				}

				setState(746);
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
				setState(747);
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
		enterRule(_localctx, 120, RULE_tupleValue);
		int _la;
		try {
			int _alt;
			setState(766);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,97,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(750);
				match(BEGIN_TUPLE);
				setState(751);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(752);
				match(BEGIN_TUPLE);
				setState(753);
				value();
				setState(758);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,95,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(754);
						match(VALUE_DELIMITER);
						setState(755);
						value();
						}
						}
					}
					setState(760);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,95,_ctx);
				}
				setState(762);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(761);
					match(VALUE_DELIMITER);
					}
				}

				setState(764);
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
		enterRule(_localctx, 122, RULE_tableValue);
		int _la;
		try {
			int _alt;
			setState(784);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,100,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(768);
				match(BEGIN_TABLE);
				setState(769);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(770);
				match(BEGIN_TABLE);
				setState(771);
				tableKeyedEntry();
				setState(776);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,98,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(772);
						match(VALUE_DELIMITER);
						setState(773);
						tableKeyedEntry();
						}
						}
					}
					setState(778);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,98,_ctx);
				}
				setState(780);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(779);
					match(VALUE_DELIMITER);
					}
				}

				setState(782);
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
		enterRule(_localctx, 124, RULE_tableKeyedEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(789);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				{
				setState(786);
				metadata();
				}
				}
				setState(791);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(792);
			_la = _input.LA(1);
			if ( !(_la==ROOT_IDENTIFIER || _la==MISSING) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			setState(793);
			match(VALUE_ASSIGN);
			setState(794);
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
		enterRule(_localctx, 126, RULE_valueList);
		int _la;
		try {
			int _alt;
			setState(818);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,105,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(796);
				match(BEGIN_PARAMETERS);
				setState(797);
				value();
				setState(802);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,102,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(798);
						match(VALUE_DELIMITER);
						setState(799);
						value();
						}
						}
					}
					setState(804);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,102,_ctx);
				}
				setState(806);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(805);
					match(VALUE_DELIMITER);
					}
				}

				setState(808);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(810);
				match(BEGIN_PARAMETERS);
				setState(811);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(812);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(814);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(813);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(816);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,104,_ctx);
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
		enterRule(_localctx, 128, RULE_callValueList);
		int _la;
		try {
			int _alt;
			setState(842);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,109,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(820);
				match(BEGIN_PARAMETERS);
				setState(821);
				callArgument();
				setState(826);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,106,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(822);
						match(VALUE_DELIMITER);
						setState(823);
						callArgument();
						}
						}
					}
					setState(828);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,106,_ctx);
				}
				setState(830);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(829);
					match(VALUE_DELIMITER);
					}
				}

				setState(832);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(834);
				match(BEGIN_PARAMETERS);
				setState(835);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(836);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(838);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(837);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(840);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,108,_ctx);
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
		enterRule(_localctx, 130, RULE_callArgument);
		int _la;
		try {
			setState(848);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,110,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(844);
				_la = _input.LA(1);
				if ( !(_la==ROOT_IDENTIFIER || _la==MISSING) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(845);
				match(VALUE_ASSIGN);
				setState(846);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(847);
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
		enterRule(_localctx, 132, RULE_argumentValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(850);
			match(BEGIN_ARGUMENT);
			setState(851);
			argumentBody();
			setState(852);
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
		enterRule(_localctx, 134, RULE_argumentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(859);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==ESCAPE || _la==BEGIN_VALUE_INLINE || _la==ARGUMENT_TEXT) {
				{
				setState(857);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case ARGUMENT_TEXT:
					{
					setState(854);
					match(ARGUMENT_TEXT);
					}
					break;
				case ESCAPE:
					{
					setState(855);
					escaped();
					}
					break;
				case BEGIN_VALUE_INLINE:
					{
					setState(856);
					inlineValue();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(861);
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
		enterRule(_localctx, 136, RULE_inlineValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(862);
			match(BEGIN_VALUE_INLINE);
			setState(863);
			value();
			setState(864);
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
		enterRule(_localctx, 138, RULE_inlineTransformation);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(866);
			match(BEGIN_VALUE_INLINE);
			setState(868);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(867);
				transformationPart();
				}
				}
				setState(870);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 78)) & ~0x3f) == 0 && ((1L << (_la - 78)) & 15L) != 0) );
			setState(872);
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
		enterRule(_localctx, 140, RULE_derivation);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(874);
			derivationRoot();
			setState(878);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,114,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(875);
					transformationPart();
					}
					}
				}
				setState(880);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,114,_ctx);
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
		enterRule(_localctx, 142, RULE_explicitRoot);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(881);
			match(EXPLICIT_ROOT);
			setState(882);
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
		enterRule(_localctx, 144, RULE_derivationRoot);
		int _la;
		try {
			setState(888);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case EXPLICIT_ROOT:
				enterOuterAlt(_localctx, 1);
				{
				setState(884);
				explicitRoot();
				}
				break;
			case ROOT_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(885);
				match(ROOT_IDENTIFIER);
				}
				break;
			case VALUE_SMART_ROOT:
				enterOuterAlt(_localctx, 3);
				{
				setState(886);
				match(VALUE_SMART_ROOT);
				setState(887);
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
		enterRule(_localctx, 146, RULE_elvisValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(890);
			match(VALUE_ELVIS);
			setState(891);
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
		enterRule(_localctx, 148, RULE_transformationPart);
		try {
			setState(903);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case VALUE_FUNCTION:
			case VALUE_PREDICATE:
				enterOuterAlt(_localctx, 1);
				{
				setState(893);
				functionChainType();
				setState(894);
				functionIdentifier();
				setState(896);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,116,_ctx) ) {
				case 1:
					{
					setState(895);
					callValueList();
					}
					break;
				}
				setState(899);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,117,_ctx) ) {
				case 1:
					{
					setState(898);
					callBinding();
					}
					break;
				}
				}
				break;
			case VALUE_MEMBER:
				enterOuterAlt(_localctx, 2);
				{
				setState(901);
				memberIdentifier();
				}
				break;
			case VALUE_WRAP:
				enterOuterAlt(_localctx, 3);
				{
				setState(902);
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
		enterRule(_localctx, 150, RULE_functionChainType);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(905);
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
		enterRule(_localctx, 152, RULE_labelIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(907);
			match(LABEL_PREFIX);
			setState(908);
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
		enterRule(_localctx, 154, RULE_memberIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(910);
			match(VALUE_MEMBER);
			setState(911);
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
		enterRule(_localctx, 156, RULE_mixinIdentifier);
		try {
			setState(916);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(913);
				match(IDENTIFIER);
				}
				break;
			case NAMESPACE_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(914);
				match(NAMESPACE_IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 3);
				{
				setState(915);
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
		enterRule(_localctx, 158, RULE_functionDeclarationIdentifier);
		try {
			setState(920);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(918);
				match(IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(919);
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
		enterRule(_localctx, 160, RULE_variableIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(922);
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
		enterRule(_localctx, 162, RULE_functionIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(924);
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
		enterRule(_localctx, 164, RULE_invocationIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(926);
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
		enterRule(_localctx, 166, RULE_kindIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(928);
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
		enterRule(_localctx, 168, RULE_expressionModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(930);
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
		enterRule(_localctx, 170, RULE_mixinModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(932);
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
		enterRule(_localctx, 172, RULE_funcModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(934);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 8070450532247928832L) != 0)) ) {
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
		enterRule(_localctx, 174, RULE_trivia);
		try {
			setState(938);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case COMMENT:
			case SLASH_COMMENT:
				enterOuterAlt(_localctx, 1);
				{
				setState(936);
				comment();
				}
				break;
			case NEWLINE:
				enterOuterAlt(_localctx, 2);
				{
				setState(937);
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
		enterRule(_localctx, 176, RULE_comment);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(940);
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
		enterRule(_localctx, 178, RULE_escaped);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(942);
			match(ESCAPE);
			setState(943);
			_la = _input.LA(1);
			if ( !(((((_la - 91)) & ~0x3f) == 0 && ((1L << (_la - 91)) & 7L) != 0)) ) {
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
		case 51:
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
		"\u0004\u0001k\u03b2\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
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
		"U\u0007U\u0002V\u0007V\u0002W\u0007W\u0002X\u0007X\u0002Y\u0007Y\u0001"+
		"\u0000\u0005\u0000\u00b6\b\u0000\n\u0000\f\u0000\u00b9\t\u0000\u0001\u0000"+
		"\u0003\u0000\u00bc\b\u0000\u0001\u0000\u0001\u0000\u0005\u0000\u00c0\b"+
		"\u0000\n\u0000\f\u0000\u00c3\t\u0000\u0005\u0000\u00c5\b\u0000\n\u0000"+
		"\f\u0000\u00c8\t\u0000\u0001\u0000\u0001\u0000\u0001\u0001\u0001\u0001"+
		"\u0005\u0001\u00ce\b\u0001\n\u0001\f\u0001\u00d1\t\u0001\u0001\u0001\u0001"+
		"\u0001\u0005\u0001\u00d5\b\u0001\n\u0001\f\u0001\u00d8\t\u0001\u0001\u0002"+
		"\u0001\u0002\u0005\u0002\u00dc\b\u0002\n\u0002\f\u0002\u00df\t\u0002\u0001"+
		"\u0002\u0001\u0002\u0001\u0002\u0003\u0002\u00e4\b\u0002\u0001\u0002\u0001"+
		"\u0002\u0001\u0002\u0003\u0002\u00e9\b\u0002\u0001\u0003\u0001\u0003\u0005"+
		"\u0003\u00ed\b\u0003\n\u0003\f\u0003\u00f0\t\u0003\u0001\u0003\u0005\u0003"+
		"\u00f3\b\u0003\n\u0003\f\u0003\u00f6\t\u0003\u0001\u0004\u0004\u0004\u00f9"+
		"\b\u0004\u000b\u0004\f\u0004\u00fa\u0001\u0005\u0001\u0005\u0001\u0005"+
		"\u0003\u0005\u0100\b\u0005\u0001\u0005\u0003\u0005\u0103\b\u0005\u0001"+
		"\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0007\u0005\u0007\u010a"+
		"\b\u0007\n\u0007\f\u0007\u010d\t\u0007\u0001\u0007\u0001\u0007\u0001\u0007"+
		"\u0003\u0007\u0112\b\u0007\u0001\u0007\u0001\u0007\u0001\b\u0001\b\u0001"+
		"\b\u0001\b\u0001\b\u0003\b\u011b\b\b\u0001\t\u0001\t\u0001\t\u0001\t\u0005"+
		"\t\u0121\b\t\n\t\f\t\u0124\t\t\u0001\t\u0001\t\u0001\n\u0005\n\u0129\b"+
		"\n\n\n\f\n\u012c\t\n\u0001\n\u0001\n\u0001\n\u0001\u000b\u0005\u000b\u0132"+
		"\b\u000b\n\u000b\f\u000b\u0135\t\u000b\u0001\u000b\u0001\u000b\u0001\u000b"+
		"\u0003\u000b\u013a\b\u000b\u0001\u000b\u0005\u000b\u013d\b\u000b\n\u000b"+
		"\f\u000b\u0140\t\u000b\u0001\u000b\u0001\u000b\u0001\f\u0001\f\u0003\f"+
		"\u0146\b\f\u0001\f\u0001\f\u0001\f\u0003\f\u014b\b\f\u0001\r\u0001\r\u0001"+
		"\r\u0001\r\u0001\u000e\u0001\u000e\u0001\u000f\u0003\u000f\u0154\b\u000f"+
		"\u0001\u000f\u0001\u000f\u0001\u000f\u0001\u000f\u0003\u000f\u015a\b\u000f"+
		"\u0003\u000f\u015c\b\u000f\u0001\u0010\u0001\u0010\u0003\u0010\u0160\b"+
		"\u0010\u0001\u0010\u0003\u0010\u0163\b\u0010\u0001\u0011\u0001\u0011\u0001"+
		"\u0011\u0005\u0011\u0168\b\u0011\n\u0011\f\u0011\u016b\t\u0011\u0001\u0012"+
		"\u0001\u0012\u0003\u0012\u016f\b\u0012\u0001\u0013\u0001\u0013\u0001\u0013"+
		"\u0001\u0013\u0003\u0013\u0175\b\u0013\u0001\u0014\u0001\u0014\u0001\u0014"+
		"\u0001\u0014\u0001\u0014\u0001\u0014\u0005\u0014\u017d\b\u0014\n\u0014"+
		"\f\u0014\u0180\t\u0014\u0001\u0014\u0003\u0014\u0183\b\u0014\u0001\u0014"+
		"\u0001\u0014\u0003\u0014\u0187\b\u0014\u0001\u0015\u0001\u0015\u0001\u0015"+
		"\u0001\u0015\u0001\u0015\u0001\u0015\u0005\u0015\u018f\b\u0015\n\u0015"+
		"\f\u0015\u0192\t\u0015\u0001\u0015\u0003\u0015\u0195\b\u0015\u0001\u0015"+
		"\u0001\u0015\u0003\u0015\u0199\b\u0015\u0001\u0016\u0001\u0016\u0001\u0016"+
		"\u0001\u0016\u0001\u0016\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017"+
		"\u0005\u0017\u01a4\b\u0017\n\u0017\f\u0017\u01a7\t\u0017\u0001\u0017\u0003"+
		"\u0017\u01aa\b\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001"+
		"\u0017\u0003\u0017\u01b1\b\u0017\u0001\u0018\u0003\u0018\u01b4\b\u0018"+
		"\u0001\u0018\u0001\u0018\u0003\u0018\u01b8\b\u0018\u0001\u0018\u0001\u0018"+
		"\u0003\u0018\u01bc\b\u0018\u0001\u0019\u0001\u0019\u0001\u001a\u0001\u001a"+
		"\u0001\u001a\u0001\u001a\u0005\u001a\u01c4\b\u001a\n\u001a\f\u001a\u01c7"+
		"\t\u001a\u0001\u001a\u0001\u001a\u0001\u001b\u0001\u001b\u0005\u001b\u01cd"+
		"\b\u001b\n\u001b\f\u001b\u01d0\t\u001b\u0001\u001b\u0001\u001b\u0001\u001b"+
		"\u0003\u001b\u01d5\b\u001b\u0001\u001c\u0001\u001c\u0001\u001c\u0001\u001c"+
		"\u0003\u001c\u01db\b\u001c\u0001\u001c\u0001\u001c\u0001\u001c\u0001\u001c"+
		"\u0001\u001c\u0001\u001c\u0001\u001c\u0001\u001c\u0001\u001c\u0001\u001c"+
		"\u0003\u001c\u01e7\b\u001c\u0001\u001d\u0001\u001d\u0001\u001d\u0003\u001d"+
		"\u01ec\b\u001d\u0001\u001d\u0001\u001d\u0001\u001d\u0003\u001d\u01f1\b"+
		"\u001d\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0005\u001e\u01f7"+
		"\b\u001e\n\u001e\f\u001e\u01fa\t\u001e\u0001\u001e\u0001\u001e\u0001\u001e"+
		"\u0001\u001e\u0003\u001e\u0200\b\u001e\u0001\u001f\u0001\u001f\u0001\u001f"+
		"\u0001\u001f\u0001\u001f\u0001 \u0001 \u0003 \u0209\b \u0001!\u0001!\u0001"+
		"\"\u0001\"\u0001\"\u0001\"\u0001\"\u0001\"\u0001#\u0001#\u0001#\u0005"+
		"#\u0216\b#\n#\f#\u0219\t#\u0001#\u0003#\u021c\b#\u0001$\u0001$\u0001$"+
		"\u0001$\u0001$\u0001%\u0001%\u0003%\u0225\b%\u0001%\u0001%\u0001%\u0001"+
		"%\u0001%\u0001&\u0005&\u022d\b&\n&\f&\u0230\t&\u0001&\u0001&\u0001&\u0005"+
		"&\u0235\b&\n&\f&\u0238\t&\u0001&\u0003&\u023b\b&\u0001\'\u0001\'\u0001"+
		"\'\u0001\'\u0001\'\u0001(\u0001(\u0003(\u0244\b(\u0001)\u0003)\u0247\b"+
		")\u0001)\u0001)\u0001)\u0003)\u024c\b)\u0001)\u0003)\u024f\b)\u0001)\u0001"+
		")\u0001)\u0001)\u0003)\u0255\b)\u0003)\u0257\b)\u0001*\u0001*\u0001*\u0001"+
		"*\u0001+\u0001+\u0001+\u0003+\u0260\b+\u0001+\u0005+\u0263\b+\n+\f+\u0266"+
		"\t+\u0001,\u0001,\u0001,\u0001,\u0001,\u0003,\u026d\b,\u0001,\u0003,\u0270"+
		"\b,\u0001-\u0001-\u0003-\u0274\b-\u0001-\u0001-\u0001-\u0001-\u0003-\u027a"+
		"\b-\u0001.\u0001.\u0001.\u0001.\u0001/\u0001/\u0001/\u0001/\u0004/\u0284"+
		"\b/\u000b/\f/\u0285\u00010\u00010\u00010\u00010\u00030\u028c\b0\u0001"+
		"1\u00011\u00031\u0290\b1\u00012\u00012\u00012\u00012\u00013\u00013\u0001"+
		"3\u00053\u0299\b3\n3\f3\u029c\t3\u00013\u00033\u029f\b3\u00013\u00013"+
		"\u00013\u00033\u02a4\b3\u00013\u00013\u00053\u02a8\b3\n3\f3\u02ab\t3\u0001"+
		"4\u00014\u00014\u00014\u00014\u00014\u00014\u00014\u00014\u00014\u0001"+
		"4\u00014\u00014\u00034\u02ba\b4\u00015\u00015\u00015\u00015\u00055\u02c0"+
		"\b5\n5\f5\u02c3\t5\u00015\u00015\u00015\u00015\u00016\u00016\u00016\u0001"+
		"6\u00056\u02cd\b6\n6\f6\u02d0\t6\u00016\u00016\u00016\u00016\u00036\u02d6"+
		"\b6\u00036\u02d8\b6\u00017\u00017\u00018\u00018\u00019\u00019\u00019\u0003"+
		"9\u02e1\b9\u0001:\u0001:\u0001:\u0003:\u02e6\b:\u0001;\u0003;\u02e9\b"+
		";\u0001;\u0001;\u0003;\u02ed\b;\u0001<\u0001<\u0001<\u0001<\u0001<\u0001"+
		"<\u0005<\u02f5\b<\n<\f<\u02f8\t<\u0001<\u0003<\u02fb\b<\u0001<\u0001<"+
		"\u0003<\u02ff\b<\u0001=\u0001=\u0001=\u0001=\u0001=\u0001=\u0005=\u0307"+
		"\b=\n=\f=\u030a\t=\u0001=\u0003=\u030d\b=\u0001=\u0001=\u0003=\u0311\b"+
		"=\u0001>\u0005>\u0314\b>\n>\f>\u0317\t>\u0001>\u0001>\u0001>\u0001>\u0001"+
		"?\u0001?\u0001?\u0001?\u0005?\u0321\b?\n?\f?\u0324\t?\u0001?\u0003?\u0327"+
		"\b?\u0001?\u0001?\u0001?\u0001?\u0001?\u0001?\u0004?\u032f\b?\u000b?\f"+
		"?\u0330\u0003?\u0333\b?\u0001@\u0001@\u0001@\u0001@\u0005@\u0339\b@\n"+
		"@\f@\u033c\t@\u0001@\u0003@\u033f\b@\u0001@\u0001@\u0001@\u0001@\u0001"+
		"@\u0001@\u0004@\u0347\b@\u000b@\f@\u0348\u0003@\u034b\b@\u0001A\u0001"+
		"A\u0001A\u0001A\u0003A\u0351\bA\u0001B\u0001B\u0001B\u0001B\u0001C\u0001"+
		"C\u0001C\u0005C\u035a\bC\nC\fC\u035d\tC\u0001D\u0001D\u0001D\u0001D\u0001"+
		"E\u0001E\u0004E\u0365\bE\u000bE\fE\u0366\u0001E\u0001E\u0001F\u0001F\u0005"+
		"F\u036d\bF\nF\fF\u0370\tF\u0001G\u0001G\u0001G\u0001H\u0001H\u0001H\u0001"+
		"H\u0003H\u0379\bH\u0001I\u0001I\u0001I\u0001J\u0001J\u0001J\u0003J\u0381"+
		"\bJ\u0001J\u0003J\u0384\bJ\u0001J\u0001J\u0003J\u0388\bJ\u0001K\u0001"+
		"K\u0001L\u0001L\u0001L\u0001M\u0001M\u0001M\u0001N\u0001N\u0001N\u0003"+
		"N\u0395\bN\u0001O\u0001O\u0003O\u0399\bO\u0001P\u0001P\u0001Q\u0001Q\u0001"+
		"R\u0001R\u0001S\u0001S\u0001T\u0001T\u0001U\u0001U\u0001V\u0001V\u0001"+
		"W\u0001W\u0003W\u03ab\bW\u0001X\u0001X\u0001Y\u0001Y\u0001Y\u0001Y\u0000"+
		"\u0001fZ\u0000\u0002\u0004\u0006\b\n\f\u000e\u0010\u0012\u0014\u0016\u0018"+
		"\u001a\u001c\u001e \"$&(*,.02468:<>@BDFHJLNPRTVXZ\\^`bdfhjlnprtvxz|~\u0080"+
		"\u0082\u0084\u0086\u0088\u008a\u008c\u008e\u0090\u0092\u0094\u0096\u0098"+
		"\u009a\u009c\u009e\u00a0\u00a2\u00a4\u00a6\u00a8\u00aa\u00ac\u00ae\u00b0"+
		"\u00b2\u0000\u000b\u0003\u0000\n\n\r\r\u001d\u001e\u0002\u0000\u0014\u0014"+
		"\u001f\u001f\u0002\u0000\r\r\u001e\u001e\u0003\u0000\r\r\u001a\u001a\u001e"+
		"\u001e\u0001\u0000NO\u0002\u0000\r\r__\u0002\u0000\n\n99\u0002\u00002"+
		"2??\u0001\u0000<>\u0001\u0000%&\u0001\u0000[]\u03fe\u0000\u00b7\u0001"+
		"\u0000\u0000\u0000\u0002\u00cb\u0001\u0000\u0000\u0000\u0004\u00e8\u0001"+
		"\u0000\u0000\u0000\u0006\u00ea\u0001\u0000\u0000\u0000\b\u00f8\u0001\u0000"+
		"\u0000\u0000\n\u0102\u0001\u0000\u0000\u0000\f\u0104\u0001\u0000\u0000"+
		"\u0000\u000e\u010b\u0001\u0000\u0000\u0000\u0010\u0115\u0001\u0000\u0000"+
		"\u0000\u0012\u011c\u0001\u0000\u0000\u0000\u0014\u012a\u0001\u0000\u0000"+
		"\u0000\u0016\u0133\u0001\u0000\u0000\u0000\u0018\u0145\u0001\u0000\u0000"+
		"\u0000\u001a\u014c\u0001\u0000\u0000\u0000\u001c\u0150\u0001\u0000\u0000"+
		"\u0000\u001e\u015b\u0001\u0000\u0000\u0000 \u0162\u0001\u0000\u0000\u0000"+
		"\"\u0164\u0001\u0000\u0000\u0000$\u016c\u0001\u0000\u0000\u0000&\u0174"+
		"\u0001\u0000\u0000\u0000(\u0186\u0001\u0000\u0000\u0000*\u0198\u0001\u0000"+
		"\u0000\u0000,\u019a\u0001\u0000\u0000\u0000.\u01b0\u0001\u0000\u0000\u0000"+
		"0\u01b3\u0001\u0000\u0000\u00002\u01bd\u0001\u0000\u0000\u00004\u01bf"+
		"\u0001\u0000\u0000\u00006\u01d4\u0001\u0000\u0000\u00008\u01e6\u0001\u0000"+
		"\u0000\u0000:\u01f0\u0001\u0000\u0000\u0000<\u01f2\u0001\u0000\u0000\u0000"+
		">\u0201\u0001\u0000\u0000\u0000@\u0208\u0001\u0000\u0000\u0000B\u020a"+
		"\u0001\u0000\u0000\u0000D\u020c\u0001\u0000\u0000\u0000F\u0212\u0001\u0000"+
		"\u0000\u0000H\u021d\u0001\u0000\u0000\u0000J\u0222\u0001\u0000\u0000\u0000"+
		"L\u022e\u0001\u0000\u0000\u0000N\u023c\u0001\u0000\u0000\u0000P\u0243"+
		"\u0001\u0000\u0000\u0000R\u0256\u0001\u0000\u0000\u0000T\u0258\u0001\u0000"+
		"\u0000\u0000V\u025c\u0001\u0000\u0000\u0000X\u026f\u0001\u0000\u0000\u0000"+
		"Z\u0279\u0001\u0000\u0000\u0000\\\u027b\u0001\u0000\u0000\u0000^\u0283"+
		"\u0001\u0000\u0000\u0000`\u0287\u0001\u0000\u0000\u0000b\u028f\u0001\u0000"+
		"\u0000\u0000d\u0291\u0001\u0000\u0000\u0000f\u02a3\u0001\u0000\u0000\u0000"+
		"h\u02b9\u0001\u0000\u0000\u0000j\u02bb\u0001\u0000\u0000\u0000l\u02d7"+
		"\u0001\u0000\u0000\u0000n\u02d9\u0001\u0000\u0000\u0000p\u02db\u0001\u0000"+
		"\u0000\u0000r\u02dd\u0001\u0000\u0000\u0000t\u02e5\u0001\u0000\u0000\u0000"+
		"v\u02ec\u0001\u0000\u0000\u0000x\u02fe\u0001\u0000\u0000\u0000z\u0310"+
		"\u0001\u0000\u0000\u0000|\u0315\u0001\u0000\u0000\u0000~\u0332\u0001\u0000"+
		"\u0000\u0000\u0080\u034a\u0001\u0000\u0000\u0000\u0082\u0350\u0001\u0000"+
		"\u0000\u0000\u0084\u0352\u0001\u0000\u0000\u0000\u0086\u035b\u0001\u0000"+
		"\u0000\u0000\u0088\u035e\u0001\u0000\u0000\u0000\u008a\u0362\u0001\u0000"+
		"\u0000\u0000\u008c\u036a\u0001\u0000\u0000\u0000\u008e\u0371\u0001\u0000"+
		"\u0000\u0000\u0090\u0378\u0001\u0000\u0000\u0000\u0092\u037a\u0001\u0000"+
		"\u0000\u0000\u0094\u0387\u0001\u0000\u0000\u0000\u0096\u0389\u0001\u0000"+
		"\u0000\u0000\u0098\u038b\u0001\u0000\u0000\u0000\u009a\u038e\u0001\u0000"+
		"\u0000\u0000\u009c\u0394\u0001\u0000\u0000\u0000\u009e\u0398\u0001\u0000"+
		"\u0000\u0000\u00a0\u039a\u0001\u0000\u0000\u0000\u00a2\u039c\u0001\u0000"+
		"\u0000\u0000\u00a4\u039e\u0001\u0000\u0000\u0000\u00a6\u03a0\u0001\u0000"+
		"\u0000\u0000\u00a8\u03a2\u0001\u0000\u0000\u0000\u00aa\u03a4\u0001\u0000"+
		"\u0000\u0000\u00ac\u03a6\u0001\u0000\u0000\u0000\u00ae\u03aa\u0001\u0000"+
		"\u0000\u0000\u00b0\u03ac\u0001\u0000\u0000\u0000\u00b2\u03ae\u0001\u0000"+
		"\u0000\u0000\u00b4\u00b6\u0003\u00aeW\u0000\u00b5\u00b4\u0001\u0000\u0000"+
		"\u0000\u00b6\u00b9\u0001\u0000\u0000\u0000\u00b7\u00b5\u0001\u0000\u0000"+
		"\u0000\u00b7\u00b8\u0001\u0000\u0000\u0000\u00b8\u00bb\u0001\u0000\u0000"+
		"\u0000\u00b9\u00b7\u0001\u0000\u0000\u0000\u00ba\u00bc\u0003\u0002\u0001"+
		"\u0000\u00bb\u00ba\u0001\u0000\u0000\u0000\u00bb\u00bc\u0001\u0000\u0000"+
		"\u0000\u00bc\u00c6\u0001\u0000\u0000\u0000\u00bd\u00c1\u0003\u0004\u0002"+
		"\u0000\u00be\u00c0\u0003\u00aeW\u0000\u00bf\u00be\u0001\u0000\u0000\u0000"+
		"\u00c0\u00c3\u0001\u0000\u0000\u0000\u00c1\u00bf\u0001\u0000\u0000\u0000"+
		"\u00c1\u00c2\u0001\u0000\u0000\u0000\u00c2\u00c5\u0001\u0000\u0000\u0000"+
		"\u00c3\u00c1\u0001\u0000\u0000\u0000\u00c4\u00bd\u0001\u0000\u0000\u0000"+
		"\u00c5\u00c8\u0001\u0000\u0000\u0000\u00c6\u00c4\u0001\u0000\u0000\u0000"+
		"\u00c6\u00c7\u0001\u0000\u0000\u0000\u00c7\u00c9\u0001\u0000\u0000\u0000"+
		"\u00c8\u00c6\u0001\u0000\u0000\u0000\u00c9\u00ca\u0005\u0000\u0000\u0001"+
		"\u00ca\u0001\u0001\u0000\u0000\u0000\u00cb\u00cf\u0003\u0006\u0003\u0000"+
		"\u00cc\u00ce\u0003\u00aeW\u0000\u00cd\u00cc\u0001\u0000\u0000\u0000\u00ce"+
		"\u00d1\u0001\u0000\u0000\u0000\u00cf\u00cd\u0001\u0000\u0000\u0000\u00cf"+
		"\u00d0\u0001\u0000\u0000\u0000\u00d0\u00d2\u0001\u0000\u0000\u0000\u00d1"+
		"\u00cf\u0001\u0000\u0000\u0000\u00d2\u00d6\u0005\'\u0000\u0000\u00d3\u00d5"+
		"\u0003\u00aeW\u0000\u00d4\u00d3\u0001\u0000\u0000\u0000\u00d5\u00d8\u0001"+
		"\u0000\u0000\u0000\u00d6\u00d4\u0001\u0000\u0000\u0000\u00d6\u00d7\u0001"+
		"\u0000\u0000\u0000\u00d7\u0003\u0001\u0000\u0000\u0000\u00d8\u00d6\u0001"+
		"\u0000\u0000\u0000\u00d9\u00dd\u0003\u0006\u0003\u0000\u00da\u00dc\u0003"+
		"\u00aeW\u0000\u00db\u00da\u0001\u0000\u0000\u0000\u00dc\u00df\u0001\u0000"+
		"\u0000\u0000\u00dd\u00db\u0001\u0000\u0000\u0000\u00dd\u00de\u0001\u0000"+
		"\u0000\u0000\u00de\u00e3\u0001\u0000\u0000\u0000\u00df\u00dd\u0001\u0000"+
		"\u0000\u0000\u00e0\u00e4\u0003\u000e\u0007\u0000\u00e1\u00e4\u0003\u0016"+
		"\u000b\u0000\u00e2\u00e4\u0003\u0010\b\u0000\u00e3\u00e0\u0001\u0000\u0000"+
		"\u0000\u00e3\u00e1\u0001\u0000\u0000\u0000\u00e3\u00e2\u0001\u0000\u0000"+
		"\u0000\u00e4\u00e9\u0001\u0000\u0000\u0000\u00e5\u00e9\u0003\u000e\u0007"+
		"\u0000\u00e6\u00e9\u0003\u0016\u000b\u0000\u00e7\u00e9\u0003\u0010\b\u0000"+
		"\u00e8\u00d9\u0001\u0000\u0000\u0000\u00e8\u00e5\u0001\u0000\u0000\u0000"+
		"\u00e8\u00e6\u0001\u0000\u0000\u0000\u00e8\u00e7\u0001\u0000\u0000\u0000"+
		"\u00e9\u0005\u0001\u0000\u0000\u0000\u00ea\u00f4\u0003\n\u0005\u0000\u00eb"+
		"\u00ed\u0003\u00aeW\u0000\u00ec\u00eb\u0001\u0000\u0000\u0000\u00ed\u00f0"+
		"\u0001\u0000\u0000\u0000\u00ee\u00ec\u0001\u0000\u0000\u0000\u00ee\u00ef"+
		"\u0001\u0000\u0000\u0000\u00ef\u00f1\u0001\u0000\u0000\u0000\u00f0\u00ee"+
		"\u0001\u0000\u0000\u0000\u00f1\u00f3\u0003\n\u0005\u0000\u00f2\u00ee\u0001"+
		"\u0000\u0000\u0000\u00f3\u00f6\u0001\u0000\u0000\u0000\u00f4\u00f2\u0001"+
		"\u0000\u0000\u0000\u00f4\u00f5\u0001\u0000\u0000\u0000\u00f5\u0007\u0001"+
		"\u0000\u0000\u0000\u00f6\u00f4\u0001\u0000\u0000\u0000\u00f7\u00f9\u0003"+
		"\n\u0005\u0000\u00f8\u00f7\u0001\u0000\u0000\u0000\u00f9\u00fa\u0001\u0000"+
		"\u0000\u0000\u00fa\u00f8\u0001\u0000\u0000\u0000\u00fa\u00fb\u0001\u0000"+
		"\u0000\u0000\u00fb\t\u0001\u0000\u0000\u0000\u00fc\u00fd\u0005\u0013\u0000"+
		"\u0000\u00fd\u00ff\u0005\n\u0000\u0000\u00fe\u0100\u0003~?\u0000\u00ff"+
		"\u00fe\u0001\u0000\u0000\u0000\u00ff\u0100\u0001\u0000\u0000\u0000\u0100"+
		"\u0103\u0001\u0000\u0000\u0000\u0101\u0103\u0003\f\u0006\u0000\u0102\u00fc"+
		"\u0001\u0000\u0000\u0000\u0102\u0101\u0001\u0000\u0000\u0000\u0103\u000b"+
		"\u0001\u0000\u0000\u0000\u0104\u0105\u0005\u0012\u0000\u0000\u0105\u0106"+
		"\u0003b1\u0000\u0106\u0107\u0005T\u0000\u0000\u0107\r\u0001\u0000\u0000"+
		"\u0000\u0108\u010a\u0003\u00aaU\u0000\u0109\u0108\u0001\u0000\u0000\u0000"+
		"\u010a\u010d\u0001\u0000\u0000\u0000\u010b\u0109\u0001\u0000\u0000\u0000"+
		"\u010b\u010c\u0001\u0000\u0000\u0000\u010c\u010e\u0001\u0000\u0000\u0000"+
		"\u010d\u010b\u0001\u0000\u0000\u0000\u010e\u010f\u00051\u0000\u0000\u010f"+
		"\u0111\u0003\u009cN\u0000\u0110\u0112\u0003.\u0017\u0000\u0111\u0110\u0001"+
		"\u0000\u0000\u0000\u0111\u0112\u0001\u0000\u0000\u0000\u0112\u0113\u0001"+
		"\u0000\u0000\u0000\u0113\u0114\u0003\u0012\t\u0000\u0114\u000f\u0001\u0000"+
		"\u0000\u0000\u0115\u0116\u0005.\u0000\u0000\u0116\u0117\u0005\n\u0000"+
		"\u0000\u0117\u0118\u0005B\u0000\u0000\u0118\u011a\u0003 \u0010\u0000\u0119"+
		"\u011b\u0005*\u0000\u0000\u011a\u0119\u0001\u0000\u0000\u0000\u011a\u011b"+
		"\u0001\u0000\u0000\u0000\u011b\u0011\u0001\u0000\u0000\u0000\u011c\u0122"+
		"\u0005(\u0000\u0000\u011d\u0121\u0003\u0014\n\u0000\u011e\u0121\u0003"+
		"\u0016\u000b\u0000\u011f\u0121\u0003\u00aeW\u0000\u0120\u011d\u0001\u0000"+
		"\u0000\u0000\u0120\u011e\u0001\u0000\u0000\u0000\u0120\u011f\u0001\u0000"+
		"\u0000\u0000\u0121\u0124\u0001\u0000\u0000\u0000\u0122\u0120\u0001\u0000"+
		"\u0000\u0000\u0122\u0123\u0001\u0000\u0000\u0000\u0123\u0125\u0001\u0000"+
		"\u0000\u0000\u0124\u0122\u0001\u0000\u0000\u0000\u0125\u0126\u0005)\u0000"+
		"\u0000\u0126\u0013\u0001\u0000\u0000\u0000\u0127\u0129\u0003\u00a8T\u0000"+
		"\u0128\u0127\u0001\u0000\u0000\u0000\u0129\u012c\u0001\u0000\u0000\u0000"+
		"\u012a\u0128\u0001\u0000\u0000\u0000\u012a\u012b\u0001\u0000\u0000\u0000"+
		"\u012b\u012d\u0001\u0000\u0000\u0000\u012c\u012a\u0001\u0000\u0000\u0000"+
		"\u012d\u012e\u00050\u0000\u0000\u012e\u012f\u00034\u001a\u0000\u012f\u0015"+
		"\u0001\u0000\u0000\u0000\u0130\u0132\u0003\u00acV\u0000\u0131\u0130\u0001"+
		"\u0000\u0000\u0000\u0132\u0135\u0001\u0000\u0000\u0000\u0133\u0131\u0001"+
		"\u0000\u0000\u0000\u0133\u0134\u0001\u0000\u0000\u0000\u0134\u0136\u0001"+
		"\u0000\u0000\u0000\u0135\u0133\u0001\u0000\u0000\u0000\u0136\u0137\u0005"+
		",\u0000\u0000\u0137\u0139\u0003\u009eO\u0000\u0138\u013a\u0003\u0018\f"+
		"\u0000\u0139\u0138\u0001\u0000\u0000\u0000\u0139\u013a\u0001\u0000\u0000"+
		"\u0000\u013a\u013e\u0001\u0000\u0000\u0000\u013b\u013d\u0003\u00aeW\u0000"+
		"\u013c\u013b\u0001\u0000\u0000\u0000\u013d\u0140\u0001\u0000\u0000\u0000"+
		"\u013e\u013c\u0001\u0000\u0000\u0000\u013e\u013f\u0001\u0000\u0000\u0000"+
		"\u013f\u0141\u0001\u0000\u0000\u0000\u0140\u013e\u0001\u0000\u0000\u0000"+
		"\u0141\u0142\u0003\u001e\u000f\u0000\u0142\u0017\u0001\u0000\u0000\u0000"+
		"\u0143\u0146\u0003.\u0017\u0000\u0144\u0146\u0003\u001a\r\u0000\u0145"+
		"\u0143\u0001\u0000\u0000\u0000\u0145\u0144\u0001\u0000\u0000\u0000\u0145"+
		"\u0146\u0001\u0000\u0000\u0000\u0146\u0147\u0001\u0000\u0000\u0000\u0147"+
		"\u014a\u0005C\u0000\u0000\u0148\u014b\u0003 \u0010\u0000\u0149\u014b\u0003"+
		"\u001c\u000e\u0000\u014a\u0148\u0001\u0000\u0000\u0000\u014a\u0149\u0001"+
		"\u0000\u0000\u0000\u014b\u0019\u0001\u0000\u0000\u0000\u014c\u014d\u0005"+
		"\t\u0000\u0000\u014d\u014e\u0003 \u0010\u0000\u014e\u014f\u0005T\u0000"+
		"\u0000\u014f\u001b\u0001\u0000\u0000\u0000\u0150\u0151\u0003.\u0017\u0000"+
		"\u0151\u001d\u0001\u0000\u0000\u0000\u0152\u0154\u0005/\u0000\u0000\u0153"+
		"\u0152\u0001\u0000\u0000\u0000\u0153\u0154\u0001\u0000\u0000\u0000\u0154"+
		"\u0155\u0001\u0000\u0000\u0000\u0155\u015c\u00034\u001a\u0000\u0156\u0157"+
		"\u0005\u0019\u0000\u0000\u0157\u0159\u0003b1\u0000\u0158\u015a\u0005\f"+
		"\u0000\u0000\u0159\u0158\u0001\u0000\u0000\u0000\u0159\u015a\u0001\u0000"+
		"\u0000\u0000\u015a\u015c\u0001\u0000\u0000\u0000\u015b\u0153\u0001\u0000"+
		"\u0000\u0000\u015b\u0156\u0001\u0000\u0000\u0000\u015c\u001f\u0001\u0000"+
		"\u0000\u0000\u015d\u015f\u0003\b\u0004\u0000\u015e\u0160\u0003\"\u0011"+
		"\u0000\u015f\u015e\u0001\u0000\u0000\u0000\u015f\u0160\u0001\u0000\u0000"+
		"\u0000\u0160\u0163\u0001\u0000\u0000\u0000\u0161\u0163\u0003\"\u0011\u0000"+
		"\u0162\u015d\u0001\u0000\u0000\u0000\u0162\u0161\u0001\u0000\u0000\u0000"+
		"\u0163!\u0001\u0000\u0000\u0000\u0164\u0169\u0003$\u0012\u0000\u0165\u0166"+
		"\u0005 \u0000\u0000\u0166\u0168\u0003$\u0012\u0000\u0167\u0165\u0001\u0000"+
		"\u0000\u0000\u0168\u016b\u0001\u0000\u0000\u0000\u0169\u0167\u0001\u0000"+
		"\u0000\u0000\u0169\u016a\u0001\u0000\u0000\u0000\u016a#\u0001\u0000\u0000"+
		"\u0000\u016b\u0169\u0001\u0000\u0000\u0000\u016c\u016e\u0003&\u0013\u0000"+
		"\u016d\u016f\u0005\u001f\u0000\u0000\u016e\u016d\u0001\u0000\u0000\u0000"+
		"\u016e\u016f\u0001\u0000\u0000\u0000\u016f%\u0001\u0000\u0000\u0000\u0170"+
		"\u0175\u00032\u0019\u0000\u0171\u0175\u0003(\u0014\u0000\u0172\u0175\u0003"+
		"*\u0015\u0000\u0173\u0175\u0003,\u0016\u0000\u0174\u0170\u0001\u0000\u0000"+
		"\u0000\u0174\u0171\u0001\u0000\u0000\u0000\u0174\u0172\u0001\u0000\u0000"+
		"\u0000\u0174\u0173\u0001\u0000\u0000\u0000\u0175\'\u0001\u0000\u0000\u0000"+
		"\u0176\u0177\u0005\u000e\u0000\u0000\u0177\u0187\u0005)\u0000\u0000\u0178"+
		"\u0179\u0005\u000e\u0000\u0000\u0179\u017e\u00030\u0018\u0000\u017a\u017b"+
		"\u0005R\u0000\u0000\u017b\u017d\u00030\u0018\u0000\u017c\u017a\u0001\u0000"+
		"\u0000\u0000\u017d\u0180\u0001\u0000\u0000\u0000\u017e\u017c\u0001\u0000"+
		"\u0000\u0000\u017e\u017f\u0001\u0000\u0000\u0000\u017f\u0182\u0001\u0000"+
		"\u0000\u0000\u0180\u017e\u0001\u0000\u0000\u0000\u0181\u0183\u0005R\u0000"+
		"\u0000\u0182\u0181\u0001\u0000\u0000\u0000\u0182\u0183\u0001\u0000\u0000"+
		"\u0000\u0183\u0184\u0001\u0000\u0000\u0000\u0184\u0185\u0005)\u0000\u0000"+
		"\u0185\u0187\u0001\u0000\u0000\u0000\u0186\u0176\u0001\u0000\u0000\u0000"+
		"\u0186\u0178\u0001\u0000\u0000\u0000\u0187)\u0001\u0000\u0000\u0000\u0188"+
		"\u0189\u0005\u000f\u0000\u0000\u0189\u0199\u0005T\u0000\u0000\u018a\u018b"+
		"\u0005\u000f\u0000\u0000\u018b\u0190\u00030\u0018\u0000\u018c\u018d\u0005"+
		"R\u0000\u0000\u018d\u018f\u00030\u0018\u0000\u018e\u018c\u0001\u0000\u0000"+
		"\u0000\u018f\u0192\u0001\u0000\u0000\u0000\u0190\u018e\u0001\u0000\u0000"+
		"\u0000\u0190\u0191\u0001\u0000\u0000\u0000\u0191\u0194\u0001\u0000\u0000"+
		"\u0000\u0192\u0190\u0001\u0000\u0000\u0000\u0193\u0195\u0005R\u0000\u0000"+
		"\u0194\u0193\u0001\u0000\u0000\u0000\u0194\u0195\u0001\u0000\u0000\u0000"+
		"\u0195\u0196\u0001\u0000\u0000\u0000\u0196\u0197\u0005T\u0000\u0000\u0197"+
		"\u0199\u0001\u0000\u0000\u0000\u0198\u0188\u0001\u0000\u0000\u0000\u0198"+
		"\u018a\u0001\u0000\u0000\u0000\u0199+\u0001\u0000\u0000\u0000\u019a\u019b"+
		"\u0005-\u0000\u0000\u019b\u019c\u0003.\u0017\u0000\u019c\u019d\u0005C"+
		"\u0000\u0000\u019d\u019e\u0003 \u0010\u0000\u019e-\u0001\u0000\u0000\u0000"+
		"\u019f\u01a0\u0005F\u0000\u0000\u01a0\u01a5\u00030\u0018\u0000\u01a1\u01a2"+
		"\u0005R\u0000\u0000\u01a2\u01a4\u00030\u0018\u0000\u01a3\u01a1\u0001\u0000"+
		"\u0000\u0000\u01a4\u01a7\u0001\u0000\u0000\u0000\u01a5\u01a3\u0001\u0000"+
		"\u0000\u0000\u01a5\u01a6\u0001\u0000\u0000\u0000\u01a6\u01a9\u0001\u0000"+
		"\u0000\u0000\u01a7\u01a5\u0001\u0000\u0000\u0000\u01a8\u01aa\u0005R\u0000"+
		"\u0000\u01a9\u01a8\u0001\u0000\u0000\u0000\u01a9\u01aa\u0001\u0000\u0000"+
		"\u0000\u01aa\u01ab\u0001\u0000\u0000\u0000\u01ab\u01ac\u0005S\u0000\u0000"+
		"\u01ac\u01b1\u0001\u0000\u0000\u0000\u01ad\u01ae\u0005F\u0000\u0000\u01ae"+
		"\u01b1\u0005S\u0000\u0000\u01af\u01b1\u0005E\u0000\u0000\u01b0\u019f\u0001"+
		"\u0000\u0000\u0000\u01b0\u01ad\u0001\u0000\u0000\u0000\u01b0\u01af\u0001"+
		"\u0000\u0000\u0000\u01b1/\u0001\u0000\u0000\u0000\u01b2\u01b4\u0003\u0006"+
		"\u0003\u0000\u01b3\u01b2\u0001\u0000\u0000\u0000\u01b3\u01b4\u0001\u0000"+
		"\u0000\u0000\u01b4\u01b5\u0001\u0000\u0000\u0000\u01b5\u01b7\u0003\"\u0011"+
		"\u0000\u01b6\u01b8\u0005\r\u0000\u0000\u01b7\u01b6\u0001\u0000\u0000\u0000"+
		"\u01b7\u01b8\u0001\u0000\u0000\u0000\u01b8\u01bb\u0001\u0000\u0000\u0000"+
		"\u01b9\u01ba\u0005X\u0000\u0000\u01ba\u01bc\u0003b1\u0000\u01bb\u01b9"+
		"\u0001\u0000\u0000\u0000\u01bb\u01bc\u0001\u0000\u0000\u0000\u01bc1\u0001"+
		"\u0000\u0000\u0000\u01bd\u01be\u0007\u0000\u0000\u0000\u01be3\u0001\u0000"+
		"\u0000\u0000\u01bf\u01c5\u0005(\u0000\u0000\u01c0\u01c4\u00036\u001b\u0000"+
		"\u01c1\u01c4\u0005*\u0000\u0000\u01c2\u01c4\u0003\u00aeW\u0000\u01c3\u01c0"+
		"\u0001\u0000\u0000\u0000\u01c3\u01c1\u0001\u0000\u0000\u0000\u01c3\u01c2"+
		"\u0001\u0000\u0000\u0000\u01c4\u01c7\u0001\u0000\u0000\u0000\u01c5\u01c3"+
		"\u0001\u0000\u0000\u0000\u01c5\u01c6\u0001\u0000\u0000\u0000\u01c6\u01c8"+
		"\u0001\u0000\u0000\u0000\u01c7\u01c5\u0001\u0000\u0000\u0000\u01c8\u01c9"+
		"\u0005)\u0000\u0000\u01c95\u0001\u0000\u0000\u0000\u01ca\u01ce\u0003\u0006"+
		"\u0003\u0000\u01cb\u01cd\u0003\u00aeW\u0000\u01cc\u01cb\u0001\u0000\u0000"+
		"\u0000\u01cd\u01d0\u0001\u0000\u0000\u0000\u01ce\u01cc\u0001\u0000\u0000"+
		"\u0000\u01ce\u01cf\u0001\u0000\u0000\u0000\u01cf\u01d1\u0001\u0000\u0000"+
		"\u0000\u01d0\u01ce\u0001\u0000\u0000\u0000\u01d1\u01d2\u00038\u001c\u0000"+
		"\u01d2\u01d5\u0001\u0000\u0000\u0000\u01d3\u01d5\u00038\u001c\u0000\u01d4"+
		"\u01ca\u0001\u0000\u0000\u0000\u01d4\u01d3\u0001\u0000\u0000\u0000\u01d5"+
		"7\u0001\u0000\u0000\u0000\u01d6\u01d7\u0003\u0098L\u0000\u01d7\u01d8\u0005"+
		"$\u0000\u0000\u01d8\u01e7\u0001\u0000\u0000\u0000\u01d9\u01db\u0003\u0098"+
		"L\u0000\u01da\u01d9\u0001\u0000\u0000\u0000\u01da\u01db\u0001\u0000\u0000"+
		"\u0000\u01db\u01dc\u0001\u0000\u0000\u0000\u01dc\u01e7\u00034\u001a\u0000"+
		"\u01dd\u01e7\u0003:\u001d\u0000\u01de\u01e7\u0003R)\u0000\u01df\u01e7"+
		"\u0003T*\u0000\u01e0\u01e7\u0003V+\u0000\u01e1\u01e7\u0003Z-\u0000\u01e2"+
		"\u01e7\u0003J%\u0000\u01e3\u01e7\u0003<\u001e\u0000\u01e4\u01e7\u0003"+
		"D\"\u0000\u01e5\u01e7\u0003d2\u0000\u01e6\u01d6\u0001\u0000\u0000\u0000"+
		"\u01e6\u01da\u0001\u0000\u0000\u0000\u01e6\u01dd\u0001\u0000\u0000\u0000"+
		"\u01e6\u01de\u0001\u0000\u0000\u0000\u01e6\u01df\u0001\u0000\u0000\u0000"+
		"\u01e6\u01e0\u0001\u0000\u0000\u0000\u01e6\u01e1\u0001\u0000\u0000\u0000"+
		"\u01e6\u01e2\u0001\u0000\u0000\u0000\u01e6\u01e3\u0001\u0000\u0000\u0000"+
		"\u01e6\u01e4\u0001\u0000\u0000\u0000\u01e6\u01e5\u0001\u0000\u0000\u0000"+
		"\u01e79\u0001\u0000\u0000\u0000\u01e8\u01e9\u0003\u00a4R\u0000\u01e9\u01eb"+
		"\u0003\u0080@\u0000\u01ea\u01ec\u0003v;\u0000\u01eb\u01ea\u0001\u0000"+
		"\u0000\u0000\u01eb\u01ec\u0001\u0000\u0000\u0000\u01ec\u01f1\u0001\u0000"+
		"\u0000\u0000\u01ed\u01ee\u0003\u00a4R\u0000\u01ee\u01ef\u0003v;\u0000"+
		"\u01ef\u01f1\u0001\u0000\u0000\u0000\u01f0\u01e8\u0001\u0000\u0000\u0000"+
		"\u01f0\u01ed\u0001\u0000\u0000\u0000\u01f1;\u0001\u0000\u0000\u0000\u01f2"+
		"\u01f3\u0005;\u0000\u0000\u01f3\u01f4\u0003B!\u0000\u01f4\u01ff\u0003"+
		"4\u001a\u0000\u01f5\u01f7\u0003\u00aeW\u0000\u01f6\u01f5\u0001\u0000\u0000"+
		"\u0000\u01f7\u01fa\u0001\u0000\u0000\u0000\u01f8\u01f6\u0001\u0000\u0000"+
		"\u0000\u01f8\u01f9\u0001\u0000\u0000\u0000\u01f9\u01fb\u0001\u0000\u0000"+
		"\u0000\u01fa\u01f8\u0001\u0000\u0000\u0000\u01fb\u01fc\u00054\u0000\u0000"+
		"\u01fc\u01fd\u0003@ \u0000\u01fd\u01fe\u0005$\u0000\u0000\u01fe\u0200"+
		"\u0001\u0000\u0000\u0000\u01ff\u01f8\u0001\u0000\u0000\u0000\u01ff\u0200"+
		"\u0001\u0000\u0000\u0000\u0200=\u0001\u0000\u0000\u0000\u0201\u0202\u0005"+
		"4\u0000\u0000\u0202\u0203\u0005C\u0000\u0000\u0203\u0204\u0003@ \u0000"+
		"\u0204\u0205\u0005$\u0000\u0000\u0205?\u0001\u0000\u0000\u0000\u0206\u0209"+
		"\u0003b1\u0000\u0207\u0209\u00036\u001b\u0000\u0208\u0206\u0001\u0000"+
		"\u0000\u0000\u0208\u0207\u0001\u0000\u0000\u0000\u0209A\u0001\u0000\u0000"+
		"\u0000\u020a\u020b\u0003~?\u0000\u020bC\u0001\u0000\u0000\u0000\u020c"+
		"\u020d\u0005;\u0000\u0000\u020d\u020e\u0005(\u0000\u0000\u020e\u020f\u0005"+
		"$\u0000\u0000\u020f\u0210\u0003F#\u0000\u0210\u0211\u0005)\u0000\u0000"+
		"\u0211E\u0001\u0000\u0000\u0000\u0212\u0217\u0003H$\u0000\u0213\u0216"+
		"\u0003H$\u0000\u0214\u0216\u0003\u00aeW\u0000\u0215\u0213\u0001\u0000"+
		"\u0000\u0000\u0215\u0214\u0001\u0000\u0000\u0000\u0216\u0219\u0001\u0000"+
		"\u0000\u0000\u0217\u0215\u0001\u0000\u0000\u0000\u0217\u0218\u0001\u0000"+
		"\u0000\u0000\u0218\u021b\u0001\u0000\u0000\u0000\u0219\u0217\u0001\u0000"+
		"\u0000\u0000\u021a\u021c\u0003>\u001f\u0000\u021b\u021a\u0001\u0000\u0000"+
		"\u0000\u021b\u021c\u0001\u0000\u0000\u0000\u021cG\u0001\u0000\u0000\u0000"+
		"\u021d\u021e\u0003B!\u0000\u021e\u021f\u0005C\u0000\u0000\u021f\u0220"+
		"\u0003@ \u0000\u0220\u0221\u0005$\u0000\u0000\u0221I\u0001\u0000\u0000"+
		"\u0000\u0222\u0224\u0005;\u0000\u0000\u0223\u0225\u0003b1\u0000\u0224"+
		"\u0223\u0001\u0000\u0000\u0000\u0224\u0225\u0001\u0000\u0000\u0000\u0225"+
		"\u0226\u0001\u0000\u0000\u0000\u0226\u0227\u0005(\u0000\u0000\u0227\u0228"+
		"\u0005$\u0000\u0000\u0228\u0229\u0003L&\u0000\u0229\u022a\u0005)\u0000"+
		"\u0000\u022aK\u0001\u0000\u0000\u0000\u022b\u022d\u0003\u00aeW\u0000\u022c"+
		"\u022b\u0001\u0000\u0000\u0000\u022d\u0230\u0001\u0000\u0000\u0000\u022e"+
		"\u022c\u0001\u0000\u0000\u0000\u022e\u022f\u0001\u0000\u0000\u0000\u022f"+
		"\u0231\u0001\u0000\u0000\u0000\u0230\u022e\u0001\u0000\u0000\u0000\u0231"+
		"\u0236\u0003N\'\u0000\u0232\u0235\u0003N\'\u0000\u0233\u0235\u0003\u00ae"+
		"W\u0000\u0234\u0232\u0001\u0000\u0000\u0000\u0234\u0233\u0001\u0000\u0000"+
		"\u0000\u0235\u0238\u0001\u0000\u0000\u0000\u0236\u0234\u0001\u0000\u0000"+
		"\u0000\u0236\u0237\u0001\u0000\u0000\u0000\u0237\u023a\u0001\u0000\u0000"+
		"\u0000\u0238\u0236\u0001\u0000\u0000\u0000\u0239\u023b\u0003>\u001f\u0000"+
		"\u023a\u0239\u0001\u0000\u0000\u0000\u023a\u023b\u0001\u0000\u0000\u0000"+
		"\u023bM\u0001\u0000\u0000\u0000\u023c\u023d\u0003P(\u0000\u023d\u023e"+
		"\u0005C\u0000\u0000\u023e\u023f\u0003@ \u0000\u023f\u0240\u0005$\u0000"+
		"\u0000\u0240O\u0001\u0000\u0000\u0000\u0241\u0244\u0003b1\u0000\u0242"+
		"\u0244\u0003\u008aE\u0000\u0243\u0241\u0001\u0000\u0000\u0000\u0243\u0242"+
		"\u0001\u0000\u0000\u0000\u0244Q\u0001\u0000\u0000\u0000\u0245\u0247\u0005"+
		":\u0000\u0000\u0246\u0245\u0001\u0000\u0000\u0000\u0246\u0247\u0001\u0000"+
		"\u0000\u0000\u0247\u0248\u0001\u0000\u0000\u0000\u0248\u0249\u00059\u0000"+
		"\u0000\u0249\u024b\u0003\u00a0P\u0000\u024a\u024c\u0003X,\u0000\u024b"+
		"\u024a\u0001\u0000\u0000\u0000\u024b\u024c\u0001\u0000\u0000\u0000\u024c"+
		"\u0257\u0001\u0000\u0000\u0000\u024d\u024f\u0005:\u0000\u0000\u024e\u024d"+
		"\u0001\u0000\u0000\u0000\u024e\u024f\u0001\u0000\u0000\u0000\u024f\u0250"+
		"\u0001\u0000\u0000\u0000\u0250\u0251\u00059\u0000\u0000\u0251\u0252\u0003"+
		" \u0010\u0000\u0252\u0254\u0003\u00a0P\u0000\u0253\u0255\u0003X,\u0000"+
		"\u0254\u0253\u0001\u0000\u0000\u0000\u0254\u0255\u0001\u0000\u0000\u0000"+
		"\u0255\u0257\u0001\u0000\u0000\u0000\u0256\u0246\u0001\u0000\u0000\u0000"+
		"\u0256\u024e\u0001\u0000\u0000\u0000\u0257S\u0001\u0000\u0000\u0000\u0258"+
		"\u0259\u0005U\u0000\u0000\u0259\u025a\u0003\u00a0P\u0000\u025a\u025b\u0003"+
		"X,\u0000\u025bU\u0001\u0000\u0000\u0000\u025c\u025d\u0005A\u0000\u0000"+
		"\u025d\u025f\u0003\u00a2Q\u0000\u025e\u0260\u0003\u0080@\u0000\u025f\u025e"+
		"\u0001\u0000\u0000\u0000\u025f\u0260\u0001\u0000\u0000\u0000\u0260\u0264"+
		"\u0001\u0000\u0000\u0000\u0261\u0263\u0003\u0094J\u0000\u0262\u0261\u0001"+
		"\u0000\u0000\u0000\u0263\u0266\u0001\u0000\u0000\u0000\u0264\u0262\u0001"+
		"\u0000\u0000\u0000\u0264\u0265\u0001\u0000\u0000\u0000\u0265W\u0001\u0000"+
		"\u0000\u0000\u0266\u0264\u0001\u0000\u0000\u0000\u0267\u026c\u0005B\u0000"+
		"\u0000\u0268\u026d\u0003b1\u0000\u0269\u026d\u0003J%\u0000\u026a\u026d"+
		"\u0003D\"\u0000\u026b\u026d\u0003:\u001d\u0000\u026c\u0268\u0001\u0000"+
		"\u0000\u0000\u026c\u0269\u0001\u0000\u0000\u0000\u026c\u026a\u0001\u0000"+
		"\u0000\u0000\u026c\u026b\u0001\u0000\u0000\u0000\u026d\u0270\u0001\u0000"+
		"\u0000\u0000\u026e\u0270\u0003v;\u0000\u026f\u0267\u0001\u0000\u0000\u0000"+
		"\u026f\u026e\u0001\u0000\u0000\u0000\u0270Y\u0001\u0000\u0000\u0000\u0271"+
		"\u0273\u00055\u0000\u0000\u0272\u0274\u0003~?\u0000\u0273\u0272\u0001"+
		"\u0000\u0000\u0000\u0273\u0274\u0001\u0000\u0000\u0000\u0274\u027a\u0001"+
		"\u0000\u0000\u0000\u0275\u0276\u00056\u0000\u0000\u0276\u027a\u0005\n"+
		"\u0000\u0000\u0277\u027a\u00058\u0000\u0000\u0278\u027a\u00057\u0000\u0000"+
		"\u0279\u0271\u0001\u0000\u0000\u0000\u0279\u0275\u0001\u0000\u0000\u0000"+
		"\u0279\u0277\u0001\u0000\u0000\u0000\u0279\u0278\u0001\u0000\u0000\u0000"+
		"\u027a[\u0001\u0000\u0000\u0000\u027b\u027c\u0005\u0007\u0000\u0000\u027c"+
		"\u027d\u0003^/\u0000\u027d\u027e\u0005\u0002\u0000\u0000\u027e]\u0001"+
		"\u0000\u0000\u0000\u027f\u0284\u0005K\u0000\u0000\u0280\u0284\u0005L\u0000"+
		"\u0000\u0281\u0284\u0005M\u0000\u0000\u0282\u0284\u0003`0\u0000\u0283"+
		"\u027f\u0001\u0000\u0000\u0000\u0283\u0280\u0001\u0000\u0000\u0000\u0283"+
		"\u0281\u0001\u0000\u0000\u0000\u0283\u0282\u0001\u0000\u0000\u0000\u0284"+
		"\u0285\u0001\u0000\u0000\u0000\u0285\u0283\u0001\u0000\u0000\u0000\u0285"+
		"\u0286\u0001\u0000\u0000\u0000\u0286_\u0001\u0000\u0000\u0000\u0287\u0288"+
		"\u0005\b\u0000\u0000\u0288\u0289\u0003\u008cF\u0000\u0289\u028b\u0005"+
		")\u0000\u0000\u028a\u028c\u0005^\u0000\u0000\u028b\u028a\u0001\u0000\u0000"+
		"\u0000\u028b\u028c\u0001\u0000\u0000\u0000\u028ca\u0001\u0000\u0000\u0000"+
		"\u028d\u0290\u0003f3\u0000\u028e\u0290\u0003\u0084B\u0000\u028f\u028d"+
		"\u0001\u0000\u0000\u0000\u028f\u028e\u0001\u0000\u0000\u0000\u0290c\u0001"+
		"\u0000\u0000\u0000\u0291\u0292\u0005D\u0000\u0000\u0292\u0293\u0003b1"+
		"\u0000\u0293\u0294\u0005\f\u0000\u0000\u0294e\u0001\u0000\u0000\u0000"+
		"\u0295\u0296\u00063\uffff\uffff\u0000\u0296\u029a\u0003h4\u0000\u0297"+
		"\u0299\u0003\u0094J\u0000\u0298\u0297\u0001\u0000\u0000\u0000\u0299\u029c"+
		"\u0001\u0000\u0000\u0000\u029a\u0298\u0001\u0000\u0000\u0000\u029a\u029b"+
		"\u0001\u0000\u0000\u0000\u029b\u029e\u0001\u0000\u0000\u0000\u029c\u029a"+
		"\u0001\u0000\u0000\u0000\u029d\u029f\u0003\u0092I\u0000\u029e\u029d\u0001"+
		"\u0000\u0000\u0000\u029e\u029f\u0001\u0000\u0000\u0000\u029f\u02a4\u0001"+
		"\u0000\u0000\u0000\u02a0\u02a1\u0003n7\u0000\u02a1\u02a2\u0003f3\u0002"+
		"\u02a2\u02a4\u0001\u0000\u0000\u0000\u02a3\u0295\u0001\u0000\u0000\u0000"+
		"\u02a3\u02a0\u0001\u0000\u0000\u0000\u02a4\u02a9\u0001\u0000\u0000\u0000"+
		"\u02a5\u02a6\n\u0001\u0000\u0000\u02a6\u02a8\u0003p8\u0000\u02a7\u02a5"+
		"\u0001\u0000\u0000\u0000\u02a8\u02ab\u0001\u0000\u0000\u0000\u02a9\u02a7"+
		"\u0001\u0000\u0000\u0000\u02a9\u02aa\u0001\u0000\u0000\u0000\u02aag\u0001"+
		"\u0000\u0000\u0000\u02ab\u02a9\u0001\u0000\u0000\u0000\u02ac\u02ba\u0003"+
		"\u0088D\u0000\u02ad\u02ba\u0003l6\u0000\u02ae\u02ba\u0003j5\u0000\u02af"+
		"\u02ba\u0003\u008cF\u0000\u02b0\u02ba\u0003z=\u0000\u02b1\u02ba\u0003"+
		"x<\u0000\u02b2\u02ba\u0003d2\u0000\u02b3\u02ba\u0003r9\u0000\u02b4\u02ba"+
		"\u0005\u001a\u0000\u0000\u02b5\u02ba\u0005\u001b\u0000\u0000\u02b6\u02ba"+
		"\u0005\u001c\u0000\u0000\u02b7\u02ba\u0005\u001d\u0000\u0000\u02b8\u02ba"+
		"\u0005\u001e\u0000\u0000\u02b9\u02ac\u0001\u0000\u0000\u0000\u02b9\u02ad"+
		"\u0001\u0000\u0000\u0000\u02b9\u02ae\u0001\u0000\u0000\u0000\u02b9\u02af"+
		"\u0001\u0000\u0000\u0000\u02b9\u02b0\u0001\u0000\u0000\u0000\u02b9\u02b1"+
		"\u0001\u0000\u0000\u0000\u02b9\u02b2\u0001\u0000\u0000\u0000\u02b9\u02b3"+
		"\u0001\u0000\u0000\u0000\u02b9\u02b4\u0001\u0000\u0000\u0000\u02b9\u02b5"+
		"\u0001\u0000\u0000\u0000\u02b9\u02b6\u0001\u0000\u0000\u0000\u02b9\u02b7"+
		"\u0001\u0000\u0000\u0000\u02b9\u02b8\u0001\u0000\u0000\u0000\u02bai\u0001"+
		"\u0000\u0000\u0000\u02bb\u02c1\u0005\u0018\u0000\u0000\u02bc\u02c0\u0003"+
		"6\u001b\u0000\u02bd\u02c0\u0005*\u0000\u0000\u02be\u02c0\u0003\u00aeW"+
		"\u0000\u02bf\u02bc\u0001\u0000\u0000\u0000\u02bf\u02bd\u0001\u0000\u0000"+
		"\u0000\u02bf\u02be\u0001\u0000\u0000\u0000\u02c0\u02c3\u0001\u0000\u0000"+
		"\u0000\u02c1\u02bf\u0001\u0000\u0000\u0000\u02c1\u02c2\u0001\u0000\u0000"+
		"\u0000\u02c2\u02c4\u0001\u0000\u0000\u0000\u02c3\u02c1\u0001\u0000\u0000"+
		"\u0000\u02c4\u02c5\u0005)\u0000\u0000\u02c5\u02c6\u0005\u0019\u0000\u0000"+
		"\u02c6\u02c7\u0003\u008cF\u0000\u02c7k\u0001\u0000\u0000\u0000\u02c8\u02ce"+
		"\u0005\u0010\u0000\u0000\u02c9\u02cd\u00036\u001b\u0000\u02ca\u02cd\u0005"+
		"*\u0000\u0000\u02cb\u02cd\u0003\u00aeW\u0000\u02cc\u02c9\u0001\u0000\u0000"+
		"\u0000\u02cc\u02ca\u0001\u0000\u0000\u0000\u02cc\u02cb\u0001\u0000\u0000"+
		"\u0000\u02cd\u02d0\u0001\u0000\u0000\u0000\u02ce\u02cc\u0001\u0000\u0000"+
		"\u0000\u02ce\u02cf\u0001\u0000\u0000\u0000\u02cf\u02d1\u0001\u0000\u0000"+
		"\u0000\u02d0\u02ce\u0001\u0000\u0000\u0000\u02d1\u02d8\u0005)\u0000\u0000"+
		"\u02d2\u02d3\u0005\u0011\u0000\u0000\u02d3\u02d5\u0003b1\u0000\u02d4\u02d6"+
		"\u0005\f\u0000\u0000\u02d5\u02d4\u0001\u0000\u0000\u0000\u02d5\u02d6\u0001"+
		"\u0000\u0000\u0000\u02d6\u02d8\u0001\u0000\u0000\u0000\u02d7\u02c8\u0001"+
		"\u0000\u0000\u0000\u02d7\u02d2\u0001\u0000\u0000\u0000\u02d8m\u0001\u0000"+
		"\u0000\u0000\u02d9\u02da\u0005V\u0000\u0000\u02dao\u0001\u0000\u0000\u0000"+
		"\u02db\u02dc\u0007\u0001\u0000\u0000\u02dcq\u0001\u0000\u0000\u0000\u02dd"+
		"\u02de\u0003\u00a2Q\u0000\u02de\u02e0\u0003\u0080@\u0000\u02df\u02e1\u0003"+
		"t:\u0000\u02e0\u02df\u0001\u0000\u0000\u0000\u02e0\u02e1\u0001\u0000\u0000"+
		"\u0000\u02e1s\u0001\u0000\u0000\u0000\u02e2\u02e3\u0005\u0016\u0000\u0000"+
		"\u02e3\u02e6\u0003~?\u0000\u02e4\u02e6\u0005\u0017\u0000\u0000\u02e5\u02e2"+
		"\u0001\u0000\u0000\u0000\u02e5\u02e4\u0001\u0000\u0000\u0000\u02e6u\u0001"+
		"\u0000\u0000\u0000\u02e7\u02e9\u0005$\u0000\u0000\u02e8\u02e7\u0001\u0000"+
		"\u0000\u0000\u02e8\u02e9\u0001\u0000\u0000\u0000\u02e9\u02ea\u0001\u0000"+
		"\u0000\u0000\u02ea\u02ed\u0003\\.\u0000\u02eb\u02ed\u0003f3\u0000\u02ec"+
		"\u02e8\u0001\u0000\u0000\u0000\u02ec\u02eb\u0001\u0000\u0000\u0000\u02ed"+
		"w\u0001\u0000\u0000\u0000\u02ee\u02ef\u0005\u000f\u0000\u0000\u02ef\u02ff"+
		"\u0005T\u0000\u0000\u02f0\u02f1\u0005\u000f\u0000\u0000\u02f1\u02f6\u0003"+
		"b1\u0000\u02f2\u02f3\u0005R\u0000\u0000\u02f3\u02f5\u0003b1\u0000\u02f4"+
		"\u02f2\u0001\u0000\u0000\u0000\u02f5\u02f8\u0001\u0000\u0000\u0000\u02f6"+
		"\u02f4\u0001\u0000\u0000\u0000\u02f6\u02f7\u0001\u0000\u0000\u0000\u02f7"+
		"\u02fa\u0001\u0000\u0000\u0000\u02f8\u02f6\u0001\u0000\u0000\u0000\u02f9"+
		"\u02fb\u0005R\u0000\u0000\u02fa\u02f9\u0001\u0000\u0000\u0000\u02fa\u02fb"+
		"\u0001\u0000\u0000\u0000\u02fb\u02fc\u0001\u0000\u0000\u0000\u02fc\u02fd"+
		"\u0005T\u0000\u0000\u02fd\u02ff\u0001\u0000\u0000\u0000\u02fe\u02ee\u0001"+
		"\u0000\u0000\u0000\u02fe\u02f0\u0001\u0000\u0000\u0000\u02ffy\u0001\u0000"+
		"\u0000\u0000\u0300\u0301\u0005\u000e\u0000\u0000\u0301\u0311\u0005)\u0000"+
		"\u0000\u0302\u0303\u0005\u000e\u0000\u0000\u0303\u0308\u0003|>\u0000\u0304"+
		"\u0305\u0005R\u0000\u0000\u0305\u0307\u0003|>\u0000\u0306\u0304\u0001"+
		"\u0000\u0000\u0000\u0307\u030a\u0001\u0000\u0000\u0000\u0308\u0306\u0001"+
		"\u0000\u0000\u0000\u0308\u0309\u0001\u0000\u0000\u0000\u0309\u030c\u0001"+
		"\u0000\u0000\u0000\u030a\u0308\u0001\u0000\u0000\u0000\u030b\u030d\u0005"+
		"R\u0000\u0000\u030c\u030b\u0001\u0000\u0000\u0000\u030c\u030d\u0001\u0000"+
		"\u0000\u0000\u030d\u030e\u0001\u0000\u0000\u0000\u030e\u030f\u0005)\u0000"+
		"\u0000\u030f\u0311\u0001\u0000\u0000\u0000\u0310\u0300\u0001\u0000\u0000"+
		"\u0000\u0310\u0302\u0001\u0000\u0000\u0000\u0311{\u0001\u0000\u0000\u0000"+
		"\u0312\u0314\u0003\n\u0005\u0000\u0313\u0312\u0001\u0000\u0000\u0000\u0314"+
		"\u0317\u0001\u0000\u0000\u0000\u0315\u0313\u0001\u0000\u0000\u0000\u0315"+
		"\u0316\u0001\u0000\u0000\u0000\u0316\u0318\u0001\u0000\u0000\u0000\u0317"+
		"\u0315\u0001\u0000\u0000\u0000\u0318\u0319\u0007\u0002\u0000\u0000\u0319"+
		"\u031a\u0005X\u0000\u0000\u031a\u031b\u0003b1\u0000\u031b}\u0001\u0000"+
		"\u0000\u0000\u031c\u031d\u0005F\u0000\u0000\u031d\u0322\u0003b1\u0000"+
		"\u031e\u031f\u0005R\u0000\u0000\u031f\u0321\u0003b1\u0000\u0320\u031e"+
		"\u0001\u0000\u0000\u0000\u0321\u0324\u0001\u0000\u0000\u0000\u0322\u0320"+
		"\u0001\u0000\u0000\u0000\u0322\u0323\u0001\u0000\u0000\u0000\u0323\u0326"+
		"\u0001\u0000\u0000\u0000\u0324\u0322\u0001\u0000\u0000\u0000\u0325\u0327"+
		"\u0005R\u0000\u0000\u0326\u0325\u0001\u0000\u0000\u0000\u0326\u0327\u0001"+
		"\u0000\u0000\u0000\u0327\u0328\u0001\u0000\u0000\u0000\u0328\u0329\u0005"+
		"S\u0000\u0000\u0329\u0333\u0001\u0000\u0000\u0000\u032a\u032b\u0005F\u0000"+
		"\u0000\u032b\u0333\u0005S\u0000\u0000\u032c\u0333\u0005E\u0000\u0000\u032d"+
		"\u032f\u0003\u0084B\u0000\u032e\u032d\u0001\u0000\u0000\u0000\u032f\u0330"+
		"\u0001\u0000\u0000\u0000\u0330\u032e\u0001\u0000\u0000\u0000\u0330\u0331"+
		"\u0001\u0000\u0000\u0000\u0331\u0333\u0001\u0000\u0000\u0000\u0332\u031c"+
		"\u0001\u0000\u0000\u0000\u0332\u032a\u0001\u0000\u0000\u0000\u0332\u032c"+
		"\u0001\u0000\u0000\u0000\u0332\u032e\u0001\u0000\u0000\u0000\u0333\u007f"+
		"\u0001\u0000\u0000\u0000\u0334\u0335\u0005F\u0000\u0000\u0335\u033a\u0003"+
		"\u0082A\u0000\u0336\u0337\u0005R\u0000\u0000\u0337\u0339\u0003\u0082A"+
		"\u0000\u0338\u0336\u0001\u0000\u0000\u0000\u0339\u033c\u0001\u0000\u0000"+
		"\u0000\u033a\u0338\u0001\u0000\u0000\u0000\u033a\u033b\u0001\u0000\u0000"+
		"\u0000\u033b\u033e\u0001\u0000\u0000\u0000\u033c\u033a\u0001\u0000\u0000"+
		"\u0000\u033d\u033f\u0005R\u0000\u0000\u033e\u033d\u0001\u0000\u0000\u0000"+
		"\u033e\u033f\u0001\u0000\u0000\u0000\u033f\u0340\u0001\u0000\u0000\u0000"+
		"\u0340\u0341\u0005S\u0000\u0000\u0341\u034b\u0001\u0000\u0000\u0000\u0342"+
		"\u0343\u0005F\u0000\u0000\u0343\u034b\u0005S\u0000\u0000\u0344\u034b\u0005"+
		"E\u0000\u0000\u0345\u0347\u0003\u0084B\u0000\u0346\u0345\u0001\u0000\u0000"+
		"\u0000\u0347\u0348\u0001\u0000\u0000\u0000\u0348\u0346\u0001\u0000\u0000"+
		"\u0000\u0348\u0349\u0001\u0000\u0000\u0000\u0349\u034b\u0001\u0000\u0000"+
		"\u0000\u034a\u0334\u0001\u0000\u0000\u0000\u034a\u0342\u0001\u0000\u0000"+
		"\u0000\u034a\u0344\u0001\u0000\u0000\u0000\u034a\u0346\u0001\u0000\u0000"+
		"\u0000\u034b\u0081\u0001\u0000\u0000\u0000\u034c\u034d\u0007\u0002\u0000"+
		"\u0000\u034d\u034e\u0005X\u0000\u0000\u034e\u0351\u0003b1\u0000\u034f"+
		"\u0351\u0003b1\u0000\u0350\u034c\u0001\u0000\u0000\u0000\u0350\u034f\u0001"+
		"\u0000\u0000\u0000\u0351\u0083\u0001\u0000\u0000\u0000\u0352\u0353\u0005"+
		"\u0004\u0000\u0000\u0353\u0354\u0003\u0086C\u0000\u0354\u0355\u0005J\u0000"+
		"\u0000\u0355\u0085\u0001\u0000\u0000\u0000\u0356\u035a\u0005I\u0000\u0000"+
		"\u0357\u035a\u0003\u00b2Y\u0000\u0358\u035a\u0003\u0088D\u0000\u0359\u0356"+
		"\u0001\u0000\u0000\u0000\u0359\u0357\u0001\u0000\u0000\u0000\u0359\u0358"+
		"\u0001\u0000\u0000\u0000\u035a\u035d\u0001\u0000\u0000\u0000\u035b\u0359"+
		"\u0001\u0000\u0000\u0000\u035b\u035c\u0001\u0000\u0000\u0000\u035c\u0087"+
		"\u0001\u0000\u0000\u0000\u035d\u035b\u0001\u0000\u0000\u0000\u035e\u035f"+
		"\u0005\t\u0000\u0000\u035f\u0360\u0003b1\u0000\u0360\u0361\u0005T\u0000"+
		"\u0000\u0361\u0089\u0001\u0000\u0000\u0000\u0362\u0364\u0005\t\u0000\u0000"+
		"\u0363\u0365\u0003\u0094J\u0000\u0364\u0363\u0001\u0000\u0000\u0000\u0365"+
		"\u0366\u0001\u0000\u0000\u0000\u0366\u0364\u0001\u0000\u0000\u0000\u0366"+
		"\u0367\u0001\u0000\u0000\u0000\u0367\u0368\u0001\u0000\u0000\u0000\u0368"+
		"\u0369\u0005T\u0000\u0000\u0369\u008b\u0001\u0000\u0000\u0000\u036a\u036e"+
		"\u0003\u0090H\u0000\u036b\u036d\u0003\u0094J\u0000\u036c\u036b\u0001\u0000"+
		"\u0000\u0000\u036d\u0370\u0001\u0000\u0000\u0000\u036e\u036c\u0001\u0000"+
		"\u0000\u0000\u036e\u036f\u0001\u0000\u0000\u0000\u036f\u008d\u0001\u0000"+
		"\u0000\u0000\u0370\u036e\u0001\u0000\u0000\u0000\u0371\u0372\u0005\u0015"+
		"\u0000\u0000\u0372\u0373\u0003~?\u0000\u0373\u008f\u0001\u0000\u0000\u0000"+
		"\u0374\u0379\u0003\u008eG\u0000\u0375\u0379\u0005\r\u0000\u0000\u0376"+
		"\u0377\u0005U\u0000\u0000\u0377\u0379\u0007\u0003\u0000\u0000\u0378\u0374"+
		"\u0001\u0000\u0000\u0000\u0378\u0375\u0001\u0000\u0000\u0000\u0378\u0376"+
		"\u0001\u0000\u0000\u0000\u0379\u0091\u0001\u0000\u0000\u0000\u037a\u037b"+
		"\u0005W\u0000\u0000\u037b\u037c\u0003b1\u0000\u037c\u0093\u0001\u0000"+
		"\u0000\u0000\u037d\u037e\u0003\u0096K\u0000\u037e\u0380\u0003\u00a2Q\u0000"+
		"\u037f\u0381\u0003\u0080@\u0000\u0380\u037f\u0001\u0000\u0000\u0000\u0380"+
		"\u0381\u0001\u0000\u0000\u0000\u0381\u0383\u0001\u0000\u0000\u0000\u0382"+
		"\u0384\u0003t:\u0000\u0383\u0382\u0001\u0000\u0000\u0000\u0383\u0384\u0001"+
		"\u0000\u0000\u0000\u0384\u0388\u0001\u0000\u0000\u0000\u0385\u0388\u0003"+
		"\u009aM\u0000\u0386\u0388\u0005Q\u0000\u0000\u0387\u037d\u0001\u0000\u0000"+
		"\u0000\u0387\u0385\u0001\u0000\u0000\u0000\u0387\u0386\u0001\u0000\u0000"+
		"\u0000\u0388\u0095\u0001\u0000\u0000\u0000\u0389\u038a\u0007\u0004\u0000"+
		"\u0000\u038a\u0097\u0001\u0000\u0000\u0000\u038b\u038c\u0005@\u0000\u0000"+
		"\u038c\u038d\u0005a\u0000\u0000\u038d\u0099\u0001\u0000\u0000\u0000\u038e"+
		"\u038f\u0005P\u0000\u0000\u038f\u0390\u0005`\u0000\u0000\u0390\u009b\u0001"+
		"\u0000\u0000\u0000\u0391\u0395\u0005\n\u0000\u0000\u0392\u0395\u0005\u000b"+
		"\u0000\u0000\u0393\u0395\u0003\u0084B\u0000\u0394\u0391\u0001\u0000\u0000"+
		"\u0000\u0394\u0392\u0001\u0000\u0000\u0000\u0394\u0393\u0001\u0000\u0000"+
		"\u0000\u0395\u009d\u0001\u0000\u0000\u0000\u0396\u0399\u0005\n\u0000\u0000"+
		"\u0397\u0399\u0003\u0084B\u0000\u0398\u0396\u0001\u0000\u0000\u0000\u0398"+
		"\u0397\u0001\u0000\u0000\u0000\u0399\u009f\u0001\u0000\u0000\u0000\u039a"+
		"\u039b\u0005\n\u0000\u0000\u039b\u00a1\u0001\u0000\u0000\u0000\u039c\u039d"+
		"\u0007\u0005\u0000\u0000\u039d\u00a3\u0001\u0000\u0000\u0000\u039e\u039f"+
		"\u0007\u0006\u0000\u0000\u039f\u00a5\u0001\u0000\u0000\u0000\u03a0\u03a1"+
		"\u0007\u0000\u0000\u0000\u03a1\u00a7\u0001\u0000\u0000\u0000\u03a2\u03a3"+
		"\u0007\u0007\u0000\u0000\u03a3\u00a9\u0001\u0000\u0000\u0000\u03a4\u03a5"+
		"\u00053\u0000\u0000\u03a5\u00ab\u0001\u0000\u0000\u0000\u03a6\u03a7\u0007"+
		"\b\u0000\u0000\u03a7\u00ad\u0001\u0000\u0000\u0000\u03a8\u03ab\u0003\u00b0"+
		"X\u0000\u03a9\u03ab\u0005$\u0000\u0000\u03aa\u03a8\u0001\u0000\u0000\u0000"+
		"\u03aa\u03a9\u0001\u0000\u0000\u0000\u03ab\u00af\u0001\u0000\u0000\u0000"+
		"\u03ac\u03ad\u0007\t\u0000\u0000\u03ad\u00b1\u0001\u0000\u0000\u0000\u03ae"+
		"\u03af\u0005\u0003\u0000\u0000\u03af\u03b0\u0007\n\u0000\u0000\u03b0\u00b3"+
		"\u0001\u0000\u0000\u0000z\u00b7\u00bb\u00c1\u00c6\u00cf\u00d6\u00dd\u00e3"+
		"\u00e8\u00ee\u00f4\u00fa\u00ff\u0102\u010b\u0111\u011a\u0120\u0122\u012a"+
		"\u0133\u0139\u013e\u0145\u014a\u0153\u0159\u015b\u015f\u0162\u0169\u016e"+
		"\u0174\u017e\u0182\u0186\u0190\u0194\u0198\u01a5\u01a9\u01b0\u01b3\u01b7"+
		"\u01bb\u01c3\u01c5\u01ce\u01d4\u01da\u01e6\u01eb\u01f0\u01f8\u01ff\u0208"+
		"\u0215\u0217\u021b\u0224\u022e\u0234\u0236\u023a\u0243\u0246\u024b\u024e"+
		"\u0254\u0256\u025f\u0264\u026c\u026f\u0273\u0279\u0283\u0285\u028b\u028f"+
		"\u029a\u029e\u02a3\u02a9\u02b9\u02bf\u02c1\u02cc\u02ce\u02d5\u02d7\u02e0"+
		"\u02e5\u02e8\u02ec\u02f6\u02fa\u02fe\u0308\u030c\u0310\u0315\u0322\u0326"+
		"\u0330\u0332\u033a\u033e\u0348\u034a\u0350\u0359\u035b\u0366\u036e\u0378"+
		"\u0380\u0383\u0387\u0394\u0398\u03aa";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}