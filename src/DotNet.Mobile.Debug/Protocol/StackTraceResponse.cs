using System.Collections.Generic;
using System.Linq;
using DotNet.Mobile.Debug.Entities;

namespace DotNet.Mobile.Debug.Protocol;

public class StackTraceResponseBody : ResponseBody {
    public StackFrame[] stackFrames { get; }
    public int totalFrames { get; }

    public StackTraceResponseBody(List<StackFrame> frames, int total) {
        stackFrames = frames.ToArray<StackFrame>();
        totalFrames = total;
    }
}