lexer grammar HixLexer;

tokens {
 ERROR_TOKEN, TERMINATOR, ESCAPE, BEGIN_ARGUMENT, BEGIN_VALUE, BEGIN_VALUE_ESCAPED, BEGIN_CONTENT,
 BEGIN_VALUE_INTERPOLATE, BEGIN_VALUE_INLINE, IDENTIFIER, NAMESPACE_IDENTIFIER, VALUE_END, ROOT_IDENTIFIER,
 BEGIN_TABLE, BEGIN_TUPLE, BEGIN_LAMBDA_BLOCK, BEGIN_LAMBDA_ARROW, BEGIN_METADATA_VALUE, METADATA_PREFIX,
 FAT_ARROW, NUMBER, BOOLEAN, NULL
}

ESCAPED_AT: '@@';
ESCAPED_START: '\\' -> pushMode(ESCAPE_MODE), skip;
OUTER_WHITESPACE: Whitespace -> skip;
NEWLINE: Newline;
COMMENT: '@#' ~[\r\n]*;
SLASH_COMMENT: '//' ~[\r\n]*;

LC: '{' -> pushMode(DEFAULT_MODE);
RC: '}' -> popMode;
SEMICOLON: ';';
COMMA: ',';

TOPLEVEL_LAMBDA_BLOCK: 'func' Space* '{' -> pushMode(DEFAULT_MODE), type(BEGIN_LAMBDA_BLOCK);
TOPLEVEL_LAMBDA_ARROW: 'func' Space* '=>' -> pushMode(VALUE_MODE), type(BEGIN_LAMBDA_ARROW);
KEYWORD_FUNC: 'func';
KEYWORD_DO: 'do';
KEYWORD_EXPRESSION: 'expression';
KEYWORD_MIXIN: 'mixin';
KEYWORD_PRELUDE: 'prelude';
KEYWORD_DERIVATION: 'derivation';
KEYWORD_ELSE: 'else';

KEYWORD_RETURN: 'return';
KEYWORD_GOTO: 'goto';
KEYWORD_BREAK: 'break';
KEYWORD_CONTINUE: 'continue';

KEYWORD_TARGET: 'target';
KEYWORD_VAR: 'var';
KEYWORD_LOCAL: 'local';
KEYWORD_CARRY: 'carry';
KEYWORD_SIG: 'sig';
KEYWORD_WHEN: 'when';
KEYWORD_PURE: 'pure';
KEYWORD_INLINE: 'inline';
KEYWORD_NOINLINE: 'noinline';
KEYWORD_STRICT: 'strict';

LABEL_PREFIX: ':' -> pushMode(LABEL_IDENTIFIER_MODE);
TOPLEVEL_BOOLEAN: Boolean -> type(BOOLEAN);
TOPLEVEL_NULL: 'null' -> type(NULL);
TOPLEVEL_IDENTIFIER: Identifier -> type(IDENTIFIER);
TOPLEVEL_NAMESPACE_IDENTIFIER: NamespacedIdentifier -> type(NAMESPACE_IDENTIFIER);
TOPLEVEL_NUMBER: Number -> type(NUMBER);

ASSIGN: '=';
ARROW: '->';
FUNCTION_ARROW: '=>' -> pushMode(VALUE_MODE), type(FAT_ARROW);

TOPLEVEL_CONTENT_BLOCK: '@>' Space? -> pushMode(CONTENT_MODE), type(BEGIN_CONTENT);
TOPLEVEL_VALUE_EXPRESSION: '@=' Space* -> pushMode(VALUE_MODE);
TOPLEVEL_BEGIN_TABLE: '@{' -> pushMode(VALUE_MODE), type(BEGIN_TABLE);
TOPLEVEL_BEGIN_TUPLE: '@[' -> pushMode(VALUE_MODE), type(BEGIN_TUPLE);
EMPTY_PARAMETERS: '(' (Space | Newline)* ')';
BEGIN_PARAMETERS: '(' -> pushMode(VALUE_MODE);
TOPLEVEL_INLINE: '[' -> pushMode(VALUE_MODE), type(BEGIN_VALUE_INLINE);
TOPLEVEL_ARGUMENT: '<' -> pushMode(ARGUMENT_MODE), type(BEGIN_ARGUMENT);
TOPLEVEL_METADATA_VALUE: '%[' -> pushMode(VALUE_MODE), type(BEGIN_METADATA_VALUE);
TOPLEVEL_METADATA: '%' -> type(METADATA_PREFIX);

mode ARGUMENT_MODE;
ARGUMENT_TEXT: ArgumentText;
ARGUMENT_BEGIN_ESCAPE: '\\' -> pushMode(ESCAPE_MODE), type(ESCAPE);
ARGUMENT_BEGIN_INTERPOLATE: '{' -> pushMode(VALUE_MODE), type(BEGIN_VALUE_INTERPOLATE);
ARGUMENT_BEGIN_INLINE: '[' -> pushMode(VALUE_MODE), type(BEGIN_VALUE_INLINE);

ARGUMENT_END: '>' -> popMode;
INVALID_ARGUMENT: . -> popMode, type(ERROR_TOKEN);

mode CONTENT_MODE; // Multiline capable string blocks supporting interpolation
CONTENT_WRAP: ContinuationWrap;
CONTENT_LINEBREAK: ContinuationNewline;
CONTENT_TEXT: OperandText;

CONTENT_END: Newline -> popMode, type(TERMINATOR);
CONTENT_INTERPOLATE: '{{' -> pushMode(CONTENT_INTERPOLATE_COMPANION), pushMode(VALUE_MODE), type(BEGIN_VALUE_INTERPOLATE);

mode VALUE_MODE; //
VALUE_LAMBDA_BLOCK: 'func' Space* '{' -> pushMode(DEFAULT_MODE), type(BEGIN_LAMBDA_BLOCK);
VALUE_LAMBDA_ARROW: 'func' Space* '=>' -> type(BEGIN_LAMBDA_ARROW);
VALUE_FUNCTION: ':' -> pushMode(FUNCTION_IDENTIFIER_MODE);
VALUE_PREDICATE: ':?' -> pushMode(FUNCTION_IDENTIFIER_MODE);
VALUE_MEMBER: '#' -> pushMode(MEMBER_IDENTIFIER_MODE);
VALUE_NUMBER: Number -> type(NUMBER);
VALUE_BOOLEAN: Boolean -> type(BOOLEAN);
VALUE_NULL: 'null' -> type(NULL);

VALUE_BEGIN_ARGUMENT: '<' -> pushMode(ARGUMENT_MODE), type(BEGIN_ARGUMENT);
VALUE_END_SEMICOLON: ';' -> popMode, type(VALUE_END);
VALUE_WRAP: (ContinuationWrap | ContinuationNewline);
VALUE_DELIMITER: ',';
END_PARAMETERS: ')' -> popMode;
VALUE_END_INTERPOLATE: '}' -> popMode, type(RC);
VALUE_END_INLINE: ']' -> popMode;
VALUE_SMART_ROOT: '$';

NOT_VALUE: '!';
VALUE_CHECK: '?';

VALUE_ELVIS: '?:';

VALUE_ASSIGN: '=';
VALUE_EXPAND: '...'; // Only used for signatures to mark varargs

VALUE_BEGIN_TABLE: '@{' -> pushMode(VALUE_MODE), type(BEGIN_TABLE);
VALUE_BEGIN_TUPLE: '@[' -> pushMode(VALUE_MODE), type(BEGIN_TUPLE);

VALUE_IDENTIFIER: Identifier -> type(ROOT_IDENTIFIER);

VALUE_BEGIN_PARAMETERS: '(' -> pushMode(VALUE_MODE), type(BEGIN_PARAMETERS);
VALUE_BEGIN_INLINE: '[' -> pushMode(VALUE_MODE), type(BEGIN_VALUE_INLINE);
VALUE_METADATA_VALUE: '%[' -> pushMode(VALUE_MODE), type(BEGIN_METADATA_VALUE);

VALUE_WHITESPACE: (Whitespace | Newline) -> skip;
INVALID_VALUE: . -> popMode, type(ERROR_TOKEN);

mode ESCAPE_MODE;
ESCAPE_MACRO: [abfnrtv] -> popMode;
ESCAPE_LITERAL: ([@{}()<>] | '\\'  | Newline) -> popMode;
ESCAPE_HEX: 'x' [0-9a-fA-F][0-9a-fA-F] -> popMode;
INVALID_ESCAPE: . -> popMode, type(ERROR_TOKEN);

mode CONTENT_INTERPOLATE_COMPANION; // Helper to catch the second closing brace of the content interpolation
END_CONTENT_INTERPOLATE: '}' -> popMode;
INVALID_INTERPOLATE: . -> popMode, type(ERROR_TOKEN);

mode FUNCTION_IDENTIFIER_MODE;
FUNCTION_IDENTIFIER: Identifier -> popMode;
INVALID_FUNCTION_IDENTIFIER: . -> popMode, type(ERROR_TOKEN);

mode MEMBER_IDENTIFIER_MODE;
MEMBER_IDENTIFIER: NumberCapableIdentifier -> popMode;
INVALID_MEMBER_IDENTIFIER: . -> popMode, type(ERROR_TOKEN);

mode IDENTIFIER_MODE;
INNER_IDENTIFIER: Identifier -> popMode, type(IDENTIFIER);
INVALID_IDENTIFIER: . -> popMode, type(ERROR_TOKEN);

mode LABEL_IDENTIFIER_MODE;
LABEL_IDENTIFIER: NumberCapableIdentifier -> popMode;
INVALID_LABEL_IDENTIFIER: . -> popMode, type(ERROR_TOKEN);


// Fragments
fragment ArgumentText: ~[{}()[\]<>\\]+;
fragment OperandText: ~[@\r\n{\\]+ | '{';
fragment NamespacedIdentifier: Identifier ('.' Identifier)+;

fragment Identifier: [a-zA-Z_][a-zA-Z_0-9]*;
fragment NumberCapableIdentifier: [a-zA-Z_0-9]+;
fragment Number: '-'? [0-9]+ ('.' [0-9]+)?;
fragment Boolean: 'true' | 'false';
fragment Space: [ \t];
fragment Whitespace: Space+;
fragment Newline: ('\r' | '\n' | '\r\n');
fragment ContinuationWrap: Newline Whitespace* '@+' Space?;
fragment ContinuationNewline: Newline Whitespace* '@>' Space?;
