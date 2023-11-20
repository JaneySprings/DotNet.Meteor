using System;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug.Extensions;

public static class MonoClientExtensions {
    public static StackFrame GetFrameSafe(this Backtrace bt, int n) {
		try {
            return bt.GetFrame(n);
        } catch (Exception) {
            return null;
        }
	}

    public static DebuggerSessionOptions DefaultDebuggerOptions = new DebuggerSessionOptions {
        EvaluationOptions = new EvaluationOptions {
            EvaluationTimeout = 5000,
            MemberEvaluationTimeout = 5000,
            UseExternalTypeResolver = false,
            AllowMethodEvaluation = true,
            GroupPrivateMembers = true,
            GroupStaticMembers = true,
            AllowToStringCalls = true,
            AllowTargetInvoke = true,
            ChunkRawStrings = false,
            EllipsizeStrings = false,
            CurrentExceptionTag = "$exception",
            IntegerDisplayFormat = IntegerDisplayFormat.Decimal,
            StackFrameFormat = new StackFrameFormat()
        },
    };
}