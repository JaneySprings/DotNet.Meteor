using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DotNet.Mobile.Shared;

namespace DotNet.Mobile.Debug.Pipeline;

public abstract class StreamCommandConnection : IDisposable {
    private bool disposed;
    private TcpListener heapshotForwarder;
    private Stream reusableStream;

    public bool HasExited { get; }
    public int Port { get; protected set; }
    public virtual bool IsUsb { get { return false; } }

    private readonly object _lockObject = new();

    IAsyncResult BeginExecuteCommand(string command, bool consumeStream, AsyncCallback callback = null, object state = null) {
        var data = Encoding.UTF8.GetBytes(command);
        if (data.Length > byte.MaxValue)
            throw new ArgumentException("Command too long");

        var buffer = new byte[data.Length + 1];
        buffer[0] = (byte)data.Length;
        Array.Copy(data, 0, buffer, 1, data.Length);

        var ar = new CommandAsyncResult(this, callback, state) {
            Buffer = buffer,
            ConsumeStream = consumeStream,
        };

        //try to re-use an existing stream
        lock (this._lockObject) {
            if (this.reusableStream != null) {
                ar.Stream = this.reusableStream;
                //if we're going to consume the stream, don't leave it around for others to re-use
                if (consumeStream) {
                    this.reusableStream = null;
                }
            }
        }

        if (ar.Stream != null) {
            ExecuteCommand_BeginWriteCommand(ar);
        } else {
            BeginConnectStream(ExecuteCommand_ConnectedCommandStream, ar).AsyncWaitHandle.WaitOne(3000);
        }

        return ar;
    }

    void ExecuteCommand_ConnectedCommandStream(IAsyncResult ar) {
        var r = (CommandAsyncResult)ar.AsyncState;
        try {
            r.Stream = EndConnectStream(ar);
            ExecuteCommand_BeginWriteCommand(r);
        } catch (Exception ex) {
            r.CompleteWithError(ex);
        }
    }

    static void ExecuteCommand_DiscardStream(Stream stream) {
        var discard = new byte[] { 7, (byte)'d', (byte)'i', (byte)'s', (byte)'c', (byte)'a', (byte)'r', (byte)'d' };
        stream.BeginWrite(discard, 0, discard.Length, ar => ((Stream)ar.AsyncState).Dispose(), stream);
    }

    void ExecuteCommand_BeginWriteCommand(CommandAsyncResult r) {
        r.Stream.BeginWrite(r.Buffer, 0, r.Buffer.Length, ExecuteCommand_WroteCommand, r);
    }

    void ExecuteCommand_WroteCommand(IAsyncResult ar) {
        var r = (CommandAsyncResult)ar.AsyncState;
        try {
            r.Stream.EndWrite(ar);
            //if the stream can be re-used, keep it
            if (!r.ConsumeStream) {
                lock (this._lockObject) {
                    if (this.reusableStream == null) {
                        this.reusableStream = r.Stream;
                        r.Stream = null;
                    }
                }
                //if there was already a re-usable stream from another thread, discard this one
                if (r.Stream != null) {
                    ExecuteCommand_DiscardStream(r.Stream);
                }
            }
            r.Complete();
        } catch (Exception ex) {
            r.CompleteWithError(ex);
        }
    }

    void CancelExecuteCommand(IAsyncResult asyncResult) {
        ((CommandAsyncResult)asyncResult).Cancel();
    }

    Stream EndExecuteCommand(IAsyncResult result) {
        var r = (CommandAsyncResult)result;
        r.CheckError();
        return r.ConsumeStream ? r.Stream : null;
    }

    class CommandAsyncResult : CancellableAsyncCommand {
        private readonly StreamCommandConnection conn;
        public byte[] Buffer;
        public bool ConsumeStream;
        public Stream Stream;

        public CommandAsyncResult(StreamCommandConnection conn, AsyncCallback callback, object state)
            : base(callback, state) {
            this.conn = conn;
        }

        protected override void CancelInnerResult(IAsyncResult innerResult) {
            if (this.Stream != null) {
                this.Stream.Dispose();
            } else {
                // conn.CancelConnect (innerResult);
            }
            this.conn.CancelExecuteCommand(innerResult);
        }
    }

    protected abstract IAsyncResult BeginConnectStream(AsyncCallback callback, object state);

    protected abstract Stream EndConnectStream(IAsyncResult result);

    IAsyncResult BeginSendHeapShotRequest(AsyncCallback callback = null, object state = null) {
        return BeginExecuteCommand("heapshot", false, callback, state);
    }

    IAsyncResult BeginSendHeapShotPort(int port, AsyncCallback callback = null, object state = null) {
        return BeginExecuteCommand("set heapshot port: " + Port, false, callback, state);
    }

    void SendStartLogProfiler(string outputFile, string profilerConfiguration, bool isDevice, string heapshotMode) {
        // We must remove the target file if it already exists, the heapshot gui will
        // read the port to request heapshots on from the file and this way it won't
        // read the port from the previous run.
        if (File.Exists(outputFile)) {
            File.Delete(outputFile);
        } else {
            var outputDir = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
        }

        if (!isDevice) {
            // app sends output directly to a file
            EndExecuteCommand(BeginExecuteCommand("start profiler: " + profilerConfiguration, false));
            return;
        }

        //for device, we need to redirect output to a local file
        Stream stream;
        int redirectPort = 0;

        stream = EndExecuteCommand(BeginExecuteCommand("start profiler: " + profilerConfiguration, true));

        bool onDemand = heapshotMode == "ondemand";
        if (onDemand) {
            // we need to forward heapshot requests from the GUI to the app too
            AsyncCallback callback = null;

            callback = new AsyncCallback((IAsyncResult ar) => {
                try {
                    TcpClient client = null;
                    string line;

                    lock (this._lockObject) {
                        if (this.heapshotForwarder == null || this.disposed)
                            return;
                        client = this.heapshotForwarder.EndAcceptTcpClient(ar);
                    }

                    using (client) {
                        using (var tcpstream = client.GetStream()) {
                            using (var reader = new StreamReader(tcpstream)) {
                                while ((line = reader.ReadLine()) != null) {
                                    if (line == "heapshot") {
                                        BeginSendHeapShotRequest(null, null);
                                    }
                                }
                            }
                        }
                        client.Close();
                    }

                    lock (this._lockObject) {
                        if (this.heapshotForwarder == null || this.disposed)
                            return;
                        this.heapshotForwarder.BeginAcceptTcpClient(callback, null);
                    }
                } catch (Exception ex) {
                    Logger.Log(ex);
                }
            });

            lock (this._lockObject) {
                this.heapshotForwarder = new TcpListener(IPAddress.Loopback, 0);
                this.heapshotForwarder.Start();
                redirectPort = ((IPEndPoint)this.heapshotForwarder.LocalEndpoint).Port;
                this.heapshotForwarder.BeginAcceptTcpClient(callback, null);
            }
        }

        // start thread to read from socket and write to file
        new System.Threading.Thread(() => WriteHeapshotOutputToFile(stream, outputFile, onDemand, redirectPort)) {
            IsBackground = true,
            Name = "HeapShot output writer"
        }.Start();
    }

    void WriteHeapshotOutputToFile(Stream stream, string outputFile, bool onDemand, int redirectPort) {
        try {
            // We must rewrite the output a little bit: the heapshot port is stored in the file,
            // and that's what the heapshot gui is using to request heapshots. However when we're
            // running on the device that port is the port on the device the profiler is listening
            // on, not the port heapshot needs to connect to (which is the one heapshot_listener is
            // listening on just above).
            using (var fs = File.OpenWrite(outputFile)) {
                int read;
                long total = 0;
                byte[] buffer = new byte[4096];
                do {
                    read = stream.Read(buffer, 0, buffer.Length);
                    if (read > 0) {
                        if (total < 29 && total + read > 30 && onDemand) {
                            // FIXME: add support for compressed output files (this is currently disabled in the gui)
                            // The port is at byte 28 & 29.
                            // We need to tell the app which port the profiler is listening on
                            EndExecuteCommand(BeginSendHeapShotPort(buffer[28] | (buffer[29] << 8)));
                            // And then tell the heapshot gui which port it should request heapshots on
                            buffer[28 - total] = (byte)(redirectPort & 0x00FF);
                            buffer[29 - total] = (byte)((redirectPort & 0xFF00) >> 8);
                        }
                        total += read;
                        fs.Write(buffer, 0, read);
                        fs.Flush();
                    }
                } while (read > 0);
            }
        } catch (Exception ex) {
            try {
                Console.WriteLine(ex);
                //tracer.Warn(Resources.IPhoneCommandConnection_ProfilerData_ReadingError, ex);
            } catch {
            }
        } finally {
            try {
                stream.Dispose();
            } catch {
            }
        }
    }

    IAsyncResult BeginSendSkipDebugger(AsyncCallback callback = null, object state = null) {
        return BeginExecuteCommand("start debugger: no", false, callback, state);
    }

    IAsyncResult SendSkipProfiler(AsyncCallback callback = null, object state = null) {
        return BeginExecuteCommand("start profiler: no", false, callback, state);
    }

    public virtual IAsyncResult BeginStartDebugger(AsyncCallback callback, object state) {
        var ar = new StartDebuggerAsyncResult(callback, state);
        var cmdAr = BeginExecuteCommand("start debugger: sdb", true, StartDebugger_GotTransport, ar);
        ar.RunningCommand = cmdAr;
        return ar;
    }

    void StartDebugger_GotTransport(IAsyncResult result) {
        var ar = (StartDebuggerAsyncResult)result.AsyncState;
        try {
            if (ar.Cancelled) {
                ar.CompleteWithError(new OperationCanceledException());
            }
            ar.Transport = EndExecuteCommand(result);
            if (ar.Transport == null) {
                throw new Exception("Resources.IPhoneCommandConnection_TransportConnect_Error");
            }
            var cmdAr = BeginExecuteCommand("connect output", true, StartDebugger_GotOutput, ar);
            StartDebugger_SetRunningCommand(ar, cmdAr);
        } catch (Exception ex) {
            ar.CompleteWithError(ex);
        }
    }

    void StartDebugger_GotOutput(IAsyncResult result) {
        var ar = (StartDebuggerAsyncResult)result.AsyncState;
        try {
            if (ar.Cancelled) {
                ar.CompleteWithError(new OperationCanceledException());
            }
            ar.Output = EndExecuteCommand(result);
            if (ar.Output == null) {
                throw new Exception("Resources.IPhoneCommandConnection_OutputConnect_Error");
            }
            var cmdAr = BeginSendSkipDebugger(StartDebugger_SentSkipProfiler, ar);
            StartDebugger_SetRunningCommand(ar, cmdAr);
        } catch (Exception ex) {
            ar.CompleteWithError(ex);
        }
    }

    void StartDebugger_SentSkipProfiler(IAsyncResult result) {
        var ar = (StartDebuggerAsyncResult)result.AsyncState;
        try {
            if (ar.Cancelled) {
                ar.CompleteWithError(new OperationCanceledException());
            }
            EndExecuteCommand(result);
            ar.Complete();
        } catch (Exception ex) {
            ar.CompleteWithError(ex);
        }
    }

    void StartDebugger_SetRunningCommand(StartDebuggerAsyncResult ar, IAsyncResult command) {
        lock (ar) {
            if (!ar.Cancelled) {
                ar.RunningCommand = command;
                return;
            }
            ar.RunningCommand = null;
        }
        CancelExecuteCommand(command);
    }

    public virtual void EndStartDebugger(IAsyncResult result, out Stream transport, out Stream output) {
        var ar = (StartDebuggerAsyncResult)result;
        ar.CheckError();
        transport = ar.Transport;
        output = ar.Output;
    }

    public void CancelStartDebugger(IAsyncResult result) {
        var ar = (StartDebuggerAsyncResult)result;
        IAsyncResult toCancel;
        lock (ar) {
            ar.Cancelled = true;
            toCancel = ar.RunningCommand;
        }
        if (toCancel != null) {
            CancelExecuteCommand(toCancel);
        }
    }

    class StartDebuggerAsyncResult : AggregateAsyncResult {
        public StartDebuggerAsyncResult(AsyncCallback callback, object state)
            : base(callback, state) {
        }

        public Stream Transport, Output;
        public IAsyncResult RunningCommand;
        public bool Cancelled;
    }

    public void StartLogProfiler(string outputFile, string profilerConfiguration, bool isDevice, string heapshotMode) {
        EndExecuteCommand(BeginSendSkipDebugger());
        SendStartLogProfiler(outputFile, profilerConfiguration, isDevice, heapshotMode);
    }

    // This is the only method that may be called from the main thread
    // All the other methods may block indefinitely (until Stop is called).
    public void Stop() {
        try {
            // Don't try forever to send 'exit process', the user might never have
            // started the app on the device.
            var ar = BeginExecuteCommand("exit process", true);
            ar.AsyncWaitHandle.WaitOne(100);
            EndExecuteCommand(ar);
        } catch (SocketException ex) {
            Logger.Log(ex);
        } finally {
            Dispose();  // make sure everything is cleaned up
        }
    }

    ~StreamCommandConnection() {
        Dispose(false);
    }

    public void Dispose() {
        if (this.disposed)
            return;

        lock (this._lockObject) {
            if (this.disposed)
                return;
            this.disposed = true;
        }

        GC.SuppressFinalize(this);
        Dispose(true);
    }

    public bool Disposed { get { return this.disposed; } }

    protected virtual void Dispose(bool disposing) {
        this.heapshotForwarder?.Stop();
    }
}