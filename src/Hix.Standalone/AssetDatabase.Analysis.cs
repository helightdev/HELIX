using Hix.Compiler;
using Hix.Standalone.Analysis;

namespace Hix.Standalone;

public sealed record IndexedPattern(string Name, string Kind, string Display, HixPattern Pattern);

public sealed partial class AssetDatabase {
  private void CreateSchema() {
    Transaction(() => {
      foreach (var sequence in new[] {"workspace", "file", "document", "tag"}) Execute("CREATE SEQUENCE " + sequence + "_ids");
      Execute("CREATE TABLE hix_schema(version INTEGER NOT NULL)");
      Execute("INSERT INTO hix_schema VALUES (?)", SchemaVersion);
      Execute("CREATE TABLE workspaces(id BIGINT PRIMARY KEY DEFAULT nextval('workspace_ids'), path VARCHAR NOT NULL UNIQUE, fingerprint VARCHAR)");
      Execute("CREATE TABLE files(id BIGINT PRIMARY KEY DEFAULT nextval('file_ids'), path VARCHAR NOT NULL UNIQUE)");
      Execute("CREATE TABLE asset_identities(id VARCHAR PRIMARY KEY)");
      Execute("CREATE TABLE assets(id VARCHAR PRIMARY KEY REFERENCES asset_identities(id), file_id BIGINT NOT NULL UNIQUE REFERENCES files(id), source_hash VARCHAR NOT NULL, diagnostic VARCHAR NOT NULL, deleted BOOLEAN NOT NULL, uid VARCHAR UNIQUE)");
      Execute("CREATE TABLE objects(asset_id VARCHAR NOT NULL REFERENCES asset_identities(id), section_id VARCHAR NOT NULL, type VARCHAR NOT NULL, PRIMARY KEY(asset_id,section_id))");
      Execute("CREATE INDEX objects_type ON objects(type)");
      Execute("CREATE TABLE tags(id BIGINT PRIMARY KEY DEFAULT nextval('tag_ids'), name VARCHAR NOT NULL UNIQUE)");
      Execute("CREATE TABLE asset_tags(asset_id VARCHAR NOT NULL REFERENCES asset_identities(id), tag_id BIGINT NOT NULL REFERENCES tags(id), PRIMARY KEY(asset_id,tag_id))");
      Execute("CREATE INDEX asset_tags_tag ON asset_tags(tag_id)");
      Execute("CREATE TABLE builds(asset_id VARCHAR PRIMARY KEY REFERENCES asset_identities(id), fingerprint VARCHAR NOT NULL, value_json VARCHAR NOT NULL, error VARCHAR NOT NULL)");
      Execute("CREATE TABLE dependencies(file_id BIGINT REFERENCES files(id), dependency_file_id BIGINT REFERENCES files(id), fingerprint VARCHAR NOT NULL, PRIMARY KEY(file_id,dependency_file_id))");
      Execute("CREATE INDEX dependency_reverse ON dependencies(dependency_file_id)");
      Execute("CREATE TABLE patterns(workspace_id BIGINT REFERENCES workspaces(id), name VARCHAR NOT NULL, kind VARCHAR NOT NULL, display VARCHAR NOT NULL, definition JSON NOT NULL, PRIMARY KEY(workspace_id,name))");
      Execute("CREATE INDEX pattern_name ON patterns(name)");
      Execute("CREATE INDEX pattern_kind ON patterns(kind)");
      Execute("CREATE TABLE documents(id BIGINT PRIMARY KEY DEFAULT nextval('document_ids'), workspace_id BIGINT NOT NULL REFERENCES workspaces(id), file_id BIGINT NOT NULL REFERENCES files(id), revision BIGINT NOT NULL, source_hash BIGINT NOT NULL, active BOOLEAN NOT NULL, UNIQUE(workspace_id,file_id))");
      Execute("CREATE TABLE symbols(document_id BIGINT NOT NULL REFERENCES documents(id), ordinal INTEGER NOT NULL, name VARCHAR NOT NULL, kind VARCHAR NOT NULL, start_offset INTEGER, end_offset INTEGER, line INTEGER, column_offset INTEGER, scope_start INTEGER, scope_end INTEGER, scope_line INTEGER, scope_column INTEGER, PRIMARY KEY(document_id,ordinal))");
      Execute("CREATE INDEX symbol_name ON symbols(name)");
      Execute("CREATE TABLE diagnostics(document_id BIGINT NOT NULL REFERENCES documents(id), ordinal INTEGER NOT NULL, message VARCHAR NOT NULL, severity VARCHAR NOT NULL, start_offset INTEGER, end_offset INTEGER, line INTEGER, column_offset INTEGER, PRIMARY KEY(document_id,ordinal))");
      Execute("CREATE TABLE symbol_references(document_id BIGINT NOT NULL REFERENCES documents(id), ordinal INTEGER NOT NULL, name VARCHAR NOT NULL, kind VARCHAR NOT NULL, start_offset INTEGER, end_offset INTEGER, line INTEGER, column_offset INTEGER, scope_start INTEGER, scope_end INTEGER, scope_line INTEGER, scope_column INTEGER, target_file_id BIGINT REFERENCES files(id), target_start INTEGER, target_end INTEGER, target_line INTEGER, target_column INTEGER, PRIMARY KEY(document_id,ordinal))");
      Execute("CREATE INDEX reference_target ON symbol_references(target_file_id)");
      Execute("CREATE TABLE type_facts(document_id BIGINT NOT NULL REFERENCES documents(id), ordinal INTEGER NOT NULL, type VARCHAR NOT NULL, kind VARCHAR NOT NULL, documentation VARCHAR NOT NULL, inlay BOOLEAN NOT NULL, start_offset INTEGER, end_offset INTEGER, line INTEGER, column_offset INTEGER, call_signature JSON, member_names JSON NOT NULL, member_types JSON NOT NULL, PRIMARY KEY(document_id,ordinal))");
      Execute("CREATE INDEX type_fact_type ON type_facts(type)");
    });
  }
  private long FileId(string path) {
    Execute("INSERT INTO files(path) VALUES (?) ON CONFLICT DO NOTHING", path);
    return Convert.ToInt64(Scalar("SELECT id FROM files WHERE path=?", path));
  }
  private long WorkspaceId(string path) {
    Execute("INSERT INTO workspaces(path) VALUES (?) ON CONFLICT DO NOTHING", path);
    return Convert.ToInt64(Scalar("SELECT id FROM workspaces WHERE path=?", path));
  }
  private static void Page(int offset, int limit) { if (offset < 0 || limit < 1 || limit > 1000) throw new ArgumentOutOfRangeException(nameof(limit)); }
  /// Query the last indexed pattern set without loading or parsing the rest of the workspace.
  public IReadOnlyList<IndexedPattern> Patterns(string name = null, string kind = null, int offset = 0, int limit = 100) {
    Page(offset,limit);
    return Read("SELECT p.name,p.kind,p.display,p.definition FROM patterns p JOIN workspaces w ON w.id=p.workspace_id WHERE w.path=? AND (? IS NULL OR p.name=?) AND (? IS NULL OR p.kind=?) ORDER BY p.name LIMIT ? OFFSET ?",
      r => new IndexedPattern(r.GetString(0),r.GetString(1),r.GetString(2),Decode<HixPattern>(r.GetString(3))), Workspace.Directory,name,name,kind,kind,limit,offset);
  }
  public WorkspaceAnalysis Analyze(string path = null) {
    var workspace = path == null ? Workspace : HixWorkspace.Find(path);
    if (!HixWorkspace.SamePath(workspace.Root, Workspace.Root)) throw new ArgumentException("Analysis belongs to a different workspace");
    var sources = workspace.Sources();
    var fingerprint = Hash(CompilerVersion + Encode(sources.Select(s => new { s.Path, Hash = Hash(s.Source) }).ToArray()));
    var workspaceId = WorkspaceId(workspace.Directory);
    if ((string)Scalar("SELECT fingerprint FROM workspaces WHERE id=?",workspaceId) == fingerprint) {
      var cached = Read("SELECT name,definition FROM patterns WHERE workspace_id=? ORDER BY name", r => (Name:r.GetString(0),Pattern:Decode<HixPattern>(r.GetString(1))),workspaceId).ToDictionary(p=>p.Name,p=>p.Pattern);
      var errors = Read("SELECT f.path,e.message FROM diagnostics e JOIN documents d ON d.id=e.document_id JOIN files f ON f.id=d.file_id WHERE d.workspace_id=? AND d.active ORDER BY f.path,e.ordinal",r=>r.GetString(0)+": "+r.GetString(1),workspaceId);
      return new(workspace.Directory,fingerprint,cached,errors.ToArray(),true);
    }
    var backend = new HixStandaloneBackend(TextWriter.Null, workspace.Directory); backend.SetCompilationSources(sources.Select(s => s.Source));
    var patterns = backend.Functions.Types.ToDictionary(t => t.Name, t => t.Pattern);
    var parsed = new Dictionary<string,LanguageAnalysis>(HixWorkspace.PathComparer);
    foreach (var source in sources) {
      var analysis = new LanguageAnalysis(source.Source, backend, recoverValidDeclarations: true); parsed[source.Path] = analysis;
      foreach (var type in analysis.TypeDeclarations) patterns[type.Name] = type.Pattern;
    }
    patterns = PatternComposition.ResolveDefinitions(patterns).ToDictionary(p => p.Key, p => p.Value);
    var snapshots = new HixAnalyzerService(backend).Synchronize(sources.Select(s => new HixDocument(s.Path, s.Source, 0)), parsed);
    Transaction(() => {
      Execute("UPDATE workspaces SET fingerprint=? WHERE id=?",fingerprint,workspaceId);
      Execute("DELETE FROM patterns WHERE workspace_id=?",workspaceId);
      foreach (var pattern in patterns) Execute("INSERT INTO patterns VALUES (?,?,?,?,?)",workspaceId,pattern.Key,pattern.Value.GetType().Name,pattern.Value.Display,Encode(pattern.Value));
      foreach (var table in new[]{"symbols","diagnostics","symbol_references","type_facts"})
        Execute("DELETE FROM " + table + " WHERE document_id IN (SELECT id FROM documents WHERE workspace_id=?)",workspaceId);
      Execute("UPDATE documents SET active=false WHERE workspace_id=?",workspaceId);
      foreach (var snapshot in snapshots) StoreDocument(workspaceId,snapshot);
    });
    return new(workspace.Directory,fingerprint,patterns,snapshots.SelectMany(s=>s.Diagnostics.Select(d=>s.Path+": "+d.Message)).ToArray(),false);
  }
  private void StoreDocument(long workspaceId,HixDocumentSnapshot snapshot) {
    var fileId=FileId(snapshot.Path);
    Execute("INSERT INTO documents(workspace_id,file_id,revision,source_hash,active) VALUES (?,?,?,?,true) ON CONFLICT(workspace_id,file_id) DO UPDATE SET revision=excluded.revision,source_hash=excluded.source_hash,active=true",workspaceId,fileId,snapshot.Revision,snapshot.SourceHash);
    var id=Convert.ToInt64(Scalar("SELECT id FROM documents WHERE workspace_id=? AND file_id=?",workspaceId,fileId));
    var ordinal=0;
    foreach(var s in snapshot.Declarations) Execute("INSERT INTO symbols VALUES (?,?,?,?,?,?,?,?,?,?,?,?)",id,ordinal++,s.Name,s.Kind,s.Range.Start,s.Range.End,s.Range.Line,s.Range.Column,s.Scope.Start,s.Scope.End,s.Scope.Line,s.Scope.Column);
    ordinal=0;
    foreach(var d in snapshot.Diagnostics) Execute("INSERT INTO diagnostics VALUES (?,?,?,?,?,?,?,?)",id,ordinal++,d.Message,d.Severity,d.Range.Start,d.Range.End,d.Range.Line,d.Range.Column);
    ordinal=0;
    foreach(var r in snapshot.References) Execute("INSERT INTO symbol_references VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)",id,ordinal++,r.Name,r.Kind,r.Range.Start,r.Range.End,r.Range.Line,r.Range.Column,r.Scope.Start,r.Scope.End,r.Scope.Line,r.Scope.Column,r.Target==null?null:(object)FileId(r.Target.Path),r.Target?.Range.Start,r.Target?.Range.End,r.Target?.Range.Line,r.Target?.Range.Column);
    ordinal=0;
    foreach(var f in snapshot.TypeFacts) Execute("INSERT INTO type_facts VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?)",id,ordinal++,f.Type,f.Kind,f.Documentation,f.Inlay,f.Range.Start,f.Range.End,f.Range.Line,f.Range.Column,f.CallSignature==null?null:Encode(f.CallSignature),Encode(f.MemberNames),Encode(f.MemberTypes));
  }
  private static HixSourceRange Range(System.Data.Common.DbDataReader r,int offset) => new(r.GetInt32(offset),r.GetInt32(offset+1),r.GetInt32(offset+2),r.GetInt32(offset+3));
  public HixDocumentSnapshot Document(string path) {
    path=Path.GetFullPath(path); var analysis=Analyze(path);
    var row=Read("SELECT d.id,d.revision,d.source_hash FROM documents d JOIN workspaces w ON w.id=d.workspace_id JOIN files f ON f.id=d.file_id WHERE w.path=? AND f.path=? AND d.active",r=>(Id:r.GetInt64(0),Revision:r.GetInt64(1),Hash:r.GetInt64(2)),analysis.Workspace,path).FirstOrDefault();
    if(row.Id==0) return null;
    return new(path,row.Revision,row.Hash,
      Read("SELECT name,kind,start_offset,end_offset,line,column_offset,scope_start,scope_end,scope_line,scope_column FROM symbols WHERE document_id=? ORDER BY ordinal",r=>new HixSymbol(r.GetString(0),r.GetString(1),Range(r,2),Range(r,6)),row.Id),
      Read("SELECT r.name,r.kind,r.start_offset,r.end_offset,r.line,r.column_offset,r.scope_start,r.scope_end,r.scope_line,r.scope_column,f.path,r.target_start,r.target_end,r.target_line,r.target_column FROM symbol_references r LEFT JOIN files f ON f.id=r.target_file_id WHERE r.document_id=? ORDER BY r.ordinal",r=>new HixReference(r.GetString(0),r.GetString(1),Range(r,2),Range(r,6),r.IsDBNull(10)?null:new HixLocation(r.GetString(10),Range(r,11))),row.Id),
      Read("SELECT message,severity,start_offset,end_offset,line,column_offset FROM diagnostics WHERE document_id=? ORDER BY ordinal",r=>new HixDiagnostic(r.GetString(0),r.GetString(1),Range(r,2)),row.Id),
      Read("SELECT type,kind,documentation,inlay,start_offset,end_offset,line,column_offset,call_signature,member_names,member_types FROM type_facts WHERE document_id=? ORDER BY ordinal",r=>new HixTypeFact(Range(r,4),r.GetString(0),r.GetString(2),r.GetBoolean(3),r.GetString(1)){CallSignature=r.IsDBNull(8)?null:Decode<HixDefinition>(r.GetString(8)),MemberNames=Decode<string[]>(r.GetString(9)),MemberTypes=Decode<string[]>(r.GetString(10))},row.Id));
  }
  public IReadOnlyList<HixLocation> Symbols(string name,int limit=100) {
    Page(0,limit);
    return Read("SELECT DISTINCT f.path,s.start_offset,s.end_offset,s.line,s.column_offset FROM symbols s JOIN documents d ON d.id=s.document_id JOIN files f ON f.id=d.file_id WHERE s.name=? AND d.active ORDER BY f.path,s.start_offset LIMIT ?",r=>new HixLocation(r.GetString(0),Range(r,1)),name,limit);
  }
}
