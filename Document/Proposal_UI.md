# UI Proposal

Proposed UI sample screens. Images are mockups rendered at the same size as the device screenshots in
[Images](./Images); their sources are in [Mockup](./Mockup).

# 🅰 Primary

## Transit

<p>
<img width="25%" src="./Images/Proposal/Transit.png" />
<img width="25%" src="./Images/Proposal/TransitTimetable.png" />
</p>

- ViewId: `UITransit` / `UITransitTimetable`
- Vertical rail colored per segment, transfer node, collapsible intermediate stops
- Route candidate tabs, previous / next departure, fare and duration summary
- Timetable: hour x minute matrix, train type color, current time row
- Reuse: BindableLayout / Graphics.Drawing

## Seat

<p>
<img width="25%" src="./Images/Proposal/Seat.png" />
</p>

- ViewId: `UISeat`
- Seat map with aisles - available / sold / selected / premium / wheelchair
- Pinch zoom and pan over a large element count, hit test by coordinate, minimap
- Reuse: Graphics.Drawing / TouchOption

## Gantt

<p>
<img width="25%" src="./Images/Proposal/Gantt.png" />
</p>

- ViewId: `UIGantt`
- Fixed task column with a horizontally scrolling date area, synchronized vertical scroll
- Today line, dependency links, progress inside the bar, day / week / month zoom
- Reuse: ClamGrid / Scroll behavior

## Kanban

<p>
<img width="25%" src="./Images/Proposal/Kanban.png" />
</p>

- ViewId: `UIKanban`
- Lanes scrolled horizontally, card drag and drop between lanes
- Drop placeholder, lane highlight, WIP limit warning
- Reuse: View Drag & Drop / CollectionView

## Ticket

<p>
<img width="25%" src="./Images/Proposal/Ticket.png" />
</p>

- ViewId: `UITicket`
- Ticket shape with perforation notches - non rectangular clip
- Stacked passes, QR code and barcode, screen brightness boost
- Reuse: BarcodeView / IScreen

# 🅱 Secondary

## Music

<p>
<img width="25%" src="./Images/Proposal/Music.png" />
</p>

- ViewId: `UIMusic`
- Synchronized lyrics scrolling with the current line highlighted
- Waveform seek bar, background color taken from the artwork
- Reuse: ColorExtractor / MediaBind / Bottom sheet

## Gallery

<p>
<img width="25%" src="./Images/Proposal/Gallery.png" />
</p>

- ViewId: `UIGallery`
- Sticky date headers, pinch to change the column count
- Selection mode with a count toolbar and range selection
- Reuse: CollectionView / StaggeredGrid

## Health

<p>
<img width="25%" src="./Images/Proposal/Health.png" />
</p>

- ViewId: `UIHealth`
- Sleep stage chart, multi ring score, weekly stacked bars
- Reuse: Graphics.Drawing / ArcMeter

## Navi

<p>
<img width="25%" src="./Images/Proposal/Navi.png" />
</p>

- ViewId: `UINavi`
- Turn by turn header with remaining distance and progress
- Lane guidance, current speed and speed limit, ETA bar
- Reuse: Graphics.Scene / MapBind

## Score

<p>
<img width="25%" src="./Images/Proposal/Score.png" />
</p>

- ViewId: `UIScore`
- Inning scoreboard, diamond with runners, ball / strike / out count, lineup
- Reuse: Graphics.Drawing

## Filer

<p>
<img width="25%" src="./Images/Proposal/Filer.png" />
</p>

- ViewId: `UIFiler`
- Breadcrumb navigation, list / grid switch, multi select
- Transfer progress banner, storage usage bar
- Reuse: StorageManager / CollectionView

## Trade

<p>
<img width="25%" src="./Images/Proposal/Trade.png" />
</p>

- ViewId: `UITrade`
- Candlestick chart with volume, order book with depth bars
- High frequency update
- Reuse: Graphics.Drawing / View Chart

# 🛠 Implementation

- Add entries to `Modules/ViewId.cs`
- `UIMenu1View` and `UIMenu2View` are full at 9 rows x 2 columns, so `UIMenu3View` is required
- Place sample data under `Models/Sample`
- Update [Example06_UI](Example06_UI.md) and [README](../README.md) after implementation
