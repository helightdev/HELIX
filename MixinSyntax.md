# Mixin syntax examples

These examples use the unchanged root ANTLR grammars. Semantic contracts and host function requirements are defined in
[Hix.md](Hix.md). Bare names in value positions are references; angle-bracket arguments are strings.

```text
mixin QualifiedCsharpName {
  prelude expression {
    // Prelude Statements
    carry test @= target:name:replaceFirst(<,>, <>);
  }
  
  expression {
    // Statements
    doSomething(); 
  }
}
```

Invocation Statements (string arguments, parameter lists, or no parameters, with support for trailing values)
```
function();
function(arg1, arg2)
function(arg1:name, arg2)
function(<string>, arg2)
function<string1><string2>

function [InlineValue]
function <Argument>
function @> String {{interpolated}}
function() @> String {{interpolated}}
function
  @> String {{interpolated}},
  @> with multiple lines!
function() @= this:name;
function @= this:name;

function(arg1) @= this:name:unwrap;
function<str1> @= this:name:unwrap;
function<str1> @= DeriveParameters(number<2>);

function(arg1) [InlineValue]
function<str1><str2> [InlineValue]
function<str1><str2> @> Another {{interpolated:replaceAll<123><234>}} String
```

Tail Values
```
[this]
[OtherInline];
<arg>
<another>;
```

Expression Tail Values
```
@= $myVariable;
@= this:name;
@= this:name:replaceAll(<123>, <234>);
@= this:name:replaceAll<123><234>;
@= DeriveParameters(this);
@> Another {{interpolated:replaceAll<123><234>}} String

receiver
  @> String {{interpolated}},
  @> with multiple lines!
  
receiver
  @> String {{interpolated}},
  @+ with multiple lines!
```

Function Transformations
```
this:function
this:function()
this:function(arg1, arg2)
this:function<string>
this:function<string1><string2>
this:?truthfulFunction
![this:?falsyFunction]
```

Variable Declarations (Take an inline value, single function call or transform expression)
```
local test = <TailValue>
local test @= ExpressionTailValue;
local test @> Interpolated
```

Inline Tuple
```
local test = @[<a>, <b>, <c>]
```

Inline Table
```
local test = @{a=<a>, b=<b>, c=<c>}
```

An `@=` value expression ends with `;`. A content block ends with a newline; its `@>` continuations insert a newline
and its `@+` continuations join without one. `@+` continues an existing content block and is not a standalone tail.
Formatting whitespace around value expressions is not emitted text. In a content block, the initial marker and its
optional single space are removed; remaining text whitespace is preserved. Interpolation uses `{{derivation}}` in
content and `[value]` inside angle-bracket strings.

Named signatures and typed returns:

```text
pure func describe
sig @{name=string} -> string
do {
  return(<Name: [param#name]>)
}
```

Selection and fallback:

```text
mixin Example.Attribute {
  expression {
    local selected = when [true] {
      [true] -> <enabled>
      else -> <disabled>
    }
    local fallback @= null ?: <default>;
    emit<CLASS> @> // [$selected] is literal content; {{local#selected}} is interpolation
  }
}
```

The host must register `emit` and other example operations. `example.mixins` is an exploratory draft containing
placeholder names, rather than a semantically valid conformance program. In particular, `:?predicate` returns a
boolean, `!` negates a value, and `?:` selects a null fallback; there is no separate `:!` transformation operator.
