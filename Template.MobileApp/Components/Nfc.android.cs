namespace Template.MobileApp.Components;

using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;

#pragma warning disable CA1819
public sealed class AndroidNfcF : INfc
{
    public byte[] Id { get; }

    private readonly NfcF nfc;

    public AndroidNfcF(byte[] id, NfcF nfc)
    {
        Id = id;
        this.nfc = nfc;
    }

    public void SetTimeout(int timeout)
    {
        nfc.Timeout = timeout;
    }

    public byte[] Access(byte[] command)
    {
        if (!nfc.IsConnected)
        {
            nfc.Connect();
        }

        try
        {
            var response = nfc.Transceive(command);
            return response ?? [];
        }
        catch (TagLostException)
        {
            return [];
        }
    }
}
#pragma warning restore CA1819

public sealed partial class NfcReader : Java.Lang.Object, NfcAdapter.IReaderCallback, Application.IActivityLifecycleCallbacks
{
    private NfcAdapter? nfcAdapter;

    private Activity? currentActivity;

    private partial void Start()
    {
        if (nfcAdapter is null)
        {
            var nfcManager = (NfcManager)Application.Context.GetSystemService(Context.NfcService)!;
            nfcAdapter = nfcManager.DefaultAdapter!;
        }

        currentActivity = ActivityResolver.CurrentActivity;
        currentActivity.Application!.RegisterActivityLifecycleCallbacks(this);

        nfcAdapter.EnableReaderMode(currentActivity, this, NfcReaderFlags.NfcF, null);
    }

    private partial void Stop()
    {
        nfcAdapter?.DisableReaderMode(currentActivity);
        currentActivity = null;
    }

    private void Pause()
    {
        if (!Enabled)
        {
            return;
        }

        nfcAdapter?.DisableReaderMode(currentActivity);
    }

    private void Resume()
    {
        if (!Enabled)
        {
            return;
        }

        nfcAdapter?.EnableReaderMode(currentActivity, this, NfcReaderFlags.NfcF, null);
    }

    // --------------------------------------------------------------------------------
    // IReaderCallback
    // --------------------------------------------------------------------------------

    public void OnTagDiscovered(Tag? tag)
    {
#pragma warning disable CA1031
        try
        {
            if (tag is null)
            {
                return;
            }

            var list = tag.GetTechList()!;
            if (list.Any(x => x == "android.nfc.tech.NfcF"))
            {
                var id = tag.GetId()!;
                var nfc = NfcF.Get(tag)!;
                Detected?.Invoke(this, new NfcEventArgs(new AndroidNfcF(id, nfc)));
            }
        }
        catch (TagLostException)
        {
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine(e);
        }
#pragma warning restore CA1031
    }

    // --------------------------------------------------------------------------------
    // IActivityLifecycleCallbacks
    // --------------------------------------------------------------------------------

    public void OnActivityCreated(Activity activity, Bundle? savedInstanceState)
    {
    }

    public void OnActivityDestroyed(Activity activity)
    {
    }

    public void OnActivityPaused(Activity activity)
    {
        Pause();
    }

    public void OnActivityResumed(Activity activity)
    {
        Resume();
    }

    public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
    {
    }

    public void OnActivityStarted(Activity activity)
    {
    }

    public void OnActivityStopped(Activity activity)
    {
    }
}
