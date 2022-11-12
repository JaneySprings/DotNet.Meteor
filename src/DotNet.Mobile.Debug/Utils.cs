using System.Reflection;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug;

public static class Utils {
    public static string ExpandVariables(string format, dynamic variables, bool underscoredOnly = true) {
        if (string.IsNullOrWhiteSpace(format))
            return format;

        variables ??= new { };

        var type = variables.GetType();
        var variableRegex = new Regex(@"\{(\w+)\}");

        return variableRegex.Replace(format, match => {
            string name = match.Groups[1].Value;
            if (!underscoredOnly || name.StartsWith("_")) {

                PropertyInfo property = type.GetProperty(name);
                if (property != null) {
                    object value = property.GetValue(variables, null);
                    return value.ToString();
                }
                return '{' + name + ": not found}";
            }
            return match.Groups[0].Value;
        });
    }

    public static IPAddress ResolveIPAddress(string addressString) {
        try {
            if (IPAddress.TryParse(addressString, out IPAddress ipaddress))
                return ipaddress;

            IPHostEntry entry = Dns.GetHostEntry(addressString);

            if (entry?.AddressList?.Length > 0) {
                if (entry.AddressList.Length == 1) {
                    return entry.AddressList[0];
                }
                foreach (IPAddress address in entry.AddressList) {
                    if (address.AddressFamily == AddressFamily.InterNetwork) {
                        return address;
                    }
                }
            }
        } catch (Exception e) { Logger.Info(e); }

        return null;
    }

    public static bool GetBoolean(dynamic container, string propertyName, bool dflt = false) {
        try {
            return (bool)container[propertyName];
        } catch (Exception e) {
            Logger.Warning(e.Message);
        }
        return dflt;
    }

    public static int GetInteger(dynamic container, string propertyName, int dflt = 0) {
        try {
            return (int)container[propertyName];
        } catch (Exception e) {
            Logger.Warning(e.Message);
        }
        return dflt;
    }

    public static string GetString(dynamic args, string property, string dflt = null) {
        var s = (string)args[property];
        if (s == null) {
            return dflt;
        }
        s = s.Trim();
        if (s.Length == 0) {
            return dflt;
        }
        return s;
    }
}