using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace HELIX.Prose {
  /// <summary>
  /// Reusable immediate-mode plain-text sink with configurable tree boundaries. Builders and stacks are
  /// retained across Reset calls; primitive formatting uses stack buffers and branch completion is patched
  /// in place rather than retaining a semantic tree.
  /// </summary>
  public sealed class ProsePlainTextWriter : ProseWriter {
    private const char HardLineBreak = '\uE000';
    private const char SoftLineBreak = '\uE001';
    private const char LinePrefixStart = '\uE002';
    private const char LinePrefixEnd = '\uE003';
    private const char AlignmentStart = '\uE004';
    private const char AlignmentEnd = '\uE005';
    private const char SuffixRepeatStart = '\uE006';
    private const char SuffixRepeatEnd = '\uE007';
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
      public int Start;
      public int End;
      public int Index;
      public bool HasValue;
    }

    private struct Frame {
      public IProseScope Scope;
      public int ModifierStart;
      public int OutputStart;
      public int StartColumn;
      public bool StartLineHasContent;
      public LinePrefixKind StartLinePrefixKind;
      public int StartLinePrefixTreeDepth;
      public bool StartLinePrefixWritten;
      public LineBreakKind StartPendingLineBreakKind;
      public int StartPropertyValueColumn;
      public int TreeDepth;
      public int TreeOutputStart;
      public int ItemOutputStart;
      public int ChildCount;
      public int PropertyCount;
      public bool PropertiesFinalized;
      public bool OwnerPropertiesWereFinalized;
      public PendingItem PendingProperty;
      public PendingItem PendingChild;
      public bool Truncated;
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
      Configuration = configuration ?? ProsePlainTextConfigurations.Unicode;
      _builder = new StringBuilder(initialCapacity);
      _frames = new Frame[initialFrameCapacity];
      _modifiers = new IProseModifier[initialFrameCapacity];
    }

    public ProsePlainTextConfiguration Configuration { get; }
    public int WrapWidth { get; set; }
    public ProseLevel MinimumLevel { get; set; }
    public int MaxTruncatableFrameLength { get; set; }
    public int Length => _builder.Length;

    public override bool BeginFrame(IProseScope scope) {
      if (scope == null) throw new ArgumentNullException(nameof(scope));
      if (IsWritingInactive()) return false;
      if (scope is ProseTree && !Configuration.ShowTrees ||
          scope is ProseName && !Configuration.ShowNames ||
          scope is ProseProperty && !Configuration.ShowProperties)
        return false;

      var ownerPropertiesWereFinalized = false;
      if (scope is ProseTree) {
        ownerPropertiesWereFinalized = OwnerPropertiesFinalized;
        PrepareForTree();
      } else if (scope is ProseProperty) {
        PrepareForProperty();
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
        Scope = scope,
        ModifierStart = _modifierCount,
        OutputStart = outputStart,
        StartColumn = startColumn,
        StartLineHasContent = startLineHasContent,
        StartLinePrefixKind = startLinePrefixKind,
        StartLinePrefixTreeDepth = startLinePrefixTreeDepth,
        StartLinePrefixWritten = startLinePrefixWritten,
        StartPendingLineBreakKind = startPendingLineBreakKind,
        StartPropertyValueColumn = startPropertyValueColumn,
        TreeDepth = _treeDepth,
        TreeOutputStart = _builder.Length,
        ItemOutputStart = _builder.Length,
        OwnerPropertiesWereFinalized = ownerPropertiesWereFinalized
      };
      _frames[_frameCount++] = frame;

      if (scope is ProseTree) {
        _treeDepth++;
      } else if (scope is ProseProperty) {
        SetLinePrefix(LinePrefixKind.Property, _treeDepth);
      } else if (scope is ProseName) {
        EnsureNewLine();
        frame = _frames[_frameCount - 1];
        frame.ItemOutputStart = _builder.Length;
        _frames[_frameCount - 1] = frame;
        SetLinePrefix(ParentScope is ProseTree ? LinePrefixKind.TreeName : LinePrefixKind.RootName, _treeDepth);
      } else if (scope is ProsePropertyValue) {
        frame = _frames[_frameCount - 1];
        frame.ItemOutputStart = _builder.Length;
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

      var index = _frameCount - 1;
      var frame = _frames[index];
      if (!IsSuppressed(index)) {
        if (frame.Scope is ProseTree) {
          CompleteTree(ref frame);
        } else if (!IsFrameOrAncestorInactive(index)) {
          if (frame.Scope is ProseName) {
            if (_lineHasContent) {
              FormatRange(
                ParentScope is ProseTree ? Configuration.TreeName : Configuration.RootName,
                frame.ItemOutputStart, _builder.Length, TextMatching.First | TextMatching.Last
              );
            }
          } else if (frame.Scope is ProsePropertyValue) {
            FormatRange(
              Configuration.PropertyValue, frame.ItemOutputStart, _builder.Length,
              TextMatching.First | TextMatching.Last |
              (_builder.Length == frame.ItemOutputStart ? TextMatching.Empty : TextMatching.None)
            );
          }
        }
      }

      for (var i = frame.ModifierStart; i < _modifierCount; i++) _modifiers[i] = null;
      _modifierCount = frame.ModifierStart;
      _treeDepth = frame.TreeDepth;
      if (frame.Scope is ProseTree or ProseName or ProseProperty) RestoreLineState(frame);
      _frames[index] = default;
      _frameCount--;

      if (frame.Scope is ProseTree && !IsSuppressedFrame(frame)) RecordCompletedTree(frame);
      else if (frame.Scope is ProseProperty && !IsSuppressedFrame(frame)) RecordCompletedProperty(frame);
    }

    public override void PushModifier(IProseModifier modifier) {
      if (modifier == null) throw new ArgumentNullException(nameof(modifier));
      if (_frameCount == 0) throw new InvalidOperationException("A modifier requires an active Prose frame.");

      EnsureModifierCapacity();
      _modifiers[_modifierCount++] = modifier;
      if (modifier is Hidden || modifier is LevelMarker level && !Includes(level.Level)) SuppressCurrentFrame();
    }

    public override void Write(string text) {
      if (string.IsNullOrEmpty(text) || IsWritingInactive()) return;
      WriteCharacters(text, 0, text.Length);
    }

    public override void Write(IProse prose) {
      if (prose == null) Write(ProseLiterals.Null);
      else prose.ToProse(this);
    }

    public override void Write<T>(T value, IProseFormatter<T> formatter) {
      if (formatter == null) throw new ArgumentNullException(nameof(formatter));

      if (typeof(T) == typeof(string) && formatter is ProseStringFormatter stringFormatter) {
        if (stringFormatter.Prefix != null) Write(stringFormatter.Prefix);
        Write(Unsafe.As<T, string>(ref value) ?? stringFormatter.NullText);
        if (stringFormatter.Suffix != null) Write(stringFormatter.Suffix);
      } else if (typeof(T) == typeof(int) && formatter is ProseIntFormatter intFormatter) {
        if (intFormatter.Prefix != null) Write(intFormatter.Prefix);
        Span<char> buffer = stackalloc char[16];
        Unsafe.As<T, int>(ref value).TryFormat(
          buffer, out var written, intFormatter.Format, CultureInfo.InvariantCulture
        );
        Write(buffer.Slice(0, written));
        if (intFormatter.Suffix != null) Write(intFormatter.Suffix);
      } else if (typeof(T) == typeof(long) && formatter is ProseLongFormatter longFormatter) {
        Span<char> buffer = stackalloc char[32];
        Unsafe.As<T, long>(ref value).TryFormat(
          buffer, out var written, longFormatter.Format, CultureInfo.InvariantCulture
        );
        Write(buffer.Slice(0, written));
      } else if (typeof(T) == typeof(float) && formatter is ProseFloatFormatter floatFormatter) {
        Span<char> buffer = stackalloc char[32];
        Unsafe.As<T, float>(ref value).TryFormat(
          buffer, out var written, floatFormatter.Format, CultureInfo.InvariantCulture
        );
        Write(buffer.Slice(0, written));
      } else if (typeof(T) == typeof(double) && formatter is ProseDoubleFormatter doubleFormatter) {
        Span<char> buffer = stackalloc char[32];
        Unsafe.As<T, double>(ref value).TryFormat(
          buffer, out var written, doubleFormatter.Format, CultureInfo.InvariantCulture
        );
        Write(buffer.Slice(0, written));
      } else if (typeof(T) == typeof(bool) && formatter is ProseBoolFormatter boolFormatter) {
        Write(Unsafe.As<T, bool>(ref value) ? boolFormatter.TrueText : boolFormatter.FalseText);
      } else {
        formatter.ToProse(this, value);
      }
    }

    public void Write(ReadOnlySpan<char> text) {
      if (text.Length == 0 || IsWritingInactive()) return;
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

    public override string ToString() => Build();

    private IProseScope ParentScope => _frameCount < 2 ? null : _frames[_frameCount - 2].Scope;

    private void PrepareForTree() {
      PrepareOwnerForChildren();
      FinalizePendingTree(false);
      EnsureNewLine();
    }

    private void PrepareForProperty() {
      FinalizePendingProperty(false);
    }

    private void CompleteTree(ref Frame frame) {
      FinalizeProperties(ref frame.PropertyCount, ref frame.PropertiesFinalized);
      FinalizePendingTree(true);
      frame.TreeOutputStart = Math.Min(frame.TreeOutputStart, _builder.Length);
    }

    private void RecordCompletedTree(Frame frame) {
      var pending = new PendingItem {
        Start = frame.TreeOutputStart,
        End = _builder.Length,
        HasValue = true
      };

      if (_treeDepth == 0) {
        pending.Index = _rootTreeCount;
        _rootTree = pending;
        _rootTreeCount++;
      } else {
        ref var parent = ref CurrentTreeFrame;
        pending.Index = parent.ChildCount;
        parent.PendingChild = pending;
        parent.ChildCount++;
      }
    }

    private void RecordCompletedProperty(Frame frame) {
      var pending = new PendingItem {
        Start = frame.ItemOutputStart,
        End = _builder.Length,
        HasValue = true
      };
      if (_treeDepth == 0) {
        pending.Index = _rootPropertyCount;
        _rootProperty = pending;
        _rootPropertyCount++;
      } else {
        ref var owner = ref CurrentTreeFrame;
        pending.Index = owner.PropertyCount;
        owner.PendingProperty = pending;
        owner.PropertyCount++;
      }
    }

    private bool OwnerPropertiesFinalized =>
      _treeDepth == 0 ? _rootPropertiesFinalized : CurrentTreeFrame.PropertiesFinalized;

    private void PrepareOwnerForChildren() {
      if (_treeDepth == 0) {
        FinalizeProperties(ref _rootPropertyCount, ref _rootPropertiesFinalized);
      } else {
        ref var owner = ref CurrentTreeFrame;
        FinalizeProperties(ref owner.PropertyCount, ref owner.PropertiesFinalized);
      }
    }

    private void FinalizeProperties(ref int propertyCount, ref bool finalized) {
      if (finalized) return;
      FinalizePendingProperty(true);
      finalized = true;
    }

    private void FinalizeRoot() {
      if (_rootFinalized) return;
      FinalizeProperties(ref _rootPropertyCount, ref _rootPropertiesFinalized);
      FinalizePendingTree(true);
      _rootFinalized = true;
    }

    private ref Frame CurrentTreeFrame {
      get {
        for (var i = _frameCount - 1; i >= 0; i--)
          if (_frames[i].Scope is ProseTree)
            return ref _frames[i];
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
          break;
        case LinePrefixKind.TreeName:
          break;
        case LinePrefixKind.NameContinuation:
          break;
        case LinePrefixKind.Property:
          break;
        case LinePrefixKind.PropertyValue:
          if (PropertyValueAlignsLine() && _column < _propertyValueColumn) {
            _builder.Append(AlignmentStart);
            AppendSpaces(_propertyValueColumn - _column);
            _builder.Append(AlignmentEnd);
          }
          break;
      }

      _linePrefixWritten = true;
      _pendingLineBreakKind = LineBreakKind.Semantic;
    }

    private void WriteCharacters(string text, int offset, int count) {
      var end = offset + count;
      var index = offset;
      var allowWrap = !HasModifier<NoWrap>();
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
      var allowWrap = !HasModifier<NoWrap>();
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

    private bool Includes(ProseLevel level) =>
      level != ProseLevel.Hidden && level != ProseLevel.Off && level >= MinimumLevel;

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
        var propertyLineMatching = CurrentLineMatching(_frames[propertyFrame].ItemOutputStart);
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
        var valueLineMatching = CurrentLineMatching(_frames[valueFrame].ItemOutputStart);
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
      for (var i = _frameCount - 1; i >= 0; i--)
        if (_frames[i].Scope is TScope)
          return i;
      return -1;
    }

    private TextMatching CurrentPropertyMatching() {
      var index = _treeDepth == 0 ? _rootPropertyCount : CurrentTreeFrame.PropertyCount;
      return ItemMatching(index, false, false);
    }

    private LineMatching CurrentLineMatching(int itemStart) {
      for (var i = _builder.Length - 1; i >= itemStart; i--)
        if (IsLineBreak(_builder[i]))
          return _builder[i] == HardLineBreak ? LineMatching.Hard : LineMatching.None;
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

    private bool IsSuppressed(int frameIndex) => _frames[frameIndex].OutputStart < 0;
    private static bool IsSuppressedFrame(Frame frame) => frame.OutputStart < 0;
    private bool IsInactive(int frameIndex) => IsSuppressed(frameIndex) || _frames[frameIndex].Truncated;

    private bool IsWritingInactive() => _frameCount > 0 && IsFrameOrAncestorInactive(_frameCount - 1);

    private bool IsFrameOrAncestorInactive(int frameIndex) {
      for (var i = frameIndex; i >= 0; i--)
        if (IsInactive(i))
          return true;
      return false;
    }

    private void SuppressCurrentFrame() {
      var index = _frameCount - 1;
      var frame = _frames[index];
      if (frame.OutputStart < 0) return;
      _builder.Length = frame.OutputStart;
      _column = frame.StartColumn;
      _lineHasContent = frame.StartLineHasContent;
      _linePrefixKind = frame.StartLinePrefixKind;
      _linePrefixTreeDepth = frame.StartLinePrefixTreeDepth;
      _linePrefixWritten = frame.StartLinePrefixWritten;
      _pendingLineBreakKind = frame.StartPendingLineBreakKind;
      _propertyValueColumn = frame.StartPropertyValueColumn;
      frame.OutputStart = -1;
      _frames[index] = frame;
      if (frame.Scope is ProseTree) SetOwnerPropertiesFinalized(frame.OwnerPropertiesWereFinalized);
    }

    private void SetOwnerPropertiesFinalized(bool value) {
      if (_treeDepth <= 1) _rootPropertiesFinalized = value;
      else {
        for (var i = _frameCount - 2; i >= 0; i--)
          if (_frames[i].Scope is ProseTree) {
            _frames[i].PropertiesFinalized = value;
            return;
          }
      }
    }

    private void RestoreLineState(Frame frame) {
      _linePrefixKind = frame.StartLinePrefixKind;
      _linePrefixTreeDepth = frame.StartLinePrefixTreeDepth;
      _propertyValueColumn = frame.StartPropertyValueColumn;
      _linePrefixWritten = _column != 0 && frame.StartLinePrefixWritten;
      _pendingLineBreakKind = frame.StartPendingLineBreakKind;
    }

    private void EnsureNewLine() => AppendLineBreak(false, LineBreakKind.Semantic);

    private void AppendLineBreak(bool force, LineBreakKind kind) {
      if (!force && !_lineHasContent) return;
      TrimCurrentLineEnd();
      _builder.Append(kind == LineBreakKind.Wrapped ? SoftLineBreak : HardLineBreak);
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
      remaining = MaxTruncatableFrameLength - (_builder.Length - frame.OutputStart);
      if (length <= remaining) return false;
      if (remaining < 0) remaining = 0;
      return true;
    }

    private void MarkTruncated(int frameIndex) {
      var frame = _frames[frameIndex];
      if (frame.Truncated) return;
      _builder.Append('…');
      _column++;
      _lineHasContent = true;
      frame.Truncated = true;
      _frames[frameIndex] = frame;
    }

    private void AppendDecoration(string text) {
      if (text.Length == 0) return;
      _builder.Append(text);
      _column += text.Length;
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
        FinalizePending(ref owner.PendingProperty, Configuration.Property, last);
      }
    }

    private void FinalizePendingTree(bool last) {
      if (_treeDepth == 0) FinalizePending(ref _rootTree, Configuration.Tree, last);
      else {
        ref var owner = ref CurrentTreeFrame;
        FinalizePending(ref owner.PendingChild, Configuration.Tree, last);
      }
    }

    private void FinalizePending(ref PendingItem pending, ItemAnchors anchors, bool last) {
      if (!pending.HasValue) return;
      FormatRange(
        anchors, pending.Start, pending.End,
        ItemMatching(pending.Index, last, pending.Start == pending.End)
      );
      pending = default;
    }

    private void FormatRange(ItemAnchors anchors, int start, int end, TextMatching matching) {
      var formatted = FormatItem(anchors, _builder.ToString(start, end - start), matching);
      _builder.Remove(start, end - start);
      _builder.Insert(start, formatted);
      RecalculateLineState();
    }

    private void RecalculateLineState() {
      var lastBreak = -1;
      for (var i = _builder.Length - 1; i >= 0; i--)
        if (IsLineBreak(_builder[i])) {
          lastBreak = i;
          break;
        }
      _column = 0;
      var hasSuffixRepeat = false;
      for (var i = lastBreak + 1; i < _builder.Length; i++) {
        if (_builder[i] == SuffixRepeatStart && i + 2 < _builder.Length &&
            _builder[i + 2] == SuffixRepeatEnd) {
          hasSuffixRepeat = true;
          i += 2;
        } else if (!IsDecorationMarker(_builder[i])) _column++;
      }
      if (hasSuffixRepeat && _column < WrapWidth) _column = WrapWidth;
      _lineHasContent = _column > 0;
      _linePrefixWritten = _lineHasContent;
    }

    private string FormatItem(ItemAnchors anchors, string inner, TextMatching matching) {
      var replacement = new StringBuilder();
      if (anchors.AppendReplacement(replacement, matching)) inner = replacement.ToString();

      var item = new StringBuilder();
      anchors.AppendPrefix(item, matching);
      item.Append(inner);
      _evaluationBuilder.Clear();
      anchors.AppendSuffix(_evaluationBuilder, matching);
      AppendSuffix(item, _evaluationBuilder, anchors.SuffixRepeater);
      var value = ToInternalText(item.ToString());
      if (value.Length == 0 && anchors.LineBreak.Count == 0) return value;
      var result = new StringBuilder(value.Length);
      var lineStart = 0;
      var lineIndex = 0;
      var prefixAllowed = true;
      while (lineStart <= value.Length) {
        var lineEnd = IndexOfLineBreak(value, lineStart);
        var isLast = lineEnd < 0 || lineEnd == value.Length - 1;
        var lineMatching = lineIndex == 0 || value[lineStart - 1] == HardLineBreak
          ? LineMatching.Hard
          : LineMatching.None;
        if (lineIndex == 0) lineMatching |= LineMatching.First;
        if (isLast) lineMatching |= LineMatching.Last;
        var contentEnd = lineEnd < 0 ? value.Length : lineEnd;
        var contentStart = lineStart;
        if (prefixAllowed && contentStart < contentEnd && value[contentStart] == AlignmentStart) {
          var alignmentEnd = value.IndexOf(
            AlignmentEnd, contentStart + 1, contentEnd - contentStart - 1
          );
          if (alignmentEnd >= 0) {
            result.Append(value, contentStart + 1, alignmentEnd - contentStart - 1);
            contentStart = alignmentEnd + 1;
          }
        }
        if (prefixAllowed) {
          _evaluationBuilder.Clear();
          anchors.AppendLinePrefix(_evaluationBuilder, matching, lineMatching);
          if (_evaluationBuilder.Length > 0) {
            result.Append(LinePrefixStart);
            result.Append(ToInternalText(_evaluationBuilder.ToString()));
            result.Append(LinePrefixEnd);
          }
        }
        if (!prefixAllowed) contentStart = SkipLineDecorations(value, contentStart, contentEnd);
        result.Append(value, contentStart, contentEnd - contentStart);

        var breakMatching = lineMatching;
        var boundary = LineBreakMode.Item;
        if (lineEnd >= 0) {
          boundary = value[lineEnd] == HardLineBreak ? LineBreakMode.Hard : LineBreakMode.Wrap;
          if (boundary == LineBreakMode.Hard) breakMatching |= LineMatching.Hard;
        }
        var mode = anchors.EvaluateLineBreak(matching, breakMatching);
        prefixAllowed = (mode & boundary) != 0;
        if (prefixAllowed)
          result.Append(boundary == LineBreakMode.Wrap ? SoftLineBreak : HardLineBreak);
        if (isLast) break;
        lineStart = lineEnd + 1;
        lineIndex++;
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
      item.Append(SuffixRepeatStart);
      item.Append(suffix[position]);
      item.Append(SuffixRepeatEnd);
      item.Append(suffix, position + 1, suffix.Length - position - 1);
    }

    private static bool IsLineBreak(char value) => value == HardLineBreak || value == SoftLineBreak;

    private static int IndexOfLineBreak(string value, int start) {
      for (var i = start; i < value.Length; i++)
        if (IsLineBreak(value[i]))
          return i;
      return -1;
    }

    private static int SkipLineDecorations(string value, int start, int end) {
      while (start < end) {
        var decorationEnd = value[start] == LinePrefixStart
          ? LinePrefixEnd
          : value[start] == AlignmentStart
            ? AlignmentEnd
            : '\0';
        if (decorationEnd == '\0') break;
        var position = value.IndexOf(decorationEnd, start + 1, end - start - 1);
        if (position < 0) break;
        start = position + 1;
      }
      return start;
    }

    private string ToInternalText(string value) => value
      .Replace("\r\n", HardLineBreak.ToString())
      .Replace("\r", HardLineBreak.ToString())
      .Replace("\n", HardLineBreak.ToString());

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
          if (character == SuffixRepeatStart && i + 2 < lineEnd &&
              value[i + 2] == SuffixRepeatEnd) {
            result.Append(value[i + 1], remaining);
            remaining = 0;
            i += 2;
          } else if (!IsDecorationMarker(character)) result.Append(character);
        }
        if (lineEnd == value.Length) break;
        result.Append('\n');
        lineStart = lineEnd + 1;
      }
      return result.ToString();
    }

    private static int VisibleWidth(string value, int start, int end) {
      var width = 0;
      for (var i = start; i < end; i++) {
        if (value[i] == SuffixRepeatStart && i + 2 < end && value[i + 2] == SuffixRepeatEnd) {
          i += 2;
          continue;
        }
        if (!IsDecorationMarker(value[i])) width++;
      }
      return width;
    }

    private static bool IsDecorationMarker(char value) => value is LinePrefixStart or LinePrefixEnd or AlignmentStart
      or AlignmentEnd or SuffixRepeatStart or SuffixRepeatEnd;

    private bool MatchesAt(string value, int position) {
      if (position < 0 || position + value.Length > _builder.Length) return false;
      for (var i = 0; i < value.Length; i++)
        if (_builder[position + i] != value[i])
          return false;
      return true;
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
      for (var i = _modifierCount - 1; i >= 0; i--)
        if (_modifiers[i] is TModifier)
          return true;
      return false;
    }

    private int FindModifierFrame<TModifier>() where TModifier : IProseModifier {
      for (var frameIndex = _frameCount - 1; frameIndex >= 0; frameIndex--) {
        var start = _frames[frameIndex].ModifierStart;
        var end = frameIndex + 1 < _frameCount ? _frames[frameIndex + 1].ModifierStart : _modifierCount;
        for (var modifierIndex = end - 1; modifierIndex >= start; modifierIndex--)
          if (_modifiers[modifierIndex] is TModifier)
            return frameIndex;
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