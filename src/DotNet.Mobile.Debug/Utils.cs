using System.Reflection;
using System.Text.RegularExpressions;

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
}