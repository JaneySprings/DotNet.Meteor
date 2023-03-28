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
        string xmlNamespace = $"assembly={assembly.GetName().Name}";

        foreach (var type in assembly.GetTypes())
            if (type.IsSubclassOf(MauiTypeLoader.BindableObject!) && !type.IsAbstract)
                elements.Add(new TypeInfo(type.Name, type.Namespace, GetAttributes(type)));

        var xmlnsAttribute = assembly.GetCustomAttributes().FirstOrDefault(it => it.GetType() == MauiTypeLoader.XmlnsDefinitionAttribute!);
        if (xmlnsAttribute != null && MauiTypeLoader.XmlnsDefinitionAttribute!.GetProperty("XmlNamespace")?.GetValue(xmlnsAttribute) is string xmlns)
            xmlNamespace = xmlns;

        return new SchemaInfo(xmlNamespace, elements);
    }

    private List<AttributeInfo> GetAttributes(Type type) {
        var attributes = new List<AttributeInfo>();

        // Event handlers
        foreach (var ev in type.GetEvents()) {
            var declaringType = ev.DeclaringType;
            var nspace = $"{declaringType?.Namespace}.{declaringType?.Name}";
            attributes.Add(new AttributeInfo(
                ev.Name, nspace, ev.EventHandlerType?.Name, isEvent: true
            ));
        }
        // Properties
        foreach (var property in type.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy)) {
            try {
                var propertyType = property.PropertyType;
                var declaringType = property.DeclaringType;
                var nspace = $"{declaringType?.Namespace}.{declaringType?.Name}";
                object fieldType = propertyType.Name;

                if (propertyType.IsEnum)
                    fieldType = Enum.GetNames(propertyType);

                if (propertyType.IsValueType && !propertyType.IsPrimitive)
                    fieldType = propertyType
                        .GetFields()
                        .ToList()
                        .ConvertAll(it => it.Name);

                attributes.Add(new AttributeInfo(property.Name, nspace, fieldType));
            } catch {
                this.logger?.Invoke($"Error injecting {property.Name} from {type.Name}");
            }
        }
        // Attached properties
        foreach (var method in type.GetMethods(BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).Where(it => it.Name.StartsWith("Get", StringComparison.Ordinal))) {
            try {
                if (method.Name.Contains("InheritedBindingContext"))
                    continue; /* Ignore this */
                var propertyType = method.ReturnType;
                var declaringType = method.DeclaringType;
                var nspace = $"{declaringType?.Namespace}.{declaringType?.Name}";
                object fieldType = propertyType.Name;

                if (propertyType.IsEnum)
                    fieldType = Enum.GetNames(propertyType);

                if (propertyType.IsValueType && !propertyType.IsPrimitive)
                    fieldType = propertyType
                        .GetFields()
                        .ToList()
                        .ConvertAll(it => it.Name);

                attributes.Add(new AttributeInfo(method.Name[3..], nspace, fieldType, isAttached: true));
            } catch {
                this.logger?.Invoke($"Error injecting {method.Name} from {type.Name}");
            }
        }

        return attributes;
    }
}