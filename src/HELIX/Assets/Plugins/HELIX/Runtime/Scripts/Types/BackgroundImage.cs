using System;
using HELIX.Compose;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
  public readonly struct BackgroundImage : IEquatable<BackgroundImage> {
    public static readonly BackgroundImage Default = new(default, null, null, null);

    public readonly Background image;
    public readonly ImageScaling? scaling;
    public readonly ImageSlice? slice;
    public readonly ImageAnchor? anchor;

    public BackgroundImage(
      Background image, ImageScaling? scaling, ImageSlice? slice, ImageAnchor? anchor
    ) {
      this.image = image;
      this.scaling = scaling;
      this.slice = slice;
      this.anchor = anchor;
    }

    public static implicit operator BackgroundImage(Texture2D texture) => new(
      Background.FromTexture2D(texture), null, null, null
    );

    public static implicit operator BackgroundImage(Sprite sprite) => new(
      Background.FromSprite(sprite), null, null, null
    );

    public static implicit operator BackgroundImage(VectorImage vectorImage) => new(
      Background.FromVectorImage(vectorImage), null, null, null
    );

    public static implicit operator BackgroundImage(RenderTexture renderTexture) => new(
      Background.FromRenderTexture(renderTexture), null, null, null
    );

    public static BackgroundImage Texture2D(
      Texture2D texture, ImageScaling? scaling = null, ImageSlice? slice = null, ImageAnchor? anchor = null
    ) => new(Background.FromTexture2D(texture), scaling, slice, anchor);

    public static BackgroundImage Sprite(
      Sprite sprite, ImageScaling? scaling = null, ImageSlice? slice = null, ImageAnchor? anchor = null
    ) => new(Background.FromSprite(sprite), scaling, slice, anchor);

    public static BackgroundImage VectorImage(
      VectorImage vectorImage, ImageScaling? scaling = null, ImageSlice? slice = null, ImageAnchor? anchor = null
    ) => new(Background.FromVectorImage(vectorImage), scaling, slice, anchor);

    public static BackgroundImage RenderTexture(
      RenderTexture renderTexture, ImageScaling? scaling = null, ImageSlice? slice = null, ImageAnchor? anchor = null
    ) => new(Background.FromRenderTexture(renderTexture), scaling, slice, anchor);

    public void ApplyShallow(IStyle style) {
      style.backgroundImage = image;
      slice?.Apply(style);
      scaling?.Apply(style);
      anchor?.Apply(style);
    }

    public void Apply(IStyle style) {
      style.backgroundImage = image;
      if (slice.HasValue) slice.Value.Apply(style);
      else ImageSlice.Unset(style);
      if (scaling.HasValue) scaling.Value.Apply(style);
      else ImageScaling.Unset(style);
      if (anchor.HasValue) anchor.Value.Apply(style);
      else ImageAnchor.Unset(style);
    }

    public void Apply(IStyle style, out UssFlag flags) {
      flags = UssFlag.Background | UssFlag.BackgroundAdvanced;
      style.backgroundImage = image;
      scaling?.Apply(style);
      anchor?.Apply(style);

      if (slice.HasValue) {
        slice.Value.Apply(style);
        flags |= UssFlag.BackgroundSlice;
      }
    }

    public static void Unset(IStyle style) {
      style.backgroundImage = StyleKeyword.Null;
      ImageSlice.Unset(style);
      ImageScaling.Unset(style);
      ImageAnchor.Unset(style);
    }


    public bool Equals(BackgroundImage other) {
      return image.Equals(other.image) && Nullable.Equals(scaling, other.scaling) &&
             Nullable.Equals(slice, other.slice) && Nullable.Equals(anchor, other.anchor);
    }

    public override bool Equals(object obj) {
      return obj is BackgroundImage other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(image, scaling, slice, anchor);
    }

    public override string ToString() {
      return
        $"{nameof(image)}: {image}, {nameof(scaling)}: {scaling}, {nameof(slice)}: {slice}, {nameof(anchor)}: {anchor}";
    }
  }

  public readonly struct ImageAnchor : IEquatable<ImageAnchor> {
    public readonly BackgroundPosition x, y;

    public ImageAnchor(BackgroundPosition x, BackgroundPosition y) {
      this.x = x;
      this.y = y;
    }

    public void Apply(IStyle style) {
      style.backgroundPositionX = x;
      style.backgroundPositionY = y;
    }

    public StyleLength4 ToPosition() {
      var length = StyleLength4.Null;
      switch (x.keyword) {
        case BackgroundPositionKeyword.Left: length.l = x.offset; break;
        case BackgroundPositionKeyword.Right: length.r = x.offset; break;
        default: length.l = Length.Percent(50f); break;
      }

      switch (y.keyword) {
        case BackgroundPositionKeyword.Top: length.t = y.offset; break;
        case BackgroundPositionKeyword.Bottom: length.b = y.offset; break;
        default: length.t = Length.Percent(50f); break;
      }
      return length;
    }

    public static void Unset(IStyle style) {
      style.backgroundPositionX = StyleKeyword.Null;
      style.backgroundPositionY = StyleKeyword.Null;
    }

    public static readonly ImageAnchor Center = new(
      new BackgroundPosition(BackgroundPositionKeyword.Center),
      new BackgroundPosition(BackgroundPositionKeyword.Center)
    );

    public static readonly ImageAnchor Top = new(
      new BackgroundPosition(BackgroundPositionKeyword.Center),
      new BackgroundPosition(BackgroundPositionKeyword.Top)
    );

    public static readonly ImageAnchor Bottom = new(
      new BackgroundPosition(BackgroundPositionKeyword.Center),
      new BackgroundPosition(BackgroundPositionKeyword.Bottom)
    );

    public static readonly ImageAnchor Left = new(
      new BackgroundPosition(BackgroundPositionKeyword.Left),
      new BackgroundPosition(BackgroundPositionKeyword.Center)
    );

    public static readonly ImageAnchor Right = new(
      new BackgroundPosition(BackgroundPositionKeyword.Right),
      new BackgroundPosition(BackgroundPositionKeyword.Center)
    );

    public static readonly ImageAnchor TopLeft = new(
      new BackgroundPosition(BackgroundPositionKeyword.Left),
      new BackgroundPosition(BackgroundPositionKeyword.Top)
    );

    public static readonly ImageAnchor TopRight = new(
      new BackgroundPosition(BackgroundPositionKeyword.Right),
      new BackgroundPosition(BackgroundPositionKeyword.Top)
    );

    public static readonly ImageAnchor BottomLeft = new(
      new BackgroundPosition(BackgroundPositionKeyword.Left),
      new BackgroundPosition(BackgroundPositionKeyword.Bottom)
    );

    public static readonly ImageAnchor BottomRight = new(
      new BackgroundPosition(BackgroundPositionKeyword.Right),
      new BackgroundPosition(BackgroundPositionKeyword.Bottom)
    );

    public ImageAnchor WithOffset(Length horizontal, Length height) {
      // TL Corrected
      return new ImageAnchor(Offset(x, horizontal), Offset(y, height));
    }

    private static BackgroundPosition Offset(BackgroundPosition position, Length offset) {
      return position.keyword switch {
        BackgroundPositionKeyword.Center => new BackgroundPosition(BackgroundPositionKeyword.Center),
        BackgroundPositionKeyword.Bottom or BackgroundPositionKeyword.Right =>
          new BackgroundPosition(position.keyword, StyleLengths.Negate(offset)),
        _ => new BackgroundPosition(position.keyword, offset)
      };
    }

    public bool Equals(ImageAnchor other) {
      return x.Equals(other.x) && y.Equals(other.y);
    }

    public override bool Equals(object obj) {
      return obj is ImageAnchor other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(x, y);
    }
  }

  public readonly struct ImageScaling : IEquatable<ImageScaling> {
    public readonly BackgroundSize fit;
    public readonly BackgroundRepeat repeat;

    public ImageScaling(BackgroundSize fit, BackgroundRepeat repeat) {
      this.fit = fit;
      this.repeat = repeat;
    }

    public void Apply(IStyle style) {
      style.backgroundSize = fit;
      style.backgroundRepeat = repeat;
    }


    public static void Unset(IStyle style) {
      style.backgroundSize = StyleKeyword.Null;
      style.backgroundRepeat = StyleKeyword.Null;
    }

    public static ImageScaling RepeatX(Length size, Repeat mode = Repeat.Repeat) {
      return new ImageScaling(
        new BackgroundSize(size, Length.Percent(100f)),
        new BackgroundRepeat(mode, Repeat.NoRepeat)
      );
    }

    public static ImageScaling RepeatY(Length size, Repeat mode = Repeat.Repeat) {
      return new ImageScaling(
        new BackgroundSize(Length.Percent(100f), size),
        new BackgroundRepeat(Repeat.NoRepeat, mode)
      );
    }

    public static ImageScaling Tile(
      Length sizeX, Length sizeY,
      Repeat modeX = Repeat.Repeat, Repeat modeY = Repeat.Repeat
    ) {
      return new ImageScaling(
        new BackgroundSize(sizeX, sizeY),
        new BackgroundRepeat(modeX, modeY)
      );
    }

    public static readonly ImageScaling Stretch = new(
      new BackgroundSize(Length.Percent(100f), Length.Percent(100f)),
      new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat)
    );

    public static readonly ImageScaling Cover = new(
      new BackgroundSize(BackgroundSizeType.Cover),
      new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat)
    );

    public static readonly ImageScaling Contain = new(
      new BackgroundSize(BackgroundSizeType.Contain),
      new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat)
    );

    public bool Equals(ImageScaling other) {
      return fit.Equals(other.fit) && repeat.Equals(other.repeat);
    }

    public override bool Equals(object obj) {
      return obj is ImageScaling other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(fit, repeat);
    }
  }

  public readonly struct ImageSlice : IEquatable<ImageSlice> {
    public readonly int4 insets;
    public readonly float scale;
    public readonly SliceType type;

    public ImageSlice(int4 insets, float scale, SliceType type) {
      this.insets = insets;
      this.scale = scale;
      this.type = type;
    }

    public static ImageSlice Of(
      int left, int top, int right, int bottom, float scale = 1f, SliceType type = SliceType.Sliced
    ) {
      return new ImageSlice(new int4(left, top, right, bottom), scale, type);
    }

    public static ImageSlice All(int inset, float scale = 1f, SliceType type = SliceType.Sliced) {
      return new ImageSlice(new int4(inset), scale, type);
    }

    public void Apply(IStyle style) {
      style.unitySliceLeft = insets.x;
      style.unitySliceTop = insets.y;
      style.unitySliceRight = insets.z;
      style.unitySliceBottom = insets.w;
      style.unitySliceScale = scale;
      style.unitySliceType = type;
    }

    public static void Unset(IStyle style) {
      style.unitySliceLeft = StyleKeyword.Null;
      style.unitySliceTop = StyleKeyword.Null;
      style.unitySliceRight = StyleKeyword.Null;
      style.unitySliceBottom = StyleKeyword.Null;
      style.unitySliceScale = StyleKeyword.Null;
      style.unitySliceType = StyleKeyword.Null;
    }

    public bool Equals(ImageSlice other) {
      return insets.Equals(other.insets) && scale.Equals(other.scale) && type == other.type;
    }

    public override bool Equals(object obj) {
      return obj is ImageSlice other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(insets, scale, (int)type);
    }
  }
}