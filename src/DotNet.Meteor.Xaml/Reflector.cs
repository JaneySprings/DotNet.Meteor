using System.Reflection;

namespace DotNet.Meteor.Xaml;

public class Reflector {
    private readonly Action<string>? logger;

    public Reflector(Action<string>? logger = null) {
        this.logger = logger;
    }

    public SchemaInfo CreateAlias(string path) {
        var assembly = Assembly.LoadFrom(path);
        var elements = new List<TypeInfo>();
        var schema = new SchemaInfo(assembly.GetName().Name, elements);


        foreach (var type in assembly.GetTypes())
            if (type.IsSubclassOf(MauiTypeLoader.BindableObject!) && !type.IsAbstract && type.GetCustomAttribute(typeof(System.ComponentModel.EditorBrowsableAttribute)) == null)
                elements.Add(new TypeInfo(type.Name, $"{type.Namespace}.{type.Name}", GetAttributes(type)));

        var xmlnsAttribute = assembly.GetCustomAttributes().FirstOrDefault(it => it.GetType() == MauiTypeLoader.XmlnsDefinitionAttribute!);
        if (xmlnsAttribute != null && MauiTypeLoader.XmlnsDefinitionAttribute!.GetProperty("XmlNamespace")?.GetValue(xmlnsAttribute) is string xmlns)
            schema.Xmlns = xmlns;

        return schema;
    }

    private List<AttributeInfo> GetAttributes(Type type) {
        var attributes = new List<AttributeInfo>();

        // Event handlers
        foreach (var ev in type.GetEvents()) {
            if (ev.GetCustomAttribute(typeof(System.ComponentModel.EditorBrowsableAttribute)) != null)
                continue;

            var declaringType = ev.DeclaringType;
            var nspace = $"{declaringType?.Namespace}.{declaringType?.Name}.{ev.EventHandlerType?.Name}";
            var isObsoleted = ev.GetCustomAttribute(typeof(System.ObsoleteAttribute)) != null;
            attributes.Add(new AttributeInfo(ev.Name, nspace) {
                IsObsolete = isObsoleted,
                IsEvent = true
            });
        }
        // Properties
        foreach (var property in type.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy)) {
            try {
                if (property.GetCustomAttribute(typeof(System.ComponentModel.EditorBrowsableAttribute)) != null)
                    continue;

                var propertyType = property.PropertyType;
                var declaringType = property.DeclaringType;
                var nspace = $"{declaringType?.Namespace}.{declaringType?.Name}.{property.Name}";

                attributes.Add(new AttributeInfo(property.Name, nspace) {
                    IsObsolete = property.GetCustomAttribute(typeof(System.ObsoleteAttribute)) != null,
                    Values = GetEnumInfos(propertyType)
                });
            } catch {
                this.logger?.Invoke($"Error injecting {property.Name} from {type.Name}");
            }
        }
        // Attached properties
        foreach (var method in type.GetMethods(BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).Where(it => it.Name.StartsWith("Get", StringComparison.Ordinal))) {
            try {
                if (method.GetCustomAttribute(typeof(System.ComponentModel.EditorBrowsableAttribute)) != null)
                    continue;

                var propertyType = method.ReturnType;
                var declaringType = method.DeclaringType;
                var nspace = $"{declaringType?.Namespace}.{declaringType?.Name}.{method.Name[3..]}";

                attributes.Add(new AttributeInfo(method.Name[3..], nspace) {
                    IsObsolete = method.GetCustomAttribute(typeof(System.ObsoleteAttribute)) != null,
                    Values = GetEnumInfos(propertyType),
                    IsAttached = true
                });
            } catch {
                this.logger?.Invoke($"Error injecting {method.Name} from {type.Name}");
            }
        }

        return attributes;
    }


    private List<EnumInfo>? GetEnumInfos(Type type) {
        var result = new List<EnumInfo>();
        if (type.IsEnum || (type.IsValueType && !type.IsPrimitive)) result.AddRange(type.GetFields().Select(it => {
            var isObsolete = it.GetCustomAttribute(typeof(System.ObsoleteAttribute)) != null;
            return new EnumInfo(it.Name, $"{type.Namespace}.{type.Name}.{it.Name}") {
                IsObsolete = isObsolete
            };
        }));

        return result.Any() ? result : null;
    }
}