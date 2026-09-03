namespace Template.MobileApp.Components;

using Android.Media;
using Android.Util;

public sealed partial class NoiseMonitor
{
    private const int SampleRate = 44100;

    private int bufferSize;

#pragma warning disable CA2213
    private AudioRecord? audioRecord;
#pragma warning restore CA2213

    private short[]? buffer;

    private partial bool SetupMeasure()
    {
        try
        {
            bufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
            buffer = ArrayPool<short>.Shared.Rent(bufferSize);

            audioRecord = new AudioRecord(AudioSource.Mic, SampleRate, ChannelIn.Mono, Encoding.Pcm16bit, bufferSize);
            audioRecord.StartRecording();
            return true;
        }
        catch (Java.Lang.Throwable ex)
        {
            // 権限拒否・デバイス使用中等。呼び出し側は開始失敗として扱う
            Log.Error(nameof(NoiseMonitor), ex, "AudioRecord setup failed.");
            return false;
        }
    }

    private partial void CleanupMeasure()
    {
        if (audioRecord is not null)
        {
            try
            {
                audioRecord.Stop();
            }
            catch (Java.Lang.Throwable ex)
            {
                Log.Warn(nameof(NoiseMonitor), ex, "AudioRecord stop failed.");
            }

            audioRecord.Release();
            audioRecord.Dispose();
            audioRecord = null;
        }

        if (buffer is not null)
        {
            ArrayPool<short>.Shared.Return(buffer);
            buffer = null;
        }
    }

    private async partial ValueTask<double> Measure()
    {
        try
        {
            var read = await audioRecord!.ReadAsync(buffer!, 0, bufferSize).ConfigureAwait(false);
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
        catch (Java.Lang.Throwable ex)
        {
            Log.Warn(nameof(NoiseMonitor), ex, "AudioRecord read failed.");
            return 0;
        }
    }
}
