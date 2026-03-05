using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    [UxmlObject, RequireDerived]
    public abstract partial class WidgetFactory {
        [RequiredMember]
        public abstract VisualElement Create(BaseWidget parentWidget);
    }

    public class FixedWidgetFactory<T> : WidgetFactory<T> where T : VisualElement {
        private readonly T _instance;

        public FixedWidgetFactory(T instance) {
            _instance = instance;
        }

        public override VisualElement Create(BaseWidget parentWidget) {
            return _instance;
        }

        protected bool Equals(FixedWidgetFactory<T> other) {
            return base.Equals(other) && EqualityComparer<T>.Default.Equals(_instance, other._instance);
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FixedWidgetFactory<T>)obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(base.GetHashCode(), _instance);
        }
    }

    public class InlineWidgetFactory<T> : WidgetFactory<T> where T : VisualElement {
        private readonly Func<BaseWidget, T> _factoryFunc;

        public InlineWidgetFactory(Func<BaseWidget, T> factoryFunc) {
            _factoryFunc = factoryFunc;
        }

        public InlineWidgetFactory(Func<T> factoryFunc) {
            _factoryFunc = _ => factoryFunc();
        }

        public override VisualElement Create(BaseWidget parentWidget) {
            return _factoryFunc(parentWidget);
        }

        protected bool Equals(InlineWidgetFactory<T> other) {
            return base.Equals(other) && Equals(_factoryFunc, other._factoryFunc);
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((InlineWidgetFactory<T>)obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(base.GetHashCode(), _factoryFunc);
        }
    }

    [Serializable]
    public class UxmlAssetWidgetFactory<T> : WidgetFactory<T> where T : VisualElement {
        public VisualTreeAsset asset;

        public UxmlAssetWidgetFactory() { }

        public UxmlAssetWidgetFactory(VisualTreeAsset asset) {
            this.asset = asset;
        }

        public override VisualElement Create(BaseWidget parentWidget) {
            var container = asset.CloneTree();
            if (container is T) return container;
            return container.Q<T>();
        }

        protected bool Equals(UxmlAssetWidgetFactory<T> other) {
            return base.Equals(other) && Equals(asset, other.asset);
        }
    }

    [UxmlObject, RequireDerived]
    public abstract partial class WidgetFactory<T> : WidgetFactory where T : VisualElement {
        protected bool Equals(WidgetFactory<T> other) {
            return GetType() == other.GetType();
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((WidgetFactory<T>)obj);
        }

        public override int GetHashCode() {
            return GetType().GetHashCode();
        }
    }

    [UxmlObject]
    public abstract partial class VisualElementWidgetFactory : WidgetFactory<VisualElement> { }

    public interface IWidgetFactoryReference {
        WidgetFactory LookupFactory();
    }

    [Serializable]
    public struct WidgetFactoryReference<T> : IWidgetFactoryReference, IEquatable<WidgetFactoryReference<T>> where T : VisualElement {
        public string factoryName;

        [NonSerialized]
        public WidgetFactory<T> resolved;

        public WidgetFactoryReference(string factoryName) {
            this.factoryName = factoryName;
            resolved = null;
        }

        public WidgetFactoryReference(WidgetFactory<T> resolved) {
            factoryName = null;
            this.resolved = resolved;
        }

        public static implicit operator WidgetFactoryReference<T>(string factoryName) {
            return new WidgetFactoryReference<T>(factoryName);
        }

        public static implicit operator WidgetFactoryReference<T>(Type factoryType) {
            return new WidgetFactoryReference<T>(factoryType.FullName);
        }

        public static implicit operator WidgetFactoryReference<T>(WidgetFactory<T> factory) {
            return new WidgetFactoryReference<T>(factory);
        }

        public static implicit operator string(WidgetFactoryReference<T> reference) {
            return reference.factoryName;
        }

        public static implicit operator WidgetFactory(WidgetFactoryReference<T> reference) {
            return reference.LookupFactory();
        }

        public static implicit operator WidgetFactory<T>(WidgetFactoryReference<T> reference) {
            return reference.LookupFactory() as WidgetFactory<T>;
        }

        public WidgetFactory LookupFactory() {
            if (resolved != null) return resolved;
            return string.IsNullOrEmpty(factoryName) ? null : RuntimeReflectionThemeLookup.GetFactory(factoryName);
        }

        public bool Equals(WidgetFactoryReference<T> other) {
            return factoryName == other.factoryName && Equals(resolved, other.resolved);
        }

        public override bool Equals(object obj) {
            return obj is WidgetFactoryReference<T> other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(factoryName, resolved);
        }
    }
}