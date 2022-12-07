namespace DotNet.Mobile.Shared {
    public interface IProcessLogger {
        void OnOutputDataReceived(string stderr);
        void OnErrorDataReceived(string stderr);
    }
}