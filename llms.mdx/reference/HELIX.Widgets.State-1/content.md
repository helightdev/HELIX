# State<T> (/reference/HELIX.Widgets.State-1)

# State<T>

```
public abstract class State<T> : DiagnosticableBase, IDiagnosticable, IBuildable where T : StatefulWidget<T>
```

<p>
The state of a <a data-furef-uid="HELIX.Widgets.StatefulWidget%601">StatefulWidget</a>. It holds its widget's internal state and is used to
update the UI when the widget configuration changes.
</p>
<p>
Once the state is created, a call to <a data-furef-uid="HELIX.Widgets.State%601.InitState">InitState</a> will be made immediately before the first
<a data-furef-uid="HELIX.Widgets.State%601.Build(HELIX.Widgets.BuildContext)">Build</a> call.
</p>
<p>
The state maintains a list of managed disposables that will be disposed when the state is disposed and also
provides a <a data-furef-uid="HELIX.Widgets.State%601.Dispose">Dispose</a> callback method.
</p>
<p>
<a data-furef-uid="HELIX.Widgets.Signals.Signal">Signal</a> values can be directly accessed from within the state's <a data-furef-uid="HELIX.Widgets.State%601.Build(HELIX.Widgets.BuildContext)">Build</a> methods and
dependencies will be automatically tracked by the <a data-furef-uid="HELIX.Widgets.Signals.SignalDependencyTracker">SignalDependencyTracker</a>.
</p>
<p>
Once any dependency changes or <a data-furef-uid="HELIX.Widgets.State%601.SetState">SetState</a> gets called, a <a data-furef-uid="HELIX.Widgets.ModificationBarrier.Rebuild(HELIX.Widgets.IWidgetElement)">ModificationBarrier.Rebuild</a>
will be scheduled. Once the <a data-furef-uid="HELIX.Widgets.Reconciler">Reconciler</a> rebuilds the object, <a data-furef-uid="HELIX.Widgets.State%601.CanReconcile(%600)">CanReconcile</a> will be
called to determine if the state can be reused with the new widget configuration. If it returns true,
<a data-furef-uid="HELIX.Widgets.State%601.DidUpdateWidget(%600)">DidUpdateWidget</a> will be called to notify the state of the new widget configuration before the
<a data-furef-uid="HELIX.Widgets.State%601.Build(HELIX.Widgets.BuildContext)">Build</a> method is called again.
</p>

## dependencyTracker

```
public SignalDependencyTracker dependencyTracker
```

The <a data-furef-uid="HELIX.Widgets.Signals.SignalDependencyTracker">SignalDependencyTracker</a> that tracks signal dependencies for this state.

## mount

```
public BuildContext mount
```

The <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> that this state is mounted to. This context will never change and can be used
to access the <a data-furef-uid="UnityEngine.UIElements.VisualElement">VisualElement</a> of the widget. Will also always be equal to the <code>context</code> passed
into the <a data-furef-uid="HELIX.Widgets.State%601.Build(HELIX.Widgets.BuildContext)">Build</a> method.

## widget

```
public T widget
```

The widget configuration that is currently bound to this state.

## Build(BuildContext)

```
public abstract Widget Build(BuildContext context)
```

Builds the widget in the given context.

## AddDisposable<S>(S)

```
protected S AddDisposable<S>(S disposable) where S : IDisposable
```

Tracks a disposable dependency that will automatically be disposed when the state is disposed.

## RemoveDisposable<S>(S)

```
protected void RemoveDisposable<S>(S disposable) where S : IDisposable
```

Removes a disposable dependency from the state's managed disposables.

## InitState()

```
public virtual void InitState()
```

<p>Called immediately before the first <a data-furef-uid="HELIX.Widgets.State%601.Build(HELIX.Widgets.BuildContext)">Build</a> call to initialize the state.</p>

## CanReconcile(T)

```
public virtual bool CanReconcile(T oldWidget)
```

<p>Called when the state is about to be rebuilt with a new widget configuration.</p>

## DidUpdateWidget(T)

```
public virtual void DidUpdateWidget(T oldWidget)
```

<p>Called when the state is rebuilt with a new widget configuration.</p>

## Dispose()

```
public virtual void Dispose()
```

## SetState()

```
public void SetState()
```

Schedules a rebuild of the widget.

## SetState(Action)

```
public Action SetState(Action action)
```

Wraps an action in a tailing <a data-furef-uid="HELIX.Widgets.State%601.SetState">SetState</a> call.

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```