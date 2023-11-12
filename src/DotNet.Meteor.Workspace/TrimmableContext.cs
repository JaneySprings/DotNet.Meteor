using System.Text.Json.Serialization;
using DotNet.Meteor.Shared;

namespace DotNet.Meteor.Workspace;

[JsonSerializable(typeof(List<DeviceData>))]
[JsonSerializable(typeof(List<Project>))]
[JsonSerializable(typeof(bool))]
public partial class TrimmableContext : JsonSerializerContext {}