using System;
using System.Collections.Generic;
using UnityEngine;

namespace HELIX.Context.Tests.Fixtures {
  public sealed class RecordingComponent : IComponent {
    private readonly ICollection<string> _trace;
    private readonly string _name;

    public RecordingComponent(ICollection<string> trace, string name) {
      _trace = trace;
      _name = name;
    }

    public RuntimeComponentData ComponentBinding { get; } = new();
    public void LoadComponent(ComponentLoadContext context) => _trace.Add($"{_name}:load");
    public void UnloadComponent() => _trace.Add($"{_name}:unload");
  }

  public sealed class RecordingConsumerComponent : IComponent {
    private readonly ICollection<string> _trace;

    public RecordingConsumerComponent(ICollection<string> trace) => _trace = trace;
    public RuntimeComponentData ComponentBinding { get; } = new();
    public void LoadComponent(ComponentLoadContext context) => _trace.Add("consumer:load");
    public void UnloadComponent() => _trace.Add("consumer:unload");
  }

  public sealed class InjectedTestComponent : MonoBehaviour, IComponent {
    public int loadCount;
    public RuntimeComponentData ComponentBinding { get; } = new();
    public void LoadComponent(ComponentLoadContext context) => loadCount++;
  }

  public sealed class ContributedComponent : IComponent {
    public RuntimeComponentData ComponentBinding { get; } = new();
  }

  public sealed class TestScope : IScope { }

  public class TestScopeHandler : ScopeHandler<TestScope> {
    private readonly IComponent _component;
    public TestScopeHandler(IComponent component) => _component = component;

    public override IEnumerable<IComponent> DiscoverComponents(ManagedContainer container, ManagedScope scope) {
      yield return _component;
    }
  }

  public sealed class RegistrarScopeHandler : TestScopeHandler, IComponent {
    public RegistrarScopeHandler(IComponent component) : base(component) { }
    public RuntimeComponentData ComponentBinding { get; } = new();
  }

  [Component(typeof(ApplicationScope), optional: true)]
  public partial class GeneratedBindTestComponent {
    [Bind] public GeneratedBindTestValue value = new();
    [Bind(required: false, proxied: true)] public GeneratedBindTestValue proxiedValue => null;
  }

  public sealed class GeneratedBindTestValue { }

  [Component(typeof(ApplicationScope), optional: true)]
  public partial class GeneratedListInjectionComponent {
    [Inject(required: true)] public IReadOnlyList<IProvider> providers;
  }

  [AttributeUsage(AttributeTargets.Class)]
  [MixinExpression(MixinOn.ConfigureComponent, 0, @"
@CODE registration.Condition(context => true);
")]
  public sealed class AlwaysEnabledAttribute : Attribute { }

  [Component(typeof(ApplicationScope), optional: true)]
  [AlwaysEnabled]
  public partial class GeneratedConditionalComponent { }
}
