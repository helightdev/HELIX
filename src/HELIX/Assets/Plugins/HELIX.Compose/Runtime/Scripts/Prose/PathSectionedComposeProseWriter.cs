using System;
using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Compose.Forms;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Prose {
  /// <summary>Optional presentation metadata attached to a path section.</summary>
  public sealed class PathSectionPresentationModifier : IProseModifier {
    public PathSectionPresentationModifier(
      string title = null, TextRole? titleRole = null, IconRef? icon = null, string description = null
    ) {
      Title = title;
      TitleRole = titleRole;
      Icon = icon;
      Description = description;
    }
    public string Title { get; }
    public TextRole? TitleRole { get; }
    public IconRef? Icon { get; }
    public string Description { get; }
  }

  public static class PathSectionModifiers {
    public static PathSectionPresentationModifier Presentation(
      string title = null, TextRole? titleRole = null, IconRef? icon = null, string description = null
    ) => new(title, titleRole, icon, description);

    public static PathSectionPresentationModifier Title(string title, TextRole? role = null) {
      if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("A section title is required.", nameof(title));
      return new PathSectionPresentationModifier(title: title, titleRole: role);
    }

    public static PathSectionPresentationModifier Icon(IconRef icon) => new(icon: icon);

    public static PathSectionPresentationModifier Description(string description) {
      if (string.IsNullOrWhiteSpace(description))
        throw new ArgumentException("A section description is required.", nameof(description));
      return new PathSectionPresentationModifier(description: description);
    }
  }

  /// <summary>An immutable, path-addressed intermediate representation of completed composables.</summary>
  public sealed class PathSectionedProse {
    public sealed class Section {
      internal readonly List<Composable> entries = new();
      internal readonly List<IProseModifier> modifiers = new();
      internal readonly List<Section> children = new();

      internal Section(FormPath path, string name) {
        Path = path;
        Name = name;
      }

      public FormPath Path { get; }
      public string Name { get; }
      public IReadOnlyList<Composable> Entries => entries;
      public IReadOnlyList<IProseModifier> Modifiers => modifiers;
      public IReadOnlyList<Section> Children => children;
    }

    internal PathSectionedProse(FormPathPool paths, Section root) {
      Paths = paths;
      Root = root;
    }

    public FormPathPool Paths { get; }
    public Section Root { get; }
  }

  /// <summary>
  /// Compose prose writer whose path scopes select independent output buffers. Nested path scopes are relative,
  /// so <c>Path("graphics")</c> followed by <c>Path("quality")</c> addresses <c>graphics.quality</c>.
  /// </summary>
  public sealed class PathSectionedComposeProseWriter : ComposeProseWriter {
    public struct PathScope : IDisposable {
      private PathSectionedComposeProseWriter _writer;
      internal PathScope(PathSectionedComposeProseWriter writer) => _writer = writer;

      public void Dispose() {
        var writer = _writer;
        _writer = null;
        writer?.EndPath();
      }
    }

    private sealed class PathFrame {
      public PathSectionedProse.Section section;
      public bool acceptsModifiers = true;
    }

    private readonly FormPathPool _paths;
    private readonly Dictionary<FormPath, PathSectionedProse.Section> _sections = new();
    private readonly List<PathFrame> _pathFrames = new();
    private readonly PathSectionedProse.Section _rootSection;

    public PathSectionedComposeProseWriter(
      FormPathPool paths = null, ProseReducer<Composable> reducer = null,
      ProseScopeDelegates<Composable> delegates = null
    ) : base(reducer ?? new ComposeProseReducer(), delegates) {
      _paths = paths ?? new FormPathPool();
      _rootSection = new PathSectionedProse.Section(_paths.Root, string.Empty);
      _sections.Add(_paths.Root, _rootSection);
    }

    public FormPathPool Paths => _paths;
    public FormPath CurrentPath => CurrentSection.Path;
    private PathSectionedProse.Section CurrentSection =>
      _pathFrames.Count == 0 ? _rootSection : _pathFrames[^1].section;

    public PathScope Path(string path) {
      if (HasActiveDelegation)
        throw new InvalidOperationException("A path cannot be switched inside a delegated Prose frame.");
      if (FrameCount != 0)
        throw new InvalidOperationException("A path can only be switched between completed Prose frames.");
      var resolved = _paths.ParseRelative(CurrentPath, path);
      var section = GetOrCreateSection(resolved);
      _pathFrames.Add(new PathFrame { section = section });
      return new PathScope(this);
    }

    public void EndPath() {
      if (HasActiveDelegation)
        throw new InvalidOperationException("A path cannot be popped inside a delegated Prose frame.");
      if (FrameCount != 0)
        throw new InvalidOperationException("All Prose frames in a path must be ended before the path is popped.");
      if (_pathFrames.Count == 0) throw new InvalidOperationException("There is no path context to pop.");
      _pathFrames.RemoveAt(_pathFrames.Count - 1);
    }

    protected override void BeforeBeginFrame(IProseScope scope) {
      StopPathModifiers();
    }

    protected override void PushModifierDirect(IProseModifier modifier) {
      if (FrameCount == 0 && _pathFrames.Count > 0 && _pathFrames[^1].acceptsModifiers) {
        CurrentSection.modifiers.Add(modifier);
        return;
      }
      base.PushModifierDirect(modifier);
    }

    public PathSectionedProse BuildSections() {
      if (HasActiveDelegation)
        throw new InvalidOperationException("All delegated Prose frames must be ended before building.");
      if (FrameCount != 0) throw new InvalidOperationException("All Prose frames must be ended before building.");
      if (_pathFrames.Count != 0)
        throw new InvalidOperationException("All path contexts must be ended before building.");
      return new PathSectionedProse(_paths, Snapshot(_rootSection));
    }

    public override Composable Build() => throw new InvalidOperationException(
      "Path-sectioned prose is an intermediate representation. Use BuildSections() and a projection instead."
    );

    public override void Reset() {
      base.Reset();
      _pathFrames.Clear();
      _sections.Clear();
      _rootSection.entries.Clear();
      _rootSection.modifiers.Clear();
      _rootSection.children.Clear();
      _sections.Add(_paths.Root, _rootSection);
    }

    protected override void AddReduced(Composable composable) {
      StopPathModifiers();
      if (FrameCount > 0) base.AddReduced(composable);
      else CurrentSection.entries.Add(composable);
    }

    private void StopPathModifiers() {
      if (_pathFrames.Count > 0) _pathFrames[^1].acceptsModifiers = false;
    }

    private PathSectionedProse.Section GetOrCreateSection(FormPath path) {
      if (_sections.TryGetValue(path, out var existing)) return existing;
      var parentPath = _paths.Parent(path);
      var parent = GetOrCreateSection(parentPath);
      var formatted = _paths.Format(path);
      var separator = formatted.LastIndexOf('.');
      var section = new PathSectionedProse.Section(path, formatted.Substring(separator + 1));
      _sections.Add(path, section);
      parent.children.Add(section);
      return section;
    }

    private PathSectionedProse.Section Snapshot(PathSectionedProse.Section source) {
      var result = new PathSectionedProse.Section(source.Path, source.Name);
      result.entries.AddRange(source.entries);
      result.modifiers.AddRange(source.modifiers);
      for (var i = 0; i < source.children.Count; i++) result.children.Add(Snapshot(source.children[i]));
      return result;
    }
  }

}
