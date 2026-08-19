lexer grammar MixinExpressionLexer;

ESCAPED_AT: '@@';
DIRECTIVE_NAME: '@' [A-Za-z0-9_]* -> pushMode(DIRECTIVE_MODE);
WHITESPACE: (NewLine | Whitespace) -> skip;

mode DIRECTIVE_MODE;

DIRECTIVE_ARGUMENT: '<' NonArgument* '>';
DIRECTIVE_TAIL: Whitespace -> popMode, pushMode(STRING_EXPR_MODE), skip;
DIRECTIVE_TAIL_NEWLINE: NewLine -> popMode, skip;

mode STRING_EXPR_MODE;

STRING_TEXT: NonMacro+;
TERMINATE_STRING_EXPR: NewLine -> popMode, skip;
BEGIN_ENCLOSED_VALUE: '@(' -> pushMode(VALUE_MODE_ENCLOSED_NAME);
VALUE_NAME: '@' [A-Za-z0-9_]*  -> pushMode(VALUE_MODE);

mode VALUE_MODE;

INVOKE_NEGATED_PREDICATE: ':!?' -> pushMode(FUNCTION_MODE);
INVOKE_PREDICATE: ':?' -> pushMode(FUNCTION_MODE);
INVOKE_FUNC: ':' -> pushMode(FUNCTION_MODE);
VALUE_ARGUMENT: '<' NonArgument* '>';
VALUE_PATH: '#' [A-Za-z0-9_]*;
STRING_TEXT_VALUE: ~[:#<@\r\n]* -> popMode;
VALUE_TAIL_NEWLINE: NewLine -> popMode, popMode, skip;

mode FUNCTION_MODE;
FUNC_NAME: [A-Za-z0-9_]* -> popMode;

mode VALUE_MODE_ENCLOSED_NAME;
ENC_VALUE_NAME: [A-Za-z0-9_]* -> popMode, pushMode(VALUE_MODE_ENCLOSED);

mode VALUE_MODE_ENCLOSED;
ENC_VALUE_ARGUMENT: '<' NonArgument* '>';
ENC_VALUE_PATH: '#' [A-Za-z0-9_]*;
ENC_INVOKE_NEGATED_PREDICATE: ':!?' -> pushMode(FUNCTION_MODE);
ENC_INVOKE_PREDICATE: ':?' -> pushMode(FUNCTION_MODE);
ENC_INVOKE_FUNC: ':' -> pushMode(FUNCTION_MODE);
ENC_STRING_TEXT_VALUE: ~[:#<)\r\n]* -> popMode;

END_ENCLOSED_VALUE: ')' -> popMode;
ENC_VALUE_TAIL_NEWLINE: NewLine -> popMode, popMode, skip;

// Fragments
fragment Identifier: [A-Za-z_] [A-Za-z0-9_]*;
fragment NonMacro: '@@' | ~[@\r\n];
fragment NonArgument: ~[>\r\n];
fragment Whitespace: [ \t]+;
fragment NewLine: [\r\n];