# Mixin syntax examples

These examples use the unchanged root ANTLR grammars. Semantic contracts and host function requirements are defined in
[Hix.md](Hix.md). Bare names in value positions are references; angle-bracket arguments are strings.

```text
mixin QualifiedCsharpName {
  prelude expression {
    // Prelude Statements
    carry local test @= target:name:replaceFirst(<,>, <>);
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

## Namespaces and file imports

Identifiers may contain dot-separated segments in declarations, patterns, values, metadata, and calls.
Member access continues to use `#`.

```hix
%using<path>
%using<path><p>
---
type path.Text = string;
meta docs.note(string text) for function;

%docs.note<Join two path segments>
pure func path.join(string left, string right) -> path.Text => <[$left]/[$right]>;
pure func main -> string => join(<home>, <file>);
pure func aliased -> string => p.join(<home>, <file>);
```

`%using<PREFIX>` brings a prefix into the current file. Imported function overloads are tried before
unqualified overloads, with imports tried in declaration order. If an imported overload does not match,
compilation continues with the next import and then the unqualified name. `%using<PREFIX><ALIAS>` makes
`ALIAS.name` resolve to `PREFIX.name`. Imports apply to the file that declares them, including calls in
its functions and mixins. They do not load files; use `%import` for that.

## Metadata retention and generated functions

A metadata declaration selects how long its annotations are retained:

```hix
compiler meta hint(string text) for function;
bytecode meta buildInfo(string text) for function;
runtime meta label(string text) for function;

%hint<compiler input>
%buildInfo<inspect in the compiled image>
%label<inspect after VM loading>
pure func example -> number => 1;
```

`compiler` annotations are erased when descriptors are emitted. This is the default for an unmodified
`meta` declaration; pattern annotations such as `%optional` still affect the compiled pattern.
`bytecode` annotations remain in the compiled image but are omitted from loaded VM descriptors.
`runtime` annotations remain in both. Retention modifiers are mutually exclusive, and overloads of
one metadata name must agree on retention. Retained arguments must be constant values.

Retention is supported by function and signature-field descriptors, type and pattern-field data,
file and mixin metadata, and derivation descriptors. Statements and temporary table values currently
have no reflection descriptors. VM loading preserves the original compiled image and creates loaded
descriptors containing only runtime annotations.

The compiler adds `%synthetic` to generated pattern constructors and lifted lambdas, and to anonymous
expression-function descriptors. `%synthetic` has runtime retention, so generated functions remain
identifiable after loading.

Calls must bind a concrete declared signature during compilation. Unknown signatures, incorrect
argument lists, and ambiguous overloads are compilation errors. Use declared parameter types and
explicit casts to make overload selection precise. The compiler never emits dynamic dispatch.

`cast(value, pattern)` converts to a primitive pattern or validates a declared pattern, raising an error on failure. `as(value, pattern)` performs the same conversion and returns null on failure; its inferred result is nullable.
Primitive names, including `pattern` itself and `any`, are pattern values. `null` and `missing`
remain constants. `pattern(value)` returns the primitive pattern matching the value, so
`is(string, pattern)` and `is(pattern, pattern)` both succeed. Casts take the receiver first:

```hix
pure func example(any value) -> number => $value:cast(number):plus(1);
type Record = @{string name};
pure func readName(any value) -> string => $value:cast(Record)#name;
```

Primitive-name cast invocations such as `number(value)` are removed. Type-name calls construct values
from declared fields; use `cast(value, Record)` to cast an existing value. `new(pattern)` creates a primitive
default value. Casts with a primitive or declared pattern propagate that type to subsequent calls.

`%inline` is compiler-only statement metadata. It expands the selected language function within an
invocation, initializer, return, or other annotated statement. It uses the bound overload, including
named/default arguments and generic inference. The old `inline(...)` function and `inline func`
modifier are removed. A `noinline` function cannot be expanded.

```hix
pure func echo(string value) -> string => $value;
pure func copied() -> string {
  %inline local value = echo(<hello>)
  return($value)
}
```

Runtime overload selection must be requested explicitly with `call(function, arguments...)`.
Prefer statically bound calls; use `call` when function selection depends on runtime values. Function
values can be obtained with `cast(<functionName>, function)`. Dynamic calls accept positional arguments.

Functions and types support repeatable `%alias<NAME>` metadata to export additional names. Generated structural factories also export `TypeName.new`, including aliases of the type. Casting uses the generic `cast` and `as` functions.
