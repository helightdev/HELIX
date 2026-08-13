using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HELIX.Prose {
  /// <summary>
  /// Reusable immediate-mode plain-text sink with configurable tree boundaries. Builders and stacks are
  /// retained across Reset calls; primitive formatting uses stack buffers and branch completion is patched
  /// in place rather than retaining a semantic tree.
  /// </summary>
  public class ProsePlainTextWriter : ProseWriter, IProseLineBreakWriter {
    private const char _hardLineBreak = '\uE000';
    private const char _softLineBreak = '\uE001';
    private const char _linePrefixStart = '\uE002';
    private const char _linePrefixEnd = '\uE003';
    private const char _alignmentStart = '\uE004';
    private const char _alignmentEnd = '\uE005';
    private const char _suffixRepeatStart = '\uE006';
    private const char _suffixRepeatEnd = '\uE007';
    private const char _requiredLineBreak = '\uE008';
    private const char _zeroWidthStart = '\uE009';
    private const char _zeroWidthEnd = '\uE00A';
    private readonly StringBuilder _builder;
    private Frame[] _frames;
    private IProseModifier[] _modifiers;
    private int _column;
    private int _frameCount;
    private bool _lineHasContent;
    private LinePrefixKind _linePrefixKind;
    private int _linePrefixTreeDepth;
    private bool _linePrefixWritten;
    private LineBreakKind _pendingLineBreakKind;
    private int _modifierCount;
    private int _propertyValueColumn;
    private PendingItem _rootProperty;
    private PendingItem _rootTree;
    private int _rootTreeCount;
    private int _rootPropertyCount;
    private bool _rootPropertiesFinalized;
    private bool _rootFinalized;
    private int _treeDepth;
    private readonly StringBuilder _evaluationBuilder = new();

    private enum LinePrefixKind : byte { None, RootName, TreeName, NameContinuation, Property, PropertyValue }

    private enum LineBreakKind : byte { Semantic, Wrapped, Explicit }

    private struct PendingItem {
      public int start;
      public int end;
      public int index;
      public bool hasValue;
    }

    private sealed class TableCellData {
      public string text;
      public ProseTextAlignment alignment;
    }

    private sealed class TableRowData {
      public bool isHeader;
      public List<TableCellData> cells;
    }

    private struct Frame {
      public IProseScope scope;
      public int modifierStart;
      public int outputStart;
      public int startColumn;
      public bool startLineHasContent;
      public LinePrefixKind startLinePrefixKind;
      public int startLinePrefixTreeDepth;
      public bool startLinePrefixWritten;
      public LineBreakKind startPendingLineBreakKind;
      public int startPropertyValueColumn;
      public int treeDepth;
      public int treeOutputStart;
      public int itemOutputStart;
      public int childCount;
      public int propertyCount;
      public bool propertiesFinalized;
      public bool ownerPropertiesWereFinalized;
      public PendingItem pendingProperty;
      public PendingItem pendingChild;
      public List<TableRowData> tableRows;
      public List<TableCellData> tableCells;
      public bool propertyPrepared;
      public bool truncated;
    }

    public ProsePlainTextWriter(
      int wrapWidth = 100,
      ProseLevel minimumLevel = ProseLevel.Debug,
      int maxTruncatableFrameLength = -1,
      int initialCapacity = 256,
      int initialFrameCapacity = 16,
      ProsePlainTextConfiguration configuration = null
    ) {
      if (wrapWidth < 1) throw new ArgumentOutOfRangeException(nameof(wrapWidth));
      if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
      if (initialFrameCapacity < 1) throw new ArgumentOutOfRangeException(nameof(initialFrameCapacity));

      WrapWidth = wrapWidth;
      MinimumLevel = minimumLevel;
      MaxTruncatableFrameLength = maxTruncatableFrameLength;
      Configuration = configuration ?? ProsePlainTextConfigurations.Sparse;
      _builder = new StringBuilder(initialCapacity);
      _frames = new Frame[initialFrameCapacity];
      _modifiers = new IProseModifier[initialFrameCapacity];
    }

    public ProsePlainTextConfiguration Configuration { get; }
    public int WrapWidth { get; set; }
    public ProseLevel MinimumLevel { get; set; }
    public int MaxTruncatableFrameLength { get; set; }
    /// <summary>Rendered character count excluding writer metadata and zero-width presentation text.</summary>
    public int Length => VisibleLength(_builder.ToString());
    protected int OutputLength => _builder.Length;

    public override bool BeginFrame(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      if (IsWritingInactive()) return false;
      if ((scope is ProseTree && !Configuration.ShowTrees) ||
          (scope is ProseName && !Configuration.ShowNames) ||
          (scope is ProseProperty && !Configuration.ShowProperties) ||
          (IsTextFeature(scope) && !Configuration.ShowTextFeatures))
        return false;

      PrepareCurrentProperty();
      if (scope is ProseSectionHeader or ProseParagraph or ProseCodeBlock or ProseListItem or ProseTable)
        EnsureNewLine();
      var ownerPropertiesWereFinalized = false;
      if (scope is ProseTree) {
        ownerPropertiesWereFinalized = OwnerPropertiesFinalized;
        PrepareForTree();
      }

      var outputStart = _builder.Length;
      var startColumn = _column;
      var startLineHasContent = _lineHasContent;
      var startLinePrefixKind = _linePrefixKind;
      var startLinePrefixTreeDepth = _linePrefixTreeDepth;
      var startLinePrefixWritten = _linePrefixWritten;
      var startPendingLineBreakKind = _pendingLineBreakKind;
      var startPropertyValueColumn = _propertyValueColumn;
      EnsureFrameCapacity();
      var frame = new Frame {
        scope = scope,
        modifierStart = _modifierCount,
        outputStart = outputStart,
        startColumn = startColumn,
        startLineHasContent = startLineHasContent,
        startLinePrefixKind = startLinePrefixKind,
        startLinePrefixTreeDepth = startLinePrefixTreeDepth,
        startLinePrefixWritten = startLinePrefixWritten,
        startPendingLineBreakKind = startPendingLineBreakKind,
        startPropertyValueColumn = startPropertyValueColumn,
        treeDepth = _treeDepth,
        treeOutputStart = _builder.Length,
        itemOutputStart = _builder.Length,
        ownerPropertiesWereFinalized = ownerPropertiesWereFinalized,
        tableRows = scope is ProseTable ? new List<TableRowData>() : null,
        tableCells = scope is ProseTableRow ? new List<TableCellData>() : null,
        propertyPrepared = scope is not ProseProperty
      };
      _frames[_frameCount++] = frame;

      if (scope is ProseTree) _treeDepth++;
      else if (scope is ProseName) {
        EnsureNewLine();
        frame = _frames[_frameCount - 1];
        frame.itemOutputStart = _builder.Length;
        _frames[_frameCount - 1] = frame;
        SetLinePrefix(ParentScope is ProseTree ? LinePrefixKind.TreeName : LinePrefixKind.RootName, _treeDepth);
      } else if (scope is ProsePropertyValue) {
        frame = _frames[_frameCount - 1];
        frame.itemOutputStart = _builder.Length;
        _frames[_frameCount - 1] = frame;
        if (_lineHasContent) {
          _evaluationBuilder.Clear();
          Configuration.PropertyValue.AppendPrefix(
            _evaluationBuilder, TextMatching.First | TextMatching.Last
          );
          _propertyValueColumn = _column + ToInternalText(_evaluationBuilder.ToString()).Length;
          _linePrefixKind = LinePrefixKind.PropertyValue;
        }
      }

      return true;
    }

    public override void PopFrame() {
      if (_frameCount == 0) throw new InvalidOperationException("There is no Prose frame to pop.");

      PrepareCurrentProperty();
      var index = _frameCount - 1;
      var frame = _frames[index];
      if (!IsSuppressed(index)) {
        ApplyTextModifiers(frame);
        switch (frame.scope) {
          case ProseTableCell: CompleteTableCell(frame, index); break;
          case ProseTableRow row: CompleteTableRow(frame, row, index); break;
          case ProseTable: CompleteTable(frame); break;
          case ProseListItem: CompleteListItem(frame, index); break;
          case ProseList:
            FormatRange(Configuration.List, frame.itemOutputStart, _builder.Length, SingleItemMatching(frame)); break;
          case ProseSection:
            FormatRange(
              Configuration.Section, frame.itemOutputStart, _builder.Length, SingleItemMatching(frame)
            ); break;
          case ProseSectionHeader:
            FormatRange(
              Configuration.SectionHeader, frame.itemOutputStart, _builder.Length, SingleItemMatching(frame)
            ); break;
          case ProseParagraph:
            FormatRange(
              Configuration.Paragraph, frame.itemOutputStart, _builder.Length, SingleItemMatching(frame)
            ); break;
          case ProseCodeBlock codeBlock:
            ApplyCodeBlock(codeBlock, frame.itemOutputStart, _builder.Length, SingleItemMatching(frame)); break;
          case ProseTree: CompleteTree(ref frame); break;
          default: {
            if (!IsFrameOrAncestorInactive(index)) {
              if (frame.scope is ProseName) {
                if (_lineHasContent) {
                  FormatRange(
                    ParentScope is ProseTree ? Configuration.TreeName : Configuration.RootName,
                    frame.itemOutputStart, _builder.Length, TextMatching.First | TextMatching.Last
                  );
                }
              } else if (frame.scope is ProsePropertyValue) {
                FormatRange(
                  Configuration.PropertyValue, frame.itemOutputStart, _builder.Length,
                  TextMatching.First | TextMatching.Last |
                  (_builder.Length == frame.itemOutputStart ? TextMatching.Empty : TextMatching.None)
                );
              }
            }
            break;
          }
        }
      }

      for (var i = frame.modifierStart; i < _modifierCount; i++) _modifiers[i] = null;
      _modifierCount = frame.modifierStart;
      _treeDepth = frame.treeDepth;
      if (frame.scope is ProseTree or ProseName or ProseProperty) RestoreLineState(frame);
      _frames[index] = default;
      _frameCount--;

      if (frame.scope is ProseTree && !IsSuppressedFrame(frame)) RecordCompletedTree(frame);
      else if (frame.scope is ProseProperty && !IsSuppressedFrame(frame)) RecordCompletedProperty(frame);
    }

    public override void PushModifier(IProseModifier modifier) {
      if (modifier == null) throw new ArgumentNullException(nameof(modifier));
      if (_frameCount == 0) throw new InvalidOperationException("A modifier requires an active Prose frame.");

      EnsureModifierCapacity();
      _modifiers[_modifierCount++] = modifier;
      if (modifier is Hidden || (modifier is LevelMarker level && !Includes(level.Level))) SuppressCurrentFrame();
    }

    public override void Write(string text) {
      if (string.IsNullOrEmpty(text) || IsWritingInactive()) return;
      PrepareCurrentProperty();
      WriteCharacters(text, 0, text.Length);
    }

    public void WriteLineBreak(bool force) {
      if (IsWritingInactive()) return;
      PrepareCurrentProperty();
      AppendLineBreak(force, LineBreakKind.Explicit, required: true);
    }

    public override void Write(IProse prose) {
      if (prose == null) Write(ProseLiterals.Null);
      else prose.ToProse(this);
    }

    public override void Write<T>(T value, IProseFormatter<T> formatter) {
      if (formatter == null) throw new ArgumentNullException(nameof(formatter));
      formatter.ToProse(this, value);
    }

    public void Write(ReadOnlySpan<char> text) {
      if (text.Length == 0 || IsWritingInactive()) return;
      PrepareCurrentProperty();
      WriteCharacters(text);
    }

    public void Reset() {
      _builder.Clear();
      Array.Clear(_frames, 0, _frameCount);
      Array.Clear(_modifiers, 0, _modifierCount);
      _column = 0;
      _frameCount = 0;
      _lineHasContent = false;
      _linePrefixKind = LinePrefixKind.None;
      _linePrefixTreeDepth = 0;
      _linePrefixWritten = false;
      _pendingLineBreakKind = LineBreakKind.Semantic;
      _modifierCount = 0;
      _propertyValueColumn = 0;
      _rootProperty = default;
      _rootTree = default;
      _rootTreeCount = 0;
      _rootPropertyCount = 0;
      _rootPropertiesFinalized = false;
      _rootFinalized = false;
      _treeDepth = 0;
      _evaluationBuilder.Clear();
    }

    public string Build() {
      if (_frameCount != 0) throw new InvalidOperationException("All Prose frames must be popped before Build.");
      FinalizeRoot();
      var result = FormatItem(
        Configuration.Root, _builder.ToString(),
        TextMatching.First | TextMatching.Last |
        (_builder.Length == 0 ? TextMatching.Empty : TextMatching.None)
      );
      return ToExternalText(result);
    }

    public override string ToString() {
      return Build();
    }

    private IProseScope ParentScope => _frameCount < 2 ? null : _frames[_frameCount - 2].scope;

    private void PrepareForTree() {
      PrepareOwnerForChildren();
      FinalizePendingTree(false);
      EnsureNewLine();
    }

    private void PrepareCurrentProperty() {
      if (_frameCount == 0) return;
      ref var frame = ref _frames[_frameCount - 1];
      if (frame.scope is not ProseProperty || frame.propertyPrepared || IsInactive(_frameCount - 1)) return;

      FinalizePendingProperty(false);
      frame.outputStart = _builder.Length;
      frame.itemOutputStart = _builder.Length;
      frame.startColumn = _column;
      frame.startLineHasContent = _lineHasContent;
      frame.startLinePrefixKind = _linePrefixKind;
      frame.startLinePrefixTreeDepth = _linePrefixTreeDepth;
      frame.startLinePrefixWritten = _linePrefixWritten;
      frame.startPendingLineBreakKind = _pendingLineBreakKind;
      frame.startPropertyValueColumn = _propertyValueColumn;
      frame.propertyPrepared = true;
      SetLinePrefix(LinePrefixKind.Property, _treeDepth);
    }

    private void CompleteTree(ref Frame frame) {
      FinalizeProperties(ref frame.propertiesFinalized);
      FinalizePendingTree(true);
      frame.treeOutputStart = Math.Min(frame.treeOutputStart, _builder.Length);
    }

    private void CompleteListItem(Frame frame, int frameIndex) {
      var listIndex = FindParentFrame<ProseList>(frameIndex);
      var itemIndex = listIndex >= 0 ? _frames[listIndex].childCount++ : 0;
      var list = listIndex >= 0 ? (ProseList)_frames[listIndex].scope : ProseList.Unordered;
      var marker = list.Kind == ProseListKind.Ordered
        ? (list.Start + itemIndex).ToString(CultureInfo.InvariantCulture) + Configuration.OrderedListMarkerSuffix
        : Configuration.UnorderedListMarker;
      var internalMarker = ToInternalText(marker);
      _builder.Insert(frame.itemOutputStart, internalMarker);
      FormatRange(
        Configuration.ListItem,
        frame.itemOutputStart,
        _builder.Length,
        ItemMatching(itemIndex, false, _builder.Length == frame.itemOutputStart)
      );
    }

    private void CompleteTableCell(Frame frame, int frameIndex) {
      var rowIndex = FindParentFrame<ProseTableRow>(frameIndex);
      if (rowIndex < 0) return;
      var text = _builder.ToString(frame.itemOutputStart, _builder.Length - frame.itemOutputStart);
      _builder.Remove(frame.itemOutputStart, _builder.Length - frame.itemOutputStart);
      RecalculateLineState();
      _frames[rowIndex].tableCells.Add(
        new TableCellData {
          text = TrimInternalLineBreaks(text),
          alignment = FrameAlignment(frame)
        }
      );
    }

    private void CompleteTableRow(Frame frame, ProseTableRow row, int frameIndex) {
      var tableIndex = FindParentFrame<ProseTable>(frameIndex);
      if (tableIndex < 0) return;
      _frames[tableIndex].tableRows.Add(
        new TableRowData {
          isHeader = row.IsHeader,
          cells = frame.tableCells
        }
      );
    }

    private void CompleteTable(Frame frame) {
      var rendered = RenderTable(frame.tableRows);
      if (rendered.Length == 0) return;
      _builder.Insert(frame.itemOutputStart, rendered);
      RecalculateLineState();
    }

    private void ApplyTextModifiers(Frame frame) {
      var style = ProseTextStyle.None;
      string linkTarget = null;
      for (var i = frame.modifierStart; i < _modifierCount; i++) {
        if (_modifiers[i] is TextStyleMarker textStyle) style |= textStyle.Style;
        else if (_modifiers[i] is LinkMarker link) linkTarget = link.Target;
      }

      var matching = SingleItemMatching(frame);
      if ((style & ProseTextStyle.Code) != 0)
        ApplyTextStyle(ProseTextStyle.Code, frame.itemOutputStart, _builder.Length, matching);
      if ((style & ProseTextStyle.Emphasis) != 0)
        ApplyTextStyle(ProseTextStyle.Emphasis, frame.itemOutputStart, _builder.Length, matching);
      if ((style & ProseTextStyle.Strong) != 0)
        ApplyTextStyle(ProseTextStyle.Strong, frame.itemOutputStart, _builder.Length, matching);
      if ((style & ProseTextStyle.Quote) != 0)
        ApplyTextStyle(ProseTextStyle.Quote, frame.itemOutputStart, _builder.Length, matching);
      if ((style & ProseTextStyle.Error) != 0)
        ApplyTextStyle(ProseTextStyle.Error, frame.itemOutputStart, _builder.Length, matching);
      if (linkTarget != null)
        ApplyLink(linkTarget, frame.itemOutputStart, _builder.Length, matching);
    }

    /// <summary>Presentation hook for semantic text styles.</summary>
    protected virtual void ApplyTextStyle(
      ProseTextStyle style, int start, int end, TextMatching matching
    ) {
      var format = style switch {
        ProseTextStyle.Emphasis => Configuration.EmphasizedText,
        ProseTextStyle.Strong => Configuration.StrongText,
        ProseTextStyle.Code => Configuration.CodeText,
        ProseTextStyle.Quote => Configuration.QuoteText,
        ProseTextStyle.Error => Configuration.ErrorText,
        _ => null
      };
      if (format != null) ApplyFormat(format, start, end, matching);
    }

    /// <summary>Presentation hook for semantic links.</summary>
    protected virtual void ApplyLink(string target, int start, int end, TextMatching matching) {
      ApplyFormat(Configuration.LinkText, start, end, matching);
      if (Configuration.LinkTargetPrefix.Length == 0 && Configuration.LinkTargetSuffix.Length == 0)
        return;
      _builder.Insert(
        _builder.Length,
        ToInternalText(Configuration.LinkTargetPrefix + target + Configuration.LinkTargetSuffix)
      );
      RecalculateLineState();
    }

    /// <summary>Presentation hook for preformatted source blocks.</summary>
    protected virtual void ApplyCodeBlock(
      ProseCodeBlock codeBlock, int start, int end, TextMatching matching
    ) {
      var prefix = Configuration.CodeBlockPrefix + codeBlock.Language;
      if (prefix.Length > 0) prefix += "\n";
      var suffix = Configuration.CodeBlockSuffix;
      if (suffix.Length > 0) suffix = "\n" + suffix;
      DecorateRange(start, end, prefix, suffix);
      ApplyFormat(Configuration.CodeBlock, start, _builder.Length, matching);
    }

    /// <summary>Applies a configured node format from a derived writer.</summary>
    protected void ApplyFormat(PTNodeFormat format, int start, int end, TextMatching matching) =>
      FormatRange(format, start, end, matching);

    /// <summary>Adds visible presentation text around a completed range.</summary>
    protected void DecorateRange(int start, int end, string prefix, string suffix) {
      if (!string.IsNullOrEmpty(suffix)) _builder.Insert(end, ToInternalText(suffix));
      if (!string.IsNullOrEmpty(prefix)) _builder.Insert(start, ToInternalText(prefix));
      RecalculateLineState();
    }

    /// <summary>
    /// Adds presentation text emitted in the result but ignored by wrapping, measurement, and alignment.
    /// </summary>
    protected void DecorateRangeZeroWidth(int start, int end, string prefix, string suffix) {
      var encodedSuffix = EncodeZeroWidth(suffix);
      var encodedPrefix = EncodeZeroWidth(prefix);
      if (encodedSuffix.Length > 0) _builder.Insert(end, encodedSuffix);
      if (encodedPrefix.Length > 0) _builder.Insert(start, encodedPrefix);
      RecalculateLineState();
    }

    private ProseTextAlignment FrameAlignment(Frame frame) {
      for (var i = frame.modifierStart; i < _modifierCount; i++)
        if (_modifiers[i] is TextAlignmentMarker alignment)
          return alignment.Alignment;
      return ProseTextAlignment.Left;
    }

    private string RenderTable(IReadOnlyList<TableRowData> rows) {
      if (rows == null || rows.Count == 0) return string.Empty;
      var columns = 0;
      for (var i = 0; i < rows.Count; i++) columns = Math.Max(columns, rows[i].cells.Count);
      if (columns == 0) return string.Empty;

      var widths = new int[columns];
      for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++) {
        var cells = rows[rowIndex].cells;
        for (var column = 0; column < cells.Count; column++) {
          var lines = CellLines(cells[column].text);
          for (var line = 0; line < lines.Length; line++)
            widths[column] = Math.Max(widths[column], VisibleWidth(lines[line], 0, lines[line].Length));
        }
      }
      for (var column = 0; column < widths.Length; column++)
        widths[column] = Math.Max(widths[column], Configuration.Table.MinimumColumnWidth);

      var result = new StringBuilder();
      for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++) {
        if (result.Length > 0) result.Append(_hardLineBreak);
        AppendTableRow(result, rows[rowIndex], widths);
        if (rows[rowIndex].isHeader) {
          result.Append(_hardLineBreak);
          AppendTableSeparator(result, widths);
        }
      }
      return result.ToString();
    }

    private void AppendTableRow(StringBuilder result, TableRowData row, IReadOnlyList<int> widths) {
      var height = 1;
      var lines = new string[widths.Count][];
      for (var column = 0; column < widths.Count; column++) {
        lines[column] = column < row.cells.Count ? CellLines(row.cells[column].text) : new[] { string.Empty };
        height = Math.Max(height, lines[column].Length);
      }

      for (var line = 0; line < height; line++) {
        if (line > 0) result.Append(_hardLineBreak);
        result.Append(Configuration.Table.LeftBorder);
        for (var column = 0; column < widths.Count; column++) {
          if (column > 0) result.Append(Configuration.Table.ColumnSeparator);
          var text = line < lines[column].Length ? lines[column][line] : string.Empty;
          var alignment = column < row.cells.Count
            ? row.cells[column].alignment
            : ProseTextAlignment.Left;
          AppendAligned(result, text, widths[column], alignment);
        }
        result.Append(Configuration.Table.RightBorder);
      }
    }

    private void AppendTableSeparator(StringBuilder result, IReadOnlyList<int> widths) {
      result.Append(Configuration.Table.LeftBorder);
      for (var column = 0; column < widths.Count; column++) {
        if (column > 0) result.Append(Configuration.Table.ColumnSeparator);
        result.Append(Configuration.Table.HeaderFill, widths[column]);
      }
      result.Append(Configuration.Table.RightBorder);
    }

    private static void AppendAligned(
      StringBuilder result, string text, int width, ProseTextAlignment alignment
    ) {
      var padding = Math.Max(0, width - VisibleWidth(text, 0, text.Length));
      var before = alignment == ProseTextAlignment.Right
        ? padding
        : alignment == ProseTextAlignment.Center
          ? padding / 2
          : 0;
      result.Append(' ', before);
      result.Append(text);
      result.Append(' ', padding - before);
    }

    private static string[] CellLines(string text) {
      if (string.IsNullOrEmpty(text)) return new[] { string.Empty };
      var lines = new List<string>();
      var start = 0;
      while (start <= text.Length) {
        var end = IndexOfLineBreak(text, start);
        if (end < 0) end = text.Length;
        lines.Add(text.Substring(start, end - start));
        if (end == text.Length) break;
        start = end + 1;
      }
      return lines.ToArray();
    }

    private static string TrimInternalLineBreaks(string text) {
      var length = text.Length;
      while (length > 0 && IsLineBreak(text[length - 1])) length--;
      return length == text.Length ? text : text[..length];
    }

    private TextMatching SingleItemMatching(Frame frame) => TextMatching.First | TextMatching.Last |
                                                            (_builder.Length == frame.itemOutputStart
                                                              ? TextMatching.Empty
                                                              : TextMatching.None);

    private int FindParentFrame<TScope>(int frameIndex) where TScope : IProseScope {
      for (var i = frameIndex - 1; i >= 0; i--)
        if (_frames[i].scope is TScope)
          return i;
      return -1;
    }

    private void RecordCompletedTree(Frame frame) {
      var pending = new PendingItem {
        start = frame.treeOutputStart,
        end = _builder.Length,
        hasValue = true
      };

      if (_treeDepth == 0) {
        pending.index = _rootTreeCount;
        _rootTree = pending;
        _rootTreeCount++;
      } else {
        ref var parent = ref CurrentTreeFrame;
        pending.index = parent.childCount;
        parent.pendingChild = pending;
        parent.childCount++;
      }
    }

    private void RecordCompletedProperty(Frame frame) {
      var pending = new PendingItem {
        start = frame.itemOutputStart,
        end = _builder.Length,
        hasValue = true
      };
      if (_treeDepth == 0) {
        pending.index = _rootPropertyCount;
        _rootProperty = pending;
        _rootPropertyCount++;
      } else {
        ref var owner = ref CurrentTreeFrame;
        pending.index = owner.propertyCount;
        owner.pendingProperty = pending;
        owner.propertyCount++;
      }
    }

    private bool OwnerPropertiesFinalized =>
      _treeDepth == 0 ? _rootPropertiesFinalized : CurrentTreeFrame.propertiesFinalized;

    private void PrepareOwnerForChildren() {
      if (_treeDepth == 0) FinalizeProperties(ref _rootPropertiesFinalized);
      else {
        ref var owner = ref CurrentTreeFrame;
        FinalizeProperties(ref owner.propertiesFinalized);
      }
    }

    private void FinalizeProperties(ref bool finalized) {
      if (finalized) return;
      FinalizePendingProperty(true);
      finalized = true;
    }

    private void FinalizeRoot() {
      if (_rootFinalized) return;
      FinalizeProperties(ref _rootPropertiesFinalized);
      FinalizePendingTree(true);
      _rootFinalized = true;
    }

    private ref Frame CurrentTreeFrame {
      get {
        for (var i = _frameCount - 1; i >= 0; i--) {
          if (_frames[i].scope is ProseTree)
            return ref _frames[i];
        }
        throw new InvalidOperationException("No active Prose tree frame.");
      }
    }

    private void SetLinePrefix(LinePrefixKind kind, int treeDepth) {
      _linePrefixKind = kind;
      _linePrefixTreeDepth = treeDepth;
      _linePrefixWritten = false;
      _propertyValueColumn = 0;
    }

    private void EnsureLinePrefix() {
      if (_linePrefixWritten) return;

      switch (_linePrefixKind) {
        case LinePrefixKind.RootName:
        case LinePrefixKind.TreeName:
        case LinePrefixKind.NameContinuation:
        case LinePrefixKind.Property:
        case LinePrefixKind.None:
          break;
        case LinePrefixKind.PropertyValue:
          if (PropertyValueAlignsLine() && _column < _propertyValueColumn) {
            _builder.Append(_alignmentStart);
            AppendSpaces(_propertyValueColumn - _column);
            _builder.Append(_alignmentEnd);
          }
          break;
        default: throw new ArgumentOutOfRangeException();
      }

      _linePrefixWritten = true;
      _pendingLineBreakKind = LineBreakKind.Semantic;
    }

    private void WriteCharacters(string text, int offset, int count) {
      var end = offset + count;
      var index = offset;
      var allowWrap = !HasModifier<NoWrap>() &&
                      FindFrame<ProseTableCell>() < 0 && FindFrame<ProseCodeBlock>() < 0;
      while (index < end) {
        if (text[index] == '\r') {
          AppendLineBreak(true, LineBreakKind.Explicit);
          index += index + 1 < end && text[index + 1] == '\n' ? 2 : 1;
          continue;
        }
        if (text[index] == '\n') {
          AppendLineBreak(true, LineBreakKind.Explicit);
          index++;
          continue;
        }

        var start = index;
        var whitespace = text[index] == ' ' || text[index] == '\t';
        while (index < end && text[index] != '\n' && text[index] != '\r' &&
               (text[index] == ' ' || text[index] == '\t') == whitespace)
          index++;
        var length = index - start;
        if (allowWrap && !whitespace && _lineHasContent &&
            _column + DeferredLinePrefixWidth + length > WrapWidth)
          AppendLineBreak(false, LineBreakKind.Wrapped);
        if (whitespace && !_lineHasContent) continue;
        Append(text, start, length);
      }
    }

    private void WriteCharacters(ReadOnlySpan<char> text) {
      var index = 0;
      var allowWrap = !HasModifier<NoWrap>() &&
                      FindFrame<ProseTableCell>() < 0 && FindFrame<ProseCodeBlock>() < 0;
      while (index < text.Length) {
        if (text[index] == '\r') {
          AppendLineBreak(true, LineBreakKind.Explicit);
          index += index + 1 < text.Length && text[index + 1] == '\n' ? 2 : 1;
          continue;
        }
        if (text[index] == '\n') {
          AppendLineBreak(true, LineBreakKind.Explicit);
          index++;
          continue;
        }

        var start = index;
        var whitespace = text[index] == ' ' || text[index] == '\t';
        while (index < text.Length && text[index] != '\n' && text[index] != '\r' &&
               (text[index] == ' ' || text[index] == '\t') == whitespace)
          index++;
        var length = index - start;
        if (allowWrap && !whitespace && _lineHasContent &&
            _column + DeferredLinePrefixWidth + length > WrapWidth)
          AppendLineBreak(false, LineBreakKind.Wrapped);
        if (whitespace && !_lineHasContent) continue;
        Append(text.Slice(start, length));
      }
    }

    private bool Includes(ProseLevel level) {
      return level != ProseLevel.Hidden && level != ProseLevel.Off && level >= MinimumLevel;
    }

    private static bool IsTextFeature(IProseScope scope) => scope is ProseSpan or ProseSection or ProseSectionHeader
      or ProseParagraph or
      ProseCodeBlock or ProseList or ProseListItem or ProseTable or ProseTableRow or
      ProseTableCell;

    private int DeferredLinePrefixWidth {
      get {
        var width = 0;
        if (_treeDepth > 0) {
          _evaluationBuilder.Clear();
          Configuration.Tree.AppendLinePrefix(
            _evaluationBuilder, TextMatching.None, LineMatching.None
          );
          width += _treeDepth * _evaluationBuilder.Length;
        }

        var propertyFrame = FindFrame<ProseProperty>();
        if (propertyFrame < 0) return width;
        var propertyLineMatching = CurrentLineMatching(_frames[propertyFrame].itemOutputStart);
        var propertyMatching = CurrentPropertyMatching();
        if ((propertyLineMatching & LineMatching.First) != 0) {
          _evaluationBuilder.Clear();
          Configuration.Property.AppendPrefix(_evaluationBuilder, propertyMatching);
          width += _evaluationBuilder.Length;
        }
        _evaluationBuilder.Clear();
        Configuration.Property.AppendLinePrefix(
          _evaluationBuilder, propertyMatching, propertyLineMatching
        );
        width += _evaluationBuilder.Length;

        var valueFrame = FindFrame<ProsePropertyValue>();
        if (valueFrame < 0) return width;
        var valueLineMatching = CurrentLineMatching(_frames[valueFrame].itemOutputStart);
        if ((valueLineMatching & LineMatching.First) != 0) {
          _evaluationBuilder.Clear();
          Configuration.PropertyValue.AppendPrefix(
            _evaluationBuilder, TextMatching.First | TextMatching.Last
          );
          width += _evaluationBuilder.Length;
        }
        _evaluationBuilder.Clear();
        Configuration.PropertyValue.AppendLinePrefix(
          _evaluationBuilder,
          TextMatching.First | TextMatching.Last,
          valueLineMatching
        );
        return width + _evaluationBuilder.Length;
      }
    }

    private int FindFrame<TScope>() where TScope : IProseScope {
      for (var i = _frameCount - 1; i >= 0; i--) {
        if (_frames[i].scope is TScope)
          return i;
      }
      return -1;
    }

    private TextMatching CurrentPropertyMatching() {
      var index = _treeDepth == 0 ? _rootPropertyCount : CurrentTreeFrame.propertyCount;
      return ItemMatching(index, false, false);
    }

    private LineMatching CurrentLineMatching(int itemStart) {
      for (var i = _builder.Length - 1; i >= itemStart; i--) {
        if (IsLineBreak(_builder[i]))
          return IsHardLineBreak(_builder[i]) ? LineMatching.Hard : LineMatching.None;
      }
      return LineMatching.First | LineMatching.Hard;
    }

    private bool PropertyValueAlignsLine() {
      var wrapped = _pendingLineBreakKind == LineBreakKind.Wrapped;
      var lineMatching = wrapped ? LineMatching.None : LineMatching.Hard;
      var boundary = wrapped ? LineBreakMode.Wrap : LineBreakMode.Hard;
      var mode = Configuration.PropertyValue.EvaluateLineBreak(
        TextMatching.First | TextMatching.Last, lineMatching
      );
      return (mode & (boundary | LineBreakMode.Align)) ==
             (boundary | LineBreakMode.Align);
    }

    private bool IsSuppressed(int frameIndex) {
      return _frames[frameIndex].outputStart < 0;
    }

    private static bool IsSuppressedFrame(Frame frame) {
      return frame.outputStart < 0;
    }

    private bool IsInactive(int frameIndex) {
      return IsSuppressed(frameIndex) || _frames[frameIndex].truncated;
    }

    private bool IsWritingInactive() {
      return _frameCount > 0 && IsFrameOrAncestorInactive(_frameCount - 1);
    }

    private bool IsFrameOrAncestorInactive(int frameIndex) {
      for (var i = frameIndex; i >= 0; i--) {
        if (IsInactive(i))
          return true;
      }
      return false;
    }

    private void SuppressCurrentFrame() {
      var index = _frameCount - 1;
      var frame = _frames[index];
      if (frame.outputStart < 0) return;
      _builder.Length = frame.outputStart;
      _column = frame.startColumn;
      _lineHasContent = frame.startLineHasContent;
      _linePrefixKind = frame.startLinePrefixKind;
      _linePrefixTreeDepth = frame.startLinePrefixTreeDepth;
      _linePrefixWritten = frame.startLinePrefixWritten;
      _pendingLineBreakKind = frame.startPendingLineBreakKind;
      _propertyValueColumn = frame.startPropertyValueColumn;
      frame.outputStart = -1;
      _frames[index] = frame;
      if (frame.scope is ProseTree) SetOwnerPropertiesFinalized(frame.ownerPropertiesWereFinalized);
    }

    private void SetOwnerPropertiesFinalized(bool value) {
      if (_treeDepth <= 1) _rootPropertiesFinalized = value;
      else {
        for (var i = _frameCount - 2; i >= 0; i--) {
          if (_frames[i].scope is ProseTree) {
            _frames[i].propertiesFinalized = value;
            return;
          }
        }
      }
    }

    private void RestoreLineState(Frame frame) {
      _linePrefixKind = frame.startLinePrefixKind;
      _linePrefixTreeDepth = frame.startLinePrefixTreeDepth;
      _propertyValueColumn = frame.startPropertyValueColumn;
      _linePrefixWritten = _column != 0 && frame.startLinePrefixWritten;
      _pendingLineBreakKind = frame.startPendingLineBreakKind;
    }

    private void EnsureNewLine() {
      AppendLineBreak(false, LineBreakKind.Semantic);
    }

    private void AppendLineBreak(bool force, LineBreakKind kind, bool required = false) {
      if (!force && !_lineHasContent) return;
      TrimCurrentLineEnd();
      _builder.Append(
        required
          ? _requiredLineBreak
          : kind == LineBreakKind.Wrapped
            ? _softLineBreak
            : _hardLineBreak
      );
      _column = 0;
      _lineHasContent = false;
      if (_linePrefixKind is LinePrefixKind.RootName or LinePrefixKind.TreeName)
        _linePrefixKind = LinePrefixKind.NameContinuation;
      _linePrefixWritten = false;
      _pendingLineBreakKind = kind;
    }

    private void TrimCurrentLineEnd() {
      while (_builder.Length > 0) {
        var last = _builder[^1];
        if (last != ' ' && last != '\t') break;
        _builder.Length--;
        _column--;
      }
    }

    private void Append(string text, int start, int length) {
      if (length == 0) return;
      EnsureLinePrefix();
      if (TryTruncate(length, out var remaining, out var truncationFrame)) {
        if (remaining > 0) AppendUnchecked(text, start, remaining);
        MarkTruncated(truncationFrame);
        return;
      }
      AppendUnchecked(text, start, length);
    }

    private void Append(ReadOnlySpan<char> text) {
      if (text.Length == 0) return;
      EnsureLinePrefix();
      if (TryTruncate(text.Length, out var remaining, out var truncationFrame)) {
        if (remaining > 0) AppendUnchecked(text.Slice(0, remaining));
        MarkTruncated(truncationFrame);
        return;
      }
      AppendUnchecked(text);
    }

    private bool TryTruncate(int length, out int remaining, out int frameIndex) {
      remaining = length;
      frameIndex = -1;
      if (MaxTruncatableFrameLength < 0 || _frameCount == 0) return false;
      frameIndex = FindModifierFrame<AllowTruncate>();
      if (frameIndex < 0) return false;
      var frame = _frames[frameIndex];
      remaining = MaxTruncatableFrameLength - VisibleLength(
        _builder.ToString(frame.outputStart, _builder.Length - frame.outputStart)
      );
      if (length <= remaining) return false;
      if (remaining < 0) remaining = 0;
      return true;
    }

    private void MarkTruncated(int frameIndex) {
      var frame = _frames[frameIndex];
      if (frame.truncated) return;
      _builder.Append('…');
      _column++;
      _lineHasContent = true;
      frame.truncated = true;
      _frames[frameIndex] = frame;
    }

    private void AppendSpaces(int count) {
      if (count <= 0) return;
      _builder.Append(' ', count);
      _column += count;
    }

    private static TextMatching ItemMatching(int index, bool last, bool empty) {
      var matching = index == 0 ? TextMatching.First : TextMatching.None;
      if (last) matching |= TextMatching.Last;
      if ((index & 1) != 0) matching |= TextMatching.Odd;
      if (empty) matching |= TextMatching.Empty;
      return matching;
    }

    private void FinalizePendingProperty(bool last) {
      if (_treeDepth == 0) FinalizePending(ref _rootProperty, Configuration.Property, last);
      else {
        ref var owner = ref CurrentTreeFrame;
        FinalizePending(ref owner.pendingProperty, Configuration.Property, last);
      }
    }

    private void FinalizePendingTree(bool last) {
      if (_treeDepth == 0) FinalizePending(ref _rootTree, Configuration.Tree, last);
      else {
        ref var owner = ref CurrentTreeFrame;
        FinalizePending(ref owner.pendingChild, Configuration.Tree, last);
      }
    }

    private void FinalizePending(ref PendingItem pending, PTNodeFormat format, bool last) {
      if (!pending.hasValue) return;
      FormatRange(
        format, pending.start, pending.end,
        ItemMatching(pending.index, last, pending.start == pending.end)
      );
      pending = default;
    }

    private void FormatRange(PTNodeFormat format, int start, int end, TextMatching matching) {
      var formatted = FormatItem(format, _builder.ToString(start, end - start), matching);
      if (start > 0 && IsLineBreak(_builder[start - 1]) &&
          formatted.Length > 0 && formatted[0] == _requiredLineBreak) {
        _builder[start - 1] = _requiredLineBreak;
        formatted = formatted[1..];
      }
      if (end < _builder.Length && IsLineBreak(_builder[end]) &&
          formatted.Length > 0 && formatted[^1] == _requiredLineBreak) {
        _builder[end] = _requiredLineBreak;
        formatted = formatted[..^1];
      }
      _builder.Remove(start, end - start);
      _builder.Insert(start, formatted);
      RecalculateLineState();
    }

    private void RecalculateLineState() {
      var lastBreak = -1;
      for (var i = _builder.Length - 1; i >= 0; i--) {
        if (IsLineBreak(_builder[i])) {
          lastBreak = i;
          break;
        }
      }
      _column = 0;
      var hasSuffixRepeat = false;
      for (var i = lastBreak + 1; i < _builder.Length; i++) {
        if (_builder[i] == _zeroWidthStart) {
          var zeroWidthEnd = IndexOfZeroWidthEnd(_builder, i + 1, _builder.Length);
          if (zeroWidthEnd < 0) break;
          i = zeroWidthEnd;
        } else if (_builder[i] == _suffixRepeatStart && i + 2 < _builder.Length &&
                   _builder[i + 2] == _suffixRepeatEnd) {
          hasSuffixRepeat = true;
          i += 2;
        } else if (!IsDecorationMarker(_builder[i])) _column++;
      }
      if (hasSuffixRepeat && _column < WrapWidth) _column = WrapWidth;
      _lineHasContent = _column > 0;
      _linePrefixWritten = _lineHasContent;
    }

    private string FormatItem(PTNodeFormat itemFormat, string inner, TextMatching matching) {
      var replacement = new StringBuilder();
      if (itemFormat.AppendReplacement(replacement, matching)) inner = replacement.ToString();

      var item = new StringBuilder();
      itemFormat.AppendPrefix(item, matching);
      item.Append(inner);
      _evaluationBuilder.Clear();
      itemFormat.AppendSuffix(_evaluationBuilder, matching);
      AppendSuffix(item, _evaluationBuilder, itemFormat.SuffixRepeater);
      var value = ToInternalText(item.ToString());
      if (value.Length == 0 && itemFormat.LineBreak.Count == 0) return value;
      var result = new StringBuilder(value.Length);
      var lineStart = 0;
      var lineIndex = 0;
      var prefixAllowed = true;
      var ensureLineBreakBefore = false;
      var ensureLineBreakAfter = false;
      while (lineStart <= value.Length) {
        var lineEnd = IndexOfLineBreak(value, lineStart);
        var isLast = lineEnd < 0 || lineEnd == value.Length - 1;
        var lineMatching = lineIndex == 0 || IsHardLineBreak(value[lineStart - 1])
          ? LineMatching.Hard
          : LineMatching.None;
        if (lineIndex == 0) lineMatching |= LineMatching.First;
        if (isLast) lineMatching |= LineMatching.Last;
        var contentEnd = lineEnd < 0 ? value.Length : lineEnd;
        var contentStart = lineStart;
        if (prefixAllowed && contentStart < contentEnd && value[contentStart] == _alignmentStart) {
          var alignmentEnd = value.IndexOf(
            _alignmentEnd, contentStart + 1, contentEnd - contentStart - 1
          );
          if (alignmentEnd >= 0) {
            result.Append(value, contentStart + 1, alignmentEnd - contentStart - 1);
            contentStart = alignmentEnd + 1;
          }
        }
        if (prefixAllowed) {
          _evaluationBuilder.Clear();
          itemFormat.AppendLinePrefix(_evaluationBuilder, matching, lineMatching);
          if (_evaluationBuilder.Length > 0) {
            result.Append(_linePrefixStart);
            result.Append(ToInternalText(_evaluationBuilder.ToString()));
            result.Append(_linePrefixEnd);
          }
        }
        if (!prefixAllowed) contentStart = SkipLineDecorations(value, contentStart, contentEnd);
        result.Append(value, contentStart, contentEnd - contentStart);

        var breakMatching = lineMatching;
        var boundary = LineBreakMode.Item;
        if (lineEnd >= 0) {
          boundary = IsHardLineBreak(value[lineEnd]) ? LineBreakMode.Hard : LineBreakMode.Wrap;
          if (boundary == LineBreakMode.Hard) breakMatching |= LineMatching.Hard;
        }
        var mode = itemFormat.EvaluateLineBreak(matching, breakMatching);
        if (lineIndex == 0) ensureLineBreakBefore = (mode & LineBreakMode.Pre) != 0;
        if (isLast) ensureLineBreakAfter = (mode & LineBreakMode.Post) != 0;
        prefixAllowed = (lineEnd >= 0 && value[lineEnd] == _requiredLineBreak) ||
                        (mode & boundary) != 0;
        if (prefixAllowed) {
          result.Append(
            lineEnd >= 0 && value[lineEnd] == _requiredLineBreak
              ? _requiredLineBreak
              : boundary == LineBreakMode.Wrap
                ? _softLineBreak
                : _hardLineBreak
          );
        }
        if (isLast) break;
        lineStart = lineEnd + 1;
        lineIndex++;
      }
      if (ensureLineBreakBefore) {
        if (result.Length > 0 && IsLineBreak(result[0])) result[0] = _requiredLineBreak;
        else result.Insert(0, _requiredLineBreak);
      }
      if (ensureLineBreakAfter) {
        if (result.Length > 0 && IsLineBreak(result[^1])) result[^1] = _requiredLineBreak;
        else result.Append(_requiredLineBreak);
      }
      return result.ToString();
    }

    private static void AppendSuffix(StringBuilder item, StringBuilder suffix, int repeater) {
      if (suffix.Length == 0) return;
      if (repeater == -1) {
        item.Append(suffix);
        return;
      }

      var position = repeater >= 0 && repeater < suffix.Length ? repeater : 0;
      item.Append(suffix, 0, position + 1);
      item.Append(_suffixRepeatStart);
      item.Append(suffix[position]);
      item.Append(_suffixRepeatEnd);
      item.Append(suffix, position + 1, suffix.Length - position - 1);
    }

    private static bool IsLineBreak(char value) {
      return value is _hardLineBreak or _softLineBreak or _requiredLineBreak;
    }

    private static bool IsHardLineBreak(char value) {
      return value is _hardLineBreak or _requiredLineBreak;
    }

    private static int IndexOfLineBreak(string value, int start) {
      for (var i = start; i < value.Length; i++) {
        if (IsLineBreak(value[i]))
          return i;
      }
      return -1;
    }

    private static int SkipLineDecorations(string value, int start, int end) {
      while (start < end) {
        var decorationEnd = value[start] == _linePrefixStart
          ? _linePrefixEnd
          : value[start] == _alignmentStart
            ? _alignmentEnd
            : '\0';
        if (decorationEnd == '\0') break;
        var position = value.IndexOf(decorationEnd, start + 1, end - start - 1);
        if (position < 0) break;
        start = position + 1;
      }
      return start;
    }

    private string ToInternalText(string value) {
      return value
        .Replace("\r\n", _hardLineBreak.ToString())
        .Replace("\r", _hardLineBreak.ToString())
        .Replace("\n", _hardLineBreak.ToString());
    }

    private string ToExternalText(string value) {
      var result = new StringBuilder(value.Length);
      var lineStart = 0;
      while (lineStart <= value.Length) {
        var lineEnd = IndexOfLineBreak(value, lineStart);
        if (lineEnd < 0) lineEnd = value.Length;
        var visibleWidth = VisibleWidth(value, lineStart, lineEnd);
        var remaining = Math.Max(0, WrapWidth - visibleWidth);
        for (var i = lineStart; i < lineEnd; i++) {
          var character = value[i];
          if (character == _zeroWidthStart) {
            var zeroWidthEnd = value.IndexOf(_zeroWidthEnd, i + 1, lineEnd - i - 1);
            if (zeroWidthEnd < 0) continue;
            result.Append(value, i + 1, zeroWidthEnd - i - 1);
            i = zeroWidthEnd;
          } else if (character == _suffixRepeatStart && i + 2 < lineEnd &&
                     value[i + 2] == _suffixRepeatEnd) {
            result.Append(value[i + 1], remaining);
            remaining = 0;
            i += 2;
          } else if (!IsDecorationMarker(character)) result.Append(character);
        }
        if (lineEnd == value.Length) break;
        result.Append(
          value[lineEnd] == _requiredLineBreak ? Configuration.RequiredLineBreak : "\n"
        );
        lineStart = lineEnd + 1;
      }
      return result.ToString();
    }

    private static int VisibleWidth(string value, int start, int end) {
      var width = 0;
      for (var i = start; i < end; i++) {
        switch (value[i]) {
          case _zeroWidthStart: {
            var zeroWidthEnd = value.IndexOf(_zeroWidthEnd, i + 1, end - i - 1);
            if (zeroWidthEnd < 0) continue;
            i = zeroWidthEnd;
            break;
          }
          case _suffixRepeatStart when i + 2 < end && value[i + 2] == _suffixRepeatEnd:
            i += 2;
            continue;
        }
        if (!IsDecorationMarker(value[i])) width++;
      }
      return width;
    }

    private static bool IsDecorationMarker(char value) {
      return value is _linePrefixStart or _linePrefixEnd or _alignmentStart
        or _alignmentEnd or _suffixRepeatStart or _suffixRepeatEnd
        or _zeroWidthStart or _zeroWidthEnd;
    }

    private static string EncodeZeroWidth(string value) => string.IsNullOrEmpty(value)
      ? string.Empty
      : _zeroWidthStart + value + _zeroWidthEnd;

    private static int IndexOfZeroWidthEnd(StringBuilder value, int start, int end) {
      for (var i = start; i < end; i++)
        if (value[i] == _zeroWidthEnd)
          return i;
      return -1;
    }

    private static int VisibleLength(string value) {
      var length = 0;
      var start = 0;
      while (start <= value.Length) {
        var end = IndexOfLineBreak(value, start);
        if (end < 0) end = value.Length;
        length += VisibleWidth(value, start, end);
        if (end == value.Length) break;
        length++;
        start = end + 1;
      }
      return length;
    }

    private void AppendUnchecked(string text, int start, int length) {
      _builder.Append(text, start, length);
      _column += length;
      _lineHasContent |= length > 0;
    }

    private void AppendUnchecked(ReadOnlySpan<char> text) {
      for (var i = 0; i < text.Length; i++) _builder.Append(text[i]);
      _column += text.Length;
      _lineHasContent |= text.Length > 0;
    }

    private bool HasModifier<TModifier>() where TModifier : IProseModifier {
      for (var i = _modifierCount - 1; i >= 0; i--) {
        if (_modifiers[i] is TModifier)
          return true;
      }
      return false;
    }

    private int FindModifierFrame<TModifier>() where TModifier : IProseModifier {
      for (var frameIndex = _frameCount - 1; frameIndex >= 0; frameIndex--) {
        var start = _frames[frameIndex].modifierStart;
        var end = frameIndex + 1 < _frameCount ? _frames[frameIndex + 1].modifierStart : _modifierCount;
        for (var modifierIndex = end - 1; modifierIndex >= start; modifierIndex--) {
          if (_modifiers[modifierIndex] is TModifier)
            return frameIndex;
        }
      }
      return -1;
    }

    private void EnsureFrameCapacity() {
      if (_frameCount < _frames.Length) return;
      Array.Resize(ref _frames, _frames.Length * 2);
    }

    private void EnsureModifierCapacity() {
      if (_modifierCount < _modifiers.Length) return;
      Array.Resize(ref _modifiers, _modifiers.Length * 2);
    }
  }
}