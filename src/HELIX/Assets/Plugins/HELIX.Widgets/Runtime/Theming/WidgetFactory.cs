using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Theming {

    [RequireDerived]
    public abstract class WidgetFactory {
        [RequiredMember]
        public abstract VisualElement Create(BaseWidget parentWidget);
    }

    [RequireDerived]
    public abstract class WidgetFactory<T> : WidgetFactory where T : VisualElement {
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

    [Serializable]
    public struct WidgetFactoryReference<T> where T : VisualElement {
        public string factoryName;
        
        public WidgetFactoryReference(string factoryName) {
            this.factoryName = factoryName;
            
        }
        
        public static implicit operator WidgetFactoryReference<T>(string factoryName) {
            return new WidgetFactoryReference<T>(factoryName);
        }
        
        public static implicit operator WidgetFactoryReference<T>(Type factoryType) {
            return new WidgetFactoryReference<T>(factoryType.FullName);
        }
        
        public static implicit operator string(WidgetFactoryReference<T> reference) {
            return reference.factoryName;
        }
    }

    public abstract class WidgetFactoryEventBase<T> : EventBase<T> where T : WidgetFactoryEventBase<T>, new() {
        public BaseWidget parentWidget;
    }
    
    public class WidgetFactoryWidgetCreatedEvent : WidgetFactoryEventBase<WidgetFactoryWidgetCreatedEvent> { }
    public class WidgetFactoryWidgetDestroyedEvent : WidgetFactoryEventBase<WidgetFactoryWidgetDestroyedEvent> { }
}