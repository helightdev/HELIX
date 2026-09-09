using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

#pragma warning disable RS1035 // Explicit opt-in profiler writes its report to the host project's Logs directory.

namespace Hix.Env;

internal static class HixProfiler {
  private static readonly ConcurrentDictionary<string, Measurement> Measurements = new();
  private static readonly object WriteGate = new();
  private static int _enabled;
  private static int _exitHandlerRegistered;
  private static long _flushedVersion;
  private static long _version;
  private static long _machineGeneration;
  private static readonly bool Forced;
  private static Timer _flushTimer;
  private static string _machineHash;
  private static string _outputPath;

  static HixProfiler() {
    var forcedPath = Environment.GetEnvironmentVariable("HELIX_MIXIN_PROFILE_PATH");
    Forced = !string.IsNullOrEmpty(forcedPath);
    if (Forced) Enable(forcedPath);
  }

  internal static bool Enabled => Volatile.Read(ref _enabled) != 0;

  internal static void Configure(bool enabled, string projectPath, string machineHash) {
    if (!enabled) {
      if (!Forced) Volatile.Write(ref _enabled, 0);
    } else {
      Enable(Path.Combine(projectPath, "Logs", "HelixSourceGenerator.profile.tsv"));
    }
    if (Enabled) ResetForMachine(machineHash ?? "");
  }

  private static void ResetForMachine(string machineHash) {
    lock (WriteGate) {
      if (string.Equals(_machineHash, machineHash, StringComparison.Ordinal)) return;
      _machineHash = machineHash;
      Interlocked.Increment(ref _machineGeneration);
      Measurements.Clear();
      RuntimeCounters.Reset();
      Volatile.Write(ref _version, 0);
      Volatile.Write(ref _flushedVersion, 0);
      if (string.IsNullOrEmpty(_outputPath)) return;
      try {
        var directory = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        using var stream = new FileStream(_outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
      } catch (Exception) {
        // Profiling is diagnostic-only and must never fail the compiler host.
      }
    }
  }

  private static void Enable(string outputPath) {
    _outputPath = outputPath;
    if (Interlocked.Exchange(ref _exitHandlerRegistered, 1) == 0)
      AppDomain.CurrentDomain.ProcessExit += static (_, _) => SafeFlush();
    Volatile.Write(ref _enabled, 1);
  }

  internal static void ScheduleFlush() {
    if (!Enabled || string.IsNullOrEmpty(_outputPath)) return;
    lock (WriteGate) {
      _flushTimer ??= new Timer(static _ => SafeFlush(), null, Timeout.Infinite, Timeout.Infinite);
      _flushTimer.Change(100, Timeout.Infinite);
    }
  }

  internal static Scope Measure(string name) {
    return Enabled ? new Scope(name) : default;
  }

  internal static Scope MeasureFunction(string name) {
    return Enabled ? new Scope("function." + name) : default;
  }

  internal static void Increment(string name) {
    if (!Enabled) return;
    var measurement = Measurements.GetOrAdd(name, static _ => new Measurement());
    Interlocked.Increment(ref measurement.Count);
    Interlocked.Increment(ref _version);
  }

  internal static void Flush() {
    if (string.IsNullOrEmpty(_outputPath)) return;
    lock (WriteGate) {
      var version = Volatile.Read(ref _version);
      if (version == Volatile.Read(ref _flushedVersion)) return;
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
      var runtime = new[] {
        "runtime.gc.gen0\t" + (GC.CollectionCount(0) - RuntimeCounters.InitialGen0Collections),
        "runtime.gc.gen1\t" + (GC.CollectionCount(1) - RuntimeCounters.InitialGen1Collections),
        "runtime.gc.gen2\t" + (GC.CollectionCount(2) - RuntimeCounters.InitialGen2Collections)
      };
      using var process = Process.GetCurrentProcess();
      var report = "# " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) +
        "\tprocess=" + process.Id.ToString(CultureInfo.InvariantCulture) + "\n" +
        "name\tcount\ttotal_ms\taverage_ms\n" + string.Join("\n", lines) + "\n" +
        string.Join("\n", runtime) + "\n\n";
      var bytes = Encoding.UTF8.GetBytes(report);
      using (var stream = new FileStream(
        _outputPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite
      )) stream.Write(bytes, 0, bytes.Length);
      Volatile.Write(ref _flushedVersion, version);
    }
  }

  private static void SafeFlush() {
    try {
      Flush();
    } catch (Exception) {
      // Profiling is diagnostic-only and must never fail the compiler host.
    }
  }

  internal readonly struct Scope : IDisposable {
    private readonly long _generation;
    private readonly string _name;
    private readonly long _started;

    internal Scope(string name) {
      _generation = Volatile.Read(ref _machineGeneration);
      _name = name;
      _started = Stopwatch.GetTimestamp();
    }

    public void Dispose() {
      if (_name is null || _generation != Volatile.Read(ref _machineGeneration)) return;
      var elapsed = Stopwatch.GetTimestamp() - _started;
      var measurement = Measurements.GetOrAdd(_name, static _ => new Measurement());
      Interlocked.Increment(ref measurement.Count);
      Interlocked.Add(ref measurement.Ticks, elapsed);
      Interlocked.Increment(ref _version);
    }
  }

  private sealed class Measurement {
    internal long Count;
    internal long Ticks;
  }

  private static class RuntimeCounters {
    internal static int InitialGen0Collections = GC.CollectionCount(0);
    internal static int InitialGen1Collections = GC.CollectionCount(1);
    internal static int InitialGen2Collections = GC.CollectionCount(2);

    internal static void Reset() {
      InitialGen0Collections = GC.CollectionCount(0);
      InitialGen1Collections = GC.CollectionCount(1);
      InitialGen2Collections = GC.CollectionCount(2);
    }
  }
}
#pragma warning restore RS1035
