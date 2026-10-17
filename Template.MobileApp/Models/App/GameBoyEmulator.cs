namespace Template.MobileApp.Models.App;

using GonFox.GameBoy.Core;
using GonFox.GameBoy.Core.Devices;
using GonFox.GameBoy.Platform;
using GonFox.GameBoy.Platform.Audio;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class GameBoySnapshot
{
    public bool Muted { get; set; }
}

public sealed record GameBoyButtonChange(
    string Source,
    JoypadButton Button,
    bool Pressed);

//--------------------------------------------------------------------------------
// Emulator
//--------------------------------------------------------------------------------

public sealed partial class GameBoyEmulator : IDisposable
{
    private const int Volume = 70;

    private readonly AudioPlayback audio;

    private readonly EmulationRunner runner;

    private readonly ButtonInputState input = new();

    private bool suspended;

    private bool resumeRunning;

    public FrameExchange Frames => runner.Frames;

    public byte Buttons => input.Mask;

    public GameBoyEmulator()
    {
        audio = new AudioPlayback(CreateAudioDevice);
        runner = new EmulationRunner(audio.Buffer, new RewindOptions(GameBoySystem.CyclesPerSecond, 0));
    }

    public void Dispose()
    {
        audio.Dispose();
        runner.Dispose();
    }

    public Task StartAsync(byte[] image, string name, bool muted)
    {
        audio.SetVolume(Volume, muted);
        audio.Prepare();
        return runner.LoadAsync(image, name);
    }

    public void SetMuted(bool muted) => audio.SetVolume(Volume, muted);

    public double Update()
    {
        var status = runner.Status;
        audio.Update(status.Running && !status.Stopped && !suspended);
        return status.Fps;
    }

    public Task ChangeButtonAsync(GameBoyButtonChange change) =>
        input.Set(change.Source, change.Button, change.Pressed) ? runner.SetButtonsAsync(input.Mask) : Task.CompletedTask;

    public async Task SuspendAsync()
    {
        if (!suspended)
        {
            suspended = true;
            resumeRunning = runner.Status.Running;
            input.Clear();
            audio.Update(false);
            await runner.SetButtonsAsync(0);
            await runner.PauseAsync();
        }
    }

    public Task ResumeAsync()
    {
        var resume = suspended && resumeRunning;
        suspended = false;
        return resume ? runner.ResumeAsync(0) : Task.CompletedTask;
    }

    private static partial IAudioDevice CreateAudioDevice();
}
