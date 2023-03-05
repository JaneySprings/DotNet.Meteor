namespace DotNet.Meteor.Debug.Protocol.Events;

/* The event indicates that debugging of the debuggee has terminated. 
* This does not mean that the debuggee itself has exited. */
public class TerminatedEvent : Event {
    public TerminatedEvent() : base("terminated") {}
}