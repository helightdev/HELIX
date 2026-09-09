using System;

namespace Hix.Env;

/// <summary>Standalone no-op compiler profiler; generator hosts provide the implementation.</summary>
public static class HixCompilerProfiler {
  public static Scope Measure(string name) => default;
  public static void Increment(string name) { }

  public readonly struct Scope : IDisposable {
    public void Dispose() { }
  }
}
