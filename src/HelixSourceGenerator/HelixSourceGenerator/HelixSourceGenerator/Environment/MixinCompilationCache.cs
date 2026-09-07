using System;
using System.Collections.Generic;
using Mixins.Roslyn;

namespace Mixins.Env;

/// <summary>
/// Process-wide cache used by the incremental generator host. This implementation
/// is intentionally outside the reusable language project.
/// </summary>
internal static class MixinCompilationCache {
  private static readonly object Gate = new();
  private static readonly Dictionary<string, MixinCompilation> Compilations = new(StringComparer.Ordinal);
  private static readonly Queue<string> CompilationOrder = new();
  private static readonly Dictionary<string, MixinLibraryFile> Files = new(StringComparer.Ordinal);
  private static readonly Queue<string> FileOrder = new();

  internal static MixinLibraryFile GetFile(string path, string content, Func<MixinLibraryFile> parse) {
    lock (Gate) {
      if (Files.TryGetValue(path, out var cached) && string.Equals(cached.Content, content, StringComparison.Ordinal))
        return cached;
      var result = parse();
      if (!Files.ContainsKey(path)) FileOrder.Enqueue(path);
      Files[path] = result;
      while (Files.Count > 64) Files.Remove(FileOrder.Dequeue());
      return result;
    }
  }

  internal static MixinCompilation GetOrCreate(string key, Func<MixinCompilation> compile) {
    lock (Gate) {
      if (Compilations.TryGetValue(key, out var cached)) return cached;
      var result = compile();
      Compilations.Add(key, result);
      CompilationOrder.Enqueue(key);
      while (Compilations.Count > 16) Compilations.Remove(CompilationOrder.Dequeue());
      return result;
    }
  }
}
