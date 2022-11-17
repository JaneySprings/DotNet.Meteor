using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DotNet.Mobile.Shared;

namespace XCode.Sdk {
    public static class XCodeTool {
        public static List<DeviceData> GetAllDevices() {
            FileInfo tool = PathUtils.GetXCDeviceTool();
            ProcessResult result = ProcessRunner.Run(tool, new ProcessArgumentBuilder().Append("list"));

            string json = string.Join(Environment.NewLine, result.StandardOutput);
            List<Device> devices = JsonSerializer.Deserialize<List<Device>>(json);

            return devices
                .Where(d => d.Error == null)
                .Select(d => new DeviceData {
                    IsEmulator = d.Simulator,
                    IsRunning = false,
                    Name = d.Name,
                    Details = d.ModelName + " (" + d.Architecture + ")",
                    Platform = d.GetPlatformType(),
                    Serial = d.Identifier
            }).ToList();
        }
    }
}