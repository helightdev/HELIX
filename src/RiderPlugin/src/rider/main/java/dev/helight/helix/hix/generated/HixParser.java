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
		KEYWORD_BREAK=46, KEYWORD_CONTINUE=47, KEYWORD_TARGET=48, KEYWORD_VAR=49,
		KEYWORD_LOCAL=50, KEYWORD_CARRY=51, KEYWORD_SIG=52, KEYWORD_WHEN=53, KEYWORD_PURE=54,
		KEYWORD_INLINE=55, KEYWORD_NOINLINE=56, KEYWORD_STRICT=57, LABEL_PREFIX=58,
		ASSIGN=59, ARROW=60, TOPLEVEL_VALUE_EXPRESSION=61, EMPTY_PARAMETERS=62,
		BEGIN_PARAMETERS=63, METADATA_WHITESPACE=64, ARGUMENT_TEXT=65, ARGUMENT_END=66,
		CONTENT_WRAP=67, CONTENT_LINEBREAK=68, CONTENT_TEXT=69, VALUE_FUNCTION=70,
		VALUE_PREDICATE=71, VALUE_MEMBER=72, VALUE_WRAP=73, VALUE_DELIMITER=74,
		END_PARAMETERS=75, VALUE_END_INLINE=76, VALUE_SMART_ROOT=77, NOT_VALUE=78,
		VALUE_CHECK=79, VALUE_ELVIS=80, VALUE_ASSIGN=81, VALUE_EXPAND=82, VALUE_WHITESPACE=83,
		ESCAPE_MACRO=84, ESCAPE_LITERAL=85, ESCAPE_HEX=86, END_CONTENT_INTERPOLATE=87,
		FUNCTION_IDENTIFIER=88, MEMBER_IDENTIFIER=89, LABEL_IDENTIFIER=90, TOPLEVEL_NULL=91;
	public static final int
		RULE_compilationUnit = 0, RULE_fileMetadataSection = 1, RULE_topLevelDeclaration = 2,
		RULE_metadataList = 3, RULE_metadata = 4, RULE_metadataValue = 5, RULE_mixinDeclaration = 6,
		RULE_typeDeclaration = 7, RULE_mixinBody = 8, RULE_expressionDeclaration = 9,
		RULE_funcDeclaration = 10, RULE_directFunctionSignature = 11, RULE_functionBody = 12,
		RULE_functionMetadata = 13, RULE_functionSignatureVariant = 14, RULE_patternExpression = 15,
		RULE_patternPrimary = 16, RULE_tablePattern = 17, RULE_tuplePattern = 18,
		RULE_delegatePattern = 19, RULE_patternParameterList = 20, RULE_patternField = 21,
		RULE_patternIdentifier = 22, RULE_signature = 23, RULE_tableSignature = 24,
		RULE_tableSignatureEntry = 25, RULE_statementBlock = 26, RULE_statement = 27,
		RULE_invocationStatement = 28, RULE_whenConditionStatement = 29, RULE_whenElseBranch = 30,
		RULE_whenResult = 31, RULE_whenChainCondition = 32, RULE_whenChainStatement = 33,
		RULE_whenChainBody = 34, RULE_whenChainBranch = 35, RULE_whenValueStatement = 36,
		RULE_whenValueBody = 37, RULE_whenValueBranch = 38, RULE_whenValueCondition = 39,
		RULE_assignmentStatement = 40, RULE_assignedValue = 41, RULE_controlflowStatement = 42,
		RULE_contentBlock = 43, RULE_contentBody = 44, RULE_contentInterpolate = 45,
		RULE_value = 46, RULE_valueExpression = 47, RULE_nonArgumentValue = 48,
		RULE_primaryValue = 49, RULE_lambdaValue = 50, RULE_prefixOperators = 51,
		RULE_postfixOperators = 52, RULE_valueStatement = 53, RULE_tailValue = 54,
		RULE_tupleValue = 55, RULE_tableValue = 56, RULE_tableKeyedEntry = 57,
		RULE_valueList = 58, RULE_argumentValue = 59, RULE_argumentBody = 60,
		RULE_inlineValue = 61, RULE_inlineTransformation = 62, RULE_derivation = 63,
		RULE_derivationRoot = 64, RULE_elvisValue = 65, RULE_transformationPart = 66,
		RULE_functionChainType = 67, RULE_labelIdentifier = 68, RULE_memberIdentifier = 69,
		RULE_mixinIdentifier = 70, RULE_functionDeclarationIdentifier = 71, RULE_variableIdentifier = 72,
		RULE_functionIdentifier = 73, RULE_kindIdentifier = 74, RULE_expressionModifier = 75,
		RULE_mixinModifier = 76, RULE_variableSpecifiers = 77, RULE_funcModifier = 78,
		RULE_trivia = 79, RULE_comment = 80, RULE_escaped = 81;
	private static String[] makeRuleNames() {
		return new String[] {
			"compilationUnit", "fileMetadataSection", "topLevelDeclaration", "metadataList",
			"metadata", "metadataValue", "mixinDeclaration", "typeDeclaration", "mixinBody",
			"expressionDeclaration", "funcDeclaration", "directFunctionSignature",
			"functionBody", "functionMetadata", "functionSignatureVariant", "patternExpression",
			"patternPrimary", "tablePattern", "tuplePattern", "delegatePattern",
			"patternParameterList", "patternField", "patternIdentifier", "signature",
			"tableSignature", "tableSignatureEntry", "statementBlock", "statement",
			"invocationStatement", "whenConditionStatement", "whenElseBranch", "whenResult",
			"whenChainCondition", "whenChainStatement", "whenChainBody", "whenChainBranch",
			"whenValueStatement", "whenValueBody", "whenValueBranch", "whenValueCondition",
			"assignmentStatement", "assignedValue", "controlflowStatement", "contentBlock",
			"contentBody", "contentInterpolate", "value", "valueExpression", "nonArgumentValue",
			"primaryValue", "lambdaValue", "prefixOperators", "postfixOperators",
			"valueStatement", "tailValue", "tupleValue", "tableValue", "tableKeyedEntry",
			"valueList", "argumentValue", "argumentBody", "inlineValue", "inlineTransformation",
			"derivation", "derivationRoot", "elvisValue", "transformationPart", "functionChainType",
			"labelIdentifier", "memberIdentifier", "mixinIdentifier", "functionDeclarationIdentifier",
			"variableIdentifier", "functionIdentifier", "kindIdentifier", "expressionModifier",
			"mixinModifier", "variableSpecifiers", "funcModifier", "trivia", "comment",
			"escaped"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null, null, null,
			"'@@'", "'\\'", null, null, null, null, "'---'", "'{'", null, null, null,
			"'func'", null, "'type'", "'do'", "'expression'", "'mixin'", "'prelude'",
			"'derivation'", "'else'", "'return'", "'goto'", "'break'", "'continue'",
			"'target'", "'var'", "'local'", "'carry'", "'sig'", "'when'", "'pure'",
			"'inline'", "'noinline'", "'strict'", null, null, "'->'", null, null,
			"'('", null, null, "'>'", null, null, null, null, "':?'", "'#'", null,
			null, "')'", "']'", "'$'", "'!'", "'?'", "'?:'", null, "'...'"
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
			"KEYWORD_TARGET", "KEYWORD_VAR", "KEYWORD_LOCAL", "KEYWORD_CARRY", "KEYWORD_SIG",
			"KEYWORD_WHEN", "KEYWORD_PURE", "KEYWORD_INLINE", "KEYWORD_NOINLINE",
			"KEYWORD_STRICT", "LABEL_PREFIX", "ASSIGN", "ARROW", "TOPLEVEL_VALUE_EXPRESSION",
			"EMPTY_PARAMETERS", "BEGIN_PARAMETERS", "METADATA_WHITESPACE", "ARGUMENT_TEXT",
			"ARGUMENT_END", "CONTENT_WRAP", "CONTENT_LINEBREAK", "CONTENT_TEXT",
			"VALUE_FUNCTION", "VALUE_PREDICATE", "VALUE_MEMBER", "VALUE_WRAP", "VALUE_DELIMITER",
			"END_PARAMETERS", "VALUE_END_INLINE", "VALUE_SMART_ROOT", "NOT_VALUE",
			"VALUE_CHECK", "VALUE_ELVIS", "VALUE_ASSIGN", "VALUE_EXPAND", "VALUE_WHITESPACE",
			"ESCAPE_MACRO", "ESCAPE_LITERAL", "ESCAPE_HEX", "END_CONTENT_INTERPOLATE",
			"FUNCTION_IDENTIFIER", "MEMBER_IDENTIFIER", "LABEL_IDENTIFIER", "TOPLEVEL_NULL"
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
			setState(167);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(164);
				trivia();
				}
				}
				setState(169);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(171);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,1,_ctx) ) {
			case 1:
				{
				setState(170);
				fileMetadataSection();
				}
				break;
			}
			setState(182);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 126106458923991040L) != 0)) {
				{
				{
				setState(173);
				topLevelDeclaration();
				setState(177);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
					{
					{
					setState(174);
					trivia();
					}
					}
					setState(179);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
				}
				setState(184);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(185);
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
			setState(187);
			metadataList();
			setState(191);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(188);
				trivia();
				}
				}
				setState(193);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(194);
			match(SECTION_DELIMITER);
			setState(198);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(195);
				trivia();
				}
				}
				setState(200);
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
			setState(216);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(201);
				metadataList();
				setState(205);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
					{
					{
					setState(202);
					trivia();
					}
					}
					setState(207);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(211);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_MIXIN:
				case KEYWORD_DERIVATION:
					{
					setState(208);
					mixinDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(209);
					funcDeclaration();
					}
					break;
				case KEYWORD_TYPE:
					{
					setState(210);
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
				setState(213);
				mixinDeclaration();
				}
				break;
			case KEYWORD_FUNC:
			case KEYWORD_PURE:
			case KEYWORD_INLINE:
			case KEYWORD_NOINLINE:
				enterOuterAlt(_localctx, 3);
				{
				setState(214);
				funcDeclaration();
				}
				break;
			case KEYWORD_TYPE:
				enterOuterAlt(_localctx, 4);
				{
				setState(215);
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
			setState(218);
			metadata();
			setState(228);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,10,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(222);
					_errHandler.sync(this);
					_la = _input.LA(1);
					while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
						{
						{
						setState(219);
						trivia();
						}
						}
						setState(224);
						_errHandler.sync(this);
						_la = _input.LA(1);
					}
					setState(225);
					metadata();
					}
					}
				}
				setState(230);
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
			setState(237);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(231);
				match(METADATA_PREFIX);
				setState(232);
				match(IDENTIFIER);
				setState(234);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & -4611686018427387888L) != 0)) {
					{
					setState(233);
					valueList();
					}
				}

				}
				break;
			case BEGIN_METADATA_VALUE:
				enterOuterAlt(_localctx, 2);
				{
				setState(236);
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
			setState(239);
			match(BEGIN_METADATA_VALUE);
			setState(240);
			value();
			setState(241);
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
			setState(246);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_DERIVATION) {
				{
				{
				setState(243);
				mixinModifier();
				}
				}
				setState(248);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(249);
			match(KEYWORD_MIXIN);
			setState(250);
			mixinIdentifier();
			setState(251);
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
			setState(253);
			match(KEYWORD_TYPE);
			setState(254);
			match(IDENTIFIER);
			setState(255);
			match(ASSIGN);
			setState(256);
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
			setState(258);
			match(LC);
			setState(264);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 270218761720561664L) != 0)) {
				{
				setState(262);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_EXPRESSION:
				case KEYWORD_PRELUDE:
				case KEYWORD_STRICT:
					{
					setState(259);
					expressionDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(260);
					funcDeclaration();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(261);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(266);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(267);
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
			setState(272);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_PRELUDE || _la==KEYWORD_STRICT) {
				{
				{
				setState(269);
				expressionModifier();
				}
				}
				setState(274);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(275);
			match(KEYWORD_EXPRESSION);
			setState(276);
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
		public DirectFunctionSignatureContext directFunctionSignature() {
			return getRuleContext(DirectFunctionSignatureContext.class,0);
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
			setState(281);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 126100789566373888L) != 0)) {
				{
				{
				setState(278);
				funcModifier();
				}
				}
				setState(283);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(284);
			match(KEYWORD_FUNC);
			setState(285);
			functionDeclarationIdentifier();
			setState(287);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==EMPTY_PARAMETERS || _la==BEGIN_PARAMETERS) {
				{
				setState(286);
				directFunctionSignature();
				}
			}

			setState(289);
			functionMetadata();
			setState(290);
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
		public PatternParameterListContext patternParameterList() {
			return getRuleContext(PatternParameterListContext.class,0);
		}
		public TerminalNode ARROW() { return getToken(HixParser.ARROW, 0); }
		public PatternExpressionContext patternExpression() {
			return getRuleContext(PatternExpressionContext.class,0);
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
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(292);
			patternParameterList();
			setState(293);
			match(ARROW);
			setState(294);
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
			setState(305);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case LC:
			case KEYWORD_DO:
				enterOuterAlt(_localctx, 1);
				{
				setState(297);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_DO) {
					{
					setState(296);
					match(KEYWORD_DO);
					}
				}

				setState(299);
				statementBlock();
				}
				break;
			case FAT_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(300);
				match(FAT_ARROW);
				setState(301);
				value();
				setState(303);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_END) {
					{
					setState(302);
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
		enterRule(_localctx, 26, RULE_functionMetadata);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(311);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==NEWLINE || _la==KEYWORD_SIG) {
				{
				setState(309);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_SIG:
					{
					setState(307);
					functionSignatureVariant();
					}
					break;
				case NEWLINE:
					{
					setState(308);
					match(NEWLINE);
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(313);
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
		enterRule(_localctx, 28, RULE_functionSignatureVariant);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(314);
			match(KEYWORD_SIG);
			setState(315);
			signature();
			setState(316);
			match(ARROW);
			setState(317);
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
		enterRule(_localctx, 30, RULE_patternExpression);
		try {
			setState(324);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_METADATA_VALUE:
			case METADATA_PREFIX:
				enterOuterAlt(_localctx, 1);
				{
				setState(319);
				metadataList();
				setState(321);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,24,_ctx) ) {
				case 1:
					{
					setState(320);
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
				setState(323);
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
		enterRule(_localctx, 32, RULE_patternPrimary);
		try {
			setState(330);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case NULL:
				enterOuterAlt(_localctx, 1);
				{
				setState(326);
				patternIdentifier();
				}
				break;
			case BEGIN_TABLE:
				enterOuterAlt(_localctx, 2);
				{
				setState(327);
				tablePattern();
				}
				break;
			case BEGIN_TUPLE:
				enterOuterAlt(_localctx, 3);
				{
				setState(328);
				tuplePattern();
				}
				break;
			case KEYWORD_DELEGATE:
				enterOuterAlt(_localctx, 4);
				{
				setState(329);
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
		enterRule(_localctx, 34, RULE_tablePattern);
		int _la;
		try {
			int _alt;
			setState(348);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,29,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(332);
				match(BEGIN_TABLE);
				setState(333);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(334);
				match(BEGIN_TABLE);
				setState(335);
				patternField();
				setState(340);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,27,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(336);
						match(VALUE_DELIMITER);
						setState(337);
						patternField();
						}
						}
					}
					setState(342);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,27,_ctx);
				}
				setState(344);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(343);
					match(VALUE_DELIMITER);
					}
				}

				setState(346);
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
		enterRule(_localctx, 36, RULE_tuplePattern);
		int _la;
		try {
			int _alt;
			setState(366);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,32,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(350);
				match(BEGIN_TUPLE);
				setState(351);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(352);
				match(BEGIN_TUPLE);
				setState(353);
				patternField();
				setState(358);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,30,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(354);
						match(VALUE_DELIMITER);
						setState(355);
						patternField();
						}
						}
					}
					setState(360);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,30,_ctx);
				}
				setState(362);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(361);
					match(VALUE_DELIMITER);
					}
				}

				setState(364);
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
		enterRule(_localctx, 38, RULE_delegatePattern);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(368);
			match(KEYWORD_DELEGATE);
			setState(369);
			patternParameterList();
			setState(370);
			match(ARROW);
			setState(371);
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
		enterRule(_localctx, 40, RULE_patternParameterList);
		int _la;
		try {
			int _alt;
			setState(390);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,35,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(373);
				match(BEGIN_PARAMETERS);
				setState(374);
				patternField();
				setState(379);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,33,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(375);
						match(VALUE_DELIMITER);
						setState(376);
						patternField();
						}
						}
					}
					setState(381);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,33,_ctx);
				}
				setState(383);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(382);
					match(VALUE_DELIMITER);
					}
				}

				setState(385);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(387);
				match(BEGIN_PARAMETERS);
				setState(388);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(389);
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
		enterRule(_localctx, 42, RULE_patternField);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(393);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				setState(392);
				metadataList();
				}
			}

			setState(395);
			patternPrimary();
			setState(397);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ROOT_IDENTIFIER) {
				{
				setState(396);
				match(ROOT_IDENTIFIER);
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
		enterRule(_localctx, 44, RULE_patternIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(399);
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
		enterRule(_localctx, 46, RULE_signature);
		try {
			setState(403);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_TABLE:
				enterOuterAlt(_localctx, 1);
				{
				setState(401);
				tableSignature();
				}
				break;
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case NULL:
				enterOuterAlt(_localctx, 2);
				{
				setState(402);
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
		enterRule(_localctx, 48, RULE_tableSignature);
		int _la;
		try {
			int _alt;
			setState(421);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,41,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(405);
				match(BEGIN_TABLE);
				setState(406);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(407);
				match(BEGIN_TABLE);
				setState(408);
				tableSignatureEntry();
				setState(413);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,39,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(409);
						match(VALUE_DELIMITER);
						setState(410);
						tableSignatureEntry();
						}
						}
					}
					setState(415);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,39,_ctx);
				}
				setState(417);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(416);
					match(VALUE_DELIMITER);
					}
				}

				setState(419);
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
		enterRule(_localctx, 50, RULE_tableSignatureEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(426);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				{
				setState(423);
				metadata();
				}
				}
				setState(428);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(430);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==VALUE_EXPAND) {
				{
				setState(429);
				match(VALUE_EXPAND);
				}
			}

			setState(432);
			match(ROOT_IDENTIFIER);
			setState(433);
			match(VALUE_ASSIGN);
			setState(434);
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
		enterRule(_localctx, 52, RULE_statementBlock);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(436);
			match(LC);
			setState(442);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 301723594524722176L) != 0)) {
				{
				setState(440);
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
					setState(437);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(438);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(439);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(444);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(445);
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
		enterRule(_localctx, 54, RULE_statement);
		int _la;
		try {
			setState(460);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,47,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(447);
				labelIdentifier();
				setState(448);
				match(NEWLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(451);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LABEL_PREFIX) {
					{
					setState(450);
					labelIdentifier();
					}
				}

				setState(453);
				statementBlock();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(454);
				invocationStatement();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(455);
				assignmentStatement();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(456);
				controlflowStatement();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(457);
				whenValueStatement();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(458);
				whenConditionStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(459);
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
		enterRule(_localctx, 56, RULE_invocationStatement);
		try {
			setState(469);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,49,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(462);
				match(IDENTIFIER);
				setState(463);
				valueList();
				setState(465);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,48,_ctx) ) {
				case 1:
					{
					setState(464);
					tailValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(467);
				match(IDENTIFIER);
				setState(468);
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
		enterRule(_localctx, 58, RULE_whenConditionStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(471);
			match(KEYWORD_WHEN);
			setState(472);
			whenChainCondition();
			setState(473);
			statementBlock();
			setState(484);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,51,_ctx) ) {
			case 1:
				{
				setState(477);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
					{
					{
					setState(474);
					trivia();
					}
					}
					setState(479);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(480);
				match(KEYWORD_ELSE);
				setState(481);
				whenResult();
				setState(482);
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
		enterRule(_localctx, 60, RULE_whenElseBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(486);
			match(KEYWORD_ELSE);
			setState(487);
			match(ARROW);
			setState(488);
			whenResult();
			setState(489);
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
		enterRule(_localctx, 62, RULE_whenResult);
		try {
			setState(493);
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
				setState(491);
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
				setState(492);
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
		enterRule(_localctx, 64, RULE_whenChainCondition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(495);
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
		enterRule(_localctx, 66, RULE_whenChainStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(497);
			match(KEYWORD_WHEN);
			setState(498);
			match(LC);
			setState(499);
			match(NEWLINE);
			setState(500);
			whenChainBody();
			setState(501);
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
		enterRule(_localctx, 68, RULE_whenChainBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(503);
			whenChainBranch();
			setState(508);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & -4611686017487863792L) != 0)) {
				{
				setState(506);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case EMPTY_PARAMETERS:
				case BEGIN_PARAMETERS:
					{
					setState(504);
					whenChainBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(505);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(510);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(512);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(511);
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
		enterRule(_localctx, 70, RULE_whenChainBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(514);
			whenChainCondition();
			setState(515);
			match(ARROW);
			setState(516);
			whenResult();
			setState(517);
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
		enterRule(_localctx, 72, RULE_whenValueStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(519);
			match(KEYWORD_WHEN);
			setState(521);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 2305843009228628496L) != 0) || ((((_la - 77)) & ~0x3f) == 0 && ((1L << (_la - 77)) & 2051L) != 0)) {
				{
				setState(520);
				value();
				}
			}

			setState(523);
			match(LC);
			setState(524);
			match(NEWLINE);
			setState(525);
			whenValueBody();
			setState(526);
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
		enterRule(_localctx, 74, RULE_whenValueBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(531);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 939524096L) != 0)) {
				{
				{
				setState(528);
				trivia();
				}
				}
				setState(533);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(534);
			whenValueBranch();
			setState(539);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 2305843010168152592L) != 0) || ((((_la - 77)) & ~0x3f) == 0 && ((1L << (_la - 77)) & 2051L) != 0)) {
				{
				setState(537);
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
					setState(535);
					whenValueBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(536);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(541);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(543);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(542);
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
		enterRule(_localctx, 76, RULE_whenValueBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(545);
			whenValueCondition();
			setState(546);
			match(ARROW);
			setState(547);
			whenResult();
			setState(548);
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
		enterRule(_localctx, 78, RULE_whenValueCondition);
		try {
			setState(552);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,61,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(550);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(551);
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
		enterRule(_localctx, 80, RULE_assignmentStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(554);
			variableSpecifiers();
			setState(555);
			variableIdentifier();
			setState(556);
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
		enterRule(_localctx, 82, RULE_assignedValue);
		try {
			setState(566);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ASSIGN:
				enterOuterAlt(_localctx, 1);
				{
				setState(558);
				match(ASSIGN);
				setState(563);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,62,_ctx) ) {
				case 1:
					{
					setState(559);
					value();
					}
					break;
				case 2:
					{
					setState(560);
					whenValueStatement();
					}
					break;
				case 3:
					{
					setState(561);
					whenChainStatement();
					}
					break;
				case 4:
					{
					setState(562);
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
				setState(565);
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
		enterRule(_localctx, 84, RULE_controlflowStatement);
		int _la;
		try {
			setState(576);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_RETURN:
				enterOuterAlt(_localctx, 1);
				{
				setState(568);
				match(KEYWORD_RETURN);
				setState(570);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & -4611686018427387888L) != 0)) {
					{
					setState(569);
					valueList();
					}
				}

				}
				break;
			case KEYWORD_GOTO:
				enterOuterAlt(_localctx, 2);
				{
				setState(572);
				match(KEYWORD_GOTO);
				setState(573);
				match(IDENTIFIER);
				}
				break;
			case KEYWORD_CONTINUE:
				enterOuterAlt(_localctx, 3);
				{
				setState(574);
				match(KEYWORD_CONTINUE);
				}
				break;
			case KEYWORD_BREAK:
				enterOuterAlt(_localctx, 4);
				{
				setState(575);
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
		enterRule(_localctx, 86, RULE_contentBlock);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(578);
			match(BEGIN_CONTENT);
			setState(579);
			contentBody();
			setState(580);
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
		enterRule(_localctx, 88, RULE_contentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(586);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				setState(586);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case CONTENT_WRAP:
					{
					setState(582);
					match(CONTENT_WRAP);
					}
					break;
				case CONTENT_LINEBREAK:
					{
					setState(583);
					match(CONTENT_LINEBREAK);
					}
					break;
				case CONTENT_TEXT:
					{
					setState(584);
					match(CONTENT_TEXT);
					}
					break;
				case BEGIN_VALUE_INTERPOLATE:
					{
					setState(585);
					contentInterpolate();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(588);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 8)) & ~0x3f) == 0 && ((1L << (_la - 8)) & 4035225266123964417L) != 0) );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
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
		enterRule(_localctx, 90, RULE_contentInterpolate);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(590);
			match(BEGIN_VALUE_INTERPOLATE);
			setState(591);
			derivation();
			setState(592);
			match(RC);
			setState(594);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==END_CONTENT_INTERPOLATE) {
				{
				setState(593);
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
		enterRule(_localctx, 92, RULE_value);
		try {
			setState(598);
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
				setState(596);
				nonArgumentValue(0);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(597);
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
		enterRule(_localctx, 94, RULE_valueExpression);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(600);
			match(TOPLEVEL_VALUE_EXPRESSION);
			setState(601);
			value();
			setState(602);
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
		int _startState = 96;
		enterRecursionRule(_localctx, 96, RULE_nonArgumentValue, _p);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(612);
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
				setState(605);
				primaryValue();
				setState(607);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,70,_ctx) ) {
				case 1:
					{
					setState(606);
					elvisValue();
					}
					break;
				}
				}
				break;
			case NOT_VALUE:
				{
				setState(609);
				prefixOperators();
				setState(610);
				nonArgumentValue(2);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(618);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,72,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					{
					_localctx = new NonArgumentValueContext(_parentctx, _parentState);
					pushNewRecursionContext(_localctx, _startState, RULE_nonArgumentValue);
					setState(614);
					if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
					setState(615);
					postfixOperators();
					}
					}
				}
				setState(620);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,72,_ctx);
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
		enterRule(_localctx, 98, RULE_primaryValue);
		try {
			setState(631);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,73,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(621);
				inlineValue();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(622);
				lambdaValue();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(623);
				derivation();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(624);
				tableValue();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(625);
				tupleValue();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(626);
				valueExpression();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(627);
				valueStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(628);
				match(NUMBER);
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(629);
				match(BOOLEAN);
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(630);
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
		enterRule(_localctx, 100, RULE_lambdaValue);
		int _la;
		try {
			setState(648);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_LAMBDA_BLOCK:
				enterOuterAlt(_localctx, 1);
				{
				setState(633);
				match(BEGIN_LAMBDA_BLOCK);
				setState(639);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 301723594524722176L) != 0)) {
					{
					setState(637);
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
						setState(634);
						statement();
						}
						break;
					case SEMICOLON:
						{
						setState(635);
						match(SEMICOLON);
						}
						break;
					case NEWLINE:
					case COMMENT:
					case SLASH_COMMENT:
						{
						setState(636);
						trivia();
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					}
					setState(641);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(642);
				match(RC);
				}
				break;
			case BEGIN_LAMBDA_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(643);
				match(BEGIN_LAMBDA_ARROW);
				setState(644);
				value();
				setState(646);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,76,_ctx) ) {
				case 1:
					{
					setState(645);
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
		enterRule(_localctx, 102, RULE_prefixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(650);
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
		enterRule(_localctx, 104, RULE_postfixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(652);
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
		enterRule(_localctx, 106, RULE_valueStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(654);
			functionIdentifier();
			setState(655);
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
		enterRule(_localctx, 108, RULE_tailValue);
		int _la;
		try {
			setState(662);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_CONTENT:
			case NEWLINE:
				enterOuterAlt(_localctx, 1);
				{
				setState(658);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==NEWLINE) {
					{
					setState(657);
					match(NEWLINE);
					}
				}

				setState(660);
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
				setState(661);
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
		enterRule(_localctx, 110, RULE_tupleValue);
		int _la;
		try {
			int _alt;
			setState(680);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,82,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(664);
				match(BEGIN_TUPLE);
				setState(665);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(666);
				match(BEGIN_TUPLE);
				setState(667);
				value();
				setState(672);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,80,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(668);
						match(VALUE_DELIMITER);
						setState(669);
						value();
						}
						}
					}
					setState(674);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,80,_ctx);
				}
				setState(676);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(675);
					match(VALUE_DELIMITER);
					}
				}

				setState(678);
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
		enterRule(_localctx, 112, RULE_tableValue);
		int _la;
		try {
			int _alt;
			setState(698);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,85,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(682);
				match(BEGIN_TABLE);
				setState(683);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(684);
				match(BEGIN_TABLE);
				setState(685);
				tableKeyedEntry();
				setState(690);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,83,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(686);
						match(VALUE_DELIMITER);
						setState(687);
						tableKeyedEntry();
						}
						}
					}
					setState(692);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,83,_ctx);
				}
				setState(694);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(693);
					match(VALUE_DELIMITER);
					}
				}

				setState(696);
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
		enterRule(_localctx, 114, RULE_tableKeyedEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(703);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BEGIN_METADATA_VALUE || _la==METADATA_PREFIX) {
				{
				{
				setState(700);
				metadata();
				}
				}
				setState(705);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(706);
			match(ROOT_IDENTIFIER);
			setState(707);
			match(VALUE_ASSIGN);
			setState(708);
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
		enterRule(_localctx, 116, RULE_valueList);
		int _la;
		try {
			int _alt;
			setState(732);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,90,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(710);
				match(BEGIN_PARAMETERS);
				setState(711);
				value();
				setState(716);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,87,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(712);
						match(VALUE_DELIMITER);
						setState(713);
						value();
						}
						}
					}
					setState(718);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,87,_ctx);
				}
				setState(720);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_DELIMITER) {
					{
					setState(719);
					match(VALUE_DELIMITER);
					}
				}

				setState(722);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(724);
				match(BEGIN_PARAMETERS);
				setState(725);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(726);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(728);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(727);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(730);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,89,_ctx);
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
		enterRule(_localctx, 118, RULE_argumentValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(734);
			match(BEGIN_ARGUMENT);
			setState(735);
			argumentBody();
			setState(736);
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
		enterRule(_localctx, 120, RULE_argumentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(743);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 3)) & ~0x3f) == 0 && ((1L << (_la - 3)) & 4611686018427387969L) != 0)) {
				{
				setState(741);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case ARGUMENT_TEXT:
					{
					setState(738);
					match(ARGUMENT_TEXT);
					}
					break;
				case ESCAPE:
					{
					setState(739);
					escaped();
					}
					break;
				case BEGIN_VALUE_INLINE:
					{
					setState(740);
					inlineValue();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(745);
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
		enterRule(_localctx, 122, RULE_inlineValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(746);
			match(BEGIN_VALUE_INLINE);
			setState(747);
			value();
			setState(748);
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
		enterRule(_localctx, 124, RULE_inlineTransformation);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(750);
			match(BEGIN_VALUE_INLINE);
			setState(752);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(751);
				transformationPart();
				}
				}
				setState(754);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 70)) & ~0x3f) == 0 && ((1L << (_la - 70)) & 15L) != 0) );
			setState(756);
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
		enterRule(_localctx, 126, RULE_derivation);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(758);
			derivationRoot();
			setState(762);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,94,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(759);
					transformationPart();
					}
					}
				}
				setState(764);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,94,_ctx);
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
		enterRule(_localctx, 128, RULE_derivationRoot);
		int _la;
		try {
			setState(768);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ROOT_IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(765);
				match(ROOT_IDENTIFIER);
				}
				break;
			case VALUE_SMART_ROOT:
				enterOuterAlt(_localctx, 2);
				{
				setState(766);
				match(VALUE_SMART_ROOT);
				setState(767);
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
		enterRule(_localctx, 130, RULE_elvisValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(770);
			match(VALUE_ELVIS);
			setState(771);
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
		enterRule(_localctx, 132, RULE_transformationPart);
		try {
			setState(780);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case VALUE_FUNCTION:
			case VALUE_PREDICATE:
				enterOuterAlt(_localctx, 1);
				{
				setState(773);
				functionChainType();
				setState(774);
				functionIdentifier();
				setState(776);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,96,_ctx) ) {
				case 1:
					{
					setState(775);
					valueList();
					}
					break;
				}
				}
				break;
			case VALUE_MEMBER:
				enterOuterAlt(_localctx, 2);
				{
				setState(778);
				memberIdentifier();
				}
				break;
			case VALUE_WRAP:
				enterOuterAlt(_localctx, 3);
				{
				setState(779);
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
		enterRule(_localctx, 134, RULE_functionChainType);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(782);
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
		enterRule(_localctx, 136, RULE_labelIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(784);
			match(LABEL_PREFIX);
			setState(785);
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
		enterRule(_localctx, 138, RULE_memberIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(787);
			match(VALUE_MEMBER);
			setState(788);
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
		enterRule(_localctx, 140, RULE_mixinIdentifier);
		try {
			setState(793);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(790);
				match(IDENTIFIER);
				}
				break;
			case NAMESPACE_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(791);
				match(NAMESPACE_IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 3);
				{
				setState(792);
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
		enterRule(_localctx, 142, RULE_functionDeclarationIdentifier);
		try {
			setState(797);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(795);
				match(IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(796);
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
		enterRule(_localctx, 144, RULE_variableIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(799);
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
		enterRule(_localctx, 146, RULE_functionIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(801);
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
		enterRule(_localctx, 148, RULE_kindIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(803);
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
		enterRule(_localctx, 150, RULE_expressionModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(805);
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
		enterRule(_localctx, 152, RULE_mixinModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(807);
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
		enterRule(_localctx, 154, RULE_variableSpecifiers);
		int _la;
		try {
			setState(817);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_LOCAL:
			case KEYWORD_CARRY:
				enterOuterAlt(_localctx, 1);
				{
				setState(810);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(809);
					match(KEYWORD_CARRY);
					}
				}

				setState(812);
				match(KEYWORD_LOCAL);
				}
				break;
			case KEYWORD_TARGET:
			case KEYWORD_VAR:
				enterOuterAlt(_localctx, 2);
				{
				{
				setState(814);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_TARGET) {
					{
					setState(813);
					match(KEYWORD_TARGET);
					}
				}

				setState(816);
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
		enterRule(_localctx, 156, RULE_funcModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(819);
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
		enterRule(_localctx, 158, RULE_trivia);
		try {
			setState(823);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case COMMENT:
			case SLASH_COMMENT:
				enterOuterAlt(_localctx, 1);
				{
				setState(821);
				comment();
				}
				break;
			case NEWLINE:
				enterOuterAlt(_localctx, 2);
				{
				setState(822);
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
		enterRule(_localctx, 160, RULE_comment);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(825);
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
		enterRule(_localctx, 162, RULE_escaped);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(827);
			match(ESCAPE);
			setState(828);
			_la = _input.LA(1);
			if ( !(((((_la - 84)) & ~0x3f) == 0 && ((1L << (_la - 84)) & 7L) != 0)) ) {
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
		case 48:
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
		"\u0004\u0001[\u033f\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
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
		"P\u0007P\u0002Q\u0007Q\u0001\u0000\u0005\u0000\u00a6\b\u0000\n\u0000\f"+
		"\u0000\u00a9\t\u0000\u0001\u0000\u0003\u0000\u00ac\b\u0000\u0001\u0000"+
		"\u0001\u0000\u0005\u0000\u00b0\b\u0000\n\u0000\f\u0000\u00b3\t\u0000\u0005"+
		"\u0000\u00b5\b\u0000\n\u0000\f\u0000\u00b8\t\u0000\u0001\u0000\u0001\u0000"+
		"\u0001\u0001\u0001\u0001\u0005\u0001\u00be\b\u0001\n\u0001\f\u0001\u00c1"+
		"\t\u0001\u0001\u0001\u0001\u0001\u0005\u0001\u00c5\b\u0001\n\u0001\f\u0001"+
		"\u00c8\t\u0001\u0001\u0002\u0001\u0002\u0005\u0002\u00cc\b\u0002\n\u0002"+
		"\f\u0002\u00cf\t\u0002\u0001\u0002\u0001\u0002\u0001\u0002\u0003\u0002"+
		"\u00d4\b\u0002\u0001\u0002\u0001\u0002\u0001\u0002\u0003\u0002\u00d9\b"+
		"\u0002\u0001\u0003\u0001\u0003\u0005\u0003\u00dd\b\u0003\n\u0003\f\u0003"+
		"\u00e0\t\u0003\u0001\u0003\u0005\u0003\u00e3\b\u0003\n\u0003\f\u0003\u00e6"+
		"\t\u0003\u0001\u0004\u0001\u0004\u0001\u0004\u0003\u0004\u00eb\b\u0004"+
		"\u0001\u0004\u0003\u0004\u00ee\b\u0004\u0001\u0005\u0001\u0005\u0001\u0005"+
		"\u0001\u0005\u0001\u0006\u0005\u0006\u00f5\b\u0006\n\u0006\f\u0006\u00f8"+
		"\t\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0007\u0001"+
		"\u0007\u0001\u0007\u0001\u0007\u0001\u0007\u0001\b\u0001\b\u0001\b\u0001"+
		"\b\u0005\b\u0107\b\b\n\b\f\b\u010a\t\b\u0001\b\u0001\b\u0001\t\u0005\t"+
		"\u010f\b\t\n\t\f\t\u0112\t\t\u0001\t\u0001\t\u0001\t\u0001\n\u0005\n\u0118"+
		"\b\n\n\n\f\n\u011b\t\n\u0001\n\u0001\n\u0001\n\u0003\n\u0120\b\n\u0001"+
		"\n\u0001\n\u0001\n\u0001\u000b\u0001\u000b\u0001\u000b\u0001\u000b\u0001"+
		"\f\u0003\f\u012a\b\f\u0001\f\u0001\f\u0001\f\u0001\f\u0003\f\u0130\b\f"+
		"\u0003\f\u0132\b\f\u0001\r\u0001\r\u0005\r\u0136\b\r\n\r\f\r\u0139\t\r"+
		"\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000f"+
		"\u0001\u000f\u0003\u000f\u0142\b\u000f\u0001\u000f\u0003\u000f\u0145\b"+
		"\u000f\u0001\u0010\u0001\u0010\u0001\u0010\u0001\u0010\u0003\u0010\u014b"+
		"\b\u0010\u0001\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0001"+
		"\u0011\u0005\u0011\u0153\b\u0011\n\u0011\f\u0011\u0156\t\u0011\u0001\u0011"+
		"\u0003\u0011\u0159\b\u0011\u0001\u0011\u0001\u0011\u0003\u0011\u015d\b"+
		"\u0011\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0001"+
		"\u0012\u0005\u0012\u0165\b\u0012\n\u0012\f\u0012\u0168\t\u0012\u0001\u0012"+
		"\u0003\u0012\u016b\b\u0012\u0001\u0012\u0001\u0012\u0003\u0012\u016f\b"+
		"\u0012\u0001\u0013\u0001\u0013\u0001\u0013\u0001\u0013\u0001\u0013\u0001"+
		"\u0014\u0001\u0014\u0001\u0014\u0001\u0014\u0005\u0014\u017a\b\u0014\n"+
		"\u0014\f\u0014\u017d\t\u0014\u0001\u0014\u0003\u0014\u0180\b\u0014\u0001"+
		"\u0014\u0001\u0014\u0001\u0014\u0001\u0014\u0001\u0014\u0003\u0014\u0187"+
		"\b\u0014\u0001\u0015\u0003\u0015\u018a\b\u0015\u0001\u0015\u0001\u0015"+
		"\u0003\u0015\u018e\b\u0015\u0001\u0016\u0001\u0016\u0001\u0017\u0001\u0017"+
		"\u0003\u0017\u0194\b\u0017\u0001\u0018\u0001\u0018\u0001\u0018\u0001\u0018"+
		"\u0001\u0018\u0001\u0018\u0005\u0018\u019c\b\u0018\n\u0018\f\u0018\u019f"+
		"\t\u0018\u0001\u0018\u0003\u0018\u01a2\b\u0018\u0001\u0018\u0001\u0018"+
		"\u0003\u0018\u01a6\b\u0018\u0001\u0019\u0005\u0019\u01a9\b\u0019\n\u0019"+
		"\f\u0019\u01ac\t\u0019\u0001\u0019\u0003\u0019\u01af\b\u0019\u0001\u0019"+
		"\u0001\u0019\u0001\u0019\u0001\u0019\u0001\u001a\u0001\u001a\u0001\u001a"+
		"\u0001\u001a\u0005\u001a\u01b9\b\u001a\n\u001a\f\u001a\u01bc\t\u001a\u0001"+
		"\u001a\u0001\u001a\u0001\u001b\u0001\u001b\u0001\u001b\u0001\u001b\u0003"+
		"\u001b\u01c4\b\u001b\u0001\u001b\u0001\u001b\u0001\u001b\u0001\u001b\u0001"+
		"\u001b\u0001\u001b\u0001\u001b\u0003\u001b\u01cd\b\u001b\u0001\u001c\u0001"+
		"\u001c\u0001\u001c\u0003\u001c\u01d2\b\u001c\u0001\u001c\u0001\u001c\u0003"+
		"\u001c\u01d6\b\u001c\u0001\u001d\u0001\u001d\u0001\u001d\u0001\u001d\u0005"+
		"\u001d\u01dc\b\u001d\n\u001d\f\u001d\u01df\t\u001d\u0001\u001d\u0001\u001d"+
		"\u0001\u001d\u0001\u001d\u0003\u001d\u01e5\b\u001d\u0001\u001e\u0001\u001e"+
		"\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001f\u0001\u001f\u0003\u001f"+
		"\u01ee\b\u001f\u0001 \u0001 \u0001!\u0001!\u0001!\u0001!\u0001!\u0001"+
		"!\u0001\"\u0001\"\u0001\"\u0005\"\u01fb\b\"\n\"\f\"\u01fe\t\"\u0001\""+
		"\u0003\"\u0201\b\"\u0001#\u0001#\u0001#\u0001#\u0001#\u0001$\u0001$\u0003"+
		"$\u020a\b$\u0001$\u0001$\u0001$\u0001$\u0001$\u0001%\u0005%\u0212\b%\n"+
		"%\f%\u0215\t%\u0001%\u0001%\u0001%\u0005%\u021a\b%\n%\f%\u021d\t%\u0001"+
		"%\u0003%\u0220\b%\u0001&\u0001&\u0001&\u0001&\u0001&\u0001\'\u0001\'\u0003"+
		"\'\u0229\b\'\u0001(\u0001(\u0001(\u0001(\u0001)\u0001)\u0001)\u0001)\u0001"+
		")\u0003)\u0234\b)\u0001)\u0003)\u0237\b)\u0001*\u0001*\u0003*\u023b\b"+
		"*\u0001*\u0001*\u0001*\u0001*\u0003*\u0241\b*\u0001+\u0001+\u0001+\u0001"+
		"+\u0001,\u0001,\u0001,\u0001,\u0004,\u024b\b,\u000b,\f,\u024c\u0001-\u0001"+
		"-\u0001-\u0001-\u0003-\u0253\b-\u0001.\u0001.\u0003.\u0257\b.\u0001/\u0001"+
		"/\u0001/\u0001/\u00010\u00010\u00010\u00030\u0260\b0\u00010\u00010\u0001"+
		"0\u00030\u0265\b0\u00010\u00010\u00050\u0269\b0\n0\f0\u026c\t0\u00011"+
		"\u00011\u00011\u00011\u00011\u00011\u00011\u00011\u00011\u00011\u0003"+
		"1\u0278\b1\u00012\u00012\u00012\u00012\u00052\u027e\b2\n2\f2\u0281\t2"+
		"\u00012\u00012\u00012\u00012\u00032\u0287\b2\u00032\u0289\b2\u00013\u0001"+
		"3\u00014\u00014\u00015\u00015\u00015\u00016\u00036\u0293\b6\u00016\u0001"+
		"6\u00036\u0297\b6\u00017\u00017\u00017\u00017\u00017\u00017\u00057\u029f"+
		"\b7\n7\f7\u02a2\t7\u00017\u00037\u02a5\b7\u00017\u00017\u00037\u02a9\b"+
		"7\u00018\u00018\u00018\u00018\u00018\u00018\u00058\u02b1\b8\n8\f8\u02b4"+
		"\t8\u00018\u00038\u02b7\b8\u00018\u00018\u00038\u02bb\b8\u00019\u0005"+
		"9\u02be\b9\n9\f9\u02c1\t9\u00019\u00019\u00019\u00019\u0001:\u0001:\u0001"+
		":\u0001:\u0005:\u02cb\b:\n:\f:\u02ce\t:\u0001:\u0003:\u02d1\b:\u0001:"+
		"\u0001:\u0001:\u0001:\u0001:\u0001:\u0004:\u02d9\b:\u000b:\f:\u02da\u0003"+
		":\u02dd\b:\u0001;\u0001;\u0001;\u0001;\u0001<\u0001<\u0001<\u0005<\u02e6"+
		"\b<\n<\f<\u02e9\t<\u0001=\u0001=\u0001=\u0001=\u0001>\u0001>\u0004>\u02f1"+
		"\b>\u000b>\f>\u02f2\u0001>\u0001>\u0001?\u0001?\u0005?\u02f9\b?\n?\f?"+
		"\u02fc\t?\u0001@\u0001@\u0001@\u0003@\u0301\b@\u0001A\u0001A\u0001A\u0001"+
		"B\u0001B\u0001B\u0003B\u0309\bB\u0001B\u0001B\u0003B\u030d\bB\u0001C\u0001"+
		"C\u0001D\u0001D\u0001D\u0001E\u0001E\u0001E\u0001F\u0001F\u0001F\u0003"+
		"F\u031a\bF\u0001G\u0001G\u0003G\u031e\bG\u0001H\u0001H\u0001I\u0001I\u0001"+
		"J\u0001J\u0001K\u0001K\u0001L\u0001L\u0001M\u0003M\u032b\bM\u0001M\u0001"+
		"M\u0003M\u032f\bM\u0001M\u0003M\u0332\bM\u0001N\u0001N\u0001O\u0001O\u0003"+
		"O\u0338\bO\u0001P\u0001P\u0001Q\u0001Q\u0001Q\u0001Q\u0000\u0001`R\u0000"+
		"\u0002\u0004\u0006\b\n\f\u000e\u0010\u0012\u0014\u0016\u0018\u001a\u001c"+
		"\u001e \"$&(*,.02468:<>@BDFHJLNPRTVXZ\\^`bdfhjlnprtvxz|~\u0080\u0082\u0084"+
		"\u0086\u0088\u008a\u008c\u008e\u0090\u0092\u0094\u0096\u0098\u009a\u009c"+
		"\u009e\u00a0\u00a2\u0000\b\u0003\u0000\n\n\r\r\u0017\u0017\u0002\u0000"+
		"\r\r\u0015\u0015\u0001\u0000FG\u0002\u0000\r\rXX\u0002\u0000))99\u0001"+
		"\u000068\u0001\u0000\u001c\u001d\u0001\u0000TV\u0376\u0000\u00a7\u0001"+
		"\u0000\u0000\u0000\u0002\u00bb\u0001\u0000\u0000\u0000\u0004\u00d8\u0001"+
		"\u0000\u0000\u0000\u0006\u00da\u0001\u0000\u0000\u0000\b\u00ed\u0001\u0000"+
		"\u0000\u0000\n\u00ef\u0001\u0000\u0000\u0000\f\u00f6\u0001\u0000\u0000"+
		"\u0000\u000e\u00fd\u0001\u0000\u0000\u0000\u0010\u0102\u0001\u0000\u0000"+
		"\u0000\u0012\u0110\u0001\u0000\u0000\u0000\u0014\u0119\u0001\u0000\u0000"+
		"\u0000\u0016\u0124\u0001\u0000\u0000\u0000\u0018\u0131\u0001\u0000\u0000"+
		"\u0000\u001a\u0137\u0001\u0000\u0000\u0000\u001c\u013a\u0001\u0000\u0000"+
		"\u0000\u001e\u0144\u0001\u0000\u0000\u0000 \u014a\u0001\u0000\u0000\u0000"+
		"\"\u015c\u0001\u0000\u0000\u0000$\u016e\u0001\u0000\u0000\u0000&\u0170"+
		"\u0001\u0000\u0000\u0000(\u0186\u0001\u0000\u0000\u0000*\u0189\u0001\u0000"+
		"\u0000\u0000,\u018f\u0001\u0000\u0000\u0000.\u0193\u0001\u0000\u0000\u0000"+
		"0\u01a5\u0001\u0000\u0000\u00002\u01aa\u0001\u0000\u0000\u00004\u01b4"+
		"\u0001\u0000\u0000\u00006\u01cc\u0001\u0000\u0000\u00008\u01d5\u0001\u0000"+
		"\u0000\u0000:\u01d7\u0001\u0000\u0000\u0000<\u01e6\u0001\u0000\u0000\u0000"+
		">\u01ed\u0001\u0000\u0000\u0000@\u01ef\u0001\u0000\u0000\u0000B\u01f1"+
		"\u0001\u0000\u0000\u0000D\u01f7\u0001\u0000\u0000\u0000F\u0202\u0001\u0000"+
		"\u0000\u0000H\u0207\u0001\u0000\u0000\u0000J\u0213\u0001\u0000\u0000\u0000"+
		"L\u0221\u0001\u0000\u0000\u0000N\u0228\u0001\u0000\u0000\u0000P\u022a"+
		"\u0001\u0000\u0000\u0000R\u0236\u0001\u0000\u0000\u0000T\u0240\u0001\u0000"+
		"\u0000\u0000V\u0242\u0001\u0000\u0000\u0000X\u024a\u0001\u0000\u0000\u0000"+
		"Z\u024e\u0001\u0000\u0000\u0000\\\u0256\u0001\u0000\u0000\u0000^\u0258"+
		"\u0001\u0000\u0000\u0000`\u0264\u0001\u0000\u0000\u0000b\u0277\u0001\u0000"+
		"\u0000\u0000d\u0288\u0001\u0000\u0000\u0000f\u028a\u0001\u0000\u0000\u0000"+
		"h\u028c\u0001\u0000\u0000\u0000j\u028e\u0001\u0000\u0000\u0000l\u0296"+
		"\u0001\u0000\u0000\u0000n\u02a8\u0001\u0000\u0000\u0000p\u02ba\u0001\u0000"+
		"\u0000\u0000r\u02bf\u0001\u0000\u0000\u0000t\u02dc\u0001\u0000\u0000\u0000"+
		"v\u02de\u0001\u0000\u0000\u0000x\u02e7\u0001\u0000\u0000\u0000z\u02ea"+
		"\u0001\u0000\u0000\u0000|\u02ee\u0001\u0000\u0000\u0000~\u02f6\u0001\u0000"+
		"\u0000\u0000\u0080\u0300\u0001\u0000\u0000\u0000\u0082\u0302\u0001\u0000"+
		"\u0000\u0000\u0084\u030c\u0001\u0000\u0000\u0000\u0086\u030e\u0001\u0000"+
		"\u0000\u0000\u0088\u0310\u0001\u0000\u0000\u0000\u008a\u0313\u0001\u0000"+
		"\u0000\u0000\u008c\u0319\u0001\u0000\u0000\u0000\u008e\u031d\u0001\u0000"+
		"\u0000\u0000\u0090\u031f\u0001\u0000\u0000\u0000\u0092\u0321\u0001\u0000"+
		"\u0000\u0000\u0094\u0323\u0001\u0000\u0000\u0000\u0096\u0325\u0001\u0000"+
		"\u0000\u0000\u0098\u0327\u0001\u0000\u0000\u0000\u009a\u0331\u0001\u0000"+
		"\u0000\u0000\u009c\u0333\u0001\u0000\u0000\u0000\u009e\u0337\u0001\u0000"+
		"\u0000\u0000\u00a0\u0339\u0001\u0000\u0000\u0000\u00a2\u033b\u0001\u0000"+
		"\u0000\u0000\u00a4\u00a6\u0003\u009eO\u0000\u00a5\u00a4\u0001\u0000\u0000"+
		"\u0000\u00a6\u00a9\u0001\u0000\u0000\u0000\u00a7\u00a5\u0001\u0000\u0000"+
		"\u0000\u00a7\u00a8\u0001\u0000\u0000\u0000\u00a8\u00ab\u0001\u0000\u0000"+
		"\u0000\u00a9\u00a7\u0001\u0000\u0000\u0000\u00aa\u00ac\u0003\u0002\u0001"+
		"\u0000\u00ab\u00aa\u0001\u0000\u0000\u0000\u00ab\u00ac\u0001\u0000\u0000"+
		"\u0000\u00ac\u00b6\u0001\u0000\u0000\u0000\u00ad\u00b1\u0003\u0004\u0002"+
		"\u0000\u00ae\u00b0\u0003\u009eO\u0000\u00af\u00ae\u0001\u0000\u0000\u0000"+
		"\u00b0\u00b3\u0001\u0000\u0000\u0000\u00b1\u00af\u0001\u0000\u0000\u0000"+
		"\u00b1\u00b2\u0001\u0000\u0000\u0000\u00b2\u00b5\u0001\u0000\u0000\u0000"+
		"\u00b3\u00b1\u0001\u0000\u0000\u0000\u00b4\u00ad\u0001\u0000\u0000\u0000"+
		"\u00b5\u00b8\u0001\u0000\u0000\u0000\u00b6\u00b4\u0001\u0000\u0000\u0000"+
		"\u00b6\u00b7\u0001\u0000\u0000\u0000\u00b7\u00b9\u0001\u0000\u0000\u0000"+
		"\u00b8\u00b6\u0001\u0000\u0000\u0000\u00b9\u00ba\u0005\u0000\u0000\u0001"+
		"\u00ba\u0001\u0001\u0000\u0000\u0000\u00bb\u00bf\u0003\u0006\u0003\u0000"+
		"\u00bc\u00be\u0003\u009eO\u0000\u00bd\u00bc\u0001\u0000\u0000\u0000\u00be"+
		"\u00c1\u0001\u0000\u0000\u0000\u00bf\u00bd\u0001\u0000\u0000\u0000\u00bf"+
		"\u00c0\u0001\u0000\u0000\u0000\u00c0\u00c2\u0001\u0000\u0000\u0000\u00c1"+
		"\u00bf\u0001\u0000\u0000\u0000\u00c2\u00c6\u0005\u001e\u0000\u0000\u00c3"+
		"\u00c5\u0003\u009eO\u0000\u00c4\u00c3\u0001\u0000\u0000\u0000\u00c5\u00c8"+
		"\u0001\u0000\u0000\u0000\u00c6\u00c4\u0001\u0000\u0000\u0000\u00c6\u00c7"+
		"\u0001\u0000\u0000\u0000\u00c7\u0003\u0001\u0000\u0000\u0000\u00c8\u00c6"+
		"\u0001\u0000\u0000\u0000\u00c9\u00cd\u0003\u0006\u0003\u0000\u00ca\u00cc"+
		"\u0003\u009eO\u0000\u00cb\u00ca\u0001\u0000\u0000\u0000\u00cc\u00cf\u0001"+
		"\u0000\u0000\u0000\u00cd\u00cb\u0001\u0000\u0000\u0000\u00cd\u00ce\u0001"+
		"\u0000\u0000\u0000\u00ce\u00d3\u0001\u0000\u0000\u0000\u00cf\u00cd\u0001"+
		"\u0000\u0000\u0000\u00d0\u00d4\u0003\f\u0006\u0000\u00d1\u00d4\u0003\u0014"+
		"\n\u0000\u00d2\u00d4\u0003\u000e\u0007\u0000\u00d3\u00d0\u0001\u0000\u0000"+
		"\u0000\u00d3\u00d1\u0001\u0000\u0000\u0000\u00d3\u00d2\u0001\u0000\u0000"+
		"\u0000\u00d4\u00d9\u0001\u0000\u0000\u0000\u00d5\u00d9\u0003\f\u0006\u0000"+
		"\u00d6\u00d9\u0003\u0014\n\u0000\u00d7\u00d9\u0003\u000e\u0007\u0000\u00d8"+
		"\u00c9\u0001\u0000\u0000\u0000\u00d8\u00d5\u0001\u0000\u0000\u0000\u00d8"+
		"\u00d6\u0001\u0000\u0000\u0000\u00d8\u00d7\u0001\u0000\u0000\u0000\u00d9"+
		"\u0005\u0001\u0000\u0000\u0000\u00da\u00e4\u0003\b\u0004\u0000\u00db\u00dd"+
		"\u0003\u009eO\u0000\u00dc\u00db\u0001\u0000\u0000\u0000\u00dd\u00e0\u0001"+
		"\u0000\u0000\u0000\u00de\u00dc\u0001\u0000\u0000\u0000\u00de\u00df\u0001"+
		"\u0000\u0000\u0000\u00df\u00e1\u0001\u0000\u0000\u0000\u00e0\u00de\u0001"+
		"\u0000\u0000\u0000\u00e1\u00e3\u0003\b\u0004\u0000\u00e2\u00de\u0001\u0000"+
		"\u0000\u0000\u00e3\u00e6\u0001\u0000\u0000\u0000\u00e4\u00e2\u0001\u0000"+
		"\u0000\u0000\u00e4\u00e5\u0001\u0000\u0000\u0000\u00e5\u0007\u0001\u0000"+
		"\u0000\u0000\u00e6\u00e4\u0001\u0000\u0000\u0000\u00e7\u00e8\u0005\u0013"+
		"\u0000\u0000\u00e8\u00ea\u0005\n\u0000\u0000\u00e9\u00eb\u0003t:\u0000"+
		"\u00ea\u00e9\u0001\u0000\u0000\u0000\u00ea\u00eb\u0001\u0000\u0000\u0000"+
		"\u00eb\u00ee\u0001\u0000\u0000\u0000\u00ec\u00ee\u0003\n\u0005\u0000\u00ed"+
		"\u00e7\u0001\u0000\u0000\u0000\u00ed\u00ec\u0001\u0000\u0000\u0000\u00ee"+
		"\t\u0001\u0000\u0000\u0000\u00ef\u00f0\u0005\u0012\u0000\u0000\u00f0\u00f1"+
		"\u0003\\.\u0000\u00f1\u00f2\u0005L\u0000\u0000\u00f2\u000b\u0001\u0000"+
		"\u0000\u0000\u00f3\u00f5\u0003\u0098L\u0000\u00f4\u00f3\u0001\u0000\u0000"+
		"\u0000\u00f5\u00f8\u0001\u0000\u0000\u0000\u00f6\u00f4\u0001\u0000\u0000"+
		"\u0000\u00f6\u00f7\u0001\u0000\u0000\u0000\u00f7\u00f9\u0001\u0000\u0000"+
		"\u0000\u00f8\u00f6\u0001\u0000\u0000\u0000\u00f9\u00fa\u0005(\u0000\u0000"+
		"\u00fa\u00fb\u0003\u008cF\u0000\u00fb\u00fc\u0003\u0010\b\u0000\u00fc"+
		"\r\u0001\u0000\u0000\u0000\u00fd\u00fe\u0005%\u0000\u0000\u00fe\u00ff"+
		"\u0005\n\u0000\u0000\u00ff\u0100\u0005;\u0000\u0000\u0100\u0101\u0003"+
		"\u001e\u000f\u0000\u0101\u000f\u0001\u0000\u0000\u0000\u0102\u0108\u0005"+
		"\u001f\u0000\u0000\u0103\u0107\u0003\u0012\t\u0000\u0104\u0107\u0003\u0014"+
		"\n\u0000\u0105\u0107\u0003\u009eO\u0000\u0106\u0103\u0001\u0000\u0000"+
		"\u0000\u0106\u0104\u0001\u0000\u0000\u0000\u0106\u0105\u0001\u0000\u0000"+
		"\u0000\u0107\u010a\u0001\u0000\u0000\u0000\u0108\u0106\u0001\u0000\u0000"+
		"\u0000\u0108\u0109\u0001\u0000\u0000\u0000\u0109\u010b\u0001\u0000\u0000"+
		"\u0000\u010a\u0108\u0001\u0000\u0000\u0000\u010b\u010c\u0005 \u0000\u0000"+
		"\u010c\u0011\u0001\u0000\u0000\u0000\u010d\u010f\u0003\u0096K\u0000\u010e"+
		"\u010d\u0001\u0000\u0000\u0000\u010f\u0112\u0001\u0000\u0000\u0000\u0110"+
		"\u010e\u0001\u0000\u0000\u0000\u0110\u0111\u0001\u0000\u0000\u0000\u0111"+
		"\u0113\u0001\u0000\u0000\u0000\u0112\u0110\u0001\u0000\u0000\u0000\u0113"+
		"\u0114\u0005\'\u0000\u0000\u0114\u0115\u00034\u001a\u0000\u0115\u0013"+
		"\u0001\u0000\u0000\u0000\u0116\u0118\u0003\u009cN\u0000\u0117\u0116\u0001"+
		"\u0000\u0000\u0000\u0118\u011b\u0001\u0000\u0000\u0000\u0119\u0117\u0001"+
		"\u0000\u0000\u0000\u0119\u011a\u0001\u0000\u0000\u0000\u011a\u011c\u0001"+
		"\u0000\u0000\u0000\u011b\u0119\u0001\u0000\u0000\u0000\u011c\u011d\u0005"+
		"#\u0000\u0000\u011d\u011f\u0003\u008eG\u0000\u011e\u0120\u0003\u0016\u000b"+
		"\u0000\u011f\u011e\u0001\u0000\u0000\u0000\u011f\u0120\u0001\u0000\u0000"+
		"\u0000\u0120\u0121\u0001\u0000\u0000\u0000\u0121\u0122\u0003\u001a\r\u0000"+
		"\u0122\u0123\u0003\u0018\f\u0000\u0123\u0015\u0001\u0000\u0000\u0000\u0124"+
		"\u0125\u0003(\u0014\u0000\u0125\u0126\u0005<\u0000\u0000\u0126\u0127\u0003"+
		"\u001e\u000f\u0000\u0127\u0017\u0001\u0000\u0000\u0000\u0128\u012a\u0005"+
		"&\u0000\u0000\u0129\u0128\u0001\u0000\u0000\u0000\u0129\u012a\u0001\u0000"+
		"\u0000\u0000\u012a\u012b\u0001\u0000\u0000\u0000\u012b\u0132\u00034\u001a"+
		"\u0000\u012c\u012d\u0005\u0014\u0000\u0000\u012d\u012f\u0003\\.\u0000"+
		"\u012e\u0130\u0005\f\u0000\u0000\u012f\u012e\u0001\u0000\u0000\u0000\u012f"+
		"\u0130\u0001\u0000\u0000\u0000\u0130\u0132\u0001\u0000\u0000\u0000\u0131"+
		"\u0129\u0001\u0000\u0000\u0000\u0131\u012c\u0001\u0000\u0000\u0000\u0132"+
		"\u0019\u0001\u0000\u0000\u0000\u0133\u0136\u0003\u001c\u000e\u0000\u0134"+
		"\u0136\u0005\u001b\u0000\u0000\u0135\u0133\u0001\u0000\u0000\u0000\u0135"+
		"\u0134\u0001\u0000\u0000\u0000\u0136\u0139\u0001\u0000\u0000\u0000\u0137"+
		"\u0135\u0001\u0000\u0000\u0000\u0137\u0138\u0001\u0000\u0000\u0000\u0138"+
		"\u001b\u0001\u0000\u0000\u0000\u0139\u0137\u0001\u0000\u0000\u0000\u013a"+
		"\u013b\u00054\u0000\u0000\u013b\u013c\u0003.\u0017\u0000\u013c\u013d\u0005"+
		"<\u0000\u0000\u013d\u013e\u0003.\u0017\u0000\u013e\u001d\u0001\u0000\u0000"+
		"\u0000\u013f\u0141\u0003\u0006\u0003\u0000\u0140\u0142\u0003 \u0010\u0000"+
		"\u0141\u0140\u0001\u0000\u0000\u0000\u0141\u0142\u0001\u0000\u0000\u0000"+
		"\u0142\u0145\u0001\u0000\u0000\u0000\u0143\u0145\u0003 \u0010\u0000\u0144"+
		"\u013f\u0001\u0000\u0000\u0000\u0144\u0143\u0001\u0000\u0000\u0000\u0145"+
		"\u001f\u0001\u0000\u0000\u0000\u0146\u014b\u0003,\u0016\u0000\u0147\u014b"+
		"\u0003\"\u0011\u0000\u0148\u014b\u0003$\u0012\u0000\u0149\u014b\u0003"+
		"&\u0013\u0000\u014a\u0146\u0001\u0000\u0000\u0000\u014a\u0147\u0001\u0000"+
		"\u0000\u0000\u014a\u0148\u0001\u0000\u0000\u0000\u014a\u0149\u0001\u0000"+
		"\u0000\u0000\u014b!\u0001\u0000\u0000\u0000\u014c\u014d\u0005\u000e\u0000"+
		"\u0000\u014d\u015d\u0005 \u0000\u0000\u014e\u014f\u0005\u000e\u0000\u0000"+
		"\u014f\u0154\u0003*\u0015\u0000\u0150\u0151\u0005J\u0000\u0000\u0151\u0153"+
		"\u0003*\u0015\u0000\u0152\u0150\u0001\u0000\u0000\u0000\u0153\u0156\u0001"+
		"\u0000\u0000\u0000\u0154\u0152\u0001\u0000\u0000\u0000\u0154\u0155\u0001"+
		"\u0000\u0000\u0000\u0155\u0158\u0001\u0000\u0000\u0000\u0156\u0154\u0001"+
		"\u0000\u0000\u0000\u0157\u0159\u0005J\u0000\u0000\u0158\u0157\u0001\u0000"+
		"\u0000\u0000\u0158\u0159\u0001\u0000\u0000\u0000\u0159\u015a\u0001\u0000"+
		"\u0000\u0000\u015a\u015b\u0005 \u0000\u0000\u015b\u015d\u0001\u0000\u0000"+
		"\u0000\u015c\u014c\u0001\u0000\u0000\u0000\u015c\u014e\u0001\u0000\u0000"+
		"\u0000\u015d#\u0001\u0000\u0000\u0000\u015e\u015f\u0005\u000f\u0000\u0000"+
		"\u015f\u016f\u0005L\u0000\u0000\u0160\u0161\u0005\u000f\u0000\u0000\u0161"+
		"\u0166\u0003*\u0015\u0000\u0162\u0163\u0005J\u0000\u0000\u0163\u0165\u0003"+
		"*\u0015\u0000\u0164\u0162\u0001\u0000\u0000\u0000\u0165\u0168\u0001\u0000"+
		"\u0000\u0000\u0166\u0164\u0001\u0000\u0000\u0000\u0166\u0167\u0001\u0000"+
		"\u0000\u0000\u0167\u016a\u0001\u0000\u0000\u0000\u0168\u0166\u0001\u0000"+
		"\u0000\u0000\u0169\u016b\u0005J\u0000\u0000\u016a\u0169\u0001\u0000\u0000"+
		"\u0000\u016a\u016b\u0001\u0000\u0000\u0000\u016b\u016c\u0001\u0000\u0000"+
		"\u0000\u016c\u016d\u0005L\u0000\u0000\u016d\u016f\u0001\u0000\u0000\u0000"+
		"\u016e\u015e\u0001\u0000\u0000\u0000\u016e\u0160\u0001\u0000\u0000\u0000"+
		"\u016f%\u0001\u0000\u0000\u0000\u0170\u0171\u0005$\u0000\u0000\u0171\u0172"+
		"\u0003(\u0014\u0000\u0172\u0173\u0005<\u0000\u0000\u0173\u0174\u0003\u001e"+
		"\u000f\u0000\u0174\'\u0001\u0000\u0000\u0000\u0175\u0176\u0005?\u0000"+
		"\u0000\u0176\u017b\u0003*\u0015\u0000\u0177\u0178\u0005J\u0000\u0000\u0178"+
		"\u017a\u0003*\u0015\u0000\u0179\u0177\u0001\u0000\u0000\u0000\u017a\u017d"+
		"\u0001\u0000\u0000\u0000\u017b\u0179\u0001\u0000\u0000\u0000\u017b\u017c"+
		"\u0001\u0000\u0000\u0000\u017c\u017f\u0001\u0000\u0000\u0000\u017d\u017b"+
		"\u0001\u0000\u0000\u0000\u017e\u0180\u0005J\u0000\u0000\u017f\u017e\u0001"+
		"\u0000\u0000\u0000\u017f\u0180\u0001\u0000\u0000\u0000\u0180\u0181\u0001"+
		"\u0000\u0000\u0000\u0181\u0182\u0005K\u0000\u0000\u0182\u0187\u0001\u0000"+
		"\u0000\u0000\u0183\u0184\u0005?\u0000\u0000\u0184\u0187\u0005K\u0000\u0000"+
		"\u0185\u0187\u0005>\u0000\u0000\u0186\u0175\u0001\u0000\u0000\u0000\u0186"+
		"\u0183\u0001\u0000\u0000\u0000\u0186\u0185\u0001\u0000\u0000\u0000\u0187"+
		")\u0001\u0000\u0000\u0000\u0188\u018a\u0003\u0006\u0003\u0000\u0189\u0188"+
		"\u0001\u0000\u0000\u0000\u0189\u018a\u0001\u0000\u0000\u0000\u018a\u018b"+
		"\u0001\u0000\u0000\u0000\u018b\u018d\u0003 \u0010\u0000\u018c\u018e\u0005"+
		"\r\u0000\u0000\u018d\u018c\u0001\u0000\u0000\u0000\u018d\u018e\u0001\u0000"+
		"\u0000\u0000\u018e+\u0001\u0000\u0000\u0000\u018f\u0190\u0007\u0000\u0000"+
		"\u0000\u0190-\u0001\u0000\u0000\u0000\u0191\u0194\u00030\u0018\u0000\u0192"+
		"\u0194\u0003\u0094J\u0000\u0193\u0191\u0001\u0000\u0000\u0000\u0193\u0192"+
		"\u0001\u0000\u0000\u0000\u0194/\u0001\u0000\u0000\u0000\u0195\u0196\u0005"+
		"\u000e\u0000\u0000\u0196\u01a6\u0005 \u0000\u0000\u0197\u0198\u0005\u000e"+
		"\u0000\u0000\u0198\u019d\u00032\u0019\u0000\u0199\u019a\u0005J\u0000\u0000"+
		"\u019a\u019c\u00032\u0019\u0000\u019b\u0199\u0001\u0000\u0000\u0000\u019c"+
		"\u019f\u0001\u0000\u0000\u0000\u019d\u019b\u0001\u0000\u0000\u0000\u019d"+
		"\u019e\u0001\u0000\u0000\u0000\u019e\u01a1\u0001\u0000\u0000\u0000\u019f"+
		"\u019d\u0001\u0000\u0000\u0000\u01a0\u01a2\u0005J\u0000\u0000\u01a1\u01a0"+
		"\u0001\u0000\u0000\u0000\u01a1\u01a2\u0001\u0000\u0000\u0000\u01a2\u01a3"+
		"\u0001\u0000\u0000\u0000\u01a3\u01a4\u0005 \u0000\u0000\u01a4\u01a6\u0001"+
		"\u0000\u0000\u0000\u01a5\u0195\u0001\u0000\u0000\u0000\u01a5\u0197\u0001"+
		"\u0000\u0000\u0000\u01a61\u0001\u0000\u0000\u0000\u01a7\u01a9\u0003\b"+
		"\u0004\u0000\u01a8\u01a7\u0001\u0000\u0000\u0000\u01a9\u01ac\u0001\u0000"+
		"\u0000\u0000\u01aa\u01a8\u0001\u0000\u0000\u0000\u01aa\u01ab\u0001\u0000"+
		"\u0000\u0000\u01ab\u01ae\u0001\u0000\u0000\u0000\u01ac\u01aa\u0001\u0000"+
		"\u0000\u0000\u01ad\u01af\u0005R\u0000\u0000\u01ae\u01ad\u0001\u0000\u0000"+
		"\u0000\u01ae\u01af\u0001\u0000\u0000\u0000\u01af\u01b0\u0001\u0000\u0000"+
		"\u0000\u01b0\u01b1\u0005\r\u0000\u0000\u01b1\u01b2\u0005Q\u0000\u0000"+
		"\u01b2\u01b3\u0003\u0094J\u0000\u01b33\u0001\u0000\u0000\u0000\u01b4\u01ba"+
		"\u0005\u001f\u0000\u0000\u01b5\u01b9\u00036\u001b\u0000\u01b6\u01b9\u0005"+
		"!\u0000\u0000\u01b7\u01b9\u0003\u009eO\u0000\u01b8\u01b5\u0001\u0000\u0000"+
		"\u0000\u01b8\u01b6\u0001\u0000\u0000\u0000\u01b8\u01b7\u0001\u0000\u0000"+
		"\u0000\u01b9\u01bc\u0001\u0000\u0000\u0000\u01ba\u01b8\u0001\u0000\u0000"+
		"\u0000\u01ba\u01bb\u0001\u0000\u0000\u0000\u01bb\u01bd\u0001\u0000\u0000"+
		"\u0000\u01bc\u01ba\u0001\u0000\u0000\u0000\u01bd\u01be\u0005 \u0000\u0000"+
		"\u01be5\u0001\u0000\u0000\u0000\u01bf\u01c0\u0003\u0088D\u0000\u01c0\u01c1"+
		"\u0005\u001b\u0000\u0000\u01c1\u01cd\u0001\u0000\u0000\u0000\u01c2\u01c4"+
		"\u0003\u0088D\u0000\u01c3\u01c2\u0001\u0000\u0000\u0000\u01c3\u01c4\u0001"+
		"\u0000\u0000\u0000\u01c4\u01c5\u0001\u0000\u0000\u0000\u01c5\u01cd\u0003"+
		"4\u001a\u0000\u01c6\u01cd\u00038\u001c\u0000\u01c7\u01cd\u0003P(\u0000"+
		"\u01c8\u01cd\u0003T*\u0000\u01c9\u01cd\u0003H$\u0000\u01ca\u01cd\u0003"+
		":\u001d\u0000\u01cb\u01cd\u0003B!\u0000\u01cc\u01bf\u0001\u0000\u0000"+
		"\u0000\u01cc\u01c3\u0001\u0000\u0000\u0000\u01cc\u01c6\u0001\u0000\u0000"+
		"\u0000\u01cc\u01c7\u0001\u0000\u0000\u0000\u01cc\u01c8\u0001\u0000\u0000"+
		"\u0000\u01cc\u01c9\u0001\u0000\u0000\u0000\u01cc\u01ca\u0001\u0000\u0000"+
		"\u0000\u01cc\u01cb\u0001\u0000\u0000\u0000\u01cd7\u0001\u0000\u0000\u0000"+
		"\u01ce\u01cf\u0005\n\u0000\u0000\u01cf\u01d1\u0003t:\u0000\u01d0\u01d2"+
		"\u0003l6\u0000\u01d1\u01d0\u0001\u0000\u0000\u0000\u01d1\u01d2\u0001\u0000"+
		"\u0000\u0000\u01d2\u01d6\u0001\u0000\u0000\u0000\u01d3\u01d4\u0005\n\u0000"+
		"\u0000\u01d4\u01d6\u0003l6\u0000\u01d5\u01ce\u0001\u0000\u0000\u0000\u01d5"+
		"\u01d3\u0001\u0000\u0000\u0000\u01d69\u0001\u0000\u0000\u0000\u01d7\u01d8"+
		"\u00055\u0000\u0000\u01d8\u01d9\u0003@ \u0000\u01d9\u01e4\u00034\u001a"+
		"\u0000\u01da\u01dc\u0003\u009eO\u0000\u01db\u01da\u0001\u0000\u0000\u0000"+
		"\u01dc\u01df\u0001\u0000\u0000\u0000\u01dd\u01db\u0001\u0000\u0000\u0000"+
		"\u01dd\u01de\u0001\u0000\u0000\u0000\u01de\u01e0\u0001\u0000\u0000\u0000"+
		"\u01df\u01dd\u0001\u0000\u0000\u0000\u01e0\u01e1\u0005+\u0000\u0000\u01e1"+
		"\u01e2\u0003>\u001f\u0000\u01e2\u01e3\u0005\u001b\u0000\u0000\u01e3\u01e5"+
		"\u0001\u0000\u0000\u0000\u01e4\u01dd\u0001\u0000\u0000\u0000\u01e4\u01e5"+
		"\u0001\u0000\u0000\u0000\u01e5;\u0001\u0000\u0000\u0000\u01e6\u01e7\u0005"+
		"+\u0000\u0000\u01e7\u01e8\u0005<\u0000\u0000\u01e8\u01e9\u0003>\u001f"+
		"\u0000\u01e9\u01ea\u0005\u001b\u0000\u0000\u01ea=\u0001\u0000\u0000\u0000"+
		"\u01eb\u01ee\u0003\\.\u0000\u01ec\u01ee\u00036\u001b\u0000\u01ed\u01eb"+
		"\u0001\u0000\u0000\u0000\u01ed\u01ec\u0001\u0000\u0000\u0000\u01ee?\u0001"+
		"\u0000\u0000\u0000\u01ef\u01f0\u0003t:\u0000\u01f0A\u0001\u0000\u0000"+
		"\u0000\u01f1\u01f2\u00055\u0000\u0000\u01f2\u01f3\u0005\u001f\u0000\u0000"+
		"\u01f3\u01f4\u0005\u001b\u0000\u0000\u01f4\u01f5\u0003D\"\u0000\u01f5"+
		"\u01f6\u0005 \u0000\u0000\u01f6C\u0001\u0000\u0000\u0000\u01f7\u01fc\u0003"+
		"F#\u0000\u01f8\u01fb\u0003F#\u0000\u01f9\u01fb\u0003\u009eO\u0000\u01fa"+
		"\u01f8\u0001\u0000\u0000\u0000\u01fa\u01f9\u0001\u0000\u0000\u0000\u01fb"+
		"\u01fe\u0001\u0000\u0000\u0000\u01fc\u01fa\u0001\u0000\u0000\u0000\u01fc"+
		"\u01fd\u0001\u0000\u0000\u0000\u01fd\u0200\u0001\u0000\u0000\u0000\u01fe"+
		"\u01fc\u0001\u0000\u0000\u0000\u01ff\u0201\u0003<\u001e\u0000\u0200\u01ff"+
		"\u0001\u0000\u0000\u0000\u0200\u0201\u0001\u0000\u0000\u0000\u0201E\u0001"+
		"\u0000\u0000\u0000\u0202\u0203\u0003@ \u0000\u0203\u0204\u0005<\u0000"+
		"\u0000\u0204\u0205\u0003>\u001f\u0000\u0205\u0206\u0005\u001b\u0000\u0000"+
		"\u0206G\u0001\u0000\u0000\u0000\u0207\u0209\u00055\u0000\u0000\u0208\u020a"+
		"\u0003\\.\u0000\u0209\u0208\u0001\u0000\u0000\u0000\u0209\u020a\u0001"+
		"\u0000\u0000\u0000\u020a\u020b\u0001\u0000\u0000\u0000\u020b\u020c\u0005"+
		"\u001f\u0000\u0000\u020c\u020d\u0005\u001b\u0000\u0000\u020d\u020e\u0003"+
		"J%\u0000\u020e\u020f\u0005 \u0000\u0000\u020fI\u0001\u0000\u0000\u0000"+
		"\u0210\u0212\u0003\u009eO\u0000\u0211\u0210\u0001\u0000\u0000\u0000\u0212"+
		"\u0215\u0001\u0000\u0000\u0000\u0213\u0211\u0001\u0000\u0000\u0000\u0213"+
		"\u0214\u0001\u0000\u0000\u0000\u0214\u0216\u0001\u0000\u0000\u0000\u0215"+
		"\u0213\u0001\u0000\u0000\u0000\u0216\u021b\u0003L&\u0000\u0217\u021a\u0003"+
		"L&\u0000\u0218\u021a\u0003\u009eO\u0000\u0219\u0217\u0001\u0000\u0000"+
		"\u0000\u0219\u0218\u0001\u0000\u0000\u0000\u021a\u021d\u0001\u0000\u0000"+
		"\u0000\u021b\u0219\u0001\u0000\u0000\u0000\u021b\u021c\u0001\u0000\u0000"+
		"\u0000\u021c\u021f\u0001\u0000\u0000\u0000\u021d\u021b\u0001\u0000\u0000"+
		"\u0000\u021e\u0220\u0003<\u001e\u0000\u021f\u021e\u0001\u0000\u0000\u0000"+
		"\u021f\u0220\u0001\u0000\u0000\u0000\u0220K\u0001\u0000\u0000\u0000\u0221"+
		"\u0222\u0003N\'\u0000\u0222\u0223\u0005<\u0000\u0000\u0223\u0224\u0003"+
		">\u001f\u0000\u0224\u0225\u0005\u001b\u0000\u0000\u0225M\u0001\u0000\u0000"+
		"\u0000\u0226\u0229\u0003\\.\u0000\u0227\u0229\u0003|>\u0000\u0228\u0226"+
		"\u0001\u0000\u0000\u0000\u0228\u0227\u0001\u0000\u0000\u0000\u0229O\u0001"+
		"\u0000\u0000\u0000\u022a\u022b\u0003\u009aM\u0000\u022b\u022c\u0003\u0090"+
		"H\u0000\u022c\u022d\u0003R)\u0000\u022dQ\u0001\u0000\u0000\u0000\u022e"+
		"\u0233\u0005;\u0000\u0000\u022f\u0234\u0003\\.\u0000\u0230\u0234\u0003"+
		"H$\u0000\u0231\u0234\u0003B!\u0000\u0232\u0234\u00038\u001c\u0000\u0233"+
		"\u022f\u0001\u0000\u0000\u0000\u0233\u0230\u0001\u0000\u0000\u0000\u0233"+
		"\u0231\u0001\u0000\u0000\u0000\u0233\u0232\u0001\u0000\u0000\u0000\u0234"+
		"\u0237\u0001\u0000\u0000\u0000\u0235\u0237\u0003l6\u0000\u0236\u022e\u0001"+
		"\u0000\u0000\u0000\u0236\u0235\u0001\u0000\u0000\u0000\u0237S\u0001\u0000"+
		"\u0000\u0000\u0238\u023a\u0005,\u0000\u0000\u0239\u023b\u0003t:\u0000"+
		"\u023a\u0239\u0001\u0000\u0000\u0000\u023a\u023b\u0001\u0000\u0000\u0000"+
		"\u023b\u0241\u0001\u0000\u0000\u0000\u023c\u023d\u0005-\u0000\u0000\u023d"+
		"\u0241\u0005\n\u0000\u0000\u023e\u0241\u0005/\u0000\u0000\u023f\u0241"+
		"\u0005.\u0000\u0000\u0240\u0238\u0001\u0000\u0000\u0000\u0240\u023c\u0001"+
		"\u0000\u0000\u0000\u0240\u023e\u0001\u0000\u0000\u0000\u0240\u023f\u0001"+
		"\u0000\u0000\u0000\u0241U\u0001\u0000\u0000\u0000\u0242\u0243\u0005\u0007"+
		"\u0000\u0000\u0243\u0244\u0003X,\u0000\u0244\u0245\u0005\u0002\u0000\u0000"+
		"\u0245W\u0001\u0000\u0000\u0000\u0246\u024b\u0005C\u0000\u0000\u0247\u024b"+
		"\u0005D\u0000\u0000\u0248\u024b\u0005E\u0000\u0000\u0249\u024b\u0003Z"+
		"-\u0000\u024a\u0246\u0001\u0000\u0000\u0000\u024a\u0247\u0001\u0000\u0000"+
		"\u0000\u024a\u0248\u0001\u0000\u0000\u0000\u024a\u0249\u0001\u0000\u0000"+
		"\u0000\u024b\u024c\u0001\u0000\u0000\u0000\u024c\u024a\u0001\u0000\u0000"+
		"\u0000\u024c\u024d\u0001\u0000\u0000\u0000\u024dY\u0001\u0000\u0000\u0000"+
		"\u024e\u024f\u0005\b\u0000\u0000\u024f\u0250\u0003~?\u0000\u0250\u0252"+
		"\u0005 \u0000\u0000\u0251\u0253\u0005W\u0000\u0000\u0252\u0251\u0001\u0000"+
		"\u0000\u0000\u0252\u0253\u0001\u0000\u0000\u0000\u0253[\u0001\u0000\u0000"+
		"\u0000\u0254\u0257\u0003`0\u0000\u0255\u0257\u0003v;\u0000\u0256\u0254"+
		"\u0001\u0000\u0000\u0000\u0256\u0255\u0001\u0000\u0000\u0000\u0257]\u0001"+
		"\u0000\u0000\u0000\u0258\u0259\u0005=\u0000\u0000\u0259\u025a\u0003\\"+
		".\u0000\u025a\u025b\u0005\f\u0000\u0000\u025b_\u0001\u0000\u0000\u0000"+
		"\u025c\u025d\u00060\uffff\uffff\u0000\u025d\u025f\u0003b1\u0000\u025e"+
		"\u0260\u0003\u0082A\u0000\u025f\u025e\u0001\u0000\u0000\u0000\u025f\u0260"+
		"\u0001\u0000\u0000\u0000\u0260\u0265\u0001\u0000\u0000\u0000\u0261\u0262"+
		"\u0003f3\u0000\u0262\u0263\u0003`0\u0002\u0263\u0265\u0001\u0000\u0000"+
		"\u0000\u0264\u025c\u0001\u0000\u0000\u0000\u0264\u0261\u0001\u0000\u0000"+
		"\u0000\u0265\u026a\u0001\u0000\u0000\u0000\u0266\u0267\n\u0001\u0000\u0000"+
		"\u0267\u0269\u0003h4\u0000\u0268\u0266\u0001\u0000\u0000\u0000\u0269\u026c"+
		"\u0001\u0000\u0000\u0000\u026a\u0268\u0001\u0000\u0000\u0000\u026a\u026b"+
		"\u0001\u0000\u0000\u0000\u026ba\u0001\u0000\u0000\u0000\u026c\u026a\u0001"+
		"\u0000\u0000\u0000\u026d\u0278\u0003z=\u0000\u026e\u0278\u0003d2\u0000"+
		"\u026f\u0278\u0003~?\u0000\u0270\u0278\u0003p8\u0000\u0271\u0278\u0003"+
		"n7\u0000\u0272\u0278\u0003^/\u0000\u0273\u0278\u0003j5\u0000\u0274\u0278"+
		"\u0005\u0015\u0000\u0000\u0275\u0278\u0005\u0016\u0000\u0000\u0276\u0278"+
		"\u0005\u0017\u0000\u0000\u0277\u026d\u0001\u0000\u0000\u0000\u0277\u026e"+
		"\u0001\u0000\u0000\u0000\u0277\u026f\u0001\u0000\u0000\u0000\u0277\u0270"+
		"\u0001\u0000\u0000\u0000\u0277\u0271\u0001\u0000\u0000\u0000\u0277\u0272"+
		"\u0001\u0000\u0000\u0000\u0277\u0273\u0001\u0000\u0000\u0000\u0277\u0274"+
		"\u0001\u0000\u0000\u0000\u0277\u0275\u0001\u0000\u0000\u0000\u0277\u0276"+
		"\u0001\u0000\u0000\u0000\u0278c\u0001\u0000\u0000\u0000\u0279\u027f\u0005"+
		"\u0010\u0000\u0000\u027a\u027e\u00036\u001b\u0000\u027b\u027e\u0005!\u0000"+
		"\u0000\u027c\u027e\u0003\u009eO\u0000\u027d\u027a\u0001\u0000\u0000\u0000"+
		"\u027d\u027b\u0001\u0000\u0000\u0000\u027d\u027c\u0001\u0000\u0000\u0000"+
		"\u027e\u0281\u0001\u0000\u0000\u0000\u027f\u027d\u0001\u0000\u0000\u0000"+
		"\u027f\u0280\u0001\u0000\u0000\u0000\u0280\u0282\u0001\u0000\u0000\u0000"+
		"\u0281\u027f\u0001\u0000\u0000\u0000\u0282\u0289\u0005 \u0000\u0000\u0283"+
		"\u0284\u0005\u0011\u0000\u0000\u0284\u0286\u0003\\.\u0000\u0285\u0287"+
		"\u0005\f\u0000\u0000\u0286\u0285\u0001\u0000\u0000\u0000\u0286\u0287\u0001"+
		"\u0000\u0000\u0000\u0287\u0289\u0001\u0000\u0000\u0000\u0288\u0279\u0001"+
		"\u0000\u0000\u0000\u0288\u0283\u0001\u0000\u0000\u0000\u0289e\u0001\u0000"+
		"\u0000\u0000\u028a\u028b\u0005N\u0000\u0000\u028bg\u0001\u0000\u0000\u0000"+
		"\u028c\u028d\u0005O\u0000\u0000\u028di\u0001\u0000\u0000\u0000\u028e\u028f"+
		"\u0003\u0092I\u0000\u028f\u0290\u0003t:\u0000\u0290k\u0001\u0000\u0000"+
		"\u0000\u0291\u0293\u0005\u001b\u0000\u0000\u0292\u0291\u0001\u0000\u0000"+
		"\u0000\u0292\u0293\u0001\u0000\u0000\u0000\u0293\u0294\u0001\u0000\u0000"+
		"\u0000\u0294\u0297\u0003V+\u0000\u0295\u0297\u0003`0\u0000\u0296\u0292"+
		"\u0001\u0000\u0000\u0000\u0296\u0295\u0001\u0000\u0000\u0000\u0297m\u0001"+
		"\u0000\u0000\u0000\u0298\u0299\u0005\u000f\u0000\u0000\u0299\u02a9\u0005"+
		"L\u0000\u0000\u029a\u029b\u0005\u000f\u0000\u0000\u029b\u02a0\u0003\\"+
		".\u0000\u029c\u029d\u0005J\u0000\u0000\u029d\u029f\u0003\\.\u0000\u029e"+
		"\u029c\u0001\u0000\u0000\u0000\u029f\u02a2\u0001\u0000\u0000\u0000\u02a0"+
		"\u029e\u0001\u0000\u0000\u0000\u02a0\u02a1\u0001\u0000\u0000\u0000\u02a1"+
		"\u02a4\u0001\u0000\u0000\u0000\u02a2\u02a0\u0001\u0000\u0000\u0000\u02a3"+
		"\u02a5\u0005J\u0000\u0000\u02a4\u02a3\u0001\u0000\u0000\u0000\u02a4\u02a5"+
		"\u0001\u0000\u0000\u0000\u02a5\u02a6\u0001\u0000\u0000\u0000\u02a6\u02a7"+
		"\u0005L\u0000\u0000\u02a7\u02a9\u0001\u0000\u0000\u0000\u02a8\u0298\u0001"+
		"\u0000\u0000\u0000\u02a8\u029a\u0001\u0000\u0000\u0000\u02a9o\u0001\u0000"+
		"\u0000\u0000\u02aa\u02ab\u0005\u000e\u0000\u0000\u02ab\u02bb\u0005 \u0000"+
		"\u0000\u02ac\u02ad\u0005\u000e\u0000\u0000\u02ad\u02b2\u0003r9\u0000\u02ae"+
		"\u02af\u0005J\u0000\u0000\u02af\u02b1\u0003r9\u0000\u02b0\u02ae\u0001"+
		"\u0000\u0000\u0000\u02b1\u02b4\u0001\u0000\u0000\u0000\u02b2\u02b0\u0001"+
		"\u0000\u0000\u0000\u02b2\u02b3\u0001\u0000\u0000\u0000\u02b3\u02b6\u0001"+
		"\u0000\u0000\u0000\u02b4\u02b2\u0001\u0000\u0000\u0000\u02b5\u02b7\u0005"+
		"J\u0000\u0000\u02b6\u02b5\u0001\u0000\u0000\u0000\u02b6\u02b7\u0001\u0000"+
		"\u0000\u0000\u02b7\u02b8\u0001\u0000\u0000\u0000\u02b8\u02b9\u0005 \u0000"+
		"\u0000\u02b9\u02bb\u0001\u0000\u0000\u0000\u02ba\u02aa\u0001\u0000\u0000"+
		"\u0000\u02ba\u02ac\u0001\u0000\u0000\u0000\u02bbq\u0001\u0000\u0000\u0000"+
		"\u02bc\u02be\u0003\b\u0004\u0000\u02bd\u02bc\u0001\u0000\u0000\u0000\u02be"+
		"\u02c1\u0001\u0000\u0000\u0000\u02bf\u02bd\u0001\u0000\u0000\u0000\u02bf"+
		"\u02c0\u0001\u0000\u0000\u0000\u02c0\u02c2\u0001\u0000\u0000\u0000\u02c1"+
		"\u02bf\u0001\u0000\u0000\u0000\u02c2\u02c3\u0005\r\u0000\u0000\u02c3\u02c4"+
		"\u0005Q\u0000\u0000\u02c4\u02c5\u0003\\.\u0000\u02c5s\u0001\u0000\u0000"+
		"\u0000\u02c6\u02c7\u0005?\u0000\u0000\u02c7\u02cc\u0003\\.\u0000\u02c8"+
		"\u02c9\u0005J\u0000\u0000\u02c9\u02cb\u0003\\.\u0000\u02ca\u02c8\u0001"+
		"\u0000\u0000\u0000\u02cb\u02ce\u0001\u0000\u0000\u0000\u02cc\u02ca\u0001"+
		"\u0000\u0000\u0000\u02cc\u02cd\u0001\u0000\u0000\u0000\u02cd\u02d0\u0001"+
		"\u0000\u0000\u0000\u02ce\u02cc\u0001\u0000\u0000\u0000\u02cf\u02d1\u0005"+
		"J\u0000\u0000\u02d0\u02cf\u0001\u0000\u0000\u0000\u02d0\u02d1\u0001\u0000"+
		"\u0000\u0000\u02d1\u02d2\u0001\u0000\u0000\u0000\u02d2\u02d3\u0005K\u0000"+
		"\u0000\u02d3\u02dd\u0001\u0000\u0000\u0000\u02d4\u02d5\u0005?\u0000\u0000"+
		"\u02d5\u02dd\u0005K\u0000\u0000\u02d6\u02dd\u0005>\u0000\u0000\u02d7\u02d9"+
		"\u0003v;\u0000\u02d8\u02d7\u0001\u0000\u0000\u0000\u02d9\u02da\u0001\u0000"+
		"\u0000\u0000\u02da\u02d8\u0001\u0000\u0000\u0000\u02da\u02db\u0001\u0000"+
		"\u0000\u0000\u02db\u02dd\u0001\u0000\u0000\u0000\u02dc\u02c6\u0001\u0000"+
		"\u0000\u0000\u02dc\u02d4\u0001\u0000\u0000\u0000\u02dc\u02d6\u0001\u0000"+
		"\u0000\u0000\u02dc\u02d8\u0001\u0000\u0000\u0000\u02ddu\u0001\u0000\u0000"+
		"\u0000\u02de\u02df\u0005\u0004\u0000\u0000\u02df\u02e0\u0003x<\u0000\u02e0"+
		"\u02e1\u0005B\u0000\u0000\u02e1w\u0001\u0000\u0000\u0000\u02e2\u02e6\u0005"+
		"A\u0000\u0000\u02e3\u02e6\u0003\u00a2Q\u0000\u02e4\u02e6\u0003z=\u0000"+
		"\u02e5\u02e2\u0001\u0000\u0000\u0000\u02e5\u02e3\u0001\u0000\u0000\u0000"+
		"\u02e5\u02e4\u0001\u0000\u0000\u0000\u02e6\u02e9\u0001\u0000\u0000\u0000"+
		"\u02e7\u02e5\u0001\u0000\u0000\u0000\u02e7\u02e8\u0001\u0000\u0000\u0000"+
		"\u02e8y\u0001\u0000\u0000\u0000\u02e9\u02e7\u0001\u0000\u0000\u0000\u02ea"+
		"\u02eb\u0005\t\u0000\u0000\u02eb\u02ec\u0003\\.\u0000\u02ec\u02ed\u0005"+
		"L\u0000\u0000\u02ed{\u0001\u0000\u0000\u0000\u02ee\u02f0\u0005\t\u0000"+
		"\u0000\u02ef\u02f1\u0003\u0084B\u0000\u02f0\u02ef\u0001\u0000\u0000\u0000"+
		"\u02f1\u02f2\u0001\u0000\u0000\u0000\u02f2\u02f0\u0001\u0000\u0000\u0000"+
		"\u02f2\u02f3\u0001\u0000\u0000\u0000\u02f3\u02f4\u0001\u0000\u0000\u0000"+
		"\u02f4\u02f5\u0005L\u0000\u0000\u02f5}\u0001\u0000\u0000\u0000\u02f6\u02fa"+
		"\u0003\u0080@\u0000\u02f7\u02f9\u0003\u0084B\u0000\u02f8\u02f7\u0001\u0000"+
		"\u0000\u0000\u02f9\u02fc\u0001\u0000\u0000\u0000\u02fa\u02f8\u0001\u0000"+
		"\u0000\u0000\u02fa\u02fb\u0001\u0000\u0000\u0000\u02fb\u007f\u0001\u0000"+
		"\u0000\u0000\u02fc\u02fa\u0001\u0000\u0000\u0000\u02fd\u0301\u0005\r\u0000"+
		"\u0000\u02fe\u02ff\u0005M\u0000\u0000\u02ff\u0301\u0007\u0001\u0000\u0000"+
		"\u0300\u02fd\u0001\u0000\u0000\u0000\u0300\u02fe\u0001\u0000\u0000\u0000"+
		"\u0301\u0081\u0001\u0000\u0000\u0000\u0302\u0303\u0005P\u0000\u0000\u0303"+
		"\u0304\u0003\\.\u0000\u0304\u0083\u0001\u0000\u0000\u0000\u0305\u0306"+
		"\u0003\u0086C\u0000\u0306\u0308\u0003\u0092I\u0000\u0307\u0309\u0003t"+
		":\u0000\u0308\u0307\u0001\u0000\u0000\u0000\u0308\u0309\u0001\u0000\u0000"+
		"\u0000\u0309\u030d\u0001\u0000\u0000\u0000\u030a\u030d\u0003\u008aE\u0000"+
		"\u030b\u030d\u0005I\u0000\u0000\u030c\u0305\u0001\u0000\u0000\u0000\u030c"+
		"\u030a\u0001\u0000\u0000\u0000\u030c\u030b\u0001\u0000\u0000\u0000\u030d"+
		"\u0085\u0001\u0000\u0000\u0000\u030e\u030f\u0007\u0002\u0000\u0000\u030f"+
		"\u0087\u0001\u0000\u0000\u0000\u0310\u0311\u0005:\u0000\u0000\u0311\u0312"+
		"\u0005Z\u0000\u0000\u0312\u0089\u0001\u0000\u0000\u0000\u0313\u0314\u0005"+
		"H\u0000\u0000\u0314\u0315\u0005Y\u0000\u0000\u0315\u008b\u0001\u0000\u0000"+
		"\u0000\u0316\u031a\u0005\n\u0000\u0000\u0317\u031a\u0005\u000b\u0000\u0000"+
		"\u0318\u031a\u0003v;\u0000\u0319\u0316\u0001\u0000\u0000\u0000\u0319\u0317"+
		"\u0001\u0000\u0000\u0000\u0319\u0318\u0001\u0000\u0000\u0000\u031a\u008d"+
		"\u0001\u0000\u0000\u0000\u031b\u031e\u0005\n\u0000\u0000\u031c\u031e\u0003"+
		"v;\u0000\u031d\u031b\u0001\u0000\u0000\u0000\u031d\u031c\u0001\u0000\u0000"+
		"\u0000\u031e\u008f\u0001\u0000\u0000\u0000\u031f\u0320\u0005\n\u0000\u0000"+
		"\u0320\u0091\u0001\u0000\u0000\u0000\u0321\u0322\u0007\u0003\u0000\u0000"+
		"\u0322\u0093\u0001\u0000\u0000\u0000\u0323\u0324\u0007\u0000\u0000\u0000"+
		"\u0324\u0095\u0001\u0000\u0000\u0000\u0325\u0326\u0007\u0004\u0000\u0000"+
		"\u0326\u0097\u0001\u0000\u0000\u0000\u0327\u0328\u0005*\u0000\u0000\u0328"+
		"\u0099\u0001\u0000\u0000\u0000\u0329\u032b\u00053\u0000\u0000\u032a\u0329"+
		"\u0001\u0000\u0000\u0000\u032a\u032b\u0001\u0000\u0000\u0000\u032b\u032c"+
		"\u0001\u0000\u0000\u0000\u032c\u0332\u00052\u0000\u0000\u032d\u032f\u0005"+
		"0\u0000\u0000\u032e\u032d\u0001\u0000\u0000\u0000\u032e\u032f\u0001\u0000"+
		"\u0000\u0000\u032f\u0330\u0001\u0000\u0000\u0000\u0330\u0332\u00051\u0000"+
		"\u0000\u0331\u032a\u0001\u0000\u0000\u0000\u0331\u032e\u0001\u0000\u0000"+
		"\u0000\u0332\u009b\u0001\u0000\u0000\u0000\u0333\u0334\u0007\u0005\u0000"+
		"\u0000\u0334\u009d\u0001\u0000\u0000\u0000\u0335\u0338\u0003\u00a0P\u0000"+
		"\u0336\u0338\u0005\u001b\u0000\u0000\u0337\u0335\u0001\u0000\u0000\u0000"+
		"\u0337\u0336\u0001\u0000\u0000\u0000\u0338\u009f\u0001\u0000\u0000\u0000"+
		"\u0339\u033a\u0007\u0006\u0000\u0000\u033a\u00a1\u0001\u0000\u0000\u0000"+
		"\u033b\u033c\u0005\u0003\u0000\u0000\u033c\u033d\u0007\u0007\u0000\u0000"+
		"\u033d\u00a3\u0001\u0000\u0000\u0000h\u00a7\u00ab\u00b1\u00b6\u00bf\u00c6"+
		"\u00cd\u00d3\u00d8\u00de\u00e4\u00ea\u00ed\u00f6\u0106\u0108\u0110\u0119"+
		"\u011f\u0129\u012f\u0131\u0135\u0137\u0141\u0144\u014a\u0154\u0158\u015c"+
		"\u0166\u016a\u016e\u017b\u017f\u0186\u0189\u018d\u0193\u019d\u01a1\u01a5"+
		"\u01aa\u01ae\u01b8\u01ba\u01c3\u01cc\u01d1\u01d5\u01dd\u01e4\u01ed\u01fa"+
		"\u01fc\u0200\u0209\u0213\u0219\u021b\u021f\u0228\u0233\u0236\u023a\u0240"+
		"\u024a\u024c\u0252\u0256\u025f\u0264\u026a\u0277\u027d\u027f\u0286\u0288"+
		"\u0292\u0296\u02a0\u02a4\u02a8\u02b2\u02b6\u02ba\u02bf\u02cc\u02d0\u02da"+
		"\u02dc\u02e5\u02e7\u02f2\u02fa\u0300\u0308\u030c\u0319\u031d\u032a\u032e"+
		"\u0331\u0337";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}