using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Debug.Events;
using DotNet.Meteor.Debug.Protocol;
using System.Text.Json;
using NLog;

namespace DotNet.Meteor.Debug.Session;

public abstract class Session: IProcessLogger {
    protected readonly Logger sessionLogger = LogManager.GetCurrentClassLogger();
    private readonly ByteBuffer rawData = new ByteBuffer();
    private Stream outputStream = null!;
    private const int InputBufferSize = 4096;
    private int sequenceNumber = 1;
    private int bodyLength = -1;
    private bool stopRequested;


    public async Task Start(Stream inputStream, Stream outputStream) {
        this.outputStream = outputStream;
        this.stopRequested = false;

        byte[] buffer = new byte[InputBufferSize];

        while (!this.stopRequested) {
            var read = await inputStream.ReadAsync(buffer);

            if (read > 0) {
                this.rawData.Append(buffer, read);
                ProcessData();
            }
            if (read == 0)
                break;
        }
        this.sessionLogger.Debug("Debugger session terminated.");
    }

    public void Stop() {
        this.stopRequested = true;
    }

    protected void SendEvent(string type, object body) {
        SendMessage(new Event(type, body));
    }

    protected void SendResponse(Response response, object body = null) {
        response.SetBody(body);
        SendMessage(response);
    }

    protected void SendErrorResponse(Response response, int id, string message) {
        var model = new ModelMessage(id, message);
        response.SetBodyError(message, new BodyError(model));
        SendMessage(response);
    }

    private void SendMessage(ProtocolBase message) {
        if (message.Seq == 0)
            message.Seq = this.sequenceNumber++;

        var data = message.ConvertToBytes();

        this.sessionLogger.Debug($"DAP Response: {JsonSerializer.Serialize((object)message)}");
        this.outputStream.Write(data, 0, data.Length);
        this.outputStream.Flush();
    }

    private void Dispatch(string req) {
        this.sessionLogger.Debug($"DAP Request: {req}");
        var request = JsonSerializer.Deserialize<Request>(req)!;
        var response = new Response(request);
        DispatchRequest(request.Command, request.Arguments, response);
        SendMessage(response);
    }

    protected abstract void DispatchRequest(string command, Argument args, Response response);

    private void ProcessData() {
        while (true) {
            if (this.bodyLength >= 0) {
                if (this.rawData.Length >= this.bodyLength) {
                    var buf = this.rawData.RemoveFirst(this.bodyLength);
                    this.bodyLength = -1;

                    Dispatch(Encoding.UTF8.GetString(buf));

                    continue;   // there may be more complete messages to process
                }
            } else {
                var s = this.rawData.GetString(Encoding.UTF8);
                var regex = new Regex(@"Content-Length: (\d+)");
                var header = "\r\n\r\n";
                var idx = s.IndexOf(header);
                if (idx != -1) {
                    Match m = regex.Match(s);
                    if (m.Success && m.Groups.Count == 2) {
                        this.bodyLength = Convert.ToInt32(m.Groups[1].ToString());
                        this.rawData.RemoveFirst(idx + header.Length);

                        continue;   // try to handle a complete message
                    }
                }
            }
            break;
        }
    }

    public void OnOutputDataReceived(string stdout) {
        SendEvent(Event.OutputEvent, new BodyOutput("stdout", stdout.Trim() + Environment.NewLine));
    }

    public void OnErrorDataReceived(string stderr) {
        SendEvent(Event.OutputEvent, new BodyOutput("stderr", stderr.Trim() + Environment.NewLine));
    }
}