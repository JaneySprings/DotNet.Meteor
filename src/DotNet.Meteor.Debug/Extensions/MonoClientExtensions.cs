using System;
using Mono.Debugging.Client;
using DebugProtocol =  Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace DotNet.Meteor.Debug;

public static class MonoClientExtensions {
    public static StackFrame GetFrameSafe(this Backtrace bt, int n) {
		try {
            return bt.GetFrame(n);
        } catch (Exception) {
            return null;
        }
	}

    public static DebugProtocol.CompletionItem ToCompletionItem(this CompletionItem item) {
        return new DebugProtocol.CompletionItem() {
            Type = item.Flags.ToCompletionItemType(),
            SortText = item.Name,
            Label = item.Name,
        };
    }

    private static DebugProtocol.CompletionItemType ToCompletionItemType(this ObjectValueFlags flags) {
        if (flags.HasFlag(ObjectValueFlags.Method))
            return DebugProtocol.CompletionItemType.Method;
        if (flags.HasFlag(ObjectValueFlags.Field))
            return DebugProtocol.CompletionItemType.Field;
        if (flags.HasFlag(ObjectValueFlags.Property))
            return DebugProtocol.CompletionItemType.Property;
        if (flags.HasFlag(ObjectValueFlags.Namespace))
            return DebugProtocol.CompletionItemType.Module;
        if (flags.HasFlag(ObjectValueFlags.Type))
            return DebugProtocol.CompletionItemType.Class;
        if (flags.HasFlag(ObjectValueFlags.Variable))
            return DebugProtocol.CompletionItemType.Variable;

        return DebugProtocol.CompletionItemType.Text;
    }
}