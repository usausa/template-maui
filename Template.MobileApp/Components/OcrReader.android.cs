namespace Template.MobileApp.Components;

using Android.Gms.Tasks;
using Android.Graphics;

using Xamarin.Google.Android.Odml.Image;
using Xamarin.Google.MLKit.Vision.Text;
using Xamarin.Google.MLKit.Vision.Text.Japanese;

#pragma warning disable CA1822
public sealed partial class OcrReader
{
    public partial async Task<string?> ReadTextAsync(Stream stream)
    {
        using var bitmap = await BitmapFactory.DecodeStreamAsync(stream);
        if (bitmap is null)
        {
            return null;
        }

#pragma warning disable CA2000
        using var recognizer = TextRecognition.GetClient(new JapaneseTextRecognizerOptions.Builder().Build());
        using var mlImage = new BitmapMlImageBuilder(bitmap).Build();
#pragma warning restore CA2000

        var task = recognizer.Process(mlImage);

        using var listener = new ProcessListener();
        task.AddOnSuccessListener(listener);
        task.AddOnFailureListener(listener);

        return await listener.Task;
    }

    private sealed class ProcessListener : Java.Lang.Object, IOnSuccessListener, IOnFailureListener
    {
        private readonly TaskCompletionSource<string?> tcs = new();

        public Task<string?> Task => tcs.Task;

        public void OnSuccess(Java.Lang.Object? result)
        {
            tcs.TrySetResult(result is Text text ? text.GetText() : null);
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            tcs.TrySetResult(null);
        }
    }
}
#pragma warning restore CA1822
