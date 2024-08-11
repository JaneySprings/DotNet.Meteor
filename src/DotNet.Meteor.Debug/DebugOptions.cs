using System.Collections.Immutable;
using System.Text.Json.Serialization;
using DotNet.Meteor.Debug.Extensions;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug;

public class DebugOptions {
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

    [JsonPropertyName("step_over_properties_and_operators")]
    public bool StepOverPropertiesAndOperators { get; set; } = ServerExtensions.DefaultDebuggerOptions.StepOverPropertiesAndOperators;

    [JsonPropertyName("search_microsoft_symbol_server")]
    public bool SearchMicrosoftSymbolServer { get; set; } = ServerExtensions.DefaultDebuggerOptions.SearchMicrosoftSymbolServer;

    [JsonPropertyName("search_nuget_symbol_server")]
    public bool SearchNuGetSymbolServer { get; set; } = ServerExtensions.DefaultDebuggerOptions.SearchNuGetSymbolServer;

    [JsonPropertyName("source_code_mappings")]
    public ImmutableDictionary<string, string> SourceCodeMappings { get; set; } = ServerExtensions.DefaultDebuggerOptions.SourceCodeMappings;

    [JsonPropertyName("automatic_sourcelink_download")]
    public bool AutomaticSourceLinkDownload { get; set; } = ServerExtensions.DefaultDebuggerOptions.AutomaticSourceLinkDownload;

    [JsonPropertyName("symbol_search_paths")]
    public ImmutableArray<string> SymbolSearchPaths { get; set; } = ServerExtensions.DefaultDebuggerOptions.SymbolSearchPaths;

    internal static IntegerDisplayFormat GetIntegerDisplayFormat(string value) {
        if (value == Mono.Debugging.Client.IntegerDisplayFormat.Decimal.ToString())
            return Mono.Debugging.Client.IntegerDisplayFormat.Decimal;
        if (value == Mono.Debugging.Client.IntegerDisplayFormat.Hexadecimal.ToString())
            return Mono.Debugging.Client.IntegerDisplayFormat.Hexadecimal;

        return Mono.Debugging.Client.IntegerDisplayFormat.Decimal;
    }
}

[JsonSerializable(typeof(DebugOptions))]
internal partial class DebuggerOptionsContext : JsonSerializerContext { }