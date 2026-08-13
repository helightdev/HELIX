using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using HELIX.Prose;
using UnityEngine;
using ProseApi = HELIX.Prose.Prose;

namespace HELIX.Examples {
  /// <summary>A deliberately broad immediate-mode Prose sample used by the NW system example.</summary>
  public sealed class DetailedProseExample : IProse {
    private enum StationState : byte { Nominal, Degraded, Emergency }

    private static readonly DetailedProseExample Instance = new();

    private static readonly ProsePropertyFormatter<string> MissionId =
      new("Mission ID", new ProseStringFormatter(prefix: "HX-"));

    private static readonly ProsePropertyFormatter<int> CrewAboard =
      new("Crew aboard", new ProseIntFormatter(min: 0, max: 64, suffix: " people"));

    private static readonly ProsePropertyFormatter<int> OrbitNumber =
      new("Orbit", new ProseIntFormatter(min: 1, suffix: " completed"));

    private static readonly ProsePropertyFormatter<int> Temperature =
      new("Temperature", new ProseIntFormatter(min: -80, max: 180, suffix: " °C"));

    private static readonly ProsePropertyFormatter<int> PowerOutput =
      new("Output", new ProseIntFormatter(min: 0, max: 100, suffix: "%"));

    private static readonly ProsePropertyFormatter<int> SignalStrength =
      new("Signal", new ProseIntFormatter(min: 0, max: 100, suffix: "%"));

    private static readonly ProseBoolFormatter OperationalState = new("operational", "offline");
    private static readonly ProseBoolFormatter EnabledState = new("enabled", "disabled");
    private static readonly ProseIntFormatter Percent = new(min: 0, max: 100, suffix: "%");
    private static readonly ProseFloatFormatter Megawatts = new("0.00");

    /// <summary>A deliberately ornate style used to exercise wide branch tokens and decorations.</summary>
    public static readonly ProsePlainTextConfiguration TestWideDecorated = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Line(prefix: "╔═ ", suffix: " ═╗"),
      treeName: PTRuleFactory.Line(
        prefix: "[ ", suffix: " ] ─", suffixRepeater: 3
      ),
      property: PTRuleFactory.Property(
        firstLinePrefix: "• ", continuationPrefix: "  "
      ),
      propertyValue: PTRuleFactory.PropertyValue(
        continuationPrefix: "↳ ", align: false
      ),
      tree: PTRuleFactory.Tree("├── ", "└── ", "│   ", "    ")
    );

    /// <summary>A compact test style with visible sections and deliberately unaligned wrapping.</summary>
    public static readonly ProsePlainTextConfiguration TestCompactSections = new(
      root: PTRuleFactory.Container(),
      rootName: PTRuleFactory.Line(prefix: "# "),
      treeName: PTRuleFactory.Line(
        prefix: "{ ", suffix: " }", continuationPrefix: "  "
      ),
      property: new PTNodeFormat(
        prefix: new[] {
          new PTStringRule(TextMatching.First, 0, "[ "),
          new PTStringRule(TextMatching.None, 0, "; ")
        },
        suffix: new[] { new PTStringRule(TextMatching.Last, 0, " ]") },
        indent: new[] {
          new PTIndentRule(TextMatching.None, LineMatching.First, 0, "- "),
          new PTIndentRule(TextMatching.None, LineMatching.Hard, 0, "!  "),
          new PTIndentRule(TextMatching.None, LineMatching.None, 0, ".. ")
        },
        lines: PTRuleFactory.LineBreaks(LineBreakMode.Wrap | LineBreakMode.Hard)
      ),
      propertyValue: PTRuleFactory.PropertyValue(
        separator: " = ", continuationPrefix: "  ", align: false
      ),
      tree: PTRuleFactory.Tree("> ", "= ", ": ", "  ")
    );

    private DetailedProseExample() { }

    public static string RenderPlainText() => RenderPlainText(ProsePlainTextConfigurations.Sparse);

    public static string RenderPlainText(ProsePlainTextConfiguration configuration) {
      var writer = new ProseTextWriter(
        wrapWidth: 96,
        minimumLevel: ProseLevel.Debug,
        maxTruncatableFrameLength: 1024,
        initialCapacity: 2048,
        configuration: configuration
      );
      writer.Write(Instance);
      return writer.Build();
    }


    private static readonly ProseUnityRichTextWriter _writer = new(
      wrapWidth: 96,
      minimumLevel: ProseLevel.Debug,
      maxTruncatableFrameLength: 1024,
      initialCapacity: 2048
    );

    public static string RenderUnityRichText() {
      _writer.Reset();
      _writer.Write(Instance);
      return _writer.Build();
    }

    public static string RenderDictionary() {
      var writer = new ProseDictionaryWriter(initialFrameCapacity: 32);
      writer.Write(Instance);
      return ProseDictionaryText.Format(writer.Root);
    }

    public static void PrintPlainText() =>
      PrintPlainText("Sparse tree", ProsePlainTextConfigurations.Sparse);

    public static void PrintPlainText(string configurationName, ProsePlainTextConfiguration configuration) =>
      Debug.Log("Detailed Prose · " + configurationName + "\n" + RenderPlainText(configuration));

    public static void PrintDictionary() => Debug.Log("Detailed Prose · dictionary\n" + RenderDictionary());

    public static void PrintUnityRichText() =>
      Debug.Log("Detailed Prose · Unity rich text\n" + RenderUnityRichText());

    public void ToProse(IProseWriter writer) {
      ProseApi.WriteName(writer, "Asteria Orbital Relay Station");
      writer.Write("ASTERIA-07", MissionId);

      WriteMissionBriefing(writer);
      WriteOperatorNote(writer);
      writer.Write(ProseSoftLineBreak.Instance);
      writer.Write(ProseLineBreak.Instance);

      ProseApi.WriteProperty(writer, "State", StationState.Degraded, ProseEnumFormatter<StationState>.Instance);
      writer.Write(37, CrewAboard);
      writer.Write(1842, OrbitNumber);
      ProseApi.WriteProperty(writer, "Autonomous control", true, OperationalState);
      ProseApi.WriteProperty(
        writer,
        "Summary",
        "Long-range relay and research platform holding a stable polar orbit while its secondary coolant " +
        "loop is isolated for inspection.",
        ProseStringFormatter.Instance
      );
      ProseApi.WriteProperty(
        writer,
        "Internal tracking token",
        "OPS-4A-9912",
        ProseStringFormatter.Instance,
        level: ProseLevel.Debug,
        hidden: true,
        noWrap: true
      );
      WriteCommandDeck(writer);
      WritePowerGrid(writer);
      WriteCommunications(writer);
      WriteScienceAndCargo(writer);
      WriteAlerts(writer);
    }

    private static void WriteMissionBriefing(IProseWriter writer) {
      if (!writer.BeginFrame(ProseSection.Instance)) return;
      try {
        ProseApi.WriteSectionHeader(writer, "Mission briefing");

        if (writer.BeginFrame(ProseParagraph.Instance)) {
          try {
            writer.Write("Station state is ");
            ProseApi.WriteSpan(writer, "degraded", ProseTextStyle.Strong);
            writer.Write(" while the ");
            ProseApi.WriteSpan(writer, "secondary coolant loop", ProseTextStyle.Emphasis);
            writer.Write(" remains isolated under tracking token ");
            ProseApi.WriteSpan(writer, "OPS-4A-9912", ProseTextStyle.Code);
            writer.Write(". Follow the ");
            ProseApi.WriteSpan(
              writer,
              "thermal recovery runbook",
              linkTarget: "https://helix.local/runbooks/thermal-recovery"
            );
            writer.Write(" until inspection is complete.");
          } finally {
            writer.PopFrame();
          }
        }

        if (writer.BeginFrame(ProseParagraph.Instance)) {
          try {
            ProseApi.WriteSpan(
              writer,
              "Keep reactor B below 70% output until valve C17-B passes its pressure cycle.",
              ProseTextStyle.Error
            );
          } finally {
            writer.PopFrame();
          }
        }

        ProseApi.WriteCodeBlock(
          writer,
          "coolant isolate C17 --tracking OPS-4A-9912\n" +
          "thermal recover --limit-reactor-b 70% --cycles 2",
          "helix"
        );

        if (writer.BeginFrame(new ProseList(ProseListKind.Ordered))) {
          try {
            ProseApi.WriteListItem(writer, "Verify coolant isolation telemetry.");
            ProseApi.WriteListItem(writer, "Inspect valve C17-B and the secondary pump manifold.");
            ProseApi.WriteListItem(writer, "Return the loop to service after two stable pressure cycles.");
          } finally {
            writer.PopFrame();
          }
        }

        WriteSubsystemTable(writer);
      } finally {
        writer.PopFrame();
      }
    }

    private static void WriteSubsystemTable(IProseWriter writer) {
      if (!writer.BeginFrame(ProseTable.Instance)) return;
      try {
        if (writer.BeginFrame(ProseTableRow.Header)) {
          try {
            ProseApi.WriteTableCell(writer, "Subsystem");
            ProseApi.WriteTableCell(writer, "State", ProseTextAlignment.Center);
            ProseApi.WriteTableCell(writer, "Load", ProseTextAlignment.Right);
            ProseApi.WriteTableCell(writer, "Owner");
          } finally {
            writer.PopFrame();
          }
        }

        WriteSubsystemRow(writer, "Primary reactor", "Nominal", 91, "Power");
        WriteSubsystemRow(writer, "Reactor B", "Recovery", 43, "Power");
        WriteSubsystemRow(writer, "Coolant loop C17", "Isolated", 0, "Engineering");
        WriteSubsystemRow(writer, "Relay array\nThis has a linebreak", "Operational but this is a very very long line, I don't know if it can actually handle this. Operational but this is a very very long line, I don't know if it can actually handle this.", 97, "Communications");
        WriteSubsystemRow(writer, "Relay array", "Operational", 97, "Communications");
      } finally {
        writer.PopFrame();
      }
    }

    private static void WriteSubsystemRow(
      IProseWriter writer, string subsystem, string state, int load, string owner
    ) {
      if (!writer.BeginFrame(ProseTableRow.Body)) return;
      try {
        ProseApi.WriteTableCell(writer, subsystem);
        ProseApi.WriteTableCell(writer, state, ProseTextAlignment.Center);
        ProseApi.WriteTableCell(writer, load, Percent, ProseTextAlignment.Right);
        ProseApi.WriteTableCell(writer, owner);
      } finally {
        writer.PopFrame();
      }
    }

    private static void WriteOperatorNote(IProseWriter writer) {
      if (!writer.BeginFrame(ProseSection.Instance)) return;
      try {
        if (!writer.BeginFrame(ProseParagraph.Instance)) return;
        try {
          ProseApi.WriteSpan(
            writer,
            "Operator note: the relay remains mission-capable; prioritize thermal stability over throughput.",
            ProseTextStyle.Quote
          );
        } finally {
          writer.PopFrame();
        }
      } finally {
        writer.PopFrame();
      }
    }

    private static void WriteCommandDeck(IProseWriter writer) {
      if (!writer.BeginFrame(ProseTree.Instance)) return;
      try {
        ProseApi.WriteName(writer, "Command deck");
        ProseApi.WriteProperty(writer, "Watch officer", "Cmdr. Imani Vale", ProseStringFormatter.Instance);
        ProseApi.WriteProperty(writer, "Shift", "Gamma", ProseStringFormatter.Instance);
        ProseApi.WriteProperty(writer, "Navigation lock", true, EnabledState);
        ProseApi.WriteProperty(writer, "Attitude error", 0.04f, ProseFloatFormatter.Instance);
        ProseApi.WriteProperty(writer, "Next maneuver", "2026-08-12 21:40 UTC", ProseStringFormatter.Instance);

        if (!writer.BeginFrame(ProseTree.Instance)) return;
        try {
          ProseApi.WriteName(writer, "Crew manifest");
          ProseApi.WriteProperty(writer, "Command", 4, ProseIntFormatter.Instance);
          ProseApi.WriteProperty(writer, "Engineering", 12, ProseIntFormatter.Instance);
          ProseApi.WriteProperty(writer, "Science", 9, ProseIntFormatter.Instance);
          ProseApi.WriteProperty(writer, "Operations", 8, ProseIntFormatter.Instance);
          ProseApi.WriteProperty(writer, "Medical", 4, ProseIntFormatter.Instance);
        } finally {
          writer.PopFrame();
        }
      } finally {
        writer.PopFrame();
      }
    }

    private static void WritePowerGrid(IProseWriter writer) {
      if (!writer.BeginFrame(ProseTree.Instance)) return;
      try {
        ProseApi.WriteName(writer, "Power grid");
        ProseApi.WriteProperty(writer, "Grid state", "Load balanced", ProseStringFormatter.Instance);
        ProseApi.WriteProperty(writer, "Battery reserve", 78, Percent);
        ProseApi.WriteProperty(writer, "Solar tracking", true, EnabledState);
        ProseApi.WriteProperty(writer, "Peak demand (MW)", 18.72f, Megawatts);

        WriteReactor(writer, "Fusion reactor A", 91, 612, true, "Primary bus");
        WriteReactor(writer, "Fusion reactor IR");
        WriteReactor(writer, "Fusion reactor B", 43, 487, true, "Reserve and thermal recovery");

        if (!writer.BeginFrame(ProseTree.Instance)) return;
        try {
          ProseApi.WriteName(writer, "Solar array wings");
          ProseApi.WriteProperty(writer, "Port wing", 96, Percent);
          ProseApi.WriteProperty(writer, "Starboard wing", 94, Percent);
          ProseApi.WriteProperty(writer, "Sun incidence", 88, Percent);
          ProseApi.WriteProperty(writer, "Micrometeorite damage", "Minor / stable", ProseStringFormatter.Instance);
        } finally {
          writer.PopFrame();
        }
      } finally {
        writer.PopFrame();
      }
    }

    private static void WriteReactor(
      IProseWriter writer,
      string name,
      int output,
      int temperature,
      bool operational,
      string assignment
    ) {
      if (!writer.BeginFrame(ProseTree.Instance)) return;
      try {
        ProseApi.WriteName(writer, name);
        writer.Write(output, PowerOutput);
        writer.Write(temperature, Temperature);
        ProseApi.WriteProperty(writer, "State", operational, OperationalState);
        ProseApi.WriteProperty(
          writer,
          "Summary",
          "Long-range relay and research platform holding a stable polar orbit while its secondary coolant " +
          "loop is isolated for inspection.",
          ProseStringFormatter.Instance
        );
        ProseApi.WriteProperty(writer, "Assignment", assignment, ProseStringFormatter.Instance);
        ProseApi.WriteProperty(writer, "Containment", 99, Percent, noWrap: true);
      } finally {
        writer.PopFrame();
      }
    }

    private static void WriteReactor(
      IProseWriter writer,
      string name
    ) {
      if (!writer.BeginFrame(ProseTree.Instance)) return;
      try {
        ProseApi.WriteName(writer, name);
      } finally {
        writer.PopFrame();
      }
    }


    private static void WriteCommunications(IProseWriter writer) {
      if (!writer.BeginFrame(ProseTree.Instance)) return;
      try {
        ProseApi.WriteName(writer, "Communications");
        ProseApi.WriteProperty(writer, "Relay mode", "Store, route, and forward", ProseStringFormatter.Instance);
        ProseApi.WriteProperty(writer, "Packets queued", 1284, ProseIntFormatter.Instance);
        ProseApi.WriteProperty(writer, "Oldest packet", "00:00:04.218", ProseStringFormatter.Instance);
        ProseApi.WriteProperty(writer, "Encryption", "HELIX-Q lattice / epoch 84", ProseStringFormatter.Instance);

        WriteAntenna(writer, "High-gain antenna North", 97, "Luna Deep Space Array", true);
        WriteAntenna(writer, "High-gain antenna South", 82, "Research vessel Nereid", true);
        WriteAntenna(writer, "Emergency omnidirectional array", 64, "Standby beacon", false);
      } finally {
        writer.PopFrame();
      }
    }

    private static void WriteAntenna(
      IProseWriter writer,
      string name,
      int signal,
      string target,
      bool transmitting
    ) {
      if (!writer.BeginFrame(ProseTree.Instance)) return;
      try {
        ProseApi.WriteName(writer, name);
        writer.Write(signal, SignalStrength);
        ProseApi.WriteProperty(writer, "Target", target, ProseStringFormatter.Instance);
        ProseApi.WriteProperty(writer, "Transmitter", transmitting, EnabledState);
        ProseApi.WriteProperty(writer, "Error correction", "LDPC 7/8", ProseStringFormatter.Instance);
      } finally {
        writer.PopFrame();
      }
    }

    private static void WriteScienceAndCargo(IProseWriter writer) {
      if (!writer.BeginFrame(ProseTree.Instance)) return;
      try {
        ProseApi.WriteName(writer, "Science and cargo");
        ProseApi.WriteProperty(writer, "Active experiments", 14, ProseIntFormatter.Instance);
        ProseApi.WriteProperty(writer, "Cold storage", -42, Temperature.ValueFormatter);
        ProseApi.WriteProperty(writer, "Sample vault", "Sealed / biometric access", ProseStringFormatter.Instance);
        ProseApi.WriteProperty(writer, "Cargo capacity", 68, Percent);

        if (!writer.BeginFrame(ProseTree.Instance)) return;
        try {
          ProseApi.WriteName(writer, "Priority payloads");
          ProseApi.WriteProperty(writer, "PX-113", "Cryogenic regolith cores", ProseStringFormatter.Instance);
          ProseApi.WriteProperty(writer, "BX-204", "Replacement coolant manifold", ProseStringFormatter.Instance);
          ProseApi.WriteProperty(writer, "MED-09", "Emergency tissue printer feedstock", ProseStringFormatter.Instance);
          ProseApi.WriteProperty(writer, "ARCHIVE", "2.4 PB encrypted survey data", ProseStringFormatter.Instance);
        } finally {
          writer.PopFrame();
        }
      } finally {
        writer.PopFrame();
      }
    }

    private static void WriteAlerts(IProseWriter writer) {
      if (!writer.BeginFrame(ProseTree.Instance)) return;
      try {
        writer.PushModifier(new LevelMarker(ProseLevel.Warning));
        writer.PushModifier(AllowTruncate.Instance);
        ProseApi.WriteName(writer, "Active alerts");
        ProseApi.WriteProperty(
          writer,
          "Warning C-17",
          "Secondary coolant loop pressure oscillation exceeded the preferred envelope three times. " +
          "The loop is isolated; reactor B is carrying thermal recovery while engineering inspects valve C17-B.",
          ProseStringFormatter.Instance,
          level: ProseLevel.Warning
        );
        ProseApi.WriteProperty(
          writer,
          "Advisory N-04",
          "North radiator deployment motor is 6% above its modeled current draw.",
          ProseStringFormatter.Instance,
          level: ProseLevel.Info
        );
        ProseApi.WriteProperty(writer, "Acknowledged by", "Lt. Sato / 18:22 UTC", ProseStringFormatter.Instance);
      } finally {
        writer.PopFrame();
      }
    }
  }

  internal static class ProseDictionaryText {
    public static string Format(Dictionary<string, object> dictionary) {
      var builder = new StringBuilder(2048);
      AppendValue(builder, dictionary, 0);
      return builder.ToString();
    }

    private static void AppendValue(StringBuilder builder, object value, int depth) {
      switch (value) {
        case null:
          builder.Append("null");
          return;
        case string text:
          AppendQuoted(builder, text);
          return;
        case bool boolean:
          builder.Append(boolean ? "true" : "false");
          return;
        case Enum enumeration:
          AppendQuoted(builder, enumeration.ToString());
          return;
        case IDictionary<string, object> dictionary:
          AppendDictionary(builder, dictionary, depth);
          return;
        case IList list:
          AppendList(builder, list, depth);
          return;
        case IFormattable formattable:
          builder.Append(formattable.ToString(null, CultureInfo.InvariantCulture));
          return;
        default:
          AppendQuoted(builder, value.ToString());
          return;
      }
    }

    private static void AppendDictionary(StringBuilder builder, IDictionary<string, object> dictionary, int depth) {
      builder.Append('{');
      var index = 0;
      foreach (var pair in dictionary) {
        if (index++ > 0) builder.Append(',');
        builder.AppendLine();
        AppendIndent(builder, depth + 1);
        AppendQuoted(builder, pair.Key);
        builder.Append(": ");
        AppendValue(builder, pair.Value, depth + 1);
      }
      if (dictionary.Count > 0) {
        builder.AppendLine();
        AppendIndent(builder, depth);
      }
      builder.Append('}');
    }

    private static void AppendList(StringBuilder builder, IList list, int depth) {
      builder.Append('[');
      for (var i = 0; i < list.Count; i++) {
        if (i > 0) builder.Append(',');
        builder.AppendLine();
        AppendIndent(builder, depth + 1);
        AppendValue(builder, list[i], depth + 1);
      }
      if (list.Count > 0) {
        builder.AppendLine();
        AppendIndent(builder, depth);
      }
      builder.Append(']');
    }

    private static void AppendQuoted(StringBuilder builder, string value) {
      builder.Append('"');
      for (var i = 0; i < value.Length; i++) {
        switch (value[i]) {
          case '"': builder.Append("\\\""); break;
          case '\\': builder.Append("\\\\"); break;
          case '\n': builder.Append("\\n"); break;
          case '\r': builder.Append("\\r"); break;
          case '\t': builder.Append("\\t"); break;
          default: builder.Append(value[i]); break;
        }
      }
      builder.Append('"');
    }

    private static void AppendIndent(StringBuilder builder, int depth) => builder.Append(' ', depth * 2);
  }
}
