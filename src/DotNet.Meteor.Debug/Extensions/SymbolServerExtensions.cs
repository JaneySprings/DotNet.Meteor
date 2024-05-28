using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug.Extensions;

public static class SymbolServerExtensions {
    public const string MicrosoftSymbolServerAddress = "https://msdl.microsoft.com/download/symbols";
    public const string NuGetSymbolServerAddress = "https://symbols.nuget.org/download/symbols";

    private static readonly HttpClient httpClient;
    private static Action<string> eventLogger;
    private static string symbolsDirectory;
    private static string sourcesDirectory;

    static SymbolServerExtensions() {
        httpClient = new HttpClient();
        SetSourcesDirectory(AppDomain.CurrentDomain.BaseDirectory);
        SetSymbolsDirectory(AppDomain.CurrentDomain.BaseDirectory);
    }

    public static void SetSourcesDirectory(string directory) {
        sourcesDirectory = Path.Combine(directory, "sources");
    }
    public static void SetSymbolsDirectory(string directory) {
        symbolsDirectory = Path.Combine(directory, "symbols");
    }
    public static void SetEventLogger(Action<string> logger) {
        eventLogger = logger;
    }
    public static string DownloadSourceFile(string uri) {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var sourceLinkUri)) {
            DebuggerLoggingService.CustomLogger.LogMessage($"Invalid source link '{uri}'");
            return null;
        }

        var outputFilePath = Path.Combine(sourcesDirectory, sourceLinkUri.LocalPath.TrimStart('/'));
        if (File.Exists(outputFilePath))
            return outputFilePath;

        _ = DownloadFileAsync(uri, outputFilePath, writeErrorInTarget: true);
        return outputFilePath;
    }
    public static string DownloadSourceSymbols(string assemblyPath, string assemblyName, string serverAddress) {
        var pdbData = GetPdbData(assemblyPath);
        if (pdbData == null)
            return null;

        var outputFilePath = Path.Combine(symbolsDirectory, pdbData.Id + ".pdb");
        if (File.Exists(outputFilePath))
            return outputFilePath;

        var request = $"{serverAddress}/{assemblyName}.pdb/{pdbData.Id}FFFFFFFF/{assemblyName}.pdb";
        // var header = $"SymbolChecksum: {pdbData.Hash}";
        if (DownloadFileAsync(request, outputFilePath).Result) {
            eventLogger?.Invoke($"Loaded symbols for '{assemblyName}'");
            return outputFilePath;
        }

        return null;
    }
    public static bool HasDebugSymbols(string assemblyPath, bool inludeSymbolServers) {
        var pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
        if (File.Exists(pdbPath))
            return true;
        if (!inludeSymbolServers)
            return false;

        var pdbData = GetPdbData(assemblyPath);
        if (pdbData == null)
            return false;

        pdbPath = Path.Combine(symbolsDirectory, pdbData.Id + ".pdb");
        return File.Exists(pdbPath);
    }

    private static async Task<bool> DownloadFileAsync(string url, string outputFilePath, bool writeErrorInTarget = false) {
        try {
            // if (!string.IsNullOrEmpty(header)) {
            //     httpClient.DefaultRequestHeaders.Remove("SymbolChecksum");
            //     httpClient.DefaultRequestHeaders.Add("SymbolChecksum", header);
            // }
            using var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return false;

            var directory = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var content = response.Content;
            var data = await content.ReadAsByteArrayAsync();
            File.WriteAllBytes(outputFilePath, data);
            return true;
        } catch (Exception ex) {
            if (writeErrorInTarget)
                File.WriteAllText(outputFilePath, ex.ToString());

            return false;
        }
    }
    private static PdbData GetPdbData(string assemblyPath) {
        try {
            using var peReader = new PEReader(File.OpenRead(assemblyPath));
            var codeViewEntries = peReader.ReadDebugDirectory().Where(entry => entry.Type == DebugDirectoryEntryType.CodeView);
            var checkSumEntries = peReader.ReadDebugDirectory().Where(entry => entry.Type == DebugDirectoryEntryType.PdbChecksum);
            if (!codeViewEntries.Any() || !checkSumEntries.Any())
                return null;

            return new PdbData(
                peReader.ReadCodeViewDebugDirectoryData(codeViewEntries.First()),
                peReader.ReadPdbChecksumDebugDirectoryData(checkSumEntries.First())
            );
        } catch (Exception ex) {
            DebuggerLoggingService.CustomLogger.LogError($"Error reading assembly '{assemblyPath}'", ex);
            return null;
        }
    }

    private class PdbData {
        private readonly CodeViewDebugDirectoryData codeView;
        private readonly PdbChecksumDebugDirectoryData checksum;

        public PdbData(CodeViewDebugDirectoryData codeView, PdbChecksumDebugDirectoryData checksum) {
            this.codeView = codeView;
            this.checksum = checksum;
        }

        public string Id => codeView.Guid.ToString("N");
        public string Hash => $"{checksum.AlgorithmName}:{BitConverter.ToString(checksum.Checksum.ToArray()).Replace("-", string.Empty)}";
    }
}
