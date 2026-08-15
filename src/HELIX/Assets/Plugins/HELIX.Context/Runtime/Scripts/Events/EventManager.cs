using System;
using System.Collections.Generic;

namespace HELIX.Context {
    public class EventManager {

        public static EventManager Instance = new();

        public readonly Dictionary<Type, IEventReactor> reactors = new();

        /// <summary>
        ///     Registers an event reactor.
        /// </summary>
        /// <typeparam name="T">type of the reactor that shall be subscribed</typeparam>
        public EventReactor<T> RegisterEvent<T>() where T : Evt<T> {
            if (reactors.TryGetValue(typeof(T), out var eventReactor)) return (EventReactor<T>)eventReactor;

            var reactor = new EventReactor<T>();
            reactors[typeof(T)] = reactor;
            return reactor;
        }

        /// <summary>
        ///     Registers an event reactor.
        /// </summary>
        public void RegisterEvent(IEventReactor reactor) {
            if (reactors.ContainsKey(reactor.TypeDelegate())) return;

            reactors[reactor.TypeDelegate()] = reactor;
        }

        /// <summary>
        ///     Unregisters an event reactor.
        /// </summary>
        /// <param name="eventType">type of the reactor which shall be unsubscribed</param>
        public void UnregisterEvent(Type eventType) {
            reactors.Remove(eventType);
        }

        /// <summary>
        ///     Unregisters an event reactor.
        /// </summary>
        public void UnregisterEvent(IEventReactor reactor) {
            reactors.Remove(reactor.TypeDelegate());
        }

        /// <summary>
        ///     Unregisters an event reactor.
        /// </summary>
        /// <typeparam name="T">type of the reactor which shall be unsubscribed</typeparam>
        public void UnregisterEvent<T>() {
            reactors.Remove(typeof(T));
        }

        /// <summary>
        ///     Retrieves an event reactor.
        /// </summary>
        /// <typeparam name="T">type of the reactor</typeparam>
        public EventReactor<T> Get<T>() where T : Evt<T> {
            return (EventReactor<T>)reactors[typeof(T)];
        }

        /// <summary>
        ///     Retrieves an event reactor.
        /// </summary>
        /// <param name="type">type of the reactor</param>
        public IEventReactor GetUnsafe(Type type) {
            return reactors[type];
        }

    }
}