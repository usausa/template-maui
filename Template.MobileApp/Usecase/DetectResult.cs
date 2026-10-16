namespace Template.MobileApp.Usecase;

public sealed record DetectResult(
    float Left,
    float Top,
    float Right,
    float Bottom,
    float Score,
    string Label);
