using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using HELIX.Prose;
using UnityEngine;

namespace HELIX.Diagnostics {
  public static class HelixDiagnostics {
    // Fixed log content marker used to detect if a log message if from HELIX as to prevent recursion
    public static string LogSignature = "<size=0>HELIX-8a9f0aac-e33d-4246-b349-56a4a5cc6d66</size>";

    public static string ShortHash(this object obj) {
      if (obj == null) return "00000";
      var v = unchecked((uint)obj.GetHashCode()) & 0xFFFFF;
      return v.ToString("x5", CultureInfo.InvariantCulture);
    }

    public static string DescribeIdentity(this object obj) {
      return (obj == null ? "<null>" : obj.GetType().Name) + "#" + obj.ShortHash();
    }

    public static string ToStringNullable(this object obj) {
      return obj == null ? "<null>" : obj.ToString();
    }

    public static Exception ProseError(
      string summary,
      string description = null,
      Action<IProseWriter> writeDetails = null,
      string hint = null,
      Exception exception = null,
      string stackTrace = null
    ) => new InvalidOperationException(
      ProseErrorText(summary, description, writeDetails, hint, exception, stackTrace),
      exception
    );

    public static string ProseErrorText(
      string summary,
      string description = null,
      Action<IProseWriter> writeDetails = null,
      string hint = null,
      Exception exception = null,
      string stackTrace = null
    ) {
      var writer = new ProseTextWriter(configuration: ProseTextConfigurations.Error);
      writer.Name(summary);
      if (!string.IsNullOrEmpty(description)) writer.WriteParagraph(description);
      writeDetails?.Invoke(writer);
      if (!string.IsNullOrEmpty(hint)) {
        writer.WriteSectionHeader("Hint");
        writer.WriteParagraph(hint);
      }
      if (exception != null) {
        writer.Property("Exception", $"{exception.GetType().Name}: {exception.Message}", Datatypes.String);
        stackTrace ??= exception.StackTrace;
      }
      if (!string.IsNullOrEmpty(stackTrace)) writer.WriteCodeBlock(stackTrace);
      return writer.Build();
    }
  }

  public static class UnityConsoleLogBridge {
    private delegate void LogCompilerVariant(
      string message,
      string fileName,
      int lineNumber,
      int columnNumber
    );

    private static LogCompilerVariant _logCompilerError;
    private static LogCompilerVariant _logCompilerWarning;
    private static LogCompilerVariant _logCompilerInfo;

    private static object _defaultEntityId;

    static UnityConsoleLogBridge() {
      var warningMethod = typeof(Debug).GetMethod(
        "LogCompilerWarning",
        BindingFlags.Static | BindingFlags.NonPublic
      );
      if (warningMethod != null) {
        _logCompilerWarning = (LogCompilerVariant)Delegate.CreateDelegate(
          typeof(LogCompilerVariant),
          warningMethod,
          false
        );
      }

      var infoMethod = typeof(Debug).GetMethod(
        "LogInformation",
        BindingFlags.Static | BindingFlags.NonPublic
      );
      if (infoMethod != null) {
        _logCompilerInfo = (LogCompilerVariant)Delegate.CreateDelegate(
          typeof(LogCompilerVariant),
          infoMethod,
          false
        );
      }

      var errorMethod = typeof(Debug).GetMethod(
        "LogCompilerError",
        BindingFlags.Static | BindingFlags.NonPublic
      );
      if (errorMethod != null) {
        _logCompilerError = (LogCompilerVariant)Delegate.CreateDelegate(
          typeof(LogCompilerVariant),
          errorMethod,
          false
        );
      }
    }

    private static bool TryExtractLine(string stackTrace, out string fileName, out int lineNumber) {
      if (stackTrace == null) {
        fileName = null;
        lineNumber = 0;
        return false;
      }

      var match = Regex.Match(stackTrace, @"in\s+(.*?):+(\d+)");
      if (match.Success) {
        fileName = match.Groups[1].Value.Trim();
        lineNumber = int.Parse(match.Groups[2].Value);
        return true;
      }
      fileName = null;
      lineNumber = 0;
      return false;
    }

    public static void Log(DiagnosticLevel level, string message, string stackTrace) {
      TryExtractLine(stackTrace, out var fileName, out var lineNumber);
      switch (level) {
        case <= DiagnosticLevel.Info:
          _logCompilerInfo(message, fileName, lineNumber, 0);
          break;
        case <= DiagnosticLevel.Warning:
          _logCompilerWarning(message, fileName, lineNumber, 0);
          break;
        case <= DiagnosticLevel.Error:
          _logCompilerError(message, fileName, lineNumber, 0);
          break;
      }
    }
  }

  public enum DiagnosticLevel {
    Hidden,
    Fine,
    Debug,
    Info,
    Warning,
    Hint,
    Summary,
    Error,
    Off
  }
}
