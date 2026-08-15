using System.Collections.Generic;
using HELIX.Context.Events;
using UnityEngine;

namespace HELIX.Context {
  public readonly struct EventHandlerList {
    public readonly List<HandlerRegistration> registrations;

    public EventHandlerList(List<HandlerRegistration> registrations) {
      this.registrations = registrations;
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
            args.Chain += async () => handler(closed);
          },
          priority
        )
      );
    }

    public delegate Awaitable AsyncHandler<in T>(T arg) where T : AsyncChainEvt<T>;

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

    public void Register(HandlerRegistration registration) {
      registrations.Add(registration);
    }

    public void Unregister(HandlerRegistration registration) {
      registration.Dispose();
      registrations.Remove(registration);
    }

    public void UnregisterAll() {
      foreach (var registration in registrations) {
        registration.Dispose();
      }
      registrations.Clear();
    }

    public static EventHandlerList Create() {
      return new EventHandlerList(new List<HandlerRegistration>());
    }
  }
}