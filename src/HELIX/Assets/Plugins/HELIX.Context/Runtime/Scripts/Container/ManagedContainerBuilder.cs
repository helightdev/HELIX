using System;
using System.Collections.Generic;

namespace HELIX.Context {
  /// <summary>Collects container policies and extensions before creating a ManagedContainer.</summary>
  public sealed class ManagedContainerBuilder {
    private readonly List<IScopeHandler> _scopeHandlers = new();
    private ScopeRules _scopeRules = ScopeRules.Default;
    private RegistrationDiscoveryProvider _discoveryProvider;
    private int _maxLoadingIterations = ManagedContainer.DefaultMaxLoadingIterations;

    public ManagedContainerBuilder WithScopeRules(ScopeRules scopeRules) {
      _scopeRules = scopeRules ?? throw new ArgumentNullException(nameof(scopeRules));
      return this;
    }

    public ManagedContainerBuilder AddScopeHandler(IScopeHandler scopeHandler) {
      if (scopeHandler == null) throw new ArgumentNullException(nameof(scopeHandler));
      _scopeHandlers.Add(scopeHandler);
      return this;
    }

    public ManagedContainerBuilder AddScopeHandlers(IEnumerable<IScopeHandler> scopeHandlers) {
      if (scopeHandlers == null) throw new ArgumentNullException(nameof(scopeHandlers));
      foreach (var handler in scopeHandlers) AddScopeHandler(handler);
      return this;
    }

    public ManagedContainerBuilder DiscoverRegistrationsWith(RegistrationDiscoveryProvider discoveryProvider) {
      _discoveryProvider = discoveryProvider ?? throw new ArgumentNullException(nameof(discoveryProvider));
      return this;
    }

    public ManagedContainerBuilder WithMaxLoadingIterations(int maxLoadingIterations) {
      if (maxLoadingIterations <= 0)
        throw new ArgumentOutOfRangeException(nameof(maxLoadingIterations), "The limit must be greater than zero.");
      _maxLoadingIterations = maxLoadingIterations;
      return this;
    }

    public ManagedContainer Build() {
      var container = new ManagedContainer(_scopeRules, _scopeHandlers, _maxLoadingIterations);
      if (_discoveryProvider == null) return container;
      try {
        container.PrepareRegistrar(_discoveryProvider());
        return container;
      } catch {
        container.Dispose();
        throw;
      }
    }
  }
}
