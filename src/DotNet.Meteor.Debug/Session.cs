using System;
using System.IO;
using DotNet.Meteor.Processes;
using Mono.Debugging.Client;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace DotNet.Meteor.Debug;

public abstract class Session: DebugAdapterBase, IProcessLogger {

    protected Session(Stream input, Stream output) {
        base.InitializeProtocolClient(input, output);
    }

    protected abstract ICustomLogger GetLogger();

    public void Start() {
        Protocol.LogMessage += LogMessage;
        Protocol.Run();
    }
    protected void Stop() {
        Protocol.LogMessage -= LogMessage;
        Protocol.Stop();
        GetLogger().LogMessage("Debugger session terminated.");
    }

    protected void SendConsoleEvent(OutputEvent.CategoryValue category, string message) {
        Protocol.SendEvent(new OutputEvent(message.Trim() + Environment.NewLine) {
            Category = category
        });
    }
    public void OnOutputDataReceived(string stdout) {
        SendConsoleEvent(OutputEvent.CategoryValue.Stdout, stdout);
    }
    public void OnErrorDataReceived(string stderr) {
        SendConsoleEvent(OutputEvent.CategoryValue.Stderr, stderr);
    }

    private void LogMessage(object sender, LogEventArgs args) {
        GetLogger().LogMessage(args.Message);
    }
}