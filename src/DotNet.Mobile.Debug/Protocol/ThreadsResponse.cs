using System.Collections.Generic;
using System.Linq;
using DotNet.Mobile.Debug.Entities;

namespace DotNet.Mobile.Debug.Protocol;

public class ThreadsResponseBody : ResponseBody {
    public Thread[] threads { get; }

    public ThreadsResponseBody(List<Thread> ths) {
        threads = ths.ToArray<Thread>();
    }
}