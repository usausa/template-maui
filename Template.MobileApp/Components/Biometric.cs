namespace Template.MobileApp.Components;
public enum BiometricMethod
{
    Biometric,
    WeakBiometric,
    DeviceCredential,
    BiometricOrDeviceCredential
}
public enum BiometricAvailability
{
    Available,
    NoHardware,
    NoneEnrolled,
    Unavailable
}
public enum BiometricResult
{
    Succeeded,
    Canceled,
    Lockout,
    Failed
}
public enum BiometricAuthenticationType
{
    Unknown,
    Biometric,
    DeviceCredential
}
public sealed record BiometricAuthenticationResult(BiometricResult Result, BiometricAuthenticationType Type);
public sealed record BiometricPromptText(string Title, string CancelText)
{
    public string? Subtitle { get; init; }

    public string? Description { get; init; }
    public bool ConfirmationRequired { get; init; } = true;
}
public sealed record BiometricLabels(string? ButtonLabel, string? PromptMessage, string? SettingName);
public enum BiometricKeyState
{
    None,
    Available,
    Invalidated
}
public enum BiometricKeyLocation
{
    Unknown,
    Software,
    // TEE
    TrustedEnvironment,
    StrongBox
}
public enum BiometricSignStatus
{
    Succeeded,
    Canceled,
    Lockout,
    Failed,
    NoKey,
    KeyInvalidated
}
public sealed record BiometricSignResult(BiometricSignStatus Status, ReadOnlyMemory<byte> Signature);

public interface IBiometricAuthenticator
{
    BiometricAvailability CheckAvailability(BiometricMethod method);
    BiometricLabels? GetLabels(BiometricMethod method);
    TimeSpan? GetTimeSinceLastAuthentication(BiometricMethod method);
    bool OpenEnrollment(BiometricMethod method);
    ValueTask<BiometricAuthenticationResult> AuthenticateAsync(BiometricPromptText text, BiometricMethod method, CancellationToken cancellationToken = default);

    BiometricKeyState CheckSigningKey();
    BiometricKeyLocation? GetSigningKeyLocation();
    byte[]? CreateSigningKey();
    byte[]? GetSigningPublicKey();
    ValueTask<BiometricSignResult> SignAsync(byte[] data, BiometricPromptText text, CancellationToken cancellationToken = default);

    void DeleteSigningKey();
}
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
