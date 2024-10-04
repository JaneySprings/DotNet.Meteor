using System.Net;
using System.Net.Sockets;
using Mono.Debugging.Client;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using NewtonConverter = Newtonsoft.Json.JsonConvert;
using DebugProtocol = Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using DotNet.Meteor.Common;
using DotNet.Meteor.Common.Extensions;
using Mono.Debugging.Soft;
using System.IO.Compression;
using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Extensions;

public static class ServerExtensions {
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
    public static JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true,
    };


    public static bool TryDeleteFile(string path) {
        if (File.Exists(path))
            File.Delete(path);
        return !File.Exists(path);
    }
    public static ProtocolException GetProtocolException(string message) {
        return new ProtocolException(message, 0, message, url: $"file://{LogConfig.DebugLogFile}");
    }
    public static int GetSourceReference(this StackFrame frame) {
        var key = string.IsNullOrEmpty(frame.SourceLocation.FileName)
            ? frame.SourceLocation.MethodName ?? "null"
            : frame.SourceLocation.FileName;
        return Math.Abs(key.GetHashCode());
    }
    public static string ExtractAndroidAssemblies(string programPath) {
        var targetDirectory = Path.GetDirectoryName(programPath)!;

        try {
            using var archive = new ZipArchive(File.OpenRead(programPath));
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
                TryDeleteFile(targetPath);

                using var fileStream = File.Create(targetPath);
                using var stream = entry.Open();
                stream.CopyTo(fileStream);
            }
            return targetDirectory;
        } catch (Exception ex) {
            DebuggerLoggingService.CustomLogger.LogError(ex.Message, ex);
            return targetDirectory;
        }
    }
    public static string? TrimExpression(this DebugProtocol.EvaluateArguments args) {
        return args.Expression?.TrimEnd(';')?.Replace("?.", ".");
    }

    public static T DoSafe<T>(Func<T> handler, Action? finalizer = null) {
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

    public static JToken? TryGetValue(this Dictionary<string, JToken> dictionary, string key) {
        if (dictionary.TryGetValue(key, out var value))
            return value;
        return null;
    }
    public static T? ToClass<T>(this JToken? jtoken) where T: class {
        if (jtoken == null)
            return default;

        string json = NewtonConverter.SerializeObject(jtoken);
        if (string.IsNullOrEmpty(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }
    public static T ToValue<T>(this JToken? jtoken) where T: struct {
        if (jtoken == null)
            return default;

        string json = NewtonConverter.SerializeObject(jtoken);
        if (string.IsNullOrEmpty(json))
            return default;

        return JsonSerializer.Deserialize<T>(json);
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
    public static DebugProtocol.Module ToModule(this Assembly assembly) {
        return new DebugProtocol.Module {
	        Id = assembly.GetHashCode(),
	        Name = assembly.Name,
            Path = assembly.Path,
            IsOptimized = assembly.Optimized,
            IsUserCode = assembly.UserCode,
            Version = assembly.Version,
            SymbolFilePath = assembly.SymbolFile,
            DateTimeStamp = assembly.TimeStamp,
            AddressRange = assembly.Address,
            SymbolStatus = assembly.SymbolStatus,
            VsAppDomain = assembly.AppDomain,
        };
    }
    public static DebugProtocol.VSSourceLinkInfo ToSourceLinkInfo(this SourceLink sourceLink) {
        return new DebugProtocol.VSSourceLinkInfo {
            Url = sourceLink?.Uri,
            RelativeFilePath = sourceLink?.RelativeFilePath,
        };
    }
}

