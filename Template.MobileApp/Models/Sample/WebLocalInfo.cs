namespace Template.MobileApp.Models.Sample;

// HybridWebView の WebResourceRequested でページへ返す応答
public sealed record WebLocalInfo(string Application, string Version, string Device, DateTime Time);
