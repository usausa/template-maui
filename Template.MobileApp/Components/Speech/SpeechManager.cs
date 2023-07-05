namespace Template.MobileApp.Components.Speech;

using CommunityToolkit.Maui.Media;

public interface ISpeechManager
{
    // Text to speech

    ValueTask SpeakAsync(string text, float? pitch = null, float? volume = null);

    void SpeakCancel();

    // Speech to text

    // TODO
    ValueTask<string?> RecognizeAsync(Action<string> progress);
}

public sealed class SpeechManager : ISpeechManager, IDisposable
{
    private readonly ITextToSpeech textToSpeech;

    private readonly ISpeechToText speechToText;

    private CancellationTokenSource? cts;

    public SpeechManager(
        ITextToSpeech textToSpeech,
        ISpeechToText speechToText)
    {
        this.textToSpeech = textToSpeech;
        this.speechToText = speechToText;
    }

    public void Dispose()
    {
        cts?.Dispose();
    }

    // ------------------------------------------------------------
    // Text to speech
    // ------------------------------------------------------------

    public async ValueTask SpeakAsync(string text, float? pitch, float? volume)
    {
        cts = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Pitch = pitch,
            Volume = volume
        };
        await textToSpeech.SpeakAsync(text, options, cts.Token);
    }

    public void SpeakCancel()
    {
        if (cts?.IsCancellationRequested ?? true)
        {
            return;
        }

        cts.Cancel();
    }

    // ------------------------------------------------------------
    // Speech to text
    // ------------------------------------------------------------

    public async ValueTask<string?> RecognizeAsync(Action<string> progress)
    {
        var granted = await speechToText.RequestPermissions(default);
        if (!granted)
        {
            return null;
        }

        var result = await speechToText.ListenAsync(
            CultureInfo.CurrentCulture,
            new Progress<string>(progress),
            default);
        result.EnsureSuccess();

        if (result.IsSuccessful)
        {
            return result.Text;
        }

        return null;
    }
}
