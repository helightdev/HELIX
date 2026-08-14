Mixin Expressions

> This is currently a draft! This is not ready for implementation!

## Variables
- `@this`
- `@target` (The annotated scope, mostly either a class or a method)
- `@attr` (Valid only if the expression is declared by an attribute)
- `@arg0`, `@arg1`, `@arg2`, `@arg3`

## Properties
- `:name` | The name of the variable.
    ```
    @this:name => ClassName
    @target:name => MethodName
    @attr:name => AttributeName
    @arg0:name => ArgumentName
    ```

- `:type` | The type of the variable.
    ```
    @this:type => QualifiedClassName
    @target:type => QualifiedMethodReturnType
    @attr:type => QualifiedAttributeType
    @arg0:type => QualifiedArgumentType
    ```

###  Boolean Pseudo Properties
- `:?is<TYPE>` | Check static inheritance
- `:?has<MEMBER>` | Check if a member concretely exists.
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

## Scopes
- `@SCOPE` | Begin a new scope ending the previous scope if there is one
- `@MATCH` BooleanExpression | Requirement for the scope to match, otherwise performs @SKIP
- `@ASSERT` BooleanExpression | Accepts the scope and asserts an expression. False will fail the generation
- `@CODE` StringExpression | Writes a single line expression string
- `@END`  | Ends the current scope without beginning a new scope. Continue evaluating the next line afterwards
- `@RETURN`  | Returns successfully from the generation
- `@SKIP` | Skips to the next scope or end label. If the is no jump target, it exits and fails the generation
- `@FAIL` | Fails the generation unconditionally

Note: Multiple boolean expressions per matcher / assertions are combined into an AND

Those are effectively equivalent expressions
```
@SCOPE
    @MATCH @arg0:?argument
    @ASSERT @arg0:type?is<IEvt>
    @CODE @this.RegisterEventHandler<@arg0:type>(@target, @attr#Priority)
    @RETURN
@SCOPE
    @MATCH @arg0:?ref
    @ASSERT @arg0:type?is<IEvt>
    @CODE @this.RegisterEventHandler<@arg0:type>(@target, @attr#Priority)
    @RETURN
@END
@FAIL
---
@SCOPE
    @MATCH @arg0:?argument
    @ASSERT @arg0:type?is<IEvt>
    @CODE @this.RegisterEventHandler<@arg0:type>(@target, @attr#Priority)
    @RETURN
@SCOPE
    @MATCH @arg0:?ref
    @ASSERT @arg0:type?is<IEvt>
    @CODE @this.RegisterEventHandler<@arg0:type>(@target, @attr#Priority)
    @RETURN
---
@MATCH @arg0:!?inout
@ASSERT @arg0:type?is<IEvt>
@CODE @this.RegisterEventHandler<@arg0:type>(@target, @attr#Priority)
```