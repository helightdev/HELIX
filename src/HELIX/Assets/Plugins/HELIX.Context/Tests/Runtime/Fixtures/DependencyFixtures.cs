using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace HELIX.Context.Tests.Fixtures {
  public interface IProvider {
    string Name { get; }
  }

  public class Provider : IProvider {
    public Provider(ICollection<string> trace = null, string name = "primary") {
      Name = name;
      trace?.Add(name);
    }

    public string Name { get; }
  }

  public sealed class SecondaryProvider : Provider {
    public SecondaryProvider(ICollection<string> trace = null) : base(trace, "secondary") { }
  }

  public sealed class SessionProvider : Provider {
    public SessionProvider(ICollection<string> trace = null) : base(trace, "session") { }
  }

  public sealed class ProviderConsumer {
    public ProviderConsumer(IProvider provider, ICollection<string> trace = null, string marker = "consumer") {
      Provider = provider;
      trace?.Add(marker);
    }

    public IProvider Provider { get; }
  }

  public sealed class OptionalProviderConsumer {
    public OptionalProviderConsumer(IProvider provider, ICollection<string> trace = null) {
      Provider = provider;
      trace?.Add("optional-consumer");
    }

    public IProvider Provider { get; }
  }

  public sealed class ProviderListConsumer {
    public ProviderListConsumer(IEnumerable<IProvider> providers, ICollection<string> trace = null) {
      Providers = providers.ToList();
      trace?.Add("list-consumer");
    }

    public IReadOnlyList<IProvider> Providers { get; }
  }

  public sealed class DependencyGate {
    public DependencyGate(ICollection<string> trace = null) => trace?.Add("gate");
  }

  public sealed class PublishedProvider : Provider {
    public PublishedProvider(string name = "published") : base(name: name) { }
  }

  public sealed class LateBindingComponent : IComponent {
    private readonly ICollection<string> _trace;
    private readonly object _value;
    private readonly TypeKey _key;
    private readonly string _marker;
    private readonly bool _failLate;

    public LateBindingComponent(
      object value,
      TypeKey key,
      ICollection<string> trace,
      string marker = "publisher",
      bool failLate = false
    ) {
      _value = value;
      _key = key;
      _trace = trace;
      _marker = marker;
      _failLate = failLate;
    }

    public RuntimeComponentData ComponentBinding { get; } = new();

    public void LoadComponent(ComponentLoadContext context) => _trace?.Add($"{_marker}:load");

    public void LoadComponentLate(ComponentLoadContext context) {
      _trace?.Add($"{_marker}:late");
      if (_failLate) throw new InvalidOperationException("late failure");
      context.Publish(_key, _value);
    }

    public void UnloadComponent() => _trace?.Add($"{_marker}:unload");
  }

  public sealed class SecondLateBindingComponent : IComponent {
    private readonly ICollection<string> _trace;
    private readonly IProvider _value;
    private readonly string _qualifier;

    public SecondLateBindingComponent(IProvider value, ICollection<string> trace, string qualifier = null) {
      _value = value;
      _trace = trace;
      _qualifier = qualifier;
    }

    public RuntimeComponentData ComponentBinding { get; } = new();
    public void LoadComponent(ComponentLoadContext context) => _trace?.Add("second-publisher:load");
    public void LoadComponentLate(ComponentLoadContext context) {
      _trace?.Add("second-publisher:late");
      context.Publish(_value, typeof(IProvider), _qualifier);
    }
  }

  public sealed class ProxyBindingComponent : IComponent {
    private readonly Func<IProvider> _supplier;
    private readonly ICollection<string> _trace;
    private readonly string _qualifier;

    public ProxyBindingComponent(
      Func<IProvider> supplier,
      ICollection<string> trace,
      string qualifier = null
    ) {
      _supplier = supplier;
      _trace = trace;
      _qualifier = qualifier;
    }

    public RuntimeComponentData ComponentBinding { get; } = new();
    public void LoadComponent(ComponentLoadContext context) => _trace?.Add("proxy:load");

    public void LoadComponentLate(ComponentLoadContext context) {
      _trace?.Add("proxy:late");
      context.PublishProxy<IProvider>(() => {
        _trace?.Add("supplier");
        return _supplier();
      }, _qualifier);
    }
  }

  public sealed class AsyncLateBindingComponent : IComponent, IEventListener {
    private readonly ICollection<string> _trace;
    private readonly object _value;
    private readonly TypeKey _key;

    public AsyncLateBindingComponent(object value, TypeKey key, ICollection<string> trace) {
      _value = value;
      _key = key;
      _trace = trace;
      HandlerList = EventHandlerList.Create();
      HandlerList.RegisterAsync<AsyncComponentLoadEvent>(OnAsyncLoad, 0);
    }

    public RuntimeComponentData ComponentBinding { get; } = new();
    public EventHandlerList HandlerList { get; }
    public AsyncHandlerValue HandlerValue { get; } = new();
    public void LoadComponent(ComponentLoadContext context) => _trace.Add("load");

    public void LoadComponentLate(ComponentLoadContext context) {
      _trace.Add("late");
      context.Publish(_key, _value);
    }

    private async UniTask OnAsyncLoad(AsyncComponentLoadEvent evt) {
      _trace.Add("async:start");
      await UniTask.Yield();
      _trace.Add("async:end");
      evt.Context.Publish(HandlerValue);
    }
  }

  public sealed class AsyncHandlerValue { }

  public sealed class PhasedScriptedDependency : ScriptedDependency {
    private readonly ICollection<string> _trace;

    public PhasedScriptedDependency(int phase, ICollection<string> trace)
      : base("phased-scripted-dependency", phase: phase) {
      _trace = trace;
    }

    public override ComponentLoadResult Load(ComponentLoadContext context) {
      _trace.Add("scripted");
      return true;
    }
  }

  public abstract class PipelineStage : IComponent {
    private readonly string _input;
    private readonly string _suffix;
    private readonly string _name;
    private readonly ICollection<string> _trace;

    protected PipelineStage(string input, string suffix, string name, ICollection<string> trace) {
      _input = input ?? string.Empty;
      _suffix = suffix;
      _name = name;
      _trace = trace;
    }

    public RuntimeComponentData ComponentBinding { get; } = new();

    public void LoadComponent(ComponentLoadContext context) => _trace.Add(_name);

    public void LoadComponentLate(ComponentLoadContext context) =>
      context.PublishProxy<string>(() => $"{_input}{_suffix}", "pipeline");
  }

  public sealed class FallbackPipelineStage : PipelineStage {
    public FallbackPipelineStage(string input, ICollection<string> trace)
      : base(input, "2;", "fallback", trace) { }
  }

  public sealed class FirstPipelineStage : PipelineStage {
    public FirstPipelineStage(string input, ICollection<string> trace)
      : base(input, "1;", "first-transformer", trace) { }
  }

  public sealed class SecondPipelineStage : PipelineStage {
    public SecondPipelineStage(string input, ICollection<string> trace)
      : base(input, "3;", "second-transformer", trace) { }
  }

  public sealed class PipelineConsumer {
    public PipelineConsumer(string value, ICollection<string> trace) {
      Value = value;
      trace.Add("consumer");
    }

    public string Value { get; }
  }

  public sealed class CollectingPipelineStage : IComponent {
    private readonly IReadOnlyList<string> _inputs;
    private readonly ICollection<string> _trace;

    public CollectingPipelineStage(IReadOnlyList<string> inputs, ICollection<string> trace) {
      _inputs = inputs;
      _trace = trace;
    }

    public RuntimeComponentData ComponentBinding { get; } = new();

    public void LoadComponent(ComponentLoadContext context) => _trace.Add("collection-transformer");

    public void LoadComponentLate(ComponentLoadContext context) =>
      context.PublishProxy<string>(() => $"{string.Join(",", _inputs)};3", "pipeline");
  }

  public sealed class PipelineListConsumer {
    public PipelineListConsumer(IReadOnlyList<string> values, ICollection<string> trace) {
      Values = values;
      trace.Add("collection-consumer");
    }

    public IReadOnlyList<string> Values { get; }
  }

  public sealed class CycleA { }
  public sealed class CycleB { }
  public sealed class MissingDependency { }
  public sealed class FailingComponent { }
  public sealed class FeatureFlag {
    public FeatureFlag(bool enabled) => Enabled = enabled;
    public bool Enabled { get; }
  }
}
