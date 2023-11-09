using System;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug;

public static class MonoClientExtensions {
    public static StackFrame GetFrameSafe(this Backtrace bt, int n) {
		try {
            return bt.GetFrame(n);
        } catch (Exception) {
            return null;
        }
	}
}