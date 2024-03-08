
namespace DotNet.Meteor.HotReload.Models;

public class HotReloadProtocol {
    public const string HandShakeKey = "handshake";
    public const string HandShakePartSeparator = "_";
    
    private bool isConnectionSuccessful;
    public bool IsConnectionSuccessful => isConnectionSuccessful;

    private bool isLegacyProtocolFormat;
    public bool IsLegacyProtocolFormat => isLegacyProtocolFormat;
    

    public void CheckServerCapabilities(string? serverResponse) {
        isLegacyProtocolFormat = true;
        isConnectionSuccessful = !string.IsNullOrEmpty(serverResponse) && serverResponse.StartsWith(HandShakeKey);
        if (!IsConnectionSuccessful)
            return;

        var responseParts = serverResponse!.Split(HandShakePartSeparator);
        if (responseParts.Length < 2)
            return;

        isLegacyProtocolFormat = false;
        var serverVersion = new Version(responseParts[1]);
        // FUTURE: check capabilities here
    }
}
