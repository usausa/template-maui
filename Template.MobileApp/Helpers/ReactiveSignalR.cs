namespace Template.MobileApp.Helpers;

using Microsoft.AspNetCore.SignalR.Client;

public static class ReactiveSignalR
{
    public static IObservable<T> CreateObservable<T>(string endPoint, string methodName, TimeSpan retryInterval)
    {
        return Observable.Create<T>(observer =>
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(endPoint)
                .WithAutomaticReconnect(new FixedIntervalRetryPolicy(retryInterval))
                .Build();

            connection.On<T>(methodName, observer.OnNext);

            var disposable = new CompositeDisposable();
            var cts = new CancellationTokenSource();

            disposable.Add(Disposable.Create(() => cts.Cancel()));
            disposable.Add(Observable.FromAsync(() => TryConnectWithRetryAsync(connection, retryInterval, cts.Token)).Subscribe());
            // ReSharper disable once AsyncVoidLambda
            disposable.Add(Disposable.Create(async () =>
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    // ReSharper disable once MethodSupportsCancellation
                    await connection.StopAsync();
                }
                await connection.DisposeAsync();
            }));

            return disposable;
        });
    }

    private static async Task<bool> TryConnectWithRetryAsync(HubConnection connection, TimeSpan retryInterval, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
#pragma warning disable CA1031
            try
            {
                await connection.StartAsync(cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                try
                {
                    await Task.Delay(retryInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }
#pragma warning restore CA1031
        }

        return false;
    }

    private sealed class FixedIntervalRetryPolicy(TimeSpan retryInterval) : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext) => retryInterval;
    }
}
