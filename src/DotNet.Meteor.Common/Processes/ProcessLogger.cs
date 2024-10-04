namespace DotNet.Meteor.Common.Processes;

public interface IProcessLogger {
    void OnOutputDataReceived(string stdout);
    void OnErrorDataReceived(string stderr);
}