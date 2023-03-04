namespace DotNet.Meteor.Debug.Protocol.Events;

/* This event indicates that the debuggee has exited. */
public class TerminatedEvent : Event {
    public TerminatedEvent() : base("terminated") {}
}