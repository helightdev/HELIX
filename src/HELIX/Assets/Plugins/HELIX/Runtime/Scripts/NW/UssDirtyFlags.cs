using System;
using Unity.Profiling;
using UnityEngine.UIElements;

namespace HELIX.NW {
  [Flags]
  public enum UssDirtyFlags : uint {
    None = 0,
    Flex = 1 << 1,
    GroupAlign = 1 << 2,
    Padding = 1 << 3,
    Margin = 1 << 4,
    Size = 1 << 5,
    Position = 1 << 6,
    BorderColor = 1 << 7,
    BorderWidth = 1 << 8,
    Radius = 1 << 9,
    Background = 1 << 10,
    BackgroundSlice = 1 << 11,
    BackgroundAdvanced = 1 << 12,
    Transform = 1 << 13,
    Visibility = 1 << 14,
    Text = 1 << 15,
    TextFont = 1 << 16,
    TextOutline = 1 << 17,
    TextLayout = 1 << 18,
    Clipping = 1 << 19,
    Transition = 1 << 20,
    Special = 1 << 21,
    Classes = 1 << 22,
    Name = 1 << 23,
    Focus = 1 << 24,
  }

  public static class UssDirtyFlagsExtensions {
    private const uint _deBruijnMagic = 0x077CB531U;
    private static readonly int[] _deBruijnTable = new int[32] {
      0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
      31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
    };

    private static readonly ProfilerMarker _marker = new("HELIX.NW.UssDirty");
    private static readonly ProfilerMarker _markerState = new("HELIX.NW.UssDirtyReset");

    public static bool EnsureIdentity(this IComposable tracker, ulong typeId) {
      if (tracker.TypeId == typeId) return true;
      tracker.TypeId = typeId;

#if ENABLE_PROFILER
      _marker.Begin();
      var currentFlags = tracker.DirtyFlags;
      if (currentFlags != UssDirtyFlags.None) {
        currentFlags.ClearFlags(tracker.Element);
        tracker.DirtyFlags = UssDirtyFlags.None;
      }
      _marker.End();

      _markerState.Begin();
      tracker.Reset();
      _markerState.End();
#else
      var currentFlags = tracker.DirtyFlags;
      if (currentFlags != UssDirtyFlags.None) {
        currentFlags.ClearFlags(tracker.Element);
        tracker.DirtyFlags = UssDirtyFlags.None;
      }

      tracker.Reset();
#endif

      return false;
    }

    public static void ClearFlags(this UssDirtyFlags flags, VisualElement element) {
      flags.ClearFlags(element.style, element);
    }

    public static void ClearFlags(this UssDirtyFlags flags, IStyle style, VisualElement element) {
      var mask = (uint)flags;

      while (mask != 0) {
        var lowestBit = mask & (uint)-(int)mask;
        var bitIndex = _deBruijnTable[(lowestBit * _deBruijnMagic) >> 27];
        ClearFlag(style, element, (UssDirtyFlags)(1u << bitIndex));
        mask &= mask - 1;
      }
    }

    private static void ClearFlag(IStyle style, VisualElement element, UssDirtyFlags flag) {
      switch (flag) {
        case UssDirtyFlags.None: break;
        case UssDirtyFlags.Flex:
          style.flexGrow = StyleKeyword.Null;
          style.flexShrink = StyleKeyword.Null;
          style.flexBasis = StyleKeyword.Null;
          break;
        case UssDirtyFlags.GroupAlign:
          style.justifyContent = StyleKeyword.Null;
          style.alignItems = StyleKeyword.Null;
          style.alignContent = StyleKeyword.Null;
          style.flexDirection = StyleKeyword.Null;
          style.flexWrap = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Padding:
          style.paddingLeft = StyleKeyword.Null;
          style.paddingRight = StyleKeyword.Null;
          style.paddingTop = StyleKeyword.Null;
          style.paddingBottom = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Margin:
          style.marginLeft = StyleKeyword.Null;
          style.marginRight = StyleKeyword.Null;
          style.marginTop = StyleKeyword.Null;
          style.marginBottom = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Size:
          style.width = StyleKeyword.Null;
          style.height = StyleKeyword.Null;
          style.minWidth = StyleKeyword.Null;
          style.minHeight = StyleKeyword.Null;
          style.maxWidth = StyleKeyword.Null;
          style.maxHeight = StyleKeyword.Null;
          style.aspectRatio = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Position:
          style.left = StyleKeyword.Null;
          style.top = StyleKeyword.Null;
          style.right = StyleKeyword.Null;
          style.bottom = StyleKeyword.Null;
          style.position = StyleKeyword.Null;
          break;
        case UssDirtyFlags.BorderColor:
          style.borderLeftColor = StyleKeyword.Null;
          style.borderRightColor = StyleKeyword.Null;
          style.borderTopColor = StyleKeyword.Null;
          style.borderBottomColor = StyleKeyword.Null;
          break;
        case UssDirtyFlags.BorderWidth:
          style.borderLeftWidth = StyleKeyword.Null;
          style.borderRightWidth = StyleKeyword.Null;
          style.borderTopWidth = StyleKeyword.Null;
          style.borderBottomWidth = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Radius:
          style.borderTopLeftRadius = StyleKeyword.Null;
          style.borderTopRightRadius = StyleKeyword.Null;
          style.borderBottomRightRadius = StyleKeyword.Null;
          style.borderBottomLeftRadius = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Background:
          style.backgroundColor = StyleKeyword.Null;
          style.backgroundImage = StyleKeyword.Null;
          style.backgroundSize = StyleKeyword.Null;
          style.backgroundRepeat = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Transform:
          style.transformOrigin = StyleKeyword.Null;
          style.translate = StyleKeyword.Null;
          style.rotate = StyleKeyword.Null;
          style.scale = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Visibility:
          style.display = StyleKeyword.Null;
          style.visibility = StyleKeyword.Null;
          style.opacity = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Text:
          style.color = StyleKeyword.Null;
          style.fontSize = StyleKeyword.Null;
          style.unityFontStyleAndWeight = StyleKeyword.Null;
          style.unityTextAlign = StyleKeyword.Null;
          break;
        case UssDirtyFlags.TextFont:
          style.unityFont = StyleKeyword.Null;
          style.unityFontDefinition = StyleKeyword.Null;
          style.letterSpacing = StyleKeyword.Null;
          style.wordSpacing = StyleKeyword.Null;
          style.unityParagraphSpacing = StyleKeyword.Null;
          break;
        case UssDirtyFlags.TextOutline:
          style.unityTextOutlineColor = StyleKeyword.Null;
          style.unityTextOutlineWidth = StyleKeyword.Null;
          style.textShadow = StyleKeyword.Null;
          break;
        case UssDirtyFlags.TextLayout:
          style.whiteSpace = StyleKeyword.Null;
          style.textOverflow = StyleKeyword.Null;
          style.unityTextOverflowPosition = StyleKeyword.Null;
          style.unityTextAutoSize = StyleKeyword.Null;
          style.unityTextGenerator = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Transition:
          style.transitionDelay = StyleKeyword.Null;
          style.transitionDuration = StyleKeyword.Null;
          style.transitionTimingFunction = StyleKeyword.Null;
          style.transitionProperty = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Clipping:
          style.overflow = StyleKeyword.Null;
          style.unityOverflowClipBox = StyleKeyword.Null;
          break;
        case UssDirtyFlags.BackgroundSlice:
          style.unitySliceTop = StyleKeyword.Null;
          style.unitySliceBottom = StyleKeyword.Null;
          style.unitySliceLeft = StyleKeyword.Null;
          style.unitySliceRight = StyleKeyword.Null;
          style.unitySliceScale = StyleKeyword.Null;
          style.unitySliceType = StyleKeyword.Null;
          break;
        case UssDirtyFlags.BackgroundAdvanced:
          style.backgroundPositionX = StyleKeyword.Null;
          style.backgroundPositionY = StyleKeyword.Null;
          style.unityBackgroundImageTintColor = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Special:
          style.filter = StyleKeyword.Null;
          style.unityMaterial = StyleKeyword.Null;
          style.cursor = StyleKeyword.Null;
          break;
        case UssDirtyFlags.Classes:
          element.ClearClassList();
          break;
        case UssDirtyFlags.Name:
          element.name = null;
          break;
        case UssDirtyFlags.Focus:
          element.pickingMode = PickingMode.Position;
          element.focusable = false;
          element.delegatesFocus = false;
          element.tabIndex = -1; // TODO: Probably when initializing I need to make sure the element is initially this
          break;

        default: throw new ArgumentOutOfRangeException(nameof(flag), flag, null);
      }
    }
  }
}