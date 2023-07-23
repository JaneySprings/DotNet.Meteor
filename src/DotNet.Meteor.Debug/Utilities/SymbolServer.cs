using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DotNet.Meteor.Debug.Utilities;

public class SymbolServer {
    private readonly string sourceDirectory;
    private readonly string binariesDirectory;
    private readonly HttpClient httpClient;

    public SymbolServer(string csprojPath) {
        this.httpClient = new HttpClient();
        this.sourceDirectory = Path.Combine(Path.GetDirectoryName(csprojPath), ".meteor", "sources");
        this.binariesDirectory = AppDomain.CurrentDomain.BaseDirectory;

        if (!Directory.Exists(this.sourceDirectory))
            Directory.CreateDirectory(this.sourceDirectory);

        var githubKeyPath = Path.Combine(this.binariesDirectory, "github.key");
        if (File.Exists(githubKeyPath))
            RegisterGithubHeader(githubKeyPath);
    }

    public string DownloadSourceFile(string url, string filePath) {
        var outputFilePath = Path.Combine(this.sourceDirectory, filePath);
        if (File.Exists(outputFilePath))
            return outputFilePath;

        DowndloadFileAsync(url, outputFilePath);
        return outputFilePath;
    }

    private async void DowndloadFileAsync(string url, string outputFilePath) {
        try {
            using var response = await httpClient.GetAsync(url);
            using var content = response.Content;
            var data = await content.ReadAsByteArrayAsync();
            var directory = Path.GetDirectoryName(outputFilePath);
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(outputFilePath, data);
        } catch (Exception ) { /*Ignore*/ }
    }

    private void RegisterGithubHeader(string githubKeyPath) {
        var githubKey = File.ReadAllText(githubKeyPath);
        var credentials = string.Format(CultureInfo.InvariantCulture, "{0}:", githubKey);
        credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
        this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }
}
