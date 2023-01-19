namespace DotNet.Meteor.Processes {
    public interface IProcessLogger {
        void OnOutputDataReceived(string stdout);
        void OnErrorDataReceived(string stderr);
    }

    public class ConsoleLogger: IProcessLogger {
        public void OnOutputDataReceived(string stdout) {
            System.Console.WriteLine(stdout);
        }
        public void OnErrorDataReceived(string stderr) {
            System.Console.WriteLine(stderr);
        }
    }
}