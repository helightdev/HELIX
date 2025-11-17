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
        public abstract void ApplyReferenceFromStyle(ICustomStyle givenStyle);
    }
    
    public class WidgetFactorySlot<T> : WidgetFactorySlot where T : VisualElement {
        private WidgetFactory _factory;
        private T _element;
        private bool _hasAppliedReference;
        private WidgetFactoryReference<T> _reference;
        private readonly ThemeProperty<WidgetFactory<T>> _themeProperty;
        
        public event OnElementCreatedDelegate OnElementCreated;
        public event OnElementDestroyedDelegate OnElementDestroyed;
        
        public WidgetFactorySlot(BaseWidget widget) : base(widget) { }

        public WidgetFactorySlot(BaseWidget widget, ThemeProperty<WidgetFactory<T>> themeProperty) : base(widget) {
            _themeProperty = themeProperty;
        }

        public WidgetFactoryReference<T> Reference {
            get => _reference;
            set {
                _reference = value;
                ApplyReference(value);
            }
        }
        
        public T Element => _element;
        public override bool HasElement => _element != null;

        private void ApplyReference(WidgetFactoryReference<T> factoryReference) {
            var factory = RuntimeReflectionThemeLookup.GetFactory(factoryReference.factoryName);
            if (factory == null || factory == _factory) return;
            _factory = factory;
            _hasAppliedReference = true;
        }

        public override void ApplyReferenceFromStyle(ICustomStyle givenStyle) {
            if (_hasAppliedReference) return;
            if (_themeProperty == null) return;
            _themeProperty.Resolve(givenStyle, out var factory);
            if (factory == null || Equals(factory, _factory)) return;
            _factory = factory;
        }
        
        public override void TryCreate() {
            if (_element != null) return;
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
            using (var evt = WidgetFactoryWidgetCreatedEvent.GetPooled()) {
                evt.parentWidget = widget;
                element.SendEvent(evt);
            }
            
            Add(element);
            _element = element;
        }

        public override void Destroy() {
            if (_element == null) return;
            using (var evt = WidgetFactoryWidgetDestroyedEvent.GetPooled()) {
                evt.parentWidget = widget;
                _element.SendEvent(evt);
            }
            OnElementDestroyed?.Invoke(_element);
            Remove(_element);
            _element = null;
        }
        
        
        public delegate void OnElementCreatedDelegate(T element);
        public delegate void OnElementDestroyedDelegate(T element);
    }
}