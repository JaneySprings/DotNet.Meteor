using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNet.Mobile.Shared;
using DotNet.Mobile.Debug.Events;
using DotNet.Mobile.Debug.Protocol;
using System.Text.Json;

namespace DotNet.Mobile.Debug {
    public abstract class Session {
        public bool TRACE;
        public bool TRACE_RESPONSE;

        protected const int BUFFER_SIZE = 4096;
        protected const string TWO_CRLF = "\r\n\r\n";
        protected static readonly Regex CONTENT_LENGTH_MATCHER = new Regex(@"Content-Length: (\d+)");

        protected static readonly Encoding Encoding = Encoding.UTF8;

        private int _sequenceNumber;

        private Stream _outputStream;

        private ByteBuffer _rawData;
        private int _bodyLength;

        private bool _stopRequested;


        protected Session() {
            this._sequenceNumber = 1;
            this._bodyLength = -1;
            this._rawData = new ByteBuffer();
        }

        public async Task Start(Stream inputStream, Stream outputStream) {
            this._outputStream = outputStream;

            byte[] buffer = new byte[BUFFER_SIZE];

            this._stopRequested = false;
            while (!this._stopRequested) {
                var read = await inputStream.ReadAsync(buffer, 0, buffer.Length);

                if (read == 0) {
                    // end of stream
                    break;
                }

                if (read > 0) {
                    this._rawData.Append(buffer, read);
                    ProcessData();
                }
            }
        }

        public void Stop() {
            this._stopRequested = true;
        }

        public void SendEvent(string type, object body) {
            SendMessage(new Event(type, body));
        }

        protected abstract void DispatchRequest(string command, Argument args, Response response);

        // ---- private ------------------------------------------------------------------------

        private void ProcessData() {
            while (true) {
                if (this._bodyLength >= 0) {
                    if (this._rawData.Length >= this._bodyLength) {
                        var buf = this._rawData.RemoveFirst(this._bodyLength);

                        this._bodyLength = -1;

                        Dispatch(Encoding.GetString(buf));

                        continue;   // there may be more complete messages to process
                    }
                } else {
                    string s = this._rawData.GetString(Encoding);
                    var idx = s.IndexOf(TWO_CRLF);
                    if (idx != -1) {
                        Match m = CONTENT_LENGTH_MATCHER.Match(s);
                        if (m.Success && m.Groups.Count == 2) {
                            this._bodyLength = Convert.ToInt32(m.Groups[1].ToString());

                            this._rawData.RemoveFirst(idx + TWO_CRLF.Length);

                            continue;   // try to handle a complete message
                        }
                    }
                }
                break;
            }
        }

        private void Dispatch(string req) {
            var request = JsonSerializer.Deserialize<Request>(req);

            switch (request.Type) {
                case "request":
                    if (this.TRACE)
                        Logger.Log($"IDE_Request[request.command]: {req}");

                    var response = new Response(request);
                    DispatchRequest(request.Command, request.Arguments, response);
                    SendMessage(response);
                break;

                case "response": {
                    Logger.Log("PSLine[137] Strange response from IDE: " + req);
                    // var response = JsonConvert.DeserializeObject<Response>(req);
                    // int seq = response.request_seq;
                    // lock (this._pendingRequests) {
                    //     if (this._pendingRequests.ContainsKey(seq)) {
                    //         var tcs = this._pendingRequests[seq];
                    //         this._pendingRequests.Remove(seq);
                    //         tcs.SetResult(response);
                    //     }
                    // }
                }
                break;
            }
        }

        protected void SendMessage(ProtocolBase message) {
            if (message.Seq == 0)
                message.Seq = this._sequenceNumber++;

            if (this.TRACE_RESPONSE)
                Logger.Log($"Debugger_Response: {JsonSerializer.Serialize((object)message)}");

            var data = message.ConvertToBytes();
            this._outputStream.Write(data, 0, data.Length);
            this._outputStream.Flush();
        }
    }
}