namespace Template.MobileApp.Components;

using Android.App;
using Android.Content;
using Android.Hardware.Biometrics;
using Android.OS;
using Android.Runtime;
using Android.Security.Keystore;

using Java.Security;
using Java.Security.Spec;

using AndroidSettings = Android.Provider.Settings;

// Android の BiometricPrompt / BiometricManager と Android Keystore を直接使う (minSdk 30 なので AndroidX は要らない)。ダイアログはシステムが出す
public sealed partial class BiometricAuthenticator
{
    private const string KeyStoreName = "AndroidKeyStore";

    private const string SigningKeyAlias = "BiometricSigningKey";

    private const string SignatureAlgorithm = "SHA256withECDSA";

    private static partial BiometricAvailability PlatformCheckAvailability(BiometricMethod method)
    {
        using var manager = GetManager();
        return manager.CanAuthenticate(ToAuthenticators(method)) switch
        {
            BiometricCode.Success => BiometricAvailability.Available,
            BiometricCode.ErrorNoHardware => BiometricAvailability.NoHardware,
            BiometricCode.ErrorNoneEnrolled => BiometricAvailability.NoneEnrolled,
            _ => BiometricAvailability.Unavailable
        };
    }

    private static partial BiometricLabels? PlatformGetLabels(BiometricMethod method)
    {
        BiometricLabels? labels = null;
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            using var manager = GetManager();
            using var strings = manager.GetStrings(ToAuthenticators(method));
            labels = new BiometricLabels(strings.ButtonLabel, strings.PromptMessage, strings.SettingName);
        }

        return labels;
    }

    // 記録は起動からの時間 (elapsedRealtime)
    private static partial TimeSpan? PlatformGetTimeSinceLastAuthentication(BiometricMethod method)
    {
        TimeSpan? elapsed = null;
        if (OperatingSystem.IsAndroidVersionAtLeast(35) && (method != BiometricMethod.WeakBiometric))
        {
            using var manager = GetManager();
            var last = manager.GetLastAuthenticationTime((BiometricManagerAuthenticators)ToAuthenticators(method));
            if (last != BiometricManager.BiometricNoAuthentication)
            {
                elapsed = TimeSpan.FromMilliseconds(SystemClock.ElapsedRealtime() - last);
            }
        }

        return elapsed;
    }

    private static partial bool PlatformOpenEnrollment(BiometricMethod method)
    {
        using var intent = new Intent(AndroidSettings.ActionBiometricEnroll);
        intent.PutExtra(AndroidSettings.ExtraBiometricAuthenticatorsAllowed, ToAuthenticators(method));
        intent.AddFlags(ActivityFlags.NewTask);
        try
        {
            Application.Context.StartActivity(intent);
            return true;
        }
        catch (ActivityNotFoundException)
        {
            return false;
        }
    }

    private static partial ValueTask<BiometricAuthenticationResult> PlatformAuthenticateAsync(BiometricPromptText text, BiometricMethod method, CancellationToken cancellationToken) =>
        PromptAsync(text, method, null, cancellationToken);

    //--------------------------------------------------------------------------------
    // Signing key
    //--------------------------------------------------------------------------------

    private static partial BiometricKeyState PlatformCheckSigningKey()
    {
        using var keyStore = LoadKeyStore();
        using var key = keyStore.GetKey(SigningKeyAlias, null)?.JavaCast<IPrivateKey>();
        using var signature = Signature.GetInstance(SignatureAlgorithm)!;
        return key is null
            ? BiometricKeyState.None
            : TryInitSign(signature, key) ? BiometricKeyState.Available : BiometricKeyState.Invalidated;
    }

    private static partial BiometricKeyLocation? PlatformGetSigningKeyLocation()
    {
        using var keyStore = LoadKeyStore();
        using var key = keyStore.GetKey(SigningKeyAlias, null)?.JavaCast<IPrivateKey>();
        BiometricKeyLocation? location = null;
        if (key is not null)
        {
            using var factory = KeyFactory.GetInstance(key.Algorithm, KeyStoreName)!;
            using var keyInfoClass = Java.Lang.Class.FromType(typeof(KeyInfo));
            using var info = factory.GetKeySpec(key, keyInfoClass)?.JavaCast<KeyInfo>();
            location = info is null ? BiometricKeyLocation.Unknown : ToLocation(info);
        }

        return location;
    }

    // 使うたびに本人確認を求める鍵は、強い生体認証を使える (生体情報が登録されている) ときだけ作れる
    private static partial byte[]? PlatformCreateSigningKey() =>
        PlatformCheckAvailability(BiometricMethod.Biometric) == BiometricAvailability.Available ? GenerateSigningKey() : null;

    private static partial byte[]? PlatformGetSigningPublicKey()
    {
        using var keyStore = LoadKeyStore();
        using var certificate = keyStore.GetCertificate(SigningKeyAlias);
        using var publicKey = certificate?.PublicKey;
        return publicKey?.GetEncoded();
    }

    // 鍵と結び付けた本人確認に成功すると、初期化した Signature で 1 回だけ署名できる
    private static async partial ValueTask<BiometricSignResult> PlatformSignAsync(byte[] data, BiometricPromptText text, CancellationToken cancellationToken)
    {
        using var keyStore = LoadKeyStore();
        using var key = keyStore.GetKey(SigningKeyAlias, null)?.JavaCast<IPrivateKey>();
        using var signature = Signature.GetInstance(SignatureAlgorithm)!;

        BiometricSignResult result;
        if (key is null)
        {
            result = new BiometricSignResult(BiometricSignStatus.NoKey, ReadOnlyMemory<byte>.Empty);
        }
        else if (!TryInitSign(signature, key))
        {
            result = new BiometricSignResult(BiometricSignStatus.KeyInvalidated, ReadOnlyMemory<byte>.Empty);
        }
        else
        {
            using var crypto = new BiometricPrompt.CryptoObject(signature);
            var authentication = await PromptAsync(text, BiometricMethod.Biometric, crypto, cancellationToken).ConfigureAwait(true);
            if (authentication.Result == BiometricResult.Succeeded)
            {
                signature.Update(data);
                result = new BiometricSignResult(BiometricSignStatus.Succeeded, signature.Sign());
            }
            else
            {
                result = new BiometricSignResult(ToSignStatus(authentication.Result), ReadOnlyMemory<byte>.Empty);
            }
        }

        return result;
    }

    private static partial void PlatformDeleteSigningKey()
    {
        using var keyStore = LoadKeyStore();
        if (keyStore.ContainsAlias(SigningKeyAlias))
        {
            keyStore.DeleteEntry(SigningKeyAlias);
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static BiometricManager GetManager() =>
        (BiometricManager)Application.Context.GetSystemService(Context.BiometricService)!;

    // crypto を付けると、その鍵の操作を認める本人確認になる
    private static async ValueTask<BiometricAuthenticationResult> PromptAsync(BiometricPromptText text, BiometricMethod method, BiometricPrompt.CryptoObject? crypto, CancellationToken cancellationToken)
    {
        var context = Application.Context;
        var executor = context.MainExecutor!;
        var completion = new TaskCompletionSource<BiometricAuthenticationResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var builder = new BiometricPrompt.Builder(context);
        builder.SetTitle(text.Title);
        if (text.Subtitle is not null)
        {
            builder.SetSubtitle(text.Subtitle);
        }

        if (text.Description is not null)
        {
            builder.SetDescription(text.Description);
        }

        builder.SetConfirmationRequired(text.ConfirmationRequired);
        builder.SetAllowedAuthenticators(ToAuthenticators(method));
        // 画面ロックを使わないときは取り消しのボタンが要る (使うときは付けられない)
        using var cancelListener = new CancelListener(completion);
        if (method is BiometricMethod.Biometric or BiometricMethod.WeakBiometric)
        {
            builder.SetNegativeButton(text.CancelText, executor, cancelListener);
        }

        using var prompt = builder.Build();
        using var signal = new CancellationSignal();
        using var callback = new AuthenticationCallback(completion);
        await using var registration = cancellationToken.Register(signal.Cancel);
        if (crypto is null)
        {
            prompt.Authenticate(signal, executor, callback);
        }
        else
        {
            prompt.Authenticate(crypto, signal, executor, callback);
        }

        return await completion.Task.ConfigureAwait(true);
    }

    // 定数は列挙 ([Flags] なし)、API の引数は int
    private static int ToAuthenticators(BiometricMethod method) => method switch
    {
        BiometricMethod.Biometric => (int)BiometricManagerAuthenticators.BiometricStrong,
        BiometricMethod.WeakBiometric => (int)BiometricManagerAuthenticators.BiometricWeak,
        BiometricMethod.DeviceCredential => (int)BiometricManagerAuthenticators.DeviceCredential,
        _ => (int)BiometricManagerAuthenticators.BiometricStrong | (int)BiometricManagerAuthenticators.DeviceCredential
    };

    private static BiometricAuthenticationType ToAuthenticationType(int? type) => type switch
    {
        (int)AuthenticationResultType.Biometric => BiometricAuthenticationType.Biometric,
        (int)AuthenticationResultType.DeviceCredential => BiometricAuthenticationType.DeviceCredential,
        _ => BiometricAuthenticationType.Unknown
    };

    private static BiometricSignStatus ToSignStatus(BiometricResult result) => result switch
    {
        BiometricResult.Succeeded => BiometricSignStatus.Succeeded,
        BiometricResult.Canceled => BiometricSignStatus.Canceled,
        BiometricResult.Lockout => BiometricSignStatus.Lockout,
        _ => BiometricSignStatus.Failed
    };

    // Android 12 から置き場所を区別できる (それより前は安全な領域かどうかだけ)
    private static BiometricKeyLocation ToLocation(KeyInfo info) =>
        OperatingSystem.IsAndroidVersionAtLeast(31)
            ? info.SecurityLevel switch
            {
                (int)KeyStoreSecurityLevel.Strongbox => BiometricKeyLocation.StrongBox,
                (int)KeyStoreSecurityLevel.TrustedEnvironment => BiometricKeyLocation.TrustedEnvironment,
                (int)KeyStoreSecurityLevel.Software => BiometricKeyLocation.Software,
                _ => BiometricKeyLocation.Unknown
            }
            : info.IsInsideSecureHardware ? BiometricKeyLocation.TrustedEnvironment : BiometricKeyLocation.Software;

    private static KeyStore LoadKeyStore()
    {
        var keyStore = KeyStore.GetInstance(KeyStoreName)!;
        keyStore.Load(null);
        return keyStore;
    }

    // StrongBox が無ければ TEE に作る
    private static byte[]? GenerateSigningKey()
    {
        try
        {
            return GenerateSigningKey(true);
        }
        catch (StrongBoxUnavailableException)
        {
            return GenerateSigningKey(false);
        }
    }

    // P-256、SHA-256。使うたびに強い生体認証を求め、生体情報の登録が変わると使えなくなる
    private static byte[]? GenerateSigningKey(bool strongBox)
    {
        using var curve = new ECGenParameterSpec("secp256r1");
        using var builder = new KeyGenParameterSpec.Builder(SigningKeyAlias, KeyStorePurpose.Sign);
        builder.SetAlgorithmParameterSpec(curve);
        builder.SetDigests(KeyProperties.DigestSha256);
        builder.SetUserAuthenticationRequired(true);
        builder.SetUserAuthenticationParameters(0, (int)KeyPropertiesAuthType.BiometricStrong);
        builder.SetInvalidatedByBiometricEnrollment(true);
        builder.SetIsStrongBoxBacked(strongBox);
        using var spec = builder.Build();

        using var generator = KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmEc, KeyStoreName)!;
        try
        {
            generator.Initialize(spec);
            using var pair = generator.GenerateKeyPair();
            using var publicKey = pair?.Public;
            return publicKey?.GetEncoded();
        }
        catch (Java.Lang.Exception e) when (e is InvalidAlgorithmParameterException or (ProviderException and not StrongBoxUnavailableException))
        {
            return null;
        }
    }

    // 生体情報の登録が変わった鍵は、署名の初期化で使えないと分かる
    private static bool TryInitSign(Signature signature, IPrivateKey key)
    {
        try
        {
            signature.InitSign(key);
            return true;
        }
        catch (KeyPermanentlyInvalidatedException)
        {
            return false;
        }
    }

    // 取り消しのボタン
    private sealed class CancelListener : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        private readonly TaskCompletionSource<BiometricAuthenticationResult> completion;

        public CancelListener(TaskCompletionSource<BiometricAuthenticationResult> completion)
        {
            this.completion = completion;
        }

        public void OnClick(IDialogInterface? dialog, int which) =>
            completion.TrySetResult(new BiometricAuthenticationResult(BiometricResult.Canceled, BiometricAuthenticationType.Unknown));
    }

    // 1 回の不一致ではダイアログが残るので扱わない (続くと OnAuthenticationError の Lockout になる)
    private sealed class AuthenticationCallback : BiometricPrompt.AuthenticationCallback
    {
        private readonly TaskCompletionSource<BiometricAuthenticationResult> completion;

        public AuthenticationCallback(TaskCompletionSource<BiometricAuthenticationResult> completion)
        {
            this.completion = completion;
        }

        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult? result) =>
            completion.TrySetResult(new BiometricAuthenticationResult(BiometricResult.Succeeded, ToAuthenticationType(result?.AuthenticationType)));

        public override void OnAuthenticationError(BiometricErrorCode errorCode, Java.Lang.ICharSequence? errString) =>
            completion.TrySetResult(new BiometricAuthenticationResult(
                errorCode switch
                {
                    BiometricErrorCode.UserCanceled or BiometricErrorCode.Canceled => BiometricResult.Canceled,
                    BiometricErrorCode.Lockout or BiometricErrorCode.LockoutPermanent => BiometricResult.Lockout,
                    _ => BiometricResult.Failed
                },
                BiometricAuthenticationType.Unknown));
    }
}
