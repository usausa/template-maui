namespace Template.MobileApp.Models.App;

using System.Text.Json.Serialization;

[JsonSerializable(typeof(TimerSnapshot))]
[JsonSerializable(typeof(Puzzle2048Snapshot))]
[JsonSerializable(typeof(MinesweeperSnapshot))]
[JsonSerializable(typeof(SudokuSnapshot))]
internal sealed partial class AppJsonContext : JsonSerializerContext;
