using System;
using System.Net;
using System.Net.Sockets;
using Mono.Debugging.Client;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using NewtonConverter = Newtonsoft.Json.JsonConvert;
using DebugProtocol =  Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using System.IO;

namespace DotNet.Meteor.Debug.Extensions;

public static class ServerExtensions {
    public static DebuggerSessionOptions DefaultDebuggerOptions = new DebuggerSessionOptions {
        EvaluationOptions = new EvaluationOptions {
            EvaluationTimeout = 5000,
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

    public static StackFrame GetFrameSafe(this Backtrace bt, int n) {
		try {
            return bt.GetFrame(n);
        } catch (Exception) {
            return null;
        }
	}


    public static string ToThreadName(this string threadName, int threadId) {
        if (!string.IsNullOrEmpty(threadName))
            return threadName;
        if (threadId == 1)
            return "Main Thread";
        return $"Thread #{threadId}";
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
}