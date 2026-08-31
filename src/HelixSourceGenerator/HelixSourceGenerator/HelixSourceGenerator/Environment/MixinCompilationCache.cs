using System;
using Mixins.Roslyn;

namespace Mixins.Env;

/// <summary>
/// Process-wide cache used by the incremental generator host. This implementation
/// is intentionally outside the reusable language project.
/// </summary>
internal static class MixinCompilationCache {
  private static readonly object Gate = new();
  private static string _key;
  private static MixinCompilation _compilation;

  internal static MixinCompilation GetOrCreate(string key, Func<MixinCompilation> compile) {
    lock (Gate) {
      if (_compilation is not null && string.Equals(_key, key, StringComparison.Ordinal))
        return _compilation;
      _compilation = compile();
      _key = key;
      return _compilation;
    }
  }
}
