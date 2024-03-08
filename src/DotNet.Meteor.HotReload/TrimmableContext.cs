using System.Text.Json.Serialization;
using DotNet.Meteor.HotReload.Models;

namespace DotNet.Meteor.HotReload;

[JsonSerializable(typeof(TransferObject))]
[JsonSerializable(typeof(bool))]
internal partial class TrimmableContext : JsonSerializerContext {}