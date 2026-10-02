namespace Template.MobileApp.Modules.Device;

using System.Security.Cryptography;

using Template.MobileApp.Components;

public sealed partial class DeviceBiometricViewModel : AppViewModelBase
{
    private readonly IBiometricAuthenticator authenticator;

    // 鍵での認証のサーバーの役 (端末の中で試す)
    private readonly BiometricSignInServer server = new();

    [ObservableProperty]
    public partial BiometricAvailability BiometricStatus { get; set; }

    [ObservableProperty]
    public partial BiometricAvailability WeakStatus { get; set; }

    [ObservableProperty]
    public partial BiometricAvailability CredentialStatus { get; set; }

    // 最後に強い生体認証か画面ロックで本人確認してからの時間
    [ObservableProperty]
    public partial TimeSpan? SinceLastAuthentication { get; set; }

    [ObservableProperty]
    public partial BiometricResult? Result { get; set; }

    [ObservableProperty]
    public partial BiometricAuthenticationType AuthenticationType { get; set; }

    // 署名の鍵と置き場所、サーバーに登録した公開鍵の指紋
    [ObservableProperty]
    public partial BiometricKeyState KeyState { get; set; }

    [ObservableProperty]
    public partial BiometricKeyLocation? KeyLocation { get; set; }

    [ObservableProperty]
    public partial ReadOnlyMemory<byte> KeyFingerprint { get; set; }

    // サーバーが出したチャレンジと、端末の署名
    [ObservableProperty]
    public partial ReadOnlyMemory<byte> Challenge { get; set; }

    [ObservableProperty]
    public partial ReadOnlyMemory<byte> Signature { get; set; }

    [ObservableProperty]
    public partial BiometricSignStatus? SignStatus { get; set; }

    // サーバーの検証 (署名、1 バイト変えたチャレンジ、同じチャレンジの 2 回目)
    [ObservableProperty]
    public partial bool? Verified { get; set; }

    [ObservableProperty]
    public partial bool? TamperedVerified { get; set; }

    [ObservableProperty]
    public partial bool? ReplayVerified { get; set; }

    public IObserveCommand EnrollCommand { get; }

    public IObserveCommand AuthenticateCommand { get; }

    public IObserveCommand RegisterCommand { get; }
    public IObserveCommand SignInCommand { get; }
    public IObserveCommand DeleteKeyCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public DeviceBiometricViewModel(
        IBiometricAuthenticator authenticator,
        Session session)
    {
        this.authenticator = authenticator;

        // 設定の登録の画面などから戻ったら読み直す
        Disposables.Add(session.AsObservable(nameof(Session.IsForeground)).Subscribe(_ =>
        {
            UpdateAvailability();
            UpdateKey();
        }));

        EnrollCommand = MakeDelegateCommand(Enroll, () => BiometricStatus == BiometricAvailability.NoneEnrolled);

        AuthenticateCommand = MakeAsyncCommand<BiometricMethod>(AuthenticateAsync);

        RegisterCommand = MakeDelegateCommand(Register, () => BiometricStatus == BiometricAvailability.Available);
        SignInCommand = MakeAsyncCommand(SignInAsync, () => KeyState == BiometricKeyState.Available);
        DeleteKeyCommand = MakeDelegateCommand(DeleteKey, () => KeyState != BiometricKeyState.None);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    // サーバーには前に登録した公開鍵がある (端末の鍵から読み直す)
    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            UpdateAvailability();
            server.Register(authenticator.GetSigningPublicKey());
            UpdateKey();
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    // 強い生体認証の登録の画面を開く (登録は利用者の操作)
    private void Enroll() => authenticator.OpenEnrollment(BiometricMethod.Biometric);

    // 顔認証などの後の確認のボタンは省く。結果の後に使えるかどうかを読み直す (失敗回数の超過で一時的に使えなくなる)
    private async Task AuthenticateAsync(BiometricMethod method)
    {
        var text = new BiometricPromptText("本人確認", "キャンセル")
        {
            Subtitle = "Device > Biometric",
            Description = authenticator.GetLabels(method)?.PromptMessage,
            ConfirmationRequired = false
        };
        var result = await authenticator.AuthenticateAsync(text, method);
        Result = result.Result;
        AuthenticationType = result.Type;
        UpdateAvailability();
    }

    // 鍵を作り (あれば作り直す)、公開鍵をサーバーに登録する
    private void Register()
    {
        server.Register(authenticator.CreateSigningKey());
        ClearSignIn();
        UpdateKey();
    }

    // サーバーがチャレンジを出し、端末が生体認証の後に署名し、サーバーが登録した公開鍵で確かめる
    private async Task SignInAsync()
    {
        ClearSignIn();
        var challenge = server.IssueChallenge();
        Challenge = challenge;

        var text = new BiometricPromptText("ログイン", "キャンセル")
        {
            Subtitle = "Key sign in",
            Description = authenticator.GetLabels(BiometricMethod.Biometric)?.PromptMessage
        };
        var result = await authenticator.SignAsync(challenge, text);
        SignStatus = result.Status;
        Signature = result.Signature;
        if (result.Status == BiometricSignStatus.Succeeded)
        {
            Verified = server.Verify(challenge, result.Signature.Span);
            TamperedVerified = server.VerifySignature(Tamper(challenge), result.Signature.Span);
            ReplayVerified = server.Verify(challenge, result.Signature.Span);
        }

        UpdateAvailability();
        UpdateKey();
    }

    private void DeleteKey()
    {
        authenticator.DeleteSigningKey();
        server.Register(null);
        ClearSignIn();
        UpdateKey();
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    // 1 バイト目の 1 ビットを変える
    private static byte[] Tamper(ReadOnlySpan<byte> data)
    {
        var tampered = data.ToArray();
        tampered[0] ^= 1;
        return tampered;
    }

    private void UpdateAvailability()
    {
        BiometricStatus = authenticator.CheckAvailability(BiometricMethod.Biometric);
        WeakStatus = authenticator.CheckAvailability(BiometricMethod.WeakBiometric);
        CredentialStatus = authenticator.CheckAvailability(BiometricMethod.DeviceCredential);
        SinceLastAuthentication = authenticator.GetTimeSinceLastAuthentication(BiometricMethod.BiometricOrDeviceCredential);
    }

    private void UpdateKey()
    {
        KeyState = authenticator.CheckSigningKey();
        KeyLocation = authenticator.GetSigningKeyLocation();
        KeyFingerprint = server.GetFingerprint();
    }

    private void ClearSignIn()
    {
        Challenge = ReadOnlyMemory<byte>.Empty;
        Signature = ReadOnlyMemory<byte>.Empty;
        SignStatus = null;
        Verified = null;
        TamperedVerified = null;
        ReplayVerified = null;
    }

    //--------------------------------------------------------------------------------
    // Server
    //--------------------------------------------------------------------------------

    // 鍵での認証のサーバーの役 (公開鍵の登録・チャレンジの発行・署名の検証)。端末の中で試すための見本で、サーバーに移すときもそのまま使える
    private sealed class BiometricSignInServer
    {
        private const int ChallengeSize = 32;

        // 登録した公開鍵 (X.509 の SubjectPublicKeyInfo)
        private byte[]? publicKey;

        // 出して、まだ受け付けていないチャレンジ
        private byte[]? challenge;

        // null で登録を消す
        public void Register(byte[]? key)
        {
            publicKey = key;
            challenge = null;
        }

        // 登録した公開鍵の指紋 (SHA-256)
        public ReadOnlyMemory<byte> GetFingerprint() =>
            publicKey is null ? ReadOnlyMemory<byte>.Empty : SHA256.HashData(publicKey);

        // 毎回新しい乱数を出し、前に出したものは受け付けない
        public byte[] IssueChallenge()
        {
            challenge = RandomNumberGenerator.GetBytes(ChallengeSize);
            return [.. challenge];
        }

        // 出したチャレンジへの署名だけを受け付ける。チャレンジは結果によらず 1 回で使い済みにする
        public bool Verify(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
        {
            var accepted = (challenge is not null) && data.SequenceEqual(challenge) && VerifySignature(data, signature);
            challenge = null;
            return accepted;
        }

        // 登録した公開鍵で署名だけを確かめる (Android の署名は DER 形式)
        public bool VerifySignature(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
        {
            var valid = false;
            if (publicKey is not null)
            {
                using var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
                valid = ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
            }

            return valid;
        }
    }
}
