namespace Template.MobileApp.Modules.Device;

using Shiny.BluetoothLE;

public sealed class DeviceBleScanViewModel : AppViewModelBase
{
    public ObservableCollection<SwitchBotTemperature> Devices { get; } = new();

    private IDisposable? scanning;

    public DeviceBleScanViewModel(IBleManager bleManager)
    {
        Disposables.Add(Observable.Timer(TimeSpan.Zero, TimeSpan.FromMinutes(1))
            .ObserveOnCurrentContext()
            .Subscribe(_ =>
            {
                scanning?.Dispose();
                scanning = bleManager.Scan()
                    .Select(ConvertData)
                    .WhereNotNull()
                    .ObserveOnCurrentContext()
                    .Subscribe(UpdateList);
            }));
        Disposables.Add(new DelegateDisposable(() =>
        {
            scanning?.Dispose();
            scanning = null;
        }));
    }

    private static SwitchBotTemperature? ConvertData(ScanResult result)
    {
        if ((result.AdvertisementData.ManufacturerData is not null) &&
            SwitchBotLogic.IsTargetCompanyId(result.AdvertisementData.ManufacturerData.CompanyId) &&
            (result.AdvertisementData.ServiceData is not null))
        {
            var sd = result.AdvertisementData.ServiceData.FirstOrDefault(static x => SwitchBotLogic.IsScanResponse(x.Uuid));
            if ((sd is not null) && (sd.Data.Length > 0) && SwitchBotLogic.IsTemperatureDevice(sd.Data[0]))
            {
                var buffer = result.AdvertisementData.ManufacturerData.Data;
                return new SwitchBotTemperature
                {
                    DeviceId = SwitchBotLogic.ExtractDeviceId(buffer),
                    Timestamp = DateTime.Now,
                    Rssi = result.Rssi,
                    Temperature = SwitchBotLogic.ExtractTemperature(buffer),
                    Humidity = SwitchBotLogic.ExtractHumidity(buffer),
                    Co2 = SwitchBotLogic.ExtractCo2(buffer)
                };
            }
        }

        return null;
    }

    private void UpdateList(SwitchBotTemperature data)
    {
        var current = Devices.FirstOrDefault(data, static (x, s) => x.DeviceId == s.DeviceId);
        if (current == null)
        {
            Devices.Add(data);
        }
        else
        {
            data.CopyTo(current);
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.DeviceMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
