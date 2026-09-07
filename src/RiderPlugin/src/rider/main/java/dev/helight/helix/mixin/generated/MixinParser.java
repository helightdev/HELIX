// Generated from MixinParser.g4 by ANTLR 4.13.2
package dev.helight.helix.mixin.generated;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast", "CheckReturnValue", "this-escape"})
public class MixinParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.13.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		ERROR_TOKEN=1, TERMINATOR=2, ESCAPE=3, BEGIN_ARGUMENT=4, BEGIN_VALUE=5, 
		BEGIN_VALUE_ESCAPED=6, BEGIN_CONTENT=7, BEGIN_VALUE_INTERPOLATE=8, BEGIN_VALUE_INLINE=9, 
		IDENTIFIER=10, NAMESPACE_IDENTIFIER=11, VALUE_END=12, ROOT_IDENTIFIER=13, 
		BEGIN_TABLE=14, BEGIN_TUPLE=15, ESCAPED_AT=16, ESCAPED_START=17, OUTER_WHITESPACE=18, 
		NEWLINE=19, COMMENT=20, SLASH_COMMENT=21, LC=22, RC=23, SEMICOLON=24, 
		COMMA=25, KEYWORD_FUNC=26, KEYWORD_DO=27, KEYWORD_EXPRESSION=28, KEYWORD_MIXIN=29, 
		KEYWORD_PRELUDE=30, KEYWORD_DERIVATION=31, KEYWORD_ELSE=32, KEYWORD_RETURN=33, 
		KEYWORD_GOTO=34, KEYWORD_BREAK=35, KEYWORD_CONTINUE=36, KEYWORD_TARGET=37, 
		KEYWORD_VAR=38, KEYWORD_LOCAL=39, KEYWORD_CARRY=40, KEYWORD_SIG=41, KEYWORD_WHEN=42, 
		KEYWORD_PURE=43, KEYWORD_INLINE=44, KEYWORD_NOINLINE=45, KEYWORD_STRICT=46, 
		LABEL_PREFIX=47, ASSIGN=48, ARROW=49, TOPLEVEL_VALUE_EXPRESSION=50, EMPTY_PARAMETERS=51, 
		BEGIN_PARAMETERS=52, ARGUMENT_TEXT=53, ARGUMENT_END=54, CONTENT_WRAP=55, 
		CONTENT_LINEBREAK=56, CONTENT_TEXT=57, VALUE_FUNCTION=58, VALUE_PREDICATE=59, 
		VALUE_MEMBER=60, VALUE_WRAP=61, VALUE_DELIMITER=62, END_PARAMETERS=63, 
		VALUE_END_INTERPOLATE=64, VALUE_END_INLINE=65, VALUE_SMART_ROOT=66, NOT_VALUE=67, 
		VALUE_CHECK=68, VALUE_ELVIS=69, VALUE_ASSIGN=70, VALUE_EXPAND=71, VALUE_WHITESPACE=72, 
		ESCAPE_MACRO=73, ESCAPE_LITERAL=74, ESCAPE_HEX=75, END_CONTENT_INTERPOLATE=76, 
		FUNCTION_IDENTIFIER=77, MEMBER_IDENTIFIER=78, LABEL_IDENTIFIER=79;
	public static final int
		RULE_compilationUnit = 0, RULE_mixinDeclaration = 1, RULE_mixinBody = 2, 
		RULE_expressionDeclaration = 3, RULE_funcDeclaration = 4, RULE_functionMetadata = 5, 
		RULE_functionSignatureVariant = 6, RULE_signature = 7, RULE_tableSignature = 8, 
		RULE_tableSignatureEntry = 9, RULE_statementBlock = 10, RULE_statement = 11, 
		RULE_invocationStatement = 12, RULE_whenConditionStatement = 13, RULE_whenElseBranch = 14, 
		RULE_whenResult = 15, RULE_whenChainCondition = 16, RULE_whenChainStatement = 17, 
		RULE_whenChainBody = 18, RULE_whenChainBranch = 19, RULE_whenValueStatement = 20, 
		RULE_whenValueBody = 21, RULE_whenValueBranch = 22, RULE_whenValueCondition = 23, 
		RULE_assignmentStatement = 24, RULE_assignedValue = 25, RULE_controlflowStatement = 26, 
		RULE_contentBlock = 27, RULE_contentBody = 28, RULE_contentInterpolate = 29, 
		RULE_value = 30, RULE_valueExpression = 31, RULE_nonArgumentValue = 32, 
		RULE_primaryValue = 33, RULE_prefixOperators = 34, RULE_postfixOperators = 35, 
		RULE_valueStatement = 36, RULE_tailValue = 37, RULE_tupleValue = 38, RULE_tableValue = 39, 
		RULE_tableKeyedEntry = 40, RULE_valueList = 41, RULE_argumentValue = 42, 
		RULE_argumentBody = 43, RULE_inlineValue = 44, RULE_inlineTransformation = 45, 
		RULE_derivation = 46, RULE_derivationRoot = 47, RULE_elvisValue = 48, 
		RULE_transformationPart = 49, RULE_functionChainType = 50, RULE_labelIdentifier = 51, 
		RULE_memberIdentifier = 52, RULE_mixinIdentifier = 53, RULE_variableIdentifier = 54, 
		RULE_functionIdentifier = 55, RULE_expressionModifier = 56, RULE_mixinModifier = 57, 
		RULE_variableSpecifiers = 58, RULE_funcModifier = 59, RULE_trivia = 60, 
		RULE_comment = 61, RULE_escaped = 62;
	private static String[] makeRuleNames() {
		return new String[] {
			"compilationUnit", "mixinDeclaration", "mixinBody", "expressionDeclaration", 
			"funcDeclaration", "functionMetadata", "functionSignatureVariant", "signature", 
			"tableSignature", "tableSignatureEntry", "statementBlock", "statement", 
			"invocationStatement", "whenConditionStatement", "whenElseBranch", "whenResult", 
			"whenChainCondition", "whenChainStatement", "whenChainBody", "whenChainBranch", 
			"whenValueStatement", "whenValueBody", "whenValueBranch", "whenValueCondition", 
			"assignmentStatement", "assignedValue", "controlflowStatement", "contentBlock", 
			"contentBody", "contentInterpolate", "value", "valueExpression", "nonArgumentValue", 
			"primaryValue", "prefixOperators", "postfixOperators", "valueStatement", 
			"tailValue", "tupleValue", "tableValue", "tableKeyedEntry", "valueList", 
			"argumentValue", "argumentBody", "inlineValue", "inlineTransformation", 
			"derivation", "derivationRoot", "elvisValue", "transformationPart", "functionChainType", 
			"labelIdentifier", "memberIdentifier", "mixinIdentifier", "variableIdentifier", 
			"functionIdentifier", "expressionModifier", "mixinModifier", "variableSpecifiers", 
			"funcModifier", "trivia", "comment", "escaped"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, null, null, null, null, null, null, null, null, null, null, 
			null, null, null, null, "'@@'", "'\\'", null, null, null, null, "'{'", 
			null, null, null, "'func'", "'do'", "'expression'", "'mixin'", "'prelude'", 
			"'derivation'", "'else'", "'return'", "'goto'", "'break'", "'continue'", 
			"'target'", "'var'", "'local'", "'carry'", "'sig'", "'when'", "'pure'", 
			"'inline'", "'noinline'", "'strict'", null, null, "'->'", null, null, 
			"'('", null, "'>'", null, null, null, null, "':?'", "'#'", null, null, 
			"')'", null, "']'", "'$'", "'!'", "'?'", "'?:'", null, "'...'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "ERROR_TOKEN", "TERMINATOR", "ESCAPE", "BEGIN_ARGUMENT", "BEGIN_VALUE", 
			"BEGIN_VALUE_ESCAPED", "BEGIN_CONTENT", "BEGIN_VALUE_INTERPOLATE", "BEGIN_VALUE_INLINE", 
			"IDENTIFIER", "NAMESPACE_IDENTIFIER", "VALUE_END", "ROOT_IDENTIFIER", 
			"BEGIN_TABLE", "BEGIN_TUPLE", "ESCAPED_AT", "ESCAPED_START", "OUTER_WHITESPACE", 
			"NEWLINE", "COMMENT", "SLASH_COMMENT", "LC", "RC", "SEMICOLON", "COMMA", 
			"KEYWORD_FUNC", "KEYWORD_DO", "KEYWORD_EXPRESSION", "KEYWORD_MIXIN", 
			"KEYWORD_PRELUDE", "KEYWORD_DERIVATION", "KEYWORD_ELSE", "KEYWORD_RETURN", 
			"KEYWORD_GOTO", "KEYWORD_BREAK", "KEYWORD_CONTINUE", "KEYWORD_TARGET", 
			"KEYWORD_VAR", "KEYWORD_LOCAL", "KEYWORD_CARRY", "KEYWORD_SIG", "KEYWORD_WHEN", 
			"KEYWORD_PURE", "KEYWORD_INLINE", "KEYWORD_NOINLINE", "KEYWORD_STRICT", 
			"LABEL_PREFIX", "ASSIGN", "ARROW", "TOPLEVEL_VALUE_EXPRESSION", "EMPTY_PARAMETERS", 
			"BEGIN_PARAMETERS", "ARGUMENT_TEXT", "ARGUMENT_END", "CONTENT_WRAP", 
			"CONTENT_LINEBREAK", "CONTENT_TEXT", "VALUE_FUNCTION", "VALUE_PREDICATE", 
			"VALUE_MEMBER", "VALUE_WRAP", "VALUE_DELIMITER", "END_PARAMETERS", "VALUE_END_INTERPOLATE", 
			"VALUE_END_INLINE", "VALUE_SMART_ROOT", "NOT_VALUE", "VALUE_CHECK", "VALUE_ELVIS", 
			"VALUE_ASSIGN", "VALUE_EXPAND", "VALUE_WHITESPACE", "ESCAPE_MACRO", "ESCAPE_LITERAL", 
			"ESCAPE_HEX", "END_CONTENT_INTERPOLATE", "FUNCTION_IDENTIFIER", "MEMBER_IDENTIFIER", 
			"LABEL_IDENTIFIER"
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
	public String getGrammarFileName() { return "MixinParser.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public MixinParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@SuppressWarnings("CheckReturnValue")
	public static class CompilationUnitContext extends ParserRuleContext {
		public TerminalNode EOF() { return getToken(MixinParser.EOF, 0); }
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitCompilationUnit(this);
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
			setState(131);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 61575406288896L) != 0)) {
				{
				setState(129);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(126);
					trivia();
					}
					break;
				case KEYWORD_MIXIN:
				case KEYWORD_DERIVATION:
					{
					setState(127);
					mixinDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(128);
					funcDeclaration();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(133);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(134);
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
		public TerminalNode KEYWORD_MIXIN() { return getToken(MixinParser.KEYWORD_MIXIN, 0); }
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitMixinDeclaration(this);
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
			setState(139);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_DERIVATION) {
				{
				{
				setState(136);
				mixinModifier();
				}
				}
				setState(141);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(142);
			match(KEYWORD_MIXIN);
			setState(143);
			mixinIdentifier();
			setState(144);
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
		public TerminalNode LC() { return getToken(MixinParser.LC, 0); }
		public TerminalNode RC() { return getToken(MixinParser.RC, 0); }
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitMixinBody(this);
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
			setState(146);
			match(LC);
			setState(152);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 131942808289280L) != 0)) {
				{
				setState(150);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_EXPRESSION:
				case KEYWORD_PRELUDE:
				case KEYWORD_STRICT:
					{
					setState(147);
					expressionDeclaration();
					}
					break;
				case KEYWORD_FUNC:
				case KEYWORD_PURE:
				case KEYWORD_INLINE:
				case KEYWORD_NOINLINE:
					{
					setState(148);
					funcDeclaration();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(149);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(154);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(155);
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
		public TerminalNode KEYWORD_EXPRESSION() { return getToken(MixinParser.KEYWORD_EXPRESSION, 0); }
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitExpressionDeclaration(this);
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
			setState(160);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==KEYWORD_PRELUDE || _la==KEYWORD_STRICT) {
				{
				{
				setState(157);
				expressionModifier();
				}
				}
				setState(162);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(163);
			match(KEYWORD_EXPRESSION);
			setState(164);
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
		public TerminalNode KEYWORD_FUNC() { return getToken(MixinParser.KEYWORD_FUNC, 0); }
		public TerminalNode IDENTIFIER() { return getToken(MixinParser.IDENTIFIER, 0); }
		public FunctionMetadataContext functionMetadata() {
			return getRuleContext(FunctionMetadataContext.class,0);
		}
		public StatementBlockContext statementBlock() {
			return getRuleContext(StatementBlockContext.class,0);
		}
		public List<FuncModifierContext> funcModifier() {
			return getRuleContexts(FuncModifierContext.class);
		}
		public FuncModifierContext funcModifier(int i) {
			return getRuleContext(FuncModifierContext.class,i);
		}
		public TerminalNode KEYWORD_DO() { return getToken(MixinParser.KEYWORD_DO, 0); }
		public FuncDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_funcDeclaration; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitFuncDeclaration(this);
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
			setState(169);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 61572651155456L) != 0)) {
				{
				{
				setState(166);
				funcModifier();
				}
				}
				setState(171);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(172);
			match(KEYWORD_FUNC);
			setState(173);
			match(IDENTIFIER);
			setState(174);
			functionMetadata();
			setState(176);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_DO) {
				{
				setState(175);
				match(KEYWORD_DO);
				}
			}

			setState(178);
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
	public static class FunctionMetadataContext extends ParserRuleContext {
		public List<FunctionSignatureVariantContext> functionSignatureVariant() {
			return getRuleContexts(FunctionSignatureVariantContext.class);
		}
		public FunctionSignatureVariantContext functionSignatureVariant(int i) {
			return getRuleContext(FunctionSignatureVariantContext.class,i);
		}
		public List<TerminalNode> NEWLINE() { return getTokens(MixinParser.NEWLINE); }
		public TerminalNode NEWLINE(int i) {
			return getToken(MixinParser.NEWLINE, i);
		}
		public FunctionMetadataContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_functionMetadata; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitFunctionMetadata(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FunctionMetadataContext functionMetadata() throws RecognitionException {
		FunctionMetadataContext _localctx = new FunctionMetadataContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_functionMetadata);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(184);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==NEWLINE || _la==KEYWORD_SIG) {
				{
				setState(182);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case KEYWORD_SIG:
					{
					setState(180);
					functionSignatureVariant();
					}
					break;
				case NEWLINE:
					{
					setState(181);
					match(NEWLINE);
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(186);
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
		public TerminalNode KEYWORD_SIG() { return getToken(MixinParser.KEYWORD_SIG, 0); }
		public List<SignatureContext> signature() {
			return getRuleContexts(SignatureContext.class);
		}
		public SignatureContext signature(int i) {
			return getRuleContext(SignatureContext.class,i);
		}
		public TerminalNode ARROW() { return getToken(MixinParser.ARROW, 0); }
		public FunctionSignatureVariantContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_functionSignatureVariant; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitFunctionSignatureVariant(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FunctionSignatureVariantContext functionSignatureVariant() throws RecognitionException {
		FunctionSignatureVariantContext _localctx = new FunctionSignatureVariantContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_functionSignatureVariant);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(187);
			match(KEYWORD_SIG);
			setState(188);
			signature();
			setState(189);
			match(ARROW);
			setState(190);
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
		public TerminalNode IDENTIFIER() { return getToken(MixinParser.IDENTIFIER, 0); }
		public SignatureContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_signature; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitSignature(this);
			else return visitor.visitChildren(this);
		}
	}

	public final SignatureContext signature() throws RecognitionException {
		SignatureContext _localctx = new SignatureContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_signature);
		try {
			setState(194);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_TABLE:
				enterOuterAlt(_localctx, 1);
				{
				setState(192);
				tableSignature();
				}
				break;
			case IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(193);
				match(IDENTIFIER);
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
		public TerminalNode BEGIN_TABLE() { return getToken(MixinParser.BEGIN_TABLE, 0); }
		public TerminalNode VALUE_END_INTERPOLATE() { return getToken(MixinParser.VALUE_END_INTERPOLATE, 0); }
		public List<TableSignatureEntryContext> tableSignatureEntry() {
			return getRuleContexts(TableSignatureEntryContext.class);
		}
		public TableSignatureEntryContext tableSignatureEntry(int i) {
			return getRuleContext(TableSignatureEntryContext.class,i);
		}
		public List<TerminalNode> VALUE_DELIMITER() { return getTokens(MixinParser.VALUE_DELIMITER); }
		public TerminalNode VALUE_DELIMITER(int i) {
			return getToken(MixinParser.VALUE_DELIMITER, i);
		}
		public TableSignatureContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tableSignature; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitTableSignature(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TableSignatureContext tableSignature() throws RecognitionException {
		TableSignatureContext _localctx = new TableSignatureContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_tableSignature);
		int _la;
		try {
			setState(209);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,12,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(196);
				match(BEGIN_TABLE);
				setState(197);
				match(VALUE_END_INTERPOLATE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(198);
				match(BEGIN_TABLE);
				setState(199);
				tableSignatureEntry();
				setState(204);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==VALUE_DELIMITER) {
					{
					{
					setState(200);
					match(VALUE_DELIMITER);
					setState(201);
					tableSignatureEntry();
					}
					}
					setState(206);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(207);
				match(VALUE_END_INTERPOLATE);
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
		public List<TerminalNode> ROOT_IDENTIFIER() { return getTokens(MixinParser.ROOT_IDENTIFIER); }
		public TerminalNode ROOT_IDENTIFIER(int i) {
			return getToken(MixinParser.ROOT_IDENTIFIER, i);
		}
		public TerminalNode VALUE_ASSIGN() { return getToken(MixinParser.VALUE_ASSIGN, 0); }
		public TerminalNode VALUE_EXPAND() { return getToken(MixinParser.VALUE_EXPAND, 0); }
		public TableSignatureEntryContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tableSignatureEntry; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitTableSignatureEntry(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TableSignatureEntryContext tableSignatureEntry() throws RecognitionException {
		TableSignatureEntryContext _localctx = new TableSignatureEntryContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_tableSignatureEntry);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(212);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==VALUE_EXPAND) {
				{
				setState(211);
				match(VALUE_EXPAND);
				}
			}

			setState(214);
			match(ROOT_IDENTIFIER);
			setState(215);
			match(VALUE_ASSIGN);
			setState(216);
			match(ROOT_IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
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
		public TerminalNode LC() { return getToken(MixinParser.LC, 0); }
		public TerminalNode RC() { return getToken(MixinParser.RC, 0); }
		public List<StatementContext> statement() {
			return getRuleContexts(StatementContext.class);
		}
		public StatementContext statement(int i) {
			return getRuleContext(StatementContext.class,i);
		}
		public List<TerminalNode> SEMICOLON() { return getTokens(MixinParser.SEMICOLON); }
		public TerminalNode SEMICOLON(int i) {
			return getToken(MixinParser.SEMICOLON, i);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitStatementBlock(this);
			else return visitor.visitChildren(this);
		}
	}

	public final StatementBlockContext statementBlock() throws RecognitionException {
		StatementBlockContext _localctx = new StatementBlockContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_statementBlock);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(218);
			match(LC);
			setState(224);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 147325992829952L) != 0)) {
				{
				setState(222);
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
					setState(219);
					statement();
					}
					break;
				case SEMICOLON:
					{
					setState(220);
					match(SEMICOLON);
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(221);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(226);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(227);
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
		public TerminalNode NEWLINE() { return getToken(MixinParser.NEWLINE, 0); }
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final StatementContext statement() throws RecognitionException {
		StatementContext _localctx = new StatementContext(_ctx, getState());
		enterRule(_localctx, 22, RULE_statement);
		int _la;
		try {
			setState(242);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,17,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(229);
				labelIdentifier();
				setState(230);
				match(NEWLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(233);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LABEL_PREFIX) {
					{
					setState(232);
					labelIdentifier();
					}
				}

				setState(235);
				statementBlock();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(236);
				invocationStatement();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(237);
				assignmentStatement();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(238);
				controlflowStatement();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(239);
				whenValueStatement();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(240);
				whenConditionStatement();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(241);
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
		public TerminalNode IDENTIFIER() { return getToken(MixinParser.IDENTIFIER, 0); }
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitInvocationStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final InvocationStatementContext invocationStatement() throws RecognitionException {
		InvocationStatementContext _localctx = new InvocationStatementContext(_ctx, getState());
		enterRule(_localctx, 24, RULE_invocationStatement);
		try {
			setState(251);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,19,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(244);
				match(IDENTIFIER);
				setState(245);
				valueList();
				setState(247);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,18,_ctx) ) {
				case 1:
					{
					setState(246);
					tailValue();
					}
					break;
				}
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(249);
				match(IDENTIFIER);
				setState(250);
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
		public TerminalNode KEYWORD_WHEN() { return getToken(MixinParser.KEYWORD_WHEN, 0); }
		public WhenChainConditionContext whenChainCondition() {
			return getRuleContext(WhenChainConditionContext.class,0);
		}
		public StatementBlockContext statementBlock() {
			return getRuleContext(StatementBlockContext.class,0);
		}
		public TerminalNode KEYWORD_ELSE() { return getToken(MixinParser.KEYWORD_ELSE, 0); }
		public WhenResultContext whenResult() {
			return getRuleContext(WhenResultContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(MixinParser.NEWLINE, 0); }
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitWhenConditionStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenConditionStatementContext whenConditionStatement() throws RecognitionException {
		WhenConditionStatementContext _localctx = new WhenConditionStatementContext(_ctx, getState());
		enterRule(_localctx, 26, RULE_whenConditionStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(253);
			match(KEYWORD_WHEN);
			setState(254);
			whenChainCondition();
			setState(255);
			statementBlock();
			setState(266);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,21,_ctx) ) {
			case 1:
				{
				setState(259);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3670016L) != 0)) {
					{
					{
					setState(256);
					trivia();
					}
					}
					setState(261);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(262);
				match(KEYWORD_ELSE);
				setState(263);
				whenResult();
				setState(264);
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
		public TerminalNode KEYWORD_ELSE() { return getToken(MixinParser.KEYWORD_ELSE, 0); }
		public TerminalNode ARROW() { return getToken(MixinParser.ARROW, 0); }
		public WhenResultContext whenResult() {
			return getRuleContext(WhenResultContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(MixinParser.NEWLINE, 0); }
		public WhenElseBranchContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenElseBranch; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitWhenElseBranch(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenElseBranchContext whenElseBranch() throws RecognitionException {
		WhenElseBranchContext _localctx = new WhenElseBranchContext(_ctx, getState());
		enterRule(_localctx, 28, RULE_whenElseBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(268);
			match(KEYWORD_ELSE);
			setState(269);
			match(ARROW);
			setState(270);
			whenResult();
			setState(271);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitWhenResult(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenResultContext whenResult() throws RecognitionException {
		WhenResultContext _localctx = new WhenResultContext(_ctx, getState());
		enterRule(_localctx, 30, RULE_whenResult);
		try {
			setState(275);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_ARGUMENT:
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(273);
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
				setState(274);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitWhenChainCondition(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenChainConditionContext whenChainCondition() throws RecognitionException {
		WhenChainConditionContext _localctx = new WhenChainConditionContext(_ctx, getState());
		enterRule(_localctx, 32, RULE_whenChainCondition);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(277);
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
		public TerminalNode KEYWORD_WHEN() { return getToken(MixinParser.KEYWORD_WHEN, 0); }
		public TerminalNode LC() { return getToken(MixinParser.LC, 0); }
		public TerminalNode NEWLINE() { return getToken(MixinParser.NEWLINE, 0); }
		public WhenChainBodyContext whenChainBody() {
			return getRuleContext(WhenChainBodyContext.class,0);
		}
		public TerminalNode RC() { return getToken(MixinParser.RC, 0); }
		public WhenChainStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenChainStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitWhenChainStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenChainStatementContext whenChainStatement() throws RecognitionException {
		WhenChainStatementContext _localctx = new WhenChainStatementContext(_ctx, getState());
		enterRule(_localctx, 34, RULE_whenChainStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(279);
			match(KEYWORD_WHEN);
			setState(280);
			match(LC);
			setState(281);
			match(NEWLINE);
			setState(282);
			whenChainBody();
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitWhenChainBody(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenChainBodyContext whenChainBody() throws RecognitionException {
		WhenChainBodyContext _localctx = new WhenChainBodyContext(_ctx, getState());
		enterRule(_localctx, 36, RULE_whenChainBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(285);
			whenChainBranch();
			setState(290);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 6755399444725776L) != 0)) {
				{
				setState(288);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case EMPTY_PARAMETERS:
				case BEGIN_PARAMETERS:
					{
					setState(286);
					whenChainBranch();
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
			setState(294);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(293);
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
		public TerminalNode ARROW() { return getToken(MixinParser.ARROW, 0); }
		public WhenResultContext whenResult() {
			return getRuleContext(WhenResultContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(MixinParser.NEWLINE, 0); }
		public WhenChainBranchContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenChainBranch; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitWhenChainBranch(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenChainBranchContext whenChainBranch() throws RecognitionException {
		WhenChainBranchContext _localctx = new WhenChainBranchContext(_ctx, getState());
		enterRule(_localctx, 38, RULE_whenChainBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(296);
			whenChainCondition();
			setState(297);
			match(ARROW);
			setState(298);
			whenResult();
			setState(299);
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
		public TerminalNode KEYWORD_WHEN() { return getToken(MixinParser.KEYWORD_WHEN, 0); }
		public TerminalNode LC() { return getToken(MixinParser.LC, 0); }
		public TerminalNode NEWLINE() { return getToken(MixinParser.NEWLINE, 0); }
		public WhenValueBodyContext whenValueBody() {
			return getRuleContext(WhenValueBodyContext.class,0);
		}
		public TerminalNode RC() { return getToken(MixinParser.RC, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public WhenValueStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenValueStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitWhenValueStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenValueStatementContext whenValueStatement() throws RecognitionException {
		WhenValueStatementContext _localctx = new WhenValueStatementContext(_ctx, getState());
		enterRule(_localctx, 40, RULE_whenValueStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(301);
			match(KEYWORD_WHEN);
			setState(303);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1125899906900496L) != 0) || ((((_la - 66)) & ~0x3f) == 0 && ((1L << (_la - 66)) & 2051L) != 0)) {
				{
				setState(302);
				value();
				}
			}

			setState(305);
			match(LC);
			setState(306);
			match(NEWLINE);
			setState(307);
			whenValueBody();
			setState(308);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitWhenValueBody(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenValueBodyContext whenValueBody() throws RecognitionException {
		WhenValueBodyContext _localctx = new WhenValueBodyContext(_ctx, getState());
		enterRule(_localctx, 42, RULE_whenValueBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(313);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 3670016L) != 0)) {
				{
				{
				setState(310);
				trivia();
				}
				}
				setState(315);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(316);
			whenValueBranch();
			setState(321);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1125899910570512L) != 0) || ((((_la - 66)) & ~0x3f) == 0 && ((1L << (_la - 66)) & 2051L) != 0)) {
				{
				setState(319);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case BEGIN_ARGUMENT:
				case BEGIN_VALUE_INLINE:
				case ROOT_IDENTIFIER:
				case BEGIN_TABLE:
				case BEGIN_TUPLE:
				case TOPLEVEL_VALUE_EXPRESSION:
				case VALUE_SMART_ROOT:
				case NOT_VALUE:
				case FUNCTION_IDENTIFIER:
					{
					setState(317);
					whenValueBranch();
					}
					break;
				case NEWLINE:
				case COMMENT:
				case SLASH_COMMENT:
					{
					setState(318);
					trivia();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(323);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(325);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==KEYWORD_ELSE) {
				{
				setState(324);
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
		public TerminalNode ARROW() { return getToken(MixinParser.ARROW, 0); }
		public WhenResultContext whenResult() {
			return getRuleContext(WhenResultContext.class,0);
		}
		public TerminalNode NEWLINE() { return getToken(MixinParser.NEWLINE, 0); }
		public WhenValueBranchContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whenValueBranch; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitWhenValueBranch(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenValueBranchContext whenValueBranch() throws RecognitionException {
		WhenValueBranchContext _localctx = new WhenValueBranchContext(_ctx, getState());
		enterRule(_localctx, 44, RULE_whenValueBranch);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(327);
			whenValueCondition();
			setState(328);
			match(ARROW);
			setState(329);
			whenResult();
			setState(330);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitWhenValueCondition(this);
			else return visitor.visitChildren(this);
		}
	}

	public final WhenValueConditionContext whenValueCondition() throws RecognitionException {
		WhenValueConditionContext _localctx = new WhenValueConditionContext(_ctx, getState());
		enterRule(_localctx, 46, RULE_whenValueCondition);
		try {
			setState(334);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,31,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(332);
				value();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(333);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitAssignmentStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final AssignmentStatementContext assignmentStatement() throws RecognitionException {
		AssignmentStatementContext _localctx = new AssignmentStatementContext(_ctx, getState());
		enterRule(_localctx, 48, RULE_assignmentStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(336);
			variableSpecifiers();
			setState(337);
			variableIdentifier();
			setState(338);
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
		public TerminalNode ASSIGN() { return getToken(MixinParser.ASSIGN, 0); }
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitAssignedValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final AssignedValueContext assignedValue() throws RecognitionException {
		AssignedValueContext _localctx = new AssignedValueContext(_ctx, getState());
		enterRule(_localctx, 50, RULE_assignedValue);
		try {
			setState(348);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ASSIGN:
				enterOuterAlt(_localctx, 1);
				{
				setState(340);
				match(ASSIGN);
				setState(345);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,32,_ctx) ) {
				case 1:
					{
					setState(341);
					value();
					}
					break;
				case 2:
					{
					setState(342);
					whenValueStatement();
					}
					break;
				case 3:
					{
					setState(343);
					whenChainStatement();
					}
					break;
				case 4:
					{
					setState(344);
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
			case NEWLINE:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(347);
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
		public TerminalNode KEYWORD_RETURN() { return getToken(MixinParser.KEYWORD_RETURN, 0); }
		public ValueListContext valueList() {
			return getRuleContext(ValueListContext.class,0);
		}
		public TerminalNode KEYWORD_GOTO() { return getToken(MixinParser.KEYWORD_GOTO, 0); }
		public TerminalNode IDENTIFIER() { return getToken(MixinParser.IDENTIFIER, 0); }
		public TerminalNode KEYWORD_CONTINUE() { return getToken(MixinParser.KEYWORD_CONTINUE, 0); }
		public TerminalNode KEYWORD_BREAK() { return getToken(MixinParser.KEYWORD_BREAK, 0); }
		public ControlflowStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_controlflowStatement; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitControlflowStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ControlflowStatementContext controlflowStatement() throws RecognitionException {
		ControlflowStatementContext _localctx = new ControlflowStatementContext(_ctx, getState());
		enterRule(_localctx, 52, RULE_controlflowStatement);
		int _la;
		try {
			setState(358);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_RETURN:
				enterOuterAlt(_localctx, 1);
				{
				setState(350);
				match(KEYWORD_RETURN);
				setState(352);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 6755399441055760L) != 0)) {
					{
					setState(351);
					valueList();
					}
				}

				}
				break;
			case KEYWORD_GOTO:
				enterOuterAlt(_localctx, 2);
				{
				setState(354);
				match(KEYWORD_GOTO);
				setState(355);
				match(IDENTIFIER);
				}
				break;
			case KEYWORD_CONTINUE:
				enterOuterAlt(_localctx, 3);
				{
				setState(356);
				match(KEYWORD_CONTINUE);
				}
				break;
			case KEYWORD_BREAK:
				enterOuterAlt(_localctx, 4);
				{
				setState(357);
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
		public TerminalNode BEGIN_CONTENT() { return getToken(MixinParser.BEGIN_CONTENT, 0); }
		public ContentBodyContext contentBody() {
			return getRuleContext(ContentBodyContext.class,0);
		}
		public TerminalNode TERMINATOR() { return getToken(MixinParser.TERMINATOR, 0); }
		public ContentBlockContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_contentBlock; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitContentBlock(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ContentBlockContext contentBlock() throws RecognitionException {
		ContentBlockContext _localctx = new ContentBlockContext(_ctx, getState());
		enterRule(_localctx, 54, RULE_contentBlock);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(360);
			match(BEGIN_CONTENT);
			setState(361);
			contentBody();
			setState(362);
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
		public List<TerminalNode> CONTENT_WRAP() { return getTokens(MixinParser.CONTENT_WRAP); }
		public TerminalNode CONTENT_WRAP(int i) {
			return getToken(MixinParser.CONTENT_WRAP, i);
		}
		public List<TerminalNode> CONTENT_LINEBREAK() { return getTokens(MixinParser.CONTENT_LINEBREAK); }
		public TerminalNode CONTENT_LINEBREAK(int i) {
			return getToken(MixinParser.CONTENT_LINEBREAK, i);
		}
		public List<TerminalNode> CONTENT_TEXT() { return getTokens(MixinParser.CONTENT_TEXT); }
		public TerminalNode CONTENT_TEXT(int i) {
			return getToken(MixinParser.CONTENT_TEXT, i);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitContentBody(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ContentBodyContext contentBody() throws RecognitionException {
		ContentBodyContext _localctx = new ContentBodyContext(_ctx, getState());
		enterRule(_localctx, 56, RULE_contentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(368); 
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				setState(368);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case CONTENT_WRAP:
					{
					setState(364);
					match(CONTENT_WRAP);
					}
					break;
				case CONTENT_LINEBREAK:
					{
					setState(365);
					match(CONTENT_LINEBREAK);
					}
					break;
				case CONTENT_TEXT:
					{
					setState(366);
					match(CONTENT_TEXT);
					}
					break;
				case BEGIN_VALUE_INTERPOLATE:
					{
					setState(367);
					contentInterpolate();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(370); 
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( (((_la) & ~0x3f) == 0 && ((1L << _la) & 252201579132748032L) != 0) );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
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
		public TerminalNode BEGIN_VALUE_INTERPOLATE() { return getToken(MixinParser.BEGIN_VALUE_INTERPOLATE, 0); }
		public DerivationContext derivation() {
			return getRuleContext(DerivationContext.class,0);
		}
		public TerminalNode VALUE_END_INTERPOLATE() { return getToken(MixinParser.VALUE_END_INTERPOLATE, 0); }
		public TerminalNode END_CONTENT_INTERPOLATE() { return getToken(MixinParser.END_CONTENT_INTERPOLATE, 0); }
		public ContentInterpolateContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_contentInterpolate; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitContentInterpolate(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ContentInterpolateContext contentInterpolate() throws RecognitionException {
		ContentInterpolateContext _localctx = new ContentInterpolateContext(_ctx, getState());
		enterRule(_localctx, 58, RULE_contentInterpolate);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(372);
			match(BEGIN_VALUE_INTERPOLATE);
			setState(373);
			derivation();
			setState(374);
			match(VALUE_END_INTERPOLATE);
			setState(376);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==END_CONTENT_INTERPOLATE) {
				{
				setState(375);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ValueContext value() throws RecognitionException {
		ValueContext _localctx = new ValueContext(_ctx, getState());
		enterRule(_localctx, 60, RULE_value);
		try {
			setState(380);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
				enterOuterAlt(_localctx, 1);
				{
				setState(378);
				nonArgumentValue(0);
				}
				break;
			case BEGIN_ARGUMENT:
				enterOuterAlt(_localctx, 2);
				{
				setState(379);
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
		public TerminalNode TOPLEVEL_VALUE_EXPRESSION() { return getToken(MixinParser.TOPLEVEL_VALUE_EXPRESSION, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public TerminalNode VALUE_END() { return getToken(MixinParser.VALUE_END, 0); }
		public ValueExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_valueExpression; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitValueExpression(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ValueExpressionContext valueExpression() throws RecognitionException {
		ValueExpressionContext _localctx = new ValueExpressionContext(_ctx, getState());
		enterRule(_localctx, 62, RULE_valueExpression);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(382);
			match(TOPLEVEL_VALUE_EXPRESSION);
			setState(383);
			value();
			setState(384);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitNonArgumentValue(this);
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
		int _startState = 64;
		enterRecursionRule(_localctx, 64, RULE_nonArgumentValue, _p);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(394);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case FUNCTION_IDENTIFIER:
				{
				setState(387);
				primaryValue();
				setState(389);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,40,_ctx) ) {
				case 1:
					{
					setState(388);
					elvisValue();
					}
					break;
				}
				}
				break;
			case NOT_VALUE:
				{
				setState(391);
				prefixOperators();
				setState(392);
				nonArgumentValue(2);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(400);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,42,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					{
					_localctx = new NonArgumentValueContext(_parentctx, _parentState);
					pushNewRecursionContext(_localctx, _startState, RULE_nonArgumentValue);
					setState(396);
					if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
					setState(397);
					postfixOperators();
					}
					} 
				}
				setState(402);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,42,_ctx);
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
		public PrimaryValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_primaryValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitPrimaryValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PrimaryValueContext primaryValue() throws RecognitionException {
		PrimaryValueContext _localctx = new PrimaryValueContext(_ctx, getState());
		enterRule(_localctx, 66, RULE_primaryValue);
		try {
			setState(409);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,43,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(403);
				inlineValue();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(404);
				derivation();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(405);
				tableValue();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(406);
				tupleValue();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(407);
				valueExpression();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(408);
				valueStatement();
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
	public static class PrefixOperatorsContext extends ParserRuleContext {
		public TerminalNode NOT_VALUE() { return getToken(MixinParser.NOT_VALUE, 0); }
		public PrefixOperatorsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_prefixOperators; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitPrefixOperators(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PrefixOperatorsContext prefixOperators() throws RecognitionException {
		PrefixOperatorsContext _localctx = new PrefixOperatorsContext(_ctx, getState());
		enterRule(_localctx, 68, RULE_prefixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(411);
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
		public TerminalNode VALUE_CHECK() { return getToken(MixinParser.VALUE_CHECK, 0); }
		public PostfixOperatorsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_postfixOperators; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitPostfixOperators(this);
			else return visitor.visitChildren(this);
		}
	}

	public final PostfixOperatorsContext postfixOperators() throws RecognitionException {
		PostfixOperatorsContext _localctx = new PostfixOperatorsContext(_ctx, getState());
		enterRule(_localctx, 70, RULE_postfixOperators);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(413);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitValueStatement(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ValueStatementContext valueStatement() throws RecognitionException {
		ValueStatementContext _localctx = new ValueStatementContext(_ctx, getState());
		enterRule(_localctx, 72, RULE_valueStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(415);
			functionIdentifier();
			setState(416);
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
		public TerminalNode NEWLINE() { return getToken(MixinParser.NEWLINE, 0); }
		public NonArgumentValueContext nonArgumentValue() {
			return getRuleContext(NonArgumentValueContext.class,0);
		}
		public TailValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tailValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitTailValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TailValueContext tailValue() throws RecognitionException {
		TailValueContext _localctx = new TailValueContext(_ctx, getState());
		enterRule(_localctx, 74, RULE_tailValue);
		int _la;
		try {
			setState(423);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BEGIN_CONTENT:
			case NEWLINE:
				enterOuterAlt(_localctx, 1);
				{
				setState(419);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==NEWLINE) {
					{
					setState(418);
					match(NEWLINE);
					}
				}

				setState(421);
				contentBlock();
				}
				break;
			case BEGIN_VALUE_INLINE:
			case ROOT_IDENTIFIER:
			case BEGIN_TABLE:
			case BEGIN_TUPLE:
			case TOPLEVEL_VALUE_EXPRESSION:
			case VALUE_SMART_ROOT:
			case NOT_VALUE:
			case FUNCTION_IDENTIFIER:
				enterOuterAlt(_localctx, 2);
				{
				setState(422);
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
		public TerminalNode BEGIN_TUPLE() { return getToken(MixinParser.BEGIN_TUPLE, 0); }
		public TerminalNode VALUE_END_INLINE() { return getToken(MixinParser.VALUE_END_INLINE, 0); }
		public List<ValueContext> value() {
			return getRuleContexts(ValueContext.class);
		}
		public ValueContext value(int i) {
			return getRuleContext(ValueContext.class,i);
		}
		public List<TerminalNode> VALUE_DELIMITER() { return getTokens(MixinParser.VALUE_DELIMITER); }
		public TerminalNode VALUE_DELIMITER(int i) {
			return getToken(MixinParser.VALUE_DELIMITER, i);
		}
		public TupleValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tupleValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitTupleValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TupleValueContext tupleValue() throws RecognitionException {
		TupleValueContext _localctx = new TupleValueContext(_ctx, getState());
		enterRule(_localctx, 76, RULE_tupleValue);
		int _la;
		try {
			setState(438);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,47,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(425);
				match(BEGIN_TUPLE);
				setState(426);
				match(VALUE_END_INLINE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(427);
				match(BEGIN_TUPLE);
				setState(428);
				value();
				setState(433);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==VALUE_DELIMITER) {
					{
					{
					setState(429);
					match(VALUE_DELIMITER);
					setState(430);
					value();
					}
					}
					setState(435);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(436);
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
		public TerminalNode BEGIN_TABLE() { return getToken(MixinParser.BEGIN_TABLE, 0); }
		public TerminalNode VALUE_END_INTERPOLATE() { return getToken(MixinParser.VALUE_END_INTERPOLATE, 0); }
		public List<TableKeyedEntryContext> tableKeyedEntry() {
			return getRuleContexts(TableKeyedEntryContext.class);
		}
		public TableKeyedEntryContext tableKeyedEntry(int i) {
			return getRuleContext(TableKeyedEntryContext.class,i);
		}
		public List<TerminalNode> VALUE_DELIMITER() { return getTokens(MixinParser.VALUE_DELIMITER); }
		public TerminalNode VALUE_DELIMITER(int i) {
			return getToken(MixinParser.VALUE_DELIMITER, i);
		}
		public TableValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tableValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitTableValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TableValueContext tableValue() throws RecognitionException {
		TableValueContext _localctx = new TableValueContext(_ctx, getState());
		enterRule(_localctx, 78, RULE_tableValue);
		int _la;
		try {
			setState(453);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,49,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(440);
				match(BEGIN_TABLE);
				setState(441);
				match(VALUE_END_INTERPOLATE);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(442);
				match(BEGIN_TABLE);
				setState(443);
				tableKeyedEntry();
				setState(448);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==VALUE_DELIMITER) {
					{
					{
					setState(444);
					match(VALUE_DELIMITER);
					setState(445);
					tableKeyedEntry();
					}
					}
					setState(450);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(451);
				match(VALUE_END_INTERPOLATE);
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
		public TerminalNode ROOT_IDENTIFIER() { return getToken(MixinParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode VALUE_ASSIGN() { return getToken(MixinParser.VALUE_ASSIGN, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public TableKeyedEntryContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_tableKeyedEntry; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitTableKeyedEntry(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TableKeyedEntryContext tableKeyedEntry() throws RecognitionException {
		TableKeyedEntryContext _localctx = new TableKeyedEntryContext(_ctx, getState());
		enterRule(_localctx, 80, RULE_tableKeyedEntry);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(455);
			match(ROOT_IDENTIFIER);
			setState(456);
			match(VALUE_ASSIGN);
			setState(457);
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
		public TerminalNode BEGIN_PARAMETERS() { return getToken(MixinParser.BEGIN_PARAMETERS, 0); }
		public List<ValueContext> value() {
			return getRuleContexts(ValueContext.class);
		}
		public ValueContext value(int i) {
			return getRuleContext(ValueContext.class,i);
		}
		public TerminalNode END_PARAMETERS() { return getToken(MixinParser.END_PARAMETERS, 0); }
		public List<TerminalNode> VALUE_DELIMITER() { return getTokens(MixinParser.VALUE_DELIMITER); }
		public TerminalNode VALUE_DELIMITER(int i) {
			return getToken(MixinParser.VALUE_DELIMITER, i);
		}
		public TerminalNode EMPTY_PARAMETERS() { return getToken(MixinParser.EMPTY_PARAMETERS, 0); }
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitValueList(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ValueListContext valueList() throws RecognitionException {
		ValueListContext _localctx = new ValueListContext(_ctx, getState());
		enterRule(_localctx, 82, RULE_valueList);
		int _la;
		try {
			int _alt;
			setState(478);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,52,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(459);
				match(BEGIN_PARAMETERS);
				setState(460);
				value();
				setState(465);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==VALUE_DELIMITER) {
					{
					{
					setState(461);
					match(VALUE_DELIMITER);
					setState(462);
					value();
					}
					}
					setState(467);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(468);
				match(END_PARAMETERS);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(470);
				match(BEGIN_PARAMETERS);
				setState(471);
				match(END_PARAMETERS);
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(472);
				match(EMPTY_PARAMETERS);
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(474); 
				_errHandler.sync(this);
				_alt = 1;
				do {
					switch (_alt) {
					case 1:
						{
						{
						setState(473);
						argumentValue();
						}
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					setState(476); 
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,51,_ctx);
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
		public TerminalNode BEGIN_ARGUMENT() { return getToken(MixinParser.BEGIN_ARGUMENT, 0); }
		public ArgumentBodyContext argumentBody() {
			return getRuleContext(ArgumentBodyContext.class,0);
		}
		public TerminalNode ARGUMENT_END() { return getToken(MixinParser.ARGUMENT_END, 0); }
		public ArgumentValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_argumentValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitArgumentValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ArgumentValueContext argumentValue() throws RecognitionException {
		ArgumentValueContext _localctx = new ArgumentValueContext(_ctx, getState());
		enterRule(_localctx, 84, RULE_argumentValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(480);
			match(BEGIN_ARGUMENT);
			setState(481);
			argumentBody();
			setState(482);
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
		public List<TerminalNode> ARGUMENT_TEXT() { return getTokens(MixinParser.ARGUMENT_TEXT); }
		public TerminalNode ARGUMENT_TEXT(int i) {
			return getToken(MixinParser.ARGUMENT_TEXT, i);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitArgumentBody(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ArgumentBodyContext argumentBody() throws RecognitionException {
		ArgumentBodyContext _localctx = new ArgumentBodyContext(_ctx, getState());
		enterRule(_localctx, 86, RULE_argumentBody);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(489);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 9007199254741512L) != 0)) {
				{
				setState(487);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case ARGUMENT_TEXT:
					{
					setState(484);
					match(ARGUMENT_TEXT);
					}
					break;
				case ESCAPE:
					{
					setState(485);
					escaped();
					}
					break;
				case BEGIN_VALUE_INLINE:
					{
					setState(486);
					inlineValue();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(491);
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
		public TerminalNode BEGIN_VALUE_INLINE() { return getToken(MixinParser.BEGIN_VALUE_INLINE, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public TerminalNode VALUE_END_INLINE() { return getToken(MixinParser.VALUE_END_INLINE, 0); }
		public InlineValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_inlineValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitInlineValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final InlineValueContext inlineValue() throws RecognitionException {
		InlineValueContext _localctx = new InlineValueContext(_ctx, getState());
		enterRule(_localctx, 88, RULE_inlineValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(492);
			match(BEGIN_VALUE_INLINE);
			setState(493);
			value();
			setState(494);
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
		public TerminalNode BEGIN_VALUE_INLINE() { return getToken(MixinParser.BEGIN_VALUE_INLINE, 0); }
		public TerminalNode VALUE_END_INLINE() { return getToken(MixinParser.VALUE_END_INLINE, 0); }
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitInlineTransformation(this);
			else return visitor.visitChildren(this);
		}
	}

	public final InlineTransformationContext inlineTransformation() throws RecognitionException {
		InlineTransformationContext _localctx = new InlineTransformationContext(_ctx, getState());
		enterRule(_localctx, 90, RULE_inlineTransformation);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(496);
			match(BEGIN_VALUE_INLINE);
			setState(498); 
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(497);
				transformationPart();
				}
				}
				setState(500); 
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( (((_la) & ~0x3f) == 0 && ((1L << _la) & 4323455642275676160L) != 0) );
			setState(502);
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
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitDerivation(this);
			else return visitor.visitChildren(this);
		}
	}

	public final DerivationContext derivation() throws RecognitionException {
		DerivationContext _localctx = new DerivationContext(_ctx, getState());
		enterRule(_localctx, 92, RULE_derivation);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(504);
			derivationRoot();
			setState(508);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,56,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(505);
					transformationPart();
					}
					} 
				}
				setState(510);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,56,_ctx);
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
		public TerminalNode ROOT_IDENTIFIER() { return getToken(MixinParser.ROOT_IDENTIFIER, 0); }
		public TerminalNode VALUE_SMART_ROOT() { return getToken(MixinParser.VALUE_SMART_ROOT, 0); }
		public DerivationRootContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_derivationRoot; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitDerivationRoot(this);
			else return visitor.visitChildren(this);
		}
	}

	public final DerivationRootContext derivationRoot() throws RecognitionException {
		DerivationRootContext _localctx = new DerivationRootContext(_ctx, getState());
		enterRule(_localctx, 94, RULE_derivationRoot);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(512);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==VALUE_SMART_ROOT) {
				{
				setState(511);
				match(VALUE_SMART_ROOT);
				}
			}

			setState(514);
			match(ROOT_IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
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
		public TerminalNode VALUE_ELVIS() { return getToken(MixinParser.VALUE_ELVIS, 0); }
		public ValueContext value() {
			return getRuleContext(ValueContext.class,0);
		}
		public ElvisValueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_elvisValue; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitElvisValue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ElvisValueContext elvisValue() throws RecognitionException {
		ElvisValueContext _localctx = new ElvisValueContext(_ctx, getState());
		enterRule(_localctx, 96, RULE_elvisValue);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(516);
			match(VALUE_ELVIS);
			setState(517);
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
		public TerminalNode VALUE_WRAP() { return getToken(MixinParser.VALUE_WRAP, 0); }
		public TransformationPartContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_transformationPart; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitTransformationPart(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TransformationPartContext transformationPart() throws RecognitionException {
		TransformationPartContext _localctx = new TransformationPartContext(_ctx, getState());
		enterRule(_localctx, 98, RULE_transformationPart);
		try {
			setState(526);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case VALUE_FUNCTION:
			case VALUE_PREDICATE:
				enterOuterAlt(_localctx, 1);
				{
				setState(519);
				functionChainType();
				setState(520);
				functionIdentifier();
				setState(522);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,58,_ctx) ) {
				case 1:
					{
					setState(521);
					valueList();
					}
					break;
				}
				}
				break;
			case VALUE_MEMBER:
				enterOuterAlt(_localctx, 2);
				{
				setState(524);
				memberIdentifier();
				}
				break;
			case VALUE_WRAP:
				enterOuterAlt(_localctx, 3);
				{
				setState(525);
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
		public TerminalNode VALUE_FUNCTION() { return getToken(MixinParser.VALUE_FUNCTION, 0); }
		public TerminalNode VALUE_PREDICATE() { return getToken(MixinParser.VALUE_PREDICATE, 0); }
		public FunctionChainTypeContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_functionChainType; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitFunctionChainType(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FunctionChainTypeContext functionChainType() throws RecognitionException {
		FunctionChainTypeContext _localctx = new FunctionChainTypeContext(_ctx, getState());
		enterRule(_localctx, 100, RULE_functionChainType);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(528);
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
		public TerminalNode LABEL_PREFIX() { return getToken(MixinParser.LABEL_PREFIX, 0); }
		public TerminalNode LABEL_IDENTIFIER() { return getToken(MixinParser.LABEL_IDENTIFIER, 0); }
		public LabelIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_labelIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitLabelIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final LabelIdentifierContext labelIdentifier() throws RecognitionException {
		LabelIdentifierContext _localctx = new LabelIdentifierContext(_ctx, getState());
		enterRule(_localctx, 102, RULE_labelIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(530);
			match(LABEL_PREFIX);
			setState(531);
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
		public TerminalNode VALUE_MEMBER() { return getToken(MixinParser.VALUE_MEMBER, 0); }
		public TerminalNode MEMBER_IDENTIFIER() { return getToken(MixinParser.MEMBER_IDENTIFIER, 0); }
		public MemberIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_memberIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitMemberIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MemberIdentifierContext memberIdentifier() throws RecognitionException {
		MemberIdentifierContext _localctx = new MemberIdentifierContext(_ctx, getState());
		enterRule(_localctx, 104, RULE_memberIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(533);
			match(VALUE_MEMBER);
			setState(534);
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
		public TerminalNode IDENTIFIER() { return getToken(MixinParser.IDENTIFIER, 0); }
		public TerminalNode NAMESPACE_IDENTIFIER() { return getToken(MixinParser.NAMESPACE_IDENTIFIER, 0); }
		public MixinIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_mixinIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitMixinIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MixinIdentifierContext mixinIdentifier() throws RecognitionException {
		MixinIdentifierContext _localctx = new MixinIdentifierContext(_ctx, getState());
		enterRule(_localctx, 106, RULE_mixinIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(536);
			_la = _input.LA(1);
			if ( !(_la==IDENTIFIER || _la==NAMESPACE_IDENTIFIER) ) {
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
	public static class VariableIdentifierContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(MixinParser.IDENTIFIER, 0); }
		public VariableIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_variableIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitVariableIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final VariableIdentifierContext variableIdentifier() throws RecognitionException {
		VariableIdentifierContext _localctx = new VariableIdentifierContext(_ctx, getState());
		enterRule(_localctx, 108, RULE_variableIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(538);
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
		public TerminalNode FUNCTION_IDENTIFIER() { return getToken(MixinParser.FUNCTION_IDENTIFIER, 0); }
		public TerminalNode ROOT_IDENTIFIER() { return getToken(MixinParser.ROOT_IDENTIFIER, 0); }
		public FunctionIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_functionIdentifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitFunctionIdentifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FunctionIdentifierContext functionIdentifier() throws RecognitionException {
		FunctionIdentifierContext _localctx = new FunctionIdentifierContext(_ctx, getState());
		enterRule(_localctx, 110, RULE_functionIdentifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(540);
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
	public static class ExpressionModifierContext extends ParserRuleContext {
		public TerminalNode KEYWORD_PRELUDE() { return getToken(MixinParser.KEYWORD_PRELUDE, 0); }
		public TerminalNode KEYWORD_STRICT() { return getToken(MixinParser.KEYWORD_STRICT, 0); }
		public ExpressionModifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_expressionModifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitExpressionModifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ExpressionModifierContext expressionModifier() throws RecognitionException {
		ExpressionModifierContext _localctx = new ExpressionModifierContext(_ctx, getState());
		enterRule(_localctx, 112, RULE_expressionModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(542);
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
		public TerminalNode KEYWORD_DERIVATION() { return getToken(MixinParser.KEYWORD_DERIVATION, 0); }
		public MixinModifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_mixinModifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitMixinModifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final MixinModifierContext mixinModifier() throws RecognitionException {
		MixinModifierContext _localctx = new MixinModifierContext(_ctx, getState());
		enterRule(_localctx, 114, RULE_mixinModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(544);
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
		public TerminalNode KEYWORD_CARRY() { return getToken(MixinParser.KEYWORD_CARRY, 0); }
		public TerminalNode KEYWORD_LOCAL() { return getToken(MixinParser.KEYWORD_LOCAL, 0); }
		public TerminalNode KEYWORD_VAR() { return getToken(MixinParser.KEYWORD_VAR, 0); }
		public TerminalNode KEYWORD_TARGET() { return getToken(MixinParser.KEYWORD_TARGET, 0); }
		public VariableSpecifiersContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_variableSpecifiers; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitVariableSpecifiers(this);
			else return visitor.visitChildren(this);
		}
	}

	public final VariableSpecifiersContext variableSpecifiers() throws RecognitionException {
		VariableSpecifiersContext _localctx = new VariableSpecifiersContext(_ctx, getState());
		enterRule(_localctx, 116, RULE_variableSpecifiers);
		int _la;
		try {
			setState(552);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case KEYWORD_CARRY:
				enterOuterAlt(_localctx, 1);
				{
				setState(546);
				match(KEYWORD_CARRY);
				}
				break;
			case KEYWORD_LOCAL:
				enterOuterAlt(_localctx, 2);
				{
				setState(547);
				match(KEYWORD_LOCAL);
				}
				break;
			case KEYWORD_TARGET:
			case KEYWORD_VAR:
				enterOuterAlt(_localctx, 3);
				{
				{
				setState(549);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==KEYWORD_TARGET) {
					{
					setState(548);
					match(KEYWORD_TARGET);
					}
				}

				setState(551);
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
		public TerminalNode KEYWORD_PURE() { return getToken(MixinParser.KEYWORD_PURE, 0); }
		public TerminalNode KEYWORD_NOINLINE() { return getToken(MixinParser.KEYWORD_NOINLINE, 0); }
		public TerminalNode KEYWORD_INLINE() { return getToken(MixinParser.KEYWORD_INLINE, 0); }
		public FuncModifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_funcModifier; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitFuncModifier(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FuncModifierContext funcModifier() throws RecognitionException {
		FuncModifierContext _localctx = new FuncModifierContext(_ctx, getState());
		enterRule(_localctx, 118, RULE_funcModifier);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(554);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 61572651155456L) != 0)) ) {
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
		public TerminalNode NEWLINE() { return getToken(MixinParser.NEWLINE, 0); }
		public TriviaContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_trivia; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitTrivia(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TriviaContext trivia() throws RecognitionException {
		TriviaContext _localctx = new TriviaContext(_ctx, getState());
		enterRule(_localctx, 120, RULE_trivia);
		try {
			setState(558);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case COMMENT:
			case SLASH_COMMENT:
				enterOuterAlt(_localctx, 1);
				{
				setState(556);
				comment();
				}
				break;
			case NEWLINE:
				enterOuterAlt(_localctx, 2);
				{
				setState(557);
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
		public TerminalNode COMMENT() { return getToken(MixinParser.COMMENT, 0); }
		public TerminalNode SLASH_COMMENT() { return getToken(MixinParser.SLASH_COMMENT, 0); }
		public CommentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_comment; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitComment(this);
			else return visitor.visitChildren(this);
		}
	}

	public final CommentContext comment() throws RecognitionException {
		CommentContext _localctx = new CommentContext(_ctx, getState());
		enterRule(_localctx, 122, RULE_comment);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(560);
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
		public TerminalNode ESCAPE() { return getToken(MixinParser.ESCAPE, 0); }
		public TerminalNode ESCAPE_MACRO() { return getToken(MixinParser.ESCAPE_MACRO, 0); }
		public TerminalNode ESCAPE_LITERAL() { return getToken(MixinParser.ESCAPE_LITERAL, 0); }
		public TerminalNode ESCAPE_HEX() { return getToken(MixinParser.ESCAPE_HEX, 0); }
		public EscapedContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_escaped; }
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof MixinParserVisitor ) return ((MixinParserVisitor<? extends T>)visitor).visitEscaped(this);
			else return visitor.visitChildren(this);
		}
	}

	public final EscapedContext escaped() throws RecognitionException {
		EscapedContext _localctx = new EscapedContext(_ctx, getState());
		enterRule(_localctx, 124, RULE_escaped);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(562);
			match(ESCAPE);
			setState(563);
			_la = _input.LA(1);
			if ( !(((((_la - 73)) & ~0x3f) == 0 && ((1L << (_la - 73)) & 7L) != 0)) ) {
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
		case 32:
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
		"\u0004\u0001O\u0236\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
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
		"<\u0007<\u0002=\u0007=\u0002>\u0007>\u0001\u0000\u0001\u0000\u0001\u0000"+
		"\u0005\u0000\u0082\b\u0000\n\u0000\f\u0000\u0085\t\u0000\u0001\u0000\u0001"+
		"\u0000\u0001\u0001\u0005\u0001\u008a\b\u0001\n\u0001\f\u0001\u008d\t\u0001"+
		"\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0002\u0001\u0002"+
		"\u0001\u0002\u0001\u0002\u0005\u0002\u0097\b\u0002\n\u0002\f\u0002\u009a"+
		"\t\u0002\u0001\u0002\u0001\u0002\u0001\u0003\u0005\u0003\u009f\b\u0003"+
		"\n\u0003\f\u0003\u00a2\t\u0003\u0001\u0003\u0001\u0003\u0001\u0003\u0001"+
		"\u0004\u0005\u0004\u00a8\b\u0004\n\u0004\f\u0004\u00ab\t\u0004\u0001\u0004"+
		"\u0001\u0004\u0001\u0004\u0001\u0004\u0003\u0004\u00b1\b\u0004\u0001\u0004"+
		"\u0001\u0004\u0001\u0005\u0001\u0005\u0005\u0005\u00b7\b\u0005\n\u0005"+
		"\f\u0005\u00ba\t\u0005\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006"+
		"\u0001\u0006\u0001\u0007\u0001\u0007\u0003\u0007\u00c3\b\u0007\u0001\b"+
		"\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0005\b\u00cb\b\b\n\b\f\b\u00ce"+
		"\t\b\u0001\b\u0001\b\u0003\b\u00d2\b\b\u0001\t\u0003\t\u00d5\b\t\u0001"+
		"\t\u0001\t\u0001\t\u0001\t\u0001\n\u0001\n\u0001\n\u0001\n\u0005\n\u00df"+
		"\b\n\n\n\f\n\u00e2\t\n\u0001\n\u0001\n\u0001\u000b\u0001\u000b\u0001\u000b"+
		"\u0001\u000b\u0003\u000b\u00ea\b\u000b\u0001\u000b\u0001\u000b\u0001\u000b"+
		"\u0001\u000b\u0001\u000b\u0001\u000b\u0001\u000b\u0003\u000b\u00f3\b\u000b"+
		"\u0001\f\u0001\f\u0001\f\u0003\f\u00f8\b\f\u0001\f\u0001\f\u0003\f\u00fc"+
		"\b\f\u0001\r\u0001\r\u0001\r\u0001\r\u0005\r\u0102\b\r\n\r\f\r\u0105\t"+
		"\r\u0001\r\u0001\r\u0001\r\u0001\r\u0003\r\u010b\b\r\u0001\u000e\u0001"+
		"\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000f\u0001\u000f\u0003"+
		"\u000f\u0114\b\u000f\u0001\u0010\u0001\u0010\u0001\u0011\u0001\u0011\u0001"+
		"\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0001\u0012\u0001\u0012\u0001"+
		"\u0012\u0005\u0012\u0121\b\u0012\n\u0012\f\u0012\u0124\t\u0012\u0001\u0012"+
		"\u0003\u0012\u0127\b\u0012\u0001\u0013\u0001\u0013\u0001\u0013\u0001\u0013"+
		"\u0001\u0013\u0001\u0014\u0001\u0014\u0003\u0014\u0130\b\u0014\u0001\u0014"+
		"\u0001\u0014\u0001\u0014\u0001\u0014\u0001\u0014\u0001\u0015\u0005\u0015"+
		"\u0138\b\u0015\n\u0015\f\u0015\u013b\t\u0015\u0001\u0015\u0001\u0015\u0001"+
		"\u0015\u0005\u0015\u0140\b\u0015\n\u0015\f\u0015\u0143\t\u0015\u0001\u0015"+
		"\u0003\u0015\u0146\b\u0015\u0001\u0016\u0001\u0016\u0001\u0016\u0001\u0016"+
		"\u0001\u0016\u0001\u0017\u0001\u0017\u0003\u0017\u014f\b\u0017\u0001\u0018"+
		"\u0001\u0018\u0001\u0018\u0001\u0018\u0001\u0019\u0001\u0019\u0001\u0019"+
		"\u0001\u0019\u0001\u0019\u0003\u0019\u015a\b\u0019\u0001\u0019\u0003\u0019"+
		"\u015d\b\u0019\u0001\u001a\u0001\u001a\u0003\u001a\u0161\b\u001a\u0001"+
		"\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0003\u001a\u0167\b\u001a\u0001"+
		"\u001b\u0001\u001b\u0001\u001b\u0001\u001b\u0001\u001c\u0001\u001c\u0001"+
		"\u001c\u0001\u001c\u0004\u001c\u0171\b\u001c\u000b\u001c\f\u001c\u0172"+
		"\u0001\u001d\u0001\u001d\u0001\u001d\u0001\u001d\u0003\u001d\u0179\b\u001d"+
		"\u0001\u001e\u0001\u001e\u0003\u001e\u017d\b\u001e\u0001\u001f\u0001\u001f"+
		"\u0001\u001f\u0001\u001f\u0001 \u0001 \u0001 \u0003 \u0186\b \u0001 \u0001"+
		" \u0001 \u0003 \u018b\b \u0001 \u0001 \u0005 \u018f\b \n \f \u0192\t "+
		"\u0001!\u0001!\u0001!\u0001!\u0001!\u0001!\u0003!\u019a\b!\u0001\"\u0001"+
		"\"\u0001#\u0001#\u0001$\u0001$\u0001$\u0001%\u0003%\u01a4\b%\u0001%\u0001"+
		"%\u0003%\u01a8\b%\u0001&\u0001&\u0001&\u0001&\u0001&\u0001&\u0005&\u01b0"+
		"\b&\n&\f&\u01b3\t&\u0001&\u0001&\u0003&\u01b7\b&\u0001\'\u0001\'\u0001"+
		"\'\u0001\'\u0001\'\u0001\'\u0005\'\u01bf\b\'\n\'\f\'\u01c2\t\'\u0001\'"+
		"\u0001\'\u0003\'\u01c6\b\'\u0001(\u0001(\u0001(\u0001(\u0001)\u0001)\u0001"+
		")\u0001)\u0005)\u01d0\b)\n)\f)\u01d3\t)\u0001)\u0001)\u0001)\u0001)\u0001"+
		")\u0001)\u0004)\u01db\b)\u000b)\f)\u01dc\u0003)\u01df\b)\u0001*\u0001"+
		"*\u0001*\u0001*\u0001+\u0001+\u0001+\u0005+\u01e8\b+\n+\f+\u01eb\t+\u0001"+
		",\u0001,\u0001,\u0001,\u0001-\u0001-\u0004-\u01f3\b-\u000b-\f-\u01f4\u0001"+
		"-\u0001-\u0001.\u0001.\u0005.\u01fb\b.\n.\f.\u01fe\t.\u0001/\u0003/\u0201"+
		"\b/\u0001/\u0001/\u00010\u00010\u00010\u00011\u00011\u00011\u00031\u020b"+
		"\b1\u00011\u00011\u00031\u020f\b1\u00012\u00012\u00013\u00013\u00013\u0001"+
		"4\u00014\u00014\u00015\u00015\u00016\u00016\u00017\u00017\u00018\u0001"+
		"8\u00019\u00019\u0001:\u0001:\u0001:\u0003:\u0226\b:\u0001:\u0003:\u0229"+
		"\b:\u0001;\u0001;\u0001<\u0001<\u0003<\u022f\b<\u0001=\u0001=\u0001>\u0001"+
		">\u0001>\u0001>\u0000\u0001@?\u0000\u0002\u0004\u0006\b\n\f\u000e\u0010"+
		"\u0012\u0014\u0016\u0018\u001a\u001c\u001e \"$&(*,.02468:<>@BDFHJLNPR"+
		"TVXZ\\^`bdfhjlnprtvxz|\u0000\u0007\u0001\u0000:;\u0001\u0000\n\u000b\u0002"+
		"\u0000\r\rMM\u0002\u0000\u001e\u001e..\u0001\u0000+-\u0001\u0000\u0014"+
		"\u0015\u0001\u0000IK\u024d\u0000\u0083\u0001\u0000\u0000\u0000\u0002\u008b"+
		"\u0001\u0000\u0000\u0000\u0004\u0092\u0001\u0000\u0000\u0000\u0006\u00a0"+
		"\u0001\u0000\u0000\u0000\b\u00a9\u0001\u0000\u0000\u0000\n\u00b8\u0001"+
		"\u0000\u0000\u0000\f\u00bb\u0001\u0000\u0000\u0000\u000e\u00c2\u0001\u0000"+
		"\u0000\u0000\u0010\u00d1\u0001\u0000\u0000\u0000\u0012\u00d4\u0001\u0000"+
		"\u0000\u0000\u0014\u00da\u0001\u0000\u0000\u0000\u0016\u00f2\u0001\u0000"+
		"\u0000\u0000\u0018\u00fb\u0001\u0000\u0000\u0000\u001a\u00fd\u0001\u0000"+
		"\u0000\u0000\u001c\u010c\u0001\u0000\u0000\u0000\u001e\u0113\u0001\u0000"+
		"\u0000\u0000 \u0115\u0001\u0000\u0000\u0000\"\u0117\u0001\u0000\u0000"+
		"\u0000$\u011d\u0001\u0000\u0000\u0000&\u0128\u0001\u0000\u0000\u0000("+
		"\u012d\u0001\u0000\u0000\u0000*\u0139\u0001\u0000\u0000\u0000,\u0147\u0001"+
		"\u0000\u0000\u0000.\u014e\u0001\u0000\u0000\u00000\u0150\u0001\u0000\u0000"+
		"\u00002\u015c\u0001\u0000\u0000\u00004\u0166\u0001\u0000\u0000\u00006"+
		"\u0168\u0001\u0000\u0000\u00008\u0170\u0001\u0000\u0000\u0000:\u0174\u0001"+
		"\u0000\u0000\u0000<\u017c\u0001\u0000\u0000\u0000>\u017e\u0001\u0000\u0000"+
		"\u0000@\u018a\u0001\u0000\u0000\u0000B\u0199\u0001\u0000\u0000\u0000D"+
		"\u019b\u0001\u0000\u0000\u0000F\u019d\u0001\u0000\u0000\u0000H\u019f\u0001"+
		"\u0000\u0000\u0000J\u01a7\u0001\u0000\u0000\u0000L\u01b6\u0001\u0000\u0000"+
		"\u0000N\u01c5\u0001\u0000\u0000\u0000P\u01c7\u0001\u0000\u0000\u0000R"+
		"\u01de\u0001\u0000\u0000\u0000T\u01e0\u0001\u0000\u0000\u0000V\u01e9\u0001"+
		"\u0000\u0000\u0000X\u01ec\u0001\u0000\u0000\u0000Z\u01f0\u0001\u0000\u0000"+
		"\u0000\\\u01f8\u0001\u0000\u0000\u0000^\u0200\u0001\u0000\u0000\u0000"+
		"`\u0204\u0001\u0000\u0000\u0000b\u020e\u0001\u0000\u0000\u0000d\u0210"+
		"\u0001\u0000\u0000\u0000f\u0212\u0001\u0000\u0000\u0000h\u0215\u0001\u0000"+
		"\u0000\u0000j\u0218\u0001\u0000\u0000\u0000l\u021a\u0001\u0000\u0000\u0000"+
		"n\u021c\u0001\u0000\u0000\u0000p\u021e\u0001\u0000\u0000\u0000r\u0220"+
		"\u0001\u0000\u0000\u0000t\u0228\u0001\u0000\u0000\u0000v\u022a\u0001\u0000"+
		"\u0000\u0000x\u022e\u0001\u0000\u0000\u0000z\u0230\u0001\u0000\u0000\u0000"+
		"|\u0232\u0001\u0000\u0000\u0000~\u0082\u0003x<\u0000\u007f\u0082\u0003"+
		"\u0002\u0001\u0000\u0080\u0082\u0003\b\u0004\u0000\u0081~\u0001\u0000"+
		"\u0000\u0000\u0081\u007f\u0001\u0000\u0000\u0000\u0081\u0080\u0001\u0000"+
		"\u0000\u0000\u0082\u0085\u0001\u0000\u0000\u0000\u0083\u0081\u0001\u0000"+
		"\u0000\u0000\u0083\u0084\u0001\u0000\u0000\u0000\u0084\u0086\u0001\u0000"+
		"\u0000\u0000\u0085\u0083\u0001\u0000\u0000\u0000\u0086\u0087\u0005\u0000"+
		"\u0000\u0001\u0087\u0001\u0001\u0000\u0000\u0000\u0088\u008a\u0003r9\u0000"+
		"\u0089\u0088\u0001\u0000\u0000\u0000\u008a\u008d\u0001\u0000\u0000\u0000"+
		"\u008b\u0089\u0001\u0000\u0000\u0000\u008b\u008c\u0001\u0000\u0000\u0000"+
		"\u008c\u008e\u0001\u0000\u0000\u0000\u008d\u008b\u0001\u0000\u0000\u0000"+
		"\u008e\u008f\u0005\u001d\u0000\u0000\u008f\u0090\u0003j5\u0000\u0090\u0091"+
		"\u0003\u0004\u0002\u0000\u0091\u0003\u0001\u0000\u0000\u0000\u0092\u0098"+
		"\u0005\u0016\u0000\u0000\u0093\u0097\u0003\u0006\u0003\u0000\u0094\u0097"+
		"\u0003\b\u0004\u0000\u0095\u0097\u0003x<\u0000\u0096\u0093\u0001\u0000"+
		"\u0000\u0000\u0096\u0094\u0001\u0000\u0000\u0000\u0096\u0095\u0001\u0000"+
		"\u0000\u0000\u0097\u009a\u0001\u0000\u0000\u0000\u0098\u0096\u0001\u0000"+
		"\u0000\u0000\u0098\u0099\u0001\u0000\u0000\u0000\u0099\u009b\u0001\u0000"+
		"\u0000\u0000\u009a\u0098\u0001\u0000\u0000\u0000\u009b\u009c\u0005\u0017"+
		"\u0000\u0000\u009c\u0005\u0001\u0000\u0000\u0000\u009d\u009f\u0003p8\u0000"+
		"\u009e\u009d\u0001\u0000\u0000\u0000\u009f\u00a2\u0001\u0000\u0000\u0000"+
		"\u00a0\u009e\u0001\u0000\u0000\u0000\u00a0\u00a1\u0001\u0000\u0000\u0000"+
		"\u00a1\u00a3\u0001\u0000\u0000\u0000\u00a2\u00a0\u0001\u0000\u0000\u0000"+
		"\u00a3\u00a4\u0005\u001c\u0000\u0000\u00a4\u00a5\u0003\u0014\n\u0000\u00a5"+
		"\u0007\u0001\u0000\u0000\u0000\u00a6\u00a8\u0003v;\u0000\u00a7\u00a6\u0001"+
		"\u0000\u0000\u0000\u00a8\u00ab\u0001\u0000\u0000\u0000\u00a9\u00a7\u0001"+
		"\u0000\u0000\u0000\u00a9\u00aa\u0001\u0000\u0000\u0000\u00aa\u00ac\u0001"+
		"\u0000\u0000\u0000\u00ab\u00a9\u0001\u0000\u0000\u0000\u00ac\u00ad\u0005"+
		"\u001a\u0000\u0000\u00ad\u00ae\u0005\n\u0000\u0000\u00ae\u00b0\u0003\n"+
		"\u0005\u0000\u00af\u00b1\u0005\u001b\u0000\u0000\u00b0\u00af\u0001\u0000"+
		"\u0000\u0000\u00b0\u00b1\u0001\u0000\u0000\u0000\u00b1\u00b2\u0001\u0000"+
		"\u0000\u0000\u00b2\u00b3\u0003\u0014\n\u0000\u00b3\t\u0001\u0000\u0000"+
		"\u0000\u00b4\u00b7\u0003\f\u0006\u0000\u00b5\u00b7\u0005\u0013\u0000\u0000"+
		"\u00b6\u00b4\u0001\u0000\u0000\u0000\u00b6\u00b5\u0001\u0000\u0000\u0000"+
		"\u00b7\u00ba\u0001\u0000\u0000\u0000\u00b8\u00b6\u0001\u0000\u0000\u0000"+
		"\u00b8\u00b9\u0001\u0000\u0000\u0000\u00b9\u000b\u0001\u0000\u0000\u0000"+
		"\u00ba\u00b8\u0001\u0000\u0000\u0000\u00bb\u00bc\u0005)\u0000\u0000\u00bc"+
		"\u00bd\u0003\u000e\u0007\u0000\u00bd\u00be\u00051\u0000\u0000\u00be\u00bf"+
		"\u0003\u000e\u0007\u0000\u00bf\r\u0001\u0000\u0000\u0000\u00c0\u00c3\u0003"+
		"\u0010\b\u0000\u00c1\u00c3\u0005\n\u0000\u0000\u00c2\u00c0\u0001\u0000"+
		"\u0000\u0000\u00c2\u00c1\u0001\u0000\u0000\u0000\u00c3\u000f\u0001\u0000"+
		"\u0000\u0000\u00c4\u00c5\u0005\u000e\u0000\u0000\u00c5\u00d2\u0005@\u0000"+
		"\u0000\u00c6\u00c7\u0005\u000e\u0000\u0000\u00c7\u00cc\u0003\u0012\t\u0000"+
		"\u00c8\u00c9\u0005>\u0000\u0000\u00c9\u00cb\u0003\u0012\t\u0000\u00ca"+
		"\u00c8\u0001\u0000\u0000\u0000\u00cb\u00ce\u0001\u0000\u0000\u0000\u00cc"+
		"\u00ca\u0001\u0000\u0000\u0000\u00cc\u00cd\u0001\u0000\u0000\u0000\u00cd"+
		"\u00cf\u0001\u0000\u0000\u0000\u00ce\u00cc\u0001\u0000\u0000\u0000\u00cf"+
		"\u00d0\u0005@\u0000\u0000\u00d0\u00d2\u0001\u0000\u0000\u0000\u00d1\u00c4"+
		"\u0001\u0000\u0000\u0000\u00d1\u00c6\u0001\u0000\u0000\u0000\u00d2\u0011"+
		"\u0001\u0000\u0000\u0000\u00d3\u00d5\u0005G\u0000\u0000\u00d4\u00d3\u0001"+
		"\u0000\u0000\u0000\u00d4\u00d5\u0001\u0000\u0000\u0000\u00d5\u00d6\u0001"+
		"\u0000\u0000\u0000\u00d6\u00d7\u0005\r\u0000\u0000\u00d7\u00d8\u0005F"+
		"\u0000\u0000\u00d8\u00d9\u0005\r\u0000\u0000\u00d9\u0013\u0001\u0000\u0000"+
		"\u0000\u00da\u00e0\u0005\u0016\u0000\u0000\u00db\u00df\u0003\u0016\u000b"+
		"\u0000\u00dc\u00df\u0005\u0018\u0000\u0000\u00dd\u00df\u0003x<\u0000\u00de"+
		"\u00db\u0001\u0000\u0000\u0000\u00de\u00dc\u0001\u0000\u0000\u0000\u00de"+
		"\u00dd\u0001\u0000\u0000\u0000\u00df\u00e2\u0001\u0000\u0000\u0000\u00e0"+
		"\u00de\u0001\u0000\u0000\u0000\u00e0\u00e1\u0001\u0000\u0000\u0000\u00e1"+
		"\u00e3\u0001\u0000\u0000\u0000\u00e2\u00e0\u0001\u0000\u0000\u0000\u00e3"+
		"\u00e4\u0005\u0017\u0000\u0000\u00e4\u0015\u0001\u0000\u0000\u0000\u00e5"+
		"\u00e6\u0003f3\u0000\u00e6\u00e7\u0005\u0013\u0000\u0000\u00e7\u00f3\u0001"+
		"\u0000\u0000\u0000\u00e8\u00ea\u0003f3\u0000\u00e9\u00e8\u0001\u0000\u0000"+
		"\u0000\u00e9\u00ea\u0001\u0000\u0000\u0000\u00ea\u00eb\u0001\u0000\u0000"+
		"\u0000\u00eb\u00f3\u0003\u0014\n\u0000\u00ec\u00f3\u0003\u0018\f\u0000"+
		"\u00ed\u00f3\u00030\u0018\u0000\u00ee\u00f3\u00034\u001a\u0000\u00ef\u00f3"+
		"\u0003(\u0014\u0000\u00f0\u00f3\u0003\u001a\r\u0000\u00f1\u00f3\u0003"+
		"\"\u0011\u0000\u00f2\u00e5\u0001\u0000\u0000\u0000\u00f2\u00e9\u0001\u0000"+
		"\u0000\u0000\u00f2\u00ec\u0001\u0000\u0000\u0000\u00f2\u00ed\u0001\u0000"+
		"\u0000\u0000\u00f2\u00ee\u0001\u0000\u0000\u0000\u00f2\u00ef\u0001\u0000"+
		"\u0000\u0000\u00f2\u00f0\u0001\u0000\u0000\u0000\u00f2\u00f1\u0001\u0000"+
		"\u0000\u0000\u00f3\u0017\u0001\u0000\u0000\u0000\u00f4\u00f5\u0005\n\u0000"+
		"\u0000\u00f5\u00f7\u0003R)\u0000\u00f6\u00f8\u0003J%\u0000\u00f7\u00f6"+
		"\u0001\u0000\u0000\u0000\u00f7\u00f8\u0001\u0000\u0000\u0000\u00f8\u00fc"+
		"\u0001\u0000\u0000\u0000\u00f9\u00fa\u0005\n\u0000\u0000\u00fa\u00fc\u0003"+
		"J%\u0000\u00fb\u00f4\u0001\u0000\u0000\u0000\u00fb\u00f9\u0001\u0000\u0000"+
		"\u0000\u00fc\u0019\u0001\u0000\u0000\u0000\u00fd\u00fe\u0005*\u0000\u0000"+
		"\u00fe\u00ff\u0003 \u0010\u0000\u00ff\u010a\u0003\u0014\n\u0000\u0100"+
		"\u0102\u0003x<\u0000\u0101\u0100\u0001\u0000\u0000\u0000\u0102\u0105\u0001"+
		"\u0000\u0000\u0000\u0103\u0101\u0001\u0000\u0000\u0000\u0103\u0104\u0001"+
		"\u0000\u0000\u0000\u0104\u0106\u0001\u0000\u0000\u0000\u0105\u0103\u0001"+
		"\u0000\u0000\u0000\u0106\u0107\u0005 \u0000\u0000\u0107\u0108\u0003\u001e"+
		"\u000f\u0000\u0108\u0109\u0005\u0013\u0000\u0000\u0109\u010b\u0001\u0000"+
		"\u0000\u0000\u010a\u0103\u0001\u0000\u0000\u0000\u010a\u010b\u0001\u0000"+
		"\u0000\u0000\u010b\u001b\u0001\u0000\u0000\u0000\u010c\u010d\u0005 \u0000"+
		"\u0000\u010d\u010e\u00051\u0000\u0000\u010e\u010f\u0003\u001e\u000f\u0000"+
		"\u010f\u0110\u0005\u0013\u0000\u0000\u0110\u001d\u0001\u0000\u0000\u0000"+
		"\u0111\u0114\u0003<\u001e\u0000\u0112\u0114\u0003\u0016\u000b\u0000\u0113"+
		"\u0111\u0001\u0000\u0000\u0000\u0113\u0112\u0001\u0000\u0000\u0000\u0114"+
		"\u001f\u0001\u0000\u0000\u0000\u0115\u0116\u0003R)\u0000\u0116!\u0001"+
		"\u0000\u0000\u0000\u0117\u0118\u0005*\u0000\u0000\u0118\u0119\u0005\u0016"+
		"\u0000\u0000\u0119\u011a\u0005\u0013\u0000\u0000\u011a\u011b\u0003$\u0012"+
		"\u0000\u011b\u011c\u0005\u0017\u0000\u0000\u011c#\u0001\u0000\u0000\u0000"+
		"\u011d\u0122\u0003&\u0013\u0000\u011e\u0121\u0003&\u0013\u0000\u011f\u0121"+
		"\u0003x<\u0000\u0120\u011e\u0001\u0000\u0000\u0000\u0120\u011f\u0001\u0000"+
		"\u0000\u0000\u0121\u0124\u0001\u0000\u0000\u0000\u0122\u0120\u0001\u0000"+
		"\u0000\u0000\u0122\u0123\u0001\u0000\u0000\u0000\u0123\u0126\u0001\u0000"+
		"\u0000\u0000\u0124\u0122\u0001\u0000\u0000\u0000\u0125\u0127\u0003\u001c"+
		"\u000e\u0000\u0126\u0125\u0001\u0000\u0000\u0000\u0126\u0127\u0001\u0000"+
		"\u0000\u0000\u0127%\u0001\u0000\u0000\u0000\u0128\u0129\u0003 \u0010\u0000"+
		"\u0129\u012a\u00051\u0000\u0000\u012a\u012b\u0003\u001e\u000f\u0000\u012b"+
		"\u012c\u0005\u0013\u0000\u0000\u012c\'\u0001\u0000\u0000\u0000\u012d\u012f"+
		"\u0005*\u0000\u0000\u012e\u0130\u0003<\u001e\u0000\u012f\u012e\u0001\u0000"+
		"\u0000\u0000\u012f\u0130\u0001\u0000\u0000\u0000\u0130\u0131\u0001\u0000"+
		"\u0000\u0000\u0131\u0132\u0005\u0016\u0000\u0000\u0132\u0133\u0005\u0013"+
		"\u0000\u0000\u0133\u0134\u0003*\u0015\u0000\u0134\u0135\u0005\u0017\u0000"+
		"\u0000\u0135)\u0001\u0000\u0000\u0000\u0136\u0138\u0003x<\u0000\u0137"+
		"\u0136\u0001\u0000\u0000\u0000\u0138\u013b\u0001\u0000\u0000\u0000\u0139"+
		"\u0137\u0001\u0000\u0000\u0000\u0139\u013a\u0001\u0000\u0000\u0000\u013a"+
		"\u013c\u0001\u0000\u0000\u0000\u013b\u0139\u0001\u0000\u0000\u0000\u013c"+
		"\u0141\u0003,\u0016\u0000\u013d\u0140\u0003,\u0016\u0000\u013e\u0140\u0003"+
		"x<\u0000\u013f\u013d\u0001\u0000\u0000\u0000\u013f\u013e\u0001\u0000\u0000"+
		"\u0000\u0140\u0143\u0001\u0000\u0000\u0000\u0141\u013f\u0001\u0000\u0000"+
		"\u0000\u0141\u0142\u0001\u0000\u0000\u0000\u0142\u0145\u0001\u0000\u0000"+
		"\u0000\u0143\u0141\u0001\u0000\u0000\u0000\u0144\u0146\u0003\u001c\u000e"+
		"\u0000\u0145\u0144\u0001\u0000\u0000\u0000\u0145\u0146\u0001\u0000\u0000"+
		"\u0000\u0146+\u0001\u0000\u0000\u0000\u0147\u0148\u0003.\u0017\u0000\u0148"+
		"\u0149\u00051\u0000\u0000\u0149\u014a\u0003\u001e\u000f\u0000\u014a\u014b"+
		"\u0005\u0013\u0000\u0000\u014b-\u0001\u0000\u0000\u0000\u014c\u014f\u0003"+
		"<\u001e\u0000\u014d\u014f\u0003Z-\u0000\u014e\u014c\u0001\u0000\u0000"+
		"\u0000\u014e\u014d\u0001\u0000\u0000\u0000\u014f/\u0001\u0000\u0000\u0000"+
		"\u0150\u0151\u0003t:\u0000\u0151\u0152\u0003l6\u0000\u0152\u0153\u0003"+
		"2\u0019\u0000\u01531\u0001\u0000\u0000\u0000\u0154\u0159\u00050\u0000"+
		"\u0000\u0155\u015a\u0003<\u001e\u0000\u0156\u015a\u0003(\u0014\u0000\u0157"+
		"\u015a\u0003\"\u0011\u0000\u0158\u015a\u0003\u0018\f\u0000\u0159\u0155"+
		"\u0001\u0000\u0000\u0000\u0159\u0156\u0001\u0000\u0000\u0000\u0159\u0157"+
		"\u0001\u0000\u0000\u0000\u0159\u0158\u0001\u0000\u0000\u0000\u015a\u015d"+
		"\u0001\u0000\u0000\u0000\u015b\u015d\u0003J%\u0000\u015c\u0154\u0001\u0000"+
		"\u0000\u0000\u015c\u015b\u0001\u0000\u0000\u0000\u015d3\u0001\u0000\u0000"+
		"\u0000\u015e\u0160\u0005!\u0000\u0000\u015f\u0161\u0003R)\u0000\u0160"+
		"\u015f\u0001\u0000\u0000\u0000\u0160\u0161\u0001\u0000\u0000\u0000\u0161"+
		"\u0167\u0001\u0000\u0000\u0000\u0162\u0163\u0005\"\u0000\u0000\u0163\u0167"+
		"\u0005\n\u0000\u0000\u0164\u0167\u0005$\u0000\u0000\u0165\u0167\u0005"+
		"#\u0000\u0000\u0166\u015e\u0001\u0000\u0000\u0000\u0166\u0162\u0001\u0000"+
		"\u0000\u0000\u0166\u0164\u0001\u0000\u0000\u0000\u0166\u0165\u0001\u0000"+
		"\u0000\u0000\u01675\u0001\u0000\u0000\u0000\u0168\u0169\u0005\u0007\u0000"+
		"\u0000\u0169\u016a\u00038\u001c\u0000\u016a\u016b\u0005\u0002\u0000\u0000"+
		"\u016b7\u0001\u0000\u0000\u0000\u016c\u0171\u00057\u0000\u0000\u016d\u0171"+
		"\u00058\u0000\u0000\u016e\u0171\u00059\u0000\u0000\u016f\u0171\u0003:"+
		"\u001d\u0000\u0170\u016c\u0001\u0000\u0000\u0000\u0170\u016d\u0001\u0000"+
		"\u0000\u0000\u0170\u016e\u0001\u0000\u0000\u0000\u0170\u016f\u0001\u0000"+
		"\u0000\u0000\u0171\u0172\u0001\u0000\u0000\u0000\u0172\u0170\u0001\u0000"+
		"\u0000\u0000\u0172\u0173\u0001\u0000\u0000\u0000\u01739\u0001\u0000\u0000"+
		"\u0000\u0174\u0175\u0005\b\u0000\u0000\u0175\u0176\u0003\\.\u0000\u0176"+
		"\u0178\u0005@\u0000\u0000\u0177\u0179\u0005L\u0000\u0000\u0178\u0177\u0001"+
		"\u0000\u0000\u0000\u0178\u0179\u0001\u0000\u0000\u0000\u0179;\u0001\u0000"+
		"\u0000\u0000\u017a\u017d\u0003@ \u0000\u017b\u017d\u0003T*\u0000\u017c"+
		"\u017a\u0001\u0000\u0000\u0000\u017c\u017b\u0001\u0000\u0000\u0000\u017d"+
		"=\u0001\u0000\u0000\u0000\u017e\u017f\u00052\u0000\u0000\u017f\u0180\u0003"+
		"<\u001e\u0000\u0180\u0181\u0005\f\u0000\u0000\u0181?\u0001\u0000\u0000"+
		"\u0000\u0182\u0183\u0006 \uffff\uffff\u0000\u0183\u0185\u0003B!\u0000"+
		"\u0184\u0186\u0003`0\u0000\u0185\u0184\u0001\u0000\u0000\u0000\u0185\u0186"+
		"\u0001\u0000\u0000\u0000\u0186\u018b\u0001\u0000\u0000\u0000\u0187\u0188"+
		"\u0003D\"\u0000\u0188\u0189\u0003@ \u0002\u0189\u018b\u0001\u0000\u0000"+
		"\u0000\u018a\u0182\u0001\u0000\u0000\u0000\u018a\u0187\u0001\u0000\u0000"+
		"\u0000\u018b\u0190\u0001\u0000\u0000\u0000\u018c\u018d\n\u0001\u0000\u0000"+
		"\u018d\u018f\u0003F#\u0000\u018e\u018c\u0001\u0000\u0000\u0000\u018f\u0192"+
		"\u0001\u0000\u0000\u0000\u0190\u018e\u0001\u0000\u0000\u0000\u0190\u0191"+
		"\u0001\u0000\u0000\u0000\u0191A\u0001\u0000\u0000\u0000\u0192\u0190\u0001"+
		"\u0000\u0000\u0000\u0193\u019a\u0003X,\u0000\u0194\u019a\u0003\\.\u0000"+
		"\u0195\u019a\u0003N\'\u0000\u0196\u019a\u0003L&\u0000\u0197\u019a\u0003"+
		">\u001f\u0000\u0198\u019a\u0003H$\u0000\u0199\u0193\u0001\u0000\u0000"+
		"\u0000\u0199\u0194\u0001\u0000\u0000\u0000\u0199\u0195\u0001\u0000\u0000"+
		"\u0000\u0199\u0196\u0001\u0000\u0000\u0000\u0199\u0197\u0001\u0000\u0000"+
		"\u0000\u0199\u0198\u0001\u0000\u0000\u0000\u019aC\u0001\u0000\u0000\u0000"+
		"\u019b\u019c\u0005C\u0000\u0000\u019cE\u0001\u0000\u0000\u0000\u019d\u019e"+
		"\u0005D\u0000\u0000\u019eG\u0001\u0000\u0000\u0000\u019f\u01a0\u0003n"+
		"7\u0000\u01a0\u01a1\u0003R)\u0000\u01a1I\u0001\u0000\u0000\u0000\u01a2"+
		"\u01a4\u0005\u0013\u0000\u0000\u01a3\u01a2\u0001\u0000\u0000\u0000\u01a3"+
		"\u01a4\u0001\u0000\u0000\u0000\u01a4\u01a5\u0001\u0000\u0000\u0000\u01a5"+
		"\u01a8\u00036\u001b\u0000\u01a6\u01a8\u0003@ \u0000\u01a7\u01a3\u0001"+
		"\u0000\u0000\u0000\u01a7\u01a6\u0001\u0000\u0000\u0000\u01a8K\u0001\u0000"+
		"\u0000\u0000\u01a9\u01aa\u0005\u000f\u0000\u0000\u01aa\u01b7\u0005A\u0000"+
		"\u0000\u01ab\u01ac\u0005\u000f\u0000\u0000\u01ac\u01b1\u0003<\u001e\u0000"+
		"\u01ad\u01ae\u0005>\u0000\u0000\u01ae\u01b0\u0003<\u001e\u0000\u01af\u01ad"+
		"\u0001\u0000\u0000\u0000\u01b0\u01b3\u0001\u0000\u0000\u0000\u01b1\u01af"+
		"\u0001\u0000\u0000\u0000\u01b1\u01b2\u0001\u0000\u0000\u0000\u01b2\u01b4"+
		"\u0001\u0000\u0000\u0000\u01b3\u01b1\u0001\u0000\u0000\u0000\u01b4\u01b5"+
		"\u0005A\u0000\u0000\u01b5\u01b7\u0001\u0000\u0000\u0000\u01b6\u01a9\u0001"+
		"\u0000\u0000\u0000\u01b6\u01ab\u0001\u0000\u0000\u0000\u01b7M\u0001\u0000"+
		"\u0000\u0000\u01b8\u01b9\u0005\u000e\u0000\u0000\u01b9\u01c6\u0005@\u0000"+
		"\u0000\u01ba\u01bb\u0005\u000e\u0000\u0000\u01bb\u01c0\u0003P(\u0000\u01bc"+
		"\u01bd\u0005>\u0000\u0000\u01bd\u01bf\u0003P(\u0000\u01be\u01bc\u0001"+
		"\u0000\u0000\u0000\u01bf\u01c2\u0001\u0000\u0000\u0000\u01c0\u01be\u0001"+
		"\u0000\u0000\u0000\u01c0\u01c1\u0001\u0000\u0000\u0000\u01c1\u01c3\u0001"+
		"\u0000\u0000\u0000\u01c2\u01c0\u0001\u0000\u0000\u0000\u01c3\u01c4\u0005"+
		"@\u0000\u0000\u01c4\u01c6\u0001\u0000\u0000\u0000\u01c5\u01b8\u0001\u0000"+
		"\u0000\u0000\u01c5\u01ba\u0001\u0000\u0000\u0000\u01c6O\u0001\u0000\u0000"+
		"\u0000\u01c7\u01c8\u0005\r\u0000\u0000\u01c8\u01c9\u0005F\u0000\u0000"+
		"\u01c9\u01ca\u0003<\u001e\u0000\u01caQ\u0001\u0000\u0000\u0000\u01cb\u01cc"+
		"\u00054\u0000\u0000\u01cc\u01d1\u0003<\u001e\u0000\u01cd\u01ce\u0005>"+
		"\u0000\u0000\u01ce\u01d0\u0003<\u001e\u0000\u01cf\u01cd\u0001\u0000\u0000"+
		"\u0000\u01d0\u01d3\u0001\u0000\u0000\u0000\u01d1\u01cf\u0001\u0000\u0000"+
		"\u0000\u01d1\u01d2\u0001\u0000\u0000\u0000\u01d2\u01d4\u0001\u0000\u0000"+
		"\u0000\u01d3\u01d1\u0001\u0000\u0000\u0000\u01d4\u01d5\u0005?\u0000\u0000"+
		"\u01d5\u01df\u0001\u0000\u0000\u0000\u01d6\u01d7\u00054\u0000\u0000\u01d7"+
		"\u01df\u0005?\u0000\u0000\u01d8\u01df\u00053\u0000\u0000\u01d9\u01db\u0003"+
		"T*\u0000\u01da\u01d9\u0001\u0000\u0000\u0000\u01db\u01dc\u0001\u0000\u0000"+
		"\u0000\u01dc\u01da\u0001\u0000\u0000\u0000\u01dc\u01dd\u0001\u0000\u0000"+
		"\u0000\u01dd\u01df\u0001\u0000\u0000\u0000\u01de\u01cb\u0001\u0000\u0000"+
		"\u0000\u01de\u01d6\u0001\u0000\u0000\u0000\u01de\u01d8\u0001\u0000\u0000"+
		"\u0000\u01de\u01da\u0001\u0000\u0000\u0000\u01dfS\u0001\u0000\u0000\u0000"+
		"\u01e0\u01e1\u0005\u0004\u0000\u0000\u01e1\u01e2\u0003V+\u0000\u01e2\u01e3"+
		"\u00056\u0000\u0000\u01e3U\u0001\u0000\u0000\u0000\u01e4\u01e8\u00055"+
		"\u0000\u0000\u01e5\u01e8\u0003|>\u0000\u01e6\u01e8\u0003X,\u0000\u01e7"+
		"\u01e4\u0001\u0000\u0000\u0000\u01e7\u01e5\u0001\u0000\u0000\u0000\u01e7"+
		"\u01e6\u0001\u0000\u0000\u0000\u01e8\u01eb\u0001\u0000\u0000\u0000\u01e9"+
		"\u01e7\u0001\u0000\u0000\u0000\u01e9\u01ea\u0001\u0000\u0000\u0000\u01ea"+
		"W\u0001\u0000\u0000\u0000\u01eb\u01e9\u0001\u0000\u0000\u0000\u01ec\u01ed"+
		"\u0005\t\u0000\u0000\u01ed\u01ee\u0003<\u001e\u0000\u01ee\u01ef\u0005"+
		"A\u0000\u0000\u01efY\u0001\u0000\u0000\u0000\u01f0\u01f2\u0005\t\u0000"+
		"\u0000\u01f1\u01f3\u0003b1\u0000\u01f2\u01f1\u0001\u0000\u0000\u0000\u01f3"+
		"\u01f4\u0001\u0000\u0000\u0000\u01f4\u01f2\u0001\u0000\u0000\u0000\u01f4"+
		"\u01f5\u0001\u0000\u0000\u0000\u01f5\u01f6\u0001\u0000\u0000\u0000\u01f6"+
		"\u01f7\u0005A\u0000\u0000\u01f7[\u0001\u0000\u0000\u0000\u01f8\u01fc\u0003"+
		"^/\u0000\u01f9\u01fb\u0003b1\u0000\u01fa\u01f9\u0001\u0000\u0000\u0000"+
		"\u01fb\u01fe\u0001\u0000\u0000\u0000\u01fc\u01fa\u0001\u0000\u0000\u0000"+
		"\u01fc\u01fd\u0001\u0000\u0000\u0000\u01fd]\u0001\u0000\u0000\u0000\u01fe"+
		"\u01fc\u0001\u0000\u0000\u0000\u01ff\u0201\u0005B\u0000\u0000\u0200\u01ff"+
		"\u0001\u0000\u0000\u0000\u0200\u0201\u0001\u0000\u0000\u0000\u0201\u0202"+
		"\u0001\u0000\u0000\u0000\u0202\u0203\u0005\r\u0000\u0000\u0203_\u0001"+
		"\u0000\u0000\u0000\u0204\u0205\u0005E\u0000\u0000\u0205\u0206\u0003<\u001e"+
		"\u0000\u0206a\u0001\u0000\u0000\u0000\u0207\u0208\u0003d2\u0000\u0208"+
		"\u020a\u0003n7\u0000\u0209\u020b\u0003R)\u0000\u020a\u0209\u0001\u0000"+
		"\u0000\u0000\u020a\u020b\u0001\u0000\u0000\u0000\u020b\u020f\u0001\u0000"+
		"\u0000\u0000\u020c\u020f\u0003h4\u0000\u020d\u020f\u0005=\u0000\u0000"+
		"\u020e\u0207\u0001\u0000\u0000\u0000\u020e\u020c\u0001\u0000\u0000\u0000"+
		"\u020e\u020d\u0001\u0000\u0000\u0000\u020fc\u0001\u0000\u0000\u0000\u0210"+
		"\u0211\u0007\u0000\u0000\u0000\u0211e\u0001\u0000\u0000\u0000\u0212\u0213"+
		"\u0005/\u0000\u0000\u0213\u0214\u0005O\u0000\u0000\u0214g\u0001\u0000"+
		"\u0000\u0000\u0215\u0216\u0005<\u0000\u0000\u0216\u0217\u0005N\u0000\u0000"+
		"\u0217i\u0001\u0000\u0000\u0000\u0218\u0219\u0007\u0001\u0000\u0000\u0219"+
		"k\u0001\u0000\u0000\u0000\u021a\u021b\u0005\n\u0000\u0000\u021bm\u0001"+
		"\u0000\u0000\u0000\u021c\u021d\u0007\u0002\u0000\u0000\u021do\u0001\u0000"+
		"\u0000\u0000\u021e\u021f\u0007\u0003\u0000\u0000\u021fq\u0001\u0000\u0000"+
		"\u0000\u0220\u0221\u0005\u001f\u0000\u0000\u0221s\u0001\u0000\u0000\u0000"+
		"\u0222\u0229\u0005(\u0000\u0000\u0223\u0229\u0005\'\u0000\u0000\u0224"+
		"\u0226\u0005%\u0000\u0000\u0225\u0224\u0001\u0000\u0000\u0000\u0225\u0226"+
		"\u0001\u0000\u0000\u0000\u0226\u0227\u0001\u0000\u0000\u0000\u0227\u0229"+
		"\u0005&\u0000\u0000\u0228\u0222\u0001\u0000\u0000\u0000\u0228\u0223\u0001"+
		"\u0000\u0000\u0000\u0228\u0225\u0001\u0000\u0000\u0000\u0229u\u0001\u0000"+
		"\u0000\u0000\u022a\u022b\u0007\u0004\u0000\u0000\u022bw\u0001\u0000\u0000"+
		"\u0000\u022c\u022f\u0003z=\u0000\u022d\u022f\u0005\u0013\u0000\u0000\u022e"+
		"\u022c\u0001\u0000\u0000\u0000\u022e\u022d\u0001\u0000\u0000\u0000\u022f"+
		"y\u0001\u0000\u0000\u0000\u0230\u0231\u0007\u0005\u0000\u0000\u0231{\u0001"+
		"\u0000\u0000\u0000\u0232\u0233\u0005\u0003\u0000\u0000\u0233\u0234\u0007"+
		"\u0006\u0000\u0000\u0234}\u0001\u0000\u0000\u0000?\u0081\u0083\u008b\u0096"+
		"\u0098\u00a0\u00a9\u00b0\u00b6\u00b8\u00c2\u00cc\u00d1\u00d4\u00de\u00e0"+
		"\u00e9\u00f2\u00f7\u00fb\u0103\u010a\u0113\u0120\u0122\u0126\u012f\u0139"+
		"\u013f\u0141\u0145\u014e\u0159\u015c\u0160\u0166\u0170\u0172\u0178\u017c"+
		"\u0185\u018a\u0190\u0199\u01a3\u01a7\u01b1\u01b6\u01c0\u01c5\u01d1\u01dc"+
		"\u01de\u01e7\u01e9\u01f4\u01fc\u0200\u020a\u020e\u0225\u0228\u022e";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}