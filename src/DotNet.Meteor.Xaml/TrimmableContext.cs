using System.Text.Json.Serialization;

namespace DotNet.Meteor.Xaml;

[JsonSerializable(typeof(SchemaInfo))]
internal partial class TrimmableContext : JsonSerializerContext {}