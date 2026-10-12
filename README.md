# Template project for MAUI

Template project for MAUI.

# Image

<p>
<img width="25%" src="Document/Images/UI_Login.png" />
<img width="25%" src="Document/Images/UI_Profile.png" />
<img width="25%" src="Document/Images/UI_Money.png" />

<img width="25%" src="Document/Images/UI_Super.png" />
<img width="25%" src="Document/Images/UI_Pos.png" />
<img width="25%" src="Document/Images/UI_KitDash.png" />

<img width="25%" src="Document/Images/UI_Shop.png" />
<img width="25%" src="Document/Images/UI_Item.png" />
<img width="25%" src="Document/Images/UI_Cart.png" />

<img width="25%" src="Document/Images/UI_Calendar.png" />
<img width="25%" src="Document/Images/UI_Schedule.png" />
<img width="25%" src="Document/Images/UI_Graph.png" />

<img width="25%" src="Document/Images/UI_Mail.png" />
<img width="25%" src="Document/Images/UI_Chat.png" />
<img width="25%" src="Document/Images/UI_Timeline.png" />

<img width="25%" src="Document/Images/UI_Grid.png" />
<img width="25%" src="Document/Images/UI_Visit.png" />
<img width="25%" src="Document/Images/Device_WiFi.png" />

<img width="25%" src="Document/Images/Device_NFC.png" />
<img width="25%" src="Document/Images/Device_BLE.png" />
<img width="25%" src="Document/Images/Device_Activity.png" />

<img width="25%" src="Document/Images/UI_Stream.png" />
<img width="25%" src="Document/Images/UI_Deck.png" />
<img width="25%" src="Document/Images/UI_Gauge.png" />

<img width="25%" src="Document/Images/UI_Meter.png" />
<img width="25%" src="Document/Images/UI_Load.png" />
<img width="25%" src="Document/Images/UI_Mixier.png" />

<img width="25%" src="Document/Images/Sample_CV.png" />
<img width="25%" src="Document/Images/UI_TreeMap.png" />
<img width="25%" src="Document/Images/View_Chart.png" />

<img width="25%" src="Document/Images/App_Calc.png" />
<img width="25%" src="Document/Images/App_Game.png" />
<img width="25%" src="Document/Images/UI_Radar.png" />

<img width="25%" src="Document/Images/UI_Flight.png" />
<img width="25%" src="Document/Images/UI_Tactical.png" />
<img width="25%" src="Document/Images/UI_Telemetry.png" />

<img width="25%" src="Document/Images/UI_Monster.png" />
<img width="25%" src="Document/Images/UI_Character.png" />
<img width="25%" src="Document/Images/UI_Social.png" />
</p>

# Implement

| Category | Feature |
| --- | --- |
| Basic | Typography / Style / Font / Converter / Behavior / Dialog / Validation / Localization / Setting |
| Navigation | Basic / Stack / Wizard / Shared / Initialize / Cancel / Dialog |
| Device | Info / Status / Sensor / Location / QR Scan(Android AI) / QR Display / Camera / OCR(Android AI) / WiFi / BLE / Bluetooth Serial / NFC / Audio / Activity / Communication / Screen / Vibrate / Feed / LED / Speak / Recognize / Local notification |
| Data | SQLite |
| Network | Web API(Data CRUD / JWT Authentication) / Storage / Realtime(SignalR) / Push(SignalR) / gRPC(Chat) / SFTP / Telemetry(OpenTelemetry) |
| View | Layout / State / Border / Shadow / Toolkit / Custom / Bottom sheet(SfBottomSheet, custom) / Drawer(SfNavigationDrawer, custom) / Animation / Easing / Effect / Drag & Drop / Lottie / SVG / Graphics / Drawing / Chart / SfChart |
| Sample | Web view / HybridWebView / Map / Map2 / Media play / Markdown / PDF reader / Object detection(Local) / Object, Tag, People, OCR(Azure AI Vision) / Chat(Ollama) / Crop |
| App | Calculator / Timer / ToDo / Sudoku / 2048 / Minesweeper |
| UI | Profile / Login / Money / Super / POS / Shop / Item / Cart / Grid(ClamGrid) / Visit / Calendar(ClamCalendar) / Schedule / Mail / Chat / Weather / News / Timeline / Kit(Dashboard, Notification, Setting, Onboarding, Tracking) / Stream / Dock / Graph / Graph2 / Load / Gauge / Meter / Mixer / TreeMap / Radar / Wheel / Monster / Character / Social / Flight / Tactical / Telemetry / Energy |

# TODO

| Category | Feature |
| --- | --- |
| Device | Background task(WorkManager) |
| Network | Tanking send |
| Device | Push(FCM) |
| Device | Biometric |
| Device | Passkey (Credential Manager + WebAuthn, pending decision) |
| Decision | Pending decisions (CoreCLR runtime) |
| Device | Health Connect (candidate, pending decision) |
| Sample | AI tool calling (candidate, pending decision) |

## Pending

| Item | Waiting for |
| --- | --- |
| Global xmlns (`http://schemas.microsoft.com/dotnet/maui/global`) for XAML | ReSharper support (build passes, inspectcode cannot resolve) |
| CoreCLR runtime (`UseMonoRuntime=false`) | .NET 11 (Shiny `[Export]` startup crash: dotnet/android#10996) |

# My Libraries

| Library | Description |
| --- | --- |
| [Usa.Smart.Mvvm](https://github.com/usausa/Smart-Net-Mvvm) | MVVM |
| [Usa.Smart.Reactive](https://github.com/usausa/Smart-Net-Reactive) | Reactive Extensions helpers |
| [Usa.Smart.Results](https://github.com/usausa/Smart-Net-Results) | Result pattern |
| [Usa.Smart.Navigation](https://github.com/usausa/Smart-Net-Navigation) | Navigation |
| [Usa.Smart.Maui](https://github.com/usausa/Smart-Net-Maui) | MAUI MVVM helpers |
| [Usa.Smart.Data.Accessor](https://github.com/usausa/Smart-Net-Data-Accessor) | Source generator based data accessor |
| [Usa.Smart.Mapper](https://github.com/usausa/Smart-Net-Mapper) | Object mapper |
| [Components.Maui](https://github.com/usausa/MauiComponents) | MAUI components(dialog, popup, screen, etc.) |
| [Rester](https://github.com/usausa/Rester) | HttpClient extensions |
| [ClamGrid](https://github.com/usausa/clam-grid) | SkiaSharp grid view |
| [ClamCalendar](https://github.com/usausa/clam-calendar) | SkiaSharp calendar view |
| [ClamGauge](https://github.com/usausa/clam-gauge) | SkiaSharp gauge view |
| [SkiaExtension](https://github.com/usausa/skia-extension) | SkiaSharp extensions |
| [Mofucat.ReactiveHub](https://github.com/usausa/mofucat-reactive-hub) | Reactive SignalR hub connection |
| [BunnyTail.DependencyInjection](https://github.com/usausa/bunnytail-dependency-injection) | Source generator based DI |
| [BunnyTail.EmbeddedBuildProperty](https://github.com/usausa/bunnytail-embedded-build-property) | Embedded build property generator |
| [BunnyTail.MemberAccessor](https://github.com/usausa/bunnytail-member-accessor) | Member accessor generator |
| [BunnyTail.XamlProperty.Maui](https://github.com/usausa/bunnytail-xaml-property) | BindableProperty generator |
