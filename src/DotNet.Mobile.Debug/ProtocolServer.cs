/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNet.Mobile.Shared;
using DotNet.Mobile.Debug.Events;
using DotNet.Mobile.Debug.Protocol;
using Newtonsoft.Json;

namespace DotNet.Mobile.Debug {
    public abstract class ProtocolServer {
        public bool TRACE;
        public bool TRACE_RESPONSE;

        protected const int BUFFER_SIZE = 4096;
        protected const string TWO_CRLF = "\r\n\r\n";
        protected static readonly Regex CONTENT_LENGTH_MATCHER = new Regex(@"Content-Length: (\d+)");

        protected static readonly Encoding Encoding = System.Text.Encoding.UTF8;

        private int _sequenceNumber;
        private Dictionary<int, TaskCompletionSource<Response>> _pendingRequests;

        private Stream _outputStream;

        private ByteBuffer _rawData;
        private int _bodyLength;

        private bool _stopRequested;


        public ProtocolServer() {
            this._sequenceNumber = 1;
            this._bodyLength = -1;
            this._rawData = new ByteBuffer();
            this._pendingRequests = new Dictionary<int, TaskCompletionSource<Response>>();
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

        public void SendEvent(Event e) {
            SendMessage(e);
        }

        public Task<Response> SendRequest(string command, dynamic args) {
            var tcs = new TaskCompletionSource<Response>();

            Request request = null;
            lock (this._pendingRequests) {
                request = new Request(this._sequenceNumber++, command, args);
                // wait for response
                this._pendingRequests.Add(request.seq, tcs);
            }

            SendMessage(request);

            return tcs.Task;
        }

        protected abstract void DispatchRequest(string command, dynamic args, Response response);

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
            var message = JsonConvert.DeserializeObject<ProtocolMessage>(req);
            if (message != null) {
                switch (message.type) {
                    case "request": {
                        var request = JsonConvert.DeserializeObject<Request>(req);

                        if (this.TRACE)
                            Logger.Log($"IDE_Request[request.command]: {req}");

                        var response = new Response(request);
                        DispatchRequest(request.command, request.arguments, response);
                        SendMessage(response);
                    }
                    break;

                    case "response": {
                        var response = JsonConvert.DeserializeObject<Response>(req);
                        int seq = response.request_seq;
                        lock (this._pendingRequests) {
                            if (this._pendingRequests.ContainsKey(seq)) {
                                var tcs = this._pendingRequests[seq];
                                this._pendingRequests.Remove(seq);
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
                message.seq = this._sequenceNumber++;
            }

            if (this.TRACE_RESPONSE && message.type == "response")
                Logger.Log($"Debugger_Response: {JsonConvert.SerializeObject(message, Formatting.Indented)}");

            if (this.TRACE && message.type == "event" && message is Event e) {
                Logger.Log("Debugger_Event[{0}]: {1}", ((Event)message).eventType, JsonConvert.SerializeObject(e.body, Formatting.Indented));
            }

            var data = ConvertToBytes(message);
            try {
                this._outputStream.Write(data, 0, data.Length);
                this._outputStream.Flush();
            } catch (Exception ex) {
                Logger.Log(ex);
            }
        }

        private static byte[] ConvertToBytes(ProtocolMessage request) {
            var asJson = JsonConvert.SerializeObject(request);
            byte[] jsonBytes = Encoding.GetBytes(asJson);

            string header = string.Format("Content-Length: {0}{1}", jsonBytes.Length, TWO_CRLF);
            byte[] headerBytes = Encoding.GetBytes(header);

            byte[] data = new byte[headerBytes.Length + jsonBytes.Length];
            System.Buffer.BlockCopy(headerBytes, 0, data, 0, headerBytes.Length);
            System.Buffer.BlockCopy(jsonBytes, 0, data, headerBytes.Length, jsonBytes.Length);

            return data;
        }
    }

    //--------------------------------------------------------------------------------------

    class ByteBuffer {
        private byte[] _buffer;

        public ByteBuffer() {
            this._buffer = new byte[0];
        }

        public int Length {
            get { return this._buffer.Length; }
        }

        public string GetString(Encoding enc) {
            return enc.GetString(this._buffer);
        }

        public void Append(byte[] b, int length) {
            byte[] newBuffer = new byte[this._buffer.Length + length];
            System.Buffer.BlockCopy(this._buffer, 0, newBuffer, 0, this._buffer.Length);
            System.Buffer.BlockCopy(b, 0, newBuffer, this._buffer.Length, length);
            this._buffer = newBuffer;
        }

        public byte[] RemoveFirst(int n) {
            byte[] b = new byte[n];
            System.Buffer.BlockCopy(this._buffer, 0, b, 0, n);
            byte[] newBuffer = new byte[this._buffer.Length - n];
            System.Buffer.BlockCopy(this._buffer, n, newBuffer, 0, this._buffer.Length - n);
            this._buffer = newBuffer;
            return b;
        }
    }
}