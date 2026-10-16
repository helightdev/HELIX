# HELIX

This project is a large "base" framework / userspace engine layer for unity, focused primarily on ui, lifecycel
management, dependency injection and mixins. It provides a compose-style ui framework and mixin system making heavy use
of code generation, aiming to be reflection free. It targets zero per frame memory allocations and generally tries to
keep them as low as possible.

## Code Style

With this project being heavily framework oriented and extensible, hardcoding behaviour that may be wanted to be changed
by the user is to be avoided. To keep readability of the codebase high, the amount of helper methods and classes should
be kept low and only used where neccessary. Visibility plumbbing should be completely avoided, there shouldn't be
methods just to access another private method when there is no reason for the private method to just be made internal or
public.

Total file size of C# sources (excluding mixin globals and similar files) should be kept below 1000 lines per file.

The use of linq and other potentially memory allocating constructs should be minimized, especially in hotpaths - They
are generally allowed though for initialization logic or very logic heavy code that is otherwise unreadble.

Exploding method variants for multiple call signatures should be avoided if it does not heavily hurt developer
experience - You should prefer optional parameters with defaul values in that case.

## Structure

The project is structured into multiple submodules with assembly definitions:

- HELIX.Core: Shared utilties and definitions used by mixins and non-mixin based code.
- HELIX.Context: The core framework, containing the mixin system and dependency injection.
- HELIX.Compose: The compose style mix-mode immediate-like ui framework.
- HELIX.UI: Integration of compose into the mixin workflow.
- HELIX.Boot: Bootstrapping module (not yet implemented).
- HELIX.Support: Third party integrations, primarily OdinInspector
- HELIX.Devtoools: Development tools and editor integrations. Will provide custom unity cli commands for testing and
  debugging that can be used by agents, the user as well as future ide plugins connected using the unity cli.

Outdated:

- HELIX.Widgets: An old flutter-style widget system that is currently being replaced and is heavily deprecated because
  of the high allocation count and memory preasure, making it unsuitable for use in production.

## Tests

Tests should be easily readable and compact, boilerplate and stub code should be put in fixtures. The tests should be
designed to be as close as possible to real use cases and should cover both simple cases as well as complex cases and
edge cases.

## Mixins

Mixin specs can be seen under MixinExpressions.md in HELIX.Context and examples are in MixinGlobals.md. The framework is
designed to be heavily used with mixins, with only the Compose module being slightly decoupled from it, being only
connected to mixins in the seperate UI module. (HELIX.Compose may be used by non mixin users, while HELIX.UI is designed
to only be used with mixins).

## Compose UI

The compose framework is heavily based on source generators for creating composition boundaries, static delegate
wrappers, prop structs and proxy methods and boilerplate id generation. When designing new components, the goal should
be to avoid using `cx.AUTHORING` where possible, since it's harder to debug, verbose and bypassing the intended
workflow. The context system may be used to make data available to descendents while allowing them to subcribe to
changes and resolve it more quickly. State management should be hanled using the context system together with the
compose specific implementation of mixins.

When designing new components, they must be designed to be as style agnostic and composable as possible, minimizing
hardcoded behaviour and values. The theming system should be used to provide the style descriptors and values, also
storing default visual behaviour. It uses the sate property system to define mutable state that can used without
allocations while being highly composable. New visual styles and defaults should be implemented using theme properties
and the state system.

Components may use custom style structs but should try sticking to existing generic structs when possible or use them
internally to make theme property reuuse easier. When introducing new styles, the semantic roles and other properties
should be respected. Theme properties can depend on other theme properties and this should be heavily used when styles
become more complex. The user should be able to change individual aspects of the visual style by changing as few theme
properties as possible while affecting most/all components.

## Compiling and Debugging

While developing, the unity editor is running and reachable using the new unity cli
`unity --help`. To run tests, the cli commands are to be used instead of
`unity test` since the latter tries running the editor in batchmode which is not supported.

Never try to manually read the unity editor log files or manipulate the unity installation. You can use the unity cli to
get the status of the recompilation and trigger a new one. You can also use the unity cli to dynamically evaluate code.
If a recompilation seems to not trigger, check the console output. If multiple calls didn't trigger a recompilation and
didn't show errors, assume the recompilation to have finished anyways, since the cli might not always pick it up.

Outside of the untiy cli, `dotnet` commands must not be used and the project must not be built by agents without user
approval. 

Never manually create .meta files or modify them, they will be always properly generated by the editor
without requiring manual intervention. If you really need them, trigger a recompilation.