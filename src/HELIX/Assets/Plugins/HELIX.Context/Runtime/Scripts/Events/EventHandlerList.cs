using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace HELIX.Context {
  public interface IEventListener {
    EventHandlerList HandlerList { get; }
  }

  public readonly struct EventHandlerList {
    public readonly List<HandlerRegistration> registrations;

    public EventHandlerList(List<HandlerRegistration> registrations) {
      this.registrations = registrations;
    }

    public bool HasHandlerFor<T>() where T : Evt<T> {
      foreach (var registration in registrations)
        if (registration is HandlerRegistration<T> typedRegistration && !typedRegistration.IsDisposed)
          return true;
      return false;
    }

    public void RaiseLocal<T>(T evt) where T : Evt<T> {
      for (var i = 0; i < registrations.Count; i++) {
        if (registrations[i] is not HandlerRegistration<T> registration) continue;
        if (registration.IsDisposed) continue;
        registration.Handler?.Invoke(ref evt);
      }

      CleanupRegistrations();
    }

    public async UniTask<T> RaiseLocalAsync<T>(T evt) where T : AsyncChainEvt<T> {
      for (var i = 0; i < registrations.Count; i++) {
        if (registrations[i] is not HandlerRegistration<T> registration) continue;
        if (registration.IsDisposed) continue;
        registration.Handler?.Invoke(ref evt);
      }
      var result = await AsyncChainEvt<T>.ExecuteAsyncChain(evt);
      CleanupRegistrations();
      return result;
    }

    public void CleanupRegistrations() {
      for (var i = registrations.Count - 1; i >= 0; i--)
        if (registrations[i].IsDisposed)
          registrations.RemoveAt(i);
    }

    public void RegisterAsync<T>(AsyncHandler<T> func, int priority) where T : AsyncChainEvt<T> {
      Register(
        EventReactor<T>.Shared.Subscribe(
          delegate(ref T args) {
            var closed = args;
            args.Chain += async () => await func(closed);
          },
          priority
        )
      );
    }

    public void RegisterAsync<T>(ConsumerEvtHandler<T> handler, int priority) where T : AsyncChainEvt<T> {
      Register(
        EventReactor<T>.Shared.Subscribe(
          delegate(ref T args) {
            var closed = args;
            args.Chain += () => {
              handler(closed);
              return UniTask.CompletedTask;
            };
          },
          priority
        )
      );
    }

    public delegate UniTask AsyncHandler<in T>(T arg) where T : AsyncChainEvt<T>;

    public void Register<T>(EvtHandler<T> handler, int priority) where T : Evt<T> {
      Register(
        EventReactor<T>.Shared.Subscribe(handler, priority)
      );
    }

    public void Register<T>(ConsumerEvtHandler<T> handler, int priority) where T : Evt<T> {
      Register(
        EventReactor<T>.Shared.Subscribe(handler, priority)
      );
    }

    public void RegisterInjector<T>(TypeKey key, Action<T> applicator) where T : class {
      Register((ref ManagedLoadEvent args) => { applicator(args.Scope.Resolve(key) as T); }, 0);
    }

    public void Register(HandlerRegistration registration) {
      registrations.Add(registration);
    }

    public void Unregister(HandlerRegistration registration) {
      registration.Dispose();
      registrations.Remove(registration);
    }

    public void UnregisterAll() {
      foreach (var registration in registrations) registration.Dispose();
      registrations.Clear();
    }

    public static EventHandlerList Create() {
      return new EventHandlerList(new List<HandlerRegistration>());
    }
  }
}