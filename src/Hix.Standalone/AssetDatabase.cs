using System.Security.Cryptography;
using System.Text;
using DuckDB.NET.Data;
using Hix.Compiler;
using Hix.Hal;
using Hix.Runtime;
using Hix.Standalone.Analysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hix.Standalone;

public sealed record AssetEntry(string Id, string Path, string SourceHash, string Diagnostic, bool Deleted) { public string Uid { get; init; } }
public sealed record AssetObject(string AssetId, string Path, string SectionId, string Type);
public sealed record AssetDependency(string Path, string Dependency, string Fingerprint);
public sealed record AssetBuild(string AssetId, string Path, string Fingerprint, bool CacheHit, string PrimarySection, IReadOnlyDictionary<string, IHixValue> Values);
public sealed record WorkspaceAnalysis(string Workspace, string Fingerprint, Dictionary<string, HixPattern> Patterns, string[] Diagnostics, bool CacheHit);

/// Persistent, rebuildable workspace index. One writer owns a database at a time. Unsaved editor
/// buffers never replace the disk index. Build output and dependency fingerprints publish atomically.
public sealed partial class AssetDatabase : IDisposable {
  private const int SchemaVersion = 3;
  private static readonly string CompilerVersion = typeof(LanguageAnalysis).Assembly.ManifestModule.ModuleVersionId + ":" + typeof(AssetDatabase).Assembly.ManifestModule.ModuleVersionId;
  private static readonly Dictionary<string, object> Gates = new(HixWorkspace.PathComparer);
  private readonly object gate;
  private readonly DuckDBConnection connection;
  private bool disposed;
  public HixWorkspace Workspace { get; }
  public string DatabasePath => Path.Combine(Workspace.CacheDirectory, "assets.duckdb");
  public static AssetDatabase Open(string path) => new(HixWorkspace.Find(path));
  private AssetDatabase(HixWorkspace workspace) {
    Workspace = workspace;
    lock (Gates) { if (!Gates.TryGetValue(workspace.Root, out gate)) Gates[workspace.Root] = gate = new object(); }
    System.Threading.Monitor.Enter(gate);
    try {
      Directory.CreateDirectory(workspace.CacheDirectory);
      // Use the connection-string builder so workspace names containing semicolons remain paths.
      var builder = new System.Data.Common.DbConnectionStringBuilder { ["Data Source"] = DatabasePath };
      connection = new DuckDBConnection(builder.ConnectionString); connection.Open();
      var version = Convert.ToInt64(Scalar("SELECT count(*) FROM information_schema.tables WHERE table_name='hix_schema'")) == 0
        ? null : Scalar("SELECT version FROM hix_schema LIMIT 1");
      if (version == null || Convert.ToInt32(version) != SchemaVersion) {
        connection.Dispose();
        File.Delete(DatabasePath);
        if (File.Exists(DatabasePath + ".wal")) File.Delete(DatabasePath + ".wal");
        connection = new DuckDBConnection(builder.ConnectionString); connection.Open();
        CreateSchema();
      }
    } catch { connection?.Dispose(); System.Threading.Monitor.Exit(gate); throw; }
  }
  public void Dispose() { if (disposed) return; disposed = true; try { connection.Dispose(); } finally { System.Threading.Monitor.Exit(gate); } }
  private DuckDBCommand Command(string sql, params object[] values) {
    if (disposed) throw new ObjectDisposedException(nameof(AssetDatabase));
    var command = connection.CreateCommand(); command.CommandText = sql;
    foreach (var value in values) { var parameter = command.CreateParameter(); parameter.Value = value ?? DBNull.Value; command.Parameters.Add(parameter); }
    return command;
  }
  private void Execute(string sql, params object[] values) { using var command = Command(sql, values); command.ExecuteNonQuery(); }
  private object Scalar(string sql, params object[] values) { using var command = Command(sql, values); var value = command.ExecuteScalar(); return value is DBNull ? null : value; }
  private List<T> Read<T>(string sql, Func<System.Data.Common.DbDataReader, T> convert, params object[] values) {
    using var command = Command(sql, values); using var reader = command.ExecuteReader(); var result = new List<T>();
    while (reader.Read()) result.Add(convert(reader)); return result;
  }
  private void Transaction(Action action) {
    Execute("BEGIN TRANSACTION");
    try { action(); Execute("COMMIT"); } catch { Execute("ROLLBACK"); throw; }
  }
  internal static string Hash(string text) { using var sha = SHA256.Create(); return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant(); }
  private static string Encode(object value) => JToken.FromObject(value, HixImageFile.Serializer()).ToString(Formatting.None);
  private static T Decode<T>(string value) => JToken.Parse(value).ToObject<T>(HixImageFile.Serializer());
  private string AssetPath(string path) {
    path = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(Workspace.Directory, path));
    if (!HixWorkspace.Contains(Workspace.Root, path) || HixWorkspace.Contains(Workspace.CacheDirectory, path)) throw new ArgumentException("Asset is outside its associated workspace: " + path);
    if (!path.EndsWith(".hal", StringComparison.OrdinalIgnoreCase) || Path.GetFileName(path).Equals("manifest.hal", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Expected a HAL asset, not a manifest");
    for (var part = path; !HixWorkspace.SamePath(part, Workspace.Root); part = Path.GetDirectoryName(part))
      if ((File.Exists(part) || Directory.Exists(part)) && (File.GetAttributes(part) & FileAttributes.ReparsePoint) != 0) throw new ArgumentException("Asset paths cannot traverse symbolic links");
    return path;
  }
  /// Explicit filesystem reconciliation. Query is indexed and does not scan the project.
  public int Refresh() {
    var paths = new HashSet<string>(HixWorkspace.Files(Workspace.Root, "*.hal", directory =>
        !File.Exists(Path.Combine(directory, "manifest.hal")) || HixWorkspace.SamePath(HixWorkspace.Find(directory).Root, Workspace.Root))
      .Where(p => !Path.GetFileName(p).Equals("manifest.hal", StringComparison.OrdinalIgnoreCase)), HixWorkspace.PathComparer);
    foreach (var path in paths) Index(path);
    foreach (var obsolete in Read("SELECT f.path FROM assets a JOIN files f ON f.id=a.file_id WHERE NOT a.deleted", r => r.GetString(0)).Where(p => !paths.Contains(p))) MarkDeleted(obsolete);
    return paths.Count;
  }
  public AssetEntry Index(string path) {
    path = AssetPath(path);
    if (!File.Exists(path)) { MarkDeleted(path); throw new FileNotFoundException("Asset does not exist", path); }
    var text = File.ReadAllText(path); var hash = Hash(text); var existing = Find(path);
    if (existing != null && !existing.Deleted && existing.SourceHash == hash) return existing;
    HalDocumentSyntax syntax = null; string diagnostic = "";
    try {
      syntax = HalDocumentSyntax.Parse(text);
      diagnostic = string.Join("; ", syntax.Diagnostics.Select(d => $"{d.Line}:{d.Column}: {d.Message}"));
      if (syntax.Sections.Count == 0) diagnostic = "Expected at least one HAL section";
      if (syntax.Sections.Any(s => s.Id == null) || syntax.Sections.GroupBy(s => s.Id).Any(g => g.Count() > 1)) diagnostic = "Missing or duplicate section id";
    } catch (Exception e) when (e is ArgumentException or NullReferenceException) { diagnostic = e.Message; }
    var id = diagnostic.Length == 0 ? syntax.Uid ?? (existing?.Uid == null ? existing?.Id : null) ?? Guid.NewGuid().ToString("N") : existing?.Id ?? Guid.NewGuid().ToString("N");
    var owner = FindId(id);
    if (owner != null && !HixWorkspace.SamePath(owner.Path, path) && File.Exists(owner.Path))
      throw new ArgumentException("Duplicate asset uid '" + id + "': " + owner.Path + " and " + path);
    var entry = new AssetEntry(id, path, hash, diagnostic, false) { Uid = diagnostic.Length == 0 ? syntax.Uid : existing?.Uid };
    Transaction(() => {
      Execute("INSERT INTO asset_identities VALUES (?) ON CONFLICT DO NOTHING", id);
      if (existing != null && existing.Id != id) {
        Execute("DELETE FROM objects WHERE asset_id=?", existing.Id);
        Execute("DELETE FROM builds WHERE asset_id=?", existing.Id);
        Execute("DELETE FROM asset_tags WHERE asset_id=?", existing.Id);
        Execute("DELETE FROM assets WHERE id=?", existing.Id);
      }
      if (owner != null && !HixWorkspace.SamePath(owner.Path, path))
        Execute("UPDATE assets SET file_id=? WHERE id=?", FileId(path), id);
      Execute("INSERT INTO assets VALUES (?,?,?,?,false,?) ON CONFLICT(file_id) DO UPDATE SET source_hash=excluded.source_hash, diagnostic=excluded.diagnostic, deleted=false, uid=excluded.uid", entry.Id, FileId(path), hash, diagnostic, entry.Uid);
      // Preserve last known objects when source is temporarily invalid, but expose the diagnostic.
      if (diagnostic.Length == 0) {
        Execute("DELETE FROM objects WHERE asset_id=?", entry.Id);
        foreach (var section in syntax.Sections) Execute("INSERT INTO objects VALUES (?,?,?)", entry.Id, section.Id, section.Type);
        Execute("DELETE FROM asset_tags WHERE asset_id=?", entry.Id);
        foreach (var tag in syntax.Tags.Distinct(StringComparer.Ordinal)) {
          Execute("INSERT INTO tags(name) VALUES (?) ON CONFLICT DO NOTHING", tag);
          Execute("INSERT INTO asset_tags SELECT ?,id FROM tags WHERE name=?", entry.Id, tag);
        }
      }
    });
    return entry;
  }
  private void MarkDeleted(string path) => Execute("UPDATE assets SET deleted=true WHERE file_id=(SELECT id FROM files WHERE path=?)", path);
  private const string AssetSelect = "SELECT a.id,f.path,a.source_hash,a.diagnostic,a.deleted,a.uid FROM assets a JOIN files f ON f.id=a.file_id ";
  private static AssetEntry Entry(System.Data.Common.DbDataReader r) => new(r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetBoolean(4)) { Uid = r.IsDBNull(5) ? null : r.GetString(5) };
  public AssetEntry Find(string path) => Read(AssetSelect + "WHERE f.path=? OR a.id=?", Entry, path, path).FirstOrDefault();
  private AssetEntry FindId(string id) => Read(AssetSelect + "WHERE a.id=?", Entry, id).FirstOrDefault();
  public IReadOnlyList<AssetObject> Query(string type = null, int offset = 0, int limit = 100, string tag = null) {
    Page(offset, limit);
    return Read("SELECT a.id,f.path,o.section_id,o.type FROM objects o JOIN assets a ON a.id=o.asset_id JOIN files f ON f.id=a.file_id WHERE NOT a.deleted AND (? IS NULL OR o.type=?) AND (? IS NULL OR EXISTS (SELECT 1 FROM asset_tags membership JOIN tags t ON t.id=membership.tag_id WHERE membership.asset_id=a.id AND t.name=?)) ORDER BY f.path,o.section_id LIMIT ? OFFSET ?",
      r => new AssetObject(r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3)), type, type, tag, tag, limit, offset);
  }
  public IReadOnlyList<string> Tags(string assetId) => Read("SELECT t.name FROM tags t JOIN asset_tags a ON a.tag_id=t.id WHERE a.asset_id=? ORDER BY t.name", r => r.GetString(0), assetId);
  public IReadOnlyList<AssetDependency> Dependencies(string path, bool reverse = false) => Read("SELECT f.path,t.path,d.fingerprint FROM dependencies d JOIN files f ON f.id=d.file_id JOIN files t ON t.id=d.dependency_file_id WHERE " + (reverse ? "t.path" : "f.path") + "=? ORDER BY f.path,t.path",
    r => new AssetDependency(r.GetString(0),r.GetString(1),r.GetString(2)), AssetPath(path));

  public AssetBuild Build(string path, string processorVersion = "hal-values-v1") => Build(AssetPath(path), processorVersion, new HashSet<string>(HixWorkspace.PathComparer), new Dictionary<string,AssetBuild>(HixWorkspace.PathComparer), new BuildContext());
  private AssetBuild Build(string path, string processorVersion, HashSet<string> active, Dictionary<string,AssetBuild> completed, BuildContext context) {
    if (completed.TryGetValue(path, out var done)) return done;
    if (!active.Add(path)) throw new ArgumentException("Cyclic asset dependency: " + string.Join(" -> ", active.Append(path)));
    AssetEntry entry = null;
    try {
      entry = Index(path); if (entry.Diagnostic.Length != 0) throw new ArgumentException(entry.Diagnostic);
      var workspace = HixWorkspace.Find(path);
      if (!context.Analyses.TryGetValue(workspace.Directory, out var analysis)) context.Analyses[workspace.Directory] = analysis = Analyze(path);
      if (analysis.Diagnostics.Length != 0) throw new ArgumentException(string.Join("; ", analysis.Diagnostics));
      var text = File.ReadAllText(path);
      if (Hash(text) != entry.SourceHash) throw new IOException("Asset changed while being read; retry the build");
      var syntax = HalDocumentSyntax.Parse(text);
      var dependencies = References(syntax).Select(p => ResolveReference(path, p, context)).Distinct(HixWorkspace.PathComparer)
        .OrderBy(p => p, StringComparer.Ordinal).ToDictionary(p => p, p => Build(p, processorVersion, active, completed, context), HixWorkspace.PathComparer);
      var fingerprint = Hash(CompilerVersion + processorVersion + entry.SourceHash + analysis.Fingerprint + Encode(dependencies.Select(p => new { Path=p.Key, p.Value.Fingerprint }).ToArray()));
      var cached = Read("SELECT fingerprint,value_json FROM builds WHERE asset_id=?", r => (Key:r.GetString(0), Json:r.GetString(1)), entry.Id).FirstOrDefault();
      if (cached.Key == fingerprint) {
        Execute("UPDATE builds SET error='' WHERE asset_id=?", entry.Id);
        return completed[path] = new(entry.Id,path,fingerprint,true,syntax.Sections[0].Id,Decode<Dictionary<string,IHixValue>>(cached.Json));
      }
      using var thread = new HixCoroutine();
      var values = HalFunctions.Evaluate(syntax, analysis.Patterns, thread, reference => {
        var target = dependencies[ResolveReference(path, reference, context)];
        var id = reference.Section ?? target.PrimarySection;
        return target.Values.TryGetValue(id, out var value) ? value : throw new ArgumentException("Unknown section '" + id + "' in " + reference.Value);
      });
      var encoded = Encode(values);
      if (File.ReadAllText(path) != text) throw new IOException("Asset changed during build; retry the build");
      Transaction(() => {
        Execute("INSERT INTO builds VALUES (?,?,?,'') ON CONFLICT(asset_id) DO UPDATE SET fingerprint=excluded.fingerprint,value_json=excluded.value_json,error=''", entry.Id,fingerprint,encoded);
        Execute("DELETE FROM dependencies WHERE file_id=?", FileId(path));
        foreach (var dependency in dependencies) Execute("INSERT INTO dependencies VALUES (?,?,?)", FileId(path),FileId(dependency.Key),dependency.Value.Fingerprint);
      });
      return completed[path] = new(entry.Id,path,fingerprint,false,syntax.Sections[0].Id,values);
    } catch (Exception e) { if (entry != null) Execute("UPDATE builds SET error=? WHERE asset_id=?", e.Message, entry.Id); throw; }
    finally { active.Remove(path); }
  }
  private sealed class BuildContext {
    public bool UidsRefreshed;
    public readonly Dictionary<string,WorkspaceAnalysis> Analyses = new(HixWorkspace.PathComparer);
  }
  private string ResolveReference(string owner, HalAssetReference reference, BuildContext context) {
    if (reference.Kind == "path") return AssetPath(Path.Combine(Path.GetDirectoryName(owner), reference.Value));
    if (reference.Kind != "uid") throw new ArgumentException("Expected a path or uid asset selector");
    if (!context.UidsRefreshed) { Refresh(); context.UidsRefreshed = true; }
    var entry = FindId(reference.Value);
    if (entry == null || entry.Deleted || entry.Uid != reference.Value) throw new ArgumentException("Unknown asset " + reference);
    return AssetPath(entry.Path);
  }
  /// Resolve an external reference using the same path/uid convention as the build pipeline.
  public AssetBuild Resolve(string owner, HalAssetReference reference) => Build(ResolveReference(AssetPath(owner), reference, new BuildContext()));
  internal static IEnumerable<HalAssetReference> References(HalDocumentSyntax document) => document.Sections.SelectMany(s => References(s.Value));
  private static IEnumerable<HalAssetReference> References(HalValueSyntax value) {
    switch (value) {
      case HalCallSyntax {Name:"asset"} call:
        yield return HalAssetReference.Parse(call); break;
      case HalTableSyntax table:
        foreach (var path in table.Fields.SelectMany(f => References(f.Value))) yield return path; break;
      case HalListSyntax list:
        foreach (var path in list.Values.SelectMany(References)) yield return path; break;
      case HalTypedSyntax typed:
        foreach (var path in References(typed.Value)) yield return path; break;
      case HalSelectionSyntax selection:
        foreach (var path in References(selection.Receiver)) yield return path; break;
    }
  }
}
