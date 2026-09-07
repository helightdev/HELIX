# Mixins

The language's primary use case is the generation of source code or text based on structured semantic input and data,
making it fit in the same niche as common template engines. However, the language is designed to be significantly more
powerful in logic expressiveness.

## Status and authority

This document specifies the intended language built on [MixinLexer.g4](MixinLexer.g4) and
[MixinParser.g4](MixinParser.g4). Those grammars define the accepted syntax; the rules below define binding,
validation, evaluation, and host integration after parsing. A syntactically valid program can still be semantically
invalid. [MixinSyntax.md](MixinSyntax.md) provides examples of the grammar's notation.

This is a specification draft, not a claim that the current compiler implements it. The current implementation in
`src/MixinLanguage/Language` still implements the earlier directive language. Its capabilities are recorded in the
host sections below. This is a breaking refactor: retain capabilities, not compatibility aliases, obsolete argument
shapes, or helper methods superseded by the new syntax. New core semantics take precedence over earlier implementation
details, particularly function-local storage, tuples, kinds, checked errors, and argument binding.

Names are case-sensitive unless a host operation explicitly documents otherwise. Keywords, storage roots, and kind
constants retain their specified meaning; an unknown bare root is a binding error, not an implicit string literal.
Text arguments use `<text>`. No new tokens or parser productions are required by the semantic rules in this document.

The following previously unspecified choices are proposed semantics for review: `strict` disables implicit hoisting;
`break`/`continue` exit/restart the enclosing block; named signatures bind a parameter table; core equality is structural
and case-sensitive; checked callee failures roll back their effects. These choices make the draft implementable, but
are not inferred guarantees of the grammar or existing runtime. The numeric comparison tolerance is likewise an
explicit draft policy rather than a value recovered from the implementation.

## Execution Model

The language is designed to be JIT-compiled into an intermediate representation together with a prepared virtual machine
state. Immutable prepared code and constants may be shared between execution entrypoints until their source or library
dependencies change. Mutable execution state must be private to an execution; it is not globally shared.

Execution is generally performed on a per-target basis, usually triggered by entrypoint marker annotation/attributes in
the targeted host language. After a target has been discovered, execution follows a two-phase model with a final emit
phase:

- Prelude Pass: The prelude pass is responsible for performing semantic analysis of the input and has access
  to the analyzer of the host language, for C# this will be the Roslyn analyzer. The prelude can then store information
  derived from the semantic input into carried storage for the later expressions to consume.

- (VM Sided) Prelude Closing Pass: The prelude outputs are collected and detached (see Value Detaching) and stored into
  cacheable VM snapshots.

- Expression Pass: The main pass of the system running on pure input data to deterministically generate derived output.

- Emit Pass: The host introspects the final expression output and derives text content and actions based on the output.
  This usually includes rendering values into text, dereferencing interned string constants and combining accumulated
  buffers into the final output.

### Value Detaching

Mixins generally allow host types to be used as values in the mixin language. However, dependency graph tracking source
generators can't properly cache intermediate state well if you store those references in pass outputs, which will lead
to many mixins being re-evaluated for compilation unit changes, causing slower compilation times and possibly lagging
source generator integrated editors.

The prelude is designed to allow for semantic preprocessing with full compilation context while offering the ability to
delay heavier computations and text generation to the later expression and emission phases using carried variables.

Carried variables are generally normal storage containers. After the prelude pass, those variables are inspected to see
if they still contain references to host types. If they do, they are 'detached' into mixin types or detached pure host
types. If this can't be done, an error is usually raised to the host.

## Builtin Types

- Table (key-value data)
- Tuple (ordered positional values)
- String
- Number
- Boolean
- Error
- Null
- Symbol
- Function
- Kind

## Compiler

### Hoisting

Writing prelude passes can be very verbose and unreadable, which is why the language allows for automatic hoisting of
expressions into the prelude pass, automatically generating required synthetic carry variables and the necessary prelude
statements to populate them.

Hoisting is dependent on the host as it has to define which value roots and functions return non-durable data. A dump of
the generated pseudo-assembly can help with identifying hoisting issues. Besides that, code can manually be moved to the
prelude pass using prelude expressions.

As hoisting is inherently local and target-dependent, it can't be done across non-inlined invocations of functions. Functions
used in the expression pass must either be marked as pure or be inlined to guarantee deterministic runtime behavior. By
default, all functions are considered impure but inlinable.

Follow for function calls in expressions:

- Pure: Call as is without hoisting
- Inlineable: Inline the function body into the expression and perform normal hoisting
- Non-Inlineable: Throw an exception. The user has to manually move the function call into the prelude pass.

Note: Values and their transformation defined in carry assignments are fully hoisted into the prelude pass.

### Storage Spaces

- var: Scoped to the execution target including all member-applied mixins
- target var, tar: Scoped to the current mixin target (i.e. a specific member like the class itself, a method, etc.)
- local: Scoped to the current call-frame.
- carry: Write-Only in prelude, Read-Only in expression, scoped only to the current persistent call-site.
- param: Available for function invocations, holding the input arguments for the function.

Storages can be referred to by their scope name with member access, for example `local#user` or `tar#user`. They can be
also resolved by using the smart variable syntax, like `$user`. This will follow a fine-to-coarse resolution
order [local, param, tar, var] (carry is excluded)

#### C#-specific Storage and References

- target: The mixin target (Class, Struct, Function, Method, Property, Field, ...) (call inherited)
- attr: The attribute instance applying this mixin (call inherited)
- this: The execution scope in which the mixins are applied (Class or Struct)

Members:

- target: Method Parameters, none
- attr: Attribute Arguments
- this: Class Members, Struct Members (Fields, Properties, Methods, etc. by name)

Note: Those storage spaces are generally read-only and only accessible in the prelude pass.

## Functions

Functions can be defined in mixins or compilation units, the first being local to the current mixin, avoiding name
collisions. Functions can have the following modifiers:

- pure: The function doesn't have to hoist and can be called in expressions
- noinline: The function cannot be inlined and must always be called as a function call.

Functions receive one bound parameter value through `param`. Without signature metadata, the runtime joins multiple
specified arguments (including the tail value) into a tuple. Named signatures instead bind a parameter table as
specified below. Transformation chains use the receiver as the first logical argument. Returning no value is equivalent
to returning a null value, while errors are represented by returning an error value.

Functions are referenced by name from their declaration scope.
- `call(function)` : Calls the function with no parameters.
- `call(function, any)` : Calls the function with the value as the parameter.
- `inline(function)` : Inlines the function with no parameters.
- `inline(function, any)` : Inlines the function with the value as the parameter.

Values:
- `function`: function kind.
- `function()`: Returns a function that ignores all parameters and returns null.
- `function(any)`: Returns the function passed into it or tries resolving strings into function references.

## Kind Type System

Types in the language are generally dynamic and very basic. Types have a set of identity functions that
make it usable as a type. The language intentionally uses uncommon naming for the type system, to avoid confusion
with host language interacting functions.

Kinds are treated as global values, similar to storage roots and host constants. All kinds must also have a type
function of the same name.

Kinds are always atoms, being a single unique value. Pure symbols may therefore be expressed as kinds, like null.

- `kind(any): kind`: Returns the mixin type kind of the value.
- `new(kind): any`: Invokes the identity function of the kind without arguments.
- `new(kind, any): any`: Invokes the identity function of the kind with the value as an argument.
- `as(any, kind): any`: Tries converting the value into the specified kind.
- `is(any, kind): bool`: Returns true if the value is of the specified kind.
- `if(any, kind, defaultValue): any`: Returns the value if it is of the specified kind, otherwise returns the
  defaultValue.
- `ifNot(any, kind, defaultValue): any`: Returns the value if it is not of the specified kind, otherwise returns the
  defaultValue.

Values:
- `kind`: The constant kind-kind value.

## Booleans
- `bool(any): bool`: Returns true if the value is truthy, otherwise returns false.
- `not(bool): bool`: Returns the negation of the boolean value.
- `not(any): bool`: Returns true if the value is falsy, otherwise returns false.
- `eq(any, any)`: Returns true if the two values are equal.
- `neq(any, any)`: Returns true if the two values are not equal.
- `exists(any): bool`: Returns false for null or a checked error, true otherwise, including false and empty values.
- `and(bool, bool, ...bool): bool`: Returns the conjunction of at least two logical arguments.
- `or(bool, bool, ...bool): bool`: Returns the disjunction of at least two logical arguments.

Values:
- `bool`: boolean kind.
- `true`: The boolean true constant.
- `false`: The boolean false constant.
- `bool()`: Returns a new boolean value (false)
- `bool(any)`: Returns the boolean value constant based on the truthiness of the value.

## Strings
- `length(string)`: Returns the number of characters in the string.
- `contains(string, string)`: Returns true if the string contains the substring. 
- `startsWith(string, string)`: Returns true if the string starts with the substring. 
- `endsWith(string, string)`: Returns true if the string ends with the substring. 
- `replaceAll(string, string, string)`: Returns a new string with all occurrences of the substring replaced. 
- `replaceFirst(string, string, string)`: Returns a new string with the first occurrence of the substring replaced. 
- `replaceLast(string, string, string)`: Returns a new string with the last occurrence of the substring replaced. 
- `substring(string, number, number)`: Returns a new string with the substring from the start index to the end index. 
- `uppercase(string)`: Returns a new string with all characters converted to uppercase.
- `lowercase(string)`: Returns a new string with all characters converted to lowercase.
- `trim(string)`: Returns a new string with all leading and trailing whitespace removed. 
- `trimStart(string)`: Returns a new string with all leading whitespace removed. 
- `trimEnd(string)`: Returns a new string with all trailing whitespace removed. 
- `split(string, string)`: Returns a tuple of strings split by the substring.
- `matches(string, pattern: string): bool`: Tests a regular expression using the C# host's .NET regex semantics.
- `regexReplaceAll(string, pattern: string, replacement: string): string`: Replaces all regex matches, supporting
  replacement-group expansion.
- `regexReplaceFirst(string, pattern: string, replacement: string): string`: Replaces the first regex match.

Regex syntax errors return errors. Regex operations must have a bounded execution time and report timeout failures.

Text construction uses the language's interpolation syntax: `[value]` inside angle-bracket strings and
`{{derivation}}` inside content blocks. No separate `format` function or placeholder language is provided.

Values:
- `string`: The string kind.
- `string()`: Returns a new empty string.
- `string(any)`: Returns the string representation of the value.

## Numbers

Numbers are generally represented as double-precision floating-point values.
The language does not have a dedicated numeric literal syntax, relying on having the number either provided or parsed
from a string. Example: `number(<42>)` or `number<42>`. `int` is not a separate core kind.

- `isInt(number)`: Returns true if the value is finite and has no fractional part.
- `round(number)`: Returns the number rounded to the nearest integer.
- `floor(number)`: Returns the number rounded down to the nearest integer.
- `ceil(number)`: Returns the number rounded up to the nearest integer.
- `abs(number)`: Returns the absolute value of the number.
- `min(number, number)`: Returns the minimum of the two numbers.
- `max(number, number)`: Returns the maximum of the two numbers.
- `clamp(number, number, number)`: Returns the number clamped between the two numbers.
- `pow(number, number)`: Returns the number raised to the power of the second number.
- `sqrt(number)`: Returns the square root of the number.
- `log(number, number)`: Returns the logarithm of the first number with respect to the second number.
- `minus(number, number)`: Returns the difference of the two numbers.
- `plus(number, number)`: Returns the sum of the two numbers.
- `mult(number, number)`: Returns the product of the two numbers.
- `div(number, number)`: Returns the quotient of the two numbers.
- `mod(number, number)`: Returns the remainder of the division of the two numbers.
- `compare(number, string, number)`:
  * The operation is one of `lt`, `gt`, `lte`, `gte`, `eq`, `neq`.
  * equality comparisons are done with shared tolerance

- `number`: The number kind.
- `number()`: Returns a new number value (0)
- `number(any)`: Returns the number representation of the value, possibly parsing it.

## Errors

Errors are special sentinel values that participate in the normal flow of the language as a data type when checked or used
in a checked context. Normally, if a function returns an error value, the scope will fail and propagate until a caller
does handle the error value. Errors can be handled by using the `?` check operator at the end, which will return the
error as an error value instead of forcing propagation.

Errors are not handled as null values and therefore are not catchable using elvis operators; however, they behave as
falsy when forced into a boolean context.

The following intrinsic functions are available for error handling:

- `catch(any)`: Returns the value if it is not an error, otherwise returns null.
- `catch(any, function)`: Returns the value if it is not an error, otherwise calls the function with the error as the
  parameter and returns its result.

Values:
- `error`: error kind.
- `error()`: Returns a new unspecified error value.
- `error(any)`: Returns an error value with the specified value as the message.

"Throwing" exceptions can be done by returning the error value.

## Null Values

Null values are used to represent the absence of a value. They can be handled using the `?:` elvis operator. Returning
no value in a function is equivalent to returning a null value. The elvis operator selects a fallback value rather than
introducing a null-safe transformation chain. Null values behave as falsy when forced into a boolean
context. Host null values are bidirectionally convertible to mixin null values.

Values:

- `null`: null kind, also used as a symbolic constant for null values.
- `null()`: Returns the null constant kind.
- `null(any)`: Returns the null constant kind.

## Tables

Tables are string-keyed associative arrays that can be used to store arbitrary data.

- `length(table)`: Returns the number of key-value pairs in the table.
- `keys(table)`: Returns a tuple of all keys in the table.
- `values(table)`: Returns a tuple of all values in the table.
- `entries(table)`: Returns a tuple of all key-value pairs in the table as tuples.
- `put(table, key, value)`: Returns a new table with the key-value pair set.
- `put(table, table)`: Returns a first table overridden by the second table's key-value pairs.
- `remove(table, string)`: Returns a new table with the key removed.
- `contains(table, value)`: Returns true if the table contains the value. (Common Intrinsic)
- `get(table, string)`: Returns the value for the key in the table or null if not found. (Member Operator)
- `get(table, string, default)`: Returns the value for the key in the table or the default value if not found.
- `has(table, string)`: Returns true if the table contains the key. (Common Intrinsic)
- `where(table, function)`: Returns a new table with only the key-value pairs that pass the test function.
- `map(table, function)`: Returns a new table with the values transformed by the function.
- `reduce(table, function, initial)`: Returns a single value by applying the function to each key-value pair in the table,
  starting with the initial value.
- `join(table, string, string)`: Returns a new string based on the table entries.
  The first string is used as a key-value separator, the second string is used as an entry separator.


Values:
- `table`: table kind.
- `table()`: Returns a new empty table.
- `table(tuple)`: Cast implementation for tables, expecting a tuple of key-value pairs as tuples.
- `table(any)`: Tries casting the value into a table, otherwise returning an empty table. This will, by default, succeed
  for tuples and tables.

## Tuples

Tuples are ordered collections of values. Like most data structures in the language, they are immutable.

- `length(tuple)`: Returns the number of values in the tuple.
- `push(tuple, any): tuple`: Returns a tuple with the value appended as one element, without spreading nested tuples.
- `pop(tuple): tuple`: Returns a tuple without its last element; an empty tuple remains empty.
- `contains(tuple, any)`: Returns true if the tuple contains the value. (Common Intrinsic)
- `get(tuple, number)`: Returns the value at the index in the tuple or null if not found. (Member Operator)
- `get(tuple, number, default)`: Returns the value at the index in the tuple or the default value if not found.
- `has(tuple, number)`: Returns true if the tuple contains the index. (Common Intrinsic)
- `indexOf(tuple, any)`: Returns the index of the value in the tuple or -1 if not found.
- `where(tuple, function)`: Returns a new tuple with only the values that pass the test function.
- `any(tuple, function)`: Returns true if any value in the tuple passes the test function.
- `all(tuple, function)`: Returns true if all values in the tuple pass the test function.
- `map(tuple, function)`: Returns a new tuple with the values transformed by the function.
- `reduce(tuple, function, initial)`: Returns a single value by applying the function to each value in the tuple,
  starting with the initial value.
- `join(tuple, string)`: Returns a new string with the tuple of strings joined by the substring.

Values:
- `tuple`: tuple kind.
- `tuple()`: Creates a new empty tuple.
- `tuple(table)`: Cast implementation for tuples, equivalent to calling `entries` on the table.
- `tuple(any)`: Tries casting the value into a tuple. Null will return an empty tuple while other values will be wrapped
  into a single-value tuple.

## Declarations and binding

### Symbols

`symbol` is the kind of host semantic references and their supported detached representations. It is distinct from
an ordinary string containing a symbol's name. `symbol()` returns null because no host symbol can be manufactured
without context; `symbol(value)` preserves a symbol or asks the host to resolve the supplied value during the prelude,
returning an error on failure. Resolution must specify its host context and diagnose ambiguous names. Pure symbolic
atoms that require no host identity can instead be represented by registered kind atoms, as described above.

### Declaration ownership

A compilation unit contains mixin declarations, function declarations, and trivia. A mixin contains expression blocks
and functions. Function declarations are collected before executable blocks are bound, allowing forward references.
Functions cannot be declared inside statement blocks under the supplied grammar.

`mixin Namespace.AttributeName { ... }` associates behavior with a host identity. `derivation mixin Name { ... }`
declares a derivation provider. The host resolves qualified mixin identities and library imports; these are not value
roots. Duplicate providers and conflicting visible function signatures are diagnostics. Repeated imports of the same
library are deduplicated without changing order.

Function lookup searches the enclosing mixin, then the compilation unit and its explicitly imported libraries, then
builtins. A local function shadows the same visible signature, rather than depending on import enumeration order.
Function references retain their declaration's environment when passed as callbacks. They do not capture mutable
caller locals. Recursive calls require a non-inline call path; recursive inline expansion is a compile-time error.

`pure` promises deterministic computation from explicit parameters and immutable constants. A pure function cannot
inspect live host state, mutate shared storage, emit output, or call an impure function. Validate this transitively.
`inline` requires expansion at each call; `noinline` forbids expansion. Combining them is invalid. `pure noinline` is
valid. Inlining preserves parameter evaluation count, private locals, labels, typed return values, and early returns.
`strict expression` disables automatic hoisting: late host-dependent operations must instead use explicitly prepared
carry data. `prelude strict expression` is redundant and has the same behavior as a prelude expression. Duplicate
modifiers are diagnostics.

### Signatures and parameter packing

The grammar's `sig input -> output` entries describe overloads of one function body. A bare kind describes one value;
`any` is the unconstrained signature kind. A table signature describes named fields and their kinds. In an input
signature its source field order also defines positional argument binding. An empty table signature accepts no
arguments. At most one input field may have `...`; it must be last and collects a tuple of remaining arguments whose
elements match its declared kind. Expansion is signature metadata, not value-level tuple spreading. Output signatures
describe the returned kind or required table fields; variadic output fields are invalid.

```text
pure func pairName
sig @{first=string, second=string} -> string
do {
  return(<[param#first] [param#second]>)
}
```

At a call, evaluate the receiver, explicit arguments, and optional tail once, in that order. A transformation receiver
is the first logical argument; a tail is the last. Select the signature against these logical arguments before packing
them. Fixed arities take precedence over variadic candidates; an exact kind match takes precedence over `any`.
Unresolved ties, wrong arity, and incompatible kinds are errors. Do not silently choose the first registration or
convert values merely to make an overload fit. A dynamically typed call performs the unresolved checks at runtime.

For functions without signature metadata, zero arguments bind `param` to null, one binds it to that value, and multiple
arguments bind it to a tuple. An explicit tuple remains one argument and is never implicitly spread. For a named
input signature, `param` is the bound field table; `$first` can therefore resolve `param#first`. A single table argument
may satisfy that table signature directly when its fields match. Bare signatures leave `param` as the supplied value.
Return signatures are checked before returning to the caller. Bare `return` and reaching the function end produce null;
`return(a, b)` produces a tuple. A value-required host operation may additionally require an explicit value-bearing
return, distinguishing `return(null)` from a valueless return.

`call(f, value)` passes exactly one value to the referenced function. `inline(f, value)` has identical value semantics
but requires a statically resolvable function. Ordinary direct calls and transformation calls use the same signatures.
Builtin signatures in this document count the receiver as an argument; the earlier runtime's metadata counts chain
arguments separately. An implementation must normalize that difference once at binding.

## Value evaluation

`[value]` groups a typed value and does not render it. `@[...]` creates a tuple; `@{key=value,...}` creates a table.
`<...>` always constructs a string, including when its body contains an inline `[value]`. Content interpolation also
renders its value as text. Argument-list parentheses delimit arguments, not arbitrary expression grouping.

Derivation transformations execute from left to right. `#name` selects a literal member name; `#0` selects tuple index
zero or the table key `0`. Computed keys use `get(value, key)`. Missing members return null; `has` distinguishes absence
from a present null. Unsupported selection on a non-container is an error unless the host supplies a selector.

`value:function(args)` calls a transformation. `value:?predicate(args)` calls the predicate form and returns a boolean,
not the original receiver. Negation uses `!` on the resulting value, for example `![target:?static]`. The grammar's
`?:` is an elvis expression, not a distinct null-safe function-chain token: `a ?: b` evaluates `b` only if `a` is null.
To conditionally transform a non-null receiver, use `when` or a callback helper. An error does not trigger elvis.
Use brackets to express intended grouping of prefix operators, postfix checks, elvis, and transformations; precedence
and attachment follow the supplied parse tree.

The postfix `?` checks the complete operand it attaches to: failures during evaluation of that operand become an
ordinary error value. For example, `catch([dangerous()]?)` can inspect the failure; `catch(dangerous())` cannot receive
an unchecked failure that already propagated during argument evaluation. A checked value remains inspectable by
`kind`, `is`, `catch`, and boolean conversion. Subsequent operations that cannot accept errors propagate it again.
Only the selected `when` branch and selected elvis fallback execute. Ordinary function arguments are eager; functions
needing delayed execution must take callbacks or be explicitly specified as intrinsics.

### Equality, truth, numbers, and rendering

Null, false, numeric zero, empty strings, empty tuples, empty tables, and checked errors are falsy; other values are
truthy. An unchecked error propagates before conversion. `eq` compares kinds and values without implicit string
conversion: strings use ordinal case-sensitive comparison, tuples compare elements in order, and tables compare keys
and values independently of insertion order. Function and kind values compare by identity. Host symbols use the host's
semantic identity while live and a stable detached identity afterward. `neq` is the complement of `eq`.

Numbers use IEEE-754 double precision. Parsing and formatting use invariant culture. Invalid conversions return errors;
division or remainder by zero and non-finite arithmetic results return errors. `round` resolves halfway cases to even;
`substring` uses a start-inclusive, end-exclusive range and errors on invalid bounds. String indices and lengths count
UTF-16 code units for the C# host. Case conversion is invariant. Core replacements treat patterns literally; regex
matching and replacement are separate host/library operations. Empty replacement patterns are errors.

Numeric `eq` is exact. The explicitly approximate `compare` uses tolerance
`abs(a-b) <= 1e-9 * max(1, abs(a), abs(b))`; `eq`/`neq` there use that test, `lt`/`gt` exclude approximately equal values,
and `lte`/`gte` include them. This tolerance does not affect keys, indexing, kind checks, or ordinary equality.

Text rendering maps null to empty text, booleans to `true`/`false`, and numbers to invariant round-trip text. Symbols
use host-defined display text. Collections require explicit `join` or a host serializer for emission; do not silently
flatten them. Error messages and function names may be rendered explicitly for diagnostics. Identity functions use
their documented zero-argument defaults; failed conversions produce errors except for the explicitly documented
table/tuple fallback conversions. `kind` is the kind of kind atoms, and `new` delegates to the selected kind's identity
function. `null` is both its sole value and its kind atom. Host type inspection uses `type` and is distinct from `kind`.

### Collection contracts

Tables retain insertion order for enumeration, callbacks, and rendering. Replacing an existing key retains its place;
new keys append. Duplicate literal keys are semantic errors. Tuples are zero-indexed. Numeric tuple indices must be
finite, integral, and non-negative; an out-of-range index is absent.
All collection updates return new values and preserve the original, including nested values.

Tuple callbacks receive each element. Table `map` and `where` callbacks receive `@{key=..., value=...}`; `map` replaces
the entry's value while retaining its key. Reducers receive `@{acc=..., value=...}` for tuples and additionally `key`
for tables. A reducer's explicit return becomes the next accumulator. Empty reduction returns its initial value;
`any` on an empty tuple is false and `all` is true. Callbacks execute once per visited entry in order. `any` and `all`
stop once the result is known. Callback errors fail the operation; no partial collection is returned.

Tables represent keyed records and dictionaries; tuples represent sequences. Collection updates require explicit
assignment, for example `local items @= local#items:push(<next>);` where `items` is a tuple. Table key/value joins use
`keys` or `values` followed by tuple `join`. There are no alternate callback shapes or table-as-list operations.

## Statements, blocks, and control flow

Statements execute in source order. Invocation statements discard successful return values; assignments retain them
without rendering. A storage assignment replaces the named binding in its specified storage. Blocks group control
flow but do not create new function frames: `local` remains scoped to the current invocation. Names are resolved by
presence, so a local containing null shadows a parameter or outer variable. Assignment never falls through the smart
lookup order; `$name` is a read convenience, not an assignment target.

`when(condition, ...) { ... }` evaluates conditions left to right as a short-circuit conjunction. An empty condition
list is true. The `else` result runs only when the conjunction is false. A condition-chain `when { (conditions) -> ... }`
tests branches in order and executes only the first match. A value-chain `when value { ... }` evaluates its selector
once; value branches use `eq`, and inline transformation branches apply to the selector and test truthiness. With no
selector, value branches themselves are tested for truthiness. `else` is the final fallback. No match produces null.

When used by an assignment, a selected value result or invocation supplies the assigned value. A selected block
normally produces null; assignments inside it do not implicitly return their values. `return`, `goto`, `break`, and
`continue` retain their control effects inside branches and are not converted into assigned values. In particular,
`return` inside a branch returns from the enclosing function or expression, not merely from `when`.

A label identifies a location or a labeled block in its containing function/expression. Duplicate labels in that
boundary are invalid. `goto name` may jump backward to form a loop, but cannot cross a function, expression, or phase
boundary, or enter a nested block from outside. `break` exits the nearest explicit statement block; `continue` restarts
that block. At an expression's outer block they respectively finish or restart the expression; a function's outer
block behaves analogously, with `break` returning null. Every backward jump and restart consumes the execution budget.
Implementations must bound instruction count and call depth and diagnose budget exhaustion.

`match(condition, ...)` is a scope guard: a false conjunction exits the current block. `match()` succeeds. `assert` uses
the same condition convention but false fails the expression with an error. These are control intrinsics resolved as
invocations, not additional grammar keywords. `fail(message)` always fails. This supplies the former scope/match/skip
behavior using blocks, guards, and jumps.

The complete control-intrinsic signatures are `match()`, `match(bool, ...bool)`, `assert()`,
`assert(bool, ...bool)`, `fail()`, and `fail(any)`. Zero-argument guards/assertions succeed; `fail()` uses a default
message. All successful guard/assertion calls return null. Conditional jumps use `when` and `goto`; block exits use
`break`. No separate labeled guard or skip method is provided.

## Phase boundaries, storage, and transactions

Prelude blocks run in declaration order for each application before its expression blocks. Expression blocks also run
in declaration order. Function declarations do not execute by being declared. The host supplies a stable order of
applications across targets; `var` sharing makes that order observable. `target var` writes `tar`; the host root
`target` remains a read-only symbol. `param` is read-only and replaced/restored on each call. Functions have fresh
locals; inlining must simulate this isolation. Values intentionally shared between calls use `var` or `tar`.

Only carry data and explicitly snapshotted durable shared state cross the prelude boundary. Ordinary prelude locals
are unavailable in late expressions. A carry slot is identified by application, expression, persistent call site,
and declaration, so equal names in separate invocations do not collide. Every late read must have a dominating
prelude write on the corresponding path; report reads of uninitialized slots. A carry assignment written in a late
expression requests hoisting of its entire dependency slice. The prelude cannot read carry slots back as ordinary
storage; use a local to compute shared intermediate results before storing carries.

Hoisting preserves dependency order, conditional execution, evaluation count, and the order of observable writes and
output. Host-independent arguments may move only when all their dependencies are available. A host operation depending
on a value computed only during late execution is an error, not permission to read Roslyn in the late VM. Do not
speculatively run operations from untaken branches. Indirect calls need known purity or explicit prelude placement.
If a transformation cannot preserve these properties, diagnose it and suggest an explicit prelude computation.

Detachment recursively visits the exact values reachable from carried state, including collection elements and
callback captures. It snapshots only requested durable host data, not every possible semantic property. A live Roslyn
symbol, compilation, syntax tree, semantic model, or closure retaining one must not survive into a late snapshot.
Unsupported detachment is a diagnostic identifying the carry and nested value path. Detached values must remain
usable by the operations explicitly supported for that detached kind; they do not grant renewed semantic access.

Prepared string constants may use an immutable compile-time pool. Runtime strings remain ordinary immutable strings
and must not be interned into that pool or the process-wide string intern table. Cache lookup is non-mutating;
computing and storing a miss are explicit operations. Snapshot equality and fingerprints use durable content, stable
ordering, code/library identity, and relevant host configuration, never mutable host object identity. Equivalent
inputs must reproduce equivalent output without a live compilation in the expression pass.

Each expression's storage writes and output are buffered and committed on success. Failure discards that expression's
changes; previously successful expressions are unaffected. Function and derivation calls participate in the caller's
transaction. A checked failure must roll back the failed callee's writes/output before its error is delivered as a
value. Normal `return` and `break` succeed and retain preceding writes. Prelude closing failure invalidates the
application's prepared snapshot and staged prelude output, so no late pass runs on partially detached state. Cached
prelude output must be replayed once on a cache hit, without re-executing semantic work.

## Host and mixin functions

Host operations are ordinary named functions or control intrinsics with definition-owned signatures, effect metadata,
documentation, and source diagnostics. These names describe the host surface required for existing features; they do
not introduce grammar productions. The host must register an operation before accepting its invocation.

### C# and host-language functions

`this` is the containing generation type, `target` the current applied symbol, and `attr` the applying attribute.
`target#0` and `target#parameterName` select method parameters; `attr#0` selects a constructor argument and `attr#Name`
selects attribute data. `this#Name` selects a type member. Ambiguous member names require an explicit host query rather
than arbitrary overload selection. Calls inherit these roots; passing a symbol as `param` does not replace `target`.

The host must support `name`, `type`, `fullName`, `visibility`, `members`, `parameters`, `attributes`, `attributesOf`,
`attributesOfExact`, `attributeOf`, `makeGeneric`, `nullableType`, and `csharpLiteral`. Queries returning symbols are
prelude-only. Detached name/type-display operations may run late when their required information was captured.
`csharpLiteral` formats host constants as valid C# source, including escaping and null; ordinary string rendering
does not perform source-code escaping.

All signatures below include the receiver as the first argument. `symbol` includes supported attribute and typed
constant handles; `typeName` is text identifying a host type, not a core kind. Live semantic queries run in the prelude.
Text-only formatting may run late after its inputs are detached.

| Method | Result and behavior |
| --- | --- |
| `name(any): string` | Host name of a symbol; otherwise its supported display text. |
| `type(symbol): symbol or null` | Associated type, including method return types and member/parameter types. |
| `fullName(symbol): string or null` | Fully qualified host type name, including generic arguments; null when no associated type exists. |
| `visibility(symbol): string` | Lowercase Roslyn accessibility name; unsupported values return errors. |
| `members(symbol): tuple` | Explicit members of a named type, excluding implicit members and accessor methods; ordered by source path, position, metadata name, then qualified display. |
| `parameters(symbol): tuple` | Parameters of a method or delegate in declaration order. |
| `attributes(symbol): tuple` | All attributes in host enumeration order. |
| `attributesOf(symbol, typeName): tuple` | Attributes matching the requested type, including compatible derived attribute types. |
| `attributesOfExact(symbol, typeName): tuple` | Attributes matching the exact requested attribute type. |
| `attributeOf(symbol, typeName): any` | First compatible attribute, or null. |
| `is(symbol, typeName): bool` | Host type compatibility, preserving existing simple/qualified-name matching and inheritance checks. |
| `unwrap(any): any` | Extract a host constant as its corresponding mixin value; null becomes null, primitive constants retain their kinds, arrays become tuples, and type constants remain symbols. Invalid host constants produce errors; other values pass through. |
| `makeGeneric(symbol, typeArgument: any): string` | Render a generic type application, replacing existing generic arguments and retaining required `global::` qualification. |
| `nullableType(symbol): string` | Render `Nullable<T>` for value types; retain reference and already-nullable types; unsupported unconstrained type parameters fail. |
| `csharpLiteral(any): any` | Render a typed constant as valid C# literal source; non-constant values pass through unchanged. |
| `identifier(string): string` | Escape a C# identifier, including keywords, using the host's identifier-escaping rules. |
| `floatTime(any): string` | Produce C# float time text: seconds by default, milliseconds divided by 1000, minutes multiplied by 60, integer ticks negated, and `%frequency` converted to its reciprocal. Null, empty, `tick`, and near-zero values produce `-0f`; malformed input fails. |

`makeGeneric` preserves the existing one-type-argument form. Additional generic arities may be registered explicitly;
they must not replace or remove that form. `floatTime` also accepts the existing case-insensitive unit spellings
`ms`, `millis`, `s`, `second`, `seconds`, `m`, `minute`, `minutes`, `t`, `tick`, and `ticks`.

Retain predicates for `ref`, `in`, `out`, `inout`, `argument`, `static`, `async`, `public`, `exposed`, `top`, `concrete`,
`partial`, `generic`, `genericMethod`, `struct`, `class`, `field`, `property`, `method`, `event`, `parameter`,
`typeSymbol`, `referenceType`, `valueType`, `nullable`, `pointer`, `containsPointer`, `enum`, `primitive`, `isSelf`,
`parameterDefault`, `nonEmptyStringConstant`, `equatableSelf`, `typedEqualsSelf`, `ordinaryTypedEqualsSelf`,
`objectEquals`, and `hashCode`. Host type compatibility checks must be distinguished from core `is(value, kind)` by
signature, e.g. a host type-name string versus a kind atom. Core `has` tests key presence and `contains` tests value
membership.

Each predicate named above has signature `predicate(symbol): bool` and is also available in predicate chains.
`ref`/`in`/`out` test parameter passing mode; `inout` means either `in` or `out`, and `argument` means an ordinary
by-value parameter. `exposed` includes public, internal, and protected-internal accessibility. `top` means a non-nested
type. `class` tests class types; `referenceType` tests reference types, including interfaces. `typeSymbol` tests named
type symbols. Use `:?typeSymbol` for that predicate and `:type` to obtain a symbol's associated type.
`isSelf` compares with the current generation type. `containsPointer` recursively inspects the type's structure.
`parameterDefault` accepts host null, primitive, and enum constants; `nonEmptyStringConstant` excludes whitespace-only
strings. Equality/hash predicates inspect the corresponding `IEquatable<T>`, `Equals`, and `GetHashCode` signatures,
including explicit interface implementations where supported. Predicate failures on incompatible values return false.

### Mixin resolution and wiring functions

The existing `wire`, `signature`, and `wireable` operations resolve generated method signatures and parameter wiring.
Preserve access to semantic types and attribute-driven injection while in the prelude. `resolveMixin(name, target)`
returns a resolved target descriptor or null; it must not silently render that descriptor before wiring checks.

| Method | Result and behavior |
| --- | --- |
| `resolveMixin(name: string, target: any): any` | Resolve a named callable target for wiring; return null when it cannot be resolved. |
| `wire(source: any, destination: any): string` | Render the ordered C# argument expressions required to wire the callables; incompatible wiring returns an error. |
| `signature(source: any, destination: any): bool` | Test callable signature equivalence according to the host's existing signature rules. |
| `wireable(source: any, destination: any): bool` | Test whether the same wiring accepted by `wire` is possible. |
| `derive(records: tuple): tuple` | Execute the derivation providers described below and return the transformed records. |

Callable inputs retain support for method symbols, delegate signatures, and resolved mixin descriptors. Resolution,
wiring, and derivation use prelude capabilities and participate in the caller's transaction.

### Mixin output and configuration functions

The following invocation surface carries the existing generator actions. Optional forms are separate signatures.

| Invocation | Effect |
| --- | --- |
| `emit(text)` | Append text to the current output target. |
| `emit(destination, text)` | Append to `CLASS`, `FILE`, `IMPLEMENTS`, `ANNOTATION`, or a named injection target. |
| `using(namespace)` | Contribute a using directive, deduplicated by the host. |
| `inject(target, text)` / `inject(target, priority, text)` | Contribute a method/mixin body fragment; omitted priority is zero. |
| `defineTarget(name, descriptor)` | Define or replace a target alias for this application. |
| `config(name, value)` | Set a recognized host configuration option; unknown options are diagnostics. |
| `log(value)` | Record a diagnostic log with source location. This one-argument form is distinct from numeric `log(number, number)`. |
| `dump(subject)` | Request host-supported `STATE`, `BUFFER`, or `AST` diagnostics. |

These invocations return null on success. Text arguments can be trailing content blocks. Output fragments retain
source order and destination metadata; the C# emitter adds the required enclosing type/namespace structure. The host
must specify line insertion independently from content-string continuation rules. Invalid destinations or priorities
are errors. Diagnostic logs may survive a failed transaction but must not be confused with committed generated output.

Target descriptors remain strings or host descriptors: `$Init` refers to an alias, `*` requests static, `^` requests
public visibility, `~QualifiedDelegate` uses a delegate signature, and `Name:QualifiedDelegate` supplies a name with
that signature. They belong inside text arguments, e.g. `<$Init>`; they are not smart variable expressions. Alias
replacement follows application order. Fragment priority and host candidate ordering must be stable and documented;
they must not depend on dictionary enumeration or parallel completion order.

### Derivation providers

`derive(records)` runs in the prelude. Its input is a tuple of table records with `symbol` (a live semantic symbol)
and `value` (any initial value, including null). Additional record fields are preserved. The outer collection is a
sequence, so it has tuple indices rather than string keys.

Every visible `derivation mixin` provider executes for each record in prepared file/import order and source declaration
order. Its identity does not filter it by the symbol's attributes; providers perform their own tests. Providers execute
their expression blocks as prelude computations. The result of each block/provider becomes `param#value` for the next;
`param#symbol` is the input symbol. Every such computation must explicitly return a value, including an explicit null
when appropriate. The invoking `this`, `target`, and `attr` remain unchanged. The caller's `param` is restored afterward.

Provider execution participates in the invoking prelude's storage and output transaction. Derivation expression blocks
share its local context, as the existing host feature requires; ordinary function calls still have their own locals.
The result preserves tuple length, order, symbols, and extra record fields, replacing only each `value`. Failure aborts
the entire operation, with entry index, provider identity, and source error in the diagnostic. Re-entering an active
provider is an error. Derivations must not be reordered or parallelized because later providers observe earlier writes.

## Implementation requirements

The current reference points are `Language/Library.cs` for registered capabilities, `Language/Functions` for builtin
behavior, `Language/Runtime/RoslynExecutionContext.cs` for host roots and derivation, and
`Language/Runtime/MixinVirtualMachine.cs` for transactional execution. Paths are relative to `src/MixinLanguage`.
The existing additional-file programs and source-generator tests are a feature corpus, not examples of this grammar.

The implementation must support the full set of generation capabilities: semantic inspection, attribute selection,
callable resolution and wiring, target aliases, output destinations and priorities, derivation, shared state, and
transactional execution. Existing programs must be rewritten to the new syntax and value contracts. Compatibility
with old method names, argument order, callback fields, or table-based sequence representations is not required.
Use the language's blocks, `when`, member selection, assignment, tuples, and canonical collection methods directly.
Do not add an alternate method merely to reproduce a form already expressible through those constructs.

Compiler and editor consumers should share one token stream and canonical semantic AST built from the grammar's parse
tree. Preserve start/end source offsets, line/column, declaration/reference identity, argument ranges, comments, and
continuations. Block and function ownership must be represented directly. Continuations are trivia for semantic
traversal but still contribute to string construction. Synthetic inline/hoisted nodes retain their originating ranges
and resolved definitions. Recovery nodes may support editor diagnostics, but erroneous programs must not execute.

### Conformance cases

An implementation should verify these observable contracts in addition to accepting the syntax examples:

- Direct, chained, and trailing calls bind the same logical arguments; nested tuples remain nested; signatures reject
  ambiguous overloads and invalid variadic fields.
- Function and inline calls agree on return values, argument evaluation count, and local/label isolation.
- Elvis and `when` do not execute unselected branches; checked failures are catchable while unchecked failures propagate.
- A present null shadows coarser storage; target variables do not leak between different symbols; carries do not collide
  across call sites and fail clearly when not initialized.
- Hoisting preserves branches and effects; strict expressions reject implicit hoisting; snapshots retain no live host
  objects; cache hits reproduce output without mutating constant pools.
- Failed expressions and checked failed callees do not commit writes or output; successful earlier expressions remain.
- Derivation preserves extra fields and ordering, passes each result onward, restores roots, and rejects recursion and
  valueless returns.
- Table replacement retains order; tuple indexing and missing/null keys differ correctly; callback errors abort transforms.
- Every required output destination, alias descriptor, wiring query, semantic predicate, and imported provider remains
  expressible through declarations and invocations in the unchanged grammar.
- Host sequence queries and derivation use tuples; table callbacks use the single documented record shape, and no
  compatibility overloads or table-as-list helpers are required.

### Grammar review notes (non-normative)

The grammar's accepted syntax is unchanged. Spelling corrections rename the parser rules to `expressionModifier`
and `variableIdentifier`; generated consumers must use those corrected rule names. Future tooling work may improve
diagnostics around overlapping `when` alternatives, operator attachment, content termination at end of file, and
interpolation/escape recovery. These notes
are not prerequisites or proposed syntax changes. Examples should follow the emitted tokens and parse tree rather
than introduce a second lexer or infer unsupported shorthand from the old language.
