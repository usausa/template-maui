namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components;

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

        Disposables.Add(nfcReader.DetectedAsObservable().Select(ConvertResult).WhereNotNull().ObserveOnCurrentContext().Subscribe(x =>
        {
            Idm = x.Idm;
            Access = x.Access;
            Logs.Clear();
            Logs.AddRange(x.Logs);
        }));
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        nfcReader.Enabled = true;
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        nfcReader.Enabled = false;
        return Task.CompletedTask;
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

    private static (string Idm, SuicaAccessData Access, List<SuicaLogData> Logs)? ConvertResult(NfcEventArgs args)
    {
        var nfcF = args.Tag;

        //var idm = nfcF.ExecutePolling(unchecked((short)0x0003));
        var idm = nfcF.ExecutePolling(unchecked((short)0xFFFF));
        if (idm.Length == 0)
        {
            return null;
        }

        var block = new ReadBlock { BlockNo = 0 };
        if (!nfcF.ExecuteReadWoe(idm, 0x008B, block))
        {
            return null;
        }

        var blocks1 = Enumerable.Range(0, 8).Select(x => new ReadBlock { BlockNo = (byte)x }).ToArray();
        var blocks2 = Enumerable.Range(8, 8).Select(x => new ReadBlock { BlockNo = (byte)x }).ToArray();
        var blocks3 = Enumerable.Range(16, 4).Select(x => new ReadBlock { BlockNo = (byte)x }).ToArray();
        if (!nfcF.ExecuteReadWoe(idm, 0x090F, blocks1) ||
            !nfcF.ExecuteReadWoe(idm, 0x090F, blocks2) ||
            !nfcF.ExecuteReadWoe(idm, 0x090F, blocks3))
        {
            return null;
        }

        return (
            Convert.ToHexString(idm),
            new SuicaAccessData
            {
                Balance = SuicaLogic.ExtractAccessBalance(block.BlockData),
                TransactionId = SuicaLogic.ExtractAccessTransactionId(block.BlockData)
            },
            blocks1.Concat(blocks2).Concat(blocks3)
                .Where(static x => SuicaLogic.IsValidLog(x.BlockData))
                .Select(static x => new SuicaLogData
                {
                    Terminal = SuicaLogic.ExtractLogTerminal(x.BlockData),
                    Process = SuicaLogic.ExtractLogProcess(x.BlockData),
                    DateTime = SuicaLogic.ExtractLogDateTime(x.BlockData),
                    Balance = SuicaLogic.ExtractLogBalance(x.BlockData),
                    TransactionId = SuicaLogic.ExtractLogTransactionId(x.BlockData)
                })
                .ToList());
    }
}
