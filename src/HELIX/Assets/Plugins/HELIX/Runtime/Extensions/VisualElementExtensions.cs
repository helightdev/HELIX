using System;
using System.Collections.Generic;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;
using IEventHandler = HELIX.Types.IEventHandler;

namespace HELIX.Extensions {
    public static partial class VisualElementExtensions {
        public static T AddTo<T>(this T element, VisualElement target) where T : VisualElement {
            target.Add(element);
            return element;
        }

        public static T AddTo<T>(this T element, VisualElement.Hierarchy target) where T : VisualElement {
            target.Add(element);
            return element;
        }

        public static T WithAdded<T>(this T target, VisualElement element) where T : VisualElement {
            target.Add(element);
            return target;
        }

        public static T AddClasses<T>(this T element, params string[] classNames) where T : VisualElement {
            foreach (var className in classNames) element.AddToClassList(className);

            return element;
        }

        public static T WithClasses<T>(this T element, params string[] classNames) where T : VisualElement {
            element.ClearClassList();
            return element.AddClasses(classNames);
        }

        public static T WithName<T>(this T element, string name) where T : VisualElement {
            element.name = name;
            return element;
        }

        public static T Display<T>(this T element, bool display) where T : VisualElement {
            element.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
            return element;
        }

        public static T Visible<T>(this T element, bool visible) where T : VisualElement {
            element.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
            return element;
        }

        public static T Opacity<T>(this T element, float opacity) where T : VisualElement {
            element.style.opacity = opacity;
            return element;
        }

        public static T Pickable<T>(this T element, bool pickable) where T : VisualElement {
            element.pickingMode = pickable ? PickingMode.Position : PickingMode.Ignore;
            return element;
        }

        public static T NoPaddingAndMargin<T>(this T element) where T : VisualElement {
            return element.NoPadding().NoMargin();
        }

        public static T WithStyle<T>(this T element, Func<IStyle, UpdateStyle> updater) where T : VisualElement {
            updater(element.style);
            return element;
        }

        public static T WithStyle<T>(this T element, Action<IStyle> updater) where T : VisualElement {
            updater(element.style);
            return element;
        }

        public static T TextColor<T>(this T element, Color color) where T : VisualElement {
            element.style.color = color;
            return element;
        }

        public static T TextSize<T>(this T element, float size) where T : VisualElement {
            element.style.fontSize = size;
            return element;
        }

        public static T BackgroundColor<T>(this T element, Color color) where T : VisualElement {
            element.style.backgroundColor = color;
            return element;
        }

        public static T Image<T>(
            this T element,
            Background background,
            BackgroundSizeType? fit = null,
            BackgroundSize? size = null,
            Color? tintColor = null
        ) where T : VisualElement {
            element.style.backgroundImage = background;
            if (size.HasValue) element.style.backgroundSize = size.Value;
            else if (fit.HasValue) element.style.backgroundSize = new BackgroundSize(fit.Value);
            if (tintColor.HasValue) element.style.unityBackgroundImageTintColor = tintColor.Value;
            return element;
        }

        public static T BackgroundImage<T>(this T element, Background background) where T : VisualElement {
            element.style.backgroundImage = background;
            return element;
        }

        public static T BackgroundImageScaling<T>(this T element, BackgroundSize size) where T : VisualElement {
            element.style.backgroundSize = size;
            return element;
        }

        public static T BackgroundImageScaling<T>(this T element, BackgroundSizeType size) where T : VisualElement {
            element.style.backgroundSize = new BackgroundSize(size);
            return element;
        }

        public static T BackgroundImageColor<T>(this T element, Color color) where T : VisualElement {
            element.style.unityBackgroundImageTintColor = color;
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
            foreach (var action in actions) action.Register(element);

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
                }
            );
            element.RegisterCallback<DetachFromPanelEvent>(_ => buffer?.Dispose());
            return element;
        }

        public static Length Percent(this int value) {
            return new Length(value, LengthUnit.Percent);
        }

        public static Length Percent(this float value) {
            return new Length(value, LengthUnit.Percent);
        }

        public static Length NormalizedPercent(this int value) {
            return new Length(value * 100f, LengthUnit.Percent);
        }

        public static Length NormalizedPercent(this float value) {
            return new Length(value * 100f, LengthUnit.Percent);
        }
    }
}