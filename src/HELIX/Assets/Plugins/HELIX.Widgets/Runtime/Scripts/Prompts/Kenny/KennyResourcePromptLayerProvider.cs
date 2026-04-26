using System.Collections.Generic;
using System.Text;
using HELIX.Widgets.Universal;

namespace HELIX.Widgets.Prompts.Kenny {

  public class KennyResourcePromptLayerProvider : SvgResourcePromptLayerProvider {
    public KennyVariant Variant { get; set; }
    public Dictionary<string, KennyVariant> Variants { get; }

    public KennyResourcePromptLayerProvider(
      string collectionPath,
      Dictionary<string, string> mapping
    ) : base(collectionPath) {
      Mapping = new Dictionary<string, string>();
      Variants = new Dictionary<string, KennyVariant>();

      foreach (var (k, v) in mapping) {
        var raw = v;

        KennyVariant variant = KennyVariant.None;
        var isOutline = v.Contains("+outline");
        if (isOutline) {
          raw = raw.Replace("+outline", "");
          variant |= KennyVariant.Outline;
        }

        var isIcon = v.Contains("+icon");
        if (isIcon) {
          raw = raw.Replace("+icon", "");
          variant |= KennyVariant.Icon;
        }

        var isVariant = v.Contains("+alternative");
        if (isVariant) {
          raw = raw.Replace("+alternative", "");
          variant |= KennyVariant.Alternative;
        }

        var isColored = v.Contains("+color");
        if (isColored) {
          raw = raw.Replace("+color", "");
          variant |= KennyVariant.Color;
        }

        Variants[k] = variant;
        Mapping[k] = raw;
      }
    }

    public bool TryGetBestMatch(string bindingPath, out string resourcePath) {
      resourcePath = null;
      if (Variants == null || !Variants.TryGetValue(bindingPath, out var available)) { return false; }

      if (!Mapping.TryGetValue(bindingPath, out var rawResourcePath)) { return false; }

      var used = Variant & available;
      while (true) {
        var builder = new StringBuilder();
        if (rawResourcePath.Contains("_button") && used.HasFlag(KennyVariant.Color)) {
          builder.Append(rawResourcePath.Replace("_button", "_button_color"));
        } else { builder.Append(rawResourcePath); }

        if (used.HasFlag(KennyVariant.Icon)) builder.Append("_icon");
        if (used.HasFlag(KennyVariant.Alternative)) builder.Append("_alternative");
        if (used.HasFlag(KennyVariant.Outline)) builder.Append("_outline");
        resourcePath = builder.ToString();

        if (TryGetImage(resourcePath, out _)) { break; }

        if (used == KennyVariant.None) return false;
        if (used.HasFlag(KennyVariant.Alternative)) used &= ~KennyVariant.Alternative;
        else if (used.HasFlag(KennyVariant.Color)) used &= ~KennyVariant.Color;
        else if (used.HasFlag(KennyVariant.Icon)) used &= ~KennyVariant.Icon;
        else if (used.HasFlag(KennyVariant.Outline)) used &= ~KennyVariant.Outline;
      }

      return true;
    }

    public override bool TryResolvePromptLayer(BuildContext context, string bindingPath, out SubstanceLayers layers) {
      layers = default;
      var success = TryGetBestMatch(bindingPath, out var resourcePath);
      if (!success) return false;
      layers = BuildSubstanceLayersFromResource(context, resourcePath);
      return true;
    }
  }
}