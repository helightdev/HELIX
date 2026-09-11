// Generated from HalLexer.g4 by ANTLR 4.13.2
package dev.helight.helix.hal.generated;
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast", "CheckReturnValue", "this-escape"})
public class HalLexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.13.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		SECTION=1, PERCENT=2, AMP=3, HASH=4, EQUALS=5, COMMA=6, LBRACE=7, RBRACE=8,
		LBRACKET=9, RBRACKET=10, LPAREN=11, RPAREN=12, LANGLE=13, RANGLE=14, TRUE=15,
		FALSE=16, NULL=17, NUMBER=18, STRING=19, IDENTIFIER=20, BARE=21, LINE_COMMENT=22,
		NEWLINE=23, SPACE=24, ERROR_TOKEN=25;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"SECTION", "PERCENT", "AMP", "HASH", "EQUALS", "COMMA", "LBRACE", "RBRACE",
			"LBRACKET", "RBRACKET", "LPAREN", "RPAREN", "LANGLE", "RANGLE", "TRUE",
			"FALSE", "NULL", "NUMBER", "STRING", "IDENTIFIER", "BARE", "LINE_COMMENT",
			"NEWLINE", "SPACE", "ERROR_TOKEN"
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


	public HalLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "HalLexer.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	public static final String _serializedATN =
		"\u0004\u0000\u0019\u00ac\u0006\uffff\uffff\u0002\u0000\u0007\u0000\u0002"+
		"\u0001\u0007\u0001\u0002\u0002\u0007\u0002\u0002\u0003\u0007\u0003\u0002"+
		"\u0004\u0007\u0004\u0002\u0005\u0007\u0005\u0002\u0006\u0007\u0006\u0002"+
		"\u0007\u0007\u0007\u0002\b\u0007\b\u0002\t\u0007\t\u0002\n\u0007\n\u0002"+
		"\u000b\u0007\u000b\u0002\f\u0007\f\u0002\r\u0007\r\u0002\u000e\u0007\u000e"+
		"\u0002\u000f\u0007\u000f\u0002\u0010\u0007\u0010\u0002\u0011\u0007\u0011"+
		"\u0002\u0012\u0007\u0012\u0002\u0013\u0007\u0013\u0002\u0014\u0007\u0014"+
		"\u0002\u0015\u0007\u0015\u0002\u0016\u0007\u0016\u0002\u0017\u0007\u0017"+
		"\u0002\u0018\u0007\u0018\u0001\u0000\u0001\u0000\u0001\u0000\u0001\u0000"+
		"\u0001\u0001\u0001\u0001\u0001\u0002\u0001\u0002\u0001\u0003\u0001\u0003"+
		"\u0001\u0004\u0001\u0004\u0001\u0005\u0001\u0005\u0001\u0006\u0001\u0006"+
		"\u0001\u0007\u0001\u0007\u0001\b\u0001\b\u0001\t\u0001\t\u0001\n\u0001"+
		"\n\u0001\u000b\u0001\u000b\u0001\f\u0001\f\u0001\r\u0001\r\u0001\u000e"+
		"\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000f\u0001\u000f"+
		"\u0001\u000f\u0001\u000f\u0001\u000f\u0001\u000f\u0001\u0010\u0001\u0010"+
		"\u0001\u0010\u0001\u0010\u0001\u0010\u0001\u0011\u0003\u0011c\b\u0011"+
		"\u0001\u0011\u0004\u0011f\b\u0011\u000b\u0011\f\u0011g\u0001\u0011\u0001"+
		"\u0011\u0004\u0011l\b\u0011\u000b\u0011\f\u0011m\u0003\u0011p\b\u0011"+
		"\u0001\u0011\u0001\u0011\u0003\u0011t\b\u0011\u0001\u0011\u0004\u0011"+
		"w\b\u0011\u000b\u0011\f\u0011x\u0003\u0011{\b\u0011\u0001\u0012\u0001"+
		"\u0012\u0001\u0012\u0001\u0012\u0005\u0012\u0081\b\u0012\n\u0012\f\u0012"+
		"\u0084\t\u0012\u0001\u0012\u0001\u0012\u0001\u0013\u0001\u0013\u0005\u0013"+
		"\u008a\b\u0013\n\u0013\f\u0013\u008d\t\u0013\u0001\u0014\u0004\u0014\u0090"+
		"\b\u0014\u000b\u0014\f\u0014\u0091\u0001\u0015\u0001\u0015\u0001\u0015"+
		"\u0001\u0015\u0005\u0015\u0098\b\u0015\n\u0015\f\u0015\u009b\t\u0015\u0001"+
		"\u0015\u0001\u0015\u0001\u0016\u0003\u0016\u00a0\b\u0016\u0001\u0016\u0001"+
		"\u0016\u0001\u0017\u0004\u0017\u00a5\b\u0017\u000b\u0017\f\u0017\u00a6"+
		"\u0001\u0017\u0001\u0017\u0001\u0018\u0001\u0018\u0000\u0000\u0019\u0001"+
		"\u0001\u0003\u0002\u0005\u0003\u0007\u0004\t\u0005\u000b\u0006\r\u0007"+
		"\u000f\b\u0011\t\u0013\n\u0015\u000b\u0017\f\u0019\r\u001b\u000e\u001d"+
		"\u000f\u001f\u0010!\u0011#\u0012%\u0013\'\u0014)\u0015+\u0016-\u0017/"+
		"\u00181\u0019\u0001\u0000\t\u0001\u000009\u0002\u0000EEee\u0002\u0000"+
		"++--\u0004\u0000\n\n\r\r\"\"\\\\\u0003\u0000AZ__az\u0005\u0000-.09AZ_"+
		"_az\f\u0000\t\n\r\r  \"#%&(),,<>[[]]{{}}\u0002\u0000\n\n\r\r\u0002\u0000"+
		"\t\t  \u00b9\u0000\u0001\u0001\u0000\u0000\u0000\u0000\u0003\u0001\u0000"+
		"\u0000\u0000\u0000\u0005\u0001\u0000\u0000\u0000\u0000\u0007\u0001\u0000"+
		"\u0000\u0000\u0000\t\u0001\u0000\u0000\u0000\u0000\u000b\u0001\u0000\u0000"+
		"\u0000\u0000\r\u0001\u0000\u0000\u0000\u0000\u000f\u0001\u0000\u0000\u0000"+
		"\u0000\u0011\u0001\u0000\u0000\u0000\u0000\u0013\u0001\u0000\u0000\u0000"+
		"\u0000\u0015\u0001\u0000\u0000\u0000\u0000\u0017\u0001\u0000\u0000\u0000"+
		"\u0000\u0019\u0001\u0000\u0000\u0000\u0000\u001b\u0001\u0000\u0000\u0000"+
		"\u0000\u001d\u0001\u0000\u0000\u0000\u0000\u001f\u0001\u0000\u0000\u0000"+
		"\u0000!\u0001\u0000\u0000\u0000\u0000#\u0001\u0000\u0000\u0000\u0000%"+
		"\u0001\u0000\u0000\u0000\u0000\'\u0001\u0000\u0000\u0000\u0000)\u0001"+
		"\u0000\u0000\u0000\u0000+\u0001\u0000\u0000\u0000\u0000-\u0001\u0000\u0000"+
		"\u0000\u0000/\u0001\u0000\u0000\u0000\u00001\u0001\u0000\u0000\u0000\u0001"+
		"3\u0001\u0000\u0000\u0000\u00037\u0001\u0000\u0000\u0000\u00059\u0001"+
		"\u0000\u0000\u0000\u0007;\u0001\u0000\u0000\u0000\t=\u0001\u0000\u0000"+
		"\u0000\u000b?\u0001\u0000\u0000\u0000\rA\u0001\u0000\u0000\u0000\u000f"+
		"C\u0001\u0000\u0000\u0000\u0011E\u0001\u0000\u0000\u0000\u0013G\u0001"+
		"\u0000\u0000\u0000\u0015I\u0001\u0000\u0000\u0000\u0017K\u0001\u0000\u0000"+
		"\u0000\u0019M\u0001\u0000\u0000\u0000\u001bO\u0001\u0000\u0000\u0000\u001d"+
		"Q\u0001\u0000\u0000\u0000\u001fV\u0001\u0000\u0000\u0000!\\\u0001\u0000"+
		"\u0000\u0000#b\u0001\u0000\u0000\u0000%|\u0001\u0000\u0000\u0000\'\u0087"+
		"\u0001\u0000\u0000\u0000)\u008f\u0001\u0000\u0000\u0000+\u0093\u0001\u0000"+
		"\u0000\u0000-\u009f\u0001\u0000\u0000\u0000/\u00a4\u0001\u0000\u0000\u0000"+
		"1\u00aa\u0001\u0000\u0000\u000034\u0005-\u0000\u000045\u0005-\u0000\u0000"+
		"56\u0005-\u0000\u00006\u0002\u0001\u0000\u0000\u000078\u0005%\u0000\u0000"+
		"8\u0004\u0001\u0000\u0000\u00009:\u0005&\u0000\u0000:\u0006\u0001\u0000"+
		"\u0000\u0000;<\u0005#\u0000\u0000<\b\u0001\u0000\u0000\u0000=>\u0005="+
		"\u0000\u0000>\n\u0001\u0000\u0000\u0000?@\u0005,\u0000\u0000@\f\u0001"+
		"\u0000\u0000\u0000AB\u0005{\u0000\u0000B\u000e\u0001\u0000\u0000\u0000"+
		"CD\u0005}\u0000\u0000D\u0010\u0001\u0000\u0000\u0000EF\u0005[\u0000\u0000"+
		"F\u0012\u0001\u0000\u0000\u0000GH\u0005]\u0000\u0000H\u0014\u0001\u0000"+
		"\u0000\u0000IJ\u0005(\u0000\u0000J\u0016\u0001\u0000\u0000\u0000KL\u0005"+
		")\u0000\u0000L\u0018\u0001\u0000\u0000\u0000MN\u0005<\u0000\u0000N\u001a"+
		"\u0001\u0000\u0000\u0000OP\u0005>\u0000\u0000P\u001c\u0001\u0000\u0000"+
		"\u0000QR\u0005t\u0000\u0000RS\u0005r\u0000\u0000ST\u0005u\u0000\u0000"+
		"TU\u0005e\u0000\u0000U\u001e\u0001\u0000\u0000\u0000VW\u0005f\u0000\u0000"+
		"WX\u0005a\u0000\u0000XY\u0005l\u0000\u0000YZ\u0005s\u0000\u0000Z[\u0005"+
		"e\u0000\u0000[ \u0001\u0000\u0000\u0000\\]\u0005n\u0000\u0000]^\u0005"+
		"u\u0000\u0000^_\u0005l\u0000\u0000_`\u0005l\u0000\u0000`\"\u0001\u0000"+
		"\u0000\u0000ac\u0005-\u0000\u0000ba\u0001\u0000\u0000\u0000bc\u0001\u0000"+
		"\u0000\u0000ce\u0001\u0000\u0000\u0000df\u0007\u0000\u0000\u0000ed\u0001"+
		"\u0000\u0000\u0000fg\u0001\u0000\u0000\u0000ge\u0001\u0000\u0000\u0000"+
		"gh\u0001\u0000\u0000\u0000ho\u0001\u0000\u0000\u0000ik\u0005.\u0000\u0000"+
		"jl\u0007\u0000\u0000\u0000kj\u0001\u0000\u0000\u0000lm\u0001\u0000\u0000"+
		"\u0000mk\u0001\u0000\u0000\u0000mn\u0001\u0000\u0000\u0000np\u0001\u0000"+
		"\u0000\u0000oi\u0001\u0000\u0000\u0000op\u0001\u0000\u0000\u0000pz\u0001"+
		"\u0000\u0000\u0000qs\u0007\u0001\u0000\u0000rt\u0007\u0002\u0000\u0000"+
		"sr\u0001\u0000\u0000\u0000st\u0001\u0000\u0000\u0000tv\u0001\u0000\u0000"+
		"\u0000uw\u0007\u0000\u0000\u0000vu\u0001\u0000\u0000\u0000wx\u0001\u0000"+
		"\u0000\u0000xv\u0001\u0000\u0000\u0000xy\u0001\u0000\u0000\u0000y{\u0001"+
		"\u0000\u0000\u0000zq\u0001\u0000\u0000\u0000z{\u0001\u0000\u0000\u0000"+
		"{$\u0001\u0000\u0000\u0000|\u0082\u0005\"\u0000\u0000}~\u0005\\\u0000"+
		"\u0000~\u0081\t\u0000\u0000\u0000\u007f\u0081\b\u0003\u0000\u0000\u0080"+
		"}\u0001\u0000\u0000\u0000\u0080\u007f\u0001\u0000\u0000\u0000\u0081\u0084"+
		"\u0001\u0000\u0000\u0000\u0082\u0080\u0001\u0000\u0000\u0000\u0082\u0083"+
		"\u0001\u0000\u0000\u0000\u0083\u0085\u0001\u0000\u0000\u0000\u0084\u0082"+
		"\u0001\u0000\u0000\u0000\u0085\u0086\u0005\"\u0000\u0000\u0086&\u0001"+
		"\u0000\u0000\u0000\u0087\u008b\u0007\u0004\u0000\u0000\u0088\u008a\u0007"+
		"\u0005\u0000\u0000\u0089\u0088\u0001\u0000\u0000\u0000\u008a\u008d\u0001"+
		"\u0000\u0000\u0000\u008b\u0089\u0001\u0000\u0000\u0000\u008b\u008c\u0001"+
		"\u0000\u0000\u0000\u008c(\u0001\u0000\u0000\u0000\u008d\u008b\u0001\u0000"+
		"\u0000\u0000\u008e\u0090\b\u0006\u0000\u0000\u008f\u008e\u0001\u0000\u0000"+
		"\u0000\u0090\u0091\u0001\u0000\u0000\u0000\u0091\u008f\u0001\u0000\u0000"+
		"\u0000\u0091\u0092\u0001\u0000\u0000\u0000\u0092*\u0001\u0000\u0000\u0000"+
		"\u0093\u0094\u0005/\u0000\u0000\u0094\u0095\u0005/\u0000\u0000\u0095\u0099"+
		"\u0001\u0000\u0000\u0000\u0096\u0098\b\u0007\u0000\u0000\u0097\u0096\u0001"+
		"\u0000\u0000\u0000\u0098\u009b\u0001\u0000\u0000\u0000\u0099\u0097\u0001"+
		"\u0000\u0000\u0000\u0099\u009a\u0001\u0000\u0000\u0000\u009a\u009c\u0001"+
		"\u0000\u0000\u0000\u009b\u0099\u0001\u0000\u0000\u0000\u009c\u009d\u0006"+
		"\u0015\u0000\u0000\u009d,\u0001\u0000\u0000\u0000\u009e\u00a0\u0005\r"+
		"\u0000\u0000\u009f\u009e\u0001\u0000\u0000\u0000\u009f\u00a0\u0001\u0000"+
		"\u0000\u0000\u00a0\u00a1\u0001\u0000\u0000\u0000\u00a1\u00a2\u0005\n\u0000"+
		"\u0000\u00a2.\u0001\u0000\u0000\u0000\u00a3\u00a5\u0007\b\u0000\u0000"+
		"\u00a4\u00a3\u0001\u0000\u0000\u0000\u00a5\u00a6\u0001\u0000\u0000\u0000"+
		"\u00a6\u00a4\u0001\u0000\u0000\u0000\u00a6\u00a7\u0001\u0000\u0000\u0000"+
		"\u00a7\u00a8\u0001\u0000\u0000\u0000\u00a8\u00a9\u0006\u0017\u0000\u0000"+
		"\u00a90\u0001\u0000\u0000\u0000\u00aa\u00ab\t\u0000\u0000\u0000\u00ab"+
		"2\u0001\u0000\u0000\u0000\u000f\u0000bgmosxz\u0080\u0082\u008b\u0091\u0099"+
		"\u009f\u00a6\u0001\u0000\u0001\u0000";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}