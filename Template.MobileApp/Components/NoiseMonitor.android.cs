namespace Template.MobileApp.Components;

using Android.Media;

public sealed partial class NoiseMonitor
{
    private const int SampleRate = 44100;

    private int bufferSize;

#pragma warning disable CA2213
    private AudioRecord? audioRecord;
#pragma warning restore CA2213

    private short[]? buffer;

    private partial void SetupMeasure()
    {
        bufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
        buffer = ArrayPool<short>.Shared.Rent(bufferSize);

        audioRecord = new AudioRecord(AudioSource.Mic, SampleRate, ChannelIn.Mono, Encoding.Pcm16bit, bufferSize);
        audioRecord.StartRecording();
    }

    private partial void CleanupMeasure()
    {
        audioRecord?.Stop();
        audioRecord?.Release();
        audioRecord?.Dispose();
        audioRecord = null;

        if (buffer is not null)
        {
            ArrayPool<short>.Shared.Return(buffer);
        }
    }

    private partial async ValueTask<double> Measure()
    {
        var read = await audioRecord!.ReadAsync(buffer!, 0, bufferSize);
        if (read <= 0)
        {
            return 0;
        }

        var sum = 0d;
        for (var i = 0; i < read; i++)
        {
            sum += buffer![i] * buffer[i];
        }

        var rms = Math.Sqrt(sum / read);
        return 20 * Math.Log10(rms == 0 ? 1 : rms);
    }
}
