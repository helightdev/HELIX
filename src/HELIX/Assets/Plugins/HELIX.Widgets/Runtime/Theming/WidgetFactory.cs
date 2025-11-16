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
    public abstract class WidgetFactory<T> : WidgetFactory where T : VisualElement {}

    [Serializable]
    public struct WidgetFactoryReference<T> {
        public string factoryName;
        
        public WidgetFactoryReference(string factoryName) {
            this.factoryName = factoryName;
            
        }
    }

    public abstract class WidgetFactoryEventBase<T> : EventBase<T> where T : WidgetFactoryEventBase<T>, new() {
        public BaseWidget parentWidget;
    }
    
    public class WidgetFactoryWidgetCreatedEvent : WidgetFactoryEventBase<WidgetFactoryWidgetCreatedEvent> { }
    public class WidgetFactoryWidgetDestroyedEvent : WidgetFactoryEventBase<WidgetFactoryWidgetDestroyedEvent> { }
}