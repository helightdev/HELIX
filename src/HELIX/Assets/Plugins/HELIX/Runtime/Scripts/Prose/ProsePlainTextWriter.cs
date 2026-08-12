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
    private BranchInfo _rootLastChild;
    private bool _rootHasChild;
    private int _rootTreeCount;
    private int _rootPropertyCount;
    private bool _rootPropertiesFinalized;
    private bool _rootFinalized;
    private int _treeDepth;

    private enum LinePrefixKind : byte {
      None,
      RootName,
      TreeName,
      NameContinuation,
      Property,
      PropertyValue
    }

    private enum LineBreakKind : byte {
      Semantic,
      Wrapped,
      Explicit
    }

    private struct BranchInfo {
      public int Start;
      public int End;
      public int LinkPosition;
      public int Depth;
      public bool IsLast;
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
      public int TreeLinkPosition;
      public int ChildCount;
      public int PropertyCount;
      public bool PropertiesFinalized;
      public bool OwnerPropertiesWereFinalized;
      public BranchInfo LastChild;
      public bool HasChild;
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

      var outputStart = _builder.Length;
      var startColumn = _column;
      var startLineHasContent = _lineHasContent;
      var startLinePrefixKind = _linePrefixKind;
      var startLinePrefixTreeDepth = _linePrefixTreeDepth;
      var startLinePrefixWritten = _linePrefixWritten;
      var startPendingLineBreakKind = _pendingLineBreakKind;
      var startPropertyValueColumn = _propertyValueColumn;
      var ownerPropertiesWereFinalized = false;
      if (scope is ProseTree) {
        ownerPropertiesWereFinalized = OwnerPropertiesFinalized;
        PrepareForTree();
      }

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
        TreeLinkPosition = -1,
        OwnerPropertiesWereFinalized = ownerPropertiesWereFinalized
      };
      _frames[_frameCount++] = frame;

      if (scope is ProseTree) {
        _treeDepth++;
      } else if (scope is ProseProperty) {
        PrepareForProperty();
        SetLinePrefix(LinePrefixKind.Property, _treeDepth);
      } else if (scope is ProseName) {
        EnsureNewLine();
        SetLinePrefix(ParentScope is ProseTree ? LinePrefixKind.TreeName : LinePrefixKind.RootName, _treeDepth);
      } else if (scope is ProsePropertyValue && _lineHasContent) {
        AppendDecoration(Configuration.PropertyValueSeparator);
        _propertyValueColumn = _column;
        _linePrefixKind = LinePrefixKind.PropertyValue;
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
              AppendDecoration(
                ParentScope is ProseTree ? Configuration.TreeNameSuffix : Configuration.RootNameSuffix
              );
              EnsureNewLine();
            }
          } else if (frame.Scope is ProseProperty) {
            if (Configuration.LineBreakProperties) EnsureNewLine();
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
      else if (frame.Scope is ProseProperty && !IsSuppressedFrame(frame)) RecordCompletedProperty();
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
      _rootHasChild = false;
      _rootTreeCount = 0;
      _rootPropertyCount = 0;
      _rootPropertiesFinalized = false;
      _rootFinalized = false;
      _treeDepth = 0;
    }

    public string Build() {
      if (_frameCount != 0) throw new InvalidOperationException("All Prose frames must be popped before Build.");
      FinalizeRoot();
      if (_rootHasChild) SetBranchLast(ref _rootLastChild, true);
      var length = _builder.Length;
      while (EndsWithAt(Configuration.LineBreak, length)) length -= Configuration.LineBreak.Length;
      return _builder.ToString(0, length);
    }

    public override string ToString() => Build();

    private IProseScope ParentScope => _frameCount < 2 ? null : _frames[_frameCount - 2].Scope;

    private void PrepareForTree() {
      PrepareOwnerForChildren();
      EnsureNewLine();
      if (_treeDepth == 0 && _rootHasChild && _rootLastChild.IsLast)
        SetBranchLast(ref _rootLastChild, false);

      var siblingCount = _treeDepth == 0 ? _rootTreeCount : CurrentTreeFrame.ChildCount;
      if (siblingCount > 0 && Configuration.TreeSeparator.Length > 0) AppendRaw(Configuration.TreeSeparator);
    }

    private void PrepareForProperty() {
      var propertyCount = OwnerPropertyCount;
      if (Configuration.LineBreakProperties) EnsureNewLine();
      if (propertyCount == 0) AppendInjection(Configuration.BeforeProperties);
      else if (!Configuration.LineBreakProperties) AppendInjection(Configuration.PropertySeparator);
    }

    private void CompleteTree(ref Frame frame) {
      FinalizeProperties(ref frame.PropertyCount, ref frame.PropertiesFinalized);
      EnsureNewLine();
      if (frame.HasChild) SetBranchLast(ref frame.LastChild, true);
      if (frame.ChildCount > 0) AppendInjection(Configuration.Footer);
      AppendInjection(Configuration.MandatoryFooter);
      frame.TreeOutputStart = Math.Min(frame.TreeOutputStart, _builder.Length);
    }

    private void RecordCompletedTree(Frame frame) {
      var branch = new BranchInfo {
        Start = frame.TreeOutputStart,
        End = _builder.Length,
        LinkPosition = frame.TreeLinkPosition,
        Depth = frame.TreeDepth + 1
      };

      if (_treeDepth == 0) {
        _rootLastChild = branch;
        _rootHasChild = true;
        _rootTreeCount++;
      } else {
        ref var parent = ref CurrentTreeFrame;
        parent.LastChild = branch;
        parent.HasChild = true;
        parent.ChildCount++;
      }
    }

    private void RecordCompletedProperty() {
      if (_treeDepth == 0) _rootPropertyCount++;
      else CurrentTreeFrame.PropertyCount++;
    }

    private int OwnerPropertyCount => _treeDepth == 0 ? _rootPropertyCount : CurrentTreeFrame.PropertyCount;

    private bool OwnerPropertiesFinalized =>
      _treeDepth == 0 ? _rootPropertiesFinalized : CurrentTreeFrame.PropertiesFinalized;

    private void PrepareOwnerForChildren() {
      if (_treeDepth == 0) {
        FinalizeProperties(ref _rootPropertyCount, ref _rootPropertiesFinalized);
        if (_rootTreeCount == 0) AppendInjection(Configuration.BeforeChildren);
      } else {
        ref var owner = ref CurrentTreeFrame;
        FinalizeProperties(ref owner.PropertyCount, ref owner.PropertiesFinalized);
        if (owner.ChildCount == 0) AppendInjection(Configuration.BeforeChildren);
      }
    }

    private void FinalizeProperties(ref int propertyCount, ref bool finalized) {
      if (finalized) return;
      if (propertyCount > 0) AppendInjection(Configuration.AfterProperties);
      AppendInjection(Configuration.MandatoryAfterProperties);
      finalized = true;
    }

    private void FinalizeRoot() {
      if (_rootFinalized) return;
      FinalizeProperties(ref _rootPropertyCount, ref _rootPropertiesFinalized);
      if (_rootTreeCount > 0) AppendInjection(Configuration.Footer);
      AppendInjection(Configuration.MandatoryFooter);
      _rootFinalized = true;
    }

    private ref Frame CurrentTreeFrame {
      get {
        for (var i = _frameCount - 1; i >= 0; i--)
          if (_frames[i].Scope is ProseTree) return ref _frames[i];
        throw new InvalidOperationException("No active Prose tree frame.");
      }
    }

    private void SetBranchLast(ref BranchInfo branch, bool last) {
      if (branch.IsLast == last || Configuration.BranchWidth == 0) {
        branch.IsLast = last;
        return;
      }

      if (branch.LinkPosition >= 0)
        ReplaceToken(
          branch.LinkPosition,
          last ? Configuration.ChildPrefix : Configuration.LastChildPrefix,
          last ? Configuration.LastChildPrefix : Configuration.ChildPrefix
        );

      var tokenOffset = (branch.Depth - 1) * Configuration.BranchWidth;
      var lineStart = branch.Start;
      while (lineStart < branch.End) {
        var position = lineStart + tokenOffset;
        if (position != branch.LinkPosition)
          ReplaceToken(
            position,
            last ? Configuration.ContinuationPrefix : Configuration.LastContinuationPrefix,
            last ? Configuration.LastContinuationPrefix : Configuration.ContinuationPrefix
          );

        var nextBreak = IndexOfLineBreak(lineStart, branch.End);
        if (nextBreak < 0) break;
        lineStart = nextBreak + Configuration.LineBreak.Length;
      }

      branch.IsLast = last;
    }

    private void ReplaceToken(int position, string expected, string replacement) {
      if (position < 0 || position + expected.Length > _builder.Length) return;
      for (var i = 0; i < expected.Length; i++)
        if (_builder[position + i] != expected[i]) return;
      for (var i = 0; i < replacement.Length; i++) _builder[position + i] = replacement[i];
    }

    private int IndexOfLineBreak(int start, int end) {
      if (Configuration.LineBreak.Length == 0) return -1;
      for (var i = start; i <= end - Configuration.LineBreak.Length; i++)
        if (MatchesAt(Configuration.LineBreak, i)) return i;
      return -1;
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
          AppendDecoration(Configuration.RootNamePrefix);
          break;
        case LinePrefixKind.TreeName:
          AppendContinuationPrefixes(Math.Max(0, _linePrefixTreeDepth - 1));
          var treeFrameIndex = FindParentTreeFrameIndex();
          if (treeFrameIndex >= 0) _frames[treeFrameIndex].TreeLinkPosition = _builder.Length;
          AppendDecoration(Configuration.ChildPrefix);
          AppendDecoration(Configuration.TreeNamePrefix);
          break;
        case LinePrefixKind.NameContinuation:
          AppendContinuationPrefixes(_linePrefixTreeDepth);
          AppendDecoration(Configuration.NameContinuationPrefix);
          break;
        case LinePrefixKind.Property:
          if (_column == 0) AppendContinuationPrefixes(_linePrefixTreeDepth);
          AppendDecoration(
            _pendingLineBreakKind == LineBreakKind.Semantic
              ? Configuration.PropertyPrefix
              : Configuration.PropertyContinuationPrefix
          );
          break;
        case LinePrefixKind.PropertyValue:
          AppendContinuationPrefixes(_linePrefixTreeDepth);
          AppendDecoration(Configuration.PropertyContinuationPrefix);
          break;
      }

      if (_pendingLineBreakKind == LineBreakKind.Wrapped)
        AppendDecoration(Configuration.WrappedLinePrefix);
      else if (_pendingLineBreakKind == LineBreakKind.Explicit)
        AppendDecoration(Configuration.ExplicitLineBreakPrefix);
      if (_linePrefixKind == LinePrefixKind.PropertyValue &&
          Configuration.AlignWrappedPropertyValues &&
          _column < _propertyValueColumn)
        AppendSpaces(_propertyValueColumn - _column);

      _linePrefixWritten = true;
      _pendingLineBreakKind = LineBreakKind.Semantic;
    }

    private int FindParentTreeFrameIndex() {
      for (var i = _frameCount - 2; i >= 0; i--)
        if (_frames[i].Scope is ProseTree) return i;
      return -1;
    }

    private void AppendContinuationPrefixes(int count) {
      for (var i = 0; i < count; i++) AppendDecoration(Configuration.ContinuationPrefix);
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
        if (allowWrap && !whitespace && _lineHasContent && _column + length > WrapWidth)
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
        if (allowWrap && !whitespace && _lineHasContent && _column + length > WrapWidth)
          AppendLineBreak(false, LineBreakKind.Wrapped);
        if (whitespace && !_lineHasContent) continue;
        Append(text.Slice(start, length));
      }
    }

    private bool Includes(ProseLevel level) =>
      level != ProseLevel.Hidden && level != ProseLevel.Off && level >= MinimumLevel;

    private bool IsSuppressed(int frameIndex) => _frames[frameIndex].OutputStart < 0;
    private static bool IsSuppressedFrame(Frame frame) => frame.OutputStart < 0;
    private bool IsInactive(int frameIndex) => IsSuppressed(frameIndex) || _frames[frameIndex].Truncated;

    private bool IsWritingInactive() => _frameCount > 0 && IsFrameOrAncestorInactive(_frameCount - 1);

    private bool IsFrameOrAncestorInactive(int frameIndex) {
      for (var i = frameIndex; i >= 0; i--)
        if (IsInactive(i)) return true;
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
      _linePrefixWritten = _column == 0 ? false : frame.StartLinePrefixWritten;
      _pendingLineBreakKind = frame.StartPendingLineBreakKind;
    }

    private void EnsureNewLine() => AppendLineBreak(false, LineBreakKind.Semantic);

    private void AppendLineBreak(bool force, LineBreakKind kind) {
      if (!force && !_lineHasContent) return;
      if (Configuration.LineBreak.Length == 0) return;
      TrimCurrentLineEnd();
      _builder.Append(Configuration.LineBreak);
      _column = 0;
      _lineHasContent = false;
      if (_linePrefixKind is LinePrefixKind.RootName or LinePrefixKind.TreeName)
        _linePrefixKind = LinePrefixKind.NameContinuation;
      _linePrefixWritten = false;
      _pendingLineBreakKind = kind;
    }

    private void TrimCurrentLineEnd() {
      while (_builder.Length > 0) {
        var last = _builder[_builder.Length - 1];
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

    private void AppendRaw(string text) {
      if (text.Length == 0) return;
      var start = 0;
      for (var i = 0; i < text.Length; i++) {
        if (text[i] != '\n') continue;
        var length = i - start;
        if (length > 0 && text[i - 1] == '\r') length--;
        if (length > 0) {
          _builder.Append(text, start, length);
          _column += length;
          _lineHasContent = true;
        }
        AppendLineBreak(true, LineBreakKind.Semantic);
        start = i + 1;
      }
      if (start < text.Length) {
        _builder.Append(text, start, text.Length - start);
        _column += text.Length - start;
        _lineHasContent = true;
      }
      _linePrefixWritten = _column > 0;
    }

    private void AppendInjection(string text) => AppendRaw(text);

    private bool MatchesAt(string value, int position) {
      if (position < 0 || position + value.Length > _builder.Length) return false;
      for (var i = 0; i < value.Length; i++)
        if (_builder[position + i] != value[i]) return false;
      return true;
    }

    private bool EndsWithAt(string value, int end) {
      if (value.Length == 0 || end < value.Length) return false;
      return MatchesAt(value, end - value.Length);
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
        if (_modifiers[i] is TModifier) return true;
      return false;
    }

    private int FindModifierFrame<TModifier>() where TModifier : IProseModifier {
      for (var frameIndex = _frameCount - 1; frameIndex >= 0; frameIndex--) {
        var start = _frames[frameIndex].ModifierStart;
        var end = frameIndex + 1 < _frameCount ? _frames[frameIndex + 1].ModifierStart : _modifierCount;
        for (var modifierIndex = end - 1; modifierIndex >= start; modifierIndex--)
          if (_modifiers[modifierIndex] is TModifier) return frameIndex;
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
