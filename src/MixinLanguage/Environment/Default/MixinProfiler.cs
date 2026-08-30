using System;

namespace HelixSourceGenerator.Shared;

/// <summary>
/// Standalone environment stub. Hosts that source-link the language may exclude
/// this file and provide an implementation with the same internal contract.
/// </summary>
internal static class MixinProfiler {
  internal static bool Enabled => false;

  internal static void Configure(bool enabled, string projectPath) { }
  internal static void ScheduleFlush() { }
  internal static void Flush() { }
  internal static Scope Measure(string name) => default;
  internal static Scope MeasureFunction(string name) => default;
  internal static void Increment(string name) { }

  internal readonly struct Scope : IDisposable {
    public void Dispose() { }
  }
}
