namespace Template.MobileApp.Helpers;

public sealed class CancellationStream : Stream
{
    private readonly Stream stream;

    private readonly CancellationToken cancel;

    public override bool CanRead => stream.CanRead;

    public override bool CanSeek => stream.CanSeek;

    public override bool CanWrite => stream.CanWrite;

    public override long Length => stream.Length;

    public override long Position
    {
        get => stream.Position;
        set => stream.Position = value;
    }

    public CancellationStream(Stream stream, CancellationToken cancel)
    {
        this.stream = stream;
        this.cancel = cancel;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            stream.Dispose();
        }

        base.Dispose(disposing);
    }

    public override void Flush() => stream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        cancel.ThrowIfCancellationRequested();
        return stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);

    public override void SetLength(long value) => stream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
    {
        cancel.ThrowIfCancellationRequested();
        stream.Write(buffer, offset, count);
    }
}
