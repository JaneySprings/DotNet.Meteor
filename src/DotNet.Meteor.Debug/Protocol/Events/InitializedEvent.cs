namespace DotNet.Meteor.Debug.Protocol.Events;

/* This event indicates that the debug adapter is ready to accept configuration requests (
 * e.g. setBreakpoints, setExceptionBreakpoints). */
public class InitializedEvent : Event {
    public InitializedEvent() : base("initialized") {}
}