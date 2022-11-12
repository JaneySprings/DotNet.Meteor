using System;
using System.Text;

namespace DotNet.Mobile.Debug.Session;

class ByteBuffer {
    private byte[] _buffer;

    public ByteBuffer() {
        this._buffer = Array.Empty<byte>();
    }

    public int Length {
        get { return this._buffer.Length; }
    }

    public string GetString(Encoding enc) {
        return enc.GetString(this._buffer);
    }

    public void Append(byte[] b, int length) {
        byte[] newBuffer = new byte[this._buffer.Length + length];
        System.Buffer.BlockCopy(this._buffer, 0, newBuffer, 0, this._buffer.Length);
        System.Buffer.BlockCopy(b, 0, newBuffer, this._buffer.Length, length);
        this._buffer = newBuffer;
    }

    public byte[] RemoveFirst(int n) {
        byte[] b = new byte[n];
        System.Buffer.BlockCopy(this._buffer, 0, b, 0, n);
        byte[] newBuffer = new byte[this._buffer.Length - n];
        System.Buffer.BlockCopy(this._buffer, n, newBuffer, 0, this._buffer.Length - n);
        this._buffer = newBuffer;
        return b;
    }
}