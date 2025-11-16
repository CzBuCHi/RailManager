using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace MockFileSystem.Utility;

[ExcludeFromCodeCoverage]
public sealed class MemoryFileStream(Action<byte[], int, int> write, Action dispose) : Stream
{
    private long _Length;

    public override void Flush() {
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (disposing) {
            dispose();
        }
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) {
        write(buffer, offset, count);
        _Length += count;
        Position += count;
    }

    public override bool CanRead  => false;
    public override bool CanSeek  => false;
    public override bool CanWrite => true;

    public override long Length => _Length;

    public override long Position { get; set; }
}