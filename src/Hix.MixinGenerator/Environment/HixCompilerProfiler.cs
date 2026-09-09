using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

#pragma warning disable RS1035

namespace Hix.Env;

/// <summary>Compilation-only profiler owned by the source-generator host.</summary>
internal static class HixCompilerProfiler {
  private static readonly ConcurrentDictionary<string, Measurement> Measurements = new();
  private static readonly object WriteGate = new();
  private static int _enabled;
  private static int _exitHandlerRegistered;
  private static long _generation;
  private static long _version;
  private static long _flushedVersion;
  private static string _machineHash;
  private static string _outputPath;
  private static Timer _flushTimer;

  internal static void Configure(bool enabled, string projectPath, string machineHash) {
    _outputPath = Path.Combine(projectPath, "Logs", "HixCompilerTimings.tsv");
    Volatile.Write(ref _enabled, enabled ? 1 : 0);
    if (enabled && Interlocked.Exchange(ref _exitHandlerRegistered, 1) == 0)
      AppDomain.CurrentDomain.ProcessExit += static (_, _) => SafeFlush();
    ResetForMachine(machineHash ?? "");
  }

  private static void ResetForMachine(string machineHash) {
    lock (WriteGate) {
      if (string.Equals(_machineHash, machineHash, StringComparison.Ordinal)) return;
      _machineHash = machineHash;
      Interlocked.Increment(ref _generation);
      Measurements.Clear();
      Volatile.Write(ref _version, 0);
      Volatile.Write(ref _flushedVersion, 0);
      if (Volatile.Read(ref _enabled) == 0 || string.IsNullOrEmpty(_outputPath)) return;
      try {
        Directory.CreateDirectory(Path.GetDirectoryName(_outputPath));
        using var stream = new FileStream(_outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
      } catch (Exception) { }
    }
  }

  internal static Scope Measure(string name) => Volatile.Read(ref _enabled) != 0 ? new Scope(name) : default;

  internal static void Increment(string name) {
    if (Volatile.Read(ref _enabled) == 0) return;
    var measurement = Measurements.GetOrAdd(name, static _ => new Measurement());
    Interlocked.Increment(ref measurement.Count);
    Interlocked.Increment(ref _version);
  }

  internal static void ScheduleFlush() {
    if (Volatile.Read(ref _enabled) == 0) return;
    lock (WriteGate) {
      _flushTimer ??= new Timer(static _ => SafeFlush(), null, Timeout.Infinite, Timeout.Infinite);
      _flushTimer.Change(100, Timeout.Infinite);
    }
  }

  internal static void Flush() {
    if (Volatile.Read(ref _enabled) == 0 || string.IsNullOrEmpty(_outputPath)) return;
    lock (WriteGate) {
      var version = Volatile.Read(ref _version);
      if (version == Volatile.Read(ref _flushedVersion)) return;
      Directory.CreateDirectory(Path.GetDirectoryName(_outputPath));
      var lines = Measurements.Select(item => new {
          item.Key, Count = Volatile.Read(ref item.Value.Count), Ticks = Volatile.Read(ref item.Value.Ticks)
        }).OrderByDescending(item => item.Ticks).Select(item => string.Join("\t",
          item.Key,
          item.Count.ToString(CultureInfo.InvariantCulture),
          (item.Ticks * 1000d / Stopwatch.Frequency).ToString("F3", CultureInfo.InvariantCulture),
          (item.Count == 0 ? 0 : item.Ticks * 1000d / Stopwatch.Frequency / item.Count)
          .ToString("F6", CultureInfo.InvariantCulture)));
      using var process = Process.GetCurrentProcess();
      var report = "# " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) +
        "\tprocess=" + process.Id.ToString(CultureInfo.InvariantCulture) + "\n" +
        "name\tcount\ttotal_ms\taverage_ms\n" + string.Join("\n", lines) + "\n";
      var bytes = Encoding.UTF8.GetBytes(report);
      using var stream = new FileStream(_outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
      stream.Write(bytes, 0, bytes.Length);
      Volatile.Write(ref _flushedVersion, version);
    }
  }

  private static void SafeFlush() {
    try { Flush(); } catch (Exception) { }
  }

  internal readonly struct Scope : IDisposable {
    private readonly long _generation;
    private readonly string _name;
    private readonly long _started;

    internal Scope(string name) {
      _generation = Volatile.Read(ref HixCompilerProfiler._generation);
      _name = name;
      _started = Stopwatch.GetTimestamp();
    }

    public void Dispose() {
      if (_name is null || _generation != Volatile.Read(ref HixCompilerProfiler._generation)) return;
      var measurement = Measurements.GetOrAdd(_name, static _ => new Measurement());
      Interlocked.Increment(ref measurement.Count);
      Interlocked.Add(ref measurement.Ticks, Stopwatch.GetTimestamp() - _started);
      Interlocked.Increment(ref _version);
    }
  }

  private sealed class Measurement {
    internal long Count;
    internal long Ticks;
  }
}

#pragma warning restore RS1035
