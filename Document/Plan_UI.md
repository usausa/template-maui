# 🗂 UI Implementation Plan

Checklist for the screens proposed in [Proposal_UI](Proposal_UI.md).

## 🏷 Legend

| Mark | Meaning |
| --- | --- |
| 🆕 | New file |
| ✏️ | Change an existing file |
| 🗑 | Delete |
| 📦 | `Models/Sample` |
| 🎨 | View / ViewModel |
| 📐 | `Graphics` (Drawing / Scene) |
| 🧩 | `Controls` / `Behaviors` |
| 📄 | Document |

Status: ⬜ todo / 🔄 doing / ✅ done

## 📊 Progress

| Phase | Scope | Status |
| --- | --- | --- |
| 🧱 P0 | Menu foundation | ✅ |
| 🧰 P1 | Shared parts | ✅ |
| 🅰 P2 | Primary screens (6) | ⬜ |
| 🅱 P3 | Secondary screens (7) | ⬜ |
| 📄 P4 | Document | ⬜ |
| 🧹 P5 | Cleanup | ⬜ |

# 🧱 P0 Menu foundation

Lands first. Every screen below can then be added one at a time.

- [x] ✏️ `Modules/ViewId.cs` - add `UIMenu3`
- [x] ✏️ `Modules/ViewId.cs` - add 13 ids at the end of the UI section
    - `UITransit` `UITransitTimetable` `UISeat` `UIGantt` `UIKanban` `UITicket`
    - `UIMusic` `UIGallery` `UIHealth` `UINavi` `UIScore` `UIFiler` `UITrade`
- [x] ✏️ `Markup/AppIcons.cs` - add 10 icons (`MenuSize` / `Colors.White`)
    - `DirectionsTransit` `EventSeat` `ViewKanban` `ConfirmationNumber` `MusicNote`
    - `PhotoLibrary` `Bedtime` `SportsBaseball` `Folder` `CandlestickChart`
    - `ViewQuilt` `ViewTimeline` `Navigation` already exist
- [x] 🎨 🆕 `Modules/UI/UIMenu3View.xaml` / `.xaml.cs` / `UIMenu3ViewModel.cs`
    - 9 rows x 2 columns, 12 entries and 6 disabled filler buttons, same as `AppMenuView`
    - the 12 entries start with `IsEnabled="False"`, each screen drops it when it lands
- [x] ✏️ `Modules/Main/MenuView.xaml` - row 4 and row 5 only, 9 rows kept
    - row 4 : `View` (Column 0) / `UI 1` (Column 1)
    - row 5 : `UI 2` (Column 0) / `UI 3` (Column 1)
- [x] ✏️ `Modules/UI/UIMenu2View.xaml` - `Function4Text` `UI 1` to `UI 3`
- [x] ✏️ `Modules/UI/UIMenu2ViewModel.cs` - `OnNotifyFunction4` to `ViewId.UIMenu3`
    - chain becomes UI 1 to UI 2 to UI 3 to UI 1, `UIMenu1` needs no change

## 🖼 Menu icons

| Entry | Icon |
| --- | --- |
| UI 3 | `ViewQuilt` |
| Transit | `DirectionsTransit` |
| Seat | `EventSeat` |
| Gantt | `ViewTimeline` |
| Kanban | `ViewKanban` |
| Ticket | `ConfirmationNumber` |
| Music | `MusicNote` |
| Gallery | `PhotoLibrary` |
| Health | `Bedtime` |
| Navi | `Navigation` |
| Score | `SportsBaseball` |
| Filer | `Folder` |
| Trade | `CandlestickChart` |

# 🧰 P1 Shared parts

Each item stands on its own and builds independently.

- [x] 🧩 🆕 `Behaviors/GestureOption.cs` - pinch and pan
    - attached : `PinchEnabled` `PanEnabled` `MinScale` `MaxScale` `ZoomScale` `ZoomOffset`
    - named `ZoomScale`, not `Scale`, because an attached `Scale` reports the same
      `PropertyName` as `VisualElement.Scale`
    - the pinch keeps the content under the origin in place, the offset is clamped to the view
    - built on `PinchGestureRecognizer` and `PanGestureRecognizer`
    - kept out of `TouchOption`, which is press feedback and carries android / ios custom mappers
    - used by Seat / Gallery
- [x] 🧩 ✏️ `Behaviors/Scroll.cs` - add `SyncTarget`
    - mirrors `ScrollX` / `ScrollY` between two scroll views, with a re-entrancy guard
    - placed here because `HideOnScrollTarget` already links one element to another
    - used by Gantt / Score
- [x] 📐 ✏️ `Graphics/Drawing/DrawingObject.cs` and `DrawingControl.cs` - add `IZoomDrawing`
    - `OnZoom(float scale, PointF offset)`, the absolute transform, so the drawing keeps no gesture state
    - `DrawingControl` watches the attached `GestureOption` values and pushes them down
    - `IInteractiveDrawing` is single pointer only, so there is no path for a transform today
    - used by Seat
- [x] 📐 ✏️ `Graphics/Drawing/ChartDrawing.cs` - add `UpdateCandle` / `AddCandle`
    - `ShowCandle` calls `Start`, which resets `progress` and `SelectedIndex`
    - a per tick call would replay the entrance animation and clear the tap selection
    - `UpdateCandle` replaces the newest bar, `AddCandle` appends one, neither animates
    - used by Trade
- [x] 🧩 🆕 `Controls/PerforatedBorder.cs` - notched ticket shape
    - notch positions depend on the content height, so the shape is rebuilt on `SizeChanged`
    - `Border` subclass, builds its own `StrokeShape` from `Radius` / `NotchRadius` / `NotchOffset`
    - used by Ticket

# 🅰 P2 Primary screens

## 🚆 Transit / TransitTimetable

- [ ] 📦 🆕 `Models/Sample/TransitRoute.cs`
    - enum `TrainKind` `TransitSegmentKind`
    - record `TransitStop`
    - `TransitSegment` (ObservableObject, `IsExpanded`) / `TransitRoute` / `TransitSample`
- [ ] 📦 🆕 `Models/Sample/StationTimetable.cs`
    - enum `StationDirection` `StationDayType`
    - record `StationDeparture`
    - `StationHour` / `StationTimetable` / `StationTimetableSample`
- [ ] 🎨 🆕 `Modules/UI/UITransitContext.cs`
- [ ] 🎨 🆕 `Modules/UI/UITransitView.xaml` / `.xaml.cs` / `UITransitViewModel.cs`
- [ ] 🎨 🆕 `Modules/UI/UITransitTimetableView.xaml` / `.xaml.cs` / `UITransitTimetableViewModel.cs`

Points

- route tabs, previous / next departure, fare and duration summary
- rail is a `BoxView` whose `HeightRequest` is proportional to the segment duration
- intermediate stops open and close through `IsExpanded` and a height animation, no `GraphicsView`
- timetable row is an hour label plus a wrapping `FlexLayout` of minute chips inside a `CollectionView`
- Function : `Back` `条件` `逆方向` `時刻表` / `Back` `方面` `種別` `現在`

## 💺 Seat

- [ ] 📦 🆕 `Models/Sample/SeatMap.cs`
    - enum `SeatKind` `SeatStatus`
    - `SeatPlace` (ObservableObject) / `SeatScreening`
    - `SeatMap` - hit test, selection limit, total price
    - `SeatSample`
- [ ] 📐 🆕 `Graphics/Drawing/SeatMapDrawing.cs`
- [ ] 🎨 🆕 `Modules/UI/UISeatView.xaml` / `.xaml.cs` / `UISeatViewModel.cs`

Points

- 16 x 16 seats, so a `Border` per seat is not viable, `GraphicsView` from the start
- the minimap is drawn inside the same drawing object
- needs 🧰 `GestureOption` and `IZoomDrawing`
- Function : `Back` `回次` `全体` `確定`

## 📊 Gantt

- [ ] 📦 🆕 `Models/Sample/ProjectSchedule.cs`
    - enum `ProjectTaskState` `GanttScale`
    - `ProjectTask`
    - `ProjectSchedule` - range, progress, delayed count
    - `ProjectSample`
- [ ] 📐 🆕 `Graphics/Drawing/GanttDrawing.cs`
- [ ] 🎨 🆕 `Modules/UI/UIGanttView.xaml` / `.xaml.cs` / `UIGanttViewModel.cs`

Points

- the left task column is a `CollectionView`, the right side is a `GraphicsView` in a horizontal `ScrollView`
- vertical scroll is linked with 🧰 `Scroll.SyncTarget`
- `ClamGridView` is not used - it defines columns through `GridColumn` and is a data grid, not a time axis
- the drawing covers date header, weekend background, today line, bars with progress fill, dependency links, milestones
- Function : `Back` `絞込` `追加` `今日`

## 🗂 Kanban

- [ ] 📦 🆕 `Models/Sample/SprintBoard.cs`
    - enum `CardLabel` `LaneKind`
    - `BoardCard` (ObservableObject) / `BoardLane` (ObservableObject, `IsOverWip`)
    - `SprintBoard` - move between lanes, burndown
    - `SprintSample`
- [ ] 🎨 🆕 `Modules/UI/UIKanbanView.xaml` / `.xaml.cs` / `UIKanbanViewModel.cs`

Points

- `DragGestureRecognizer` / `DropGestureRecognizer`, already shown in the View sample
- drop placeholder, lane highlight, auto scroll at the lane edge
- Function : `Back` `絞込` `追加` `一覧`

## 🎫 Ticket

- [ ] 📦 🆕 `Models/Sample/TicketWallet.cs`
    - enum `TicketState`
    - `TicketPass` / `TicketWallet` / `TicketSample`
- [ ] 🎨 🆕 `Modules/UI/UITicketView.xaml` / `.xaml.cs` / `UITicketViewModel.cs`

Points

- needs 🧰 `PerforatedBorder`
- `Controls/BarcodeView` for 1D and `Converters/QrImageSourceConverter` for QR, both already exist
- `IScreen.SetScreenBrightness` for the brightness boost, as used in `DeviceMisc`
- `CarouselView` for the stacked passes
- Function : `Back` `分配` `追加` `使用済`

# 🅱 P3 Secondary screens

## 🎵 Music

- [ ] 📦 🆕 `Models/Sample/MusicPlayer.cs`
    - record `LyricLine`
    - `MusicTrack`
    - `MusicPlayer` - position, current lyric line, waveform
    - `MusicSample`
- [ ] 📐 🆕 `Graphics/Drawing/WaveformDrawing.cs`
- [ ] 🎨 🆕 `Modules/UI/UIMusicView.xaml` / `.xaml.cs` / `UIMusicViewModel.cs`

Points

- `ColorExtractor` for the background color, `MediaBind` and the bottom sheet for playback
- the current lyric line is followed with `CollectionView.ScrollTo`
- Function : `Back` `歌詞` `キュー` `出力`

## 🖼 Gallery

- [ ] 📦 🆕 `Models/Sample/PhotoAlbum.cs`
    - `PhotoItem` (ObservableObject, `IsSelected`) / `PhotoSection`
    - `PhotoAlbum` - selected count, total size, range select
    - `PhotoSample`
- [ ] 🎨 🆕 `Modules/UI/UIGalleryView.xaml` / `.xaml.cs` / `UIGalleryViewModel.cs`

Points

- `GroupHeaderTemplate` does not stick on android, so the date header is a single element outside
  the `CollectionView`, swapped from the `Scrolled` event
- the column count changes through 🧰 `GestureOption`
- Function : `Back` `全選択` `解除` `共有`

## 😴 Health

- [ ] 📦 🆕 `Models/Sample/SleepLog.cs`
    - enum `SleepStage`
    - record `SleepSpan`
    - `SleepNight`
    - `SleepLog` - weekly average, advice
    - `SleepSample`
- [ ] 📐 🆕 `Graphics/Drawing/SleepStageDrawing.cs`
- [ ] 🎨 🆕 `Modules/UI/UIHealthView.xaml` / `.xaml.cs` / `UIHealthViewModel.cs`

Points

- the multi ring is three `ArcMeter` stacked in a `Grid`, no change to `ArcMeter` yet
- the weekly stacked bars reuse `ChartDrawing.ShowStacked`
- the 4 lane stage band is the only new drawing
- Function : `Back` `週` `月` `共有`

## 🧭 Navi

- [ ] 📦 🆕 `Models/Sample/RouteGuidance.cs`
    - enum `TurnKind` `LaneArrow`
    - record `GuideStep` `LaneGuide` `NaviRoad` `NaviPoi`
    - `RouteGuidance` - remaining distance, ETA, progress to the next turn
    - `NaviSample`
- [ ] 📐 🆕 `Graphics/Scene/NaviScene.cs`
- [ ] 🎨 🆕 `Modules/UI/UINaviView.xaml` / `.xaml.cs` / `UINaviViewModel.cs`

Points

- `SceneControl` and `SceneObject`, the same family as Flight and Tactical
- road grid, route polyline, vehicle marker and POI labels are drawn in the scene
- lane guidance and the ETA bar stay in XAML
- Function : `Back` `全体` `音声` `周辺`

## ⚾ Score

- [ ] 📦 🆕 `Models/Sample/BaseballGame.cs`
    - enum `AtBatResult`
    - `BaseballTeam` / `BaseballInning` / `BaseballBatter`
    - `BaseballGame` - count, runners, lineup
    - `BaseballSample`
- [ ] 📐 🆕 `Graphics/Drawing/DiamondDrawing.cs`
- [ ] 🎨 🆕 `Modules/UI/UIScoreView.xaml` / `.xaml.cs` / `UIScoreViewModel.cs`

Points

- the inning table scrolls horizontally with 🧰 `Scroll.SyncTarget`
- Function : `Back` `速報` `成績` `順位`

## 📁 Filer

- [ ] 📦 🆕 `Models/Sample/CloudDrive.cs`
    - enum `DriveItemKind` `DriveSyncState`
    - `DriveEntry` (ObservableObject, `IsSelected`) / `DriveCrumb` / `DriveUsage` / `DriveTransfer`
    - `CloudDrive`
    - `DriveSample`
- [ ] 🎨 🆕 `Modules/UI/UIFilerView.xaml` / `.xaml.cs` / `UIFilerViewModel.cs`

Points

- XAML only, no new drawing
- the extension color mapping stays a private method of the view model
- Function : `Back` `選択` `移動` `削除`

## 📈 Trade

- [ ] 📦 🆕 `Models/Sample/StockBoard.cs`
    - `StockQuote` (ObservableObject)
    - record `StockDepth`
    - `StockBoard` - tick update, trade log
    - `StockSample`
- [ ] 🎨 🆕 `Modules/UI/UITradeView.xaml` / `.xaml.cs` / `UITradeViewModel.cs`

Points

- the candlestick reuses `ChartDrawing.ShowCandle` and `CandlePoint`, updated through 🧰 `UpdateCandle`
- the depth board is XAML, a `BindableLayout` with the bar width bound to the quantity ratio
- Function : `Back` `歩み値` `チャート` `注文`

# 📄 P4 Document

- [ ] 📄 🆕 `Document/Images/UI_*.png` - device screenshots for the 13 screens
- [ ] 📄 ✏️ `Document/Example06_UI.md` - add the 13 screens
- [ ] 📄 ✏️ `README.md`

# 🧹 P5 Cleanup

- [ ] 🗑 `Document/Proposal_UI.md`
- [ ] 🗑 `Document/Mockup/`
- [ ] 🗑 `Document/Images/Proposal/`
- [ ] 🗑 `Document/Plan_UI.md`

# 📝 Naming

- `Models/Sample/Timetable.cs` is the schedule domain (`TimetableEvent` / `TimetableCalculator`),
  so the train timetable goes to `StationTimetable.cs`
- `Models/Sample/OrderBook.cs` is the business order list (`OrderInfo` / `OrderStatus`),
  so the stock depth goes to `StockBoard.cs`
- `Models/Sample/MapGuide.cs` already owns `MapSample`, so the Navi loader is `NaviSample`
- `Components/StorageManager` is a device component, so the file browser model is `CloudDrive.cs`
- every `Models/Sample` file keeps the existing section layout - `Enum` / `Data` / `Service` / `Sample`,
  with the loader as `<Prefix>Sample`
