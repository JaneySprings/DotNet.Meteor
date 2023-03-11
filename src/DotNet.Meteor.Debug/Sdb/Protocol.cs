using System.IO;

namespace DotNet.Meteor.Debug.Sdb;

public static class Protocol {
    public static void WriteCommand(Stream stream, string command) {
        byte[] commandBytes = new byte[command.Length + 1];
        commandBytes[0] = (byte)command.Length;
        for (int i = 0; i < command.Length; i++) {
            commandBytes[i + 1] = (byte)command[i];
        }
        stream.Write(commandBytes, 0, commandBytes.Length);
    }
}