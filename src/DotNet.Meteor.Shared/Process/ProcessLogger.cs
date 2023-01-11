namespace DotNet.Meteor.Shared {
    public interface IProcessLogger {
        void OnOutputDataReceived(string stderr);
        void OnErrorDataReceived(string stderr);
    }
}