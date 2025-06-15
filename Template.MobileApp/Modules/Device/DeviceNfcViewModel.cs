namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components.Nfc;
using Template.MobileApp.Domain.FeliCa;

public sealed partial class DeviceNfcViewModel : AppViewModelBase
{
    private readonly INfcReader nfcReader;

    [ObservableProperty]
    public partial string Idm { get; set; } = string.Empty;

    [ObservableProperty]
    public partial SuicaAccessData? Access { get; set; }

    public ObservableCollection<SuicaLogData> Logs { get; } = new();

    public DeviceNfcViewModel(INfcReader nfcReader)
    {
        this.nfcReader = nfcReader;

        Disposables.Add(nfcReader.ObserveDetectedOnCurrentContext().Subscribe(OnReaderDetected));
    }

    public override void OnNavigatedTo(INavigationContext context)
    {
        nfcReader.Enabled = true;
    }

    public override void OnNavigatingFrom(INavigationContext context)
    {
        nfcReader.Enabled = false;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4()
    {
        Idm = string.Empty;
        Access = null;
        Logs.Clear();
        return Task.CompletedTask;
    }

    private void OnReaderDetected(NfcEventArgs args)
    {
        Logs.Clear();

        var nfcF = args.Tag;

        //var idm = nfcF.ExecutePolling(unchecked((short)0x0003));
        var idm = nfcF.ExecutePolling(unchecked((short)0xFFFF));
        if (idm.Length == 0)
        {
            return;
        }

        var block = new ReadBlock { BlockNo = 0 };
        if (!nfcF.ExecuteReadWoe(idm, 0x008B, block))
        {
            return;
        }

        var blocks1 = Enumerable.Range(0, 8).Select(x => new ReadBlock { BlockNo = (byte)x }).ToArray();
        var blocks2 = Enumerable.Range(8, 8).Select(x => new ReadBlock { BlockNo = (byte)x }).ToArray();
        var blocks3 = Enumerable.Range(16, 4).Select(x => new ReadBlock { BlockNo = (byte)x }).ToArray();
        if (!nfcF.ExecuteReadWoe(idm, 0x090F, blocks1) ||
            !nfcF.ExecuteReadWoe(idm, 0x090F, blocks2) ||
            !nfcF.ExecuteReadWoe(idm, 0x090F, blocks3))
        {
            return;
        }

        Idm = Convert.ToHexString(idm);
        Access = Suica.ConvertToAccessData(block.BlockData);
        Logs.AddRange(blocks1.Concat(blocks2).Concat(blocks3).Select(x => Suica.ConvertToLogData(x.BlockData)).OfType<SuicaLogData>().ToArray());
    }
}
