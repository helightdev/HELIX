Mixin Expressions

> This is currently a draft! This is not ready for implementation!

## Variables
- `@this`
- `@target` (The annotated scope, mostly either a class or a method)
- `@attr` (Valid only if the expression is declared by an attribute)
- `@arg` (Access to method arguments by name or index using paths)
- `@local` (Access to per execution local temporary data)
- `@var` (Access to per mixin class generation data)

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
  
TODO:
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

Boolean Pseudo Properties may be inverted with :!?. Example @this:!?static => This class is not static

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
- `@MATCH` BooleanExpression | Requirement for the scope to match, otherwise performs @SKIP
- `@ASSERT` BooleanExpression | Accepts the scope and asserts an expression. False will fail the generation
- `@CODE` StringExpression | Appends a single line of an expression string at the determined target location
- `@CODE<TARGET>` StringExpression | Same as normal @CODE
- `@CODE<InjectTarget>` StringExpression | Injects code at the predefined target with the given name.
- `@CODE<CLASS>` StringExpression | Appends the code line at the end of the current partial class (for new methods, parameters, etc.) 
- `@CODE<FILE>` StringExpression | Appends the code line in the same namespace scope outside the class (for new types)
- `@CODE<IMPLEMENTS>` StringExpression | Adds a single implements entry based on the generated string.
- `@CODE<ANNOTATION>` StringExpression | Adds a single annotation entry based on the generated string.
- `@USING` StringExpression | Adds a using statement at the top of the file
- `@LOCAL<NAME>` StringExpression | Stores the value of the given string expression into @local#name
- `@VAR<NAME>` StringExpression | Stores the value of the given string expression into @var#name
- `@END`  | Ends the current scope without beginning a new scope. Continue evaluating the next line afterward
- `@RETURN`  | Returns successfully from the generation
- `@GOTO<LABEL>` | Jumps to scope at the given local label.
- `@SKIP` | Skips to the next scope or end label. If the is no jump target, it exits and fails the generation
- `@FAIL` | Fails the generation unconditionally
- `@LOG` String Expression | Logs the given string expression to the console
- `@DUMP<STATE>` | Dumps the current state of the mixin expression to the console
- `@DUMP<BUFFER>` | Dumps the current string buffer of the mixin expression to the console

Note: Multiple boolean expressions per matcher / assertions are combined into an AND
Note: Code lines are buffered until the end of the expression's execution and only then applied

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

## Functions

Functions can be declared and called using `@FUNC` and `@CALL` respectively. 
Functions share the same locals and variables as the calling scope. A failure inside a function will propagate
upwards to the calling scope, returns inside the function will only return from the function and continue 
executing the calling scope. Functions may not be nested and must be closed with `@END` in a balanced manner.
Functions may include scopes which are also allowed to use the `@END` expression.

## Prepared Expressions

Mixins can be declared in `MixinPrepareGlobal` to be prepared in advance before being
available to all mixin expression attributes. A model is generated and collected before running subsequent
mixin source generators. The contents evaluated by those files effectively declares a global scope being present
in all mixin expressions. Custom procedures are therefore defined using functions that can be called from
any mixin expression. Target Ordering and Declaration is still left to the called. The prepared mixins may also
therefore declare global variables that are available to all mixin expressions.