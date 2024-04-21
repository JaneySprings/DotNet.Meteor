using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug.Extensions;

public static class SymbolServerExtensions {
    private static readonly HttpClient httpClient;
    private static string tempDirectory;

    static SymbolServerExtensions() {
        httpClient = new HttpClient();
        tempDirectory = AppDomain.CurrentDomain.BaseDirectory;

        var githubKey = Environment.GetEnvironmentVariable("GH_TOKEN");
        if (!string.IsNullOrEmpty(githubKey))
            RegisterGithubHeader(githubKey);
    }

    public static void SetTempDirectory(string directory) {
        tempDirectory = directory;
    }
    public static string DownloadSourceFile(SourceLink link) {
        var sourcesDirectory = Path.Combine(tempDirectory, "sources");
        if (!Directory.Exists(sourcesDirectory))
            Directory.CreateDirectory(sourcesDirectory);

        var outputFilePath = Path.Combine(sourcesDirectory, link.RelativeFilePath);
        if (File.Exists(outputFilePath))
            return outputFilePath;

        _ = DownloadFileAsync(link.Uri, outputFilePath);
        return outputFilePath;
    }

    private static async Task DownloadFileAsync(string url, string outputFilePath) {
        var directory = Path.GetDirectoryName(outputFilePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        try {
            using var response = await httpClient.GetAsync(url);
            using var content = response.Content;
            var data = await content.ReadAsByteArrayAsync();
            File.WriteAllBytes(outputFilePath, data);
        } catch (Exception ex) {
            File.WriteAllText(outputFilePath, ex.ToString());
        }
    }
    private static void RegisterGithubHeader(string githubKey) {
        var credentials = string.Format(CultureInfo.InvariantCulture, "{0}:", githubKey);
        credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }
}
