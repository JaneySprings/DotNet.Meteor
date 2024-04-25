using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;

namespace DotNet.Meteor.Debug.Extensions;

public static class MonoExtensions {
    public static string ToThreadName(this string threadName, int threadId) {
        if (!string.IsNullOrEmpty(threadName))
            return threadName;
        if (threadId == 1)
            return "Main Thread";
        return $"Thread #{threadId}";
    }
    public static string ToDisplayValue(this ObjectValue value) {
        var dv = value.DisplayValue ?? "<error getting value>";
        if (dv.Length > 1 && dv[0] == '{' && dv[dv.Length - 1] == '}')
            dv = dv.Substring(1, dv.Length - 2).Replace(Environment.NewLine, " ");
        return dv;
    }

    public static StackFrame GetFrameSafe(this Backtrace bt, int n) {
        try {
            return bt.GetFrame(n);
        } catch (Exception) {
            return null;
        }
    }
    public static ThreadInfo FindThread(this SoftDebuggerSession session, long id) {
        var process = session.GetProcesses().FirstOrDefault();
        if (process == null)
            return null;

        return process.GetThreads().FirstOrDefault(it => it.Id == id);
    }
    public static ExceptionInfo FindException(this SoftDebuggerSession session, long id) {
        var thread = session.FindThread(id);
        if (thread == null)
            return null;

        for (int i = 0; i < thread.Backtrace.FrameCount; i++) {
            var frame = thread.Backtrace.GetFrameSafe(i);
            var ex = frame?.GetException();
            if (ex != null)
                return ex;
        }

        return null;
    }
    public static ObjectValue GetExpressionValue(this StackFrame frame, string expression, EvaluationOptions evaluationOptions, bool useExternalTypeResolver) {
        var options = evaluationOptions.Clone();
        options.UseExternalTypeResolver = useExternalTypeResolver;
        return frame.GetExpressionValue(expression, options);
    }
    public static string RemapSourceLocation(this SoftDebuggerSession session, SourceLocation location) {
        if (location == null || string.IsNullOrEmpty(location.FileName))
            return null;

        foreach (var remap in session.Options.SourceCodeMappings) {
            if (location.FileName.Contains(remap.Key))
                return location.FileName.Replace(remap.Key, remap.Value);
        }

        return location.FileName;
    }

    public static void SetUserAssemblyNames(this SoftDebuggerStartInfo startInfo, string assembliesDirectory, DebuggerSessionOptions options) {
        var includeSymbolServers = options.SearchMicrosoftSymbolServer || options.SearchNuGetSymbolServer;
        var assembliesPaths = Directory.EnumerateFiles(assembliesDirectory, "*.dll");
        var files = assembliesPaths.Where(it => SymbolServerExtensions.HasDebugSymbols(it, includeSymbolServers));
        if (!files.Any())
            return;

        var pathMap = new Dictionary<string, string>();
        var names = new List<AssemblyName>();

        foreach (var file in files) {
            try {
                using var asm = Mono.Cecil.AssemblyDefinition.ReadAssembly(file);
                if (string.IsNullOrEmpty(asm.Name.Name)) {
                    DebuggerLoggingService.CustomLogger.LogMessage($"Assembly '{file}' has no name");
                    continue;
                }

                AssemblyName name = new AssemblyName(asm.Name.FullName);
                if (!pathMap.ContainsKey(asm.Name.FullName))
                    pathMap.Add(asm.Name.FullName, file);

                names.Add(name);
                DebuggerLoggingService.CustomLogger.LogMessage($"User assembly '{name.Name}' added");
            } catch (Exception e) {
                DebuggerLoggingService.CustomLogger.LogError($"Error reading assembly '{file}'", e);
            }
        }

        startInfo.UserAssemblyNames = names;
        startInfo.AssemblyPathMap = pathMap;
    }
    public static void SetAssemblySymbols(this SoftDebuggerStartInfo startInfo, string assembliesDirectory, DebuggerSessionOptions options) {
        var useMicrosoftServer = options.SearchMicrosoftSymbolServer;
        var useNuGetServer = options.SearchNuGetSymbolServer;
        startInfo.SetAssemblySymbols(assembliesDirectory, useMicrosoftServer, useNuGetServer);
    }
    public static void SetAssemblySymbols(this SoftDebuggerStartInfo startInfo, string assembliesDirectory, bool useMicrosoftServer, bool useNuGetServer) {
        var assemblyPaths = Directory.EnumerateFiles(assembliesDirectory, "*.dll");
        var targetAssemblyPaths = assemblyPaths.Where(it => !File.Exists(Path.ChangeExtension(it, ".pdb")));
        if (!targetAssemblyPaths.Any() || (!useMicrosoftServer && !useNuGetServer))
            return;

        var symbolPathMap = new Dictionary<string, string>();
        foreach (var assemblyPath in targetAssemblyPaths) {
            string symbolsFilePath = null;
            string assemblyName = null;

            if (string.IsNullOrEmpty(symbolsFilePath) && useMicrosoftServer)
                symbolsFilePath = SymbolServerExtensions.DownloadSourceSymbols(assemblyPath, SymbolServerExtensions.MicrosoftSymbolServerAddress);
            if (string.IsNullOrEmpty(symbolsFilePath) && useNuGetServer)
                symbolsFilePath = SymbolServerExtensions.DownloadSourceSymbols(assemblyPath, SymbolServerExtensions.NuGetSymbolServerAddress);

            if (string.IsNullOrEmpty(symbolsFilePath)) {
                DebuggerLoggingService.CustomLogger.LogMessage($"No symbols found for '{assemblyPath}'");
                continue;
            }

            using var asm = Mono.Cecil.AssemblyDefinition.ReadAssembly(assemblyPath);
            assemblyName = asm.Name.FullName;

            if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(symbolsFilePath))
                symbolPathMap.Add(assemblyName, symbolsFilePath);
        }

        startInfo.SymbolPathMap = symbolPathMap;
    }

    public static void WriteSdbCommand(this ISoftDebuggerConnectionProvider connectionProvider, Stream stream, string command) {
        byte[] commandBytes = new byte[command.Length + 1];
        commandBytes[0] = (byte)command.Length;
        for (int i = 0; i < command.Length; i++)
            commandBytes[i + 1] = (byte)command[i];

        stream.Write(commandBytes, 0, commandBytes.Length);
    }
}