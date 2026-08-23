namespace DotNet.Meteor.Workspace.HotReload;

public interface IConnectionProvider {
    public StreamReader? TransportReader { get; }
    public StreamWriter? TransportWriter { get; }

    Task<bool> PrepareTransportAsync();
    Task<bool> TryConnectAsync();

}