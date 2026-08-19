parser grammar MixinExpressionParser;

content: statement* EOF;

statement: directive expression?;

directive: DIRECTIVE_NAME DIRECTIVE_ARGUMENT*;

expression: (text | enclosedValue | value)+;

value: valueName valueChainPart*;

enclosedValue: BEGIN_ENCLOSED_VALUE valueName valueChainPart* END_ENCLOSED_VALUE;

valueChainPart
    : functionBody
    | VALUE_PATH
    ;

functionBody: functionCallType FUNC_NAME functionArgument*;

functionCallType
    : INVOKE_NEGATED_PREDICATE | ENC_INVOKE_NEGATED_PREDICATE
    | INVOKE_PREDICATE | ENC_INVOKE_PREDICATE
    | INVOKE_FUNC | ENC_INVOKE_FUNC
    ;

functionArgument: VALUE_ARGUMENT | ENC_VALUE_ARGUMENT;
valueName: VALUE_NAME | ENC_VALUE_NAME;
text: STRING_TEXT_VALUE | ENC_STRING_TEXT_VALUE | STRING_TEXT;

