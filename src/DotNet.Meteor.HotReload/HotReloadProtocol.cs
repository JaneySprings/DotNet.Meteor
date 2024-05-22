namespace DotNet.Meteor.HotReload.Models;

public class HotReloadProtocol {
    public const string HandShakeKey = "handshake";
    public const string HandShakePartSeparator = "_";

    public bool IsConnectionSuccessful { get; private set; }
    public bool IsLegacyProtocolFormat { get; private set; }

    public void CheckServerCapabilities(string? serverResponse) {
        IsLegacyProtocolFormat = true;
        IsConnectionSuccessful = !string.IsNullOrEmpty(serverResponse) && serverResponse.StartsWith(HandShakeKey, StringComparison.OrdinalIgnoreCase);
        if (!IsConnectionSuccessful)
            return;

        var responseParts = serverResponse!.Split(HandShakePartSeparator);
        if (responseParts.Length < 2)
            return;

        IsLegacyProtocolFormat = false;
        var serverVersion = new Version(responseParts[1]);
        // FUTURE: check capabilities here
    }
}
