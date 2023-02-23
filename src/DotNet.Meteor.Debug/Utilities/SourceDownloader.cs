using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;

namespace DotNet.Meteor.Debug.Utilities;

public class SourceDownloader {
    private readonly Logger logger = LogManager.GetCurrentClassLogger();
    private string sourceDirectory;

    public void Configure(string csprojPath) {
        this.sourceDirectory = Path.Combine(Path.GetDirectoryName(csprojPath), ".meteor", "sources");
        if (!Directory.Exists(this.sourceDirectory))
            Directory.CreateDirectory(this.sourceDirectory);
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
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            using var content = response.Content;
            var data = await content.ReadAsByteArrayAsync();
            var directory = Path.GetDirectoryName(outputFilePath);
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(outputFilePath, data);
        } catch (Exception e) {
            this.logger.Error(e);
        }
    }
}