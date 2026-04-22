using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Substances;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Prompts {
  public interface IPromptProvider {
    bool TryResolvePrompt(BuildContext context, string bindingPath, out SubstanceLayers layers);
  }

  public abstract class SvgResourcePromptProvider : IPromptProvider {
    public Dictionary<string, string> Mapping { get; protected set; }
    public string CollectionPath { get; protected set; }
    public Color Tint { get; set; } = Color.white;
    private readonly Dictionary<string, VectorImage> _images;

    protected SvgResourcePromptProvider(string collectionPath) {
      CollectionPath = collectionPath;
      _images = new Dictionary<string, VectorImage>();
      var collection = Resources.Load<HelixSvgCollection>(CollectionPath);
      foreach (var prompt in collection.prompts) {
        _images[prompt.name.ToLowerInvariant()] = prompt;
      }
    }

    protected bool TryGetImage(string name, out VectorImage image) {
      return _images.TryGetValue(name.ToLowerInvariant(), out image);
    }

    public virtual bool TryResolvePrompt(BuildContext context, string bindingPath, out SubstanceLayers layers) {
      if (Mapping != null && Mapping.TryGetValue(bindingPath, out var resourcePath)) {
        layers = BuildSubstanceLayersFromResource(context, resourcePath);
        return true;
      }

      layers = default;
      return false;
    }

    public virtual SubstanceLayers BuildSubstanceLayersFromResource(BuildContext context, string resourcePath) {
      return new BoxSubstance {
        background = new BackgroundStyle {
          //color = Color.red,
          image = LoadBackgroundFromResource(resourcePath),
          fit = new BackgroundSize(BackgroundSizeType.Contain),
          imageTintColor = Tint,
        }
      };
    }

    public virtual Background LoadBackgroundFromResource(string resourcePath) {
      if (!TryGetImage(resourcePath, out var image) || !image) {
        Debug.LogError($"Could not find prompt image for {resourcePath} in collection {CollectionPath}");
        return new Background();
      }

      return Background.FromVectorImage(image);
    }
  }

  public class KennyResourcePromptProvider : SvgResourcePromptProvider {
    public KennyVariant Variant { get; set; }
    public Dictionary<string, KennyVariant> Variants { get; set; }

    public KennyResourcePromptProvider(
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
      if (Variants == null || !Variants.TryGetValue(bindingPath, out var available)) {
        return false;
      }

      if (!Mapping.TryGetValue(bindingPath, out var rawResourcePath)) {
        return false;
      }

      var used = Variant & available;
      while (true) {
        var builder = new StringBuilder();
        if (rawResourcePath.Contains("_button") && used.HasFlag(KennyVariant.Color)) {
          builder.Append(rawResourcePath.Replace("_button", "_button_color"));
        } else {
          builder.Append(rawResourcePath);
        }

        if (used.HasFlag(KennyVariant.Icon)) builder.Append("_icon");
        if (used.HasFlag(KennyVariant.Alternative)) builder.Append("_alternative");
        if (used.HasFlag(KennyVariant.Outline)) builder.Append("_outline");
        resourcePath = builder.ToString();

        if (TryGetImage(resourcePath, out _)) {
          break;
        }

        if (used == KennyVariant.None) return false;
        if (used.HasFlag(KennyVariant.Alternative)) used &= ~KennyVariant.Alternative;
        else if (used.HasFlag(KennyVariant.Color)) used &= ~KennyVariant.Color;
        else if (used.HasFlag(KennyVariant.Icon)) used &= ~KennyVariant.Icon;
        else if (used.HasFlag(KennyVariant.Outline)) used &= ~KennyVariant.Outline;
      }

      return true;
    }

    public override bool TryResolvePrompt(BuildContext context, string bindingPath, out SubstanceLayers layers) {
      layers = default;
      var success = TryGetBestMatch(bindingPath, out var resourcePath);
      if (!success) return false;
      layers = BuildSubstanceLayersFromResource(context, resourcePath);
      return true;
    }
  }

  [Flags]
  public enum KennyVariant {
    None = 0,
    Icon = 1 << 1,
    Alternative = 1 << 2,
    Color = 1 << 3,
    Outline = 1 << 4,
  }
}