namespace Template.MobileApp.Modules.Device;

using Template.MobileApp.Components;

public sealed partial class DeviceNfcViewModel : AppViewModelBase
{
    private readonly ILogger<DeviceNfcViewModel> log;

    private readonly IDialog dialog;

    private readonly INfcReader nfcReader;

    [ObservableProperty]
    public partial string Idm { get; set; } = string.Empty;

    [ObservableProperty]
    public partial SuicaAccessData? Access { get; set; }

    public ObservableCollection<SuicaLogData> Logs { get; } = [];

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public DeviceNfcViewModel(
        ILogger<DeviceNfcViewModel> log,
        IDialog dialog,
        INfcReader nfcReader)
    {
        this.log = log;
        this.dialog = dialog;
        this.nfcReader = nfcReader;

        Disposables.Add(nfcReader.DetectedAsObservable().Select(ConvertResult).WhereNotNull().ObserveOnCurrentContext().Subscribe(
            x =>
            {
                Idm = x.Idm;
                Access = x.Access;
                Logs.Clear();
                Logs.AddRange(x.Logs);
            },
            log.WarnNfcReadError));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (nfcReader.IsSupported)
        {
            nfcReader.Enabled = true;
        }
        else
        {
            await dialog.InformationAsync("NFC is not supported on this device.");
        }
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

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private (string Idm, SuicaAccessData Access, List<SuicaLogData> Logs)? ConvertResult(NfcEventArgs args)
    {
        try
        {
            return SuicaHelper.ParseTag(args.Tag);
        }
        catch (Exception ex) when (ex is ArgumentException or OverflowException or IndexOutOfRangeException)
        {
            log.WarnNfcReadError(ex);
            return null;
        }
    }
}
