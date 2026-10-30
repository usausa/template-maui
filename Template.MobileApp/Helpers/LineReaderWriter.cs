namespace Template.MobileApp.Helpers;

using System.IO.Pipelines;

using Smart.Threading;

#pragma warning disable CA1819
public sealed class LineReaderWriterOption
{
    public byte[] Terminator { get; set; } = "\r\n"u8.ToArray();

    public Encoding Encoding { get; set; } = Encoding.UTF8;
}
#pragma warning restore CA1819

public sealed class LineReaderWriter : IAsyncDisposable
{
    private static readonly LineReaderWriterOption DefaultOption = new();

    private readonly LineReaderWriterOption option;

    private readonly PipeReader reader;

    private readonly PipeWriter writer;

    public int Timeout { get; set; } = 5000;

    public LineReaderWriter(Stream stream, LineReaderWriterOption? option = null)
        : this(stream, stream, option)
    {
    }

    public LineReaderWriter(Stream reader, Stream writer, LineReaderWriterOption? option = null)
    {
        this.reader = PipeReader.Create(reader);
        this.writer = PipeWriter.Create(writer);
        this.option = option ?? DefaultOption;
    }

    public async ValueTask DisposeAsync()
    {
        await reader.CompleteAsync().ConfigureAwait(false);
        await writer.CompleteAsync().ConfigureAwait(false);
    }

    public async ValueTask<bool> WriteLineAsync(string line)
    {
        var maxBytes = option.Encoding.GetMaxByteCount(line.Length);
        var span = writer.GetSpan(maxBytes);
        var bytes = option.Encoding.GetBytes(line, span);
        writer.Advance(bytes);

        var terminator = option.Terminator;
        span = writer.GetSpan(terminator.Length);
        terminator.CopyTo(span);
        writer.Advance(terminator.Length);

        using var cancel = new CancellationTokenSource(Timeout);
        try
        {
            var result = await writer.FlushAsync(cancel.Token).ConfigureAwait(false);
            return result.IsCompleted;
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }

        return false;
    }

    public async ValueTask<string?> ReadLineAsync()
    {
        using var cancel = new ReusableCancellationTokenSource();
        try
        {
            while (true)
            {
                cancel.CancelAfter(Timeout);
                var result = await reader.ReadAsync(cancel.Token).ConfigureAwait(false);

                var buffer = result.Buffer;

                if (!buffer.IsEmpty && TryReadLine(ref buffer, out var line))
                {
                    reader.AdvanceTo(buffer.Start, buffer.End);
                    return line;
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }

                cancel.Reset();
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }

        return null;
    }

    private bool TryReadLine(ref ReadOnlySequence<byte> buffer, out string line)
    {
        var terminator = option.Terminator;
        var seqReader = new SequenceReader<byte>(buffer);
        if (seqReader.TryReadTo(out ReadOnlySequence<byte> l, terminator))
        {
            buffer = buffer.Slice(seqReader.Position);

            var length = (int)l.Length;
            if (l.IsSingleSegment)
            {
                line = option.Encoding.GetString(l.First.Span[..length]);
            }
            else
            {
                var pooledBytes = default(byte[]?);
                var bytes = length <= 512 ? stackalloc byte[length] : (pooledBytes = ArrayPool<byte>.Shared.Rent(length)).AsSpan();

                l.CopyTo(bytes);
                line = option.Encoding.GetString(bytes[..length]);

                if (pooledBytes is not null)
                {
                    ArrayPool<byte>.Shared.Return(pooledBytes);
                }
            }

            return true;
        }

        line = null!;
        return false;
    }
}
