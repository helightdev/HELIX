Mixin Expressions

## Variables
- `@this`
- `@target` (The annotated scope, mostly either a class or a method)
- `@attr` (Valid only if the expression is declared by an attribute)
- `@arg` (Access to method arguments by name or index using paths)
- `@local` (Access to per execution local temporary data)
- `@var` (Access to per mixin class generation data)
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
- `:?struct` | Check if the target's type is a struct
- `:?class` | Check if the target's type is managed / class
- `:?matches<REGEX>` | Check if the stringified value matches the regex
- `:?signature<METHOD>` | Checks if the signatures of two methods or delegates match.
- `:?wireable<FROM_METHOD><TO_METHOD>` | See :wire
- `:?structHasEquality` | Checks if a struct model uses equality
- `:?structNoArgs` | Checks if a struct model has no argument / no constructor
- `:?structAugment` | Checks if a struct model is using augmenting

Boolean Pseudo Properties may be inverted with :!?. Example @this:!?static => This class is not static

### Special Operations

- `:replace<REGEX><REPLACEMENT>` | Replaces the matches of the given regex with replacement
- `:replaceFirst<REGEX><REPLACEMENT>` | Same as replace but just with the first match
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
- `:propStructCall<TARGET><VARIABLE>` | Generates a call to the string TARGET (Possibly the method name or reference)
  expecting an instance of the prop being accessible through the string VARIABLE. Generates the call signature by
  unwrapping the struct into the method call. Must be called on a prop struct handle.
- `:structParams` / `:structParams<PREFIX>` | Returns the prop struct's comma-separated constructor parameter
  declarations. An optional prefix is prepended and joined with a comma only when parameters exist.
- `:structArgs` / `:structArgs<PREFIX>` | Returns the prop struct's comma-separated constructor/call arguments,
  preserving parameter modifiers. An optional prefix is prepended and joined with a comma only when arguments exist.
- `:makeGeneric<TYPE>` | Makes an unbound generic type into a bound generic type with the given type parameter.
- `:visibility` | Returns the visibility of a type or member (public, private, internal, protected...)

Example for the wire method: `@target:name(@local#Method:wire<(@target)>);`

### Tables
Are a custom structure similar to lua tables but immutable used as expression lists or objects.
Values are resolved through `#` paths and `#:?exists`.
For tables, `:has<>` is defined as checking for contains value.
Operations on tables generally never fail.

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
- `@END`  | Ends the current scope without beginning a new scope. Continue evaluating the next line afterward
- `@RETURN`  | Returns successfully from the generation
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
- `@PROP_STRUCT<StructName><LocalLabel>[<datatype>][<noGenerate>]` SyntaxTarget | Analyzes the syntax target as
  a prop struct and assigns its reference handle to the local variable at local label. By default it declares the
  struct without a datatype member. The optional `datatype` flag includes the datatype member when the struct is
  generated. The optional `noGenerate` flag suppresses the complete struct declaration while still returning the
  handle. Flags may be supplied in either order.
- `@AUGMENT_STRUCT<LocalLabel>` SyntaxTarget | Augments the prop struct with the given name for syntax target
  and assigns a reference handle to the local variable at local label. When a direct `@this#StructName` target does
  not exist, declares an empty public struct with that name; the generated type remains available to later references
  in the same expression.

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
- `:?method` | Is a method
- `:?delegate` | Is a delegate
- `:?event` | Is an event
- `:?field` | Is a field
- `:?property` | Is a property
- Integrate more things with tables
- Make :?has and some other string based expressions work better with non string types
- Make behavior more clear in docs
