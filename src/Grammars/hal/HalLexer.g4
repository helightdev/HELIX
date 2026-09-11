lexer grammar HalLexer;

SECTION: '---';
PERCENT: '%';
AMP: '&';
HASH: '#';
EQUALS: '=';
COMMA: ',';
LBRACE: '{';
RBRACE: '}';
LBRACKET: '[';
RBRACKET: ']';
LPAREN: '(';
RPAREN: ')';
LANGLE: '<';
RANGLE: '>';

TRUE: 'true';
FALSE: 'false';
NULL: 'null';
NUMBER: '-'? [0-9]+ ('.' [0-9]+)? ([eE] [+-]? [0-9]+)?;
STRING: '"' ('\\' . | ~["\\\r\n])* '"';
IDENTIFIER: [A-Za-z_] [A-Za-z0-9_.-]*;
BARE: ~[ \t\r\n%&#=,{}[\]()<>"]+;
LINE_COMMENT: '//' ~[\r\n]* -> channel(HIDDEN);
NEWLINE: '\r'? '\n';
SPACE: [ \t]+ -> channel(HIDDEN);
ERROR_TOKEN: .;
