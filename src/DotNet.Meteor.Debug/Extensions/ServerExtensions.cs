using System;
using System.Net;
using System.Net.Sockets;
using Mono.Debugging.Client;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using NewtonConverter = Newtonsoft.Json.JsonConvert;

namespace DotNet.Meteor.Debug.Extensions;

public static class ServerExtensions {
    public static DebuggerSessionOptions DefaultDebuggerOptions = new DebuggerSessionOptions {
        EvaluationOptions = new EvaluationOptions {
            EvaluationTimeout = 5000,
            MemberEvaluationTimeout = 5000,
            UseExternalTypeResolver = false,
            AllowMethodEvaluation = true,
            GroupPrivateMembers = true,
            GroupStaticMembers = true,
            AllowToStringCalls = true,
            AllowTargetInvoke = true,
            ChunkRawStrings = false,
            EllipsizeStrings = false,
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
}