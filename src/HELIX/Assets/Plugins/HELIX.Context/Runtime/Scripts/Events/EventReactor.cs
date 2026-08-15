using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HELIX.Context {
  /// <summary>
  ///     .Net event based proxy event handler and invocator.
  ///     All events used by the class should have mutable public properties for their data.
  ///     Event subscribers should manipulate the properties of the event they receive to alter data.
  /// </summary>
  /// <typeparam name="T">type of the event</typeparam>
  public class EventReactor<T> : IEventReactor where T : Evt<T> {
    private static EventReactor<T> _globalReactor;
    private readonly List<HandlerRegistration<T>> _registrations = new();

    public static EventReactor<T> Shared => _globalReactor ??= EventManager.Instance.RegisterEvent<T>();

    /// <summary>
    ///     Returns the type of the generic <see cref="T" />.
    /// </summary>
    public Type TypeDelegate() {
      return typeof(T);
    }

    /// <summary>
    ///     Invokes the multicast event system.
    ///     See <see cref="EventReactor{T}" /> for details about required property attributes.
    /// </summary>
    /// <param name="obj">the delegate boxed as an object</param>
    public void RaiseUnsafe(ref object obj) {
      var evt = (T)obj;
      Raise(ref evt);
      obj = evt;
    }

    /// <summary>
    ///     Subscribes a method to the backing event.
    ///     Uses reflections to create method delegates of type <see cref="T" />
    ///     which can be subscribed normally.
    /// </summary>
    /// <param name="obj">the instance of object which method shall be hooked</param>
    /// <param name="info">the method which shall be hooked</param>
    /// <param name="priority">the priority of the subscription</param>
    public object SubscribeUnsafe(object obj, MethodInfo info, int priority = 0) {
      var handler = DelegateUtils.CreateDelegate<EvtHandler<T>>(obj, info);

#if DEBUG
      return Subscribe(handler, priority, $"{info.DeclaringType?.Name ?? "@"}.{info.Name}");
#else
            return Subscribe(handler, priority);
#endif
    }

    /// <summary>
    ///     Unsubscribes a handler registration from the backing event.
    /// </summary>
    /// <param name="subscription">
    ///     The subscription object returned by <see cref="SubscribeUnsafe" /> which shall be
    ///     unsubscribed
    /// </param>
    public void UnsubscribeUnsafe(object subscription) {
      Unsubscribe(subscription as HandlerRegistration<T>);
    }

    public string ResolveDebugName(object subscription) {
      lock (_registrations) {
        return _registrations.FirstOrDefault(x => x.Handler == subscription as EvtHandler<T>)?.DebugName;
      }
    }

    public EventReactorInfo CreateInfo() {
      var priorityRows = new List<EventReactorInfo.PriorityRow>();
      lock (_registrations) {
        foreach (var group in _registrations.GroupBy(x => x.Priority)) {
          var handlers = group
            .GroupBy(y => y.DebugName)
            .Select(g => g.Count() == 1 ? g.Key : $"{g.Key} ({g.Count()})")
            .ToList();

          priorityRows.Add(
            new EventReactorInfo.PriorityRow {
              Priority = group.Key,
              Handlers = handlers
            }
          );
        }
      }

      return new EventReactorInfo {
        Name = typeof(T).Name,
        Type = typeof(T),
        Rows = priorityRows
      };
    }

    /// <summary>
    ///     Invokes the multicast event system.
    ///     See <see cref="EventReactor{T}" /> for details about required property attributes.
    /// </summary>
    /// <param name="evt">the event argument object</param>
    public void Raise(ref T evt) {
      lock (_registrations) {
        for (var index = 0; index < _registrations.Count; index++) {
          var registration = _registrations[index];
          if (registration.IsDisposed) continue;
          registration.Handler.Invoke(ref evt);
        }

        for (var i = _registrations.Count - 1; i >= 0; i--) {
          var registration = _registrations[i];
          if (registration.IsDisposed) _registrations.RemoveAt(i);
        }
      }
    }

    /// <summary>
    ///     Safely raises an event by invoking all registered event handlers for the specified event.
    ///     Ensures that exceptions thrown by individual handlers are caught and logged,
    ///     allowing remaining handlers to execute uninterrupted.
    /// </summary>
    /// <param name="evt">The event instance to be processed by the registered handlers.</param>
    public void RaiseSafe(ref T evt) {
      lock (_registrations) {
        for (var index = 0; index < _registrations.Count; index++) {
          var registration = _registrations[index];
          try {
            if (registration.IsDisposed) continue;
            registration.Handler.Invoke(ref evt);
          } catch (Exception e) {
            Debug.LogError($"Error invoking event handler {registration.DebugName} for event {typeof(T).Name}");
            Debug.LogException(e);
          }
        }

        for (var i = _registrations.Count - 1; i >= 0; i--) {
          var registration = _registrations[i];
          if (registration.IsDisposed) {
            registration.Dispose();
            _registrations.RemoveAt(i);
          }
        }
      }
    }

    /// <summary>
    ///     Subscribes a delegate to the backing event.
    /// </summary>
    /// <param name="handler">the delegate to subscribe</param>
    /// <param name="priority">the priority of the subscription</param>
    /// <param name="debugName">The debug name of the subscription</param>
    /// <param name="interceptor">An optional interceptor to wrap the handler with.</param>
    public HandlerRegistration<T> Subscribe(
      EvtHandler<T> handler,
      int priority = 0,
      string debugName = null,
      EventInterceptor<T> interceptor = null
    ) {
      if (handler == null) throw new ArgumentNullException(nameof(handler));

      lock (_registrations) {
#if DEBUG
        debugName ??= EventReactorInfo.GetDebugName(handler);
#endif

        if (interceptor != null) handler = interceptor(handler);
        var registration = new HandlerRegistration<T>(
          this,
          priority,
          handler,
          debugName
        );

        var index = _registrations.Count;

        for (var i = 0; i < _registrations.Count; i++) {
          if (priority < _registrations[i].Priority) {
            index = i;
            break;
          }
        }

        _registrations.Insert(index, registration);
        return registration;
      }
    }


    /// <summary>
    ///     Subscribes a delegate to the backing event.
    /// </summary>
    /// <param name="handler">the delegate to subscribe</param>
    /// <param name="priority">the priority of the subscription</param>
    /// <param name="debugName">The debug name of the subscription</param>
    /// <param name="interceptor">An optional interceptor to wrap the handler with.</param>
    public HandlerRegistration<T> Subscribe(
      ConsumerEvtHandler<T> handler,
      int priority = 0,
      string debugName = null,
      EventInterceptor<T> interceptor = null
    ) {
      return Subscribe((ref T args) => handler(args), priority, debugName, interceptor);
    }

    /// <summary>
    ///     Subscribes a delegate to the backing event.
    /// </summary>
    /// <param name="handler">the delegate to subscribe</param>
    /// <param name="priority">the priority of the subscription</param>
    /// <param name="debugName">The debug name of the subscription</param>
    /// <param name="interceptor">An optional interceptor to wrap the handler with.</param>
    public HandlerRegistration<T>
      SubscribeOnce(
        EvtHandler<T> handler,
        int priority = 0,
        string debugName = null,
        EventInterceptor<T> interceptor = null
      ) {
      lock (_registrations) {
        var consumer = new SingleConsumer(handler);
        var registration = Subscribe(consumer.Handle, priority, debugName, interceptor);
        consumer.registration = registration;
        return registration;
      }
    }


    /// <summary>
    ///     Subscribes a delegate to the backing event.
    /// </summary>
    /// <param name="handler">the delegate to subscribe</param>
    /// <param name="priority">the priority of the subscription</param>
    /// <param name="debugName">The debug name of the subscription</param>
    /// <param name="interceptor">An optional interceptor to wrap the handler with.</param>
    public HandlerRegistration<T>
      SubscribeOnce(
        ConsumerEvtHandler<T> handler,
        int priority = 0,
        string debugName = null,
        EventInterceptor<T> interceptor = null
      ) {
      lock (_registrations) {
        var consumer = new SingleConsumer((ref T args) => handler(args));
        var registration = Subscribe(consumer.Handle, priority, debugName, interceptor);
        consumer.registration = registration;
        return registration;
      }
    }

    /// <summary>
    ///     Subscribes a stream handler delegate to the backing event.
    ///     The handler will be invoked and if it returns true, it will be unsubscribed automatically.
    /// </summary>
    /// <param name="handler">the stream handler delegate to subscribe</param>
    /// <param name="priority">the priority of the subscription</param>
    /// <param name="debugName">The debug name of the subscription</param>
    /// <param name="interceptor">An optional interceptor to wrap the handler with.</param>
    public HandlerRegistration<T> SubscribeStream(
      StreamEvtHandler<T> handler,
      int priority = 0,
      string debugName = null,
      EventInterceptor<T> interceptor = null
    ) {
      lock (_registrations) {
        var consumer = new StreamConsumer(handler);
        var registration = Subscribe(consumer.Handle, priority, debugName, interceptor);
        consumer.registration = registration;
        return registration;
      }
    }


    /// <summary>
    ///     Unsubscribes a delegate from the backing event.
    /// </summary>
    /// <param name="registration">the registration to unsubscribe</param>
    public void Unsubscribe(HandlerRegistration<T> registration) {
      lock (_registrations) { _registrations.Remove(registration); }
    }

    private class SingleConsumer {
      private readonly EvtHandler<T> _handler;
      internal HandlerRegistration<T> registration;

      public SingleConsumer(EvtHandler<T> handler) {
        _handler = handler;
      }

      public void Handle(ref T evt) {
        try { _handler.Invoke(ref evt); } finally { registration.DisposeLater(); }
      }
    }

    private class StreamConsumer {
      private readonly StreamEvtHandler<T> _handler;
      internal HandlerRegistration<T> registration;

      public StreamConsumer(StreamEvtHandler<T> handler) {
        _handler = handler;
      }

      public void Handle(ref T evt) {
        var result = _handler.Invoke(ref evt);
        if (result) registration.DisposeLater();
      }
    }
  }

  internal class DelegateUtils {
    public static T CreateDelegate<T>(MethodInfo info) where T : Delegate {
      var delegateType = typeof(T);
      var delegated = info.CreateDelegate(delegateType);
      return (T)delegated;
    }

    public static T CreateDelegate<T>(object instance, MethodInfo info) where T : Delegate {
      var delegateType = typeof(T);
      var delegated = info.CreateDelegate(delegateType, instance);
      return (T)delegated;
    }
  }


  public abstract class HandlerRegistration : IDisposable {
    public int Priority { get; protected set; }
    public string DebugName { get; protected set; }
    public bool IsDisposed { get; protected set; }
    public Action onDisposed;

    // public void OnDisposeRemoveFrom(IDisposableContainer container) {
    //     var reference = new WeakReference<IDisposableContainer>(container);
    //     onDisposed = () => {
    //         if (reference.TryGetTarget(out var target)) { target.RemoveOnDestroyDisposal(this); }
    //     };
    // }

    public virtual void Dispose() {
      onDisposed?.Invoke();
      onDisposed = null;
      IsDisposed = true;
    }
  }

  public class HandlerRegistration<T> : HandlerRegistration, IDisposable where T : Evt<T> {
    public HandlerRegistration(
      EventReactor<T> reactor,
      int priority,
      EvtHandler<T> handler,
      string debugName
    ) {
      Priority = priority;
      Handler = handler;
      DebugName = debugName ?? "unknown";
      Reactor = reactor;
    }

    public EvtHandler<T> Handler { get; }
    public EventReactor<T> Reactor { get; private set; }

    public override void Dispose() {
      base.Dispose();
      onDisposed?.Invoke();
      onDisposed = null;
      Reactor?.Unsubscribe(this);
      Reactor = null;
    }

    public void DisposeLater() {
      IsDisposed = true;
      Reactor = null;
    }
  }

  public delegate void EvtHandler<T>(ref T args) where T : Evt<T>;

  public delegate void ConsumerEvtHandler<in T>(T args) where T : Evt<T>;

  public delegate bool StreamEvtHandler<T>(ref T args) where T : Evt<T>;

  public interface IEventReactor {
    Type TypeDelegate();
    void RaiseUnsafe(ref object obj);
    object SubscribeUnsafe(object obj, MethodInfo info, int priority = 0);
    void UnsubscribeUnsafe(object subscription);
    string ResolveDebugName(object subscription);
    EventReactorInfo CreateInfo();
  }

  public class EventReactorInfo {
    public string Name { get; set; }
    public Type Type { get; set; }
    public List<PriorityRow> Rows { get; set; } = new();

    public static string GetDebugName(Delegate handler) {
      try {
        var method = handler.Method;
        var declaringType = method.DeclaringType;
        var name = method.Name;
        return $"{declaringType?.Name ?? "@"}.{name}";
      } catch (MissingMemberException) { return "Private Method"; }
    }

    public class PriorityRow {
      public int Priority { get; set; }
      public List<string> Handlers { get; set; } = new();
    }
  }
}