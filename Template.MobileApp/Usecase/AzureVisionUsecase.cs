namespace Template.MobileApp.Usecase;

using Azure;
using Azure.AI.Vision.ImageAnalysis;

public sealed record TagResult(string Name, float Confidence);

// Azure AI Vision (Image Analysis 4.0)。接続先とキーは設定から呼び出しのたびに取得する。失敗は Result の Error (例外のメッセージ)
public sealed class AzureVisionUsecase
{
    private const int JpegQuality = 90;

    // 枠を描く物体 / 人物の信頼度の下限
    private const float MinConfidence = 0.5f;

    private readonly Settings settings;

    public AzureVisionUsecase(Settings settings)
    {
        this.settings = settings;
    }

    // 物体 (枠 + ラベル + 信頼度)
    public Task<Result<DetectResult[]>> DetectObjectsAsync(SKBitmap bitmap) =>
        AnalyzeAsync(bitmap, VisualFeatures.Objects, default, result => result.Objects.Values
            .Where(static x => (x.Tags.Count > 0) && (x.Tags[0].Confidence >= MinConfidence))
            .Select(x => ToDetectResult(x.BoundingBox, bitmap, x.Tags[0].Confidence, x.Tags[0].Name))
            .ToArray());

    // 人物 (枠 + 信頼度)
    public Task<Result<DetectResult[]>> DetectPeopleAsync(SKBitmap bitmap) =>
        AnalyzeAsync(bitmap, VisualFeatures.People, default, result => result.People.Values
            .Where(static x => x.Confidence >= MinConfidence)
            .Select(x => ToDetectResult(x.BoundingBox, bitmap, x.Confidence, "person"))
            .ToArray());

    // タグ (画像全体の内容。日本語)
    public Task<Result<TagResult[]>> DetectTagsAsync(SKBitmap bitmap) =>
        AnalyzeAsync(bitmap, VisualFeatures.Tags, new ImageAnalysisOptions { Language = "ja" }, static result => result.Tags.Values
            .Select(x => new TagResult(x.Name, x.Confidence))
            .ToArray());

    // 文字 (行ごとの外接矩形 + テキスト)
    public Task<Result<DetectResult[]>> ReadTextAsync(SKBitmap bitmap) =>
        AnalyzeAsync(bitmap, VisualFeatures.Read, default, result => result.Read.Blocks
            .SelectMany(static x => x.Lines)
            .Select(x => new DetectResult(
                x.BoundingPolygon.Min(static p => p.X) / (float)bitmap.Width,
                x.BoundingPolygon.Min(static p => p.Y) / (float)bitmap.Height,
                x.BoundingPolygon.Max(static p => p.X) / (float)bitmap.Width,
                x.BoundingPolygon.Max(static p => p.Y) / (float)bitmap.Height,
                1f,
                x.Text))
            .ToArray());

    private async Task<Result<T>> AnalyzeAsync<T>(SKBitmap bitmap, VisualFeatures features, ImageAnalysisOptions options, Func<ImageAnalysisResult, T> selector)
    {
        try
        {
            var (endpoint, credential) = await ResolveCredentialAsync().ConfigureAwait(false);
            var client = new ImageAnalysisClient(endpoint, credential);
            var response = await client.AnalyzeAsync(Encode(bitmap), features, options).ConfigureAwait(false);
            return Result.Success(selector(response.Value));
        }
        catch (Exception ex) when (ex is RequestFailedException or HttpRequestException or InvalidOperationException)
        {
            return Result.Failure<T>(ex.Message);
        }
    }

    private async ValueTask<(Uri EndPoint, AzureKeyCredential Credential)> ResolveCredentialAsync()
    {
        var endpoint = settings.AIServiceEndPoint;
        var key = await settings.GetAIServiceKeyAsync().ConfigureAwait(false);
        if (String.IsNullOrEmpty(endpoint) || String.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("AI service is not configured.");
        }

        return (new Uri(endpoint), new AzureKeyCredential(key));
    }

    private static BinaryData Encode(SKBitmap bitmap)
    {
        using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);
        return BinaryData.FromBytes(data.ToArray());
    }

    private static DetectResult ToDetectResult(ImageBoundingBox box, SKBitmap bitmap, float score, string label) =>
        new(box.X / (float)bitmap.Width, box.Y / (float)bitmap.Height, (box.X + box.Width) / (float)bitmap.Width, (box.Y + box.Height) / (float)bitmap.Height, score, label);
}
