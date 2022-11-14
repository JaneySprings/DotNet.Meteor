using System.Collections.Generic;
using System.Linq;
using DotNet.Mobile.Debug.Entities;

namespace DotNet.Mobile.Debug.Protocol;

public class VariablesResponseBody : ResponseBody {
    public Variable[] variables { get; }

    public VariablesResponseBody(List<Variable> vars) {
        variables = vars.ToArray<Variable>();
    }
}