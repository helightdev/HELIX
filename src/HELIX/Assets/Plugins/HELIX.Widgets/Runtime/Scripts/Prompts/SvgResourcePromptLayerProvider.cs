using System.Collections.Generic;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Substances;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Prompts {
  public abstract class SvgResourcePromptLayerProvider : IPromptLayerProvider {
    public Dictionary<string, string> Mapping { get; protected set; }
    public string CollectionPath { get; protected set; }
    public Color Tint { get; set; } = Color.white;
    private readonly Dictionary<string, VectorImage> _images;

    protected SvgResourcePromptLayerProvider(string collectionPath) {
      CollectionPath = collectionPath;
      _images = new Dictionary<string, VectorImage>();
      var collection = Resources.Load<HelixSvgCollection>(CollectionPath);
      foreach (var prompt in collection.prompts) { _images[prompt.name.ToLowerInvariant()] = prompt; }
    }

    protected bool TryGetImage(string name, out VectorImage image) {
      return _images.TryGetValue(name.ToLowerInvariant(), out image);
    }

    public virtual bool TryResolvePromptLayer(BuildContext context, string bindingPath, out SubstanceLayers layers) {
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
}