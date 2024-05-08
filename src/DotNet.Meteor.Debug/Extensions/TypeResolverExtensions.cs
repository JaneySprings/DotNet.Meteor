using System.Collections.Generic;
using Mono.Cecil;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug.Extensions;

public static class TypeResolverExtensions {
    private static readonly Dictionary<string, string> typesCache = new Dictionary<string, string>();
    private static EvaluationOptions evaluationOptions;
    private static StackFrame context;

    public static void RegisterTypes(IEnumerable<TypeDefinition> types) {
        foreach (var type in types) {
            if (string.IsNullOrEmpty(type.FullName) || string.IsNullOrEmpty(type.Name))
                continue;

            typesCache.TryAdd(type.Name, type.FullName);
        }
    }
    public static void SetContext(StackFrame frame, EvaluationOptions options) {
        evaluationOptions = options;
        context = frame;
    }

    public static string ResolveType(string type, SourceLocation _) {
        if (context == null || evaluationOptions == null)
            return type;

        var options = evaluationOptions.Clone();
        options.UseExternalTypeResolver = false;

        var value = context.GetExpressionValue(type, options);
        if (value.Flags.HasFlag(ObjectValueFlags.Object) && value.Flags.HasFlag(ObjectValueFlags.Namespace))
            return typesCache.TryGetValue(type, out var resolvedType) ? resolvedType : type;

        return type;
    }
}