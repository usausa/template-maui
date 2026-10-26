namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record LyricLine(TimeSpan Time, string Text);

public sealed class MusicTrack
{
    public required string Title { get; init; }

    public required string Artist { get; init; }

    public required string Album { get; init; }

    public required string Artwork { get; init; }

    public required TimeSpan Duration { get; init; }

    public required IReadOnlyList<LyricLine> Lyrics { get; init; }

    public required IReadOnlyList<float> Waveform { get; init; }
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed partial class MusicPlayer : ObservableObject
{
    public required string Playlist { get; init; }

    public required int PlaylistCount { get; init; }

    public required IReadOnlyList<MusicTrack> Tracks { get; init; }

    [ObservableProperty(NotifyAlso = [nameof(Track)])]
    public partial int Index { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(Progress), nameof(Remain)])]
    public partial TimeSpan Position { get; set; }

    [ObservableProperty]
    public partial bool IsPlaying { get; set; } = true;

    [ObservableProperty]
    public partial bool IsShuffle { get; set; }

    [ObservableProperty]
    public partial bool IsRepeat { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; } = true;

    public MusicTrack Track => Tracks[Index];

    public double Progress => Track.Duration > TimeSpan.Zero ? Position / Track.Duration : 0;

    public TimeSpan Remain => Track.Duration - Position;

    public int LineIndex
    {
        get
        {
            var lyrics = Track.Lyrics;
            var index = 0;
            for (var i = 0; i < lyrics.Count; i++)
            {
                if (lyrics[i].Time <= Position)
                {
                    index = i;
                }
            }

            return index;
        }
    }

    public void Advance(TimeSpan delta)
    {
        var next = Position + delta;
        if (next >= Track.Duration)
        {
            if (IsRepeat)
            {
                Position = TimeSpan.Zero;
            }
            else
            {
                Move(1);
            }

            return;
        }

        Position = next;
    }

    public void Move(int offset)
    {
        Index = (Index + offset + Tracks.Count) % Tracks.Count;
        Position = TimeSpan.Zero;
    }

    public void Seek(double ratio) =>
        Position = Track.Duration * Math.Clamp(ratio, 0d, 1d);
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class MusicSample
{
    private const int WaveformCount = 96;

    private static readonly string[][] Verses =
    [
        [
            "金色の波が丘を越える", "風は遠い国の歌を運ぶ", "刈り入れの鐘が谷に響く",
            "夕暮れに母の声が溶ける", "名も知らぬ鳥が舞い上がる", "空の果てまで道は続く",
            "麦の海を渡る風になって", "君の住む町まで届けにいこう", "古い竪琴の弦が震えるたび",
            "忘れた歌がまた目を覚ます", "実りの夜に灯をともして", "巡る季節をそっと見送ろう"
        ],
        [
            "鋼の拳を握りしめ", "吠えろ 大地を揺るがして", "燃える瞳に映るのは",
            "守ると決めた街の灯", "砕けた盾を掲げても", "心のエンジンは止まらない",
            "限界なんて超えていけ", "火花を散らして突き進め", "痛みも涙も燃料にして",
            "今こそ叫べ 魂の名を", "鋼鉄の咆哮が空を裂く", "明日を掴むまで戦い抜け"
        ],
        [
            "枝の先に灯るランタン", "森の奥で笛が呼んでいる", "輪になって踊ろう 手をつないで",
            "古い樹々も目を覚ます夜", "葉の隙間から星が降る", "数えきれない願いを乗せて",
            "弦をはじく指が速くなる", "足音が大地を叩いていく", "名前のない森の精たちも",
            "今夜だけは隣で笑う", "朝が来たら夢と呼ぶけれど", "胸の灯はずっと消えない"
        ],
        [
            "光の速さで駆け抜けろ", "星の海を切り裂いて", "遠い彼方で呼んでいる",
            "まだ見ぬ仲間の声がする", "闇が行く手を塞いでも", "この胸の炎は消せない",
            "銀河の果てまで届け この声", "運命なんて撃ち抜いて", "一人じゃないと知ったから",
            "何度でも立ち上がれる", "輝け 銀河を貫く槍よ", "新しい夜明けを連れてこい"
        ]
    ];

    private static readonly (string Title, string Artist, string Album, string Artwork, int Seconds)[] Plan =
    [
        ("麦の海を渡る風", "WILLOW CHORUS", "FIELD OF HARPS", "music01.jpg", 242),
        ("鋼鉄の咆哮", "STARBLAZE", "GALAXY ROAR", "music02.png", 198),
        ("星降る森の祭り", "WILLOW CHORUS", "FIELD OF HARPS", "music03.png", 274),
        ("銀河を貫け", "STARBLAZE", "GALAXY ROAR", "music04.jpg", 221)
    ];

    private const string ArtworkFolder = "Music";

    public static MusicPlayer LoadPlayer()
    {
        var tracks = new List<MusicTrack>(Plan.Length);
        for (var i = 0; i < Plan.Length; i++)
        {
            var (title, artist, album, artwork, seconds) = Plan[i];
            var duration = TimeSpan.FromSeconds(seconds);
            tracks.Add(new MusicTrack
            {
                Title = title,
                Artist = artist,
                Album = album,
                Artwork = Path.Combine(ArtworkFolder, artwork),
                Duration = duration,
                Lyrics = CreateLyrics(Verses[i], duration),
                Waveform = CreateWaveform(i)
            });
        }

        return new MusicPlayer
        {
            Playlist = "深夜のドライブ",
            PlaylistCount = 42,
            Tracks = tracks,
            Position = TimeSpan.FromSeconds(138)
        };
    }

    private static LyricLine[] CreateLyrics(string[] verse, TimeSpan duration)
    {
        var start = duration.TotalSeconds * 0.08;
        var step = (duration.TotalSeconds * 0.84) / verse.Length;
        return [.. verse.Select((x, i) => new LyricLine(TimeSpan.FromSeconds(start + (step * i)), x))];
    }

    private static float[] CreateWaveform(int seed)
    {
        var values = new float[WaveformCount];
        for (var i = 0; i < WaveformCount; i++)
        {
            var phase = (double)i / WaveformCount;
            var envelope = 0.45 + (0.35 * Math.Sin(phase * Math.PI));
            var detail = 0.2 * Math.Sin((i * (3 + seed)) + (seed * 1.7));
            var grain = 0.12 * Math.Sin(i * (0.7 + (seed * 0.13)));
            values[i] = (float)Math.Clamp(envelope + detail + grain, 0.12, 1.0);
        }

        return values;
    }
}
