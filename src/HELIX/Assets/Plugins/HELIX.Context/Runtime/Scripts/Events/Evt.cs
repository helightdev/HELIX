using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HELIX.Context {
    // ReSharper disable once InconsistentNaming
    public interface Evt {

        public static void Raise<T>(ref T evt) where T : Evt<T> {
            Evt<T>.Reactor.Raise(ref evt);
        }

        public static void Raise<T>(T evt) where T : Evt<T> {
            Evt<T>.Reactor.Raise(ref evt);
        }

        public static void RaiseSafe<T>(ref T evt) where T : Evt<T> {
            Evt<T>.Reactor.RaiseSafe(ref evt);
        }

        public static void RaiseSafe<T>(T evt) where T : Evt<T> {
            Evt<T>.Reactor.RaiseSafe(ref evt);
        }

        public static HandlerRegistration<T> Subscribe<T>(
            EvtHandler<T> action,
            int priority = 0,
            string debugName = null
        ) where T : Evt<T> {
#if DEBUG
            debugName ??= EventReactorInfo.GetDebugName(action);
#endif
            return EventReactor<T>.Shared.Subscribe(action, priority, debugName);
        }

        public static HandlerRegistration<T> Subscribe<T>(
            ConsumerEvtHandler<T> action,
            int priority = 0,
            string debugName = null
        ) where T : Evt<T> {
#if DEBUG
            debugName ??= EventReactorInfo.GetDebugName(action);
#endif
            return EventReactor<T>.Shared.Subscribe(action, priority, debugName);
        }

        public static HandlerRegistration<T> SubscribeOnce<T>(
            EvtHandler<T> action,
            int priority = 0,
            string debugName = null
        ) where T : Evt<T> {
#if DEBUG
            debugName ??= EventReactorInfo.GetDebugName(action);
#endif
            return EventReactor<T>.Shared.SubscribeOnce(action, priority, debugName);
        }

        public static HandlerRegistration<T> SubscribeOnce<T>(
            ConsumerEvtHandler<T> action,
            int priority = 0,
            string debugName = null
        ) where T : Evt<T> {
#if DEBUG
            debugName ??= EventReactorInfo.GetDebugName(action);
#endif
            return EventReactor<T>.Shared.SubscribeOnce(action, priority, debugName);
        }

        public static HandlerRegistration<T> SubscribeStream<T>(
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

        public static EventReactor<TSelf> Reactor => EventReactor<TSelf>.Shared;

        public static HandlerRegistration<TSelf> Subscribe(
            EvtHandler<TSelf> action,
            int priority = 0,
            string debugName = null
        ) => Evt.Subscribe(action, priority, debugName);

        public static HandlerRegistration<TSelf> Subscribe(
            ConsumerEvtHandler<TSelf> action,
            int priority = 0,
            string debugName = null
        ) => Evt.Subscribe(action, priority, debugName);

        public static HandlerRegistration<TSelf> SubscribeOnce(
            EvtHandler<TSelf> action,
            int priority = 0,
            string debugName = null
        ) => Evt.SubscribeOnce(action, priority, debugName);

        public static HandlerRegistration<TSelf> SubscribeOnce(
            ConsumerEvtHandler<TSelf> action,
            int priority = 0,
            string debugName = null
        ) => Evt.SubscribeOnce(action, priority, debugName);

        public static HandlerRegistration<TSelf> SubscribeStream(
            StreamEvtHandler<TSelf> action,
            int priority = 0,
            string debugName = null
        ) => Evt.SubscribeStream(action, priority, debugName);
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

  public interface IAsyncChainEvt {}

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
            foreach (var action in _chain) {
                if (action == null) continue;
                try { await action.Invoke(); } catch (Exception e) {
                    Debug.LogError($"Exception in async chain of event {GetType().Name}");
                    Debug.LogException(e);
                }
            }

            return referenced;
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