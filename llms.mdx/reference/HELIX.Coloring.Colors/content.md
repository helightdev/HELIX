# Colors (/reference/HELIX.Coloring.Colors)

# Colors

```
public static class Colors
```

## Red

```
public static Color Red { get; }
```

## Green

```
public static Color Green { get; }
```

## Blue

```
public static Color Blue { get; }
```

## Yellow

```
public static Color Yellow { get; }
```

## Cyan

```
public static Color Cyan { get; }
```

## Pink

```
public static Color Pink { get; }
```

## Purple

```
public static Color Purple { get; }
```

## DeepPurple

```
public static Color DeepPurple { get; }
```

## Indigo

```
public static Color Indigo { get; }
```

## LightBlue

```
public static Color LightBlue { get; }
```

## Teal

```
public static Color Teal { get; }
```

## Orange

```
public static Color Orange { get; }
```

## DeepOrange

```
public static Color DeepOrange { get; }
```

## Brown

```
public static Color Brown { get; }
```

## Grey

```
public static Color Grey { get; }
```

## BlueGrey

```
public static Color BlueGrey { get; }
```

## LightGreen

```
public static Color LightGreen { get; }
```

## Lime

```
public static Color Lime { get; }
```

## Amber

```
public static Color Amber { get; }
```

## Transparent

```
public static readonly Color Transparent
```

A gray color with no alpha. Aims to mitigate the effect of straight alpha blending.
Lerping/Blending with this color will most often yield better results than defaulting to black transparent.

## BlackTransparent

```
public static readonly Color BlackTransparent
```

## Black

```
public static readonly Color Black
```

## Black05

```
public static readonly Color Black05
```

## Black10

```
public static readonly Color Black10
```

## Black15

```
public static readonly Color Black15
```

## Black20

```
public static readonly Color Black20
```

## Black30

```
public static readonly Color Black30
```

## Black40

```
public static readonly Color Black40
```

## Black50

```
public static readonly Color Black50
```

## Black60

```
public static readonly Color Black60
```

## Black70

```
public static readonly Color Black70
```

## Black80

```
public static readonly Color Black80
```

## Black90

```
public static readonly Color Black90
```

## Black95

```
public static readonly Color Black95
```

## WhiteTransparent

```
public static readonly Color WhiteTransparent
```

## White

```
public static readonly Color White
```

## White05

```
public static readonly Color White05
```

## White10

```
public static readonly Color White10
```

## White15

```
public static readonly Color White15
```

## White20

```
public static readonly Color White20
```

## White30

```
public static readonly Color White30
```

## White40

```
public static readonly Color White40
```

## White50

```
public static readonly Color White50
```

## White60

```
public static readonly Color White60
```

## White70

```
public static readonly Color White70
```

## White80

```
public static readonly Color White80
```

## White90

```
public static readonly Color White90
```

## White95

```
public static readonly Color White95
```

## Hex(string)

```
public static Color Hex(string hex)
```

## Rgb(int, int, int)

```
public static Color Rgb(int r, int g, int b)
```

## Argb(int, int, int, int)

```
public static Color Argb(int a, int r, int g, int b)
```

## Argb(int)

```
public static Color Argb(int argb)
```

## Argb(uint)

```
public static Color Argb(uint argb)
```

## OkLch(float, float, float)

```
public static Color OkLch(float l, float c, float h)
```

## OkLab(float, float, float)

```
public static Color OkLab(float l, float a, float b)
```

## Hsv(float, float, float)

```
public static Color Hsv(float h, float s, float v)
```

## AlphaBlend(Color, Color)

```
public static Color AlphaBlend(Color background, Color foreground)
```

## WithOpacity(Color, float)

```
public static Color WithOpacity(this Color color, float alpha)
```

## MultiplyOpacity(Color, float)

```
public static Color MultiplyOpacity(this Color color, float alpha)
```

## ComputeLuminance(Color)

```
public static float ComputeLuminance(this Color gamma)
```

## ToHex(Color)

```
public static string ToHex(this Color color)
```