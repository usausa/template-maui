namespace Template.MobileApp.Models.App;

using GonFox.GameBoy.Platform.Android;
using GonFox.GameBoy.Platform.Audio;

public sealed partial class GameBoyEmulator
{
    private static partial IAudioDevice CreateAudioDevice() => new AudioTrackDevice();
}
