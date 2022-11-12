namespace DotNet.Mobile.Shared {
    public static class Logger {
        public static void Info(string message) {
            System.Console.WriteLine(message);
        }

        public static void Warning(string message) {
            //TODO: colorize?
            Info(message);
        }

        public static void Error(string message) {
            System.Console.WriteLine(message);
            throw new System.Exception(message);
        }
    }
}