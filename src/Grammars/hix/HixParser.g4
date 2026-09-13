parser grammar HixParser;

options { tokenVocab=HixLexer; }

compilationUnit: trivia* fileMetadataSection? (topLevelDeclaration trivia*)* EOF;

fileMetadataSection: metadataList trivia* SECTION_DELIMITER trivia*;

topLevelDeclaration
    : metadataList trivia* (mixinDeclaration | funcDeclaration | typeDeclaration | metaDeclaration)
    | mixinDeclaration
    | funcDeclaration
    | typeDeclaration
    | metaDeclaration
    ;

metadataList: metadata (trivia* metadata)*;
inlineMetadataList: metadata+;

metadata
    : METADATA_PREFIX IDENTIFIER argumentValue* valueList?
    | metadataValue
    ;

metadataValue: BEGIN_METADATA_VALUE value VALUE_END_INLINE;

// Declarations
mixinDeclaration: mixinModifier* KEYWORD_MIXIN mixinIdentifier patternParameterList? mixinBody;

typeDeclaration: KEYWORD_TYPE IDENTIFIER ASSIGN patternExpression SEMICOLON?;

metaDeclaration: metaModifier* KEYWORD_META IDENTIFIER patternParameterList KEYWORD_FOR metaTarget (COMMA? metaTarget)* SEMICOLON;
metaTarget: IDENTIFIER | KEYWORD_TYPE | KEYWORD_MIXIN | KEYWORD_EXPRESSION;

mixinBody: LC (expressionDeclaration | funcDeclaration | trivia)* RC;

expressionDeclaration: expressionModifier* KEYWORD_EXPRESSION statementBlock;

funcDeclaration
    : funcModifier* KEYWORD_FUNC functionDeclarationIdentifier directFunctionSignature? trivia* functionBody
    ;

directFunctionSignature: (patternParameterList | collectiveInputPattern)? ARROW (patternExpression | outputSignatureFields);
collectiveInputPattern: BEGIN_VALUE_INLINE patternExpression VALUE_END_INLINE;
outputSignatureFields: patternParameterList;

functionBody: KEYWORD_DO? statementBlock | FAT_ARROW value VALUE_END? | KEYWORD_EXTERNAL IDENTIFIER? SEMICOLON?;

// Patterns
patternExpression
    : inlineMetadataList patternUnion?
    | patternUnion
    ;

patternUnion: patternTerm (PIPE patternTerm)*;
patternTerm: patternPrimary QUESTION?;

patternPrimary
    : patternIdentifier
    | tablePattern
    | tuplePattern
    | delegatePattern
    ;

tablePattern
    : BEGIN_TABLE RC
    | BEGIN_TABLE patternField (VALUE_DELIMITER patternField)* VALUE_DELIMITER? RC
    ;

tuplePattern
    : BEGIN_TUPLE VALUE_END_INLINE
    | BEGIN_TUPLE patternField (VALUE_DELIMITER patternField)* VALUE_DELIMITER? VALUE_END_INLINE
    ;

delegatePattern: KEYWORD_DELEGATE patternParameterList ARROW patternExpression;

patternParameterList
    : BEGIN_PARAMETERS patternField (VALUE_DELIMITER patternField)* VALUE_DELIMITER? END_PARAMETERS
    | BEGIN_PARAMETERS END_PARAMETERS
    | EMPTY_PARAMETERS
    ;

patternField: metadataList? patternUnion ROOT_IDENTIFIER? (VALUE_ASSIGN value)?;
patternIdentifier: IDENTIFIER | ROOT_IDENTIFIER | NULL | MISSING;

// Actual Statements
statementBlock: LC (statement | SEMICOLON | trivia)* RC;

statement
    : metadataList trivia* statementBody
    | statementBody
    ;

statementBody
    : labelIdentifier NEWLINE
    | labelIdentifier? statementBlock
    | invocationStatement
    | localDeclarationStatement
    | localAssignmentStatement
    | toplevelDerivationStatement
    | controlflowStatement
    | whenValueStatement
    | whenConditionStatement
    | whenChainStatement
    | valueExpression
    ;

// Calls
invocationStatement
    : invocationIdentifier callValueList tailValue?
    | invocationIdentifier tailValue
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

// Variables and dynamic registries
localDeclarationStatement
    : KEYWORD_CARRY? KEYWORD_LOCAL variableIdentifier assignedValue?
    | KEYWORD_CARRY? KEYWORD_LOCAL patternExpression variableIdentifier assignedValue?
    ;

localAssignmentStatement: VALUE_SMART_ROOT variableIdentifier assignedValue;

toplevelDerivationStatement
    : TOPLEVEL_DERIVATION functionIdentifier callValueList? transformationPart*
    ;

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

contentInterpolate: BEGIN_VALUE_INTERPOLATE derivation RC END_CONTENT_INTERPOLATE?;

// Values
value: nonArgumentValue | argumentValue;

valueExpression: TOPLEVEL_VALUE_EXPRESSION value VALUE_END;

nonArgumentValue
    : primaryValue transformationPart* elvisValue?
    | prefixOperators nonArgumentValue
    | nonArgumentValue postfixOperators
    ;

primaryValue
    : inlineValue
    | lambdaValue
    | inlineBlockValue
    | derivation
    | tableValue
    | tupleValue
    | valueExpression
    | valueStatement
    | NUMBER
    | STRING
    | BOOLEAN
    | NULL
    | MISSING
    ;

inlineBlockValue: BEGIN_INLINE_BLOCK (statement | SEMICOLON | trivia)* RC FAT_ARROW derivation;

lambdaValue
    : BEGIN_LAMBDA_BLOCK (statement | SEMICOLON | trivia)* RC
    | BEGIN_LAMBDA_ARROW value VALUE_END?
    ;

prefixOperators: NOT_VALUE;
postfixOperators: QUESTION | CAST_BOOLEAN;

valueStatement: functionIdentifier callValueList callBinding?;

// Explicit dispatch is used when reconstructing syntax-free bytecode.
callBinding: STATIC_CALL valueList | DYNAMIC_CALL;

tailValue
    : NEWLINE? contentBlock
    | nonArgumentValue
    ;

tupleValue
    : BEGIN_TUPLE VALUE_END_INLINE
    | BEGIN_TUPLE value (VALUE_DELIMITER value)* VALUE_DELIMITER? VALUE_END_INLINE
    ;

tableValue
    : BEGIN_TABLE RC
    | BEGIN_TABLE tableKeyedEntry (VALUE_DELIMITER tableKeyedEntry)* VALUE_DELIMITER? RC
    ;

tableKeyedEntry: metadata* (ROOT_IDENTIFIER | MISSING) VALUE_ASSIGN value;

valueList
    : BEGIN_PARAMETERS value (VALUE_DELIMITER value)* VALUE_DELIMITER? END_PARAMETERS
    | BEGIN_PARAMETERS END_PARAMETERS
    | EMPTY_PARAMETERS
    | argumentValue+
    ;

// Named arguments only exist at call sites. Keeping them out of `value` prevents
// `name = value` from competing with table fields and assignments.
callValueList
    : BEGIN_PARAMETERS callArgument (VALUE_DELIMITER callArgument)* VALUE_DELIMITER? END_PARAMETERS
    | BEGIN_PARAMETERS END_PARAMETERS
    | EMPTY_PARAMETERS
    | argumentValue+
    ;

callArgument: (ROOT_IDENTIFIER | MISSING) VALUE_ASSIGN value | value;

argumentValue: BEGIN_ARGUMENT argumentBody ARGUMENT_END;

argumentBody: (ARGUMENT_TEXT | escaped | inlineValue)*;

inlineValue: BEGIN_VALUE_INLINE value VALUE_END_INLINE;

inlineTransformation: BEGIN_VALUE_INLINE transformationPart+ VALUE_END_INLINE;

derivation: derivationRoot transformationPart*;

explicitRoot: EXPLICIT_ROOT valueList;

derivationRoot
    : explicitRoot
    | ROOT_IDENTIFIER
    | VALUE_SMART_ROOT (ROOT_IDENTIFIER | NUMBER | MISSING)
    ;

elvisValue: VALUE_ELVIS value;

transformationPart
    : functionChainType functionIdentifier callValueList? callBinding?
    | memberIdentifier
    | VALUE_WRAP
    ;

functionChainType: VALUE_FUNCTION | VALUE_PREDICATE;

// Identifiers
labelIdentifier: LABEL_PREFIX LABEL_IDENTIFIER;
memberIdentifier: VALUE_MEMBER MEMBER_IDENTIFIER;
mixinIdentifier: IDENTIFIER | NAMESPACE_IDENTIFIER | argumentValue;
functionDeclarationIdentifier: IDENTIFIER | argumentValue;
variableIdentifier: IDENTIFIER;
functionIdentifier: FUNCTION_IDENTIFIER | ROOT_IDENTIFIER;
invocationIdentifier: IDENTIFIER | KEYWORD_LOCAL;
kindIdentifier: IDENTIFIER | ROOT_IDENTIFIER | NULL | MISSING;

// Modifiers
expressionModifier: KEYWORD_PRELUDE | KEYWORD_STRICT;
mixinModifier: KEYWORD_DERIVATION;
metaModifier: KEYWORD_COMPILER | KEYWORD_BYTECODE | KEYWORD_RUNTIME;
funcModifier: KEYWORD_PURE | KEYWORD_NOINLINE | KEYWORD_INLINE;

// Common
trivia: comment | NEWLINE;
comment: COMMENT | SLASH_COMMENT;
escaped: ESCAPE (ESCAPE_MACRO | ESCAPE_LITERAL | ESCAPE_HEX);
