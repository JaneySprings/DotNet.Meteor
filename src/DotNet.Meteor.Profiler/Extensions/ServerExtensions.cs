using System.Text.Json;
using Newtonsoft.Json.Linq;
using NewtonConverter = Newtonsoft.Json.JsonConvert;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using DotNet.Meteor.Common;
using System.Text.Json.Serialization;
using DotNet.Meteor.Profiler.Logging;

namespace DotNet.Meteor.Profiler.Extensions;

public static class ServerExtensions {
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
    public static T DoSafe<T>(Func<T> handler, Action? finalizer = null) {
        try {
            return handler.Invoke();
        } catch (Exception ex) {
            finalizer?.Invoke();
            if (ex is ProtocolException)
                throw;
            CurrentSessionLogger.Error($"[Handled] {ex}");
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

        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }
    public static string ToAndroidEnvString(this Dictionary<string, string> environment) {
        //https://github.com/dotnet/android/blob/b1241329f531b985b9c462b3f684e2ca3e0db98d/Documentation/workflow/SystemProperties.md#debugmonoenv
        var maxEnvLength = 49;
        var envPairs = new List<string>();
        
        foreach (var env in environment) {
            if (maxEnvLength - env.Key.Length - 2 < 0)
                break;

            maxEnvLength -= env.Key.Length + 1;
            if (maxEnvLength >= env.Value.Length)
                envPairs.Add($"{env.Key}={env.Value}");
            else
                envPairs.Add($"{env.Key}={env.Value.Substring(env.Value.Length - maxEnvLength)}");

            maxEnvLength -= env.Value.Length + 1; // +1 for '|'
        }
    
        return $"'{string.Join('|', envPairs)}'";
    }
}

