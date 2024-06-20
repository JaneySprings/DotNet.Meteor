using System.Net;
using System.Net.Sockets;
using Mono.Debugging.Client;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using NewtonConverter = Newtonsoft.Json.JsonConvert;
using DebugProtocol = Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using System.IO;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using DotNet.Meteor.Common;
using DotNet.Meteor.Common.Extensions;
using Mono.Debugging.Soft;
using System.IO.Compression;
using System;
using System.Linq;

namespace DotNet.Meteor.Debug.Extensions;

public static class ServerExtensions {
    private static bool isAndroidAssembliesExtracted;
    public static DebuggerSessionOptions DefaultDebuggerOptions { get; } = new DebuggerSessionOptions {
        EvaluationOptions = new EvaluationOptions {
            EvaluationTimeout = 1000,
            MemberEvaluationTimeout = 5000,
            UseExternalTypeResolver = true,
            AllowMethodEvaluation = true,
            GroupPrivateMembers = true,
            GroupStaticMembers = true,
            AllowToStringCalls = true,
            AllowTargetInvoke = true,
            EllipsizeStrings = true,
            EllipsizedLength = 260,
            CurrentExceptionTag = "$exception",
            IntegerDisplayFormat = IntegerDisplayFormat.Decimal,
            StackFrameFormat = new StackFrameFormat()
        },
        ProjectAssembliesOnly = true
    };

    public static int FindFreePort() {
        TcpListener listener = null;
        try {
            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        } finally {
            listener.Stop();
        }
    }
    public static bool TryDeleteFile(string path) {
        if (File.Exists(path))
            File.Delete(path);
        return !File.Exists(path);
    }
    public static ProtocolException GetProtocolException(string message) {
        return new ProtocolException(message, 0, message, url: $"file://{LogConfig.DebugLogFile}");
    }
    public static string ExtractAndroidAssemblies(string assemblyPath) {
        var targetDirectory = Path.GetDirectoryName(assemblyPath)!;
        if (isAndroidAssembliesExtracted)
            return targetDirectory;

        try {
            using var archive = new ZipArchive(File.OpenRead(assemblyPath));
            var assembliesEntry = archive.Entries.Where(entry => entry.FullName.StartsWith("assemblies", StringComparison.OrdinalIgnoreCase));
            if (!assembliesEntry.Any()) {
                // For net9+ the assemblies are not in the assemblies folder
                assembliesEntry = archive.Entries.Where(entry =>
                    entry.FullName.EndsWith(".dll.so", StringComparison.OrdinalIgnoreCase) ||
                    entry.FullName.EndsWith(".pdb.so", StringComparison.OrdinalIgnoreCase)
                );
            }
            if (!assembliesEntry.Any())
                return targetDirectory;

            foreach (var entry in assembliesEntry) {
                var assemblyFileName = entry.Name.TrimStart("lib_").TrimEnd(".so");
                var targetPath = Path.Combine(targetDirectory, assemblyFileName);
                if (File.Exists(targetPath))
                    File.Delete(targetPath);

                using var fileStream = File.Create(targetPath);
                using var stream = entry.Open();
                stream.CopyTo(fileStream);
            }
            isAndroidAssembliesExtracted = true;
            return targetDirectory;
        } catch (Exception ex) {
            DebuggerLoggingService.CustomLogger.LogError(ex.Message, ex);
            return targetDirectory;
        }
    }
    public static string TrimExpression(this DebugProtocol.EvaluateArguments args) {
        return args.Expression?.TrimEnd(';')?.Replace("?.", ".");
    }

    public static T DoSafe<T>(Func<T> handler, Action finalizer = null) {
        try {
            return handler.Invoke();
        } catch (Exception ex) {
            finalizer?.Invoke();
            if (ex is ProtocolException)
                throw;
            DebuggerLoggingService.CustomLogger.LogError($"[Handled] {ex.Message}", ex);
            throw GetProtocolException(ex.Message);
        }
    }

    public static T ToObject<T>(this JToken jtoken, JsonTypeInfo<T> type) {
        if (jtoken == null)
            return default;

        string json = NewtonConverter.SerializeObject(jtoken);
        if (string.IsNullOrEmpty(json))
            return default;

        return JsonSerializer.Deserialize(json, type);
    }
    public static DebugProtocol.CompletionItem ToCompletionItem(this CompletionItem item) {
        return new DebugProtocol.CompletionItem() {
            Type = item.Flags.ToCompletionItemType(),
            SortText = item.Name,
            Label = item.Name,
        };
    }
    private static DebugProtocol.CompletionItemType ToCompletionItemType(this ObjectValueFlags flags) {
        if (flags.HasFlag(ObjectValueFlags.Method))
            return DebugProtocol.CompletionItemType.Method;
        if (flags.HasFlag(ObjectValueFlags.Field))
            return DebugProtocol.CompletionItemType.Field;
        if (flags.HasFlag(ObjectValueFlags.Property))
            return DebugProtocol.CompletionItemType.Property;
        if (flags.HasFlag(ObjectValueFlags.Namespace))
            return DebugProtocol.CompletionItemType.Module;
        if (flags.HasFlag(ObjectValueFlags.Type))
            return DebugProtocol.CompletionItemType.Class;
        if (flags.HasFlag(ObjectValueFlags.Variable))
            return DebugProtocol.CompletionItemType.Variable;

        return DebugProtocol.CompletionItemType.Text;
    }
    public static DebugProtocol.Breakpoint ToBreakpoint(this Breakpoint breakpoint, SoftDebuggerSession session) {
        return new DebugProtocol.Breakpoint() {
            Id = breakpoint.GetHashCode(),
            Verified = breakpoint.GetStatus(session) == BreakEventStatus.Bound,
            Message = breakpoint.GetStatusMessage(session),
            Line = breakpoint.Line,
            Column = breakpoint.Column,
        };
    }
}

public class HotReloadRequest : DebugProtocol.DebugRequest<HotReloadArguments> {
    public HotReloadRequest() : base("hotReload") {}
}
public class HotReloadResponse : DebugProtocol.ResponseBody {}
public class HotReloadArguments {
    public string FilePath { get; set; }
}
