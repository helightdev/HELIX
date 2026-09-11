parser grammar HalParser;

options { tokenVocab=HalLexer; }

document: newlines sectionBlock (newlines sectionBlock)* newlines EOF;
sectionBlock: metadata* section;
metadata: PERCENT IDENTIFIER metadataArguments? newlines;
metadataArguments
  : LANGLE metadataArgument* RANGLE
  | LPAREN argumentList? RPAREN
  ;
metadataArgument: scalarAtom | COMMA | EQUALS | HASH | AMP | PERCENT;

section: SECTION IDENTIFIER (AMP scalarAtom)? lineEnd sectionEntry*;
sectionEntry: metadata* field;
field: fieldKey (EQUALS value | collectionValue) lineEnd;
fieldKey: IDENTIFIER | STRING;

value
  : IDENTIFIER typedContainer selection*
  | call selection*
  | table selection*
  | list selection*
  | looseScalar selection*
  ;
typedContainer: table | list;
table: LBRACE newlines (tableEntry ((COMMA newlines | NEWLINE+) tableEntry)* COMMA? newlines)? RBRACE;
tableEntry: metadata* fieldKey (EQUALS value | collectionValue);
list: LBRACKET newlines (value ((COMMA newlines | NEWLINE+) value)* COMMA? newlines)? RBRACKET;
collectionValue: IDENTIFIER? typedContainer;
call: IDENTIFIER LPAREN newlines argumentList? RPAREN;
argumentList: argument (COMMA newlines argument)* COMMA? newlines;
argument: IDENTIFIER EQUALS value | value;
selection: HASH IDENTIFIER;
looseScalar: scalarAtom+;
scalarAtom: STRING | NUMBER | TRUE | FALSE | NULL | IDENTIFIER | BARE;

lineEnd: (COMMA? NEWLINE+ | COMMA | EOF);
newlines: NEWLINE*;
