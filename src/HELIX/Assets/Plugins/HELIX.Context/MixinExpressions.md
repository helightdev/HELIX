Mixin Expressions

## Variables
- `@this`
- `@target` (The annotated scope, mostly either a class or a method)
- `@attr` (Valid only if the expression is declared by an attribute)
- `@arg` (Access to method arguments by name or index using paths)
- `@local` (Access to per execution local temporary data)
- `@var` (Access to per mixin class generation data)
- `@tar` (Access to transactional variables associated with the current `@target` symbol)
- `@true` (The value true)
- `@false` (The value false)
- `@null` (The value null)
- `@table` (An empty immutable table)
- `@param` (The call parameter if inside an expression function)

## Properties
- `:name` | The name of the variable.
    ```
    @this:name => ClassName
    @target:name => MethodName
    @attr:name => AttributeName
    @arg#0:name => ArgumentName
    ```

- `:type` | The type of the variable.
    ```
    @this:type => QualifiedClassName
    @target:type => QualifiedMethodReturnType
    @attr:type => QualifiedAttributeType
    @arg#0:type => QualifiedArgumentType
    ```
- `:fullName` | The full (not qualified) name of a type. (Includes generic type parameters)
- `:unwrap` | Unwraps a 'wrapped' value
  - string to its unquoted literal value
  - type to the qualified name but without `global::`

###  Boolean Pseudo Properties
- `:?is<TYPE>` | Check static inheritance
- `:?has<MEMBER>` | Check if a member concretely exists.
- `:?eq<VALUE>` | Checks if a value is equal to the stringified given value.
- `:?exists` | Check if the target exists
- `:?isSelf` | Check if (mostly a method parameter) is the type of @this
- `:?ref` | Check if a method parameter is ref
- `:?in` | Check if a method parameter is in
- `:?out` | Check if a method parameter is out
- `:?async` | Check if a method is async
- `:?inout` | Check if a method parameter is in or out
- `:?argument` | Check if a method parameter is a normal argument, aka. neither in,ref,out
- `:?static` | Check for static flag for class or method
- `:?public` | Check for public visibility
- `:?exposed` | Check for either public or internal visibility
- `:?top` | Check if a type is toplevel
- `:?concrete` | Check if the target is concrete, aka neither abstract/virtual or an abstract class/interface class
- `:?partial` | Check for partial flag
- `:?generic` | Check if the target is generic
- `:?genericMethod` | Check if the value is a generic method.
- `:?struct` | Check if the target's type is a struct
- `:?class` | Check if the target's type is managed / class
- `:?field` | Check if the value is a field symbol
- `:?property` | Check if the value is a property symbol
- `:?method` | Check if the value is a method symbol
- `:?event` | Check if the value is an event symbol
- `:?parameter` | Check if the value is a parameter symbol
- `:?type` | Check if the value is a named type symbol. This predicate spelling is distinct from the existing `:type` value transformation.
- `:?primitive` | Check if the value's type is an enum or a C# primitive supported by direct equality.
- `:?containsPointer` | Check if the value's type contains a pointer, including nested generic and array types.
- `:?typedEqualsSelf`, `:?ordinaryTypedEqualsSelf`, `:?objectEquals`, `:?hashCode` | Inspect equality members declared by a named type.
- `:?parameterDefault`, `:?nonEmptyStringConstant` | Validate typed constants used by generated optional parameters.
- `:?matches<REGEX>` | Check if the stringified value matches the regex
- `:?signature<METHOD>` | Checks if the signatures of two methods or delegates match.
- `:?wireable<FROM_METHOD><TO_METHOD>` | See :wire

Boolean Pseudo Properties may be inverted with :!?. Example @this:!?static => This class is not static

### Special Operations

- `:replace<REGEX><REPLACEMENT>` | Replaces the matches of the given regex with replacement
- `:replaceFirst<REGEX><REPLACEMENT>` | Same as replace but just with the first match
- `:format<ARG0><ARG1?>` | Formats the value as an invariant composite format string. Invalid format strings fail.
- `:identifier` | Escapes the rendered value when it is a C# keyword or contextual keyword.
- `:switch<IF_TRUTHY><IF_FALSY>` | Executes on a trueness value and switch between two values
- `:wire<TO_METHOD>` | Tries wiring the from method/delegate into the other method/delegate. The to method must have a signature
   capable of receiving the from data, having at maximum the same amount of arguments. The intersection arguments must
   be assignable from FROM to TO. This returns the rewritten call arguments as a string or fails. No arguments result
   in an empty string.
- `:put<KEY><VALUE>` | Returns a new table with Key,Value added. If the value wasn't a table, this creates a new table
- `:remove<KEY>` | Removes a value from a table and returns the new table
- `:push<VALUE>` | Pushes a value onto the table, keyed by the length
- `:pop` | Remove the value with Length-1 from the table. If used as a list, this is the inverse
   opposite `push`.
- `:size` | returns the size of a table or string. Otherwise always returns 0.
- `:floatTime` | parses a time format string into float seconds
   Empty or null or only `tick`: 0f
   `<Number>s|second|seconds`: seconds
   `<Number>ms|millis`: milliseconds
   `<Integer>t|tick|ticks`: ticks (the value multiplied by -1, must be handled by the supporting structure)
   `<Number>m|minute|minutes`: minutes
   `%<Number>`: times per second
   `<Number>`: seconds (if no unity is specified)
- `:makeGeneric<TYPE>` | Makes an unbound generic type into a bound generic type with the given type parameter.
- `:visibility` | Returns the visibility of a type or member (public, private, internal, protected...)
- `:members` | Returns the members declared directly on a type as an integer-indexed table of semantic values.
  Source members retain declaration order. Members without source locations use a deterministic fallback order.
  Inherited members are not included. Using this operation on a value that does not resolve to a type fails.
- `:parameters` | Returns the parameters of a method or delegate as an integer-indexed table of semantic values in
  signature order. Methods without parameters return an empty table. Using this operation on another value fails.
- `:nullableType` | Returns the C# type syntax for a typed semantic value with nullable input support. Reference types
  and nullable value types are unchanged; non-nullable value types gain `?`. Unconstrained type parameters fail.
- `:csharpLiteral` | Renders a typed constant as valid C# source, including escaped strings and characters, booleans,
  numeric suffixes, enum members or casts, `typeof` values, arrays, and `null`.
- `:derive` | Derives the values in a specially shaped integer-indexed table by executing every visible derivation
  provider. See **Derivation** below.
- `:reduce<INITIAL><FUNCTION>` | Performs a strict left fold over a table in iteration order. See **Table reduction**
  below. Write `<(@local#Initial)>` when the initial value is dynamic.

Example for the wire method: `@target:name(@local#Method:wire<(@target)>);`

### Tables
Are a custom structure similar to lua tables but immutable used as expression lists or objects.
Values are resolved through `#` paths and `#:?exists`.
For tables, `:has<>` is defined as checking for contains value.
Basic operations on tables generally never fail. Operations with structural or callback requirements, such as
`:derive` and `:reduce`, fail when those requirements are not met.

### Table reduction

`:reduce<INITIAL><FUNCTION>` folds a table into a single value. The initial value is evaluated once before iteration.
For every entry, the named function is called with the following table as `@param`:

```
acc   => the current accumulator
key   => the current entry key
value => the current entry value
```

The function must return a value using `@RETURN Expression`. That value becomes the accumulator for the next entry.
The final accumulator is returned by `:reduce`. Reducing an empty table returns the evaluated initial value without
calling the function.

Reduction is a strict left fold and always follows the table's iteration order. Variables are shared with the calling
expression. Each invocation receives a fresh `@param` table, and failure inside the transformer fails the reduction.
Reaching the end of the transformer or using a valueless `@RETURN` fails because `@null` is a valid returned value and
must remain distinct from a missing return.

```
@FUNC<CollectNames>
  @RETURN @param#acc:push<(@param#value:name)>
@END

@LOCAL<Names> @this:members:reduce<(@table)><CollectNames>
```

## Derivation

Derivation applies globally visible function-like expressions to caller-provided values associated with semantic
symbols. It allows an outer mixin to seed and transform a model without feature-specific host functions.

### Derivation input

`:derive` requires an integer-indexed table whose values are tables containing at least `symbol` and `value`:

```
0 => {
  symbol => FieldSymbol,
  value  => InitialValue
}
1 => {
  symbol => PropertySymbol,
  value  => InitialValue
}
```

The caller constructs the initial value. It may be any mixin value, including `@null`, a string, a semantic value, or
a nested table. This allows callers to seed implicit information such as default datatypes before annotations refine
it. Additional entries in each record are allowed and preserved.

Every outer key must be a non-negative integer. Every `symbol` must be a semantic symbol. Invalid input fails with an
error that identifies the invalid outer-table key.

The input can be constructed with the existing table transformations:

```
@FUNC<SeedMember>
  @RETURN @table:put<symbol><(@param)>:put<value><(@table)>
@END

@LOCAL<Members> @this:members:mapValues<SeedMember>
@LOCAL<Derived> @local#Members:derive
```

### Derivation evaluation

For every input entry, `:derive` starts with the entry's `value` and evaluates every derivation provider visible in the
prepared additional-file compilation. Each returned value is passed to the following provider. Providers decide
whether they apply using ordinary control flow, semantic inspection, and `@tar`; attributes applied to `symbol` do not
select providers and do not replace the invoking expression's roots.

Evaluation order is deterministic and observable:

1. Outer table iteration order.
2. Prepared additional-file/import order.
3. Source declaration order within each file.

Derivation is sequential and must not be reordered or parallelized. Given transformations `A`, `B`, and `C`, the final
value is `C(B(A(initial)))`.

The returned outer table retains the original keys and order. Each record retains its `symbol` and any additional
entries; only `value` is replaced. Tables remain immutable, so neither the outer input table nor its records are
modified.

Failure in any derivation expression fails the complete `:derive` operation. No partially derived table is returned.
The error should identify the table entry, derivation provider, and original expression error.

### Derivation declarations

A derivation provider is declared in an additional file with `@DERIVATION<QualifiedAttributeType>`. The qualified type
is the provider's identity for cataloging and diagnostics; it is not matched against attributes on `symbol`.
The same qualified type may not be declared as both `@ANNOTATION` and `@DERIVATION`:

```
@DERIVATION<HELIX.PropAttribute>
  @LOCAL<Prop> @param#symbol:attributeOf<HELIX.PropAttribute>
  @RETURN @param#value:put<required><false>:put<default><(@local#Prop#value)>
@END
```

During execution:

- `@this`, `@target`, and `@attr` retain their values from the annotation expression that invoked `:derive`.
- `@param#symbol` is the semantic symbol whose value is being derived.
- `@param#value` is the value returned by the preceding transformation, or the caller-provided initial value for the
  first transformation.

A derivation expression must successfully return a value using `@RETURN Expression`. Reaching its end or using a
valueless `@RETURN` is an error. `@RETURN @null` is a valid value-producing return.

Derivation expressions execute in prelude mode and have the same capabilities as an ordinary annotation prelude.
They may inspect semantic and attribute values, manipulate tables, read and write variables, call functions, use
control flow, return values, fail, emit new class or file code, and contribute mixins or other supported output. Output
is buffered and committed according to the same transactional rules as output from the invoking annotation prelude.

Derivation evaluation retains the invoking expression's execution context. Variables and locals are shared, including
across different symbols, so later transformations observe earlier writes. The only root value changed for an
individual evaluation is `@param`, which is temporarily bound to its `{ symbol, value }` argument table and restored
after the derivation expression returns. Existing transactional behavior applies: a failed containing expression does
not commit variable or output changes.

Recursive invocation of the same derivation provider is invalid. Implementations detect the cycle and report the
active provider.

### Dynamic Only Operations

- `:and<(BooleanExpression)><...>`
- `:or<(BooleanExpression)><...>`

Note: A simple not may be done using `@false:eq<(BooleanExpression)>`

### Members
Member Reference: `<member>#<reference>`
```
@this#animal:type => Type of the field 'animal'
@attr#order => The 'order' member of the annotated attribute
@target#evt => For example, the 'evt' argument of a method
```
All expressions may be wrapped once using `()` round brackets. Example: `@(this:type)`

## ScopesiedMethodReturnType
    @attr:type => QualifiedAttributeType
    @arg#0:type => QualifiedArgumentType
    ```
- `@SCOPE` | Begin a new scope ending the previous scope if there is one
- `@SCOPE<LABEL>` | Begin a new scope ending the previous scope if there is one while storing a local label pointer of the given name
- `@FUNC<LABEL>` | Begin declaring a function of the given name.
- `@CALL<LABEL>` | Call a function of the given name. Functions share the same locals and variables as the calling scope.
- `@CALL<LABEL>` Expression | Call a function of the given name with the value being put as @param.
- `@INLINE<LABEL>` | Expands a function body at the directive before prelude/late carry hoisting.
- `@MATCH` BooleanExpression | Requirement for the scope to match, otherwise performs @SKIP
- `@MATCH<LABEL>` BooleanExpression | Requirement for the scope to match, otherwise jumps to label
- `@ASSERT` BooleanExpression | Accepts the scope and asserts an expression. False will fail the generation
- `@CODE` StringExpression | Appends a single line of an expression string at the determined target location
- `@CODE<TARGET>` StringExpression | Same as normal @CODE
- `@CODE<InjectTarget>` StringExpression | Injects code at the predefined target with the given name.
- `@MIXIN<Target>` StringExpression | Injects code at given target with default priority (0).
- `@MIXIN<Target><Priority>` StringExpression | Injects code at given target with a specified priority.
- `@CODE<CLASS>` StringExpression | Appends the code line at the end of the current partial class (for new methods, parameters, etc.) 
- `@CODE<FILE>` StringExpression | Appends the code line in the same namespace scope outside the class (for new types)
- `@CODE<IMPLEMENTS>` StringExpression | Adds a single implements entry based on the generated string.
- `@CODE<ANNOTATION>` StringExpression | Adds a single annotation entry based on the generated string.
- `@USING` StringExpression | Adds a using statement at the top of the file
- `@LOCAL<NAME>` Expression | Stores the value of the given string expression into @local#name
- `@VAR<NAME>` Expression | Stores the value of the given string expression into @var#name
- `@TAR<NAME>` Expression | Transactionally stores the typed value in `@tar#name` for the current `@target`. Successful
  later annotation preludes on the same target observe it; a failed expression does not publish the write.
- `@END`  | Ends the current scope without beginning a new scope. Continue evaluating the next line afterward
- `@RETURN` | Returns successfully without a value. This is valid for ordinary generation and function calls, but is
  invalid where a value-producing return is required, including derivation expressions and reduce transformers.
- `@RETURN` Expression | Returns successfully with the typed expression value. `@RETURN @null` is a value-producing
  return and is distinct from a valueless `@RETURN`.
- `@GOTO<LABEL>` | Jumps to scope at the given local label.
- `@SKIP` | Skips to the next scope or end label. If the is no jump target, it exits and fails the generation
- `@FAIL` | Fails the generation unconditionally
- `@FAIL` String Expression | Fails the generation with the message being the given string expression
- `@LOG` String Expression | Logs the given string expression to the console
- `@DUMP<STATE>` | Dumps the current state of the mixin expression to the console
- `@DUMP<BUFFER>` | Dumps the current string buffer of the mixin expression to the console
- `@DUMP<AST>` | Dumps the currently available ast nodes from both the global and when available the local scope
- `@RESOLVE_MIXIN<LocalLabel>` StringExpression | Tries resolving the mixin target assigning it to the local variable at label,
   otherwise null. This is intended to be used for wiring and allowing dynamic signature checks.
- `@PUT<LABEL><KEY>` Expression | Shorthand helper for table :put with a given key into a local variable
- `@PUSH<LABEL>` Expression | Shorthand helper for table :push into a local variable

Note: Multiple boolean expressions per matcher / assertions are combined into an AND
Note: Code lines are buffered until the end of the expression's execution and only then applied

Arguments written with `<>` are always constant and don't allow variable inputs
Arugments may be dynamic when written as `<()>`, they will then allow `<(StringExpression)>` as the input of the function.
A function may definie multiple inputs (though currently not used) by diamonds without delimiters, for example
`@IMAGINARY<Argument0><Argument1> Primary Expression`, in this case you could maybe also write
`@IMAGINARY<true><(@var#identifier)> Primary Expression`

Those are effectively equivalent expressions
```
@SCOPE
    @MATCH @arg#0:?argument
    @ASSERT @arg#0:type:?is<IEvt>
    @CODE @this.RegisterEventHandler<@arg#0:type>(@target, @attr#Priority)
    @RETURN
@SCOPE
    @MATCH @arg#0:?ref
    @ASSERT @arg#0:type:?is<IEvt>
    @CODE @this.RegisterEventHandler<@arg#0:type>(@target, @attr#Priority)
    @RETURN
@END
@FAIL
---
@SCOPE
    @MATCH @arg#0:?argument
    @ASSERT @arg#0:type:?is<IEvt>
    @CODE @this.RegisterEventHandler<@arg#0:type>(@target, @attr#Priority)
    @RETURN
@SCOPE
    @MATCH @arg#0:?ref
    @ASSERT @arg#0:type:?is<IEvt>
    @CODE @this.RegisterEventHandler<@arg#0:type>(@target, @attr#Priority)
    @RETURN
---
@MATCH @arg#0:!?inout
@ASSERT @arg#0:type:?is<IEvt>
@CODE @this.RegisterEventHandler<@arg#0:type>(@target, @attr#Priority)
```

## Targets
Targets are most commonly the name of the parameterless instance method of a mixin object: `OnAwake`.
They may implicitly be declared by the `On` prefix together with one of the default targets in `MixinOn`.

There are special cases for lifecycle related targets, they start with `$` and are translated into type specific
method, usually Awake and OnDestroy.

Static methods may be referenced by prepending `*` in front of the name.
Methods are forced to be public by prepending `^` in front of the name. (* and ^ may be in any order)

Another way to declare targets is by declaring the fully qualified name of a delegate that is then used as the signature
of the method prefix by `~`. Example: `~HELIX.Context.RegistrationConfigurator`

This syntax also supports methods with parameters which are otherwise unsupported. The delegate reference-based declaration
is compatible with the static modifier, allowing `*~HELIX.Context.RegistrationConfigurator` as well. 

Otherwise, general targets can be described by a delegate, but a different name as follows:
`^*Configure:HELIX.Context.RegistrationConfigurator`
where modifiers and the name are at the beginning of the expression,
with the type being delayed and specified after the `:`.

## Functions

Functions can be declared and called using `@FUNC` and `@CALL` respectively. 
Functions share the same locals and variables as the calling scope. A failure inside a function will propagate
upwards to the calling scope, returns inside the function will only return from the function and continue 
executing the calling scope. Functions may not be nested and must be closed with `@END` in a balanced manner.
Functions may include scopes which are also allowed to use the `@END` expression.
`@RETURN Expression` returns a typed value to the caller; a reference-only operand retains tables, semantic values,
booleans, and null without rendering them to strings. `@RETURN` without an operand is a successful valueless return.
Callers such as `:derive` and `:reduce` may require a value-producing return and fail when a called function returns
without one or reaches its end.
`@INLINE` may reference either a prepared library function or a function declared in the same expression. Each
expansion receives private scope labels. A successful `@RETURN` exits only the inlined body by jumping to its end;
return expressions are evaluated for failures, but their values are discarded. Recursive inlining is invalid.

## Additional-file mixins

All mixin behavior lives in Unity additional files under `Assets/Mixins`. The filename must be
`<Name>.HelixSourceGenerator.additionalfile`. An attribute or mixin interface is associated with its behavior by a
qualified-name companion block:

```text
@ANNOTATION<My.Namespace.MyAttribute>
  @DEFINE_TARGET<Init><^Initialize>
  @PRELUDE
    @CARRY<MemberName> @target:name
  @END

  @MIXIN<$Init><0> InitializeMember("@carry#MemberName");
@END
```

The optional `@PRELUDE ... @END` section performs Roslyn-dependent analysis and collection. Everything else in the
annotation block is the normal late expression. `@DEFINE_TARGET<Name><Value>` declares an annotation-provided target
alias, referenced as `$Name`. C# attribute classes contain only their normal constructor/data API; expression text,
orders, and target declarations are not stored in C# metadata.

A qualified provider may instead define function-like, prelude-only transformation behavior with a derivation block:

```text
@DERIVATION<My.Namespace.MyAttribute>
  @RETURN @param#value:put<name><(@param#symbol:name)>
@END
```

Derivation blocks are discovered only by `:derive`; they are not evaluated as ordinary target-producing annotations.
The same qualified provider cannot have both block types. Every visible derivation executes for each record and may
filter itself using `@tar` or semantic data. Derivations have the full capabilities of an ordinary annotation prelude.

Reusable `@FUNC` declarations can be placed in the same files. A static marker class associates a function library
with a file using `[MixinLibrary("Name")]`, and a target or mixin attribute opts into those functions using
`[MixinImport(typeof(MyLibrary))]`. HELIX additional files must therefore be imported into the consuming project's
`Assets/Mixins` directory together with the corresponding HELIX modules.

Additional-file libraries are parsed and prepared independently of the compilation before evaluating consumers.
Imports placed on a mixin attribute are available whenever that attribute contributes expressions, including
attributes introduced through `RequireMixin`. Repeated imports of the same library are deduplicated, while
conflicting function declarations are reported as errors.


## TODO
- `@LABEL<LABEL>` | No-Op that acts like `@SCOPE<LABEL>\n@END` to define jump targets more cleanly.
- `:?delegate` | Is a delegate
- Integrate more things with tables
- Make :?has and some other string based expressions work better with non string types
- Make behavior more clear in docs
