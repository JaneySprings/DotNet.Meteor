using System.Reflection;
using System.Windows.Markup;

namespace DotNet.Meteor.Xaml;

public class Reflector {
    private readonly Action<string>? logger;

    public Reflector(Action<string>? logger = null) {
        this.logger = logger;
    }

    public SchemaInfo ParseAssembly(Assembly assembly) {
        var elements = new List<TypeInfo>();
        string xmlNamespace = $"assembly={assembly.GetName().Name}";

        foreach (var type in assembly.GetTypes()) {
            if (!type.IsSubclassOf(MauiTypeLoader.VisualElement!) || type.IsAbstract)
                continue;
            elements.Add(new TypeInfo {
                Name = type.Name,
                Namespace = $"{type.Namespace}.{type.Name}",
                Attributes = GetAttributes(type)
            });
        }

        var xmlnsAttribute = assembly.GetCustomAttributes()
            .FirstOrDefault(it => it.GetType() == MauiTypeLoader.XmlnsDefinitionAttribute!);

        if (xmlnsAttribute != null && MauiTypeLoader.XmlnsDefinitionAttribute!.GetProperty("XmlNamespace")?.GetValue(xmlnsAttribute) is string xmlns)
            xmlNamespace = xmlns;

        return new SchemaInfo {
            Xmlns = xmlNamespace,
            Types = elements
        };
    }

    private List<AttributeInfo> GetAttributes(Type type) {
        var properties = new List<AttributeInfo>();

        properties.AddRange(GetEventHandlers(type));

        // Bindable properties
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)) {
            try {
                var fieldValue = field.GetValue(null);
                if (fieldValue?.GetType() != MauiTypeLoader.BindableProperty!)
                    continue;

                var bindablePropertyReturnType = MauiTypeLoader.BindableProperty!.GetProperty("ReturnType")?.GetValue(fieldValue) as Type;
                var bindablePropertyName = MauiTypeLoader.BindableProperty!.GetProperty("PropertyName")?.GetValue(fieldValue) as string;
                if (bindablePropertyReturnType == null)
                    continue;

                object fieldType = $"{bindablePropertyReturnType.Namespace}.{bindablePropertyReturnType.Name}";
                if (bindablePropertyReturnType.IsEnum) {
                    fieldType = Enum.GetNames(bindablePropertyReturnType);
                } else if (bindablePropertyReturnType.IsValueType && !bindablePropertyReturnType.IsPrimitive) {
                    fieldType = bindablePropertyReturnType
                        .GetFields()
                        .ToList()
                        .ConvertAll(it => it.Name);
                }

                properties.Add(new AttributeInfo {
                    Name = bindablePropertyName,
                    Type = fieldType,
                    Namespace = $"{type.Namespace}.{type.Name}.{bindablePropertyName}",
                });
            } catch {
                this.logger?.Invoke($"Error parsing {field.Name} in {type.Name}");
            }
        }

        return properties;
    }
    private static List<AttributeInfo> GetEventHandlers(Type type) {
        var events = new List<AttributeInfo>();

        foreach (var ev in type.GetEvents()) {
            events.Add(new AttributeInfo {
                Name = ev.Name,
                Type = $"{ev.EventHandlerType?.Namespace}.{ev.EventHandlerType?.Name}",
                Namespace = $"{type.Namespace}.{type.Name}",
            });
        }
        return events;
    }
}