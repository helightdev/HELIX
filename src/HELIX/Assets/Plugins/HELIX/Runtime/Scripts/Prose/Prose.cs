using System;

namespace HELIX.Prose {
  /// <summary>Low-boilerplate immediate-mode producers for the built-in semantic frames.</summary>
  public static class Prose {
    public static void WriteName(IProseWriter writer, string name) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (!writer.BeginFrame(ProseName.Instance)) return;
      try { writer.Write(name); } finally { writer.PopFrame(); }
    }

    public static void WriteProperty<T>(
      IProseWriter writer,
      string key,
      T value,
      IProseFormatter<T> formatter,
      ProseLevel level = ProseLevel.Info,
      bool hidden = false,
      bool noWrap = false
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (!writer.BeginFrame(ProseProperty.Instance)) return;
      try {
        writer.PushModifier(LevelMarker.For(level));
        if (hidden) writer.PushModifier(Hidden.Instance);
        if (noWrap) writer.PushModifier(NoWrap.Instance);

        if (writer.BeginFrame(ProsePropertyKey.Instance)) {
          try { writer.Write(key); } finally { writer.PopFrame(); }
        }

        if (writer.BeginFrame(ProsePropertyValue.Instance)) {
          try { writer.Write(value, formatter); } finally { writer.PopFrame(); }
        }
      } finally {
        writer.PopFrame();
      }
    }
  }
}
