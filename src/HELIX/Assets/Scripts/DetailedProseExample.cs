using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using HELIX.Prose;
using UnityEngine;

namespace HELIX.Examples {
  /// <summary>A deliberately broad immediate-mode Prose sample used by the NW system example.</summary>
  public sealed class DetailedProseExample : IProse {
    private enum StationState : byte { Nominal, Degraded, Emergency }

    private static readonly DetailedProseExample Instance = new();

    public static IProse Prose => Instance;

    private static readonly ProsePropertyDatatype<string> MissionId =
      new("Mission ID", new ProseStringDatatype(prefix: "HX-"));

    private static readonly ProsePropertyDatatype<int> CrewAboard =
      new("Crew aboard", new ProseIntDatatype(min: 0, max: 64, suffix: " people"));

    private static readonly ProsePropertyDatatype<int> OrbitNumber =
      new("Orbit", new ProseIntDatatype(min: 1, suffix: " completed"));

    private static readonly ProsePropertyDatatype<int> Temperature =
      new("Temperature", new ProseIntDatatype(min: -80, max: 180, suffix: " °C"));

    private static readonly ProsePropertyDatatype<int> PowerOutput =
      new("Output", new ProseIntDatatype(min: 0, max: 100, suffix: "%"));

    private static readonly ProsePropertyDatatype<int> SignalStrength =
      new("Signal", new ProseIntDatatype(min: 0, max: 100, suffix: "%"));

    private static readonly ProseBoolDatatype OperationalState = new("operational", "offline");
    private static readonly ProseBoolDatatype EnabledState = new("enabled", "disabled");
    private static readonly ProseIntDatatype Percent = new(min: 0, max: 100, suffix: "%");
    private static readonly ProseFloatDatatype Megawatts = new("0.00");

    /// <summary>A deliberately ornate style used to exercise wide branch tokens and decorations.</summary>
    public static readonly ProseTextConfiguration TestWideDecorated = new(
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
    public static readonly ProseTextConfiguration TestCompactSections = new(
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

    public static string RenderPlainText() => RenderPlainText(ProseTextConfigurations.Sparse);

    public static string RenderPlainText(ProseTextConfiguration configuration) {
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

    public static void PrintPlainText() => PrintPlainText("Sparse tree", ProseTextConfigurations.Sparse);

    public static void PrintPlainText(string configurationName, ProseTextConfiguration configuration) => Debug.Log(
      "Detailed Prose · " + configurationName + "\n" + RenderPlainText(configuration)
    );

    public static void PrintDictionary() => Debug.Log("Detailed Prose · dictionary\n" + RenderDictionary());

    public static void PrintUnityRichText() => Debug.Log("Detailed Prose · Unity rich text\n" + RenderUnityRichText());

    public void ToProse(IProseWriter writer) {
      writer.Name("Asteria Orbital Relay Station");
      writer.Write("ASTERIA-07", MissionId);

      WriteMissionBriefing(writer);
      WriteOperatorNote(writer);
      // writer.Write(ProseSoftLineBreak.Instance);
      // writer.Write(ProseLineBreak.Instance);

      writer.Property(
        "State", StationState.Degraded, ProseDatatypes.Enum<StationState>(),
        description: "Degraded while the secondary coolant loop is isolated",
        defaultValue: StationState.Nominal
      );
      writer.Write(37, CrewAboard);
      writer.Write(1842, OrbitNumber);
      writer.Property("Autonomous control", true, OperationalState, defaultValue: true);
      writer.Property(
        "Status ", "Mission-capable", ProseDatatypes.String,
        hideSeparator: true
      );
      writer.Property(
        "Operator note", "Prioritize thermal stability over throughput.", ProseDatatypes.String,
        hideName: true
      );
      writer.Property(
        "Summary",
        "Long-range relay and research platform holding a stable polar orbit while its secondary coolant " +
        "loop is isolated for inspection.",
        ProseDatatypes.String
      );
      writer.Property(
        "Internal tracking token",
        "OPS-4A-9912",
        ProseDatatypes.String,
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
      using (writer.Section()) {
        writer.WriteSectionHeader("Mission briefing");

        using (writer.Paragraph()) {
          writer.Write("Station state is ");
          writer.WriteSpan("degraded", ProseTextStyle.Strong);
          writer.Write(" while the ");
          writer.WriteSpan("secondary coolant loop", ProseTextStyle.Emphasis);
          writer.Write(" remains isolated under tracking token ");
          writer.WriteSpan("OPS-4A-9912", ProseTextStyle.Code);
          writer.Write(". Follow the ");
          writer.WriteSpan(
            "thermal recovery runbook",
            linkTarget: "https://helix.local/runbooks/thermal-recovery"
          );
          writer.Write(" until inspection is complete.");
        }

        using (writer.Paragraph()) {
          writer.WriteSpan(
            "Keep reactor B below 70% output until valve C17-B passes its pressure cycle.",
            ProseTextStyle.Error
          );
        }

        writer.WriteCodeBlock(
          "coolant isolate C17 --tracking OPS-4A-9912\n" +
          "thermal recover --limit-reactor-b 70% --cycles 2",
          "helix"
        );

        using (writer.OrderedList()) {
          writer.WriteListItem("Verify coolant isolation telemetry.");
          writer.WriteListItem("Inspect valve C17-B and the secondary pump manifold.");
          writer.WriteListItem("Return the loop to service after two stable pressure cycles.");
        }

        WriteSubsystemTable(writer);
      }
    }

    private static void WriteSubsystemTable(IProseWriter writer) {
      using (writer.Table()) {
        using (writer.TableRow(header: true)) {
          writer.WriteTableCell("Subsystem");
          writer.WriteTableCell("State", ProseTextAlignment.Center);
          writer.WriteTableCell("Load", ProseTextAlignment.Right);
          writer.WriteTableCell("Owner");
        }

        WriteSubsystemRow(writer, "Primary reactor", "Nominal", 91, "Power");
        WriteSubsystemRow(writer, "Reactor B", "Recovery", 43, "Power");
        WriteSubsystemRow(writer, "Coolant loop C17", "Isolated", 0, "Engineering");
        WriteSubsystemRow(
          writer, "Relay array\nThis has a linebreak",
          "Operational but this is a very very long line, I don't know if it can actually handle this.", 97,
          "Communications"
        );
        WriteSubsystemRow(writer, "Relay array", "Operational", 97, "Communications");
      }
    }

    private static void WriteSubsystemRow(
      IProseWriter writer, string subsystem, string state, int load, string owner
    ) {
      using (writer.TableRow()) {
        writer.WriteTableCell(subsystem);
        writer.WriteTableCell(state, ProseTextAlignment.Center);
        writer.WriteTableCell(load, Percent, ProseTextAlignment.Right);
        writer.WriteTableCell(owner);
      }
    }

    private static void WriteOperatorNote(IProseWriter writer) {
      using (writer.Section())
      using (writer.Paragraph())
        writer.WriteSpan(
          "Operator note: the relay remains mission-capable; prioritize thermal stability over throughput.",
          ProseTextStyle.Quote
        );
    }

    private static void WriteCommandDeck(IProseWriter writer) {
      using (writer.Tree()) {
        writer.Name("Command deck");
        writer.Property("Watch officer", "Cmdr. Imani Vale", ProseDatatypes.String);
        writer.Property("Shift", "Gamma", ProseDatatypes.String);
        writer.Property("Navigation lock", true, EnabledState);
        writer.Property("Attitude error", 0.04f, ProseDatatypes.Float);
        writer.Property("Next maneuver", "2026-08-12 21:40 UTC", ProseDatatypes.String);

        using (writer.Tree()) {
          writer.Name("Crew manifest");
          writer.Property("Command", 4, ProseDatatypes.Int);
          writer.Property("Engineering", 12, ProseDatatypes.Int);
          writer.Property("Science", 9, ProseDatatypes.Int);
          writer.Property("Operations", 8, ProseDatatypes.Int);
          writer.Property("Medical", 4, ProseDatatypes.Int);
        }
      }
    }

    private static void WritePowerGrid(IProseWriter writer) {
      using (writer.Tree()) {
        writer.Name("Power grid");
        writer.Property("Grid state", "Load balanced", ProseDatatypes.String);
        writer.Property("Battery reserve", 78, Percent);
        writer.Property("Solar tracking", true, EnabledState);
        writer.Property("Peak demand (MW)", 18.72f, Megawatts);

        WriteReactor(writer, "Fusion reactor A", 91, 612, true, "Primary bus");
        WriteReactor(writer, "Fusion reactor IR");
        WriteReactor(writer, "Fusion reactor B", 43, 487, true, "Reserve and thermal recovery");

        using (writer.Tree()) {
          writer.Name("Solar array wings");
          writer.Property("Port wing", 96, Percent);
          writer.Property("Starboard wing", 94, Percent);
          writer.Property("Sun incidence", 88, Percent);
          writer.Property("Micrometeorite damage", "Minor / stable", ProseDatatypes.String);
        }
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
      using (writer.Tree()) {
        writer.Name(name);
        writer.Write(output, PowerOutput);
        writer.Write(temperature, Temperature);
        writer.Property("State", operational, OperationalState);
        writer.Property(
          "Summary",
          "Long-range relay and research platform holding a stable polar orbit while its secondary coolant " +
          "loop is isolated for inspection.",
          ProseDatatypes.String
        );
        writer.Property("Assignment", assignment, ProseDatatypes.String);
        writer.Property("Containment", 99, Percent, noWrap: true);
      }
    }

    private static void WriteReactor(
      IProseWriter writer,
      string name
    ) {
      using (writer.Tree()) {
        writer.Name(name);
      }
    }


    private static void WriteCommunications(IProseWriter writer) {
      using (writer.Tree()) {
        writer.Name("Communications");
        writer.Property("Relay mode", "Store, route, and forward", ProseDatatypes.String);
        writer.Property("Packets queued", 1284, ProseDatatypes.Int);
        writer.Property("Oldest packet", "00:00:04.218", ProseDatatypes.String);
        writer.Property("Encryption", "HELIX-Q lattice / epoch 84", ProseDatatypes.String);

        WriteAntenna(writer, "High-gain antenna North", 97, "Luna Deep Space Array", true);
        WriteAntenna(writer, "High-gain antenna South", 82, "Research vessel Nereid", true);
        WriteAntenna(writer, "Emergency omnidirectional array", 64, "Standby beacon", false);
      }
    }

    private static void WriteAntenna(
      IProseWriter writer,
      string name,
      int signal,
      string target,
      bool transmitting
    ) {
      using (writer.Tree()) {
        writer.Name(name);
        writer.Write(signal, SignalStrength);
        writer.Property("Target", target, ProseDatatypes.String);
        writer.Property("Transmitter", transmitting, EnabledState);
        writer.Property("Error correction", "LDPC 7/8", ProseDatatypes.String);
      }
    }

    private static void WriteScienceAndCargo(IProseWriter writer) {
      using (writer.Tree()) {
        writer.Name("Science and cargo");
        writer.Property("Active experiments", 14, ProseDatatypes.Int);
        writer.Property("Cold storage", -42, Temperature.ValueDatatype);
        writer.Property("Sample vault", "Sealed / biometric access", ProseDatatypes.String);
        writer.Property("Cargo capacity", 68, Percent);

        using (writer.Tree()) {
          writer.Name("Priority payloads");
          writer.Property("PX-113", "Cryogenic regolith cores", ProseDatatypes.String);
          writer.Property("BX-204", "Replacement coolant manifold", ProseDatatypes.String);
          writer.Property("MED-09", "Emergency tissue printer feedstock", ProseDatatypes.String);
          writer.Property("ARCHIVE", "2.4 PB encrypted survey data", ProseDatatypes.String);
        }
      }
    }

    private static void WriteAlerts(IProseWriter writer) {
      using (writer.Tree()) {
        writer.PushModifier(ProseModifiers.Level(ProseLevel.Warning));
        writer.PushModifier(ProseModifiers.AllowTruncate);
        writer.Name("Active alerts");
        writer.Property(
          "Warning C-17",
          "Secondary coolant loop pressure oscillation exceeded the preferred envelope three times. " +
          "The loop is isolated; reactor B is carrying thermal recovery while engineering inspects valve C17-B.",
          ProseDatatypes.String,
          level: ProseLevel.Warning
        );
        writer.Property(
          "Advisory N-04",
          "North radiator deployment motor is 6% above its modeled current draw.",
          ProseDatatypes.String,
          level: ProseLevel.Info
        );
        writer.Property("Acknowledged by", "Lt. Sato / 18:22 UTC", ProseDatatypes.String);
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
