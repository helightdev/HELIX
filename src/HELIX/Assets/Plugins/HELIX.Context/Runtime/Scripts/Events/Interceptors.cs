using System;
using UnityEngine;

namespace HELIX.Context {
  public delegate EvtHandler<T> EventInterceptor<T>(EvtHandler<T> handler) where T : Evt<T>;


  public static class EvtInterceptors {
    public static EventInterceptor<T> Throttle<T>(float time) where T : Evt<T> {
      var lastTime = -Mathf.Infinity;
      return handler => (ref T evt) => {
        if (!(Time.time - lastTime >= time)) return;
        lastTime = Time.time;
        handler?.Invoke(ref evt);
      };
    }

    public static EventInterceptor<T> ActiveAndEnabled<T>(Behaviour component) where T : Evt<T> {
      return handler => (ref T evt) => {
        if (!component.isActiveAndEnabled) return;
        handler?.Invoke(ref evt);
      };
    }

    public static EventInterceptor<T> LogInterceptor<T>(Action<string> logger) where T : Evt<T> {
      return handler => (ref T evt) => {
        logger($"Event {typeof(T).Name} triggered with data: {evt}");
        handler?.Invoke(ref evt);
      };
    }

    public static EventInterceptor<T> Chain<T>(EventInterceptor<T> first, EventInterceptor<T> next)
    where T : Evt<T> {
      if (first == null) return next;
      return next == null ? first : handler => first(next(handler));
    }
  }
}