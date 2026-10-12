namespace Template.MobileApp.Modules.UI;

public sealed class UIHomeViewModel : AppViewModelBase
{
    public SmartHome Home { get; } = new(HomeSample.LoadRooms());

    public IReadOnlyList<Selectable<HomeRoom>> Rooms { get; }

    public IObserveCommand BackCommand { get; }
    public IObserveCommand RoomCommand { get; }
    public IObserveCommand ToggleCommand { get; }
    public IObserveCommand DownCommand { get; }
    public IObserveCommand UpCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIHomeViewModel()
    {
        Rooms = Home.Rooms.Select(x => new Selectable<HomeRoom>(x) { IsSelected = x == Home.Room }).ToArray();

        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);
        RoomCommand = MakeDelegateCommand<Selectable<HomeRoom>>(SelectRoom);
        ToggleCommand = MakeDelegateCommand<HomeDevice>(static x => x.Toggle());
        DownCommand = MakeDelegateCommand(() => Home.ChangeTemperature(-SmartHome.TemperatureStep));
        UpCommand = MakeDelegateCommand(() => Home.ChangeTemperature(SmartHome.TemperatureStep));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void SelectRoom(Selectable<HomeRoom> room)
    {
        Home.Select(room.Item);
        foreach (var x in Rooms)
        {
            x.IsSelected = x == room;
        }
    }
}
