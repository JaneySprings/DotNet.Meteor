using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Extensions;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug;

public class DebuggerOptions {
    [JsonPropertyName("evaluation_timeout")] 
    public int EvaluationTimeout { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.EvaluationTimeout;

    [JsonPropertyName("member_evaluation_timeout")] 
    public int MemberEvaluationTimeout { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.MemberEvaluationTimeout;

    [JsonPropertyName("allow_target_invoke")] 
    public bool AllowTargetInvoke { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.AllowTargetInvoke;

    [JsonPropertyName("allow_method_evaluation")]
    public bool AllowMethodEvaluation { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.AllowMethodEvaluation;

    [JsonPropertyName("allow_to_string_calls")]
    public bool AllowToStringCalls { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.AllowToStringCalls;

    [JsonPropertyName("flatten_hierarchy")]
    public bool FlattenHierarchy { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.FlattenHierarchy;

    [JsonPropertyName("group_private_members")]
    public bool GroupPrivateMembers { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.GroupPrivateMembers;

    [JsonPropertyName("group_static_members")]
    public bool GroupStaticMembers { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.GroupStaticMembers;

    [JsonPropertyName("use_external_type_resolver")]
    public bool UseExternalTypeResolver { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.UseExternalTypeResolver;

    [JsonPropertyName("integer_display_format")]
    public string IntegerDisplayFormat { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.IntegerDisplayFormat.ToString();

    [JsonPropertyName("current_exception_tag")]
    public string CurrentExceptionTag { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.CurrentExceptionTag;

    [JsonPropertyName("ellipsize_strings")]
    public bool EllipsizeStrings { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.EllipsizeStrings;

    [JsonPropertyName("ellipsized_length")]
    public int EllipsizedLength { get; set; } = ServerExtensions.DefaultDebuggerOptions.EvaluationOptions.EllipsizedLength;

    [JsonPropertyName("project_assemblies_only")]
    public bool ProjectAssembliesOnly { get; set; } = ServerExtensions.DefaultDebuggerOptions.ProjectAssembliesOnly;

    internal static IntegerDisplayFormat GetIntegerDisplayFormat(string value) {
        if (value == Mono.Debugging.Client.IntegerDisplayFormat.Decimal.ToString())
            return Mono.Debugging.Client.IntegerDisplayFormat.Decimal;
        if (value == Mono.Debugging.Client.IntegerDisplayFormat.Hexadecimal.ToString())
            return Mono.Debugging.Client.IntegerDisplayFormat.Hexadecimal;

        return Mono.Debugging.Client.IntegerDisplayFormat.Decimal;
    }
}

[JsonSerializable(typeof(DebuggerOptions))]
internal partial class DebuggerOptionsContext : JsonSerializerContext {}