# ModificationBarrier (/reference/HELIX.Widgets.ModificationBarrier)

# ModificationBarrier

```
public static class ModificationBarrier
```

<p>
Provides a mechanism for managing and restricting modifications to the widget hierarchy while
ensuring proper handling of callbacks, rebuilds, and disposals during a modification frame.
</p>
<p>
The barrier represents a reentrant context in which <a data-furef-uid="HELIX.Widgets.Reconciler">Reconciler</a> actions, disposals,
and frame callbacks are collected and executed once the outermost modification frame completes.
</p>

## MaxCallbacksPerFrame

```
public static int MaxCallbacksPerFrame
```

## MaxRebuildsPerFrame

```
public static int MaxRebuildsPerFrame
```

## MaxDisposalsPerFrame

```
public static int MaxDisposalsPerFrame
```

## UseRuntimeHelper

```
public static bool UseRuntimeHelper
```

## InsideModification

```
public static bool InsideModification { get; }
```

## Run(Action)

```
public static void Run(Action action)
```

Runs the provided action within the barrier context.

## EnqueueHierarchyDisposable(IHierarchyDisposable)

```
public static void EnqueueHierarchyDisposable(IHierarchyDisposable disposable)
```

Enqueues a disposable to be disposed at the end of the current frame.

## TryDisposeHierarchyDisposable(IHierarchyDisposable)

```
public static void TryDisposeHierarchyDisposable(IHierarchyDisposable disposable)
```

<p>Tries to dispose of the provided disposable at the earliest possible time.</p>
<p>
If currently inside a finalization context, the disposable will be disposed of immediately.
Otherwise, it will be enqueued for disposal at the end of the current frame if <a data-furef-uid="HELIX.Widgets.ModificationBarrier.InsideModification">ModificationBarrier.InsideModification</a>
or at the end of the next future frame.
</p>

## RemoveHierarchyDisposable(IHierarchyDisposable)

```
public static void RemoveHierarchyDisposable(IHierarchyDisposable disposable)
```

## RemoveRebuild(IWidgetElement)

```
public static void RemoveRebuild(IWidgetElement element)
```

## Rebuild(IWidgetElement)

```
public static void Rebuild(IWidgetElement element)
```

Enqueues an element for rebuild.

## AddPostFrameCallback(Action)

```
public static void AddPostFrameCallback(Action callback)
```

Enqueues a callback to be run at the end of the current frame.