using System;
using System.Collections.Generic;
using HELIX.Widgets.Elements;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {
    [UxmlObject]
    [RequireDerived]
    public abstract partial class ElementFactory {

        [RequiredMember]
        public abstract VisualElement Create(BaseElement parentElement);

    }

    public class FixedElementFactory<T> : ElementFactory<T> where T : VisualElement {

        private readonly T _instance;

        public FixedElementFactory(T instance) {
            _instance = instance;
        }

        public override VisualElement Create(BaseElement parentElement) {
            return _instance;
        }

        protected bool Equals(FixedElementFactory<T> other) {
            return base.Equals(other) && EqualityComparer<T>.Default.Equals(_instance, other._instance);
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FixedElementFactory<T>)obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(base.GetHashCode(), _instance);
        }

    }

    public class InlineElementFactory<T> : ElementFactory<T> where T : VisualElement {

        private readonly Func<BaseElement, T> _factoryFunc;

        public InlineElementFactory(Func<BaseElement, T> factoryFunc) {
            _factoryFunc = factoryFunc;
        }

        public InlineElementFactory(Func<T> factoryFunc) {
            _factoryFunc = _ => factoryFunc();
        }

        public override VisualElement Create(BaseElement parentElement) {
            return _factoryFunc(parentElement);
        }

        protected bool Equals(InlineElementFactory<T> other) {
            return base.Equals(other) && Equals(_factoryFunc, other._factoryFunc);
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((InlineElementFactory<T>)obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(base.GetHashCode(), _factoryFunc);
        }

    }

    [Serializable]
    public class UxmlAssetElementFactory<T> : ElementFactory<T> where T : VisualElement {

        public VisualTreeAsset asset;

        public UxmlAssetElementFactory() { }

        public UxmlAssetElementFactory(VisualTreeAsset asset) {
            this.asset = asset;
        }

        public override VisualElement Create(BaseElement parentElement) {
            var container = asset.CloneTree();
            if (container is T) return container;
            return container.Q<T>();
        }

        protected bool Equals(UxmlAssetElementFactory<T> other) {
            return base.Equals(other) && Equals(asset, other.asset);
        }

    }

    [UxmlObject]
    [RequireDerived]
    public abstract partial class ElementFactory<T> : ElementFactory where T : VisualElement {

        protected bool Equals(ElementFactory<T> other) {
            return GetType() == other.GetType();
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ElementFactory<T>)obj);
        }

        public override int GetHashCode() {
            return GetType().GetHashCode();
        }

    }

    [UxmlObject]
    public abstract partial class VisualElementFactory : ElementFactory<VisualElement> {

        public static VisualElementFactory Warp(ElementFactory<VisualElement> element) {
            return new WrappedVisualElementFactory(element);
        }

    }

    public class WrappedVisualElementFactory : VisualElementFactory {

        private readonly ElementFactory<VisualElement> _innerFactory;

        public WrappedVisualElementFactory(ElementFactory<VisualElement> innerFactory) {
            _innerFactory = innerFactory;
        }

        public override VisualElement Create(BaseElement parentElement) {
            return _innerFactory.Create(parentElement);
        }

    }

    public interface IWidgetFactoryReference : IMaybeThemeValue {

        ElementFactory LookupFactory();

    }

    [Serializable]
    public struct ElementFactoryReference<T> : IWidgetFactoryReference, IMaybeThemeValue<T>,
        IEquatable<ElementFactoryReference<T>>
        where T : VisualElement {

        public string factoryName;

        [NonSerialized]
        public ElementFactory<T> resolved;

        public ElementFactoryReference(string factoryName) {
            this.factoryName = factoryName;
            resolved = null;
        }

        public ElementFactoryReference(ElementFactory<T> resolved) {
            factoryName = null;
            this.resolved = resolved;
        }

        public static implicit operator ElementFactoryReference<T>(string factoryName) {
            return new ElementFactoryReference<T>(factoryName);
        }

        public static implicit operator ElementFactoryReference<T>(Type factoryType) {
            return new ElementFactoryReference<T>(factoryType.FullName);
        }

        public static implicit operator ElementFactoryReference<T>(ElementFactory<T> factory) {
            return new ElementFactoryReference<T>(factory);
        }

        public static implicit operator string(ElementFactoryReference<T> reference) {
            return reference.factoryName;
        }

        public static implicit operator ElementFactory(ElementFactoryReference<T> reference) {
            return reference.LookupFactory();
        }

        public static implicit operator ElementFactory<T>(ElementFactoryReference<T> reference) {
            return reference.LookupFactory() as ElementFactory<T>;
        }

        public ElementFactory LookupFactory() {
            if (resolved != null) return resolved;
            return string.IsNullOrEmpty(factoryName) ? null : RuntimeReflectionThemeLookup.GetFactory(factoryName);
        }

        public bool Equals(ElementFactoryReference<T> other) {
            return factoryName == other.factoryName && Equals(resolved, other.resolved);
        }

        public override bool Equals(object obj) {
            return obj is ElementFactoryReference<T> other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(factoryName, resolved);
        }

        public bool TryGetThemeValue(out object result) {
            var factory = LookupFactory();
            if (factory != null) {
                result = factory;
                return true;
            }

            result = null;
            return false;
        }

    }
}