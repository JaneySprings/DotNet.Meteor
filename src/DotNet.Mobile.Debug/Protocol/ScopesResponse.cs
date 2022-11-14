using System.Collections.Generic;
using System.Linq;
using DotNet.Mobile.Debug.Entities;

namespace DotNet.Mobile.Debug.Protocol;

public class ScopesResponseBody : ResponseBody {
    public Scope[] scopes { get; }

    public ScopesResponseBody(List<Scope> scps) {
        scopes = scps.ToArray<Scope>();
    }
}