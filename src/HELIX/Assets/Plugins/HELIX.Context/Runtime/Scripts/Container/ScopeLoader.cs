using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace HELIX.Context {
  /// <summary>Reusable container service that bounds transient dependency evidence to one scope load.</summary>
  internal sealed class ScopeLoader {
    private static readonly AsyncLocal<ScopeLoader> _active = new();
    private readonly Dictionary<string, HashSet<RegistrationEntry>> _publications = new();
    private readonly HashSet<string> _anonymousPublications = new(StringComparer.Ordinal);
    private readonly HashSet<IScriptedDependency> _scripted = new(ReferenceComparer<IScriptedDependency>.Instance);
    private ManagedScope _scope;

    public static ScopeLoader Active => _active.Value ??
      throw new ScopeLifecycleException("No scope is currently loading.");
    public static ScopeLoader ActiveOrNull => _active.Value;

    public void Load(ManagedScope scope, Action operation) {
      Begin(scope);
      try { operation(); } finally { End(); }
    }

    public async UniTask LoadAsync(ManagedScope scope, Func<UniTask> operation) {
      Begin(scope);
      try { await operation(); } finally { End(); }
    }

    public void Record(IScriptedDependency dependency) => _scripted.Add(dependency);
    public bool Contains(IScriptedDependency dependency) => _scripted.Contains(dependency);
    public void Publish(string wireKey) => _anonymousPublications.Add(wireKey);

    public void Publish(RegistrationEntry owner, string wireKey) {
      if (!_publications.TryGetValue(wireKey, out var owners)) _publications.Add(wireKey, owners = new());
      owners.Add(owner);
    }

    public bool HasPublication(string wireKey) => !string.IsNullOrEmpty(wireKey) &&
      (_anonymousPublications.Contains(wireKey) ||
        _publications.TryGetValue(wireKey, out var owners) && owners.Count > 0);

    public bool WasPublishedBy(RegistrationEntry owner, string wireKey) => wireKey != null &&
      _publications.TryGetValue(wireKey, out var owners) && owners.Contains(owner);

    public IEnumerable<string> PublicationsBy(RegistrationEntry owner) => _publications
      .Where(pair => pair.Value.Contains(owner)).Select(static pair => pair.Key);

    private void Begin(ManagedScope scope) {
      if (_scope != null) throw new ScopeLifecycleException("The scope loader is already loading a scope.");
      _scope = scope;
      _active.Value = this;
    }

    private void End() {
      _scope = null;
      _active.Value = null;
      _publications.Clear();
      _anonymousPublications.Clear();
      _scripted.Clear();
    }
  }
}