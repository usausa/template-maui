namespace Template.MobileApp.Services;

using System.Net.Http.Headers;

public sealed class ApiDelegatingHandler : DelegatingHandler
{
    private readonly ApiContext apiContext;

    public ApiDelegatingHandler(ApiContext apiContext)
    {
        this.apiContext = apiContext;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = apiContext.Token;
        if (!String.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
