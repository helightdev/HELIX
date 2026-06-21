using System;
using System.Collections.Generic;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Universal;
using UnityEngine;

namespace HELIX.Widgets.Forms {
  public abstract class FieldSpec : WidgetSpec {
    public string Path { get; set; }
    public IReadOnlyList<IFormValidator> Validators { get; set; }
    public ValidationMode ValidationMode { get; set; } = ValidationMode.OnChange;
    public bool Enabled { get; set; } = true;

    public FieldDecoration Decoration { get; set; } = new();

  }

  public class FieldDecoration {
    public Widget Label { get; set; }
    public Widget Description { get; set; }
    public Widget Icon { get; set; }
    public Widget Tooltip { get; set; }
    public Widget Before { get; set; }
    public Widget After { get; set; }
    public Widget Between { get; set; }
    public Widget Prefix { get; set; }
    public Widget Suffix { get; set; }
  }

  [Flags]
  public enum FieldDecorationMask {
    None = 0,

    Label = 1 << 0,
    Description = 1 << 1,
    Icon = 1 << 2,
    Tooltip = 1 << 3,
    Before = 1 << 4,
    After = 1 << 5,
    Between = 1 << 6,
    Prefix = 1 << 7,
    Suffix = 1 << 8,

    All = Label | Description | Icon | Tooltip | Before | After | Between | Prefix | Suffix,
  }

  public abstract class FieldSpec<T> : FieldSpec {
    public T DefaultValue { get; set; }

    public Type ValueType => typeof(T);
  }

  public class StringFieldSpec : FieldSpec<string> {
    public bool Multiline { get; set; } = false;
    public bool Autocorrect { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public bool IsPasswordField { get; set; } = false;
    public bool IsDelayed { get; set; } = false;
    public bool HideMobileInput { get; set; } = true;
    public TouchScreenKeyboardType KeyboardType { get; set; } = TouchScreenKeyboardType.Default;
    public char MaskChar { get; set; } = '*';
    public int MaxLength { get; set; } = -1;
  }
}