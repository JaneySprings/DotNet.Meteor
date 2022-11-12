using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.Session;

public abstract class ProtocolServer {
    public bool Trace { get; set; }
    public bool TraceResponse { get; set; }

    protected const int BufferSize = 4096;
    protected const string ContentLengthRegex = @"Content-Length: (\d+)";
    protected static readonly Encoding Encoding = Encoding.UTF8;

    private const string CrlfSequence = "\r\n\r\n";
    private Dictionary<int, TaskCompletionSource<Response>> pendingRequests = new();
    private Stream outputStream;
    private ByteBuffer rawData = new ByteBuffer();
    private int sequenceNumber = 1;
    private int bodyLength = -1;
    private bool stopRequested;

    // ---- visible ------------------------------------------------------------------------

    public async Task Start(Stream inputStream, Stream outputStream) {
        this.outputStream = outputStream;

        byte[] buffer = new byte[BufferSize];

        this.stopRequested = false;
        while (!this.stopRequested) {
            var read = await inputStream.ReadAsync(buffer);

            if (read == 0) {
                // end of stream
                break;
            }

            if (read > 0) {
                this.rawData.Append(buffer, read);
                ProcessData();
            }
        }
    }

    public void Stop() => this.stopRequested = true;
    public void SendEvent(Event e) => SendMessage(e);

    public Task<Response> SendRequest(string command, dynamic args) {
        var tcs = new TaskCompletionSource<Response>();

        Request request = null;
        lock (this.pendingRequests) {
            request = new Request(this.sequenceNumber++, command, args);
            // wait for response
            this.pendingRequests.Add(request.seq, tcs);
        }

        SendMessage(request);

        return tcs.Task;
    }

    protected abstract void DispatchRequest(string command, dynamic args, Response response);

    // ---- private ------------------------------------------------------------------------

    private void ProcessData() {
        while (true) {
            if (this.bodyLength >= 0) {
                if (this.rawData.Length >= this.bodyLength) {
                    var buf = this.rawData.RemoveFirst(this.bodyLength);

                    this.bodyLength = -1;
                    Dispatch(Encoding.GetString(buf));

                    continue;   // there may be more complete messages to process
                }
            } else {
                string s = this.rawData.GetString(Encoding);
                var idx = s.IndexOf(CrlfSequence);
                if (idx != -1) {
                    Match m = new Regex(ContentLengthRegex).Match(s);
                    if (m.Success && m.Groups.Count == 2) {
                        this.bodyLength = Convert.ToInt32(m.Groups[1].ToString());
                        this.rawData.RemoveFirst(idx + CrlfSequence.Length);

                        continue;   // try to handle a complete message
                    }
                }
            }
            break;
        }
    }

    private void Dispatch(string req) {
        var message = JsonSerializer.Deserialize<ProtocolMessage>(req);
        if (message != null) {
            switch (message.type) {
                case "request": {
                    var request = JsonSerializer.Deserialize<Request>(req);

                    if (this.Trace)
                        Logger.Info($"C {request.command}: {JsonSerializer.Serialize(request.arguments)}");

                    var response = new Response(request);
                    DispatchRequest(request.command, request.arguments, response);
                    SendMessage(response);
                }
                break;
                case "response": {
                    var response = JsonSerializer.Deserialize<Response>(req);
                    int seq = response.request_seq;
                    lock (this.pendingRequests) {
                        if (this.pendingRequests.ContainsKey(seq)) {
                            var tcs = this.pendingRequests[seq];
                            this.pendingRequests.Remove(seq);
                            tcs.SetResult(response);
                        }
                    }
                }
                break;
            }
        }
    }

    protected void SendMessage(ProtocolMessage message) {
        if (message.seq == 0) {
            message.seq = this.sequenceNumber++;
        }

        if (this.TraceResponse && message.type == "response")
            Logger.Info($"R: {JsonSerializer.Serialize(message)}");

        if (this.Trace && message.type == "event" && message is Event e) {
            Logger.Info($"E {e.EventType}: {JsonSerializer.Serialize(e.Body)}");
        }

        var data = ConvertToBytes(message);
        try {
            this.outputStream.Write(data, 0, data.Length);
            this.outputStream.Flush();
        } catch (Exception ex) {
            Logger.Warning(ex.Message + "\n" + ex.StackTrace);
        }
    }

    private static byte[] ConvertToBytes(ProtocolMessage request) {
        var asJson = JsonSerializer.Serialize(request);
        byte[] jsonBytes = Encoding.GetBytes(asJson);

        string header = string.Format("Content-Length: {0}{1}", jsonBytes.Length, CrlfSequence);
        byte[] headerBytes = Encoding.GetBytes(header);

        byte[] data = new byte[headerBytes.Length + jsonBytes.Length];
        Buffer.BlockCopy(headerBytes, 0, data, 0, headerBytes.Length);
        Buffer.BlockCopy(jsonBytes, 0, data, headerBytes.Length, jsonBytes.Length);

        return data;
    }
}