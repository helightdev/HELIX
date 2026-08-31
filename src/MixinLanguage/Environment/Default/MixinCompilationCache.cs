using System;
using Mixins.Roslyn;

namespace Mixins.Env;

/// <summary>
///   Standalone environment stub. A normal library host receives deterministic
///   fresh compilations and owns any caching policy outside the language.
/// </summary>
internal static class MixinCompilationCache {
  internal static MixinCompilation GetOrCreate(string key, Func<MixinCompilation> compile) {
    return compile();
  }
}