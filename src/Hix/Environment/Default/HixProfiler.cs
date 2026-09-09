using System;

namespace Hix.Env;

/// <summary>
///   Standalone environment stub. Hosts that source-link the language may exclude
///   this file and provide an implementation with the same public contract.
/// </summary>
public static class HixProfiler {
  public static bool Enabled => false;

  public static void Configure(bool enabled, string projectPath) { }
  public static void ScheduleFlush() { }
  public static void Flush() { }

  public static Scope Measure(string name) {
    return default;
  }

  public static Scope MeasureFunction(string name) {
    return default;
  }

  public static void Increment(string name) { }

  public readonly struct Scope : IDisposable {
    public void Dispose() { }
  }
}