using System;
using System.Collections.Generic;
using System.Text.Json;
using XCode.Sdk;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.CLI {
    public static class AppleCommand {
        public static void AppleDevicesAsJson(string[] args) {
            List<DeviceData> devices = AppleDevices();
            Console.WriteLine(JsonSerializer.Serialize(devices));
        }

        public static List<DeviceData> AppleDevices() {
            if (!RuntimeSystem.IsMacOS)
                return new List<DeviceData>();
            return XCodeTool.AllDevices();
        }
    }
}
