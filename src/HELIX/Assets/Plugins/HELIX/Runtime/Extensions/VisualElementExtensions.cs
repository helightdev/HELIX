using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HELIX.Types;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using IEventHandler = HELIX.Types.IEventHandler;

namespace HELIX.Extensions {
    public static partial class VisualElementExtensions {
        public static T AddClasses<T>(this T element, params string[] classNames) where T : VisualElement {
            foreach (var className in classNames) {
                element.AddToClassList(className);
            }

            return element;
        }

        public static T WithClasses<T>(this T element, params string[] classNames) where T : VisualElement {
            element.ClearClassList();
            return element.AddClasses(classNames);
        }

        public static T NoPaddingAndMargin<T>(this T element) where T : VisualElement => element.NoBorder().NoMargin();

        public static T WithStyle<T>(this T element, Func<IStyle, UpdateStyle> updater) where T : VisualElement {
            updater(element.style);
            return element;
        }

        public static T TextColor<T>(this T element, Color color) where T : VisualElement {
            element.style.color = color;
            return element;
        }

        public static T BackgroundColor<T>(this T element, Color color) where T : VisualElement {
            element.style.backgroundColor = color;
            return element;
        }

        public static T Transitions<T>(this T element, params Transition[] transitions) where T : VisualElement {
            var names = new List<StylePropertyName>();
            var durations = new List<TimeValue>();
            var easings = new List<EasingFunction>();
            var delays = new List<TimeValue>();
            foreach (var transition in transitions) {
                names.Add(transition.property);
                durations.Add(transition.duration);
                easings.Add(transition.easing);
                delays.Add(transition.delay);
            }

            element.style.transitionProperty = names;
            element.style.transitionDuration = durations;
            element.style.transitionTimingFunction = easings;
            element.style.transitionDelay = delays;
            return element;
        }

        public static T WithCallback<T>(this T element, IEventHandler action) where T : VisualElement {
            action.Register(element);
            return element;
        }
        
        public static T WithCallback<T>(this T element, params IEventHandler[] actions) where T : VisualElement {
            foreach (var action in actions) {
                action.Register(element);
            }
            return element;
        }

        public static T RegisterOnAttachRetroactively<T>(this T element, Action<T> action) where T : VisualElement {
            element.RegisterCallback<AttachToPanelEvent>(_ => action(element));
            if (element.panel != null) action(element);
            return element;
        }

        public static T BindDisposable<T>(this T element, IDisposable disposable) where T : VisualElement {
            element.RegisterCallback<DetachFromPanelEvent>(_ => disposable.Dispose());
            return element;
        }

        public static T BindDisposable<T>(this T element, Func<IDisposable> disposable) where T : VisualElement {
            var buffer = default(IDisposable);
            element.RegisterOnAttachRetroactively(_ => {
                buffer?.Dispose();
                buffer = disposable();
            });
            element.RegisterCallback<DetachFromPanelEvent>(_ => buffer?.Dispose());
            return element;
        }
    }
}