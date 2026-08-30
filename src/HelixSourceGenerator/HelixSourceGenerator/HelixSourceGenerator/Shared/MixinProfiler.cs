using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

#pragma warning disable RS1035 // Explicit opt-in profiler writes its report to the host project's Logs directory.

namespace HelixSourceGenerator.Shared;

internal static class MixinProfiler {
  private static readonly ConcurrentDictionary<string, Measurement> Measurements = new();
  private static readonly object WriteGate = new();
  private static int _enabled;
  private static int _exitHandlerRegistered;
  private static string _outputPath;

  internal static bool Enabled => Volatile.Read(ref _enabled) != 0;

  internal static void Configure(bool enabled, string projectPath) {
    if (!enabled) {
      Volatile.Write(ref _enabled, 0);
      return;
    }
    _outputPath = Path.Combine(projectPath, "Logs", "HelixSourceGenerator.profile.tsv");
    if (Interlocked.Exchange(ref _exitHandlerRegistered, 1) == 0)
      AppDomain.CurrentDomain.ProcessExit += static (_, _) => Flush();
    Volatile.Write(ref _enabled, 1);
  }

  internal static Scope Measure(string name) {
    return Enabled ? new Scope(name) : default;
  }

  internal static Scope MeasureFunction(string name) {
    return Enabled ? new Scope("function." + name) : default;
  }

  internal static void Flush() {
    if (!Enabled || string.IsNullOrEmpty(_outputPath)) return;
    lock (WriteGate) {
      var directory = Path.GetDirectoryName(_outputPath);
      if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
      var lines = Measurements.Select(item => new {
            item.Key, Count = Volatile.Read(ref item.Value.Count), Ticks = Volatile.Read(ref item.Value.Ticks)
          }
        )
        .OrderByDescending(item => item.Ticks)
        .Select(item => string.Join(
            "\t",
            item.Key,
            item.Count.ToString(CultureInfo.InvariantCulture),
            (item.Ticks * 1000d / Stopwatch.Frequency).ToString("F3", CultureInfo.InvariantCulture),
            (item.Count == 0 ? 0 : item.Ticks * 1000d / Stopwatch.Frequency / item.Count)
            .ToString("F6", CultureInfo.InvariantCulture)
          )
        );
      File.WriteAllText(_outputPath, "name\tcount\ttotal_ms\taverage_ms\n" + string.Join("\n", lines) + "\n");
    }
  }

  internal readonly struct Scope : IDisposable {
    private readonly string _name;
    private readonly long _started;

    internal Scope(string name) {
      _name = name;
      _started = Stopwatch.GetTimestamp();
    }

    public void Dispose() {
      if (_name is null) return;
      var elapsed = Stopwatch.GetTimestamp() - _started;
      var measurement = Measurements.GetOrAdd(_name, static _ => new Measurement());
      Interlocked.Increment(ref measurement.Count);
      Interlocked.Add(ref measurement.Ticks, elapsed);
    }
  }

  private sealed class Measurement {
    internal long Count;
    internal long Ticks;
  }
}
#pragma warning restore RS1035