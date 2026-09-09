using System;
using System.IO;
using System.Text;

#pragma warning disable RS1035

namespace Hix.Env;

internal static class HixDebugReporter {
  private static readonly object Gate = new();
  private static string machineHash;
  private static string outputPath;
  private static bool enabled;

  internal static void Configure(bool active, string projectPath, string hash) {
    lock (Gate) {
      enabled = active;
      if (!active) return;
      outputPath = Path.Combine(projectPath, "Logs", "HelixSourceGenerator.debug.log");
      if (string.Equals(machineHash, hash, StringComparison.Ordinal)) return;
      machineHash = hash;
      try {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
      } catch (Exception) { }
    }
  }

  internal static void Write(string report) {
    lock (Gate) {
      if (!enabled || string.IsNullOrEmpty(outputPath) || string.IsNullOrEmpty(report)) return;
      try {
        var bytes = Encoding.UTF8.GetBytes(report + Environment.NewLine);
        // Debug dumps describe one generated target and are intentionally replaced;
        // appending repeats the shared program image once per target and grows without bound.
        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        stream.Write(bytes, 0, bytes.Length);
      } catch (Exception) { }
    }
  }
}

#pragma warning restore RS1035
