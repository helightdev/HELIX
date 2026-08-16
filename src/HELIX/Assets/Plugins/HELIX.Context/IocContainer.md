# IoC Container
The IoC container in HELIX Context aims to reflect the usual workflow known from Spring and Quarkus as closely as possible
while adapting it to game contexts and maintaining full reflectionless AOT compatibility using Roslyn source generators.

It is based on mixins adding behavior to classes and configuring "beans" (here mostly referred to as components)
using configuration delegates. Those components are discovered through their stereotype annotations by the Roslyn
source generator and registered statically at assembly load time.

Registrations are the boundary between generated and runtime code. A registration describes the component's activation,
scope, exposed keys, dependencies, publications, handler bindings and load order. The container can therefore reason
about component lifetimes and initialization without reflecting over component types at runtime.

The term **component** describes any container-managed instance. **Service** is a semantic specialization used for
game systems and application logic; it follows the same basic lifecycle and registration rules.

## Scopes
The root accessor to the system is the `HXContainer` (current name) that functions similar to the application context
in Spring, being the actual data holder for the IoC data. It holds a list of all scopes and registered types for
introspection. 

The actual data is mostly stored in scopes to allow more fine-grained lifetime management.
Each component that aims to be loaded has to associate itself to a scope and at runtime every service/component is
associated with a managed scope as well. The first scope always loaded is the `RegistrarScope`. This scope allows
the registration of new scopes and possible processors before the actual application is being loaded. Following this,
the singleton scope `ApplicationScope` is loaded next. This scope will live from the start of the application to the
end of the application and doesn't allow reinitialization in the scope of the container.

Additional scopes are defined as follows:

- `SessionScope` is associated with a single game session. A game session is defined as the game actually running.
  In the context of the game, this is generally effectively a semi-singleton. The main use case for this is defining
  systems that do not run in something like main menus or lobbies where one may use another custom scope or none at all.
  This scope depends on the application scope and may be started or stopped multiple times.
- `SceneScope` is associated with a scene, generally still managed by the user and meant as a scene-specific group.
- `GameObjectScope` is bound to the lifecycle of a `GameObject` and will initialize and deinitialize with it. It is a child of the
  application scope, session scope, or scene scope. Hierarchies are not fully strictly enforced on a semantic level.

Scopes form an ownership tree. A component can depend on components from its own scope or an ancestor scope, but not
from a child or sibling scope. This prevents a longer-lived component from directly retaining a component that may be
disposed earlier. Where such a relationship is needed, it should be represented through a scope-aware provider or a
lifecycle event rather than a direct reference.

The following invariants apply to the managed scope hierarchy:

- A live scope instance is associated with exactly one managed scope.
- A child scope never outlives its parent.
- Disposing a parent first disposes all of its child scopes.
- The application scope is created at most once for a container.
- Session, scene and `GameObject` scopes may have multiple distinct instances.

Custom scopes may implement `IScope`. Their permitted parents and multiplicity can be described through scope
registrations or processors in the registrar scope. Hierarchies do not have to be encoded in the type system, but the
container must reject cycles and invalid lifetime relationships when creating a scope.

## Dependency Graph
The dependency graph is constructed after discovery and semi-statically pre-evaluated for each scope in advance
using the wire keys and dependencies of registrations. The wire keys are used to create comparable identities for
static pre-construction. They are only used if the dependency has the `Wirable` flag; otherwise the wire key is ignored.

A type-based wire key consists of the dependency type and an optional qualifier. Qualifiers distinguish multiple
components that expose the same type. Registrations may expose multiple keys, for example their concrete type and one
or more service interfaces. Each dependency is either required or optional: a missing required dependency prevents the
owning component from loading, while an optional dependency does not.

All components can (besides their own type-based keys) also expose publications. Those describe dependencies the
component may (after initialization) possibly provide. If the published dependency is marked with Required, this
dependency is interpreted as being guaranteed to be provided by the component; failure to do so will result in an error.

Scripted dependencies represent dependencies that require work instead of a direct component lookup, such as loading
an asset or waiting for an external resource. They carry their own stage, order and flags and can participate in graph
matching by supplying a wire key. An implicitly loadable scripted dependency may be scheduled by the container when no
component publication already satisfies it.

While the actual loading of the dependency graph is dynamic, the static pre-evaluation is used to provide static
insight with IDE tools into the dependency flow and estimated initialization order and to speed initialization by
presorting initialization queues.

Graph validation should produce explicit diagnostics for missing required dependencies, ambiguous providers, duplicate
keys and dependency cycles. Static diagnostics may be best-effort where publications are dynamic, but a scope must not
be considered initialized while any of its required dependencies remain unresolved.

## Initialization
The initialization of a scope may happen either synchronously or asynchronously. Bean definitions allow the system
to determine if they are sync capable or require async initialization by scanning their handler bindings for the
`ComponentAsyncInitEvent`. If a scope is trying to be loaded synchronously while requiring async init, the load fails with an
exception. If the load is done asynchronously, it may handle both normal synchronous initialization as well as async
initialization. Loading is performed following the dependency graph with each component first doing its synchronous
initialization code before (if present and allowed) running async initialization using `RaiseLocal`.

Initialization is performed in 3 stages:

- `PreInit` prepares dependencies needed before normal components are activated.
- `Init` activates components and invokes their regular initialization handlers.
- `PostInit` performs work that requires the scope's regular components to already be available.

Components/services generally run in `Init`. Since they may declare scripted dependencies, those may require loading
in `PreInit`, `Init` or `PostInit`. Both scripted dependencies and components/services may define a load order.
Loading is performed iteratively until either all services are loaded with their dependencies satisfied or
no new service is able to be loaded without breaking its binding constraints. The load order is only relevant for 
participants loaded in the same loading pass; it never overrides a dependency edge or initialization stage.

A scope becomes available for resolution only after its required initialization has completed successfully. If a
component fails to activate or initialize, scope initialization fails as a whole. Components already created for that
scope are unloaded in reverse dependency order so callers never observe a partially initialized scope.

## Resolution
Resolution begins in the requesting component's managed scope and proceeds through its ancestors. A match includes both
the requested type and qualifier. Multiple equally valid providers are considered ambiguous unless the request
explicitly asks for a collection or another selection policy.

The generated injection metadata and the dependency graph must describe the same lookup. This ensures that a dependency
validated during discovery cannot resolve differently at runtime. Plain objects should generally use constructor
injection, while Unity-created objects may use generated field or property injection where their construction is owned
by Unity or by a component activator.

## Deinitialization
Scope deinitialization mirrors initialization and is performed in the following order:

- The scope stops accepting new resolutions and cancels scope-owned asynchronous work.
- Child scopes are deinitialized first.
- Components are unloaded in reverse dependency order through their `UnloadComponent` hooks.
- Locally registered handlers and other owned resources are disposed.
- Container-owned Unity objects are destroyed.
- The scope is detached from its parent and removed from the container.

Deinitialization is idempotent. A failure in one component's cleanup is reported but does not prevent the remaining
components from being unloaded. This guarantees that the scope releases as much owned state as possible even when an
individual teardown handler fails.

## Scene Injected Components 