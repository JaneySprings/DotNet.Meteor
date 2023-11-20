using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Extensions;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug;

public class DebuggerOptions {
    [JsonPropertyName("evaluation_timeout")] 
    public int EvaluationTimeout { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.EvaluationTimeout;

    [JsonPropertyName("member_evaluation_timeout")] 
    public int MemberEvaluationTimeout { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.MemberEvaluationTimeout;

    [JsonPropertyName("allow_target_invoke")] 
    public bool AllowTargetInvoke { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.AllowTargetInvoke;

    [JsonPropertyName("allow_method_evaluation")]
    public bool AllowMethodEvaluation { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.AllowMethodEvaluation;

    [JsonPropertyName("allow_to_string_calls")]
    public bool AllowToStringCalls { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.AllowToStringCalls;

    [JsonPropertyName("flatten_hierarchy")]
    public bool FlattenHierarchy { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.FlattenHierarchy;

    [JsonPropertyName("group_private_members")]
    public bool GroupPrivateMembers { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.GroupPrivateMembers;

    [JsonPropertyName("group_static_members")]
    public bool GroupStaticMembers { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.GroupStaticMembers;

    [JsonPropertyName("use_external_type_resolver")]
    public bool UseExternalTypeResolver { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.UseExternalTypeResolver;

    [JsonPropertyName("integer_display_format")]
    public string IntegerDisplayFormat { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.IntegerDisplayFormat.ToString();

    [JsonPropertyName("current_exception_tag")]
    public string CurrentExceptionTag { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.CurrentExceptionTag;

    [JsonPropertyName("ellipsize_strings")]
    public bool EllipsizeStrings { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.EllipsizeStrings;

    [JsonPropertyName("ellipsized_length")]
    public int EllipsizedLength { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.EllipsizedLength;

    [JsonPropertyName("chunk_raw_strings")]
    public bool ChunkRawStrings { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.ChunkRawStrings;

    [JsonPropertyName("ienumerable")]
    public bool IEnumerable { get; set; } = MonoClientExtensions.DefaultDebuggerOptions.EvaluationOptions.IEnumerable;


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