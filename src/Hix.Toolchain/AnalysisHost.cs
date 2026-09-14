using System.Net;
using System.Text;
using Hix.Standalone.Analysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

internal static class AnalysisHost {
  private static readonly JsonSerializerSettings Json = new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
  public static int Run(string[] args) {
    var server = new HixAnalysisServer();
    if (args[0] == "analysis-server") {
      string line;
      while ((line = Console.ReadLine()) != null) {
        try { Console.WriteLine(Reply(server, JObject.Parse(line))); }
        catch (Exception e) { Console.WriteLine(JsonConvert.SerializeObject(new { error = new { message = e.Message } }, Json)); }
      }
      return 0;
    }
    string root = Directory.GetCurrentDirectory(), web = Path.Combine(AppContext.BaseDirectory, "wwwroot"); var port = 5178;
    for (var i = 1; i < args.Length; i++) {
      if (i + 1 >= args.Length) throw new ArgumentException("Missing option value");
      switch (args[i]) {
        case "--root": root = Path.GetFullPath(args[++i]); break;
        case "--web": web = Path.GetFullPath(args[++i]); break;
        case "--port": port = int.Parse(args[++i]); break;
        default: throw new ArgumentException("Unknown option: " + args[i]);
      }
    }
    using var listener = new HttpListener(); var origin = $"http://localhost:{port}";
    listener.Prefixes.Add(origin + "/"); listener.Start();
    Console.WriteLine($"HAL graph editor: {origin} (files: {root})");
    while (listener.IsListening) {
      var context = listener.GetContext();
      try {
        var request = context.Request; var response = context.Response;
        // Local editor: same-origin JSON requests only, no cross-origin filesystem access.
        if (request.Headers["Origin"] is { } caller && caller != origin) throw new UnauthorizedAccessException("Origin rejected");
        if (request.Url.AbsolutePath == "/api" && request.HttpMethod == "POST") {
          if (!(request.ContentType ?? "").StartsWith("application/json", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Expected application/json");
          if (request.ContentLength64 > 4 * 1024 * 1024) throw new ArgumentException("Request too large");
          using var reader = new StreamReader(request.InputStream);
          var message = JObject.Parse(reader.ReadToEnd()); var method = (string)message["method"]; var parameters = message["params"] as JObject ?? new();
          if (method == "files/list") {
            var path = SafePath(root, (string)parameters["path"] ?? root);
            var entries = Directory.EnumerateFileSystemEntries(path).Where(p => (File.GetAttributes(p) & FileAttributes.ReparsePoint) == 0)
              .Where(p => Path.GetFileName(p) != ".hix" && (Directory.Exists(p) || p.EndsWith(".hal", StringComparison.OrdinalIgnoreCase)))
              .OrderBy(p => !Directory.Exists(p)).ThenBy(p => p).Select(p => new { path = p, name = Path.GetFileName(p), directory = Directory.Exists(p) });
            Send(response, JsonConvert.SerializeObject(new { result = new { root, path, entries } }, Json), "application/json");
          } else {
            if (method == "hal/open") parameters["path"] = SafePath(root, (string)parameters["path"]);
            else if (method?.StartsWith("hal/") == true) SafePath(root, server.Hal.PathOf((string)parameters["session"]));
            else if (method?.StartsWith("assets/") == true) parameters["path"] = SafePath(root, (string)parameters["path"] ?? root);
            if (method is not ("hal/open" or "hal/get" or "hal/candidates" or "hal/change" or "hal/edit" or "hal/save" or "hal/undo" or "hal/redo" or "hal/close" or "assets/index" or "assets/query" or "assets/build" or "assets/dependencies" or "assets/analyze" or "assets/document" or "assets/symbols" or "assets/patterns" or "assets/tags")) throw new ArgumentException("Unknown HTTP method");
            message["params"] = parameters; Send(response, Reply(server, message), "application/json");
          }
        } else if (request.HttpMethod == "GET") {
          var path = SafePath(web, request.Url.AbsolutePath == "/" ? "index.html" : Uri.UnescapeDataString(request.Url.AbsolutePath).TrimStart('/'));
          if (!File.Exists(path)) { response.StatusCode = 404; response.Close(); continue; }
          response.ContentType = Path.GetExtension(path) switch { ".js" => "text/javascript", ".css" => "text/css", ".html" => "text/html", ".svg" => "image/svg+xml", _ => "application/octet-stream" };
          var bytes = File.ReadAllBytes(path); response.OutputStream.Write(bytes); response.Close();
        } else { response.StatusCode = 405; response.Close(); }
      } catch (Exception e) {
        context.Response.StatusCode = e is UnauthorizedAccessException ? 403 : 400;
        Send(context.Response, JsonConvert.SerializeObject(new { error = new { message = e.Message } }, Json), "application/json");
      }
    }
    return 0;
  }
  private static string SafePath(string root, string path) {
    root = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar);
    var full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
    var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    if (full != root && !full.StartsWith(root + Path.DirectorySeparatorChar, comparison)) throw new UnauthorizedAccessException("Path is outside the server root");
    for (var part = full; part != root; part = Path.GetDirectoryName(part))
      if ((File.Exists(part) || Directory.Exists(part)) && (File.GetAttributes(part) & FileAttributes.ReparsePoint) != 0) throw new UnauthorizedAccessException("Symbolic links are not browsable");
    return full;
  }
  private static string Reply(HixAnalysisServer server, JObject request) {
    try { return JsonConvert.SerializeObject(new { id = request["id"], result = server.Dispatch((string)request["method"], request["params"] as JObject ?? new()) }, Json); }
    catch (Exception e) { return JsonConvert.SerializeObject(new { id = request["id"], error = new { code = e is InvalidOperationException ? "conflict" : "invalidRequest", message = e.Message } }, Json); }
  }
  private static void Send(HttpListenerResponse response, string body, string type) {
    response.ContentType = type; var bytes = Encoding.UTF8.GetBytes(body); response.OutputStream.Write(bytes); response.Close();
  }
}
