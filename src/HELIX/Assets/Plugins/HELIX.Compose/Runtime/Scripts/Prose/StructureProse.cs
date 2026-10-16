using System;
using HELIX.Prose;

namespace HELIX.Compose {
  /// <summary>
  ///   Writes a structured datatype as one field per property, so a generated prop struct becomes a form
  ///   without anything being declared twice. Nested structures become a path scope and recurse, which is
  ///   what makes the field paths line up with <see cref="FormController" />'s own nesting.
  /// </summary>
  /// <remarks>
  ///   The walk goes through <see cref="IStructurePropertyVisitor{T}" /> rather than
  ///   <see cref="ICompositeDatatype" /> so each property keeps its real value type: the field is a
  ///   <c>ProseField&lt;TValue&gt;</c>, its control resolves through <see cref="SpecConfig" /> for that
  ///   type, and no value is boxed on the way.
  /// </remarks>
  public static class StructureProse {
    /// <summary>
    ///   Writes every property of <paramref name="datatype" /> as a field below
    ///   <paramref name="path" />. An empty path writes them at the writer's current level.
    /// </summary>
    public static void WriteStructure<T>(
      this IProseWriter writer,
      StructureDatatype<T> datatype,
      string path = null,
      int maxDepth = 8
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      if (maxDepth < 1) throw new ArgumentOutOfRangeException(nameof(maxDepth));
      var value = default(T);
      var visitor = new FieldWriter<T>(writer, path, maxDepth);
      for (var i = 0; i < datatype.Properties.Count; i++)
        datatype.Properties[i].Visit(ref value, ref visitor);
    }

    /// <summary>
    ///   Writes a structure as a named section containing its fields — the shape an options page or a
    ///   nested object wants.
    /// </summary>
    public static void WriteStructureSection<T>(
      this IProseWriter writer,
      StructureDatatype<T> datatype,
      string path = null,
      string header = null,
      int maxDepth = 8
    ) {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (datatype == null) throw new ArgumentNullException(nameof(datatype));
      using (writer.Section()) {
        writer.WriteSectionHeader(header ?? datatype.Name);
        writer.WriteStructure(datatype, path, maxDepth);
      }
    }

    private struct FieldWriter<T> : IStructurePropertyVisitor<T> {
      private readonly IProseWriter _writer;
      private readonly string _path;
      private readonly int _depth;

      internal FieldWriter(IProseWriter writer, string path, int depth) {
        _writer = writer;
        _path = path;
        _depth = depth;
      }

      public void Visit<TValue>(ref T structure, StructurePropertyDatatype<T, TValue> property) {
        var path = Join(_path, property.FieldName);

        // A nested structure contributes its own properties rather than one opaque control, which is
        // the difference between a form and the single inline row a composite control would render.
        if (_depth > 1 && property.Datatype is StructureDatatype<TValue> nested) {
          using (_writer.Section()) {
            _writer.WriteSectionHeader(property.Label);
            if (property.Description != null) _writer.Description(property.Description);
            _writer.WriteStructure(nested, path, _depth - 1);
          }
          return;
        }

        _writer.WriteField(path, property.Label, property.Datatype, property.Description);
      }

      private static string Join(string prefix, string name) =>
        string.IsNullOrEmpty(prefix) ? name : prefix + "." + name;
    }
  }
}
