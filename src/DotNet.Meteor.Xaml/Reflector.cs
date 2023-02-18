using System.Reflection;

namespace DotNet.Meteor.Xaml;

public class Reflector {
    private readonly Action<string>? logger;

    public Reflector(Action<string>? logger = null) {
        this.logger = logger;
    }

    public SchemaInfo CreateAlias(Assembly assembly) {
        var elements = new List<TypeInfo>();
        string xmlNamespace = $"assembly={assembly.GetName().Name}";

        foreach (var type in assembly.GetTypes())
            if (type.IsSubclassOf(MauiTypeLoader.VisualElement!) && !type.IsAbstract)
                elements.Add(new TypeInfo(type.Name, type.Namespace, GetAttributes(type)));

        var xmlnsAttribute = assembly.GetCustomAttributes().FirstOrDefault(it => it.GetType() == MauiTypeLoader.XmlnsDefinitionAttribute!);
        if (xmlnsAttribute != null && MauiTypeLoader.XmlnsDefinitionAttribute!.GetProperty("XmlNamespace")?.GetValue(xmlnsAttribute) is string xmlns)
            xmlNamespace = xmlns;

        return new SchemaInfo(xmlNamespace, elements);
    }

    private List<AttributeInfo> GetAttributes(Type type) {
        var properties = new List<AttributeInfo>();

        // Event handlers
        foreach (var ev in type.GetEvents()) {
            var declaringType = ev.DeclaringType;
            var nspace = $"{declaringType?.Namespace}.{declaringType?.Name}";
            properties.Add(new AttributeInfo(
                ev.Name, nspace, ev.EventHandlerType?.Name
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

                properties.Add(new AttributeInfo(property.Name, nspace, fieldType));
            } catch {
                this.logger?.Invoke($"Error injecting {property.Name} from {type.Name}");
            }
        }

        return properties;
    }
}