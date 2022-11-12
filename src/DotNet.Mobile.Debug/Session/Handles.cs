using System.Collections.Generic;

namespace DotNet.Mobile.Debug.Session;

public class Handles<T> {
    private const int START_HANDLE = 1000;

    private int _nextHandle;
    private Dictionary<int, T> _handleMap;

    public Handles() {
        this._nextHandle = START_HANDLE;
        this._handleMap = new Dictionary<int, T>();
    }

    public void Reset() {
        this._nextHandle = START_HANDLE;
        this._handleMap.Clear();
    }

    public int Create(T value) {
        var handle = this._nextHandle++;
        this._handleMap[handle] = value;
        return handle;
    }

    public bool TryGet(int handle, out T value) {
        return this._handleMap.TryGetValue(handle, out value);
    }

    public T Get(int handle, T dflt) {
        if (this._handleMap.TryGetValue(handle, out T value)) {
            return value;
        }
        return dflt;
    }
}