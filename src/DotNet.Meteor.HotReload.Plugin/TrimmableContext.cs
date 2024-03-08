using System.Text.Json.Serialization;
using DotNet.Meteor.HotReload.Plugin.Models;

namespace DotNet.Meteor.HotReload.Plugin;

[JsonSerializable(typeof(TransferObject))]
internal partial class TrimmableContext : JsonSerializerContext {}