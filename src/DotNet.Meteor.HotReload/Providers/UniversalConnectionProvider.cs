using System.Net.Sockets;
using DotNet.Meteor.Common;

namespace DotNet.Meteor.HotReload.Providers;

public class UniversalConnectionProvider : IConnectionProvider {
    private const int MaxConnectionAttempts = 3;
    private const int TimeBetweenConnectionAttempts = 500;
    private StreamReader? transportReader;
    private StreamWriter? transportWriter;
    
    protected int Port { get; init; }

    public StreamReader? TransportReader => transportReader;
    public StreamWriter? TransportWriter => transportWriter;

    public UniversalConnectionProvider(int port) {
        Port = port == 0 ? RuntimeSystem.GetFreePort() : port;
    }

    public Task<bool> PrepareTransportAsync() {
        return Task.FromResult(true);
    }
    public async Task<bool> TryConnectAsync() {
        for (var i = 0; i < MaxConnectionAttempts; i++) {
            try {
                var client = new TcpClient("localhost", Port);
                var stream = client.GetStream();
                transportReader = new StreamReader(stream);
                transportWriter = new StreamWriter(stream) { AutoFlush = true };
                return true;
            } catch {
                await Task.Delay(TimeBetweenConnectionAttempts);
            }
        }
        return false;
    }
}