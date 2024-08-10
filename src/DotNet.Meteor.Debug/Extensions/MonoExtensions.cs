using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
    public static string GetAssemblyCode(this StackFrame frame) {
        var assemblyLines = frame.Disassemble(-1, -1);
        var sb = new StringBuilder();
        foreach (var line in assemblyLines)
            sb.AppendLine($"({line.SourceLine}) IL_{line.Address:0000}: {line.Code}");
        
        return sb.ToString();
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
    public static string RemapSourceLocation(this SoftDebuggerSession session, SourceLocation location) {
        if (location == null || string.IsNullOrEmpty(location.FileName))
            return null;

        foreach (var remap in session.Options.SourceCodeMappings) {
            if (location.FileName.Contains(remap.Key))
                return location.FileName.Replace(remap.Key, remap.Value);
        }

        return location.FileName;
    }

    public static void SetAssemblies(this SoftDebuggerStartInfo startInfo, string assembliesDirectory, DebuggerSessionOptions options) {
        var useSymbolServers = options.SearchMicrosoftSymbolServer || options.SearchNuGetSymbolServer;
        var assemblyPaths = Directory.EnumerateFiles(assembliesDirectory, "*.dll");
        var assemblyPathMap = new Dictionary<string, string>();
        var assemblySymbolPathMap = new Dictionary<string, string>();
        var assemblyNames = new List<AssemblyName>();

        foreach (var assemblyPath in assemblyPaths) {
            try {
                using var assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly(assemblyPath);
                if (string.IsNullOrEmpty(assemblyDefinition.Name.FullName)) {
                    DebuggerLoggingService.CustomLogger.LogMessage($"Assembly '{assemblyPath}' has no name");
                    continue;
                }

                string assemblySymbolsFilePath = Path.ChangeExtension(assemblyPath, ".pdb");
                if (!File.Exists(assemblySymbolsFilePath))
                    assemblySymbolsFilePath = null; 
                if (string.IsNullOrEmpty(assemblySymbolsFilePath) && options.SearchMicrosoftSymbolServer)
                    assemblySymbolsFilePath = SymbolServerExtensions.DownloadSourceSymbols(assemblyPath, assemblyDefinition.Name.Name, SymbolServerExtensions.MicrosoftSymbolServerAddress);
                if (string.IsNullOrEmpty(assemblySymbolsFilePath) && options.SearchNuGetSymbolServer)
                    assemblySymbolsFilePath = SymbolServerExtensions.DownloadSourceSymbols(assemblyPath, assemblyDefinition.Name.Name, SymbolServerExtensions.NuGetSymbolServerAddress);
                if (string.IsNullOrEmpty(assemblySymbolsFilePath))
                    DebuggerLoggingService.CustomLogger.LogMessage($"No symbols found for '{assemblyPath}'");
                

                if (options.EvaluationOptions.UseExternalTypeResolver)
                    TypeResolverExtensions.RegisterTypes(assemblyDefinition.MainModule.Types);

                if (!string.IsNullOrEmpty(assemblySymbolsFilePath))
                    assemblySymbolPathMap.Add(assemblyDefinition.Name.FullName, assemblySymbolsFilePath);

                if (options.ProjectAssembliesOnly && SymbolServerExtensions.HasDebugSymbols(assemblyPath, useSymbolServers)) {
                    var assemblyName = new AssemblyName(assemblyDefinition.Name.FullName);
                    assemblyPathMap.TryAdd(assemblyDefinition.Name.FullName, assemblyPath);
                    assemblyNames.Add(assemblyName);
                    DebuggerLoggingService.CustomLogger.LogMessage($"User assembly '{assemblyName.Name}' added");
                }
            } catch (Exception e) {
                DebuggerLoggingService.CustomLogger.LogError($"Error while processing assembly '{assemblyPath}'", e);
            }
        }

        startInfo.SymbolPathMap = assemblySymbolPathMap;
        startInfo.AssemblyPathMap = assemblyPathMap;
        startInfo.UserAssemblyNames = assemblyNames;
    }

    public static void WriteSdbCommand(this ISoftDebuggerConnectionProvider connectionProvider, Stream stream, string command) {
        byte[] commandBytes = new byte[command.Length + 1];
        commandBytes[0] = (byte)command.Length;
        for (int i = 0; i < command.Length; i++)
            commandBytes[i + 1] = (byte)command[i];

        stream.Write(commandBytes, 0, commandBytes.Length);
    }
}