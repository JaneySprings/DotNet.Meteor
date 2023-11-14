using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace DotNet.Meteor.Shared;

[JsonSerializable(typeof(List<DeviceData>))]
[JsonSerializable(typeof(List<Project>))]
[JsonSerializable(typeof(DeviceData))]
[JsonSerializable(typeof(Project))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
public partial class TrimmableContext : JsonSerializerContext {}