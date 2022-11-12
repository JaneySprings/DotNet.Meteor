using System.Threading;

namespace DotNet.Mobile.Debug.Session;

public class MonoDebugSession: DebugSession {
    private const int MaxChildren = 100;
    private const int MaxConnectionAttempts = 20;
    private const int ConnectionAttemptInterval = 500;

    private AutoResetEvent resumeEvent = new AutoResetEvent(false);
    private bool debugExecuting = false;
}