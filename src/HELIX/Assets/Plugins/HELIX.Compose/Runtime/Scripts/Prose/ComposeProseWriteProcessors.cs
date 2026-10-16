using System;
using System.Collections.Generic;
using HELIX.Compose;

namespace HELIX.Prose {
  public static class ComposeProseWriterDelegateExtensions {
    /// <summary>Delegates a semantic scope to another Compose writer and inserts its baked result into the owner.</summary>
    public static ProseWriterDelegates DelegateCompose<TScope>(
      this ProseWriterDelegates delegates, Func<TScope, ComposeProseWriter> writerFactory
    ) where TScope : IProseScope {
      if (delegates == null) throw new ArgumentNullException(nameof(delegates));
      if (writerFactory == null) throw new ArgumentNullException(nameof(writerFactory));
      return delegates.Delegate<TScope>(
        scope => writerFactory(scope),
        static (owner, delegated) => {
          if (owner is not ComposeProseWriter composeOwner || delegated is not ComposeProseWriter composeDelegate)
            throw new InvalidOperationException("Compose scope delegation requires Compose prose writers.");
          composeOwner.AddDelegatedComposable(composeDelegate.Build());
        }
      );
    }
  }

  public delegate bool ComposeProseScopeHandler<in TScope>(
    TScope scope, IReadOnlyList<Composable> children, IReadOnlyList<IProseModifier> modifiers,
    out Composable composable
  ) where TScope : IProseScope;

  public interface IComposeProseScopeProcessor {
    bool TryProcess(
      IProseScope scope, IReadOnlyList<Composable> children, IReadOnlyList<IProseModifier> modifiers,
      out Composable composable
    );
  }

  /// <summary>
  /// Optionally replaces a typed prose value write with a composable. Returning false delegates to the next
  /// processor and eventually to the writer's normal formatter handling.
  /// </summary>
  public interface IComposeProseWriteProcessor<T> {
    bool TryProcess(T value, IProseFormatter<T> formatter, out Composable composable);
  }

  public delegate bool ComposeProseWriteHandler<T>(
    T value, IProseFormatter<T> formatter, out Composable composable
  );

  /// <summary>An ordered, exact-value-type registry shared by one or more Compose prose writers.</summary>
  public sealed class ComposeProseWriteProcessors {
    private readonly Dictionary<Type, object> _processors = new();
    private readonly List<IComposeProseScopeProcessor> _scopeProcessors = new();

    public ComposeProseWriteProcessors Add<T>(IComposeProseWriteProcessor<T> processor) {
      if (processor == null) throw new ArgumentNullException(nameof(processor));
      GetOrCreate<T>().Add(processor);
      return this;
    }

    public ComposeProseWriteProcessors Handle<T>(ComposeProseWriteHandler<T> handler) {
      if (handler == null) throw new ArgumentNullException(nameof(handler));
      return Add(new InlineComposeProseWriteProcessor<T>(handler));
    }

    /// <summary>Always handles the exact value type by converting it directly to a composable.</summary>
    public ComposeProseWriteProcessors Convert<T>(Func<T, Composable> converter) {
      if (converter == null) throw new ArgumentNullException(nameof(converter));
      return Add(new ConvertingComposeProseWriteProcessor<T>(converter));
    }

    /// <summary>
    /// Expands the original formatter into a writer supplied for each accepted value, then uses its baked output.
    /// The factory must return a fresh or reset writer whose frames are balanced by the formatter.
    /// </summary>
    public ComposeProseWriteProcessors Delegate<T>(
      Func<T, IProseFormatter<T>, ComposeProseWriter> writerFactory,
      Func<T, IProseFormatter<T>, bool> accept = null
    ) {
      if (writerFactory == null) throw new ArgumentNullException(nameof(writerFactory));
      return Add(new DelegatingComposeProseWriteProcessor<T>(writerFactory, accept));
    }

    /// <summary>Handles a completed scope after its children have been converted to composables.</summary>
    public ComposeProseWriteProcessors HandleScope<TScope>(ComposeProseScopeHandler<TScope> handler)
      where TScope : IProseScope {
      if (handler == null) throw new ArgumentNullException(nameof(handler));
      _scopeProcessors.Add(new TypedComposeProseScopeProcessor<TScope>(handler));
      return this;
    }

    public ComposeProseWriteProcessors AddScope(IComposeProseScopeProcessor processor) {
      if (processor == null) throw new ArgumentNullException(nameof(processor));
      _scopeProcessors.Add(processor);
      return this;
    }

    public bool Remove<T>(IComposeProseWriteProcessor<T> processor) {
      if (processor == null || !_processors.TryGetValue(typeof(T), out var found)) return false;
      var processors = (List<IComposeProseWriteProcessor<T>>)found;
      var removed = processors.Remove(processor);
      if (processors.Count == 0) _processors.Remove(typeof(T));
      return removed;
    }

    public void Clear() {
      _processors.Clear();
      _scopeProcessors.Clear();
    }

    internal bool TryProcess<T>(T value, IProseFormatter<T> formatter, out Composable composable) {
      if (_processors.TryGetValue(typeof(T), out var found)) {
        var processors = (List<IComposeProseWriteProcessor<T>>)found;
        for (var i = 0; i < processors.Count; i++)
          if (processors[i].TryProcess(value, formatter, out composable))
            return true;
      }
      composable = null;
      return false;
    }

    internal bool TryProcessScope(
      IProseScope scope, IReadOnlyList<Composable> children, IReadOnlyList<IProseModifier> modifiers,
      out Composable composable
    ) {
      for (var i = 0; i < _scopeProcessors.Count; i++)
        if (_scopeProcessors[i].TryProcess(scope, children, modifiers, out composable))
          return true;
      composable = null;
      return false;
    }

    private List<IComposeProseWriteProcessor<T>> GetOrCreate<T>() {
      if (_processors.TryGetValue(typeof(T), out var found))
        return (List<IComposeProseWriteProcessor<T>>)found;
      var processors = new List<IComposeProseWriteProcessor<T>>();
      _processors.Add(typeof(T), processors);
      return processors;
    }
  }

  internal sealed class TypedComposeProseScopeProcessor<TScope> : IComposeProseScopeProcessor
    where TScope : IProseScope {
    private readonly ComposeProseScopeHandler<TScope> _handler;
    public TypedComposeProseScopeProcessor(ComposeProseScopeHandler<TScope> handler) => _handler = handler;

    public bool TryProcess(
      IProseScope scope, IReadOnlyList<Composable> children, IReadOnlyList<IProseModifier> modifiers,
      out Composable composable
    ) {
      if (scope is TScope typed) return _handler(typed, children, modifiers, out composable);
      composable = null;
      return false;
    }
  }

  internal sealed class InlineComposeProseWriteProcessor<T> : IComposeProseWriteProcessor<T> {
    private readonly ComposeProseWriteHandler<T> _handler;
    public InlineComposeProseWriteProcessor(ComposeProseWriteHandler<T> handler) => _handler = handler;
    public bool TryProcess(T value, IProseFormatter<T> formatter, out Composable composable) =>
      _handler(value, formatter, out composable);
  }

  internal sealed class ConvertingComposeProseWriteProcessor<T> : IComposeProseWriteProcessor<T> {
    private readonly Func<T, Composable> _converter;
    public ConvertingComposeProseWriteProcessor(Func<T, Composable> converter) => _converter = converter;

    public bool TryProcess(T value, IProseFormatter<T> formatter, out Composable composable) {
      composable = _converter(value);
      return true;
    }
  }

  internal sealed class DelegatingComposeProseWriteProcessor<T> : IComposeProseWriteProcessor<T> {
    private readonly Func<T, IProseFormatter<T>, ComposeProseWriter> _writerFactory;
    private readonly Func<T, IProseFormatter<T>, bool> _accept;

    public DelegatingComposeProseWriteProcessor(
      Func<T, IProseFormatter<T>, ComposeProseWriter> writerFactory,
      Func<T, IProseFormatter<T>, bool> accept
    ) {
      _writerFactory = writerFactory;
      _accept = accept;
    }

    public bool TryProcess(T value, IProseFormatter<T> formatter, out Composable composable) {
      if (_accept != null && !_accept(value, formatter)) {
        composable = null;
        return false;
      }
      var writer = _writerFactory(value, formatter) ??
                   throw new InvalidOperationException("A delegated Compose prose writer factory returned null.");
      formatter.ToProse(writer, value);
      composable = writer.Build();
      return true;
    }
  }
}
