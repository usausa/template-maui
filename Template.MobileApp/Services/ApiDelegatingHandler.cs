namespace Template.MobileApp.Services;

using System.Net.Http.Headers;
using System.Text.Json;

public sealed class ApiDelegatingHandler : DelegatingHandler
{
    private const string ProblemMediaType = "application/problem+json";

    private readonly ILogger<ApiDelegatingHandler> log;

    private readonly ApiContext apiContext;

    public ApiDelegatingHandler(ILogger<ApiDelegatingHandler> log, ApiContext apiContext)
    {
        this.log = log;
        this.apiContext = apiContext;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = apiContext.Token;
        if (!String.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode && (response.Content.Headers.ContentType?.MediaType == ProblemMediaType))
        {
            await LogProblemAsync(request, response, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }

    private async ValueTask LogProblemAsync(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
            log.WarnApiProblem(request.Method, request.RequestUri?.AbsolutePath, (int)response.StatusCode, FindString(document.RootElement, "title"), FindString(document.RootElement, "traceId"));
        }
        catch (Exception ex) when (ex is JsonException or HttpRequestException or IOException)
        {
            // Do nothing
        }
    }

    private static string? FindString(JsonElement element, string name) =>
        (element.ValueKind == JsonValueKind.Object) && element.TryGetProperty(name, out var value) && (value.ValueKind == JsonValueKind.String) ? value.GetString() : null;
}
