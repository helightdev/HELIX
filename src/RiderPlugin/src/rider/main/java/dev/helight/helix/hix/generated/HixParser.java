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
		FAT_ARROW=18, NUMBER=19, BOOLEAN=20, NULL=21, ESCAPED_AT=22, ESCAPED_START=23,
		OUTER_WHITESPACE=24, NEWLINE=25, COMMENT=26, SLASH_COMMENT=27, LC=28,
		RC=29, SEMICOLON=30, COMMA=31, KEYWORD_FUNC=32, KEYWORD_DO=33, KEYWORD_EXPRESSION=34,
		KEYWORD_MIXIN=35, KEYWORD_PRELUDE=36, KEYWORD_DERIVATION=37, KEYWORD_ELSE=38,
		KEYWORD_RETURN=39, KEYWORD_GOTO=40, KEYWORD_BREAK=41, KEYWORD_CONTINUE=42,
		KEYWORD_TARGET=43, KEYWORD_VAR=44, KEYWORD_LOCAL=45, KEYWORD_CARRY=46,
		KEYWORD_SIG=47, KEYWORD_WHEN=48, KEYWORD_PURE=49, KEYWORD_INLINE=50, KEYWORD_NOINLINE=51,
		KEYWORD_STRICT=52, LABEL_PREFIX=53, ASSIGN=54, ARROW=55, TOPLEVEL_VALUE_EXPRESSION=56,
		EMPTY_PARAMETERS=57, BEGIN_PARAMETERS=58, ARGUMENT_TEXT=59, ARGUMENT_END=60,
		CONTENT_WRAP=61, CONTENT_LINEBREAK=62, CONTENT_TEXT=63, VALUE_FUNCTION=64,
		VALUE_PREDICATE=65, VALUE_MEMBER=66, VALUE_WRAP=67, VALUE_DELIMITER=68,
		END_PARAMETERS=69, VALUE_END_INLINE=70, VALUE_SMART_ROOT=71, NOT_VALUE=72,
		VALUE_CHECK=73, VALUE_ELVIS=74, VALUE_ASSIGN=75, VALUE_EXPAND=76, VALUE_WHITESPACE=77,
		ESCAPE_MACRO=78, ESCAPE_LITERAL=79, ESCAPE_HEX=80, END_CONTENT_INTERPOLATE=81,
		FUNCTION_IDENTIFIER=82, MEMBER_IDENTIFIER=83, LABEL_IDENTIFIER=84, TOPLEVEL_NULL=85;
	public static final int
		RULE_compilationUnit = 0, RULE_mixinDeclaration = 1, RULE_mixinBody = 2,
		RULE_expressionDeclaration = 3, RULE_funcDeclaration = 4, RULE_functionBody = 5,
		RULE_functionMetadata = 6, RULE_functionSignatureVariant = 7, RULE_signature = 8,
		RULE_tableSignature = 9, RULE_tableSignatureEntry = 10, RULE_statementBlock = 11,
		RULE_statement = 12, RULE_invocationStatement = 13, RULE_whenConditionStatement = 14,
		RULE_whenElseBranch = 15, RULE_whenResult = 16, RULE_whenChainCondition = 17,
		RULE_whenChainStatement = 18, RULE_whenChainBody = 19, RULE_whenChainBranch = 20,
		RULE_whenValueStatement = 21, RULE_whenValueBody = 22, RULE_whenValueBranch = 23,
		RULE_whenValueCondition = 24, RULE_assignmentStatement = 25, RULE_assignedValue = 26,
		RULE_controlflowStatement = 27, RULE_contentBlock = 28, RULE_contentBody = 29,
		RULE_contentInterpolate = 30, RULE_value = 31, RULE_valueExpression = 32,
		RULE_nonArgumentValue = 33, RULE_primaryValue = 34, RULE_lambdaValue = 35,
		RULE_prefixOperators = 36, RULE_postfixOperators = 37, RULE_valueStatement = 38,
		RULE_tailValue = 39, RULE_tupleValue = 40, RULE_tableValue = 41, RULE_tableKeyedEntry = 42,
		RULE_valueList = 43, RULE_argumentValue = 44, RULE_argumentBody = 45,
		RULE_inlineValue = 46, RULE_inlineTransformation = 47, RULE_derivation = 48,
		RULE_derivationRoot = 49, RULE_elvisValue = 50, RULE_transformationPart = 51,
		RULE_functionChainType = 52, RULE_labelIdentifier = 53, RULE_memberIdentifier = 54,
		RULE_mixinIdentifier = 55, RULE_functionDeclarationIdentifier = 56, RULE_variableIdentifier = 57,
		RULE_functionIdentifier = 58, RULE_kindIdentifier = 59, RULE_expressionModifier = 60,
		RULE_mixinModifier = 61, RULE_variableSpecifiers = 62, RULE_funcModifier = 63,
		RULE_trivia = 64, RULE_comment = 65, RULE_escaped = 66;
	private static String[] makeRuleNames() {
		return new String[] {
			"compilationUnit", "mixinDeclaration", "mixinBody", "expressionDeclaration",
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
			null, null, null, null, null, null, null, null, null, null, "'@@'", "'\\'",
			null, null, null, null, "'{'", null, null, null, "'func'", "'do'", "'expression'",
			"'mixin'", "'prelude'", "'derivation'", "'else'", "'return'", "'goto'",
			"'break'", "'continue'", "'target'", "'var'", "'local'", "'carry'", "'sig'",
			"'when'", "'pure'", "'inline'", "'noinline'", "'strict'", null, null,
			"'->'", null, null, "'('", null, "'>'", null, null, null, null, "':?'",
			"'#'", null, null, "')'", "']'", "'$'", "'!'", "'?'", "'?:'", null, "'...'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "ERROR_TOKEN", "TERMINATOR", "ESCAPE", "BEGIN_ARGUMENT", "BEGIN_VALUE",
			"BEGIN_VALUE_ESCAPED", "BEGIN_CONTENT", "BEGIN_VALUE_INTERPOLATE", "BEGIN_VALUE_INLINE",
			"IDENTIFIER", "NAMESPACE_IDENTIFIER", "VALUE_END", "ROOT_IDENTIFIER",
			"BEGIN_TABLE", "BEGIN_TUPLE", "BEGIN_LAMBDA_BLOCK", "BEGIN_LAMBDA_ARROW",
			"FAT_ARROW", "NUMBER", "BOOLEAN", "NULL", "ESCAPED_AT", "ESCAPED_START",
			"OUTER_WHITESPACE", "NEWLINE", "COMMENT", "SLASH_COMMENT", "LC", "RC",
			"SEMICOLON", "COMMA", "KEYWORD_FUNC", "KEYWORD_DO", "KEYWORD_EXPRESSION",
			"KEYWORD_MIXIN", "KEYWORD_PRELUDE", "KEYWORD_DERIVATION", "KEYWORD_ELSE",
			"KEYWORD_RETURN", "KEYWORD_GOTO", "KEYWORD_BREAK", "KEYWORD_CONTINUE",
			"KEYWORD_TARGET", "KEYWORD_VAR", "KEYWORD_LOCAL", "KEYWORD_CARRY", "KEYWORD_SIG",
			"KEYWORD_WHEN", "KEYWORD_PURE", "KEYWORD_INLINE", "KEYWORD_NOINLINE",
			"KEYWORD_STRICT", "LABEL_PREFIX", "ASSIGN", "ARROW", "TOPLEVEL_VALUE_EXPRESSION",
			"EMPTY_PARAMETERS", "BEGIN_PARAMETERS", "ARGUMENT_TEXT", "ARGUMENT_END",
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
		public List<MixinDeclarationContext> mixinDeclaration() {
			return getRuleContexts(MixinDeclarationContext.class);
		}
		public MixinDeclarationContext mixinDeclaration(int i) {
			return getRuleContext(MixinDeclarationContext.class,i);
		}
		public List<FuncDeclarationContext> funcDeclaration() {
			return getRuleContexts(FuncDeclarationContext.class);
		}
		public FuncDeclarationContext funcDeclaration(int i) {
			return getRuleContext(FuncDeclarationContext.class,i);
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
			setState(139);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3940826002489344L) != 0)) {
				{
				setState(137);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(134);
					trivia();
					}
					break;
				case KEYWORD_MIXIN:
				case KEYWORD_DERIVATION:
					{
					setState(135);
					mixinDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(136);
					funcDeclaration();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(141);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(142);
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
		enterRule(_localctx, 2, RULE_mixinDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(147);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_DERIVATION) {
				{
				{
				setState(144);
				mixinModifier();
				}
				}
				setState(149);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(150);
			match(KEYWORD_MIXIN);
			setState(151);
			mixinIdentifier();
			setState(152);
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
		enterRule(_localctx, 4, RULE_mixinBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(154);
			match(LC);
			setState(160);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 8444339730513920L) != 0)) {
				{
				setState(158);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_EXPRESSION:
				case KEYWORD_PRELUDE:
				case KEYWORD_STRICT:
					{
					setState(155);
					expressionDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(156);
					funcDeclaration();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(157);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(162);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(163);
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
		enterRule(_localctx, 6, RULE_expressionDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(168);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_PRELUDE || _la==KEYWORD_STRICT) {
				{
				{
				setState(165);
				expressionModifier();
				}
				}
				setState(170);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(171);
			match(KEYWORD_EXPRESSION);
			setState(172);
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
		enterRule(_localctx, 8, RULE_funcDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(177);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3940649673949184L) != 0)) {
				{
				{
				setState(174);
				funcModifier();
				}
				}
				setState(179);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(180);
			match(KEYWORD_FUNC);
			setState(181);
			functionDeclarationIdentifier();
			setState(182);
			functionMetadata();
			setState(183);
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
		enterRule(_localctx, 10, RULE_functionBody);
		int _la;
		try {
			setState(194);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case LC:
			case KEYWORD_DO:
				enterOuterAlt(_localctx, 1);
				{
				setState(186);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_DO) {
					{
					setState(185);
					match(KEYWORD_DO);
					}
				}

				setState(188);
				statementBlock();
				}
				break;
			case FAT_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(189);
				match(FAT_ARROW);
				setState(190);
				value();
				setState(192);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==VALUE_END) {
					{
					setState(191);
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
		enterRule(_localctx, 12, RULE_functionMetadata);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(200);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==NEWLINE || _la==KEYWORD_SIG) {
				{
				setState(198);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_SIG:
					{
					setState(196);
					functionSignatureVariant();
					}
					break;
				case NEWLINE:
					{
					setState(197);
					match(NEWLINE);
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(202);
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
		enterRule(_localctx, 14, RULE_functionSignatureVariant);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(203);
			match(KEYWORD_SIG);
			setState(204);
			signature();
			setState(205);
			match(ARROW);
			setState(206);
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
		enterRule(_localctx, 16, RULE_signature);
		try {
			setState(210);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_TABLE:
				enterOuterAlt(_localctx, 1);
				{
				setState(208);
				tableSignature();
				}
				break;
			case IDENTIFIER:
			case ROOT_IDENTIFIER:
			case NULL:
				enterOuterAlt(_localctx, 2);
				{
				setState(209);
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
		enterRule(_localctx, 18, RULE_tableSignature);
		int _la;
		try {
			setState(225);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,14,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(212);
				match(BEGIN_TABLE);
				setState(213);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(214);
				match(BEGIN_TABLE);
				setState(215);
				tableSignatureEntry();
				setState(220);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==VALUE_DELIMITER) {
					{
					{
					setState(216);
					match(VALUE_DELIMITER);
					setState(217);
					tableSignatureEntry();
					}
					}
					setState(222);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(223);
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
		enterRule(_localctx, 20, RULE_tableSignatureEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(228);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==VALUE_EXPAND) {
				{
				setState(227);
				match(VALUE_EXPAND);
				}
			}

			setState(230);
			match(ROOT_IDENTIFIER);
			setState(231);
			match(VALUE_ASSIGN);
			setState(232);
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
		enterRule(_localctx, 22, RULE_statementBlock);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(234);
			match(LC);
			setState(240);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 9428863541052416L) != 0)) {
				{
				setState(238);
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
					setState(235);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(236);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(237);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(242);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(243);
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
		enterRule(_localctx, 24, RULE_statement);
		int _la;
		try {
			setState(258);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,19,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(245);
				labelIdentifier();
				setState(246);
				match(NEWLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(249);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LABEL_PREFIX) {
					{
					setState(248);
					labelIdentifier();
					}
				}

				setState(251);
				statementBlock();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(252);
				invocationStatement();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(253);
				assignmentStatement();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(254);
				controlflowStatement();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(255);
				whenValueStatement();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(256);
				whenConditionStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(257);
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
		enterRule(_localctx, 26, RULE_invocationStatement);
		try {
			setState(267);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,21,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(260);
				match(IDENTIFIER);
				setState(261);
				valueList();
				setState(263);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,20,_ctx) ) {
				case 1:
					{
					setState(262);
					tailValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(265);
				match(IDENTIFIER);
				setState(266);
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
		enterRule(_localctx, 28, RULE_whenConditionStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(269);
			match(KEYWORD_WHEN);
			setState(270);
			whenChainCondition();
			setState(271);
			statementBlock();
			setState(282);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,23,_ctx) ) {
			case 1:
				{
				setState(275);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 234881024L) != 0)) {
					{
					{
					setState(272);
					trivia();
					}
					}
					setState(277);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(278);
				match(KEYWORD_ELSE);
				setState(279);
				whenResult();
				setState(280);
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
		enterRule(_localctx, 30, RULE_whenElseBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(284);
			match(KEYWORD_ELSE);
			setState(285);
			match(ARROW);
			setState(286);
			whenResult();
			setState(287);
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
		enterRule(_localctx, 32, RULE_whenResult);
		try {
			setState(291);
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
				setState(289);
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
				setState(290);
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
		enterRule(_localctx, 34, RULE_whenChainCondition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(293);
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
		enterRule(_localctx, 36, RULE_whenChainStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(295);
			match(KEYWORD_WHEN);
			setState(296);
			match(LC);
			setState(297);
			match(NEWLINE);
			setState(298);
			whenChainBody();
			setState(299);
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
		enterRule(_localctx, 38, RULE_whenChainBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(301);
			whenChainBranch();
			setState(306);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 432345564462448656L) != 0)) {
				{
				setState(304);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case EMPTY_PARAMETERS:
				case BEGIN_PARAMETERS:
					{
					setState(302);
					whenChainBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(303);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(308);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(310);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(309);
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
		enterRule(_localctx, 40, RULE_whenChainBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(312);
			whenChainCondition();
			setState(313);
			match(ARROW);
			setState(314);
			whenResult();
			setState(315);
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
		enterRule(_localctx, 42, RULE_whenValueStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(317);
			match(KEYWORD_WHEN);
			setState(319);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 72057594041852432L) != 0) || ((((_la - 71)) & ~0x3f) == 0 && ((1L << (_la - 71)) & 2051L) != 0)) {
				{
				setState(318);
				value();
				}
			}

			setState(321);
			match(LC);
			setState(322);
			match(NEWLINE);
			setState(323);
			whenValueBody();
			setState(324);
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
		enterRule(_localctx, 44, RULE_whenValueBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(329);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 234881024L) != 0)) {
				{
				{
				setState(326);
				trivia();
				}
				}
				setState(331);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(332);
			whenValueBranch();
			setState(337);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 72057594276733456L) != 0) || ((((_la - 71)) & ~0x3f) == 0 && ((1L << (_la - 71)) & 2051L) != 0)) {
				{
				setState(335);
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
					setState(333);
					whenValueBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(334);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(339);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(341);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(340);
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
		enterRule(_localctx, 46, RULE_whenValueBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(343);
			whenValueCondition();
			setState(344);
			match(ARROW);
			setState(345);
			whenResult();
			setState(346);
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
		enterRule(_localctx, 48, RULE_whenValueCondition);
		try {
			setState(350);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,33,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(348);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(349);
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
		enterRule(_localctx, 50, RULE_assignmentStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(352);
			variableSpecifiers();
			setState(353);
			variableIdentifier();
			setState(354);
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
		enterRule(_localctx, 52, RULE_assignedValue);
		try {
			setState(364);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ASSIGN:
				enterOuterAlt(_localctx, 1);
				{
				setState(356);
				match(ASSIGN);
				setState(361);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,34,_ctx) ) {
				case 1:
					{
					setState(357);
					value();
					}
					break;
				case 2:
					{
					setState(358);
					whenValueStatement();
					}
					break;
				case 3:
					{
					setState(359);
					whenChainStatement();
					}
					break;
				case 4:
					{
					setState(360);
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
				setState(363);
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
		enterRule(_localctx, 54, RULE_controlflowStatement);
		int _la;
		try {
			setState(374);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_RETURN:
				enterOuterAlt(_localctx, 1);
				{
				setState(366);
				match(KEYWORD_RETURN);
				setState(368);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 432345564227567632L) != 0)) {
					{
					setState(367);
					valueList();
					}
				}

				}
				break;
			case KEYWORD_GOTO:
				enterOuterAlt(_localctx, 2);
				{
				setState(370);
				match(KEYWORD_GOTO);
				setState(371);
				match(IDENTIFIER);
				}
				break;
			case KEYWORD_CONTINUE:
				enterOuterAlt(_localctx, 3);
				{
				setState(372);
				match(KEYWORD_CONTINUE);
				}
				break;
			case KEYWORD_BREAK:
				enterOuterAlt(_localctx, 4);
				{
				setState(373);
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
		enterRule(_localctx, 56, RULE_contentBlock);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(376);
			match(BEGIN_CONTENT);
			setState(377);
			contentBody();
			setState(378);
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
		enterRule(_localctx, 58, RULE_contentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(384);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				setState(384);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case CONTENT_WRAP:
					{
					setState(380);
					match(CONTENT_WRAP);
					}
					break;
				case CONTENT_LINEBREAK:
					{
					setState(381);
					match(CONTENT_LINEBREAK);
					}
					break;
				case CONTENT_TEXT:
					{
					setState(382);
					match(CONTENT_TEXT);
					}
					break;
				case BEGIN_VALUE_INTERPOLATE:
					{
					setState(383);
					contentInterpolate();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(386);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( (((_la) & ~0x3f) == 0 && ((1L << _la) & -2305843009213693696L) != 0) );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
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
		enterRule(_localctx, 60, RULE_contentInterpolate);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(388);
			match(BEGIN_VALUE_INTERPOLATE);
			setState(389);
			derivation();
			setState(390);
			match(RC);
			setState(392);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==END_CONTENT_INTERPOLATE) {
				{
				setState(391);
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
		enterRule(_localctx, 62, RULE_value);
		try {
			setState(396);
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
				setState(394);
				nonArgumentValue(0);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(395);
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
		enterRule(_localctx, 64, RULE_valueExpression);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(398);
			match(TOPLEVEL_VALUE_EXPRESSION);
			setState(399);
			value();
			setState(400);
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
		int _startState = 66;
		enterRecursionRule(_localctx, 66, RULE_nonArgumentValue, _p);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(410);
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
				setState(403);
				primaryValue();
				setState(405);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,42,_ctx) ) {
				case 1:
					{
					setState(404);
					elvisValue();
					}
					break;
				}
				}
				break;
			case NOT_VALUE:
				{
				setState(407);
				prefixOperators();
				setState(408);
				nonArgumentValue(2);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(416);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,44,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					{
					_localctx = new NonArgumentValueContext(_parentctx, _parentState);
					pushNewRecursionContext(_localctx, _startState, RULE_nonArgumentValue);
					setState(412);
					if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
					setState(413);
					postfixOperators();
					}
					}
				}
				setState(418);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,44,_ctx);
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
		enterRule(_localctx, 68, RULE_primaryValue);
		try {
			setState(429);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,45,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(419);
				inlineValue();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(420);
				lambdaValue();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(421);
				derivation();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(422);
				tableValue();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(423);
				tupleValue();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(424);
				valueExpression();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(425);
				valueStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(426);
				match(NUMBER);
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(427);
				match(BOOLEAN);
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(428);
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
		enterRule(_localctx, 70, RULE_lambdaValue);
		int _la;
		try {
			setState(446);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_LAMBDA_BLOCK:
				enterOuterAlt(_localctx, 1);
				{
				setState(431);
				match(BEGIN_LAMBDA_BLOCK);
				setState(437);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 9428863541052416L) != 0)) {
					{
					setState(435);
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
						setState(432);
						statement();
						}
						break;
					case SEMICOLON:
						{
						setState(433);
						match(SEMICOLON);
						}
						break;
					case NEWLINE:
					case COMMENT:
					case SLASH_COMMENT:
						{
						setState(434);
						trivia();
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					}
					setState(439);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(440);
				match(RC);
				}
				break;
			case BEGIN_LAMBDA_ARROW:
				enterOuterAlt(_localctx, 2);
				{
				setState(441);
				match(BEGIN_LAMBDA_ARROW);
				setState(442);
				value();
				setState(444);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,48,_ctx) ) {
				case 1:
					{
					setState(443);
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
		enterRule(_localctx, 72, RULE_prefixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(448);
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
		enterRule(_localctx, 74, RULE_postfixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(450);
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
		enterRule(_localctx, 76, RULE_valueStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(452);
			functionIdentifier();
			setState(453);
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
		enterRule(_localctx, 78, RULE_tailValue);
		int _la;
		try {
			setState(460);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_CONTENT:
			case NEWLINE:
				enterOuterAlt(_localctx, 1);
				{
				setState(456);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==NEWLINE) {
					{
					setState(455);
					match(NEWLINE);
					}
				}

				setState(458);
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
				setState(459);
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
		enterRule(_localctx, 80, RULE_tupleValue);
		int _la;
		try {
			setState(475);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,53,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(462);
				match(BEGIN_TUPLE);
				setState(463);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(464);
				match(BEGIN_TUPLE);
				setState(465);
				value();
				setState(470);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==VALUE_DELIMITER) {
					{
					{
					setState(466);
					match(VALUE_DELIMITER);
					setState(467);
					value();
					}
					}
					setState(472);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(473);
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
		enterRule(_localctx, 82, RULE_tableValue);
		int _la;
		try {
			setState(490);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,55,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(477);
				match(BEGIN_TABLE);
				setState(478);
				match(RC);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(479);
				match(BEGIN_TABLE);
				setState(480);
				tableKeyedEntry();
				setState(485);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==VALUE_DELIMITER) {
					{
					{
					setState(481);
					match(VALUE_DELIMITER);
					setState(482);
					tableKeyedEntry();
					}
					}
					setState(487);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(488);
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
		enterRule(_localctx, 84, RULE_tableKeyedEntry);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(492);
			match(ROOT_IDENTIFIER);
			setState(493);
			match(VALUE_ASSIGN);
			setState(494);
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
		enterRule(_localctx, 86, RULE_valueList);
		int _la;
		try {
			int _alt;
			setState(515);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,58,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(496);
				match(BEGIN_PARAMETERS);
				setState(497);
				value();
				setState(502);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==VALUE_DELIMITER) {
					{
					{
					setState(498);
					match(VALUE_DELIMITER);
					setState(499);
					value();
					}
					}
					setState(504);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(505);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(507);
				match(BEGIN_PARAMETERS);
				setState(508);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(509);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(511);
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(510);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(513);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,57,_ctx);
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
		enterRule(_localctx, 88, RULE_argumentValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(517);
			match(BEGIN_ARGUMENT);
			setState(518);
			argumentBody();
			setState(519);
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
		enterRule(_localctx, 90, RULE_argumentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(526);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 576460752303424008L) != 0)) {
				{
				setState(524);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case ARGUMENT_TEXT:
					{
					setState(521);
					match(ARGUMENT_TEXT);
					}
					break;
				case ESCAPE:
					{
					setState(522);
					escaped();
					}
					break;
				case BEGIN_VALUE_INLINE:
					{
					setState(523);
					inlineValue();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(528);
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
		enterRule(_localctx, 92, RULE_inlineValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(529);
			match(BEGIN_VALUE_INLINE);
			setState(530);
			value();
			setState(531);
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
		enterRule(_localctx, 94, RULE_inlineTransformation);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(533);
			match(BEGIN_VALUE_INLINE);
			setState(535);
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(534);
				transformationPart();
				}
				}
				setState(537);
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( ((((_la - 64)) & ~0x3f) == 0 && ((1L << (_la - 64)) & 15L) != 0) );
			setState(539);
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
		enterRule(_localctx, 96, RULE_derivation);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(541);
			derivationRoot();
			setState(545);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,62,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(542);
					transformationPart();
					}
					}
				}
				setState(547);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,62,_ctx);
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
		enterRule(_localctx, 98, RULE_derivationRoot);
		int _la;
		try {
			setState(551);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ROOT_IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(548);
				match(ROOT_IDENTIFIER);
				}
				break;
			case VALUE_SMART_ROOT:
				enterOuterAlt(_localctx, 2);
				{
				setState(549);
				match(VALUE_SMART_ROOT);
				setState(550);
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
		enterRule(_localctx, 100, RULE_elvisValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(553);
			match(VALUE_ELVIS);
			setState(554);
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
		enterRule(_localctx, 102, RULE_transformationPart);
		try {
			setState(563);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case VALUE_FUNCTION:
			case VALUE_PREDICATE:
				enterOuterAlt(_localctx, 1);
				{
				setState(556);
				functionChainType();
				setState(557);
				functionIdentifier();
				setState(559);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,64,_ctx) ) {
				case 1:
					{
					setState(558);
					valueList();
					}
					break;
				}
				}
				break;
			case VALUE_MEMBER:
				enterOuterAlt(_localctx, 2);
				{
				setState(561);
				memberIdentifier();
				}
				break;
			case VALUE_WRAP:
				enterOuterAlt(_localctx, 3);
				{
				setState(562);
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
		enterRule(_localctx, 104, RULE_functionChainType);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(565);
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
		enterRule(_localctx, 106, RULE_labelIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(567);
			match(LABEL_PREFIX);
			setState(568);
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
		enterRule(_localctx, 108, RULE_memberIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(570);
			match(VALUE_MEMBER);
			setState(571);
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
		enterRule(_localctx, 110, RULE_mixinIdentifier);
		try {
			setState(576);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(573);
				match(IDENTIFIER);
				}
				break;
			case NAMESPACE_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(574);
				match(NAMESPACE_IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 3);
				{
				setState(575);
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
		enterRule(_localctx, 112, RULE_functionDeclarationIdentifier);
		try {
			setState(580);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(578);
				match(IDENTIFIER);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(579);
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
		enterRule(_localctx, 114, RULE_variableIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(582);
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
		enterRule(_localctx, 116, RULE_functionIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(584);
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
		enterRule(_localctx, 118, RULE_kindIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(586);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 2106368L) != 0)) ) {
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
		enterRule(_localctx, 120, RULE_expressionModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(588);
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
		enterRule(_localctx, 122, RULE_mixinModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(590);
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
		enterRule(_localctx, 124, RULE_variableSpecifiers);
		int _la;
		try {
			setState(600);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_LOCAL:
			case KEYWORD_CARRY:
				enterOuterAlt(_localctx, 1);
				{
				setState(593);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_CARRY) {
					{
					setState(592);
					match(KEYWORD_CARRY);
					}
				}

				setState(595);
				match(KEYWORD_LOCAL);
				}
				break;
			case KEYWORD_TARGET:
			case KEYWORD_VAR:
				enterOuterAlt(_localctx, 2);
				{
				{
				setState(597);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_TARGET) {
					{
					setState(596);
					match(KEYWORD_TARGET);
					}
				}

				setState(599);
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
		enterRule(_localctx, 126, RULE_funcModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(602);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 3940649673949184L) != 0)) ) {
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
		enterRule(_localctx, 128, RULE_trivia);
		try {
			setState(606);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case COMMENT:
			case SLASH_COMMENT:
				enterOuterAlt(_localctx, 1);
				{
				setState(604);
				comment();
				}
				break;
			case NEWLINE:
				enterOuterAlt(_localctx, 2);
				{
				setState(605);
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
		enterRule(_localctx, 130, RULE_comment);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(608);
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
		enterRule(_localctx, 132, RULE_escaped);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(610);
			match(ESCAPE);
			setState(611);
			_la = _input.LA(1);
			if ( !(((((_la - 78)) & ~0x3f) == 0 && ((1L << (_la - 78)) & 7L) != 0)) ) {
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
		case 33:
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
		"\u0004\u0001U\u0266\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
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
		"A\u0007A\u0002B\u0007B\u0001\u0000\u0001\u0000\u0001\u0000\u0005\u0000"+
		"\u008a\b\u0000\n\u0000\f\u0000\u008d\t\u0000\u0001\u0000\u0001\u0000\u0001"+
		"\u0001\u0005\u0001\u0092\b\u0001\n\u0001\f\u0001\u0095\t\u0001\u0001\u0001"+
		"\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0002\u0001\u0002\u0001\u0002"+
		"\u0001\u0002\u0005\u0002\u009f\b\u0002\n\u0002\f\u0002\u00a2\t\u0002\u0001"+
		"\u0002\u0001\u0002\u0001\u0003\u0005\u0003\u00a7\b\u0003\n\u0003\f\u0003"+
		"\u00aa\t\u0003\u0001\u0003\u0001\u0003\u0001\u0003\u0001\u0004\u0005\u0004"+
		"\u00b0\b\u0004\n\u0004\f\u0004\u00b3\t\u0004\u0001\u0004\u0001\u0004\u0001"+
		"\u0004\u0001\u0004\u0001\u0004\u0001\u0005\u0003\u0005\u00bb\b\u0005\u0001"+
		"\u0005\u0001\u0005\u0001\u0005\u0001\u0005\u0003\u0005\u00c1\b\u0005\u0003"+
		"\u0005\u00c3\b\u0005\u0001\u0006\u0001\u0006\u0005\u0006\u00c7\b\u0006"+
		"\n\u0006\f\u0006\u00ca\t\u0006\u0001\u0007\u0001\u0007\u0001\u0007\u0001"+
		"\u0007\u0001\u0007\u0001\b\u0001\b\u0003\b\u00d3\b\b\u0001\t\u0001\t\u0001"+
		"\t\u0001\t\u0001\t\u0001\t\u0005\t\u00db\b\t\n\t\f\t\u00de\t\t\u0001\t"+
		"\u0001\t\u0003\t\u00e2\b\t\u0001\n\u0003\n\u00e5\b\n\u0001\n\u0001\n\u0001"+
		"\n\u0001\n\u0001\u000b\u0001\u000b\u0001\u000b\u0001\u000b\u0005\u000b"+
		"\u00ef\b\u000b\n\u000b\f\u000b\u00f2\t\u000b\u0001\u000b\u0001\u000b\u0001"+
		"\f\u0001\f\u0001\f\u0001\f\u0003\f\u00fa\b\f\u0001\f\u0001\f\u0001\f\u0001"+
		"\f\u0001\f\u0001\f\u0001\f\u0003\f\u0103\b\f\u0001\r\u0001\r\u0001\r\u0003"+
		"\r\u0108\b\r\u0001\r\u0001\r\u0003\r\u010c\b\r\u0001\u000e\u0001\u000e"+
		"\u0001\u000e\u0001\u000e\u0005\u000e\u0112\b\u000e\n\u000e\f\u000e\u0115"+
		"\t\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0003\u000e\u011b"+
		"\b\u000e\u0001\u000f\u0001\u000f\u0001\u000f\u0001\u000f\u0001\u000f\u0001"+
		"\u0010\u0001\u0010\u0003\u0010\u0124\b\u0010\u0001\u0011\u0001\u0011\u0001"+
		"\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0001"+
		"\u0013\u0001\u0013\u0001\u0013\u0005\u0013\u0131\b\u0013\n\u0013\f\u0013"+
		"\u0134\t\u0013\u0001\u0013\u0003\u0013\u0137\b\u0013\u0001\u0014\u0001"+
		"\u0014\u0001\u0014\u0001\u0014\u0001\u0014\u0001\u0015\u0001\u0015\u0003"+
		"\u0015\u0140\b\u0015\u0001\u0015\u0001\u0015\u0001\u0015\u0001\u0015\u0001"+
		"\u0015\u0001\u0016\u0005\u0016\u0148\b\u0016\n\u0016\f\u0016\u014b\t\u0016"+
		"\u0001\u0016\u0001\u0016\u0001\u0016\u0005\u0016\u0150\b\u0016\n\u0016"+
		"\f\u0016\u0153\t\u0016\u0001\u0016\u0003\u0016\u0156\b\u0016\u0001\u0017"+
		"\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0018\u0001\u0018"+
		"\u0003\u0018\u015f\b\u0018\u0001\u0019\u0001\u0019\u0001\u0019\u0001\u0019"+
		"\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0003\u001a"+
		"\u016a\b\u001a\u0001\u001a\u0003\u001a\u016d\b\u001a\u0001\u001b\u0001"+
		"\u001b\u0003\u001b\u0171\b\u001b\u0001\u001b\u0001\u001b\u0001\u001b\u0001"+
		"\u001b\u0003\u001b\u0177\b\u001b\u0001\u001c\u0001\u001c\u0001\u001c\u0001"+
		"\u001c\u0001\u001d\u0001\u001d\u0001\u001d\u0001\u001d\u0004\u001d\u0181"+
		"\b\u001d\u000b\u001d\f\u001d\u0182\u0001\u001e\u0001\u001e\u0001\u001e"+
		"\u0001\u001e\u0003\u001e\u0189\b\u001e\u0001\u001f\u0001\u001f\u0003\u001f"+
		"\u018d\b\u001f\u0001 \u0001 \u0001 \u0001 \u0001!\u0001!\u0001!\u0003"+
		"!\u0196\b!\u0001!\u0001!\u0001!\u0003!\u019b\b!\u0001!\u0001!\u0005!\u019f"+
		"\b!\n!\f!\u01a2\t!\u0001\"\u0001\"\u0001\"\u0001\"\u0001\"\u0001\"\u0001"+
		"\"\u0001\"\u0001\"\u0001\"\u0003\"\u01ae\b\"\u0001#\u0001#\u0001#\u0001"+
		"#\u0005#\u01b4\b#\n#\f#\u01b7\t#\u0001#\u0001#\u0001#\u0001#\u0003#\u01bd"+
		"\b#\u0003#\u01bf\b#\u0001$\u0001$\u0001%\u0001%\u0001&\u0001&\u0001&\u0001"+
		"\'\u0003\'\u01c9\b\'\u0001\'\u0001\'\u0003\'\u01cd\b\'\u0001(\u0001(\u0001"+
		"(\u0001(\u0001(\u0001(\u0005(\u01d5\b(\n(\f(\u01d8\t(\u0001(\u0001(\u0003"+
		"(\u01dc\b(\u0001)\u0001)\u0001)\u0001)\u0001)\u0001)\u0005)\u01e4\b)\n"+
		")\f)\u01e7\t)\u0001)\u0001)\u0003)\u01eb\b)\u0001*\u0001*\u0001*\u0001"+
		"*\u0001+\u0001+\u0001+\u0001+\u0005+\u01f5\b+\n+\f+\u01f8\t+\u0001+\u0001"+
		"+\u0001+\u0001+\u0001+\u0001+\u0004+\u0200\b+\u000b+\f+\u0201\u0003+\u0204"+
		"\b+\u0001,\u0001,\u0001,\u0001,\u0001-\u0001-\u0001-\u0005-\u020d\b-\n"+
		"-\f-\u0210\t-\u0001.\u0001.\u0001.\u0001.\u0001/\u0001/\u0004/\u0218\b"+
		"/\u000b/\f/\u0219\u0001/\u0001/\u00010\u00010\u00050\u0220\b0\n0\f0\u0223"+
		"\t0\u00011\u00011\u00011\u00031\u0228\b1\u00012\u00012\u00012\u00013\u0001"+
		"3\u00013\u00033\u0230\b3\u00013\u00013\u00033\u0234\b3\u00014\u00014\u0001"+
		"5\u00015\u00015\u00016\u00016\u00016\u00017\u00017\u00017\u00037\u0241"+
		"\b7\u00018\u00018\u00038\u0245\b8\u00019\u00019\u0001:\u0001:\u0001;\u0001"+
		";\u0001<\u0001<\u0001=\u0001=\u0001>\u0003>\u0252\b>\u0001>\u0001>\u0003"+
		">\u0256\b>\u0001>\u0003>\u0259\b>\u0001?\u0001?\u0001@\u0001@\u0003@\u025f"+
		"\b@\u0001A\u0001A\u0001B\u0001B\u0001B\u0001B\u0000\u0001BC\u0000\u0002"+
		"\u0004\u0006\b\n\f\u000e\u0010\u0012\u0014\u0016\u0018\u001a\u001c\u001e"+
		" \"$&(*,.02468:<>@BDFHJLNPRTVXZ\\^`bdfhjlnprtvxz|~\u0080\u0082\u0084\u0000"+
		"\b\u0002\u0000\r\r\u0013\u0013\u0001\u0000@A\u0002\u0000\r\rRR\u0003\u0000"+
		"\n\n\r\r\u0015\u0015\u0002\u0000$$44\u0001\u000013\u0001\u0000\u001a\u001b"+
		"\u0001\u0000NP\u0287\u0000\u008b\u0001\u0000\u0000\u0000\u0002\u0093\u0001"+
		"\u0000\u0000\u0000\u0004\u009a\u0001\u0000\u0000\u0000\u0006\u00a8\u0001"+
		"\u0000\u0000\u0000\b\u00b1\u0001\u0000\u0000\u0000\n\u00c2\u0001\u0000"+
		"\u0000\u0000\f\u00c8\u0001\u0000\u0000\u0000\u000e\u00cb\u0001\u0000\u0000"+
		"\u0000\u0010\u00d2\u0001\u0000\u0000\u0000\u0012\u00e1\u0001\u0000\u0000"+
		"\u0000\u0014\u00e4\u0001\u0000\u0000\u0000\u0016\u00ea\u0001\u0000\u0000"+
		"\u0000\u0018\u0102\u0001\u0000\u0000\u0000\u001a\u010b\u0001\u0000\u0000"+
		"\u0000\u001c\u010d\u0001\u0000\u0000\u0000\u001e\u011c\u0001\u0000\u0000"+
		"\u0000 \u0123\u0001\u0000\u0000\u0000\"\u0125\u0001\u0000\u0000\u0000"+
		"$\u0127\u0001\u0000\u0000\u0000&\u012d\u0001\u0000\u0000\u0000(\u0138"+
		"\u0001\u0000\u0000\u0000*\u013d\u0001\u0000\u0000\u0000,\u0149\u0001\u0000"+
		"\u0000\u0000.\u0157\u0001\u0000\u0000\u00000\u015e\u0001\u0000\u0000\u0000"+
		"2\u0160\u0001\u0000\u0000\u00004\u016c\u0001\u0000\u0000\u00006\u0176"+
		"\u0001\u0000\u0000\u00008\u0178\u0001\u0000\u0000\u0000:\u0180\u0001\u0000"+
		"\u0000\u0000<\u0184\u0001\u0000\u0000\u0000>\u018c\u0001\u0000\u0000\u0000"+
		"@\u018e\u0001\u0000\u0000\u0000B\u019a\u0001\u0000\u0000\u0000D\u01ad"+
		"\u0001\u0000\u0000\u0000F\u01be\u0001\u0000\u0000\u0000H\u01c0\u0001\u0000"+
		"\u0000\u0000J\u01c2\u0001\u0000\u0000\u0000L\u01c4\u0001\u0000\u0000\u0000"+
		"N\u01cc\u0001\u0000\u0000\u0000P\u01db\u0001\u0000\u0000\u0000R\u01ea"+
		"\u0001\u0000\u0000\u0000T\u01ec\u0001\u0000\u0000\u0000V\u0203\u0001\u0000"+
		"\u0000\u0000X\u0205\u0001\u0000\u0000\u0000Z\u020e\u0001\u0000\u0000\u0000"+
		"\\\u0211\u0001\u0000\u0000\u0000^\u0215\u0001\u0000\u0000\u0000`\u021d"+
		"\u0001\u0000\u0000\u0000b\u0227\u0001\u0000\u0000\u0000d\u0229\u0001\u0000"+
		"\u0000\u0000f\u0233\u0001\u0000\u0000\u0000h\u0235\u0001\u0000\u0000\u0000"+
		"j\u0237\u0001\u0000\u0000\u0000l\u023a\u0001\u0000\u0000\u0000n\u0240"+
		"\u0001\u0000\u0000\u0000p\u0244\u0001\u0000\u0000\u0000r\u0246\u0001\u0000"+
		"\u0000\u0000t\u0248\u0001\u0000\u0000\u0000v\u024a\u0001\u0000\u0000\u0000"+
		"x\u024c\u0001\u0000\u0000\u0000z\u024e\u0001\u0000\u0000\u0000|\u0258"+
		"\u0001\u0000\u0000\u0000~\u025a\u0001\u0000\u0000\u0000\u0080\u025e\u0001"+
		"\u0000\u0000\u0000\u0082\u0260\u0001\u0000\u0000\u0000\u0084\u0262\u0001"+
		"\u0000\u0000\u0000\u0086\u008a\u0003\u0080@\u0000\u0087\u008a\u0003\u0002"+
		"\u0001\u0000\u0088\u008a\u0003\b\u0004\u0000\u0089\u0086\u0001\u0000\u0000"+
		"\u0000\u0089\u0087\u0001\u0000\u0000\u0000\u0089\u0088\u0001\u0000\u0000"+
		"\u0000\u008a\u008d\u0001\u0000\u0000\u0000\u008b\u0089\u0001\u0000\u0000"+
		"\u0000\u008b\u008c\u0001\u0000\u0000\u0000\u008c\u008e\u0001\u0000\u0000"+
		"\u0000\u008d\u008b\u0001\u0000\u0000\u0000\u008e\u008f\u0005\u0000\u0000"+
		"\u0001\u008f\u0001\u0001\u0000\u0000\u0000\u0090\u0092\u0003z=\u0000\u0091"+
		"\u0090\u0001\u0000\u0000\u0000\u0092\u0095\u0001\u0000\u0000\u0000\u0093"+
		"\u0091\u0001\u0000\u0000\u0000\u0093\u0094\u0001\u0000\u0000\u0000\u0094"+
		"\u0096\u0001\u0000\u0000\u0000\u0095\u0093\u0001\u0000\u0000\u0000\u0096"+
		"\u0097\u0005#\u0000\u0000\u0097\u0098\u0003n7\u0000\u0098\u0099\u0003"+
		"\u0004\u0002\u0000\u0099\u0003\u0001\u0000\u0000\u0000\u009a\u00a0\u0005"+
		"\u001c\u0000\u0000\u009b\u009f\u0003\u0006\u0003\u0000\u009c\u009f\u0003"+
		"\b\u0004\u0000\u009d\u009f\u0003\u0080@\u0000\u009e\u009b\u0001\u0000"+
		"\u0000\u0000\u009e\u009c\u0001\u0000\u0000\u0000\u009e\u009d\u0001\u0000"+
		"\u0000\u0000\u009f\u00a2\u0001\u0000\u0000\u0000\u00a0\u009e\u0001\u0000"+
		"\u0000\u0000\u00a0\u00a1\u0001\u0000\u0000\u0000\u00a1\u00a3\u0001\u0000"+
		"\u0000\u0000\u00a2\u00a0\u0001\u0000\u0000\u0000\u00a3\u00a4\u0005\u001d"+
		"\u0000\u0000\u00a4\u0005\u0001\u0000\u0000\u0000\u00a5\u00a7\u0003x<\u0000"+
		"\u00a6\u00a5\u0001\u0000\u0000\u0000\u00a7\u00aa\u0001\u0000\u0000\u0000"+
		"\u00a8\u00a6\u0001\u0000\u0000\u0000\u00a8\u00a9\u0001\u0000\u0000\u0000"+
		"\u00a9\u00ab\u0001\u0000\u0000\u0000\u00aa\u00a8\u0001\u0000\u0000\u0000"+
		"\u00ab\u00ac\u0005\"\u0000\u0000\u00ac\u00ad\u0003\u0016\u000b\u0000\u00ad"+
		"\u0007\u0001\u0000\u0000\u0000\u00ae\u00b0\u0003~?\u0000\u00af\u00ae\u0001"+
		"\u0000\u0000\u0000\u00b0\u00b3\u0001\u0000\u0000\u0000\u00b1\u00af\u0001"+
		"\u0000\u0000\u0000\u00b1\u00b2\u0001\u0000\u0000\u0000\u00b2\u00b4\u0001"+
		"\u0000\u0000\u0000\u00b3\u00b1\u0001\u0000\u0000\u0000\u00b4\u00b5\u0005"+
		" \u0000\u0000\u00b5\u00b6\u0003p8\u0000\u00b6\u00b7\u0003\f\u0006\u0000"+
		"\u00b7\u00b8\u0003\n\u0005\u0000\u00b8\t\u0001\u0000\u0000\u0000\u00b9"+
		"\u00bb\u0005!\u0000\u0000\u00ba\u00b9\u0001\u0000\u0000\u0000\u00ba\u00bb"+
		"\u0001\u0000\u0000\u0000\u00bb\u00bc\u0001\u0000\u0000\u0000\u00bc\u00c3"+
		"\u0003\u0016\u000b\u0000\u00bd\u00be\u0005\u0012\u0000\u0000\u00be\u00c0"+
		"\u0003>\u001f\u0000\u00bf\u00c1\u0005\f\u0000\u0000\u00c0\u00bf\u0001"+
		"\u0000\u0000\u0000\u00c0\u00c1\u0001\u0000\u0000\u0000\u00c1\u00c3\u0001"+
		"\u0000\u0000\u0000\u00c2\u00ba\u0001\u0000\u0000\u0000\u00c2\u00bd\u0001"+
		"\u0000\u0000\u0000\u00c3\u000b\u0001\u0000\u0000\u0000\u00c4\u00c7\u0003"+
		"\u000e\u0007\u0000\u00c5\u00c7\u0005\u0019\u0000\u0000\u00c6\u00c4\u0001"+
		"\u0000\u0000\u0000\u00c6\u00c5\u0001\u0000\u0000\u0000\u00c7\u00ca\u0001"+
		"\u0000\u0000\u0000\u00c8\u00c6\u0001\u0000\u0000\u0000\u00c8\u00c9\u0001"+
		"\u0000\u0000\u0000\u00c9\r\u0001\u0000\u0000\u0000\u00ca\u00c8\u0001\u0000"+
		"\u0000\u0000\u00cb\u00cc\u0005/\u0000\u0000\u00cc\u00cd\u0003\u0010\b"+
		"\u0000\u00cd\u00ce\u00057\u0000\u0000\u00ce\u00cf\u0003\u0010\b\u0000"+
		"\u00cf\u000f\u0001\u0000\u0000\u0000\u00d0\u00d3\u0003\u0012\t\u0000\u00d1"+
		"\u00d3\u0003v;\u0000\u00d2\u00d0\u0001\u0000\u0000\u0000\u00d2\u00d1\u0001"+
		"\u0000\u0000\u0000\u00d3\u0011\u0001\u0000\u0000\u0000\u00d4\u00d5\u0005"+
		"\u000e\u0000\u0000\u00d5\u00e2\u0005\u001d\u0000\u0000\u00d6\u00d7\u0005"+
		"\u000e\u0000\u0000\u00d7\u00dc\u0003\u0014\n\u0000\u00d8\u00d9\u0005D"+
		"\u0000\u0000\u00d9\u00db\u0003\u0014\n\u0000\u00da\u00d8\u0001\u0000\u0000"+
		"\u0000\u00db\u00de\u0001\u0000\u0000\u0000\u00dc\u00da\u0001\u0000\u0000"+
		"\u0000\u00dc\u00dd\u0001\u0000\u0000\u0000\u00dd\u00df\u0001\u0000\u0000"+
		"\u0000\u00de\u00dc\u0001\u0000\u0000\u0000\u00df\u00e0\u0005\u001d\u0000"+
		"\u0000\u00e0\u00e2\u0001\u0000\u0000\u0000\u00e1\u00d4\u0001\u0000\u0000"+
		"\u0000\u00e1\u00d6\u0001\u0000\u0000\u0000\u00e2\u0013\u0001\u0000\u0000"+
		"\u0000\u00e3\u00e5\u0005L\u0000\u0000\u00e4\u00e3\u0001\u0000\u0000\u0000"+
		"\u00e4\u00e5\u0001\u0000\u0000\u0000\u00e5\u00e6\u0001\u0000\u0000\u0000"+
		"\u00e6\u00e7\u0005\r\u0000\u0000\u00e7\u00e8\u0005K\u0000\u0000\u00e8"+
		"\u00e9\u0003v;\u0000\u00e9\u0015\u0001\u0000\u0000\u0000\u00ea\u00f0\u0005"+
		"\u001c\u0000\u0000\u00eb\u00ef\u0003\u0018\f\u0000\u00ec\u00ef\u0005\u001e"+
		"\u0000\u0000\u00ed\u00ef\u0003\u0080@\u0000\u00ee\u00eb\u0001\u0000\u0000"+
		"\u0000\u00ee\u00ec\u0001\u0000\u0000\u0000\u00ee\u00ed\u0001\u0000\u0000"+
		"\u0000\u00ef\u00f2\u0001\u0000\u0000\u0000\u00f0\u00ee\u0001\u0000\u0000"+
		"\u0000\u00f0\u00f1\u0001\u0000\u0000\u0000\u00f1\u00f3\u0001\u0000\u0000"+
		"\u0000\u00f2\u00f0\u0001\u0000\u0000\u0000\u00f3\u00f4\u0005\u001d\u0000"+
		"\u0000\u00f4\u0017\u0001\u0000\u0000\u0000\u00f5\u00f6\u0003j5\u0000\u00f6"+
		"\u00f7\u0005\u0019\u0000\u0000\u00f7\u0103\u0001\u0000\u0000\u0000\u00f8"+
		"\u00fa\u0003j5\u0000\u00f9\u00f8\u0001\u0000\u0000\u0000\u00f9\u00fa\u0001"+
		"\u0000\u0000\u0000\u00fa\u00fb\u0001\u0000\u0000\u0000\u00fb\u0103\u0003"+
		"\u0016\u000b\u0000\u00fc\u0103\u0003\u001a\r\u0000\u00fd\u0103\u00032"+
		"\u0019\u0000\u00fe\u0103\u00036\u001b\u0000\u00ff\u0103\u0003*\u0015\u0000"+
		"\u0100\u0103\u0003\u001c\u000e\u0000\u0101\u0103\u0003$\u0012\u0000\u0102"+
		"\u00f5\u0001\u0000\u0000\u0000\u0102\u00f9\u0001\u0000\u0000\u0000\u0102"+
		"\u00fc\u0001\u0000\u0000\u0000\u0102\u00fd\u0001\u0000\u0000\u0000\u0102"+
		"\u00fe\u0001\u0000\u0000\u0000\u0102\u00ff\u0001\u0000\u0000\u0000\u0102"+
		"\u0100\u0001\u0000\u0000\u0000\u0102\u0101\u0001\u0000\u0000\u0000\u0103"+
		"\u0019\u0001\u0000\u0000\u0000\u0104\u0105\u0005\n\u0000\u0000\u0105\u0107"+
		"\u0003V+\u0000\u0106\u0108\u0003N\'\u0000\u0107\u0106\u0001\u0000\u0000"+
		"\u0000\u0107\u0108\u0001\u0000\u0000\u0000\u0108\u010c\u0001\u0000\u0000"+
		"\u0000\u0109\u010a\u0005\n\u0000\u0000\u010a\u010c\u0003N\'\u0000\u010b"+
		"\u0104\u0001\u0000\u0000\u0000\u010b\u0109\u0001\u0000\u0000\u0000\u010c"+
		"\u001b\u0001\u0000\u0000\u0000\u010d\u010e\u00050\u0000\u0000\u010e\u010f"+
		"\u0003\"\u0011\u0000\u010f\u011a\u0003\u0016\u000b\u0000\u0110\u0112\u0003"+
		"\u0080@\u0000\u0111\u0110\u0001\u0000\u0000\u0000\u0112\u0115\u0001\u0000"+
		"\u0000\u0000\u0113\u0111\u0001\u0000\u0000\u0000\u0113\u0114\u0001\u0000"+
		"\u0000\u0000\u0114\u0116\u0001\u0000\u0000\u0000\u0115\u0113\u0001\u0000"+
		"\u0000\u0000\u0116\u0117\u0005&\u0000\u0000\u0117\u0118\u0003 \u0010\u0000"+
		"\u0118\u0119\u0005\u0019\u0000\u0000\u0119\u011b\u0001\u0000\u0000\u0000"+
		"\u011a\u0113\u0001\u0000\u0000\u0000\u011a\u011b\u0001\u0000\u0000\u0000"+
		"\u011b\u001d\u0001\u0000\u0000\u0000\u011c\u011d\u0005&\u0000\u0000\u011d"+
		"\u011e\u00057\u0000\u0000\u011e\u011f\u0003 \u0010\u0000\u011f\u0120\u0005"+
		"\u0019\u0000\u0000\u0120\u001f\u0001\u0000\u0000\u0000\u0121\u0124\u0003"+
		">\u001f\u0000\u0122\u0124\u0003\u0018\f\u0000\u0123\u0121\u0001\u0000"+
		"\u0000\u0000\u0123\u0122\u0001\u0000\u0000\u0000\u0124!\u0001\u0000\u0000"+
		"\u0000\u0125\u0126\u0003V+\u0000\u0126#\u0001\u0000\u0000\u0000\u0127"+
		"\u0128\u00050\u0000\u0000\u0128\u0129\u0005\u001c\u0000\u0000\u0129\u012a"+
		"\u0005\u0019\u0000\u0000\u012a\u012b\u0003&\u0013\u0000\u012b\u012c\u0005"+
		"\u001d\u0000\u0000\u012c%\u0001\u0000\u0000\u0000\u012d\u0132\u0003(\u0014"+
		"\u0000\u012e\u0131\u0003(\u0014\u0000\u012f\u0131\u0003\u0080@\u0000\u0130"+
		"\u012e\u0001\u0000\u0000\u0000\u0130\u012f\u0001\u0000\u0000\u0000\u0131"+
		"\u0134\u0001\u0000\u0000\u0000\u0132\u0130\u0001\u0000\u0000\u0000\u0132"+
		"\u0133\u0001\u0000\u0000\u0000\u0133\u0136\u0001\u0000\u0000\u0000\u0134"+
		"\u0132\u0001\u0000\u0000\u0000\u0135\u0137\u0003\u001e\u000f\u0000\u0136"+
		"\u0135\u0001\u0000\u0000\u0000\u0136\u0137\u0001\u0000\u0000\u0000\u0137"+
		"\'\u0001\u0000\u0000\u0000\u0138\u0139\u0003\"\u0011\u0000\u0139\u013a"+
		"\u00057\u0000\u0000\u013a\u013b\u0003 \u0010\u0000\u013b\u013c\u0005\u0019"+
		"\u0000\u0000\u013c)\u0001\u0000\u0000\u0000\u013d\u013f\u00050\u0000\u0000"+
		"\u013e\u0140\u0003>\u001f\u0000\u013f\u013e\u0001\u0000\u0000\u0000\u013f"+
		"\u0140\u0001\u0000\u0000\u0000\u0140\u0141\u0001\u0000\u0000\u0000\u0141"+
		"\u0142\u0005\u001c\u0000\u0000\u0142\u0143\u0005\u0019\u0000\u0000\u0143"+
		"\u0144\u0003,\u0016\u0000\u0144\u0145\u0005\u001d\u0000\u0000\u0145+\u0001"+
		"\u0000\u0000\u0000\u0146\u0148\u0003\u0080@\u0000\u0147\u0146\u0001\u0000"+
		"\u0000\u0000\u0148\u014b\u0001\u0000\u0000\u0000\u0149\u0147\u0001\u0000"+
		"\u0000\u0000\u0149\u014a\u0001\u0000\u0000\u0000\u014a\u014c\u0001\u0000"+
		"\u0000\u0000\u014b\u0149\u0001\u0000\u0000\u0000\u014c\u0151\u0003.\u0017"+
		"\u0000\u014d\u0150\u0003.\u0017\u0000\u014e\u0150\u0003\u0080@\u0000\u014f"+
		"\u014d\u0001\u0000\u0000\u0000\u014f\u014e\u0001\u0000\u0000\u0000\u0150"+
		"\u0153\u0001\u0000\u0000\u0000\u0151\u014f\u0001\u0000\u0000\u0000\u0151"+
		"\u0152\u0001\u0000\u0000\u0000\u0152\u0155\u0001\u0000\u0000\u0000\u0153"+
		"\u0151\u0001\u0000\u0000\u0000\u0154\u0156\u0003\u001e\u000f\u0000\u0155"+
		"\u0154\u0001\u0000\u0000\u0000\u0155\u0156\u0001\u0000\u0000\u0000\u0156"+
		"-\u0001\u0000\u0000\u0000\u0157\u0158\u00030\u0018\u0000\u0158\u0159\u0005"+
		"7\u0000\u0000\u0159\u015a\u0003 \u0010\u0000\u015a\u015b\u0005\u0019\u0000"+
		"\u0000\u015b/\u0001\u0000\u0000\u0000\u015c\u015f\u0003>\u001f\u0000\u015d"+
		"\u015f\u0003^/\u0000\u015e\u015c\u0001\u0000\u0000\u0000\u015e\u015d\u0001"+
		"\u0000\u0000\u0000\u015f1\u0001\u0000\u0000\u0000\u0160\u0161\u0003|>"+
		"\u0000\u0161\u0162\u0003r9\u0000\u0162\u0163\u00034\u001a\u0000\u0163"+
		"3\u0001\u0000\u0000\u0000\u0164\u0169\u00056\u0000\u0000\u0165\u016a\u0003"+
		">\u001f\u0000\u0166\u016a\u0003*\u0015\u0000\u0167\u016a\u0003$\u0012"+
		"\u0000\u0168\u016a\u0003\u001a\r\u0000\u0169\u0165\u0001\u0000\u0000\u0000"+
		"\u0169\u0166\u0001\u0000\u0000\u0000\u0169\u0167\u0001\u0000\u0000\u0000"+
		"\u0169\u0168\u0001\u0000\u0000\u0000\u016a\u016d\u0001\u0000\u0000\u0000"+
		"\u016b\u016d\u0003N\'\u0000\u016c\u0164\u0001\u0000\u0000\u0000\u016c"+
		"\u016b\u0001\u0000\u0000\u0000\u016d5\u0001\u0000\u0000\u0000\u016e\u0170"+
		"\u0005\'\u0000\u0000\u016f\u0171\u0003V+\u0000\u0170\u016f\u0001\u0000"+
		"\u0000\u0000\u0170\u0171\u0001\u0000\u0000\u0000\u0171\u0177\u0001\u0000"+
		"\u0000\u0000\u0172\u0173\u0005(\u0000\u0000\u0173\u0177\u0005\n\u0000"+
		"\u0000\u0174\u0177\u0005*\u0000\u0000\u0175\u0177\u0005)\u0000\u0000\u0176"+
		"\u016e\u0001\u0000\u0000\u0000\u0176\u0172\u0001\u0000\u0000\u0000\u0176"+
		"\u0174\u0001\u0000\u0000\u0000\u0176\u0175\u0001\u0000\u0000\u0000\u0177"+
		"7\u0001\u0000\u0000\u0000\u0178\u0179\u0005\u0007\u0000\u0000\u0179\u017a"+
		"\u0003:\u001d\u0000\u017a\u017b\u0005\u0002\u0000\u0000\u017b9\u0001\u0000"+
		"\u0000\u0000\u017c\u0181\u0005=\u0000\u0000\u017d\u0181\u0005>\u0000\u0000"+
		"\u017e\u0181\u0005?\u0000\u0000\u017f\u0181\u0003<\u001e\u0000\u0180\u017c"+
		"\u0001\u0000\u0000\u0000\u0180\u017d\u0001\u0000\u0000\u0000\u0180\u017e"+
		"\u0001\u0000\u0000\u0000\u0180\u017f\u0001\u0000\u0000\u0000\u0181\u0182"+
		"\u0001\u0000\u0000\u0000\u0182\u0180\u0001\u0000\u0000\u0000\u0182\u0183"+
		"\u0001\u0000\u0000\u0000\u0183;\u0001\u0000\u0000\u0000\u0184\u0185\u0005"+
		"\b\u0000\u0000\u0185\u0186\u0003`0\u0000\u0186\u0188\u0005\u001d\u0000"+
		"\u0000\u0187\u0189\u0005Q\u0000\u0000\u0188\u0187\u0001\u0000\u0000\u0000"+
		"\u0188\u0189\u0001\u0000\u0000\u0000\u0189=\u0001\u0000\u0000\u0000\u018a"+
		"\u018d\u0003B!\u0000\u018b\u018d\u0003X,\u0000\u018c\u018a\u0001\u0000"+
		"\u0000\u0000\u018c\u018b\u0001\u0000\u0000\u0000\u018d?\u0001\u0000\u0000"+
		"\u0000\u018e\u018f\u00058\u0000\u0000\u018f\u0190\u0003>\u001f\u0000\u0190"+
		"\u0191\u0005\f\u0000\u0000\u0191A\u0001\u0000\u0000\u0000\u0192\u0193"+
		"\u0006!\uffff\uffff\u0000\u0193\u0195\u0003D\"\u0000\u0194\u0196\u0003"+
		"d2\u0000\u0195\u0194\u0001\u0000\u0000\u0000\u0195\u0196\u0001\u0000\u0000"+
		"\u0000\u0196\u019b\u0001\u0000\u0000\u0000\u0197\u0198\u0003H$\u0000\u0198"+
		"\u0199\u0003B!\u0002\u0199\u019b\u0001\u0000\u0000\u0000\u019a\u0192\u0001"+
		"\u0000\u0000\u0000\u019a\u0197\u0001\u0000\u0000\u0000\u019b\u01a0\u0001"+
		"\u0000\u0000\u0000\u019c\u019d\n\u0001\u0000\u0000\u019d\u019f\u0003J"+
		"%\u0000\u019e\u019c\u0001\u0000\u0000\u0000\u019f\u01a2\u0001\u0000\u0000"+
		"\u0000\u01a0\u019e\u0001\u0000\u0000\u0000\u01a0\u01a1\u0001\u0000\u0000"+
		"\u0000\u01a1C\u0001\u0000\u0000\u0000\u01a2\u01a0\u0001\u0000\u0000\u0000"+
		"\u01a3\u01ae\u0003\\.\u0000\u01a4\u01ae\u0003F#\u0000\u01a5\u01ae\u0003"+
		"`0\u0000\u01a6\u01ae\u0003R)\u0000\u01a7\u01ae\u0003P(\u0000\u01a8\u01ae"+
		"\u0003@ \u0000\u01a9\u01ae\u0003L&\u0000\u01aa\u01ae\u0005\u0013\u0000"+
		"\u0000\u01ab\u01ae\u0005\u0014\u0000\u0000\u01ac\u01ae\u0005\u0015\u0000"+
		"\u0000\u01ad\u01a3\u0001\u0000\u0000\u0000\u01ad\u01a4\u0001\u0000\u0000"+
		"\u0000\u01ad\u01a5\u0001\u0000\u0000\u0000\u01ad\u01a6\u0001\u0000\u0000"+
		"\u0000\u01ad\u01a7\u0001\u0000\u0000\u0000\u01ad\u01a8\u0001\u0000\u0000"+
		"\u0000\u01ad\u01a9\u0001\u0000\u0000\u0000\u01ad\u01aa\u0001\u0000\u0000"+
		"\u0000\u01ad\u01ab\u0001\u0000\u0000\u0000\u01ad\u01ac\u0001\u0000\u0000"+
		"\u0000\u01aeE\u0001\u0000\u0000\u0000\u01af\u01b5\u0005\u0010\u0000\u0000"+
		"\u01b0\u01b4\u0003\u0018\f\u0000\u01b1\u01b4\u0005\u001e\u0000\u0000\u01b2"+
		"\u01b4\u0003\u0080@\u0000\u01b3\u01b0\u0001\u0000\u0000\u0000\u01b3\u01b1"+
		"\u0001\u0000\u0000\u0000\u01b3\u01b2\u0001\u0000\u0000\u0000\u01b4\u01b7"+
		"\u0001\u0000\u0000\u0000\u01b5\u01b3\u0001\u0000\u0000\u0000\u01b5\u01b6"+
		"\u0001\u0000\u0000\u0000\u01b6\u01b8\u0001\u0000\u0000\u0000\u01b7\u01b5"+
		"\u0001\u0000\u0000\u0000\u01b8\u01bf\u0005\u001d\u0000\u0000\u01b9\u01ba"+
		"\u0005\u0011\u0000\u0000\u01ba\u01bc\u0003>\u001f\u0000\u01bb\u01bd\u0005"+
		"\f\u0000\u0000\u01bc\u01bb\u0001\u0000\u0000\u0000\u01bc\u01bd\u0001\u0000"+
		"\u0000\u0000\u01bd\u01bf\u0001\u0000\u0000\u0000\u01be\u01af\u0001\u0000"+
		"\u0000\u0000\u01be\u01b9\u0001\u0000\u0000\u0000\u01bfG\u0001\u0000\u0000"+
		"\u0000\u01c0\u01c1\u0005H\u0000\u0000\u01c1I\u0001\u0000\u0000\u0000\u01c2"+
		"\u01c3\u0005I\u0000\u0000\u01c3K\u0001\u0000\u0000\u0000\u01c4\u01c5\u0003"+
		"t:\u0000\u01c5\u01c6\u0003V+\u0000\u01c6M\u0001\u0000\u0000\u0000\u01c7"+
		"\u01c9\u0005\u0019\u0000\u0000\u01c8\u01c7\u0001\u0000\u0000\u0000\u01c8"+
		"\u01c9\u0001\u0000\u0000\u0000\u01c9\u01ca\u0001\u0000\u0000\u0000\u01ca"+
		"\u01cd\u00038\u001c\u0000\u01cb\u01cd\u0003B!\u0000\u01cc\u01c8\u0001"+
		"\u0000\u0000\u0000\u01cc\u01cb\u0001\u0000\u0000\u0000\u01cdO\u0001\u0000"+
		"\u0000\u0000\u01ce\u01cf\u0005\u000f\u0000\u0000\u01cf\u01dc\u0005F\u0000"+
		"\u0000\u01d0\u01d1\u0005\u000f\u0000\u0000\u01d1\u01d6\u0003>\u001f\u0000"+
		"\u01d2\u01d3\u0005D\u0000\u0000\u01d3\u01d5\u0003>\u001f\u0000\u01d4\u01d2"+
		"\u0001\u0000\u0000\u0000\u01d5\u01d8\u0001\u0000\u0000\u0000\u01d6\u01d4"+
		"\u0001\u0000\u0000\u0000\u01d6\u01d7\u0001\u0000\u0000\u0000\u01d7\u01d9"+
		"\u0001\u0000\u0000\u0000\u01d8\u01d6\u0001\u0000\u0000\u0000\u01d9\u01da"+
		"\u0005F\u0000\u0000\u01da\u01dc\u0001\u0000\u0000\u0000\u01db\u01ce\u0001"+
		"\u0000\u0000\u0000\u01db\u01d0\u0001\u0000\u0000\u0000\u01dcQ\u0001\u0000"+
		"\u0000\u0000\u01dd\u01de\u0005\u000e\u0000\u0000\u01de\u01eb\u0005\u001d"+
		"\u0000\u0000\u01df\u01e0\u0005\u000e\u0000\u0000\u01e0\u01e5\u0003T*\u0000"+
		"\u01e1\u01e2\u0005D\u0000\u0000\u01e2\u01e4\u0003T*\u0000\u01e3\u01e1"+
		"\u0001\u0000\u0000\u0000\u01e4\u01e7\u0001\u0000\u0000\u0000\u01e5\u01e3"+
		"\u0001\u0000\u0000\u0000\u01e5\u01e6\u0001\u0000\u0000\u0000\u01e6\u01e8"+
		"\u0001\u0000\u0000\u0000\u01e7\u01e5\u0001\u0000\u0000\u0000\u01e8\u01e9"+
		"\u0005\u001d\u0000\u0000\u01e9\u01eb\u0001\u0000\u0000\u0000\u01ea\u01dd"+
		"\u0001\u0000\u0000\u0000\u01ea\u01df\u0001\u0000\u0000\u0000\u01ebS\u0001"+
		"\u0000\u0000\u0000\u01ec\u01ed\u0005\r\u0000\u0000\u01ed\u01ee\u0005K"+
		"\u0000\u0000\u01ee\u01ef\u0003>\u001f\u0000\u01efU\u0001\u0000\u0000\u0000"+
		"\u01f0\u01f1\u0005:\u0000\u0000\u01f1\u01f6\u0003>\u001f\u0000\u01f2\u01f3"+
		"\u0005D\u0000\u0000\u01f3\u01f5\u0003>\u001f\u0000\u01f4\u01f2\u0001\u0000"+
		"\u0000\u0000\u01f5\u01f8\u0001\u0000\u0000\u0000\u01f6\u01f4\u0001\u0000"+
		"\u0000\u0000\u01f6\u01f7\u0001\u0000\u0000\u0000\u01f7\u01f9\u0001\u0000"+
		"\u0000\u0000\u01f8\u01f6\u0001\u0000\u0000\u0000\u01f9\u01fa\u0005E\u0000"+
		"\u0000\u01fa\u0204\u0001\u0000\u0000\u0000\u01fb\u01fc\u0005:\u0000\u0000"+
		"\u01fc\u0204\u0005E\u0000\u0000\u01fd\u0204\u00059\u0000\u0000\u01fe\u0200"+
		"\u0003X,\u0000\u01ff\u01fe\u0001\u0000\u0000\u0000\u0200\u0201\u0001\u0000"+
		"\u0000\u0000\u0201\u01ff\u0001\u0000\u0000\u0000\u0201\u0202\u0001\u0000"+
		"\u0000\u0000\u0202\u0204\u0001\u0000\u0000\u0000\u0203\u01f0\u0001\u0000"+
		"\u0000\u0000\u0203\u01fb\u0001\u0000\u0000\u0000\u0203\u01fd\u0001\u0000"+
		"\u0000\u0000\u0203\u01ff\u0001\u0000\u0000\u0000\u0204W\u0001\u0000\u0000"+
		"\u0000\u0205\u0206\u0005\u0004\u0000\u0000\u0206\u0207\u0003Z-\u0000\u0207"+
		"\u0208\u0005<\u0000\u0000\u0208Y\u0001\u0000\u0000\u0000\u0209\u020d\u0005"+
		";\u0000\u0000\u020a\u020d\u0003\u0084B\u0000\u020b\u020d\u0003\\.\u0000"+
		"\u020c\u0209\u0001\u0000\u0000\u0000\u020c\u020a\u0001\u0000\u0000\u0000"+
		"\u020c\u020b\u0001\u0000\u0000\u0000\u020d\u0210\u0001\u0000\u0000\u0000"+
		"\u020e\u020c\u0001\u0000\u0000\u0000\u020e\u020f\u0001\u0000\u0000\u0000"+
		"\u020f[\u0001\u0000\u0000\u0000\u0210\u020e\u0001\u0000\u0000\u0000\u0211"+
		"\u0212\u0005\t\u0000\u0000\u0212\u0213\u0003>\u001f\u0000\u0213\u0214"+
		"\u0005F\u0000\u0000\u0214]\u0001\u0000\u0000\u0000\u0215\u0217\u0005\t"+
		"\u0000\u0000\u0216\u0218\u0003f3\u0000\u0217\u0216\u0001\u0000\u0000\u0000"+
		"\u0218\u0219\u0001\u0000\u0000\u0000\u0219\u0217\u0001\u0000\u0000\u0000"+
		"\u0219\u021a\u0001\u0000\u0000\u0000\u021a\u021b\u0001\u0000\u0000\u0000"+
		"\u021b\u021c\u0005F\u0000\u0000\u021c_\u0001\u0000\u0000\u0000\u021d\u0221"+
		"\u0003b1\u0000\u021e\u0220\u0003f3\u0000\u021f\u021e\u0001\u0000\u0000"+
		"\u0000\u0220\u0223\u0001\u0000\u0000\u0000\u0221\u021f\u0001\u0000\u0000"+
		"\u0000\u0221\u0222\u0001\u0000\u0000\u0000\u0222a\u0001\u0000\u0000\u0000"+
		"\u0223\u0221\u0001\u0000\u0000\u0000\u0224\u0228\u0005\r\u0000\u0000\u0225"+
		"\u0226\u0005G\u0000\u0000\u0226\u0228\u0007\u0000\u0000\u0000\u0227\u0224"+
		"\u0001\u0000\u0000\u0000\u0227\u0225\u0001\u0000\u0000\u0000\u0228c\u0001"+
		"\u0000\u0000\u0000\u0229\u022a\u0005J\u0000\u0000\u022a\u022b\u0003>\u001f"+
		"\u0000\u022be\u0001\u0000\u0000\u0000\u022c\u022d\u0003h4\u0000\u022d"+
		"\u022f\u0003t:\u0000\u022e\u0230\u0003V+\u0000\u022f\u022e\u0001\u0000"+
		"\u0000\u0000\u022f\u0230\u0001\u0000\u0000\u0000\u0230\u0234\u0001\u0000"+
		"\u0000\u0000\u0231\u0234\u0003l6\u0000\u0232\u0234\u0005C\u0000\u0000"+
		"\u0233\u022c\u0001\u0000\u0000\u0000\u0233\u0231\u0001\u0000\u0000\u0000"+
		"\u0233\u0232\u0001\u0000\u0000\u0000\u0234g\u0001\u0000\u0000\u0000\u0235"+
		"\u0236\u0007\u0001\u0000\u0000\u0236i\u0001\u0000\u0000\u0000\u0237\u0238"+
		"\u00055\u0000\u0000\u0238\u0239\u0005T\u0000\u0000\u0239k\u0001\u0000"+
		"\u0000\u0000\u023a\u023b\u0005B\u0000\u0000\u023b\u023c\u0005S\u0000\u0000"+
		"\u023cm\u0001\u0000\u0000\u0000\u023d\u0241\u0005\n\u0000\u0000\u023e"+
		"\u0241\u0005\u000b\u0000\u0000\u023f\u0241\u0003X,\u0000\u0240\u023d\u0001"+
		"\u0000\u0000\u0000\u0240\u023e\u0001\u0000\u0000\u0000\u0240\u023f\u0001"+
		"\u0000\u0000\u0000\u0241o\u0001\u0000\u0000\u0000\u0242\u0245\u0005\n"+
		"\u0000\u0000\u0243\u0245\u0003X,\u0000\u0244\u0242\u0001\u0000\u0000\u0000"+
		"\u0244\u0243\u0001\u0000\u0000\u0000\u0245q\u0001\u0000\u0000\u0000\u0246"+
		"\u0247\u0005\n\u0000\u0000\u0247s\u0001\u0000\u0000\u0000\u0248\u0249"+
		"\u0007\u0002\u0000\u0000\u0249u\u0001\u0000\u0000\u0000\u024a\u024b\u0007"+
		"\u0003\u0000\u0000\u024bw\u0001\u0000\u0000\u0000\u024c\u024d\u0007\u0004"+
		"\u0000\u0000\u024dy\u0001\u0000\u0000\u0000\u024e\u024f\u0005%\u0000\u0000"+
		"\u024f{\u0001\u0000\u0000\u0000\u0250\u0252\u0005.\u0000\u0000\u0251\u0250"+
		"\u0001\u0000\u0000\u0000\u0251\u0252\u0001\u0000\u0000\u0000\u0252\u0253"+
		"\u0001\u0000\u0000\u0000\u0253\u0259\u0005-\u0000\u0000\u0254\u0256\u0005"+
		"+\u0000\u0000\u0255\u0254\u0001\u0000\u0000\u0000\u0255\u0256\u0001\u0000"+
		"\u0000\u0000\u0256\u0257\u0001\u0000\u0000\u0000\u0257\u0259\u0005,\u0000"+
		"\u0000\u0258\u0251\u0001\u0000\u0000\u0000\u0258\u0255\u0001\u0000\u0000"+
		"\u0000\u0259}\u0001\u0000\u0000\u0000\u025a\u025b\u0007\u0005\u0000\u0000"+
		"\u025b\u007f\u0001\u0000\u0000\u0000\u025c\u025f\u0003\u0082A\u0000\u025d"+
		"\u025f\u0005\u0019\u0000\u0000\u025e\u025c\u0001\u0000\u0000\u0000\u025e"+
		"\u025d\u0001\u0000\u0000\u0000\u025f\u0081\u0001\u0000\u0000\u0000\u0260"+
		"\u0261\u0007\u0006\u0000\u0000\u0261\u0083\u0001\u0000\u0000\u0000\u0262"+
		"\u0263\u0005\u0003\u0000\u0000\u0263\u0264\u0007\u0007\u0000\u0000\u0264"+
		"\u0085\u0001\u0000\u0000\u0000H\u0089\u008b\u0093\u009e\u00a0\u00a8\u00b1"+
		"\u00ba\u00c0\u00c2\u00c6\u00c8\u00d2\u00dc\u00e1\u00e4\u00ee\u00f0\u00f9"+
		"\u0102\u0107\u010b\u0113\u011a\u0123\u0130\u0132\u0136\u013f\u0149\u014f"+
		"\u0151\u0155\u015e\u0169\u016c\u0170\u0176\u0180\u0182\u0188\u018c\u0195"+
		"\u019a\u01a0\u01ad\u01b3\u01b5\u01bc\u01be\u01c8\u01cc\u01d6\u01db\u01e5"+
		"\u01ea\u01f6\u0201\u0203\u020c\u020e\u0219\u0221\u0227\u022f\u0233\u0240"+
		"\u0244\u0251\u0255\u0258\u025e";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}