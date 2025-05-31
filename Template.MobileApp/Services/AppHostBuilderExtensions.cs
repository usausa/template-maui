namespace Template.MobileApp.Services;

using System.Net.Http.Headers;

public static class AppHostBuilderExtensions
{
    public static MauiAppBuilder ConfigureHttpClient(this MauiAppBuilder builder)
    {
        builder.Services
            .AddHttpClient(ApiNames.Default, (p, client) =>
            {
                client.BaseAddress = p.GetRequiredService<ApiContext>().BaseAddress;
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new SocketsHttpHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    PooledConnectionLifetime = TimeSpan.FromMinutes(1)
                };
//#pragma warning disable CA5359
//                handler.SslOptions.RemoteCertificateValidationCallback = static (_, _, _, _) => true;
//#pragma warning restore CA5359
                return handler;
            })
            .AddHttpMessageHandler<ApiDelegatingHandler>();

        builder.Services.AddTransient<ApiDelegatingHandler>();

        builder.Services.AddSingleton<ApiContext>();

        return builder;
    }
}
