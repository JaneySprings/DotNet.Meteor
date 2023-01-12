namespace DotNet.Meteor.Shared {
    public interface IProcessLogger {
        void OnOutputDataReceived(string stderr);
        void OnErrorDataReceived(string stderr);
    }

    public class ConsoleLogger: IProcessLogger {
        public void OnOutputDataReceived(string stderr) {
            System.Console.WriteLine(stderr);
        }
        public void OnErrorDataReceived(string stderr) {
            System.Console.WriteLine(stderr);
        }
    }
}