using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HELIX.Context {
  // ReSharper disable once InconsistentNaming
  public interface Evt {
    static void Raise<T>(ref T evt) where T : Evt<T> {
      Evt<T>.Reactor.Raise(ref evt);
    }

    static void Raise<T>(T evt) where T : Evt<T> {
      Evt<T>.Reactor.Raise(ref evt);
    }

    static void RaiseSafe<T>(ref T evt) where T : Evt<T> {
      Evt<T>.Reactor.RaiseSafe(ref evt);
    }

    static void RaiseSafe<T>(T evt) where T : Evt<T> {
      Evt<T>.Reactor.RaiseSafe(ref evt);
    }

    static HandlerRegistration<T> Subscribe<T>(
      EvtHandler<T> action,
      int priority = 0,
      string debugName = null
    ) where T : Evt<T> {
#if DEBUG
      debugName ??= EventReactorInfo.GetDebugName(action);
#endif
      return EventReactor<T>.Shared.Subscribe(action, priority, debugName);
    }

    static HandlerRegistration<T> Subscribe<T>(
      ConsumerEvtHandler<T> action,
      int priority = 0,
      string debugName = null
    ) where T : Evt<T> {
#if DEBUG
      debugName ??= EventReactorInfo.GetDebugName(action);
#endif
      return EventReactor<T>.Shared.Subscribe(action, priority, debugName);
    }

    static HandlerRegistration<T> SubscribeOnce<T>(
      EvtHandler<T> action,
      int priority = 0,
      string debugName = null
    ) where T : Evt<T> {
#if DEBUG
      debugName ??= EventReactorInfo.GetDebugName(action);
#endif
      return EventReactor<T>.Shared.SubscribeOnce(action, priority, debugName);
    }

    static HandlerRegistration<T> SubscribeOnce<T>(
      ConsumerEvtHandler<T> action,
      int priority = 0,
      string debugName = null
    ) where T : Evt<T> {
#if DEBUG
      debugName ??= EventReactorInfo.GetDebugName(action);
#endif
      return EventReactor<T>.Shared.SubscribeOnce(action, priority, debugName);
    }

    static HandlerRegistration<T> SubscribeStream<T>(
      StreamEvtHandler<T> action,
      int priority = 0,
      string debugName = null
    ) where T : Evt<T> {
#if DEBUG
      debugName ??= EventReactorInfo.GetDebugName(action);
#endif
      return EventReactor<T>.Shared.SubscribeStream(action, priority, debugName);
    }
  }

  // ReSharper disable once InconsistentNaming
  public interface Evt<TSelf> : Evt where TSelf : Evt<TSelf> {
    static EventReactor<TSelf> Reactor => EventReactor<TSelf>.Shared;

    static HandlerRegistration<TSelf> Subscribe(
      EvtHandler<TSelf> action,
      int priority = 0,
      string debugName = null
    ) {
      return Evt.Subscribe(action, priority, debugName);
    }

    static HandlerRegistration<TSelf> Subscribe(
      ConsumerEvtHandler<TSelf> action,
      int priority = 0,
      string debugName = null
    ) {
      return Evt.Subscribe(action, priority, debugName);
    }

    static HandlerRegistration<TSelf> SubscribeOnce(
      EvtHandler<TSelf> action,
      int priority = 0,
      string debugName = null
    ) {
      return Evt.SubscribeOnce(action, priority, debugName);
    }

    static HandlerRegistration<TSelf> SubscribeOnce(
      ConsumerEvtHandler<TSelf> action,
      int priority = 0,
      string debugName = null
    ) {
      return Evt.SubscribeOnce(action, priority, debugName);
    }

    static HandlerRegistration<TSelf> SubscribeStream(
      StreamEvtHandler<TSelf> action,
      int priority = 0,
      string debugName = null
    ) {
      return Evt.SubscribeStream(action, priority, debugName);
    }
  }

  public static class EvtExtensions {
    public static TSelf Raise<TSelf>(this TSelf evt) where TSelf : Evt<TSelf> {
      var self = evt;
      Evt.Raise(ref self);
      return self;
    }

    public static TSelf RaiseSafe<TSelf>(this TSelf evt) where TSelf : Evt<TSelf> {
      var self = evt;
      Evt.RaiseSafe(ref self);
      return self;
    }
  }

  public interface IAsyncChainEvt { }

  public abstract class AsyncChainEvt<TSelf> : Evt<AsyncChainEvt<TSelf>>, Evt<TSelf>, IAsyncChainEvt
  where TSelf : AsyncChainEvt<TSelf>, Evt<TSelf> {
    private readonly LinkedList<Func<UniTask>> _chain = new();

    public event Func<UniTask> Chain {
      add => _chain.AddLast(value);
      remove => throw new NotSupportedException();
    }

    public async UniTask<TSelf> RaiseAsync() {
      var referenced = this as TSelf;
      Evt<TSelf>.Raise(ref referenced);
      return await ExecuteAsyncChain(referenced);
    }

    public static async UniTask<TSelf> ExecuteAsyncChain(TSelf self) {
      foreach (var action in self._chain) {
        if (action == null) continue;
        try { await action.Invoke(); } catch (Exception e) {
          Debug.LogError($"Exception in async chain of event {self.GetType().Name}");
          Debug.LogException(e);
        }
      }
      return self;
    }

    public void Reset() {
      _chain.Clear();
    }
  }

  /// <summary>
  ///     Commonly used event priorities.
  /// </summary>
  public static class EventPriority {
    public const int First = int.MinValue;
    public const int Earlier = -200;
    public const int Early = -100;
    public const int Normal = 0;
    public const int Late = 100;
    public const int Later = 200;
    public const int Last = int.MaxValue;
  }
}