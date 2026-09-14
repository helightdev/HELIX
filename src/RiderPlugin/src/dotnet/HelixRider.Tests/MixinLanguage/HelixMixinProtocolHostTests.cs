using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HelixRider.Protocol;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
namespace HelixRider.Tests.MixinLanguage;

[TestFixture]
public sealed class HelixMixinProtocolHostTests {
    [Test]
    public void CSharpHostDeclarationsAreAnalyzedByExternalToolchain() {
        var executable = Environment.GetEnvironmentVariable("HIX_TOOLCHAIN");
        Assert.That(executable, Is.Not.Null.And.Not.Empty, "Set HIX_TOOLCHAIN to the built executable");
        using var process = Process.Start(new ProcessStartInfo(executable, "analysis-server") {
            UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
        });
        try {
            JToken Request(string method, object args) {
                process.StandardInput.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(new { id = 1, method, @params = args }));
                process.StandardInput.Flush();
                var read = process.StandardOutput.ReadLineAsync();
                Assert.That(read.Wait(TimeSpan.FromSeconds(30)), Is.True, "Toolchain response timeout");
                var response = JObject.Parse(read.Result);
                Assert.That(response["error"], Is.Null, response.ToString());
                return response["result"];
            }
            Request("hix/configureInterop", new { schema = JObject.Parse(HelixMixinLanguageHost.ExportCSharpHostSchema()) });
            var definitions = Request("hix/definitions", new { backend = "Unity" });
            Assert.That(definitions.Any(x => (string)x["name"] == "using"), Is.True);
            Assert.That(definitions.Any(x => (string)x["kind"] == "OutputTarget"), Is.True);
            var snapshots = Request("hix/synchronize", new { backend = "Unity", documents = new[] {
                new { path = "/virtual/interop.hix", text = "func main { using(<System>) }", revision = 7 }
            } });
            Assert.That((long)snapshots[0]["revision"], Is.EqualTo(7));
            Assert.That(snapshots[0]["diagnostics"].Count(), Is.Zero, snapshots.ToString());
        } finally { if (!process.HasExited) process.Kill(); }
    }
}
