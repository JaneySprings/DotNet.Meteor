using System.Collections.Generic;

namespace DotNet.Meteor.Debug;

public class Handles<T> {
    private const int StartHandle = 1000;

    private int nextHandle;
    private readonly Dictionary<int, T> handleMap;

    public Handles() {
        nextHandle = StartHandle;
        handleMap = new Dictionary<int, T>();
    }

    public void Reset() {
        nextHandle = StartHandle;
        handleMap.Clear();
    }

    public int Create(T value) {
        var handle = nextHandle++;
        handleMap[handle] = value;
        return handle;
    }

    public bool TryGet(int handle, out T value) {
        return handleMap.TryGetValue(handle, out value);
    }

    public T Get(int handle, T defaultValue) {
        if (handleMap.TryGetValue(handle, out T value))
            return value;
        return defaultValue;
    }
}