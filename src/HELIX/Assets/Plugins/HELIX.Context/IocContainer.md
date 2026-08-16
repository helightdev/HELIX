# IoC Container
The ioc cotnainer in helix context aims to reflect the usual workflow known from Spring and Quarkus as closely as possible
while trying to adapt it to game contexts, maintaining full reflectionless aot compatibility using rosyln source generators.

It based on mixins adding to base classes and configuring "beans" (here jsut mostly referred to as components)
using configuration delegate. Those components are then discovered using their stereotype annotation by the roslyn 
source generator before being statically discovered at assembly load time.

## Scopes
The root accessor to the system is the `HXContainer` (current name) that functions similar to the application context
in spring, being the actual dataholder for the ioc data. It holds a list of all scopes and registered types for
introspection. 

The actual data is mostly stored in scopes though as to allow more finegrained lifetime management. 
Each component that aims to be loaded has to associate itself to a scope and at runtime every service/component is
associated to a managed scope as well. The first scope being always loaded is the "RegistrarScope". This scope allows
the registration of new scopes and possible processors before the actual application is being loaded. Following this,
the singleton scope `ApplicationScope` is loaded next. This scope will live from the start of the application to the
end of the application and doesn't allow reinitialization in the scope of the container.

Additional scopes are defined as follows:
- `SessionScope` associated to single game session. A game session is defined as the game actually running.
  In the context of the game, this is generally effectively a semi singleton - The main usecase for this is defining
  systems that do not run in something like main menus or lobbies where one may use another custom scope or none at all.
  This scope depends on the application scope and may be started or stopped multiple times.
- `SceneScope` associated to a scene, generally still managed by the user and meant as a semi specific group
- `GameObjectScope` bound to the lifecycle of a gameobject, will initialize and deinitialize with it. Either child of the
  application scope, session scope, or scene scope. Hierarchies are not fully strictly enforced on a semantic level.

## Dependency Graph
The dependency graph is constructed after discovery and currently semi-statically pre-evaluated for each scope in advance
using the wire keys and dependencies of registrations. The wire keys are used to create comparable identities for
static pre-cosntruction. They are only used if the dependency has the flag wirable, otherwise the wire key is ignored.

All components can (besides their own type based keys) also expose publications. Those describe depedencies the
component may (after initialization) possibly provide. If the published dependency is marked with Required, this
dependency is interpreted as being guarantted to be provided by the component, failure to do so will result in an error.

While the actual loading of the dependency graph is dynamic, the static preevaluation is later used to provide static
insight with ide tools into the dependency flow and estimated initialization order and to speed initialization by
presorting initialization queues.

## Initialization
The initialization of a scope may happen either synchronously or asynchronously. Bean Definitions allow th system
to determine if they are sync capabable or require async initialization by scanning their handler bindings for the
`AsyncInitEvent`. If a scope is trying to be loaded synchronously while requiring async init, the load fails with an
exception. If the load is done asynchronously, it may handle both normal synchronous initialization as well as async
initialization. Loading is peformed following the dependency graph with each component first doing its synchronous
initialization code before (if present and allowed) running async initialization using `RaiseLocal`.

Initialization is performed in 3 stages:
- PreInit
- Init
- PostInit

Components/Services generally run in init. Since they may declare scripted dependencies, those may require loading
in PreInit, Init or PostInit. Both scripted dependencies and components/service may define a load order.
Loading is performed iteratively until either all services are loaded having their dependencies satisfied or
no new service is able to be loaded without breaking its binding constraints. The load order is only relevant for 
participants loaded in the same loading pass.
