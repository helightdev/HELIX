namespace HELIX.Theming {
  public static class ColorRoles {
    public const ColorRole Transparent = ColorRole.Transparent;
    public const ColorRole Primary = ColorRole.Primary;
    public const ColorRole Secondary = ColorRole.Secondary;
    public const ColorRole Tertiary = ColorRole.Tertiary;
    public const ColorRole Error = ColorRole.Error;
    public const ColorRole Surface = ColorRole.Surface;

    public const ColorRole OnPrimary = ColorRole.Primary | ColorRole.On;
    public const ColorRole OnSecondary = ColorRole.Secondary | ColorRole.On;
    public const ColorRole OnTertiary = ColorRole.Tertiary | ColorRole.On;
    public const ColorRole OnError = ColorRole.Error | ColorRole.On;
    public const ColorRole OnSurface = ColorRole.Surface | ColorRole.On;

    public const ColorRole PrimaryContainer = ColorRole.Primary | ColorRole.Container;
    public const ColorRole SecondaryContainer = ColorRole.Secondary | ColorRole.Container;
    public const ColorRole TertiaryContainer = ColorRole.Tertiary | ColorRole.Container;
    public const ColorRole ErrorContainer = ColorRole.Error | ColorRole.Container;
    public const ColorRole SurfaceContainer = ColorRole.Surface | ColorRole.Container;
    public const ColorRole SurfaceContainerLow = ColorRole.SurfaceLow | ColorRole.Container;
    public const ColorRole SurfaceContainerHigh = ColorRole.SurfaceHigh | ColorRole.Container;
    public const ColorRole SurfaceContainerHighest = ColorRole.SurfaceHighest | ColorRole.Container;

    public const ColorRole OnPrimaryContainer = ColorRole.Primary | ColorRole.Container | ColorRole.On;
    public const ColorRole OnSecondaryContainer = ColorRole.Secondary | ColorRole.Container | ColorRole.On;
    public const ColorRole OnTertiaryContainer = ColorRole.Tertiary | ColorRole.Container | ColorRole.On;
    public const ColorRole OnErrorContainer = ColorRole.Error | ColorRole.Container | ColorRole.On;
    public const ColorRole OnSurfaceContainer = ColorRole.Surface | ColorRole.Container | ColorRole.On;
    public const ColorRole OnSurfaceContainerLow = ColorRole.SurfaceLow | ColorRole.Container | ColorRole.On;
    public const ColorRole OnSurfaceContainerHigh = ColorRole.SurfaceHigh | ColorRole.Container | ColorRole.On;
    public const ColorRole OnSurfaceContainerHighest = ColorRole.SurfaceHighest | ColorRole.Container | ColorRole.On;

    public const ColorRole SurfaceInverse = ColorRole.SurfaceInverse;
    public const ColorRole OnSurfaceInverse = ColorRole.SurfaceInverse | ColorRole.On;
    public const ColorRole SurfaceVariant = ColorRole.SurfaceVariant;
    public const ColorRole OnSurfaceVariant = ColorRole.SurfaceVariant | ColorRole.On;

    public const ColorRole Scrim = ColorRole.Scrim;
    public const ColorRole Shadow = ColorRole.Shadow;
    public const ColorRole SurfaceTint = ColorRole.SurfaceTint;
    public const ColorRole Outline = ColorRole.Outline;
    public const ColorRole Focus = ColorRole.Focus;
    public const ColorRole DisabledLow = ColorRole.DisabledLow;
    public const ColorRole DisabledHigh = ColorRole.DisabledHigh;
  }
}