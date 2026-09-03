namespace Template.MobileApp.Messaging;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

public interface IMediaController
{
    void Attach(MediaElement view);

    void Detach();
}

public sealed class MediaController : IMediaController
{
    private MediaElement? player;

    public Action<bool>? PlayingChanged { get; set; }

    public Action<bool>? LoadingChanged { get; set; }

    void IMediaController.Attach(MediaElement view)
    {
        player = view;
        player.StateChanged += OnStateChanged;
    }

    void IMediaController.Detach()
    {
        if (player is not null)
        {
            player.StateChanged -= OnStateChanged;
            player = null;
        }
    }

    private void OnStateChanged(object? sender, MediaStateChangedEventArgs e)
    {
        PlayingChanged?.Invoke(e.NewState == MediaElementState.Playing);
        LoadingChanged?.Invoke(e.NewState is MediaElementState.Opening or MediaElementState.Buffering);
    }

    public void TogglePlay()
    {
        if (player is null)
        {
            return;
        }

        if (player.CurrentState == MediaElementState.Playing)
        {
            player.Pause();
        }
        else
        {
            player.Play();
        }
    }

    public void SeekTo(double seconds)
    {
        if (player is null)
        {
            return;
        }

        _ = player.SeekTo(TimeSpan.FromSeconds(seconds), CancellationToken.None);
    }
}
