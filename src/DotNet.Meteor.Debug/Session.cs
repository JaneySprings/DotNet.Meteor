using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Debug.Protocol;
using DotNet.Meteor.Debug.Protocol.Events;
using DotNet.Meteor.Debug.Utilities;
using System.Text.Json;
using NLog;

namespace DotNet.Meteor.Debug;

public abstract class Session: IProcessLogger {
    protected readonly Logger sessionLogger = LogManager.GetCurrentClassLogger();
    private bool stopGlobalLoop;
    private Stream outputStream;


#region Event pump
    public async Task Start(Stream inputStream, Stream outputStream, int messageBufferSize = 1024) {
        this.outputStream = outputStream;
        var stringBuilder = new StringBuilder();

        while (!this.stopGlobalLoop) {
            int readedBytes;
            do {
                var buffer = new byte[messageBufferSize];
                readedBytes = await inputStream.ReadAsync(buffer);
                stringBuilder.Append(Encoding.UTF8.GetString(buffer));
            } while (readedBytes == messageBufferSize);

            if (stringBuilder.Length == 0)
                break;

            var readed = stringBuilder.ToString();
            stringBuilder.Clear();

            foreach(Match match in Regex.Matches(readed, @"{(.*)}", RegexOptions.Multiline)) {
                var request = JsonSerializer.Deserialize<Request>(match.Value)!;
                var response = new Response(request);
                this.sessionLogger.Debug($"DAP Request: {match.Value}");

                try {
                    DispatchRequest(request.Command, request.Arguments, response);
                } catch (Exception e) {
                    var message = $"Error occurred while processing {request.Command} request. " + e.Message;
                    response.SetError(message, new ErrorResponseBody(e));
                    this.sessionLogger.Error(e, message);
                }

                SendMessage(response);
            }
        }
        this.sessionLogger.Debug("Debugger session terminated.");
    }

    protected void StopGlobalLoop() {
        this.stopGlobalLoop = true;
    }

    protected void SendMessage(ProtocolMessage message) {
        var data = message.ConvertToBytes();
        this.sessionLogger.Debug($"DAP Response: {JsonSerializer.Serialize((object)message)}");
        this.outputStream.Write(data, 0, data.Length);
        this.outputStream.Flush();
    }

    protected void SendConsoleEvent(string category, string message) {
        SendMessage(new OutputEvent(category, message.Trim() + Environment.NewLine));
    }

    private void DispatchRequest(string command, Arguments args, Response response) {
        switch(command) {
            case "initialize": Initialize(response, args); break;
            case "launch": Launch(response, args); break;
            case "attach": Attach(response, args); break;
            case "next": Next(response, args); break;
            case "continue": Continue(response, args); break;
            case "stepIn": StepIn(response, args); break;
            case "stepOut": StepOut(response, args); break;
            case "pause": Pause(response, args); break;
            case "stackTrace": StackTrace(response, args); break;
            case "scopes": Scopes(response, args); break;
            case "variables": Variables(response, args); break;
            case "source": Source(response, args); break;
            case "threads": Threads(response, args); break;
            case "setBreakpoints": SetBreakpoints(response, args); break;
            case "setExceptionBreakpoints": SetExceptionBreakpoints(response, args); break;
            case "evaluate": Evaluate(response, args); break;
            case "disconnect": Disconnect(response, args); break;
            default: this.sessionLogger.Error($"unrecognized request '{command}'"); break;
        }
    }

#endregion

    protected abstract void Initialize(Response response, Arguments args);
    protected abstract void Launch(Response response, Arguments arguments);
    protected abstract void Attach(Response response, Arguments arguments);
    protected abstract void Disconnect(Response response, Arguments arguments);
    protected abstract void SetExceptionBreakpoints(Response response, Arguments arguments);
    protected abstract void SetBreakpoints(Response response, Arguments arguments);
    protected abstract void Continue(Response response, Arguments arguments);
    protected abstract void Next(Response response, Arguments arguments);
    protected abstract void StepIn(Response response, Arguments arguments);
    protected abstract void StepOut(Response response, Arguments arguments);
    protected abstract void Pause(Response response, Arguments arguments);
    protected abstract void StackTrace(Response response, Arguments arguments);
    protected abstract void Scopes(Response response, Arguments arguments);
    protected abstract void Variables(Response response, Arguments arguments);
    protected abstract void Source(Response response, Arguments arguments);
    protected abstract void Threads(Response response, Arguments arguments);
    protected abstract void Evaluate(Response response, Arguments arguments);

    public void OnOutputDataReceived(string stdout) {
        SendConsoleEvent("stdout", stdout);
    }
    public void OnErrorDataReceived(string stderr) {
        SendConsoleEvent("stderr", stderr);
    }
}