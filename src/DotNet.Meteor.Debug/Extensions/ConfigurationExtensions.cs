using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using NewtonConverter = Newtonsoft.Json.JsonConvert;

namespace DotNet.Meteor.Debug;

public static class ConfigurationExtensions {
    public static T ToObject<T>(this JToken jtoken, JsonTypeInfo<T> type) {
        string json = NewtonConverter.SerializeObject(jtoken);
        return JsonSerializer.Deserialize(json, type);
    }
}