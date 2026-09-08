parser grammar HixParser;

options { tokenVocab=HixLexer; }

compilationUnit: (trivia | mixinDeclaration | funcDeclaration)* EOF;

// Declarations
mixinDeclaration: mixinModifier* KEYWORD_MIXIN mixinIdentifier mixinBody;

mixinBody: LC (expressionDeclaration | funcDeclaration | trivia)* RC;

expressionDeclaration: expressionModifier* KEYWORD_EXPRESSION statementBlock;

funcDeclaration: funcModifier* KEYWORD_FUNC IDENTIFIER functionMetadata KEYWORD_DO? statementBlock;

functionMetadata: (functionSignatureVariant | NEWLINE)*;

functionSignatureVariant: KEYWORD_SIG signature ARROW signature;

// Signatures
signature: tableSignature | IDENTIFIER;

tableSignature
    : BEGIN_TABLE VALUE_END_INTERPOLATE
    | BEGIN_TABLE tableSignatureEntry (VALUE_DELIMITER tableSignatureEntry)* VALUE_END_INTERPOLATE
    ;

tableSignatureEntry: VALUE_EXPAND? ROOT_IDENTIFIER VALUE_ASSIGN ROOT_IDENTIFIER;

// Actual Statements
statementBlock: LC (statement | SEMICOLON | trivia)* RC;

statement
    : labelIdentifier NEWLINE
    | labelIdentifier? statementBlock
    | invocationStatement
    | assignmentStatement
    | controlflowStatement
    | whenValueStatement
    | whenConditionStatement
    | whenChainStatement
    ;

// Calls
invocationStatement
    : IDENTIFIER valueList tailValue?
    | IDENTIFIER tailValue
    ;


// When Conditions
whenConditionStatement: KEYWORD_WHEN whenChainCondition statementBlock (trivia* KEYWORD_ELSE whenResult NEWLINE)?;

whenElseBranch: KEYWORD_ELSE ARROW whenResult NEWLINE;

whenResult: value | statement;

whenChainCondition: valueList;

// When Chain
whenChainStatement: KEYWORD_WHEN LC NEWLINE whenChainBody RC;

whenChainBody: whenChainBranch (whenChainBranch | trivia)* whenElseBranch?;

whenChainBranch: whenChainCondition ARROW whenResult NEWLINE;

// When Value Chain (Switch-like)
whenValueStatement: KEYWORD_WHEN value? LC NEWLINE whenValueBody RC;

whenValueBody: trivia* whenValueBranch (whenValueBranch | trivia)* whenElseBranch?;

whenValueBranch: whenValueCondition ARROW whenResult NEWLINE;

whenValueCondition: value | inlineTransformation;

// Assignments
assignmentStatement: variableSpecifiers variableIdentifier assignedValue;

assignedValue
    : ASSIGN (value | whenValueStatement | whenChainStatement | invocationStatement)
    | tailValue
    ;

// Control Flow
controlflowStatement
    : KEYWORD_RETURN valueList?
    | KEYWORD_GOTO IDENTIFIER
    | KEYWORD_CONTINUE
    | KEYWORD_BREAK
    ;

// Content String
contentBlock: BEGIN_CONTENT contentBody TERMINATOR;

contentBody: (CONTENT_WRAP | CONTENT_LINEBREAK | CONTENT_TEXT | contentInterpolate)+;

contentInterpolate: BEGIN_VALUE_INTERPOLATE derivation VALUE_END_INTERPOLATE END_CONTENT_INTERPOLATE?;

// Values
value: nonArgumentValue | argumentValue;

valueExpression: TOPLEVEL_VALUE_EXPRESSION value VALUE_END;

nonArgumentValue
    : primaryValue elvisValue?
    | prefixOperators nonArgumentValue
    | nonArgumentValue postfixOperators
    ;

primaryValue
    : inlineValue
    | derivation
    | tableValue
    | tupleValue
    | valueExpression
    | valueStatement
    ;

prefixOperators: NOT_VALUE;
postfixOperators: VALUE_CHECK;

valueStatement: functionIdentifier valueList;

tailValue
    : NEWLINE? contentBlock
    | nonArgumentValue
    ;

tupleValue
    : BEGIN_TUPLE VALUE_END_INLINE
    | BEGIN_TUPLE value (VALUE_DELIMITER value)* VALUE_END_INLINE
    ;

tableValue
    : BEGIN_TABLE VALUE_END_INTERPOLATE
    | BEGIN_TABLE tableKeyedEntry (VALUE_DELIMITER tableKeyedEntry)* VALUE_END_INTERPOLATE
    ;

tableKeyedEntry: ROOT_IDENTIFIER VALUE_ASSIGN value;

valueList
    : BEGIN_PARAMETERS value (VALUE_DELIMITER value)* END_PARAMETERS
    | BEGIN_PARAMETERS END_PARAMETERS
    | EMPTY_PARAMETERS
    | argumentValue+
    ;

argumentValue: BEGIN_ARGUMENT argumentBody ARGUMENT_END;

argumentBody: (ARGUMENT_TEXT | escaped | inlineValue)*;

inlineValue: BEGIN_VALUE_INLINE value VALUE_END_INLINE;

inlineTransformation: BEGIN_VALUE_INLINE transformationPart+ VALUE_END_INLINE;

derivation: derivationRoot transformationPart*;

derivationRoot : VALUE_SMART_ROOT? ROOT_IDENTIFIER;

elvisValue: VALUE_ELVIS value;

transformationPart
    : functionChainType functionIdentifier valueList?
    | memberIdentifier
    | VALUE_WRAP
    ;

functionChainType: VALUE_FUNCTION | VALUE_PREDICATE;

// Identifiers
labelIdentifier: LABEL_PREFIX LABEL_IDENTIFIER;
memberIdentifier: VALUE_MEMBER MEMBER_IDENTIFIER;
mixinIdentifier: IDENTIFIER | NAMESPACE_IDENTIFIER;
variableIdentifier: IDENTIFIER;
functionIdentifier: FUNCTION_IDENTIFIER | ROOT_IDENTIFIER;

// Modifiers
expressionModifier: KEYWORD_PRELUDE | KEYWORD_STRICT;
mixinModifier: KEYWORD_DERIVATION;
variableSpecifiers: KEYWORD_CARRY | KEYWORD_LOCAL | (KEYWORD_TARGET? KEYWORD_VAR);
funcModifier: KEYWORD_PURE | KEYWORD_NOINLINE | KEYWORD_INLINE;

// Common
trivia: comment | NEWLINE;
comment: COMMENT | SLASH_COMMENT;
escaped: ESCAPE (ESCAPE_MACRO | ESCAPE_LITERAL | ESCAPE_HEX);
