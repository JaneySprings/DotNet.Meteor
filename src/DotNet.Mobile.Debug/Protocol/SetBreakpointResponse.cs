using System.Collections.Generic;
using System.Linq;
using DotNet.Mobile.Debug.Entities;

namespace DotNet.Mobile.Debug.Protocol;

public class SetBreakpointsResponseBody : ResponseBody {
    public Breakpoint[] breakpoints { get; }

    public SetBreakpointsResponseBody(List<Breakpoint> bpts = null) {
        if (bpts == null)
            breakpoints = new Breakpoint[0];
        else
            breakpoints = bpts.ToArray<Breakpoint>();
    }
}