# DebouncedScheduler (/reference/HELIX.Animation.DebouncedScheduler)

# DebouncedScheduler

```
public class DebouncedScheduler : IVisualElementScheduler
```

Represents a scheduler that executes tasks in a debounced manner, ensuring that only one task
is scheduled at a time within the Unity UIElements system.

## DebouncedScheduler(IVisualElementScheduler)

```
public DebouncedScheduler(IVisualElementScheduler scheduler)
```

## DebouncedScheduler(VisualElement)

```
public DebouncedScheduler(VisualElement element)
```

## Execute(Action<TimerState>)

```
public IVisualElementScheduledItem Execute(Action<TimerState> timerUpdateEvent)
```

## Execute(Action)

```
public IVisualElementScheduledItem Execute(Action updateEvent)
```

<p>
Schedule this action to be executed later.
</p>

## Stop()

```
public void Stop()
```

## Replace(IVisualElementScheduledItem)

```
public void Replace(IVisualElementScheduledItem newItem)
```