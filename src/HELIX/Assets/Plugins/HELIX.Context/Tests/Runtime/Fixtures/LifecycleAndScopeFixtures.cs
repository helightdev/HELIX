using System;
using System.Collections.Generic;
using UnityEngine;

namespace HELIX.Context.Tests.Fixtures {
  public sealed class RecordingComponent : IManaged {
    private readonly ICollection<string> _trace;
    private readonly string _name;

    public RecordingComponent(ICollection<string> trace, string name) {
      _trace = trace;
      _name = name;
    }

    public RuntimeManagedData managed { get; } = new();
    public void LoadManaged(ManagedLoadContext context) => _trace.Add($"{_name}:load");
    public void UnloadManaged() => _trace.Add($"{_name}:unload");
  }

  public sealed class RecordingConsumerComponent : IManaged {
    private readonly ICollection<string> _trace;

    public RecordingConsumerComponent(ICollection<string> trace) => _trace = trace;
    public RuntimeManagedData managed { get; } = new();
    public void LoadManaged(ManagedLoadContext context) => _trace.Add("consumer:load");
    public void UnloadManaged() => _trace.Add("consumer:unload");
  }

  public sealed class InjectedTestComponent : MonoBehaviour, IManaged {
    public int loadCount;
    public RuntimeManagedData managed { get; } = new();
    public void LoadManaged(ManagedLoadContext context) => loadCount++;
  }

  public sealed class ContributedComponent : IManaged {
    public RuntimeManagedData managed { get; } = new();
  }

  public sealed class TestScope : IScope { }

  public class TestScopeHandler : ScopeHandler<TestScope> {
    private readonly IManaged _component;
    public TestScopeHandler(IManaged component) => _component = component;

    public override IEnumerable<IManaged> DiscoverComponents(ManagedContainer container, ManagedScope scope) {
      yield return _component;
    }
  }

  public sealed class RegistrarScopeHandler : TestScopeHandler, IManaged {
    public RegistrarScopeHandler(IManaged component) : base(component) { }
    public RuntimeManagedData managed { get; } = new();
  }

  [Managed(typeof(ApplicationScope), optional: true)]
  public partial class GeneratedBindTestComponent {
    [Bind] public GeneratedBindTestValue value = new();
    [Bind(required: false, proxied: true)] public GeneratedBindTestValue proxiedValue => null;
  }

  public sealed class GeneratedBindTestValue { }

  [Managed(typeof(ApplicationScope), optional: true)]
  public partial class GeneratedListInjectionComponent {
    [Inject(required: true)] public IReadOnlyList<IProvider> providers;
  }

  [AttributeUsage(AttributeTargets.Class)]
  public sealed class AlwaysEnabledAttribute : Attribute { }

  [Managed(typeof(ApplicationScope), optional: true)]
  [AlwaysEnabled]
  public partial class GeneratedConditionalComponent { }

  [Managed(typeof(ApplicationScope), optional: true, phase: -250)]
  public partial class GeneratedPhasedComponent { }
}
