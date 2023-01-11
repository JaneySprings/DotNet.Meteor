using System;
using System.Text;

namespace DotNet.Meteor.Debug.Session;

class ByteBuffer {
    private byte[] buffer = new byte[0];

    public int Length => this.buffer.Length;

    public string GetString(Encoding enc) => enc.GetString(this.buffer);
    public void Append(byte[] b, int length) {
        byte[] newBuffer = new byte[this.buffer.Length + length];
        Buffer.BlockCopy(this.buffer, 0, newBuffer, 0, this.buffer.Length);
        Buffer.BlockCopy(b, 0, newBuffer, this.buffer.Length, length);
        this.buffer = newBuffer;
    }
    public byte[] RemoveFirst(int n) {
        byte[] b = new byte[n];
        Buffer.BlockCopy(this.buffer, 0, b, 0, n);
        byte[] newBuffer = new byte[this.buffer.Length - n];
        Buffer.BlockCopy(this.buffer, n, newBuffer, 0, this.buffer.Length - n);
        this.buffer = newBuffer;
        return b;
    }
}