namespace Template.MobileApp.Usecase;

// 検出結果 (座標は画像に対する比率)
public sealed record DetectResult(
    float Left,
    float Top,
    float Right,
    float Bottom,
    float Score,
    string Label);
