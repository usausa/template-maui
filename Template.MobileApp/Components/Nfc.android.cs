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
        try
        {
            if (!nfc.IsConnected)
            {
                nfc.Connect();
            }

            var response = nfc.Transceive(command);
            return response ?? [];
        }
        catch (Java.IO.IOException)
        {
            // TagLostExceptionを含む。タグ喪失・接続失敗は応答なしとして扱い、購読側のシーケンスに例外を伝播させない
            return [];
        }
    }
}
#pragma warning restore CA1819

public sealed partial class NfcReader : Java.Lang.Object, NfcAdapter.IReaderCallback, Application.IActivityLifecycleCallbacks
{
    private NfcAdapter? nfcAdapter;

    private bool adapterResolved;

    private Activity? currentActivity;

    public partial bool IsSupported => ResolveAdapter() is not null;

    private NfcAdapter? ResolveAdapter()
    {
        if (!adapterResolved)
        {
            var nfcManager = (NfcManager?)Application.Context.GetSystemService(Context.NfcService);
            nfcAdapter = nfcManager?.DefaultAdapter;
            adapterResolved = true;
        }

        return nfcAdapter;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Enabled)
            {
                Enabled = false;
            }

            nfcAdapter?.Dispose();
            nfcAdapter = null;
        }
        base.Dispose(disposing);
    }

    private partial void Start()
    {
        var adapter = ResolveAdapter();
        if (adapter is null)
        {
            return;
        }

        currentActivity = ActivityResolver.CurrentActivity;
        currentActivity.Application!.RegisterActivityLifecycleCallbacks(this);

        adapter.EnableReaderMode(currentActivity, this, NfcReaderFlags.NfcF, null);
    }

    private partial void Stop()
    {
        nfcAdapter?.DisableReaderMode(currentActivity);
        currentActivity?.Application!.UnregisterActivityLifecycleCallbacks(this);
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
                try
                {
                    Detected?.Invoke(this, new NfcEventArgs(new AndroidNfcF(id, nfc)));
                }
                finally
                {
                    // タグはイベントハンドラ内でのみ有効な契約とし、検出ごとの接続リークを防ぐ
                    try
                    {
                        nfc.Close();
                    }
                    catch (Java.IO.IOException)
                    {
                    }

                    nfc.Dispose();
                }
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
