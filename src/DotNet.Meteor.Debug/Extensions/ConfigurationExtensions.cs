using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using NewtonConverter = Newtonsoft.Json.JsonConvert;

namespace DotNet.Meteor.Debug.Extensions;

public static class ConfigurationExtensions {
    public static T ToObject<T>(this JToken jtoken, JsonTypeInfo<T> type) {
        if (jtoken == null)
            return default;

        string json = NewtonConverter.SerializeObject(jtoken);
        if (string.IsNullOrEmpty(json))
            return default;

        return JsonSerializer.Deserialize(json, type);
    }
}