using System.Collections.Generic;
using Mono.Cecil;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug.Extensions;

public static class TypeResolverExtensions {
    private static readonly Dictionary<string, string> typesCache = new Dictionary<string, string>();

    public static void RegisterTypes(IEnumerable<TypeDefinition> types) {
        foreach (var type in types) {
            if (string.IsNullOrEmpty(type.FullName) || string.IsNullOrEmpty(type.Name))
                continue;

            typesCache.TryAdd(type.Name, type.FullName);
        }
    }

    public static string ResolveType(string type, SourceLocation _) {
        return typesCache.TryGetValue(type, out var resolvedType) ? resolvedType : type;
    }
}