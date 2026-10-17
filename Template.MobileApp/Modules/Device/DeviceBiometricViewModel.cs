namespace Template.MobileApp.Modules.Device;

using System.Security.Cryptography;

using Template.MobileApp.Components;

public sealed partial class DeviceBiometricViewModel : AppViewModelBase
{
    // Field

    private readonly IBiometricAuthenticator authenticator;

    private readonly BiometricSignInServer server = new();

    // Availability

    [ObservableProperty]
    public partial BiometricAvailability BiometricStatus { get; set; }

    [ObservableProperty]
    public partial BiometricAvailability WeakStatus { get; set; }

    [ObservableProperty]
    public partial BiometricAvailability CredentialStatus { get; set; }

    [ObservableProperty]
    public partial TimeSpan? SinceLastAuthentication { get; set; }

    // Authenticate

    [ObservableProperty]
    public partial BiometricResult? Result { get; set; }

    [ObservableProperty]
    public partial BiometricAuthenticationType AuthenticationType { get; set; }

    // Key sign in

    [ObservableProperty]
    public partial BiometricKeyState KeyState { get; set; }

    [ObservableProperty]
    public partial BiometricKeyLocation? KeyLocation { get; set; }

    [ObservableProperty]
    public partial ReadOnlyMemory<byte> KeyFingerprint { get; set; }

    [ObservableProperty]
    public partial ReadOnlyMemory<byte> Challenge { get; set; }

    [ObservableProperty]
    public partial ReadOnlyMemory<byte> Signature { get; set; }

    [ObservableProperty]
    public partial BiometricSignStatus? SignStatus { get; set; }

    [ObservableProperty]
    public partial bool? Verified { get; set; }

    [ObservableProperty]
    public partial bool? TamperedVerified { get; set; }

    [ObservableProperty]
    public partial bool? ReplayVerified { get; set; }

    // Command

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

    private void Enroll() => authenticator.OpenEnrollment(BiometricMethod.Biometric);

    private async Task AuthenticateAsync(BiometricMethod method)
    {
        var text = new BiometricPromptText("Verify", "Cancel")
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

    private void Register()
    {
        server.Register(authenticator.CreateSigningKey());
        ClearSignIn();
        UpdateKey();
    }

    private async Task SignInAsync()
    {
        ClearSignIn();
        var challenge = server.IssueChallenge();
        Challenge = challenge;

        var text = new BiometricPromptText("Log in", "Cancel")
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

    private sealed class BiometricSignInServer
    {
        private const int ChallengeSize = 32;

        private byte[]? publicKey;

        private byte[]? challenge;

        public void Register(byte[]? key)
        {
            publicKey = key;
            challenge = null;
        }

        public ReadOnlyMemory<byte> GetFingerprint() =>
            publicKey is null ? ReadOnlyMemory<byte>.Empty : SHA256.HashData(publicKey);

        public byte[] IssueChallenge()
        {
            challenge = RandomNumberGenerator.GetBytes(ChallengeSize);
            return [.. challenge];
        }

        public bool Verify(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
        {
            var accepted = (challenge is not null) && data.SequenceEqual(challenge) && VerifySignature(data, signature);
            challenge = null;
            return accepted;
        }

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
