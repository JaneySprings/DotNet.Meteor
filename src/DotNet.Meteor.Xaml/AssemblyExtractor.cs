using System.Reflection;
using System.Linq;

namespace DotNet.Meteor.Xaml;

public class AssemblyExtractor {
    public List<TypeInfo> ProcessAssembly(string path) {
        if (!File.Exists(path))
            throw new FileNotFoundException(path);

        var assembly = Assembly.LoadFrom(path);
        var elements = new List<TypeInfo>();

        foreach (var type in assembly.GetTypes()) {
            if (!type.IsSubclassOf(typeof(VisualElement)) || type.IsAbstract)
                continue;

            elements.Add(GetTypeInfo(type));
        }
        return elements;
    }


    private TypeInfo GetTypeInfo(Type type) {
        return new TypeInfo {
            Name = type.Name,
            Namespace = $"{type.Namespace}.{type.Name}",
            Properties = GetProperties(type)
        };
    }

    private List<PropertyInfo> GetProperties(Type type) {
        var properties = new List<PropertyInfo>();

        // Event handlers
        foreach (var ev in type.GetEvents()) {
            properties.Add(new PropertyInfo {
                Name = ev.Name,
                Type = $"{ev.EventHandlerType.Namespace}.{ev.EventHandlerType.Name}",
                Namespace = $"{type.Namespace}.{type.Name}",
            });
        }
        // Bindable properties
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)) {
            if (field.FieldType != typeof(BindableProperty))
                continue;

            if (field.GetValue(null) is not BindableProperty bindableProperty)
                continue;

            object fieldType = $"{bindableProperty.ReturnType.Namespace}.{bindableProperty.ReturnType.Name}";
            if (bindableProperty.ReturnType.IsEnum) {
                fieldType = Enum.GetNames(bindableProperty.ReturnType);
            } else if (bindableProperty.ReturnType.IsValueType && !bindableProperty.ReturnType.IsPrimitive) {
                fieldType = bindableProperty.ReturnType
                    .GetFields()
                    .ToList()
                    .ConvertAll(it => it.Name);
            }

            properties.Add(new PropertyInfo {
                Name = bindableProperty.PropertyName,
                Type = fieldType,
                Namespace = $"{type.Namespace}.{type.Name}.{bindableProperty.PropertyName}",
            });
        }

        return properties;
    }
}