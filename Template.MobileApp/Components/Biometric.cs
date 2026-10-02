namespace Template.MobileApp.Components;

// 本人確認に使う方式
public enum BiometricMethod
{
    // 強い生体認証 (クラス 3) だけ
    Biometric,
    // 弱い生体認証 (クラス 2) 以上。鍵には使えない
    WeakBiometric,
    // 画面ロック (PIN・パターン・パスワード) だけ
    DeviceCredential,
    // 強い生体認証か画面ロック
    BiometricOrDeviceCredential
}

// 使えるかどうか
public enum BiometricAvailability
{
    Available,
    // センサーが無い
    NoHardware,
    // 生体情報 (画面ロック) が登録されていない
    NoneEnrolled,
    // 一時的に使えない (センサーの使用中・セキュリティ更新が要るなど)
    Unavailable
}

// 本人確認の結果
public enum BiometricResult
{
    Succeeded,
    Canceled,
    // 失敗回数の超過で一時的に使えない
    Lockout,
    Failed
}

// 本人確認に使われた方式
public enum BiometricAuthenticationType
{
    Unknown,
    Biometric,
    DeviceCredential
}

// Type は成功のときだけ
public sealed record BiometricAuthenticationResult(BiometricResult Result, BiometricAuthenticationType Type);

// ダイアログの文言 (CancelText は生体認証だけのときの取り消しのボタン)
public sealed record BiometricPromptText(string Title, string CancelText)
{
    public string? Subtitle { get; init; }

    public string? Description { get; init; }

    // false で、顔認証などの受け身の認証の後の確認のボタンを省く
    public bool ConfirmationRequired { get; init; } = true;
}

// 端末に合わせたシステムの文言 (ボタン・ダイアログの説明・設定の名前)
public sealed record BiometricLabels(string? ButtonLabel, string? PromptMessage, string? SettingName);

// 署名の鍵の状態
public enum BiometricKeyState
{
    // 鍵が無い
    None,
    Available,
    // 生体情報の登録が変わって使えない (消して作り直す)
    Invalidated
}

// 鍵の置き場所
public enum BiometricKeyLocation
{
    Unknown,
    Software,
    // TEE
    TrustedEnvironment,
    // 専用のセキュリティチップ
    StrongBox
}

// 署名の結果
public enum BiometricSignStatus
{
    Succeeded,
    Canceled,
    // 失敗回数の超過で一時的に使えない
    Lockout,
    Failed,
    // 鍵が無い
    NoKey,
    // 生体情報の登録が変わって鍵が使えない (消して作り直す)
    KeyInvalidated
}

// 署名は成功のときだけ (SHA256withECDSA の DER 形式。失敗のときは空)
public sealed record BiometricSignResult(BiometricSignStatus Status, ReadOnlyMemory<byte> Signature);

public interface IBiometricAuthenticator
{
    BiometricAvailability CheckAvailability(BiometricMethod method);

    // Android 12 から (それより前は null)
    BiometricLabels? GetLabels(BiometricMethod method);

    // 最後に本人確認してからの時間。Android 15 から、弱い生体認証を除く (記録が無いときも null)
    TimeSpan? GetTimeSinceLastAuthentication(BiometricMethod method);

    // 設定の登録の画面を開く (登録は利用者の操作)。開けなければ false
    bool OpenEnrollment(BiometricMethod method);

    // UI スレッドから呼ぶ。システムのダイアログで本人確認をする
    ValueTask<BiometricAuthenticationResult> AuthenticateAsync(BiometricPromptText text, BiometricMethod method, CancellationToken cancellationToken = default);

    // 署名の鍵は Android Keystore の EC 鍵 (P-256)。使うたびに強い生体認証を求め、生体情報の登録が変わると使えなくなる

    BiometricKeyState CheckSigningKey();

    // 鍵が無ければ null
    BiometricKeyLocation? GetSigningKeyLocation();

    // 鍵を作り (あれば作り直す。StrongBox があれば StrongBox に)、公開鍵 (X.509 の SubjectPublicKeyInfo) を返す。作れなければ null (生体情報が登録されていないなど)
    byte[]? CreateSigningKey();

    // 鍵が無ければ null
    byte[]? GetSigningPublicKey();

    // UI スレッドから呼ぶ。強い生体認証の後にデータに署名する (秘密鍵は Keystore の外に出ない)
    ValueTask<BiometricSignResult> SignAsync(byte[] data, BiometricPromptText text, CancellationToken cancellationToken = default);

    void DeleteSigningKey();
}

// 処理は各プラットフォームの PlatformXxx
public sealed partial class BiometricAuthenticator : IBiometricAuthenticator
{
    public BiometricAvailability CheckAvailability(BiometricMethod method) =>
        PlatformCheckAvailability(method);

    public BiometricLabels? GetLabels(BiometricMethod method) =>
        PlatformGetLabels(method);

    public TimeSpan? GetTimeSinceLastAuthentication(BiometricMethod method) =>
        PlatformGetTimeSinceLastAuthentication(method);

    public bool OpenEnrollment(BiometricMethod method) =>
        PlatformOpenEnrollment(method);

    public ValueTask<BiometricAuthenticationResult> AuthenticateAsync(BiometricPromptText text, BiometricMethod method, CancellationToken cancellationToken = default) =>
        PlatformAuthenticateAsync(text, method, cancellationToken);

    public BiometricKeyState CheckSigningKey() =>
        PlatformCheckSigningKey();

    public BiometricKeyLocation? GetSigningKeyLocation() =>
        PlatformGetSigningKeyLocation();

    public byte[]? CreateSigningKey() =>
        PlatformCreateSigningKey();

    public byte[]? GetSigningPublicKey() =>
        PlatformGetSigningPublicKey();

    public ValueTask<BiometricSignResult> SignAsync(byte[] data, BiometricPromptText text, CancellationToken cancellationToken = default) =>
        PlatformSignAsync(data, text, cancellationToken);

    public void DeleteSigningKey() =>
        PlatformDeleteSigningKey();

    private static partial BiometricAvailability PlatformCheckAvailability(BiometricMethod method);

    private static partial BiometricLabels? PlatformGetLabels(BiometricMethod method);

    private static partial TimeSpan? PlatformGetTimeSinceLastAuthentication(BiometricMethod method);

    private static partial bool PlatformOpenEnrollment(BiometricMethod method);

    private static partial ValueTask<BiometricAuthenticationResult> PlatformAuthenticateAsync(BiometricPromptText text, BiometricMethod method, CancellationToken cancellationToken);

    private static partial BiometricKeyState PlatformCheckSigningKey();

    private static partial BiometricKeyLocation? PlatformGetSigningKeyLocation();

    private static partial byte[]? PlatformCreateSigningKey();

    private static partial byte[]? PlatformGetSigningPublicKey();

    private static partial ValueTask<BiometricSignResult> PlatformSignAsync(byte[] data, BiometricPromptText text, CancellationToken cancellationToken);

    private static partial void PlatformDeleteSigningKey();
}
