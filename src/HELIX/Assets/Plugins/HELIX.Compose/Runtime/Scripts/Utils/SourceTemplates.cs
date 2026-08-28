using JetBrains.Annotations;

namespace HELIX.Compose {
  public static class SourceTemplates {
    [SourceTemplate]
    public static void row(this ref Composition cx) {
      using (cx.Row()) {
        //$ $END$
      }
    }

    [SourceTemplate]
    public static void column(this ref Composition cx) {
      using (cx.Column()) {
        //$ $END$
      }
    }

    [SourceTemplate]
    public static void writeContext(this ref Composition cx) {
      using (cx.WriteContext(out var context)) {
        //$ $END$
      }
    }
  }
}