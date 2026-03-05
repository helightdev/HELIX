using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    public abstract class WidgetFactorySlot : VisualElement {
        protected readonly BaseWidget widget;

        protected WidgetFactorySlot(BaseWidget widget) {
            this.widget = widget;
        }

        public abstract bool HasElement { get; }

        public abstract void Destroy();
        public abstract void Recreate();
        public abstract void TryCreate();
        public abstract void ApplyReferenceFromTheme();
    }

    public interface IPublicWidgetFactorySlot<T> where T : VisualElement {
        WidgetFactoryReference<T> Reference { get; set; }
        T Element { get; }
        bool HasElement { get; }
        void SetMapped(WidgetFactory<T> value);
        TMapped GetMapped<TMapped>() where TMapped : WidgetFactory<T>;
    }

    public class WidgetFactorySlot<T> : WidgetFactorySlot, IPublicWidgetFactorySlot<T> where T : VisualElement {
        public delegate void OnElementCreatedDelegate(T element);

        public delegate void OnElementDestroyedDelegate(T element);

        private readonly ThemeProperty<WidgetFactory<T>> _themeProperty;
        private WidgetFactory _factory;
        private WidgetFactory _fallback;
        private bool _hasExplicitReference;
        private object _mappedValue;
        private WidgetFactoryReference<T> _reference;

        public WidgetFactorySlot(BaseWidget widget) : base(widget) { }

        public WidgetFactorySlot(BaseWidget widget, ThemeProperty<WidgetFactory<T>> themeProperty) : base(widget) {
            _themeProperty = themeProperty;
        }

        public WidgetFactoryReference<T> Reference {
            get => _reference;
            set {
                if (Equals(_reference, value)) return;
                _reference = value;
                ApplyReference(value);
            }
        }

        public void SetMapped(WidgetFactory<T> value) {
            _mappedValue = value;
            Reference = value == null ? default : new WidgetFactoryReference<T>(value);
        }

        public TMapped GetMapped<TMapped>() where TMapped : WidgetFactory<T> {
            if (_mappedValue is TMapped mapped) return mapped;
            return null;
        }

        public T Element { get; private set; }

        public override bool HasElement => Element != null;

        public event OnElementCreatedDelegate OnElementCreated;
        public event OnElementDestroyedDelegate OnElementDestroyed;

        private void ApplyReference(WidgetFactoryReference<T> factoryReference) {
            var factory = factoryReference.LookupFactory();

            // Try resolving from parent style on late unset
            if (factory == null) {
                var parentStyle = widget.parent?.customStyle;
                if (parentStyle != null && _themeProperty != null)
                    factory = WidgetThemeProvider.Resolve(widget.ThemeProvider, _themeProperty);
                factory ??= _fallback;
                _hasExplicitReference = false;
            } else _hasExplicitReference = true;

            if (factory == null || factory == _factory) return;
            _factory = factory;
            if (HasElement) Recreate();
        }

        public override void ApplyReferenceFromTheme() {
            if (_hasExplicitReference) return;
            var factory = _fallback;
            if (_themeProperty != null) factory = WidgetThemeProvider.Resolve(widget.ThemeProvider, _themeProperty);

            if (factory == null || Equals(factory, _factory)) return;
            _factory = factory;
            _hasExplicitReference = false;
            if (HasElement) Recreate();
        }

        public override void TryCreate() {
            if (Element != null) return;
            Instantiate();
        }

        public override void Recreate() {
            if (_factory == null) return;
            Destroy();
            Instantiate();
        }

        private void Instantiate() {
            if (_factory == null) return;
            var element = _factory.Create(widget) as T;
            if (element == null) {
                Debug.LogError($"Factory {_factory.GetType().Name} failed to create element of type {typeof(T).Name}");
                return;
            }

            OnElementCreated?.Invoke(element);
            Add(element);
            Element = element;
        }

        public override void Destroy() {
            if (Element == null) return;
            OnElementDestroyed?.Invoke(Element);
            Remove(Element);
            Element = null;
        }

        public void SetFallback(WidgetFactory fallback) {
            _fallback = fallback;
        }
    }
}