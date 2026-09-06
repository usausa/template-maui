# 変更内容まとめ (uibase → fix3)

`Works3/Template` 配下の変更を、git タグの区間ごとに **画面単位** でまとめたドキュメント。
**Git の差分を確認しながら「何を行なったのか」を確認するための参考資料**とすることを目的とする。
各区間は「A. 画面単位の変更」→「B. 画面以外の変更」(+「C. この区間のナレッジ」) の順で記載し、区間に紐付かない恒常情報(ポリシー / 意図的差異 / 資産レシピ / 決定アーカイブ)は末尾の**付録**に置く。

## 区間の概要

| 区間 | 期間 | コミット | 変更ファイル | 変更行 | 性格 |
|---|---|---|---|---|---|
| [1. uibase → uibase2](#1-uibase--uibase2--ui-ブラッシュアップ) | 2026-05-16 → 07-21 | 35 | 338 | +30,254 / -2,474 | **UI ブラッシュアップ**(新規20画面・全画面の作り込み・描画基盤再編) |
| [2. uibase2 → uibase3](#2-uibase2--uibase3--コードレビュー対応) | 2026-07-21 → 08-15 | 10 | 122 | +2,472 / -1,020 | **コードレビュー対応**(不具合修正・堅牢化・権限/DB/通信の見直し) |
| [3. uibase3 → uibase4](#3-uibase3--uibase4--外部リファレンス評価とライブラリ追従) | 2026-08-15 → 09-01 | 1 | 10 | +1,273 / -26 | **外部リファレンス評価**の追加とライブラリ API 追従 |
| [4. uibase4 → fix1](#4-uibase4--fix1--アナライザ設定の全面見直し) | 2026-09-01 | 1 | 42 | +308 / -172 | **アナライザ設定の全面見直し**と機械的追従 |
| [5. fix1 → plus1](#5-fix1--plus1--外部リファレンス評価の実装フェーズ110) | 2026-09-01 → 09-03 | 3 | 130 | +9,967 / -1,781 | **外部リファレンス評価の実装**(新規12画面・App モジュール新設・SCP・描画基盤拡張) |
| [6. plus1 → baseup1](#6-plus1--baseup1--基盤刷新di-移行白画面対策メニュー再編) | 2026-09-03 → 09-05 | 9 | 135 | +2,129 / -1,693 | **基盤刷新**(DI コンテナ移行・BACK/白画面対策・メニュー再編・ドキュメント統合) |
| [7. baseup1 → fix2](#7-baseup1--fix2--resharper-全件対応と-scene-描画の重大バグ修正) | 2026-09-05 | 1 | 66 | +459 / -333 | **ReSharper 全件対応**(254 件)と **Scene 描画の重大バグ修正**(かくつき・ANR・SIGSEGV) |
| [8. fix2 → back](#8-fix2--back--back初期化方式の刷新白画面対策-b-1-の方式変更) | 2026-09-05 → 09-06 | 4 | 15 | +276 / -233 | **BACK/初期化方式の刷新**(白画面対策 B-1 を StartupState 方式へ・ApplicationInitializer 廃止) |
| [9. back → fix3](#9-back--fix3--calendar--location-の手直し) | 2026-09-06 | 2 | 4 | +15 / -36 | **Calendar / Location の手直し**(Debug 計測撤去・未取得表示の空状態化) |

- 関連ドキュメント: 残作業(実機確認 / 実テスト / 画像アセット / バックログ)は `Task_Checklist.md`(**2026-09-03 に `UI_Verification_Checklist.md` + `Implementation_Checklist.md` + 旧 `UI_Task_Checklist.md` + `Image_Asset_Expansion_Plan.md` を統合**)。
  - `Fix_Checklist.md`(区間2で作成)と `Reference_Summary.md` / `Reference_Analysis.md`(区間3で作成)は**区間5で削除**され、内容は本書と上記へ統合された。
  - `UI_Development_Log.md`(区間1で新設した経緯・ナレッジの記録)は **2026-09-03 に本書へ統合して削除**(ナレッジ=各区間の C 節、恒常情報=付録)。

---

# 1. uibase → uibase2 — UI ブラッシュアップ

第1弾ブラッシュアップ + 別リポジトリ `Work-Project-MauiUI` からの UISample 取り込み + 第2弾ブラッシュアップ + 画面改名 + スタイル切り出しをまとめた区間。**UI 系はこの区間がほぼすべて**。

## A. 画面単位の変更

### A-1. メニュー画面

| 画面 | 変更内容 |
|---|---|
| UIMenu | Timeline→Graph2 改名に伴う並び替え。UIProfile2 / UICockpit 廃止で **11行 → 10行×3列=30ボタン・空セルなし** に縮小 |
| SampleMenu | 空セル 7 箇所の `IsVisible` 除去 → 「可視の無効ボタン」に統一 |
| SampleCvNetMenu | 同上(4 箇所) |
| ViewMenu | 空セルを可視の無効タイル(灰色)に変更 |
| Main/Menu・BasicMenu・DeviceMenu・NavigationMenu・NetworkMenu | 空セルの扱い以外の差異(番号プレフィックス / 絵文字 / 列数 / アイコン有無)は**意図的に維持**(統一しない方針) |

### A-2. UI モジュール — 新規追加(20 画面)

いずれも View / View.xaml.cs / ViewModel の 3 ファイル一式を新設し、`ViewId` とメニューへ登録。

| 画面 | 種別 | 内容 |
|---|---|---|
| UIShop | EC | 商品カード(Popular 横スクロール + All Items 2列グリッド)。SfEffectsView Ripple、FadeUp 段差入場、**検索 Entry を実結線**(タイトル部分一致で絞り込み) |
| UIItem | EC | 商品詳細。数量ステッパー(1..99)、サイズタグ 30/50/100ml のタップ切替、画像 Pop 入場 |
| UICart | EC | カート。`UICartItem` を ObservableObject 化しステッパー結線 + 合計再計算(件数 / 小計 / 割引10% / 合計連動)、Total は CountUp + Bounce、Checkout ダイアログ結線 |
| UIStream | 配信 | 「+ My List」トグル結線、TopBar/Hero の FadeUp + レーティング Pop、ポスター Ripple |
| UIStreamDetail | 配信 | タブ切替(Trailers ⇔ More Like This)で FadeIn、Favorite / Download トグル、Trailer 行・Related タイル Ripple |
| UIChat | 通信 | Send / Receive / System のバブル + リアクションピル(4列2行折返し)、スタンプトレイ |
| UICalendar | 日付 | `CalendarView2` を用いた月表示。日付タップ / イベントタップでトースト表示 |
| UITimeline | 日付 | 旧 `UITimelineSample`。進行中イベントのドット Pulse + ヘッダ/リストの FadeUp 入場 |
| UIGraph | 可視化 | Git グラフ表現の可視化画面(`Resources/Raw/Graph/repository.json` を読み込み) |
| UIGraph2 | 可視化 | 旧 `UITimeline`(Git グラフ別表現)を改名。リスト全体の FadeUp 入場 |
| UIFeel | キャラ | 7つの hex セルのタップ選択(自系統色の枠3px + チェックバッジ移動)、中央→外周の Pop 開花入場 |
| UIPet | キャラ | ステータスバー4本を ProgressBar 化 + `ProgressTo` による伸長アニメ、数値 CountUp、Heart ボタン結線(HP+5 / 上限400)、Add to Party トグル |
| UIKitOnboard | Kit | Skip / Get Started 結線(→ Kit Dashboard)、ページスワイプでテキスト FadeIn |
| UIKitSetting | Kit | Switch を ObservableProperty 化して実バインド、グループ FadeUp 段差 |
| UIKitDash | Kit | メトリクス4カード Pop 段差、Heart Rate カード FadeUp、ベル未読ドット Pulse、リンク2カード Ripple |
| UIKitNotify | Kit | 未読/既読の視覚差(左アクセントバー + 青背景 + 太字 + 未読ドット)、行タップで既読化 |
| UIKitTracking | Kit | ステップ3状態(完了=緑チェック / 進行中=青ドット Pulse / 未来=灰)、完了区間の縦線を緑に塗り分け |
| UIFlight | HUD | 旧 `UIFlightHud`。SkiaSharp 自走描画(`FlightHudScene`) |
| UITactical | HUD | 旧 `UIMechHud`。SkiaSharp 自走描画(`MechHudScene`) |
| UIEnergy | HUD | **UICockpit を廃止して差し替え**(`UICockpitView.xaml` をベースに再構成)。`EnergyFlowScene` |
| UITelemetry | HUD | SkiaSharp 自走描画(`TelemetryScene`) |

### A-3. UI モジュール — 既存画面の改修(18 画面)

| 画面 | 変更内容 |
|---|---|
| UILogin | 背景 / レイアウト / メッセージ / 入力 / Keep login / パスワード表示トグル / Forget password の各セクションを整備。左アイコンを Size 30→28 |
| UIMoney | Background / Header / Rank / Menu / Detail / Bottom select + バッジのカード構成に整備、エントランス演出を全面適用。メニューアイコンを `MoneyIcon` マークアップ拡張(既定28)へ集約 |
| UISuper | 検索バー + ポイント残高 / バナーカルーセル / ミニアプリ / クーポンの縦構成。FontSize を許可値の大きい側へ拡大 |
| UIPos | POS 明細・小計まわりを整備(増減ボタンの参照実装) |
| UIProfile | **旧 UIProfile2 のカード型デザインを統合**(UIProfile2 は View/VM/ViewId/メニューとも削除)。パララックスヘッダー、SNSアクション3トグル(フォロー/いいね/お気に入り)、写真6枚グリッド、色付きタグ、区切り線統計、Bio |
| UICharacter | Character / Class / Detail のカード構成に整備 |
| UIDock | ドックのアイコン配置 / 演出を微調整 |
| UIMail | Header / Messages / Empty state / Indicator / Floating Button / Tab の構成に整備 |
| UISocial | Shared / 背景 / Icon / Episode / Counter / Alert の構成、作戦中止の Back 結線(SfEffectsView) |
| UISchedule | 7日チップ + タイムテーブルの構成、FontSize 17→18 |
| UITreeMap | 撮影瞬間のシャッターフラッシュ演出 |
| UIRadar | 飾りステータス追加(Random 使用のため CA5394 をファイル先頭 pragma で抑止) |
| UIGauge | 6種サンプル(Pressure / Humidity / Temperature / Wind / Speed / Tachometer)を整備 |
| UIMeter | 四隅ビネット追加 |
| UIMixer | Knob / dB scale / Slider / Channel / Equalizer / Frequency を整備(クラス名 **Mixier → Mixer** にスペル統一) |
| UILoad | 微調整 |
| UITimeline(旧 Timeline) | 内容を旧 TimelineSample に差し替え(旧内容は UIGraph2 へ) |
| UICockpit | **廃止**(View.xaml.cs / ViewModel 削除、View.xaml は UIEnergyView.xaml へ転用。`CockpitControls` も削除) |

### A-4. View モジュール

| 画面 | 変更内容 |
|---|---|
| ViewEffect | **新規**。演出資産のカタログ画面。常時アニメ(Wave×3 時間差 + Pulse + ON/OFF Switch)と変化フィードバック(Fire で Bounce + Highlight + Flash 同時発火) |
| ViewAnimation | 2 InfoCard 化(Click 系4ボタン=**Shake 新設** + Tap 系タイル)、空セル解消 |
| ViewBorder | コメントアウトされていた `StrokeLineJoin` / `StrokeLineCap` の Picker を復活 |
| ViewCarousel | 中央カード強調(非中央=Scale 0.92 + Opacity 0.55)、白カード影、画像角丸クリップ、背景グレー |
| ViewCollection | ▼▲ を Expand_less/more アイコン化、スワイプ項目をアイコン+文字の縦積みに |
| ViewDrawing | 色6チップ + 線幅 Slider のパレット、プレビュー額装 + 未描画時の空状態 |
| ViewEasing | 4×3 均等グリッドへ再設計、背景に Easing 曲線(`EasingCurveView` 新設)、12セル目=実行状態セル(Run 中 Pulse + Function4 無効化) |
| ViewGraphics | Add line/circle/rect(乱数色)/ Clear ボタン + 図形数チップ(`ShapeDrawing` に Circle 追加) |
| ViewLottie | プレイヤー UI(シーク Slider + mm:ss.f 等幅表示 + 円形 Play/Pause・Reset、白カード額装) |
| ViewRefresh | 初回ロード(1秒)中に EmptyView のスケルトン4行、ヘッダに Newspaper アイコン + 件数ピル、行カード白 + 枠線化 |
| ViewSvg | 3 SVG 切替(dotnet_bot / vite / react)チップボタン + ファイル名表示 + 額装 |

### A-5. Device モジュール(17 画面)

| 画面 | 変更内容 |
|---|---|
| DeviceInfo | 3 InfoCard 化 + FadeUp 段差入場 |
| DeviceSensor | Vector3 / Quaternion を VM で軸分解(RGB=XYZ 軸バッジ + 中央ティック付きバー)。Compass カード=実描画ダイヤル(30°目盛 + N/E/S/W + 赤針)、Level カード=気泡水準器を新設(`SensorDrawing`) |
| DeviceStatus | 開発用バッテリーアイコン列挙を削除 → 動的バッテリーアイコン + 残量ゲージ(20%以下=赤 / 充電中=緑)+ Network 状態チップ(緑/琥珀/赤) |
| DeviceAudio | プレイヤーカード UI(250ms ポーリング、シーク Slider + DragCompleted、円形トランスポート3ボタン、再生中アイコン Pulse、音量%表示) |
| DeviceQrDisplay | Entry ライブ編集 → QR 再生成 + 額装 + 空状態表示 |
| DeviceQrScan | 結果空時「Scan a code...」プレースホルダ + 検出時にカメラ面へ白 Flash |
| DeviceLocation | 画面上部に 240px の地図(IsShowingUser)を常設、初期=東京駅→測位毎に現在地へ移動(`MapController.MoveTo`)。測位待ち空状態 + Position/Motion カード + タイムスタンプ Highlight |
| DeviceOcr | カメラにガイド枠 + **結果は画面内パネル表示**(ダイアログ廃止)+ 実行中オーバーレイ |
| DeviceBluetooth | 状態を**インライン状態チップ**(Idle/Connecting/Printing/Completed/Failed)で表示(ダイアログ廃止、State enum + IsBusy) |
| DeviceBleScan | 未検出時 EmptyView(Bluetooth_searching Pulse + 「Scanning...」)+ 温湿度/CO2 値の更新時 Bounce |
| DeviceBleHost | Podcasts アイコン円(Advertising 中 Pulse)+ 状態チップの中央カード化 |
| DeviceNfc | 履歴 EmptyView「Suica をかざしてください」(Contactless Pulse・ダークテーマ)+ 残高 CountUp(600ms) |
| DeviceMisc | 15 アクションを 4 InfoCard(Screen / Feedback / Light / Speech)に分類、Material アイコン付き ActionButton 化、音声認識中は赤マイク Pulse |
| DeviceCommunication | タップ行カード×3(色分けアイコン円 + Ripple) |
| DeviceActivity | 歩数(96pt)を CountUp 化 |
| DeviceWiFi / DeviceBiometric | 「Not implemented」空状態パネル(暫定) |

### A-6. Navigation モジュール(14 画面)

| 画面 | 変更内容 |
|---|---|
| Wizard Input1 / Input2 | 入力フィールド定型(キャプション + Border + フォーカスで青枠)、`StepIndicator`(1/3・2/3)、ステップ説明カード + ヒント。**Next(▶️)は入力があるまで無効**(`WizardContext` を ObservableObject 化)。Input2 の Placeholder 誤記(Data1→Data2)修正 |
| Wizard Result | 入力サマリカード(Data1/Data2)+ 完了カード、Function4 表示を ✔️ に |
| Stack 1 / 2 / 3 | 本文が空だった3画面を中央カード化(レベル別アクセント 1=Blue / 2=Teal / 3=DeepPurple、Looks_one〜3 アイコン円 + StepIndicator + キー操作ヒント + Pop 入場) |
| Shared Input / Main1 / Main2 | SharedInput に「Return to Shared1/2」チップ(遷移元により Indigo/Teal)+ 入力フィールド定型。Main1/Main2 は系統色の数字円 + チップ + No 大型表示 + FadeUp |
| Edit List / Detail | List=一覧行を白カード化(Id=等幅 #n ピル、Edit/Delete に PressEffect)+ 空時 EmptyView。Detail=入力フィールド定型 + インラインエラー表示 |
| Navigate Cancel | パターン説明カード常設(Amber Help + Yes/No の挙動説明) |
| Navigate Initialize | 初期化3秒間スケルトン + Hourglass Pulse → 完了で緑 Task_alt カードが FadeIn |
| InputNumber(モーダル) | 全キーにローカル派生スタイル(共有 Input*Button + PressEffect)、✔/❌ を Material の Check/Close グリフに変更 |

### A-7. Basic / Main / Data / Network モジュール

| 画面 | 変更内容 |
|---|---|
| BasicValidation | **ルートの `ContentPage.Behaviors` 誤記を `ContentView.Behaviors` に修正**(バリデーションが実際に動くようになった)。エラーで枠が赤 Highlight + エラーラベル FadeIn、Error/Clear/Focus をアイコン付きボタン化 |
| BasicBehavior | 2 Entry を Border + フォーカス枠(青/緑)で色分け + イベント値の Highlight カード |
| BasicConverter | タイポ修正(Upper/**Lowwer→Lower**)+ 未使用 `ToChecked` 削除、CheckBox + ラベル横並び化、変換結果に Highlight |
| BasicStyle | 全 Action/Information ボタンを ActionCommand で結線し「Last action」カードに押したボタン名を Highlight。SelectButton は隣接セグメント配置 |
| BasicFont | ScrollView 化(実機で全フォント見える)+ フォント毎 InfoCard 10枚 + JetBrainsMono / NotoSerifJP 見本追加 |
| BasicTypography | ScrollView 化 + 4 セクションカード |
| BasicLocale | 現在カルチャカード + リソースキー2件の一覧 |
| BasicDialog | 11 ボタンを 4 カテゴリの InfoCard に整理 |
| Setting(Main) | スキャンガイドピル + 設定値パネル、値変更時 Highlight、未設定時「(not set)」 |
| Data | CRUD / Bulk の 2 カード化 + BulkDataCount に CountUp |
| NetworkRealtime | TODO 解消=鋸波 → **ランダムウォーク擬似データ**(CPU ゆらぎ+スパイク / Memory ドリフト / Network バースト)、StatControl 3枚(青/緑/橙)、500ms 更新 |
| NetworkMenu | VM の未使用 `RealtimeCommand`(デッドコード)を削除 |

### A-8. Sample モジュール

多くは第1弾で整備済み。この区間の主な変更は FontSize 拡大 / アイコンのマークアップ拡張化 / 軽微な整形。

| 画面 | 変更内容 |
|---|---|
| SampleChart | チャート表示整備 + FontSize 13→14 |
| SampleChat | チャット UI 微調整(共通コントロール `ChatView` 側で整備) |
| SampleMedia / SamplePdf / SampleMap1 / SampleMap2 / SampleMarkdown / SampleWebApp / SampleWebBasic | 表示・操作を整備(Map 系は `MapBind` / `MapController` 経由へ) |
| SampleCvLocal / SampleCvNet(Face/Object/Ocr/People/Tag) | CV 系の表示整備 |

### A-9. 画面横断の変更

| 項目 | 内容 |
|---|---|
| FontSize 統一 | XAML/C# の FontSize を許可値の大きい側へ統一(13→14 / 15→16 / 17→18 ほか)。Skia 自走描画(Scene)は 8.5→9 / 9.5→10 / 7.5→8 / 7→8 / 13→14 / 15→16 / 17→18、Telemetry の GEAR 桁のみ 56→72。before/after 比較画像を `Document/FontSize_{FlightHud,MechHud,Telemetry,Energy}_{Before,After}.png` に追加 |
| アイコン共通化 | 生 Unicode・絵文字を `markup:Material` / `Fluent` / `MenuIcon` / `MoneyIcon` へ置換(72箇所)。メニューアイコン 24→28。動的色の 11 箇所のみ `FontImageSource` 残置 |
| スタイル切り出し | 36 画面の直書き視覚属性を画面ローカル `ResourceDictionary` の Style へ切り出し(**値は不変更=見た目は同一**) |
| 画面改名 | UITimelineSample→**UITimeline** / UIFlightHud→**UIFlight** / UIMechHud→**UITactical** / Timeline→**Graph2**、UICockpit→廃止 |

A-9 の補足:

- **FontSize の規則**: 許可値(付録A参照)への統一。Scene(Skia 自走描画)は情報量の観点から Excel 標準値(6/8/9/10 等)の小フォントも許可し、**Android では WPF 版より小さく見えるため大きい側へ丸める**
- **Profile 統合の採用基準**: 基本要素(写真グリッド / 色付きタグ / 統計 / Bio)は旧 Profile2 を採用し、旧 Profile 優位の**パララックスヘッダーのみ移植**(`Scroll.ParallaxTarget`)。SNS アクション 3 トグル(フォロー / いいね / お気に入り)は「OFF=白地+色アイコン / ON=色地+白アイコン」で統一
- **スタイル切り出しの規約**(36 画面で統一):
  - 定型スタイルキー: `RootGrid` / `RootScroll` / `PageStack` / `CenterCardBorder` / `CardStack` / `IconCircleBorder(+Label)` / `CenterTitleLabel` / `CardDivider` / `CenterHintLabel` / `Empty*` 系。色違いの繰り返しは BasedOn 派生
  - 定義順: コンバータ → ルート要素(`Root*`) → `PageStack` → 本文の出現順(コンテナ→中身、BasedOn 派生は基底の直後)
  - スタイル化せず残す直指定 = 動作パラメータ(`FocusedStroke` / `HighlightColor` / `ProgressColor` / `AccentColor` / DataTrigger の状態色 / セグメント重ね `Margin=-1` / 行 Grid の `ColumnSpacing`)と `FontImageSource`(Style 不可の既知制約)のみ
  - 対象外 = 直書きが僅少な 10 画面(Navigation 4 / Device 4 / Basic 2)と、バリエーション自体がコンテンツの画面(BasicFont / ViewEffect / DeviceSensor / UIFeel / UIMeter / メニュー系)

## B. 画面以外の変更

### B-1. 新規コントロール(`Controls/`)

| ファイル | 用途 |
|---|---|
| `InfoCard.xaml(.cs)` | 見出しアイコン + タイトル + 区切り線付きの角丸カード(ControlTemplate 方式) |
| `StatusChip.xaml(.cs)` | アイコン + テキストのピル型チップ(DataTrigger で状態色分け) |
| `StepIndicator.cs` | ●●● のステップ表示(現在=ピル型強調 + 「n / N」) |
| `EasingCurveView.cs` | Easing 曲線を背景描画する GraphicsView |
| `Gauge.cs` | 汎用ゲージ(SKCanvasView) |
| `CalendarView.xaml(.cs)` / `CalendarView2.xaml(.cs)` | 月カレンダー(MAUI 部品版 / Skia 自前描画版) |
| `DayTimetableView.cs` | 日次タイムテーブル描画 |
| `GraphRowSurface.cs` | Git グラフの行描画 |
| `CameraOverlayView.cs` | カメラのガイド枠オーバーレイ |
| `AiChatTemplateSelector.cs` / `DeckButtonTemplateSelector.cs` | テンプレートセレクタ |
| `MixerEqualizer.cs` / `MixerKnob.cs` / `MixerSlider.cs` | 旧 `Mixier*` から**スペル修正のリネーム** |
| `TimelineCell.cs` | **削除**(Graph 系の再構成に伴う) |

### B-2. Behaviors / Converters / Markup

| ファイル | 内容 |
|---|---|
| `Behaviors/AnimationOption.cs` | **新規**。入場/常時/フィードバック演出の添付プロパティ群(FadeUp / Pop / EnterDelay / EnterTrigger / Pulse / Wave / Bounce / FadeIn / Flash / Highlight / Shake / **ProgressTo**) |
| `Behaviors/MapBind.cs` + `Messaging/MapController.cs` | **新規**。Map をコントローラパターンで操作(`MoveTo`) |
| `Behaviors/MediaBind.cs` + `Messaging/MediaController.cs` | **新規**。MediaElement をコントローラパターンで操作 |
| `Behaviors/SliderOption.cs` | **新規**。`DragCompletedCommand` |
| `Behaviors/ButtonOption.cs` / `LabelOption.cs` / `Focus.cs` / `Scroll.cs` | 拡張(PressEffect / CountUp / フォーカス枠 / OverScroll 抑止) |
| `Converters/*` | 9 件新規(`AlternateRowBackground` / `BadgeCount` / `CenteredRatio` / `ChatTime` / `CollectionNotEmpty` / `CompassDirection` / `DecibelToColor` / `DurationToSeconds` / `RefKindBrush`) |
| `Markup/FontIconExtensions.cs` | **新規**。アイコン用マークアップ拡張(基底 `Material` / `Fluent` + 用途別 `MenuIcon` / `MoneyIcon`) |

### B-3. 描画基盤(`Graphics/`)の 2 名前空間分離

用途で分離し、対称形の命名({概念}Object / Control / Xxx{概念})に統一。

- **`Graphics.Drawing`**(IDrawable・データ駆動): `DrawingObject`(旧 `GraphicsObject`)/ `DrawingControl`(旧 `GraphicsControl`)/ `ActivityDrawing` / `BarcodeDrawing` / `DetectDrawing` / `LoadDrawing` / `ShapeDrawing`(旧 `*Graphics` からリネーム)+ 新規 `ChartDrawing` / `ColorTreeMapDrawing` / `SensorDrawing`
- **`Graphics.Scene`**(SKCanvas・自走アニメ): `SceneObject` / `SceneControl`(旧 `AnimatedSkiaView` 後継)+ `EnergyFlowScene` / `FlightHudScene` / `MechHudScene` / `TelemetryScene`

### B-4. モデル・サービス

| ファイル | 内容 |
|---|---|
| `Models/Sample/Calendar/*` | **新規 11 ファイル**(`MonthViewBuilder` / `ScheduleEvent` / `TimetableDay` / `Stamp` / `DayKind` ほか) |
| `Models/Sample/Chat/*` | **新規 4 ファイル**(`ChatMessage` / `AiChatMessage` / `MessageReaction` / `MessageType`。旧 `Models/Sample/ChatMessage.cs` は削除) |
| `Models/Sample/Graph/*` | **新規 4 ファイル**(`GraphBuilder` / `GraphLayout` / `GraphModels` / `TimelineRow`。旧 `Models/Sample/TimelineRow.cs` は削除) |
| `Models/Sample/{MapSpot,RadarTarget,SocialNotificationInfo,SocialUnit,SuperItems}.cs` | 新規 |
| `Models/Sample/PhotoItem.cs` | ObservableObject 化(+ `IsCurrent`) |
| `Services/ScheduleService.cs` / `HolidayService.cs` | **新規**。スケジュール / 祝日のサンプルデータ供給 |

### B-5. リソース・ビルド設定

| 項目 | 内容 |
|---|---|
| フォント | `JetBrainsMono-Regular.ttf` 追加。`MauiProgram` に **JetBrainsMono / NotoSerifJP** を登録 |
| 画像 | `ic_camera.svg` / `ic_send.svg` / `ic_sticker.svg` / `stamp01〜08.png` を追加 |
| データ | `Resources/Raw/Graph/repository.json` を追加(UIGraph / UIGraph2 用) |
| `Styles.xaml` | リソース・派生スタイルを追加(**共有スタイルの既存定義は不変更**=ポリシー遵守) |
| `Settings.XamlStyler` | 属性並び順に `HorizontalOptions, VerticalOptions` を追加 |
| `MauiProgram.cs` | BLE ホスティングの登録方法を変更(`AddBleHostedCharacteristic<UserCharacteristic>` → `AddSingleton<UserCharacteristic>`) |
| パッケージ更新 | SkiaSharp 3.119.2→**4.148.0**、Smart.Navigation 系 2.20.0→**3.1.0**、Shiny 系 4.0.1→**5.1.1**、CommunityToolkit.Maui 14.1.1→14.2.0、MAUI 10.0.60→10.0.80、Svg.Skia 4.9.0→5.1.1 ほか多数 |
| `NoWarn` | `NU1903` を **TODO 付きで暫定追加**(次区間で解消) |
| ドキュメント | `Document/UI_Development_Log.md` / `Document/UI_Verification_Checklist.md` を新設 |

## C. この区間のナレッジ

- **正確な API 名**(発明注意): CountUp=`LabelOption.CountUpValue/CountUpFormat/CountUpDuration`、フォーカス枠=`Focus.FocusedStroke/FocusedThickness`(親 Border 必須)、Make* コマンドヘルパは canExecute 自動再評価(`.Observe()` は存在しない)、`s:NullToText` は Null/NonNull 置換のみ(値パススルー不可 → 空状態は 2 ラベル + `NullToBoolConverter`)
- `Vector3` / `Quaternion` の X/Y/Z は**フィールド**のためバインド不可 → VM で NotifyAlso 連動の計算プロパティに分解
- MAUI `Button.CornerRadius` は**全周一括**(左右個別の角丸は不可)
- `PathF` は IDisposable(CA2000 → `using var`)。CA5394(Random)はファイル先頭 `#pragma` の前例(UIRadarViewModel)。算術式は括弧明示(IDE0048/SA1407)
- **CollectionView 行の入場アニメは不成立**(リサイクルで Loaded が再発火する)。BindableLayout の行はリサイクルされないため可
- SkiaSharp: `SKPath` 直接構築は CS0618 → `SKPathBuilder`+`Detach()`。`DrawText` は SKTextAlign 付き / `DrawBitmap` は SKSamplingOptions 付きオーバーロードを使用。日本語は `SocialFonts.NotoSerifJP`(`SKTypeface.Default` は豆腐化)、絵文字は `SKFontManager.MatchCharacter`+異体字セレクタ U+FE0F 除去
- **FontImageSource は Style 不可・マークアップ拡張はバインド不可**(検証済)→ 動的色の 11 箇所だけ FontImageSource 直書きが残る理由
- ControlTemplate+ContentPresenter+`TemplateBinding` に Converter 指定可(InfoCard / StatusChip で実証)。CarouselView の中央強調は項目モデルの IsCurrent+`CurrentItemChangedCommand`+DataTrigger で code-behind 不要
- 画面定型: GrayLighten5 背景+Padding 12+InfoCard+FadeUp 段差(0/80/160…)、単一機能画面は中央カード+円形アイコン(96)+説明+ヒント
- ビルド警告ベースライン=8 件(CS8785×1=Smart 生成器、XA4301×7=ネイティブ lib 重複)。Visual Studio 併用時は obj のファイルロックで CLI ビルドが失敗することがある(リトライか該当 VS を閉じる)

---

# 2. uibase2 → uibase3 — コードレビュー対応

コードレビュー(2026-08-05)に基づく Phase 0〜9 の修正コミット `2f533e0f` と、その後の追補・差し戻しからなる区間。**UI の見た目を変える変更はほぼ無く、不具合修正・堅牢化・設計是正が中心**。

## A. 画面単位の変更

### A-1. 不具合の修正

| 画面 | 変更内容 |
|---|---|
| Edit List | **編集/削除ボタンでクラッシュしていた問題を修正**。`Button` の要素レベル `x:DataType` により `CommandParameter="{Binding}"` が `TypedBinding<EditListViewModel, EditListViewModel>` としてコンパイルされ、実 BindingContext(`WorkEntity`)と不一致で null になっていた(uibase2 以前から存在した不具合) |
| Device Status | **クラッシュの修正**。Manifest から `BATTERY_STATS` を削除したことで `IBattery` が `PermissionException` を投げていた(`Permissions.EnsureDeclared` は付与ではなく**宣言の有無**を見る)。宣言を復活 |
| Device QrScan / Setting(Main) | **カメラ権限の Check→Request を追加**。起動時の一括要求を廃止した際の漏れで、新規インストール時にスキャンが動作しなかった |
| UIDock | CPU/Memory ボタンの `Parameter` が両方 `"VolumeDown"` だった誤りを `"Cpu"` / `"Memory"` に修正 |
| UIGraph / UIGraph2 | JSON デシリアライズ結果の `null!` を `?? RepositoryData.Empty` に統一 |
| Network(サーバ時刻表示) | `.ToLocalTime()` を追加。DateTime の UTC 統一の副作用で API 経路の表示が 9 時間ずれていた |
| Data | 保存を `DateTime.Now` → `DateTime.UtcNow`、表示側で `ToLocalTime()` |

### A-2. ライフサイクル・リソース解放

| 画面 | 変更内容 |
|---|---|
| UIMeter | **二重起動ガード**(`if (loopTask is not null) return;`)+ `await loopTask` の try/finally 化 |
| Device Audio | 購読前に `polling?.Dispose()`(二重購読の防止) |
| Device Bluetooth | `finally` で状態復帰。想定外の例外で `State=Printing` のまま `IsBusy` が固着し印刷ボタンが永久に無効化されていた |
| Device BleScan | 外側タイマ購読にも `onError` を追加 |
| Device Nfc | **Rx シーケンス死亡経路を解消**。`ConvertResult` を `ParseTag` に分離して解析例外(`ArgumentException` / `OverflowException` / `IndexOutOfRangeException`)を null 化 + WARN ログ、`Subscribe` に `onError` 追加 |
| UITreeMap / ViewDrawing | `Dispose` で `ImageHelper.ReplaceBitmap(Image, null)`、代入も `ReplaceBitmap` 経由に統一(SKBitmap の所有権を一元化) |
| UIMail | `SKBitmap` を `Disposables.Add` して画面破棄時に解放(アンマネージドメモリのリーク) |
| SampleCvLocal | **再入防止ガード**。`await DetectAsync(bitmap)` 中に再実行されると `ReplaceBitmap` が推論中のビットマップを破棄していた(use-after-dispose) |

### A-3. 権限フローの画面側への移動

起動時の一括権限要求を廃止し、各画面の `OnNavigatedToAsync` で Check→Request するよう変更。

| 画面 | 要求する権限 |
|---|---|
| Device Camera / Device QrScan / Device Ocr / UITreeMap / Setting(Main) | カメラ |
| Device Location | 位置情報(`LocationAlways` → **`LocationWhenInUse`** に緩和) |
| Device Activity | ActivityRecognition(未許可時はダイアログ表示) |
| UILoad | マイク |

### A-4. 非搭載ハードウェアへの対応

| 画面 | 変更内容 |
|---|---|
| Device Sensor | 各センサーの `IsSupported` を見て開始、`IsMonitoring` を見て停止(非搭載センサーの `FeatureNotSupportedException` を回避) |
| Device Bluetooth / Device Nfc | アダプタ未搭載時に null を返す `IsSupported` を追加(下記 B-2 参照) |

### A-5. DI・共通化

| 画面 | 変更内容 |
|---|---|
| UICalendar / UISchedule | `ScheduleService` / `HolidayService` を VM 内 `new` から **DI 注入**へ変更 |
| Device Misc | `speech.RecognizeCancel()` → `await speech.RecognizeCancelAsync()` |
| SampleCvNet(Face/Object/Ocr/People/Tag) | Phase 7-3 で基底クラス `SampleCvNetViewModelBase` に共通化 → **同区間内で差し戻し**、5画面を独立 VM に戻して基底クラスを削除(サンプルは1画面で完結して読める方が良いという判断)。ビットマップ所有権とカメラ権限チェックは維持 |

### A-6. XAML の是正(見た目の変更なし)

| 画面 | 変更内容 |
|---|---|
| UICharacter / UIChat / UIGraph2 ほか | `RelativeSource` バインドに `x:DataType` を付与(意図の記述。生成コードは従来どおり) |
| UIProfile | 写真一覧を `CollectionView` → **`FlexLayout` + `BindableLayout`** に変更(件数固定・縦 ScrollView 内で仮想化が効かないため)。`Basis="33.33%"` で 3列×2行 |
| UIMoney / UIMail / UIPos / UILoad / UIShop / UIItem / UIGraph / DeviceBleScan / ViewRefresh ほか | **Style キーのリネーム**(共有スタイルを隠蔽していたキーの解消)。衝突した1〜2キーだけでなく、同じ画面の同じ役割グループ全体を同じプレフィックス規則に統一(計 40 キー・98 箇所) |
| ViewCollection / DeviceBleScan | 軽微な整形 |

## B. 画面以外の変更

### B-1. 起動シーケンス・アプリ基盤

| ファイル | 内容 |
|---|---|
| `ApplicationInitializer.cs` | `async void Initialize` を廃止し **`StartupTask` を公開**。DB 初期化の失敗(`IOException` / `UnauthorizedAccessException` / `SqliteException`)を捕捉して `InitializeError` に保持 |
| `App.xaml.cs` | 起動時の一括権限要求を削除。`StartupTask` の完了を待ってから遷移し、**DB 初期化失敗時は原因を提示して `Quit()`**(従来は無言でクラッシュ→次回起動でも同じ所で落ちるループになっていた) |
| `MauiProgram.cs` | `ApplicationInitializer` を単体登録し `IMauiInitializeService` はファクトリ経由に。`ScheduleService` / `HolidayService` / `INetworkInteraction` を DI 登録 |
| `MainPage.xaml(.cs)` / `MainPageViewModel.cs` | Function ボタンの状態を **`FunctionState` に集約**(Title / HeaderVisible / FunctionVisible も `NotificationValue<T>` 化)。`HeaderVisible` の既定を `true`→**`false`**(初回ナビゲーションまで空タイトルのヘッダーが出ていた)。Back の fire-and-forget に例外ログを追加 |
| `Modules/ValidationHelper.cs` | **新規**。`AppViewModelBase` / `AppDialogViewModelBase` に重複していた検証処理を集約(バッファは `[ThreadStatic]`) |
| `Shell/IShellControl.cs` / `ShellProperty.cs` / `DiagnosticPanel.xaml.cs` | Shell 状態を `FunctionState` に統合。DiagnosticPanel は `HandlerChanged` → **`Loaded`/`Unloaded`** に変更し、**世代番号でタイマーの多重起動を防止**(`StopMonitor` はフラグを倒すだけで次 tick までタイマーが生存するため) |

### B-2. Components(デバイス層)の堅牢化

| ファイル | 内容 |
|---|---|
| `BluetoothSerial.android.cs` | アダプタ未搭載を許容(`IsSupported`)。`RegisterReceiver` を `ContextCompat.RegisterReceiver(..., ReceiverNotExported)` に変更、**Discovery 30秒 / Bond 60秒のタイムアウト**を追加、解除処理を `finally` に集約、`socket.Dispose()` を追加 |
| `Nfc.android.cs` / `Nfc.cs` | アダプタ解決を遅延化 + `IsSupported`。**タグはイベントハンドラ内でのみ有効**という契約にし、検出ごとに `Close`/`Dispose`(接続リーク防止)。`TagLostException` を含む `Java.IO.IOException` を握って購読側へ伝播させない。`Dispose` で ReaderMode を解除 |
| `NoiseMonitor.android.cs` / `NoiseMonitor.cs` | 停止を **`StopAsync`** 化(stop/start レースの解消) |
| `OcrReader.*` | ログ出力とキャンセル対応 |
| `ActivityRecognizer.*` | コンポーネント内での権限要求を削除(呼び出し側に移動)。イベントが**UI スレッド以外から発火する**契約を XML コメントで明記 |
| `NfcExtensions.cs` | `SubArray` の負サイズをガード |

### B-3. 通信・データアクセス

| ファイル | 内容 |
|---|---|
| `Usecase/NetworkInteraction.cs` | **新規**。`INetworkInteraction` / `DialogNetworkInteraction` で `NetworkOperator` を `IDialog` から分離(UI なしテスト・再利用が可能に) |
| `Usecase/NetworkOperator.cs` | エラー分類(`NetworkErrorKind`)による統一とリトライ上限の導入(219行の全面書き換え) |
| `Services/HttpService.cs` | 全メソッドに `CancellationToken` を追加。`IHttpClientFactory` のクライアントを `using` しない(ハンドラはプール管理)。転送系を **`ApiNames.Transfer`** クライアントへ分離、進捗コールバックを共通化し **Content-Length 不明時は通知しない** |
| `Services/AppHostBuilderExtensions.cs` | 転送用 HttpClient を追加(**Timeout 10分**。無限にすると中断手段が無くなるため上限を設ける) |
| `Services/ApiContext.cs` | `BaseAddress` / `Token` を **volatile** 化(UI スレッドの書き込みを通信スレッドが読むため) |
| `Services/ParameterBuilder.cs` | クエリのキー・値を **`Uri.EscapeDataString`** でエンコード |
| `Services/DataService.cs` | `busy_timeout` を **接続文字列 `Default Timeout=3`** に変更(PRAGMA は接続単位で他接続に効かない)。**WAL 有効化**と `-wal`/`-shm` の削除漏れ修正。Work の採番を `INSERT ... (SELECT COALESCE(MAX(Id),0)+1 ...)` の単文にして **SELECT MAX→INSERT の非アトミック競合を解消**。`Using`→`UsingAsync` の誤用を修正 |
| `Usecase/CognitiveUsecase.cs` | 初期化を `SemaphoreSlim` で保護(並行初期化による `InferenceSession` リーク防止)、`ArrayPool` の返却を `finally` に移動(出力読み取り完了前に返していた)、リサイズ後の `SKBitmap` を `using` |
| `Helpers/ReactiveSignalR.cs` / `Helpers/Data/*` / `Helpers/Json/*` | async void の除去、DateTime の UTC 正規化(`DateTimeTypeHandler` / `DateTimeConverter`) |
| `State/Settings.cs` | AI サービスキーを **Preferences → `SecureStorage`** へ移行(旧値の自動移行付き)。空文字は `Remove` として扱う |

### B-4. 権限・Manifest

| 項目 | 内容 |
|---|---|
| `Permissions.cs` | `CheckStatusAsync` → 未許可なら `RequestAsync` の共通実装に統一。`ActivityRecognition` 権限クラスを追加。位置は `LocationAlways` → `LocationWhenInUse` |
| `AndroidManifest.xml` | 不要権限を削除(`CHANGE_WIFI_STATE` / `READ/WRITE_EXTERNAL_STORAGE` / `FLASHLIGHT` / `ACCESS_BACKGROUND_LOCATION` / `USE_BIOMETRIC` / `USE_FINGERPRINT`)。`BATTERY_STATS` は**宣言が必須のため復活**。Android 11 向けレガシー Bluetooth(`maxSdkVersion="30"`)を追加、`BLUETOOTH_SCAN` に `neverForLocation` を付与。`uses-feature`(nfc / camera / bluetooth / microphone、いずれも `required="false"`)を追加 |

### B-5. ビルド・アナライザ・署名

| 項目 | 内容 |
|---|---|
| `NoWarn` | `NU1608;NU1903`(TODO 付き暫定)→ **`XA4301` のみ**(理由コメント付き)。NU1903 は `SQLitePCLRaw` の版数対応で解消 |
| Release 署名 | csproj 直書きの `example.keystore` 設定を廃止し、**gitignore 対象の `.Signing.props`** から注入する方式に変更(未配置時は debug 署名)。`example.keystore` を削除 |
| `#pragma warning disable` | 発火し得ない抑止 9 行 / 5 ファイル(`SpeedGauge` CA1001 / `NewsItem` CA1056 / `DeviceInfoViewModel` SA1135 / `LabelOption.android` CA1416×2 / `SocialControls` CA1822)を削除。Phase 9-2 で追記された理由コメント 27 件も削除 |
| `Styles.xaml` | 参照 0 のスタイル 4 件を削除(`FillHorizontalStack` / `InputEntry` / `ItemCollectionLabel` / `SideFlexLayout`) |
| パッケージ更新 | CommunityToolkit.Maui 14.2.0→**15.0.0**、BarcodeScanning 3.0.4→3.1.0、Grpc 2.80/2.81→2.83.0、MAUI 10.0.80→10.0.90、Smart 系各種 |
| `.gitignore` / `AGENTS.md` | `.Signing.props` を追加 / 改行コードのルール(既存は変更しない・新規は CRLF)を追記 |
| ドキュメント | `Document/Development.md` に**実案件適用時の注意**(DB 初期化・シークレット・Release 署名・開発用 HTTPS 証明書・コンポーネントのスレッド契約)を追記。`Code_Review.md` / `Fix_Plan.md` / `Implementation_Plan.md` を作成後 **`Fix_Checklist.md` へ集約して削除**。`README.md` / `README-ja.md` に Android 専用である旨を追記 |

---

# 3. uibase3 → uibase4 — 外部リファレンス評価とライブラリ追従

10 ファイルのみの小さな区間。**画面の見た目・動作を変える変更は無く**、ドキュメント追加とライブラリ API 追従が中心。

## A. 画面単位の変更

なし。

## B. 画面以外の変更

| 項目 | 内容 |
|---|---|
| `Document/Reference_Analysis.md`(新規 973行) | 外部の記事・OSS 51 件の詳細評価。**導入済みパッケージの未使用機能が大量にある**ことが最大の発見(Syncfusion は SfEffectsView のみ使用・約30種が未使用、CommunityToolkit.Maui も多数未使用、標準の `Stepper` / `DatePicker` / `TimePicker` / `RadioButton` / `SearchBar` は使用箇所ゼロ) |
| `Document/Reference_Summary.md`(新規 278行) | 上記の人間向け要約。取り込み候補 41 件、新規 NuGet 追加は 1 件のみ、判断項目 D1〜D19 の決定内容 |
| `Modules/ValidationHelper.cs` | `AccessorRegistry.FindAccessor` → **`AccessorProvider.FindAccessor`**(ライブラリの API 変更に追従) |
| `Resources/Styles/Styles.xaml` | 前区間で削除した 4 スタイル(`FillHorizontalStack` / `InputEntry` / `ItemCollectionLabel` / `SideFlexLayout`)を**復活**。加えて `NoErrorColor` / `GroupSpan` / `ItemCollectionGrid` を追加 |
| `.editorconfig` | `dotnet_style_operator_placement_when_wrapping` を `beginning_of_line` → **`end_of_line`** |
| `README.md` / `README-ja.md` | 「Android 専用」の記述を削除 |
| `Template.MobileApp/example.keystore` | 復活(署名自体は `.Signing.props` 方式のまま) |
| `Modules/Basic/BasicLocalViewModel.cs` | 旧ファイル名のまま `BasicLocaleViewModel` クラスが**重複追加**(次区間で削除) |

---

# 4. uibase4 → fix1 — アナライザ設定の全面見直し

コミット `af18c6a4`「Fix1」1本のみ。**アナライザ / エディタ設定の全面見直しと、それに伴う機械的なコード追従**。画面の見た目・動作を変える変更は無い。

## A. 画面単位の変更

| 画面 | 変更内容 |
|---|---|
| Basic Locale | 前区間で重複追加された `Modules/Basic/BasicLocalViewModel.cs` を**削除**(正は `BasicLocaleViewModel.cs`) |
| Basic Style | `BasicStyleViewModel` に CA1002 の pragma を付与 |
| Navigation Shared Input | `SharedInputView.xaml.cs` の namespace に CA1716 の pragma を付与 |
| Sample Chat / UIChat | `string.IsNullOrWhiteSpace` → `String.IsNullOrWhiteSpace` |

## B. 画面以外の変更

### B-1. アナライザ設定(この区間の主目的)

| ファイル | 内容 |
|---|---|
| `Analyzers.ruleset` | ルール名前空間を `Microsoft.CodeQuality.Analyzers` → **`Microsoft.CodeAnalysis.NetAnalyzers`** に修正(**従来の CA 抑止が効いていなかった**)。Hidden 指定を整理し、**CA1002 / CA1305 / CA1416 / CA1716 / CA1721 / CA1724 / CA1873 / CA2007 などの一括抑止を廃止**。StyleCop 側は SA1009 / SA1025 / SA1111 / SA1118 / SA1606・1607・1614・1616・1622・1623・1626・1629 / SA1642・1643 / SA1652 を Hidden に追加 |
| `GlobalSuppressions.cs` | 全体で許容するものを assembly 属性へ移動(CA1305 / CA1721 / CA1873 / CA2007) |
| 各ファイルの `#pragma` | ruleset から外したルールを**必要な箇所だけ** pragma で抑止(CA1724=`App` / `Extensions` / `Result` / `Parameters` / `Permissions`、CA1716=`Select` / `SharedInputView`、CA1002=`ShapeDrawing` / `ColorExtractor` / `DataService` / `BasicStyleViewModel`) |
| `.editorconfig` | 大幅な見直し(198 行)。`[*.slnx]` / `[*.{razor,cshtml}]` / `[*.{xaml,axaml}]` のセクションを追加。多くのルールの重大度を `warning` / `silent` → **`none` または重大度なし**へ変更(括弧・using 整理・式形式メンバーなど)。`csharp_style_expression_bodied_local_functions` / `_operators` は `when_on_single_line:warning` に変更 |

### B-2. アナライザ有効化に伴うコード追従

| 項目 | 対象 |
|---|---|
| **括弧の明示**(`always_for_clarity`) | `CalendarView.xaml.cs` / `CalendarView2.xaml.cs` / `MixerSlider.cs` / `GraphRowSurface.cs` / `BarcodeDrawing.cs` / `MonthViewBuilder.cs` / `HolidayService.cs` / `ScheduleService.cs` / `DeviceState.cs` / `AlternateRowBackgroundConverter.cs` / `BadgeCountConverter.cs` / `NumericInputModel.cs` ほか |
| **BCL 型名の使用**(`string`→`String` 等) | `LabelOption.cs` / `Gauge.cs` / `DayTimetableView.cs`(`float`→`Single`)/ `GraphRowSurface.cs`(`float`→`Single`・`double`→`Double`)/ `SampleChatViewModel.cs` / `UIChatViewModel.cs` |
| null 許容の整理 | `Gauge.cs`(`Unit!` → `Unit`)/ `DrawingControl.cs`(`Drawable = null!` → `null`) |
| BOM 除去 | `Directory.Build.targets` / `Template.MobileApp.csproj` / `AndroidManifest.xml` |
| `*.Designer.cs` | 自動生成物の追従 |

### B-3. パッケージ更新

`Microsoft.Maui.*` 10.0.90→**10.0.100**、`CommunityToolkit.Maui` 15.0.0→15.0.1、`Usa.Smart.Resolver`(+ DI 拡張)2.15.0→**3.0.0**、`Usa.Smart.Core` 2.16.0→2.19.0、`Usa.Smart.Mvvm` 2.8.0→2.10.0、`Usa.Smart.Converter` 2.15.0→2.16.0。

### B-4. ドキュメント

`Reference_Summary.md` / `Reference_Analysis.md` を改訂。ユーザー指示により **SCP(SSH.NET)を B-20 として追加**し、判断項目 **D20〜D22(SSH.NET 追加可否 / SCP サンプルのスコープ / 接続情報の保管とホスト鍵検証)を未決として追記**。

---

# 5. fix1 → plus1 — 外部リファレンス評価の実装(フェーズ1〜10)

区間3で作成した外部リファレンス評価(記事・OSS 51 件)の**採用項目を実装した区間**。コミットは3本。

| コミット | 日付 | 内容 |
|---|---|---|
| `8ad7b772` Update plus p1 | 2026-09-01 | フェーズ1(基盤・低コストの穴埋め) |
| `51aa581b` Update plus p2 | 2026-09-01 | フェーズ2(抽選ホイール) |
| `3f213e4b` Update plux p x | 2026-09-03 | フェーズ3〜9 + Release トリミング修正 + D8 本採用 + SCP + ドキュメント整理 |

**新規 12 画面 + `Modules/App` および `Layouts/` の新設**。新規 NuGet は `SSH.NET` の 1 件のみで、他はすべて**導入済みパッケージの未使用機能のサンプル化**または自作。

## A. 画面単位の変更

### A-1. メニュー画面(空きセルの結線)

| 画面 | 変更内容 |
|---|---|
| Main/Menu | 9行 → **10行**。`10.App` を追加(`Modules/App` への入口) |
| BasicMenu | Row8 の空きボタンを **Setting**(`BasicSetting`)に結線 |
| ViewMenu | 空きセル5つを **Layout / DragDrop / State / Toolkit / Custom** に結線 |
| SampleMenu | 空きセル2つを **Sf Chart**(`SampleSfChart`)/ **Crop**(`SampleCrop`、Material `Crop` アイコン)に結線 |
| NetworkMenu | `Grid.Row="7"` の空きボタンを **SCP** に結線(それまで到達不能だった `NetworkScpView` が開けるようになった) |
| UIMenu | 10行 → **11行**。「描画デモ」グループを追加し **Wheel**(Material `Attractions`)を配置。残り2セルは規約どおり可視の無効ボタン |

### A-2. 新規画面(12 画面)

#### App モジュール(新設)

| 画面 | 内容 |
|---|---|
| AppMenu | `Modules/App` の入口メニュー |
| AppCalc | 科学電卓。コアは純モデル `Models/App/ExpressionCalculator`(トークナイザ → **操車場アルゴリズム**で中置→RPN → RPN 評価器の3段構成、NuGet 不使用)。四則 / %(百分率)/ 括弧 / 単項マイナス / 三角関数(DEG)/ log / ln / exp / √ / 累乗(右結合)/ 階乗 / π / e / **暗黙の乗算**(2π, 3(1+2))に対応。5列ダークレイアウト、結果表示は DSEG7、入力行末尾の「│」カーソルは `FormattedString` の Span 表現。「=」直後は演算子入力で結果から継続 |
| AppGame | 数独。盤面ロジックは純モデル `Models/App/SudokuGame`(バックトラッキングで完全解を生成し 36 マスを残して問題化 / 入力 / 行・列・ボックスの矛盾判定 / 完成判定)。盤面は `UniformItemsLayout`(9列)+ `BindableLayout`、3x3 区切りはセル VM の `Margin` で表現。矛盾は赤字、COMPLETE! バナー + Bounce。**ライフゲーム / 2048 は純モデルの差し替えで追加できる方針**をコメントで明記 |

#### View モジュール(5 画面)

| 画面 | 内容 |
|---|---|
| ViewLayout | CommunityToolkit の `DockLayout`(上下左右ドック+残り充填)/ `UniformItemsLayout`(MaxColumns=4)。自作レイアウト3種(CircularLayout=曜日リング / StaggeredGrid=カードウォール / Cascade=MDI ウィンドウ風)も同画面へ追記 |
| ViewState | `StateContainer`(Loading / Empty / Error / 既定=Success の切替)+ `LazyView`(`x:TypeArguments` で `ViewStatePanelView` を遅延生成)。`Behaviors/LazyViewOption.cs` を新設し、`LoadViewAsync()` の呼び出しを VM のフラグから起動(code-behind 回避) |
| ViewToolkit | ルートを `SfBottomSheet` にし Content=`SfTabView`(タブ3枚)。入力タブ=`SfOtpInput` / `SfSegmentedControl` / `SfChipGroup`、表示タブ=`AvatarView` / `RatingView` / `Expander` / `SfAccordion`、シートタブ=ボタンで `IsOpen` |
| ViewDragDrop | 標準 `DragGestureRecognizer` / `DropGestureRecognizer` のデモ。①同一リスト内の並べ替え ②TODO⇔DONE のリスト間移動(列の空き領域へのドロップは末尾追加)③ゴミ箱ドロップで削除(`DragOverCommand` / `DragLeaveCommand` でハイライト)。3リスト共用の `DataTemplate` 1本で実装 |
| ViewCustom | 自作コントロール4種のカタログ(計画の `ViewInputView` と**統合して1画面に変更**)。`MarqueeLabel` / `TreeView`(+`TreeNode`)/ `ColorPicker`(RGBA スライダ4本 + ARGB hex)/ `DurationPicker`(時0-23・分5分刻み → `TimeSpan`) |

#### Basic / Sample / UI モジュール(4 画面)

| 画面 | 内容 |
|---|---|
| BasicSetting | 使用ゼロだった標準コントロール `Stepper` / `DatePicker` / `TimePicker` / `RadioButton`(`RadioButtonGroup.GroupName` + `SelectedValue`)/ `SearchBar`(`SearchCommand`)を `Switch` / `Slider` / `Picker` / `Entry` と共に網羅。**各コントロールに `ToolTipProperties.Text` を付与**(長押しで表示)。Summary カードで双方向バインドを確認 |
| SampleSfChart | Syncfusion チャートのダッシュボード。Cartesian Column / Doughnut / **Polar(レーダー)** / Funnel + Pyramid / **SparkLine・SparkColumn・SparkWinLoss** / **Sunburst(2階層)** |
| SampleCrop | 画像切り抜き。`CropDrawing`(`IInteractiveDrawing` + `ExportPng` 共用構成)で枠移動 + 四隅ハンドルリサイズ(ヒット半径28・最小64・画像内クランプ)、三分割線 + 減光。書き出しは exporting フラグで `OnDraw` を出力モードに切替 |
| UIWheel | 抽選ホイール。`WheelDrawing`(扇形 + 回転テキスト + ハブ/リム/上部ポインタ)。`Spin(extra, length, completed)` が CubicOut で減速停止し**完走時のみ**当選項目を通知、離脱時は `CancelSpin()` で中断(通知なし)。ホイールタップ / SPIN ボタンの両方で回転、結果は `HasResult` の DataTrigger で「？」⇔当選名を切替 + Bounce。code-behind なし |

### A-3. 既存画面の強化

| 画面 | 変更内容 |
|---|---|
| NetworkScp | **空スタブを実装**(フェーズ6)。接続先カード / 転送カード(**`FilePicker`** — 使用ゼロ API のサンプル化、リモートファイル名 Entry、ProgressBar、アップロード / ダウンロード / キャンセル)/ ログカード(最新20件)。ダウンロード先は `FileSystem.CacheDirectory` |
| Setting(Main) | SCP セクション(Host / User / Password)を追加し、**設定投入は設定画面の QR に統一**(`SettingViewModel.DetectCommand` に `ScpHost` / `ScpPort` / `ScpUser` / `ScpPassword` の4キーを追加)。項目増に伴い**ラベルと現在値を横並び**に変更(`SettingRowGrid` + キャプション幅108固定) |
| EditList | **複数選択 + 一括操作**。Function3=Select トグルで `SelectionMode` を None⇔Multiple、`SelectedItems` は `ObservableCollection<object>` にバインド。行は VSM Selected で青ハイライト、選択モード中は行ボタンを DataTrigger で非表示。下部バー=件数 + 全選択 + 一括削除(確認ダイアログ) |
| BasicBehavior | `MaskedBehavior`(電話番号)/ `UserStoppedTypingBehavior`(800ms)/ `EventToCommandBehavior`(Switch.Toggled)を追記 |
| BasicValidation | **相関検証**を追加。`Confirm` に `[Compare(nameof(Password))]`、Password 変更時は `PropertyChanged` 購読で Confirm を `ClearErrors`→`Validate` 再検証。CT 検証 Behavior は `EmailValidationBehavior` / `NumericValidationBehavior`(`Flags=ValidateOnValueChanged` + Invalid/ValidStyle で文字色切替) |
| BasicLocale | `ResourceManager.GetResourceSet` で resx キーを列挙し **neutral / ja / current の3値を一覧表示** + カルチャ別書式カード(current / en-US / de-DE / ja-JP の N2 / C / D / t) |
| SampleMap1 | **Google Maps `MapElements`**。右上 FAB 3個(Route / Pentagon / Circle)で経路(Polyline=スポット巡回)/ 範囲(Polygon=皇居周辺)/ 円(Circle=東京駅1.5km)をトグル |
| SampleMap2 | **Mapsui 強化**。画面左上のトグルパネルで機能グループ別マネージャを個別に有効化(ウィジェット / スポット+コールアウト / 図形 / GeoJSON / クラスタリング)+ **SkiaSharp オーバーレイ**のグラデーション経路 |
| SampleChart | `ChartKind` に **Stacked(積み上げ棒)/ Scatter(散布図)/ Heat(ヒートマップ)** を追加。**要素ごとのディレイ出現**(Stacked=棒ごと / Scatter=点ごと / Heat=行ごと。ディレイ系は全体1000ms)。Line の折れ線は**値の高さで色補間し線分ごとに塗り分け**(低=青→高=赤) |
| SampleChat | **音声フロー4ステップ**(モック)。マイク FAB + オーバーレイ(`StepIndicator` 4段)で ①録音(タイマー秒数 + Pulse 脈動 + 赤 Stop 化)②文字起こし(1.5秒待ち→固定文)③抽出プレビュー(`VoiceExtractItem` 4件)④承認で `InputText` へ反映。× / 画面離脱でリセット |
| UISchedule / UICalendar | `IScheduleEventProvider` への依存に変更(下記 B-5)。`DayTimetableView` がイベントを**カード描画**(白地 + ドロップシャドウ + 枠線 + アクセントバー)、幅110以上のカード右上に所要時間、空き時間帯を薄緑 + 「空き xh」表示。VM の日合計(予定 n 件 / 合計 / 空き)を追加 |
| UITelemetry | **Function2 で DIRECT⇔BUFFER を切替**(ダブルバッファの比較デモ。画面左上に MODE 表示)。滞在中のみフレーム統計 `[SceneStats]` を出力 |
| ViewCollection | `RemainingItemsThreshold="3"` + `RemainingItemsThresholdReachedCommand` で**無限スクロール**(最大16グループ)。Footer に読み込み済み件数 |
| ViewEffect | **`SKConfettiView`**(Celebrate トグル)+ `TouchBehavior`(PressedScale / 長押し Command)+ `IconTintColorBehavior`(画像の色替え)を追記 |
| ViewRefresh | 自作スケルトンを **`SfShimmer` の `CustomView`** に入れ、形はそのままで波アニメーションを追加 |
| ViewGraphics | `SketchDrawing`(フリーハンド描画・ストローク色自動循環・Undo/Clear)+ **PNG 出力**結果の表示、`PulseRingDrawing`(波紋リング)、`ProgressArcDrawing`(カウントダウンボタンの残量リング)を追加 |
| ViewLottie | **スクロール連動 / 長押し進行**。横スクロール帯(幅900のグラデーション Border)と長押しボタンを追加し、共通の `ScrubCommand` で Lottie の Progress を駆動 |
| ViewShadow | 末尾に**ニューモーフィズム**を追加。同色タイル(#E0E5EC)の Border 2枚重ねで暗影(#A3B1C6, +8+8)と明影(White, -8-8)を合成、凹は影の向きを反転。ページ全体を ScrollView 化 |
| ViewSvg | `SvgView` 拡張に伴い VM から SKSvg ロード処理が消え、**パス切替だけの VM** になった(`IFileSystem` 依存も除去) |
| UIFlight / UITactical / UIEnergy / UITelemetry | 静的レイヤの `SKPicture` キャッシュ適用(下記 B-1)。Flight は**レーダーブリップのタップ選択**(最近傍14単位以内、再タップで解除、選択リング + TGT 情報行)を追加 |

## B. 画面以外の変更

### B-1. Graphics 基盤の拡張

| 対象 | 内容 |
|---|---|
| `DrawingObject` | **単発アニメーション + 完了通知** `AnimateValue(name, start, end, length, easing, frame, completed)` を新設。`IAnimatable` の `Animate` 拡張で減速停止し、**完走時のみ** completed を呼ぶ(中断時は呼ばない)。`AbortAnimation` / `AnimationIsRunning` も公開 |
| `IInteractiveDrawing` / `DrawingControl` | タッチ(Start / Drag / End)を Drawing へ転送。`DrawingObject.ExportPng(stream, w, h)` は**表示と同じ `OnDraw`** を `PlatformBitmapExportContext` へ流して画像化 |
| 新規 Drawing | `WheelDrawing`(抽選ホイール)/ `SketchDrawing`(フリーハンド)/ `CropDrawing`(切り抜き)/ `PulseRingDrawing`(波紋)/ `ProgressArcDrawing`(残量リング) |
| `ChartDrawing` | Stacked / Scatter / Heat の3種追加、要素ディレイ出現、値連動のグラデーション線 |
| `SceneObject` | **静的レイヤの `SKPicture` キャッシュ** `DrawCachedLayer(canvas, key, w, h, draw)`(サイズ変化時のみ再記録)。**ヒットテスト** `Touch` + `SceneControl.EnableTouchEvents`。**論理解像度固定** `VirtualSize`(opt-in・uniform scale + レターボックス + タッチ座標逆変換)。**ダブルバッファ** `UseDoubleBuffer`(ループスレッドでオフスクリーン `SKSurface` に描画 → `Snapshot()` をロック付き交換、UI スレッドは転写のみ)+ フレーム統計 `[SceneStats]` |
| `ScenePool<T>` | **新規**。短命オブジェクトの再利用プール(Rent / Return + CreatedCount) |
| 各 Scene | キャッシュ適用: Energy=ドット背景(約500円/フレームの描画を排除)/ Flight=レーダー盤面 + ロール目盛 / Tactical=マップ静的層(パネル枠 + 等高線ストローク + グリッド + 固定ラベル)/ Telemetry=タコメーター盤面 + Gフォース盤面(破線円の `SKPathEffect` 毎フレーム生成も解消) |

**D8(ダブルバッファ)の Release 実測 → 本採用**

| モード | 描画1回の avg | max | フレーム数/3秒 | 実効フレームレート |
|---|---|---|---|---|
| DIRECT(UI スレッド描画) | 16.5〜17.8ms | 19〜44ms | 90〜97 | 約 30fps |
| BUFFER(ループスレッド描画+転写) | **14.0〜14.5ms** | 17〜37ms | **171〜186** | **約 60fps** |

フレームレートがほぼ倍増したため **`UseDoubleBuffer` の既定を ON**(4シーン全て)に変更。Telemetry の Function2 トグルは比較デモとして残置。
※ 実演として入れた Energy の火花は「わかりづらい」ためユーザー判断で削除(`ScenePool<T>` は基盤として残置)。

### B-2. `Layouts/` の新設(第3の拡張ポイント)

| ファイル | 内容 |
|---|---|
| `CircularLayout.cs` | 子要素を円周に均等配置(真上開始・時計回り)。`Radius` 未指定は領域から自動、添付プロパティ `Angle` で個別角度も可 |
| `StaggeredGrid.cs` | 高さの異なるカードを**最も低い列へ詰める** Pinterest 型 |
| `AppLayoutManagerFactory.cs` | `ILayoutManagerFactory` のデモ。DI 登録したファクトリが `CascadeStackLayout` のときだけカスケード配置のマネージャを返し、他は null(=既定)。**サブクラス側を変更せずに配置アルゴリズムを差し替えられる**ことを示す |

### B-3. コントロール / ビヘイビア

| ファイル | 内容 |
|---|---|
| `Controls/SvgView.cs` | **拡張**。`Source`(アプリパッケージ内パス)/ `Placeholder` / `ErrorPlaceholder` と `Loading` / `Ready` / `Error` イベント、`SKSvg` の共有キャッシュ。従来の `Svg`(SKSvg 直接バインド)は互換維持で `Source` が優先 |
| `Controls/MarqueeLabel.cs` | **新規**。クリップした Grid 内の Label を無限スクロール。Loaded / Unloaded / SizeChanged で開始・停止・再計算 |
| `Controls/TreeView.cs` | **新規**。展開中ノードをフラット化して並べ直す簡易ツリー。▸/▾ 切替・行選択(`SelectedNode` TwoWay) |
| `Controls/ColorPicker.cs` | **新規**。RGBA スライダ4本 + プレビュー + ARGB hex(`SelectedColor` TwoWay・再入ガード) |
| `Controls/DurationPicker.cs` | **新規**。時 / 分の Picker 2個 → `TimeSpan`(TwoWay) |
| `Controls/DayTimetableView.cs` | イベントのカード描画・空き時間帯表示に対応 |
| `Behaviors/LazyViewOption.cs` | **新規**。`LoadViewAsync()` を VM のフラグから起動する添付プロパティ(code-behind 回避) |
| `Behaviors/Scroll.cs` | `RatioCommand` を新設(ScrollView のスクロール量を 0-1 に正規化して ICommand へ) |
| `Behaviors/AnimationOption.cs` | `HoldCommand` / `HoldDuration` を新設(Button の Pressed/Released で 0-1 を進め、途中離しは 250ms で巻き戻し、完走後の再押下は先頭から) |

### B-4. 地図基盤

| ファイル | 内容 |
|---|---|
| `Messaging/MapController.cs` | `SetRoute`(Polyline)/ `SetArea`(Polygon)/ `SetCircle`(Circle)を追加 |
| `Messaging/MapsuiMapManagers.cs` | **新規**。`IMapsuiMapManager`(Attach / Detach)を `MapsuiController` の辞書に登録し個別に有効化する**機能グループ別マネージャ構成**。`MapsuiWidgetManager`(ScaleBar + ZoomInOut)/ `MapsuiSpotManager`(`PointFeature` + `SymbolStyle` + **`CalloutStyle`**、`Map.Tapped` → `GetMapInfo` でトグル)/ `MapsuiShapeManager`(NTS の LineString / Polygon)/ `MapsuiGeoJsonManager`(`Resources/Raw/Map/tokyo.geojson` を EPSG:4326 のまま読み `ICoordinateFilter` で球面メルカトルへ再投影)/ `MapsuiClusterManager`(固定シード240点を解像度連動のグリッドクラスタリング、`ViewportChanged` でズーム変化時のみ再計算) |
| `Behaviors/MapsuiBind.cs` | `Overlay` 添付プロパティで `SKCanvasView`(InputTransparent)を同じコントローラに結線。`Viewport.WorldToScreen` で経度緯度→画面座標へ変換し、**線分ごとに `SKShader.CreateLinearGradient` を差し替えるグラデーション経路**を白ハロー付きで描画 |
| `Resources/Raw/Map/tokyo.geojson` | **新規**。GeoJSON サンプルデータ |

### B-5. モデル / サービス

| ファイル | 内容 |
|---|---|
| `Models/App/ExpressionCalculator.cs` | **新規**。式評価エンジン(AppCalc のコア。UI 非依存の純モデル) |
| `Models/App/SudokuGame.cs` | **新規**。数独の生成 / 入力 / 矛盾判定 / 完成判定(AppGame のコア) |
| `Models/Sample/Calendar/TimetableCalculator.cs` | **新規**。区間マージ / 空き算出 / 所要時間表記を集約し、描画と VM の日合計で共用 |
| `Services/IScheduleEventProvider.cs` | **新規**。`ScheduleService` が実装し、`UICalendar` / `UISchedule` 両 VM はインターフェース依存へ(`BindSingleton<IScheduleEventProvider, ScheduleService>()`) |
| `Services/ScpService.cs` | **新規**。`ScpClient` ラッパ(DI 登録)。`ConnectionInfo` + `RemotePathTransformation.ShellQuote`(旧形式 ctor は CS0618=コマンドインジェクション注意のため不使用)。`Uploading` / `Downloading` イベントを `IProgress<double>` へ中継、キャンセルは `CancellationToken.Register(client.Disconnect)`(転送 API が同期のため) |
| `State/Settings.cs` | SCP 設定を追加。`ScpHost` / `ScpPort`(既定22)/ `ScpUser` は `IPreferences`、`ScpPassword` は `SecureStorage`。**ホスト鍵指紋の設定は D22-b の変更で撤去**(QR 照合は行わず、サーバ指紋の参考表示のみ) |
| `MauiProgram.cs` | `ConfigureLifecycleEvents` に Android の Create / Start / Resume / Pause / Stop / Destroy フックを実装(挙動は変えずログのみ。`adb logcat -s AppLifecycle`)。`ConfigureCustomLayouts`(`ILayoutManagerFactory` の DI 登録)を追加。`ScpService` / `IScheduleEventProvider` を登録 |

### B-6. Release ビルドのトリミング対応(フェーズ8の計測で発覚)

初の Release 実行で**既存の潜在問題**が表面化(この区間の実装変更とは無関係)。

- **症状**: Release(トリミング有効)で起動時に `TargetInvocationException` → アプリ落ち。Debug は正常
- **原因**: トリマーが `PageContextStorage`(Smart.Navigation)のコンストラクタを削除し、Smart.Resolver の `StandardProvider.CreateFactory` が「No constructor available」で失敗(リフレクションでコンストラクタを解決するためトリミングと相性が悪い)
- **修正**: csproj に **`TrimmerRootAssembly` を追加**(`Template.MobileApp` + Smart 系 + MauiComponents 系 + `Renci.SshNet` / `BouncyCastle.Cryptography` の計21アセンブリ)
- **副産物(恒久コード)**: `ApplicationInitializer.Initialize` に**起動失敗時の完全な例外連鎖ログ**(`StartupError` タグ)を追加。トリミング時は例外メッセージがリソースキー化されるため、これが無いと原因が追えない。また `Console.WriteLine` は Release の Android では logcat に出ないと判明したため、診断 / 計測ログは `Android.Util.Log` 直接出力へ変更

### B-7. パッケージ

`SSH.NET` 2026.0.0 を追加(この区間で追加した唯一の NuGet)。推移依存の増分は **`BouncyCastle.Cryptography` 2.7.0 のみ**。

### B-8. ドキュメントの整理(ユーザー指示)

| 対象 | 内容 |
|---|---|
| `Reference_Analysis.md` / `Reference_Summary.md` | **削除**。採用項目は実装完了したため、残る価値(決定 D1〜D22 と不採用理由)はアーカイブへ移設(現在は**本書の付録D**。当時は `UI_Development_Log.md` 末尾) |
| `Fix_Checklist.md` | **削除**。第1部の残課題は `Implementation_Checklist.md` へ統合、第2部の完了記録は削除 |
| `Implementation_Checklist.md` | **新規**。残項目のみに再構成(実機確認 + SCP 実テスト + 保留・対象外 + 他案件の残課題)。**2026-09-03 に `Task_Checklist.md` へ統合** |
| `UI_Verification_Checklist.md` | 残確認のみに再構成(0〜12章の実装済み画面別変更記録を削除)。**2026-09-03 に `Task_Checklist.md` へ統合** |

**対応不要 / 取りやめの確定**

- **Blazor Hybrid**: 別リポジトリ `template-maui-blazor` が BlazorWebView + Routes/Layout/Pages の完全な Hybrid 構成で充足しているため**対応不要で確定**
- **MBTiles(`BruTile.MbTiles`)**: SQLitePCLRaw 3.0 系との衝突リスク + デモ用アセット未保有により**取りやめ確定**(パッケージ未追加のため削除対象なし)

## C. この区間のナレッジ

### MAUI / XAML

- **MAUI 10 で `Page.OnBackButtonPressed` は素の `ContentPage` では呼ばれない**。`MauiAppCompatActivity` の `OnBackPressed()` override が廃止され、AndroidX `OnBackPressedDispatcher` のコールバック 1 本になったため。有効判定は `Window.CanConsumeBackNavigation` で、Shell / NavigationPage / FlyoutPage / MultiPage 以外は常に false → **自前で `OnBackPressedCallback` を登録するしかない**。`android:enableOnBackInvokedCallback="false"` の退避策も効かない (dispatcher に有効なコールバックが無いだけなので結局システム既定の finish になる)。関連: dotnet/maui#31266 (OnBackButtonPressed の見直し提案)
- **`App.OnStart` はプロセスに 1 回しか呼ばれない**(`Application.SendStart()` の `_isStarted` ガード)。Android では Activity 再生成のたびに `CreateWindow` は呼ばれるが `OnStart` は呼ばれないため、**画面構築を `OnStart` に依存させると再生成で白画面になる**。公式ドキュメント (App lifecycle) も標準は `Window` のイベント (`Created` = Android の `OnPostCreate`) で、`Application.OnStart` は登場しない。関連: dotnet/maui#18845 (Verified / Backlog・未修正)
- Activity 再生成は BACK 以外でも起きる。`ConfigurationChanges` に `FontScale` / `Locale` が無いため、**端末のフォントサイズ・言語変更で必ず再生成**される (`adb shell settings put system font_scale 1.30` で再現可能)
- `launchMode` (`singleTop` / `singleTask` / `singleInstance`) は「既存インスタンスの再利用方法」の設定なので、**finish 済みで再利用対象が無いケースには効かない**。BACK で Task / ActivityRecord は消滅し、プロセスだけが `oom_score_adj` 900 の空プロセスとして残る
- **Syncfusion の URL 名前空間は Charts / SparkCharts / SunburstChart を解決できない**(MAUIG1001 の不可解な ElementNode エラー)→ チャート系は `clr-namespace` で参照。チャートの `Fill`/`Stroke` は Brush 型のためリテラル色を指定
- Smart.Mvvm の `[ObservableProperty]` に CommunityToolkit 流の `OnXxxChanged` partial フックは無い(CS0759)→ `PropertyChanged` 購読が本プロジェクトの定型
- `SKConfettiView` は `Systems` を明示定義(xmlns の assembly は `SkiaSharp.Extended.UI`)。`SfShimmer.CustomView` は既存スケルトンに波アニメだけ足せる
- カスタムレイアウトは `Layout` 継承 + `CreateLayoutManager()` が最小構成。`ArrangeChildren` の bounds は **Padding 込み**。`ILayoutManagerFactory` は **null 返却で既定マネージャへフォールバック**するため対象型以外へ影響しない
- C# コントロールの自己バインドは typed `SetBinding(..., static (T v) => v.Prop, source: this)` が使える。TwoWay の合成コントロールは「updating フラグで再入ガード」が定石
- `CollectionView.SelectedItems` は `ObservableCollection<object>` をバインドすると**双方向**に機能する(選択変更で中身が更新され、コード側の Add も UI に反映)
- 相関検証は `[Compare(nameof(X))]` + `PropertyChanged` 購読で相手側を `ClearErrors`→`Validate` 再検証

### SkiaSharp / Graphics

- ICanvas(MAUI Graphics)の**ストロークにグラデーションは使えない** → 区間分割 + 色補間で代替(Line チャートのグラデーション線)
- Drawing の PNG 出力は `Microsoft.Maui.Graphics.Platform.PlatformBitmapExportContext` が**追加パッケージなし**で使える
- SkiaSharp 4 系: `SKCanvas.DrawImage` は `SKSamplingOptions` 付きオーバーロードを使う
- `SKPictureRecorder` は共有ペイントを使う描画でも記録可(描画呼び出し時点のペイントを記録)。キャッシュ再生は呼び出し時点のキャンバス変換の中で行われるため**仮想座標系のまま記録**して良い
- `PathF.AddArc` の角度は**反時計回りが正**(0°=3時方向)/ `ICanvas.Rotate` は**時計回りが正**。回転後に `(cx+r, cy)` へ右寄せ描画すると半径方向の外向きテキストになる
- `Animation` を直接 new すると CA2000 → `IAnimatable.Animate` 拡張なら生成が MAUI 側に隠れ警告なし。async メソッド内の `Dispatcher.Dispatch` は CA1849 → `DispatchAsync`
- `System.Threading.Lock`(net9+)は `lock` 文でそのまま使える

### Mapsui / 地図

- **`Mapsui.Styles.Color.FromString` は 3/6 桁 hex のみ対応。8 桁(アルファ付き)は実行時 ArgumentException でクラッシュ** → 半透明は `new Color(r, g, b, a)`(MAUI の `Color.FromArgb` は 8 桁可、という差異に注意)
- `Map.Widgets` は `ConcurrentQueue` のため取り外し不可 → トグルは `widget.Enabled` で行う。タップ判定は v5 では `Map.Tapped` + `e.GetMapInfo([layer])`(`IsMapInfoLayer` は廃止)
- GeoJSON は `GeoJsonProvider`(ファイルパス前提)より **GeoJSON4STJ 直接デシリアライズ + `ICoordinateFilter` 再投影**がアセット運用に合う(追加パッケージ不要)。`SymbolStyle` は `SymbolType`+`Fill`/`Outline` で図形シンボル可
- Mapsui のビューポートは論理座標系 → `SKCanvasView` の物理ピクセルとは `e.Info.Width / view.Width` でスケールを合わせる

### SSH / SCP

- SSH.NET の `ScpClient(string, ...)` ctor は**旧形式(CS0618)** — パス未エスケープでコマンドインジェクションの恐れ → `ConnectionInfo` + `RemotePathTransformation.ShellQuote`
- `ScpClient` の転送 API は同期のみ → `Task.Run` + `CancellationToken.Register(client.Disconnect)` でキャンセル対応
- `SettingParser` は改行区切りのため **1 値に改行を含められない**(PEM 秘密鍵は QR に載せられない)。QR ペイロード例(指紋設定は撤去済み):

```
ScpHost=192.168.1.10
ScpPort=22
ScpUser=deploy
ScpPassword=********
```

### アナライザ / .NET

- IDisposable の所有は**フィールドでなく get-only プロパティ + 宣言時初期化**にする(CA2000/CA2213 は `Disposables.Add` を所有移転と認識しない)。`CancellationTokenSource` フィールドは `Dispose(bool)` オーバーライドで明示 Dispose
- 構造体の CA1815 は `readonly record struct` 化が最小修正 / private 例外クラスは CA1064 → public + 標準 3 コンストラクタ / `Random.Shuffle`(.NET 8+)で Fisher-Yates が 1 行
- `IImage` は `Microsoft.Maui.IImage` と `Microsoft.Maui.Graphics.IImage` で衝突(CS0104)→ `using` エイリアスで解決
- インターフェースのパラメータ名 `end` は CA1716(VB 予約語)→ `startDate`/`endDate`。公開 static メソッドの `List<T>` 戻り値は CA1002 → `IReadOnlyList<T>`
- ctor 内で自プロパティを参照するラムダは、コマンド代入**前**に `PropertyChanged` を購読すると CS8602 → 購読を代入後へ移動。`await` 跨ぎの状態ガードは CA1508(常に true 扱い)に注意

### Release / 計測

- **`Console.WriteLine` は Release の Android では logcat に出ない**(stdout 転送は Debug のみ)→ 診断 / 計測ログは `Android.Util.Log` 直接出力(`StartupError` / `SceneStats` タグ)
- トリミング時は例外メッセージがリソースキー化されるため、**起動失敗時の完全な例外連鎖ログが無いと原因が追えない**。調査手順: `adb logcat -d -b crash` でスタック → メッセージがキーのみなら例外連鎖ログを仕込んで再現 → 内部例外で特定
- 新規作成したテキストファイルは **LF になっていることがある** → CRLF 規約のため作成後に改行コードを確認して変換する(パイプ処理は python の `os.walk` が確実)

---

# 6. plus1 → baseup1 — 基盤刷新(DI 移行・白画面対策・メニュー再編)

フェーズ10 の記録類に続けて、**DI コンテナ移行・BACK/白画面対策・メニュー再編・ドキュメント統合**を行なった区間(2026-09-03 → 09-05)。コミットは 9 本。

## A. 画面単位の変更

### A-1. メニュー画面の再編

| 画面 | 変更 |
|---|---|
| メインメニュー(`Modules/Main/MenuView.xaml`) | **番号プレフィックス廃止**・並び替え(View → Sample → UI → App、**Setting を最後**)→ **9 段×2 列へ再構成**(Data\|Network / Sample\|App / UI 1\|UI 2 をペア行に・Setting 最終行・余り 1 行は可視の無効ボタン)+**全ボタンに Material アイコン追加**(`MenuIconButton` 化。Widgets/Navigation/Devices/Storage/Cloud/Layers/Science/Apps/Palette/Insights/Settings) |
| UI メニュー(`Modules/UI/UIMenu1*` / `UIMenu2*`) | **旧 UIMenu を UIMenu1(アプリ系 18 画面)/ UIMenu2(可視化・計器・HUD 系 13 画面)へ分離**。グループ毎に行を分け、余りセルは可視の無効ボタン。**F4 で相互遷移**、31 画面の戻り先を所属メニューへ振り分け。旧 UIMenuView/VM は削除。**各 3 列×9 段**(メニュー規約を 9 段基本へ改定。2 列化も検討したが UI 1 の 18 ボタンは 2 列×9 段=18 セルちょうどでグループ行分けが成立せず、**列数は UI 1/UI 2 で統一する方針=両方 3 列**を維持して拡張行を追加) |
| `Modules/Main/MenuViewModel.cs` | ルート画面の BACK に `AndroidHelper.MoveTaskToBack()` を結線。戻り先が無いルートでは終了せずバックグラウンドへ送る(Android の作法)。Activity が生き残るので再生成経路も踏まない |

## B. 画面以外の変更

### B-1. DI コンテナ移行(Usa.Smart.Resolver → BunnyTail.DependencyInjection 0.4.0)

`template-maui2` の 69ba9a41 と同様の変更(2026-09-03)。

- csproj: Smart.Resolver 系 2+Navigation.Resolver+MauiComponents.Resolver 参照を削除、Smart.Navigation 3.4→**3.8** / Mvvm 2.11 / BunnyTail 系整合、TrimmerRootAssembly から Resolver 系 4 行削除
- `MauiProgram`: `GeneratedServiceProviderFactory`+`IServiceCollection` 化。View/ViewModel/Context は `[ComponentRegistration]` のソース生成 `AddViews`/`AddViewModels`/`AddContexts`、HttpClient 登録も ConfigureContainer へ統合し `Services/AppHostBuilderExtensions.cs` 削除
- **`GeneratedFactory.cs` 新設**: ライブラリ内部登録型のファクトリ明示生成(Shiny 4 型+MauiComponents 8 型+App+PopupFocusPlugin+CT PopupService)
- `WizardContext`: IInitializable/IDisposable → **`IScopeLifecycle`**。`ApplicationInitializer` に DEBUG 時のフォールバック報告出力

### B-2. 移行で表面化した不具合の修正(`Shell/ShellProperty.cs` / `ShellUpdateBehavior.cs`)

退場ビューのバインディング解除が ShellProperty 変更を発火し、遷移直後のタイトル/F キー状態を旧値で上書き(Smart.Navigation 3.8 で解除順が変化)→ **現在ビューのみ反映する CurrentView ガード**を追加。

### B-3. BACK キーと Activity 再生成(白画面)対策(2026-09-04)

| ファイル | 内容 |
|---|---|
| `Platforms/Android/MainActivity.cs` | **BACK キーの受け取りを自前化**。MAUI 10 の `MauiAppCompatActivity` は `OnBackPressed()` の override を廃止し、AndroidX `OnBackPressedDispatcher` へ登録した `MauiOnBackPressedCallback` のみで BACK を処理する。その `Enabled` は `Window.CanConsumeBackNavigation`(Shell/NavigationPage/FlyoutPage/MultiPage のみ true)で決まるため、**素の ContentPage では `Page.OnBackButtonPressed` が一切呼ばれない**。`base.OnCreate` の後に自前の `OnBackPressedCallback`(`Enabled=true`)を追加して `Page.SendBackButtonPressed()` へ流す(後勝ちで先に呼ばれる)。未処理時は自身を一時無効化して `OnBackPressedDispatcher.OnBackPressed()` へフォールバック |
| `State/StartupState.cs` / `App.xaml.cs` / `MainPageViewModel.cs` / `MauiProgram.cs` | **Activity 再生成時の初期画面復帰** (2026-09-06 に方式変更)。`Application.SendStart()` は `_isStarted` ガードでプロセス内 1 回のみのため、プロセス生存のまま Activity が作り直されると `App.OnStart()` が再実行されず初回遷移が走らない → 新しい `MainPage` のコンテナが空で**白画面**。**初期画面への遷移を `MainPageViewModel.OnCreated()` へ移動**した(`MainPage.xaml` の `s:AppLifecycleBehavior` が `Window.Created` を購読するため **Activity 生成のたびに必ず走り**、復帰時の `Resumed` では走らない)。`OnCreated` を `async void` にして 「`await startup.Completed` → `Navigator.Exit()` → `ForwardAsync(ViewId.Menu)`」を実行する。起動時の初期化(DB再構築・クラッシュレポート)は `App.OnStart` に残し、完了を **`State/StartupState.cs`** へ通知する(`TaskCompletionSource` を隠蔽し `Completed` / `NotifyCompleted()` のみ公開。**完了後に待ち始めても即座に返る**ため作り直し後の ViewModel でも取りこぼさない。単発の `IReactiveMessenger` は `Subject<T>` でリプレイしないため不可)。`OnDestroying` の `destroying` フラグは初期化中に作り直された場合の二重遷移防止。旧方式(`CreateWindow` で 2 回目以降の `Window.Created` を拾う `windowCreated` / `RestoreInitialViewAsync` / 専用ログ 2 件)は撤去し `App` は元の姿へ |
| (他テンプレートへ横展開) | A-1 (BACK の受け取り) は `template-maui` / `template-maui2` / `template-maui-keyboard` / `template-maui-blazor` へ反映済み。**新方式の B-1 (`StartupState` + `OnCreated`) は `template-maui-keyboard` のみ**。`template-maui-blazor` は `INavigator` の参照が 1 箇所も無く UI が `BlazorWebView` として XAML 宣言済みのため **B-1 は対象外**で、代わりに DB 初期化のエラー処理 (`InitializeDataAsync` + ダイアログ + `Quit()`) を揃えた。`template-maui` への反映は 2026-09-06 のユーザー判断で**不要**。全プロジェクト 0 エラー・自コード由来の警告 0 |

### B-4. リソース — Images の用途別階層化(画像アセット拡充の前準備)

`Resources/Images/` を**用途別 10 フォルダへ階層化**(Banner/Character/Chat/Common/Login/Onboard/Pet/Profile/Shop/Stream=Raw と同じ PascalCase。`MauiImage` glob を `Resources\Images\**` へ変更、参照はファイル名のまま)+**プレースホルダ 42 枚を配置**(現在スロットで使用中の既存画像のコピー。実素材は同名上書きで反映)。

### B-5. ドキュメントの統合(2026-09-03)

| ファイル | 内容 |
|---|---|
| `Document/Change_Summary.md` | **本書**。タグ区間×画面単位の変更まとめとして新設(`UI_Development_Log.md` を統合して削除。ナレッジ=各区間の C 節、恒常情報=付録) |
| `Document/Task_Checklist.md` | **新規**。残作業の統合マスター(`UI_Verification_Checklist.md`+`Implementation_Checklist.md`+旧 `UI_Task_Checklist.md`+`Image_Asset_Expansion_Plan.md` を統合し、4 本とも削除) |
| `Document/Development.md`(+39行) | フェーズ10。「リスト表示」「タッチフィードバック」「Release ビルドでの検証と計測」の 3 節を追記 |

### B-6. その他

- `.editorconfig` の軽微な調整
- `MauiProgram` に `BusyState.Default` の Singleton 登録を追加

## C. この区間のナレッジ

### DI コンテナ移行(BunnyTail.DependencyInjection)

- 運用: DEBUG 起動時に `DescribeRuntimeFallbacks` の出力(そのまま貼れる属性行)を `GeneratedFactory.cs` へ貼り、ライブラリ内部で登録される型(`AddComponentsXxx` / `UseShiny` 等)のファクトリを明示生成する。自コードの `AddSingleton<T>` 等はジェネレータが自動生成する
- `Shiny.AndroidPlatform` は属性を書いてもファクトリ生成されない(生成不能な ctor)ため**リフレクションフォールバックのまま残置**(従来も Smart.Resolver のリフレクション生成であり同等)
- **退場ビューのバインディング解除が ShellProperty(バインドされた Function4Enabled 等)の変更を発火し、遷移直後のシェル状態を旧値で上書きする**(Smart.Navigation 3.8 で BindingContext 解除順が変化)→ `ShellProperty` に「現在ビューのみ反映」の CurrentView ガードを追加(症状=タイトルが 1 画面遅れる。Wizard / Lottie / Edit List などバインドを持つ画面の離脱で再現)
- ページスコープは Navigation 3.8 の DI 拡張が担う: Context を DI に Transient 登録+`IScopeLifecycle`(OnScopeInitialize/OnScopeTerminate)。`[Scope]` プロパティ注入・複数画面での共有・離脱時破棄まで従来どおり(実機で確認済み)
- `SudokuCellViewModel(int,int)` のような手動 new 前提の型も "ViewModel$" パターンで DI 登録される(解決されなければ無害。ValidateOnBuild は無効)
- 検証中に **BACK 終了→即再起動で白画面**になる事象を確認(コールド起動 / ホーム→再開は正常)。プロセス生存中の再起動で `App.OnStart` の初回ナビゲーションが走らない構造によるもので **DI 移行とは独立** → 本区間の B-3 で対策済み
- adb での Entry 入力は日本語 IME の未確定に注意: `input text` の後 KEYCODE_ENTER(66) で確定し、BACK(4) でキーボードを閉じてから画面下の F キーをタップする

---

# 7. baseup1 → fix2 — ReSharper 全件対応と Scene 描画の重大バグ修正

`jb inspectcode` の指摘 **254 件を全数分類して対応**し、その過程で発覚した **Scene 描画基盤の重大バグ(かくつき・ANR・SIGSEGV)を修正**した区間(2026-09-05)。コミットは 1 本。

## A. 画面単位の変更

### A-1. DeviceLocationView / ViewCustomView — 未取得値の「-」表示

- `FallbackValue='-'` を 8 バインドへ追加(位置**未取得**=`Location` が null でパス不成立のとき「-」表示)
- Motion 4 項目(Altitude/Course/Speed/Accuracy)は **`TargetNullValue='-'` を併用**(取得済みでも Course/Speed 等の**末端プロパティが null** のとき「-」表示。従来は単位だけが残っていた)。実機確認済み
- `AnimationOption.HighlightTrigger` の 1 バインドのみ**意図的に未適用**(FallbackValue を付けると初回位置取得時にハイライトが発火する挙動変化が出るため)

### A-2. XAML 横断(ReSharper 対応)

- **方針決定: Grid の `RowDefinitions`/`ColumnDefinitions` は Style の Setter で定義せず Grid 側に個別記述**(12 スタイル→55 Grid へインライン化。Basic 4+Data+Device 4+ViewEasing の 10 画面。値は同一のため表示不変)= `Xaml.IndexOutOfGridDefinition` 誤検知 99 件解消
- `x:Reference`/`RelativeSource` バインドの誤検知 25 件は `ReSharper disable/restore Xaml.BindingWithContextNotResolved` コメントで範囲抑止(14 箇所)
- XAML `x:Name` リネーム(`_self`→`Self`×2・`indicators`→`Indicators`)、冗長 xmlns 削除 2

## B. 画面以外の変更

### B-1. 【重大バグ修正】`Graphics/Scene/SceneObject.cs` — ダブルバッファ描画がメインスレッドで実行されていた

区間5(D8)で導入したダブルバッファの `Loop` は、`await WaitForNextTickAsync` に `ConfigureAwait(false)` が無く、`Start()`(main)からの継続が main の SynchronizationContext へ戻るため、**RenderToBuffer(フルスクリーン CPU 描画+Snapshot)+転写の 2 重描画を毎フレーム main で実行**していた。

- 症状: Scene 画面(Flight 等)で main 飽和(実測 Release 112%/Debug は 1 フレーム 3.25 秒)=**全 UI のかくつき・タップ不応答(ANR)**、退場時の Stop/Dispose 競合で **SIGSEGV**(fault addr 0x48。9/5 に 4 連発)
- 修正: ①`ConfigureAwait(false)` でループをスレッドプール化 ②`Stop()` がループ Task の完了を待ってから CTS を破棄(進行中フレームと Dispose の競合防止) ③ループ稼働中は main の直接描画フォールバックを無効化(共有 SKPaint の 2 スレッド同時使用を根絶)
- **実機検証済み(Debug/Release 両方)**: Flight 表示中 main 128%→28〜36%(描画はワーカー)・Back 即応答・退場後全スレッドアイドル・Scene 4 画面連続入退場でクラッシュ/ANR ゼロ

### B-2. ReSharper inspectcode 対応(C# 側)

- **機械修正 68 件**: 末尾カンマ削除 41 / `async`→Task 直返し 11(`HttpService` 全 API+`NetworkScpViewModel`)/ 冗長な既定値引数 4 / 空 `default: break;` 3 / 冗長 using 2(`MediaController`=CT.Maui 15 で `MediaElementState` が Core へ移動済み・`MapsuiMapManagers`)/ partial の重複基底型 1(`CalendarView.xaml.cs`)/ 空行 1 ほか
- **個別判断分(ステップバイステップ・都度ユーザー確認)**: `field` キーワード化(`UICalendarViewModel`=`#pragma IDE0032` 撤去)/ null 免罪符→**`ReSharper disable once` へ変更**(`DrawingControl`=Roslyn IDE0370 との板挟み解消。`ImageHelper` は `bitmap!`)/ `MixerEqualizer` の冗長条件 `(peak > 0)` 削除 / `BluetoothSerial` の引数 `adapter`→`bluetoothAdapter` / StyleCop SA1500(field 初期化子構文の誤検知)を `#pragma` 局所抑止
- 未使用代入 17 件は **Debug 計測(`[Conditional]` の `Debug.WriteLine`)でのみ使用のため現状維持で確定**(ユーザー決定)
- **最終残 18 件=全て確定済みの許容**(Debug 計測 17+HighlightTrigger 1)。残対応の経緯は `Task_Checklist.md` 6 節

## C. この区間のナレッジ

- inspectcode は **Bash 系シェルで実行**する(PowerShell は `--properties:` がコロンで分割され「Specify only one solution file」で失敗)。`.sln.DotSettings`(旧 .sln 名)は .slnx 解析にも適用される
- **Release 解析では `[Conditional("DEBUG")]` の `Debug.WriteLine` でのみ使う変数が RedundantAssignment 誤検知**になる(削除すると Debug ビルドが壊れる)
- **ReSharper と Roslyn の nullable 解釈が食い違うことがある**: null 代入を R# だけが指摘し、`!` を付けると Roslyn が IDE0370「抑制は不要」→ 素の null+`// ReSharper disable once` で両立。**StyleCop SA1500 は C# 14 の field 初期化子構文 `} = 値;` を誤検知** → `#pragma` 局所抑止
- `FallbackValue` は「パス不成立(親が null)」のみに効き、**末端プロパティ自体の null には `TargetNullValue`** が必要(どちらも `StringFormat` を通らず素の値が表示される)
- 稀に inspectcode のソースジェネレータ実行が COR_E_APPLICATION 例外で失敗し、CSharpErrors 数百件の不良 run になる → そのまま再実行すれば正常化する
- Scene バグの調査手法: bugreport の ANR trace(main が libSkiaSharp 内)+tombstone 一覧(`am_crash`/`am_anr` は `logcat -b events`)+`top -b -n 1 -H -p` のスレッド別 CPU+「メニューのみ=アイドル / Flight 入場で発症」の二分探索。**Mono アプリは ART 系ダンプ(`am profile` / `kill -3`)が効かない**

---

# 8. fix2 → back — BACK/初期化方式の刷新(白画面対策 B-1 の方式変更)

白画面対策の B-1(Activity 再生成時の初期画面復帰)を、**旧方式(`App.CreateWindow` での復帰)から `StartupState` 方式へ作り直した**区間(2026-09-05 → 09-06)。コミットは 4 本(サブモジュール参照更新 1 本を含む)。詳細な経緯・実測・判断の記録は `Task_Checklist.md` 0 節。

## A. 画面単位の変更

なし(シェル基盤のみ)。

## B. 画面以外の変更

### B-1. 初期画面遷移の `MainPageViewModel` への移設(`StartupState` 方式)

- **初期画面への遷移を `App.OnStart` から `MainPageViewModel.OnCreated` へ移す**。`MainPage` は `Window` 生成のたびに作り直され、`MainPage.xaml` の `s:AppLifecycleBehavior` が `Window.Created` で `IAppLifecycle.OnCreated()` を呼ぶため、**Activity の作り直しのたびに必ず走る**(復帰時は `Resumed` なので呼ばれない)
- 起動時の初期化(DB 再構築・クラッシュレポート表示)は `App` に残し、完了を **`State/StartupState.cs`(新規)** へ通知。`TaskCompletionSource` を隠して `Completed` / `NotifyCompleted()` だけを公開し、**完了後に待ち始めても即座に返る**ため、作り直しで生成し直された ViewModel でも取りこぼさない
- 旧方式(`App.CreateWindow` で 2 回目以降の `Window.Created` を拾い `RestoreInitialViewAsync`)は撤去。`windowCreated` フラグ・`CurrentViewId` ガード・専用ログ 2 件が不要になり、`App.CreateWindow` は素の実装へ戻った
- `OnDestroying` で立てる `destroying` フラグで、初期化が終わる前に作り直された場合に新旧 ViewModel が二重に遷移するのを防止
- 新方式の他テンプレートへの展開状況は区間 6 B-3 の表と `Task_Checklist.md` 0-6 を参照(`template-maui` は反映不要=ユーザー判断)

### B-2. `ApplicationInitializer` の廃止(初期化の `App` への集約)

`ApplicationInitializer`(`IMauiInitializeService`・103 行)を削除し、DB 初期化は `App.OnStart` 内の `InitializeDataAsync()` へ移動。`MauiProgram` に `StartupState` の Singleton 登録を追加。

### B-3. 細かな警告整理

アナライザ抑止の追加(`BasicStyleViewModel`=IDE0028 / `CollectionGroup`=CA1000 等)とコレクション式化(`[.. items]`)など少量の機械的整理(`LineReaderWriter` / `TreeMapNode` / `DeviceNfcViewModel` / `SampleCvLocalViewModel` / `ViewCollectionViewModel`)。

## C. この区間のナレッジ

- `Application.SendStart()` は `_isStarted` ガードで**プロセス内 1 回のみ**。プロセス生存中の Activity 再生成では `App.OnStart()` が再実行されないため、「作り直しのたびに走ってほしい初期遷移」は `Window.Created` 起点(`AppLifecycleBehavior` → `OnCreated`)へ置く
- 単発のイベントバス(`IReactiveMessenger`)は `Subject<T>` 実装で**リプレイしない**ため、再生成後に購読しても通知が来ず白画面に戻る → 完了状態の受け渡しは `TaskCompletionSource` を包んだ状態クラス(`StartupState`)で行う
- 既知の副作用: `Navigator.Exit()` は `Controller` を経由せず `provider.CloseView` を直接呼ぶため `plugin.OnClose` が走らない(= `ScopePlugin` の参照カウントが減らない)。本アプリで `[Scope]` を使うのは Navigation > Wizard の 3 画面のみで、影響は「作り直し後に Wizard の入力値が残る」程度

---

# 9. back → fix3 — Calendar / Location の手直し

## A. 画面単位の変更

### A-1. DeviceLocationView — 未取得表示を「空状態パネル」へ変更

区間 7 で入れた `FallbackValue='-'` / `TargetNullValue='-'` 方式を撤回し、**測位待ちは専用の空状態表示(Acquiring location... パネル+Pulse アニメーション)へデザイン変更**。`Location` の null 判定(`NullToBoolConverter`)で空状態と測位結果を切り替える。

### A-2. UICalendar — Debug 計測の撤去

- `MonthViewBuilder.cs` / `UICalendarViewModel.cs`: `Stopwatch`+`Debug.WriteLine` の計測コードを**撤去**(区間 7 の 6-3 で「現状維持」とした判断を変更)
- `CalendarView.xaml.cs`(旧 XAML 版): 計測は残し `// ReSharper disable RedundantAssignment` コメントで抑止(※この旧版は区間 10 で削除)
- あわせて `UICalendarViewModel` の SA1500 `#pragma` を `FirstDayOfWeek` プロパティ全体を囲む位置へ調整

## B. 画面以外の変更

なし。

---

# 10. fix3 以降(次のタグまでの変更)

### 旧 CalendarView の廃止と CalendarView2 のリネーム(C-14+D19 の実施。2026-09-06)

- **旧 XAML 版 `Controls/CalendarView.xaml(.cs)`(未参照 1,490 行)を削除**し、**Skia 自前描画版 `CalendarView2` を `CalendarView` へリネーム**(git mv。クラス名 / `x:Class` / `typeof` 参照など 70 箇所を置換)
- `UICalendarView.xaml`: タグを `controls:CalendarView` へ変更し、「タグ名を変えるだけで従来版へ切り替えられる(未決定)」の切替コメントを実態(一本化済み)へ合わせた
- `CalendarSelectionMode` 等の共有型は独立ファイルのため影響なし。ビルド警告ゼロ・実機で表示 / 月送り / イベント / 選択モードバーの動作確認済み

---

# 付録(区間に紐付かない恒常情報)

## 付録A. 開発ポリシー(恒常・実装時は常に遵守)

- 共有 `Styles.xaml` は変更しない(**BasedOn 派生 or 新規リソース辞書**で対応)
- **View の code-behind 不使用**(Behavior / Trigger / VM / コントローラパターンで実装。再利用コントロールは `Controls/` に配置可)
- ビルド**警告ゼロ**(抑制が必要な場合は事前確認。Random の CA5394 のみファイル先頭 pragma の前例=UIRadarViewModel)
- フォントサイズは許可値のみ: `6, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 32, 36, 48, 72, 96, 160`
- アイコンは `markup:Material` / `markup:Fluent`(生 Unicode・絵文字は使わない)。サイズは Material スケール(18/24/36/48)推奨
- メニューは空セル=「可視の無効ボタン」で統一済み。それ以外のメニュー差異は**意図的なので統一しない**(下表)
- Grid の `RowDefinitions`/`ColumnDefinitions` は **Style の Setter で定義せず Grid 側に個別記述**(2026-09-05 決定。ReSharper が Style 経由の定義を解決できず誤検知するため)
- ReSharper の XAML バインド誤検知(`x:Reference`/`RelativeSource` に `x:DataType` 指定済みで実動作は正常)は `<!-- ReSharper disable/restore Xaml.BindingWithContextNotResolved -->` で該当範囲のみ抑止
- コミットは実機確認後にユーザーが実施
- **設定項目の投入は設定画面の QR に統一**(`SettingParser` の `key=value`)。手入力 Entry は作らない

## 付録B. 意図的差異・統合しない判断

### メニュー画面の差異一覧(2026-07-07 実態)

メニュー間の差異には**敢えて統一していない面がある**(ユーザー指示)。統一済みは「空セルの扱い」のみ。

| メニュー | ボタンスタイル | アイコン・絵文字 | 段数(行×列) | 空セルの扱い | ガード・その他 |
|---|---|---|---|---|---|
| Main/Menu(ルート) | `MenuButton`×9 | なし(「1.Basic」等の**番号プレフィックス**) | 9×1+フッタ | なし(全セル使用) | フッタに Flavor/Version 表示 |
| BasicMenu | `MenuButton`×9 | なし | 9×1 | 空の無効ボタン1(可視) | — |
| DeviceMenu | `MenuButton`×18 | なし | 9×2 | 無効ボタン2(WiFi/Biometric=**名前付き**・可視) | 遷移先は「Not implemented」パネル |
| NavigationMenu | `MenuButton`×11 | **絵文字プレフィックス**(🍇 Edit 等) | 9×2 | 空の無効ボタン3(可視) | — |
| NetworkMenu | `MenuButton`×12 | なし | 9×2 | 空の無効ボタン2(可視) | BaseAddress 未設定時は機能ボタン無効 |
| SampleMenu | `MenuIconButton`×11 | **Material**(`markup:MenuIcon`) | 9×2 | 空の無効ボタン7(可視)※旧・非表示→統一で可視化 | — |
| SampleCvNetMenu | `MenuIconButton`×5 | **Material**(`markup:MenuIcon`) | 9×1 | 空の無効ボタン4(可視)※旧・非表示→統一で可視化 | AI 未設定時はダイアログ |
| ViewMenu | `MenuButton`×18 | なし | 9×2 | 空の無効ボタン6(可視・灰色タイル) | — |
| UIMenu | `MenuIconButton`×30 | **Material**(`markup:MenuIcon`) | 10×3 | 空セルなし(全セル使用)※Profile2/Cockpit 廃止で 11 行→10 行に縮小 | XAML コメントで比較グルーピング |

- 残る差異(意図的に維持): ①アイコン=Material 3画面・絵文字 1画面・テキストのみ 5画面 ②番号プレフィックス=Main のみ ③列数=1/2/3列混在 ④入場アニメ・PressEffect は全メニュー未使用 ⑤ガード方式(Network=ボタン無効/CvNet=ダイアログ)

※ 表は 2026-07-07 時点の実態。その後の区間5で空セルの多くが結線された(Main=10 行化で App 追加 / Basic=Setting / View=Layout・DragDrop・State・Toolkit・Custom / Sample=Sf Chart・Crop / Network=SCP / UI=11 行化で Wheel)。**差異を統一しない方針自体は不変**。
※ 2026-09-03: メインメニューの**番号プレフィックスを廃止**し並び替え(View → Sample → UI → App → Setting 最後、UI 行のみ 2 列)。**UIMenu は UIMenu1(アプリ系 18)/ UIMenu2(可視化・計器・HUD 系 13)へ分離**(F4 相互遷移)。**メニューは 8 段以上を確保し、グループ毎に行を分けて余りセルを可視の無効ボタンにする形へ統一**(ユーザー指示)。
※ 2026-09-05: **メニュー規約を 9 段基本へ改定**(ユーザー指示)。メインメニュー=9 段×2 列(関連項目 Data\|Network / Sample\|App / UI 1\|UI 2 をペア行に・Setting 最終行・余り行は無効ボタン)+**全ボタンに Material アイコン追加**。UI 1/UI 2=**各 3 列×9 段**。2 列化は UI 1(18 ボタン=2 列×9 段の 18 セルちょうど)でグループ行分けが成立しないため見送り、**UI 1/UI 2 の列数は統一する**(ユーザー決定=片方だけの 2 列化はしない)。

### 対応しない・保留と確定した項目(旧チェックリストから移設)

- **スコープ外**(外部リファレンス評価の前提): 生体認証 / カスタムハンドラ / App Actions / iOS / テーマ切替(AppThemeBinding) / 非同期検証 / セッションプロバイダ抽象
- **コードレビュー対応(区間2)での除外**: `OnNotifyFunction1` の 116 ファイル重複解消 / SemanticProperties・AutomationId の付与 / gRPC・SignalR・Ollama の実装 / QR コードからの通信先・API キー無検証受け入れ
- **保留**(必要になるまで扱わない): ダークモード対応 / ローカライズ整備 / iOS 対応 / DB マイグレーション機構
- Walkthrough(B-18)= 実装しない(D16)/ NavigationRail・月次集計(C-9/C-11)= 取り下げ(D10)/ Blazor(5-4)= 対応不要 / MBTiles(4-3)= 取りやめ(いずれも詳細は付録D と区間5 B-8)

### 画面統合・類似性分析の結論

- **画面そのものの統合価値が高いのは3組のみ**: ①FlightHud/MechHud/Telemetry/Energy(完全同型)→ **独立維持で確定** ②Profile/Profile2 → **Profile2 ベースで UIProfile へ統合完了(2026-07-07・経緯は完了記録参照)** ③Timeline/Graph(同一 Git グラフの異表現)→ **Graph2 改名で両立(実施済み)**
- その他(EC 3画面・Stream 親子・Chat/Mail・UIKit 5画面・Meter/Radar/Social 等)は**画面マージ非推奨**。部品/スタイル共通化の候補は挙がったが**対応不要で確定**(2026-07-07)
- **Radar/HUD 技術メモ**(D6=別途対応の材料): レーダー描画が2実装ある(`RadarScreen`=MAUI Graphics+外部バインド / `FlightHudScreen` 内蔵=SkiaSharp 自走)。整理するなら SkiaSharp 側(Scene 化)へ寄せるのが自然。両 API を跨ぐ描画共有は不可
- メニュー非掲載の7画面(UIItem/UICart/UIStreamDetail/UIKitNotify/UIKitSetting/UIKitOnboard/UIKitTracking)は**親子フロー**であり統合対象ではない

## 付録C. 資産レシピ表 — 既存資産の正確な名前(適用時のコピー元)

| 資産 | XAML での書き方 | 主な用途 |
|---|---|---|
| エントランス | `behaviors:AnimationOption.EnterAnimation="FadeUp\|Pop"` + `EnterDelay`(ms 段差) + `EnterTrigger`(再実行) | 静的カード/リスト行の入場 |
| 常時アニメ | `behaviors:AnimationOption.Pulse` / `Wave`+`WaveDelay` | 進行中ドット、待受/スキャン中の生感 |
| 変化フィードバック | `behaviors:AnimationOption.BounceTrigger`+`BounceValue` / `FlashTrigger` / `FadeInTrigger` / `HighlightTrigger`+`HighlightColor` | 値更新、追加/削除、タブ切替、CTA 押下 |
| バー伸長 | `behaviors:AnimationOption.ProgressTo`(ProgressBar を 800ms CubicOut で伸長) | ステータスバー・ゲージ |
| カウントアップ | `behaviors:LabelOption.CountUpValue`+`CountUpFormat`(+`CountUpDuration`) | 金額・件数・歩数(Loaded 数え上げ対応) |
| フォーカス枠 | `behaviors:Focus.FocusedStroke`+`FocusedThickness`(**親 Border 必須**) | Entry/Editor の入力体験 |
| 押下 | `behaviors:ButtonOption.PressEffect="True"`(Button/ImageButton)+`HapticFeedback` / `toolkit:SfEffectsView TouchDownEffects="Ripple"`(+`TouchDownCommand`/`TouchDownCommandParameter`) | 全タップ要素 |
| バッジ | `converters:BadgeCountConverter`(0→空、Max 超→「99+」) | 件数バッジ |
| アイコン | `{markup:Material Glyph={x:Static fonts:MaterialIcons.Xxx}, Color=.., Size=..}` / `{markup:Fluent ..}` / `{markup:MenuIcon ..}` | 絵文字・生 Unicode の置換(バインド不可な点に注意) |
| カード/チップ/ステップ | `controls:InfoCard`(Title/Icon/IconColor+Content)/ `controls:StatusChip`(Text/Icon/ChipColor/IconColor/TextColor)/ `controls:StepIndicator`(CurrentStep/TotalSteps/AccentColor) | 第2弾で新設した共通部品 |
| 空状態 | `CollectionView.EmptyView` / 中央 VStack+円形アイコン(96)+説明の定型 | 0件/未取得/未実装の表示 |
| その他 | `CameraOverlayView`(撮影ガイド枠)/ `MapBind`+`MapController(.MoveTo)` / `EasingCurveView` / `JetBrainsMono`(等幅数値)/ `NotoSerifJP`(Skia 日本語) | — |

## 付録D. 外部リファレンス評価 決定・不採用アーカイブ(旧 Reference_Analysis.md / Reference_Summary.md より)

51 件 (S-01〜S-51) を評価し、採用分は全て実装完了 (2026-09-01〜02)。QR ペイロード例や実装対象は各完了記録を参照。

### 決定事項 (D1〜D22)

| # | 決定 |
| --- | --- |
| D1 | NuGet は同等機能が既存に無い場合のみ追加 (実績: SSH.NET のみ。BlazorWebView 用は 5-4 不要化により追加せず) |
| D2 | アプリ風サンプルは `Modules/App/` + トップメニュー `10.App` (電卓 / 数独) |
| D3 | 電卓は科学電卓。複雑化時は ①四則+%+括弧 → ②三角関数等 → ③累乗・階乗 の順で削る |
| D4 | 設定画面サンプルは `Modules/Basic/BasicSettingView` |
| D5 | ミニゲームは数独 (盤面モデル差し替え可能にしてライフゲーム / 2048 に備える) |
| D6 | リスト D&D は `Modules/View/ViewDragDropView` |
| D7 | ロケールのみ強化 (9-3)・テーマ (`AppThemeBinding`) は触らない |
| D8 | ダブルバッファ = 試験導入 → Release 実測 (30fps→60fps) → **本採用・既定 ON** |
| D9 | タブ / ボトムシートは Syncfusion 本線 |
| D10 | TimeRecorder 由来は新規画面を作らず既存スケジュール強化のみ (9-2) |
| D11 | 非同期検証は実装しない (相関検証のみ) |
| D12 | 未使用機能サンプルは種別別新規画面 + 既存画面追記の併用 |
| D13 | Shiny Controls パッケージは参照しない |
| D14 | ChatView 機能追加は全て見送り。`RemainingItemsThreshold` は `ViewCollectionView` へ振り替え |
| D15 | Scheduler は `IScheduleEventProvider` 化のみ (9-2 で実施予定) |
| D16 | Walkthrough は実装しない (方針メモ: `Grid` 全面オーバーレイ + `Border` くり抜き + 対象要素の絶対座標取得 + `ScrollView` 内追従に注意 + 初回判定は `State/Settings.cs`) |
| D17 | 自作入力は `ColorPicker` / `DurationPicker` のみ (RangeSlider / AutoComplete は難度中で見送り) |
| D18 | チャット UI の二重実装 (`Controls/ChatView` ⇔ `UIChatView`) は現状維持。C-13 (バブル色) は検討扱い |
| D19 | 旧 `CalendarView` (未参照 1,490 行) の削除/リネームは**後日対応** (`CalendarView2` が正。C-14 のコメント実態合わせも同時) → **2026-09-06 実施済み**(区間 10) |
| D20 | SSH.NET 2026.0.0 追加 (増分 = BouncyCastle.Cryptography のみ) |
| D21 | SCP のみ (SFTP / コマンド実行は対象外) |
| D22 | 設定投入は設定画面の QR に統一 (全項目)。D22-a = パスワード認証のみ / D22-b = **指紋設定は撤去し参考表示のみ** (2026-09-02 変更。当初の QR 配布指紋照合は撤去) |

### 不採用 (1) — サンプルとしては不要だが、ライブラリ / ツール / 資料としては有用

LiveCharts2 (自前 ChartDrawing + Syncfusion で充足) / Sharpnado.Tabs (SfTabView で充足) / Maui.VirtualListView・MPowerKit.VirtualizeListView (データ規模的に不要) / AiForms.SettingsView (BasicSettingView で達成) / MPowerKit.GoogleMaps (API キー前提。マネージャ分割設計のみ Mapsui 実装へ反映済み) / ArcGIS (商用) / Maui.Nuke (iOS スコープ外) / ImageCropper.Maui (ネイティブラッパ。自作 = 9-6) / Evergine 3D / DrawnUI 全面採用 (実験的。SKPicture キャッシュ等の部分技法は実装済み) / Grial FluentEmoji (CDN 依存) / CSLA (相関検証のみ 3-6 へ) / LocalizationResourceManager (根本切替は不要) / AlohaKit.Layouts (CircularLayout のみ自作済み) / TemplateMAUI (Marquee/TreeView のみ自作済み) / Plugin.Maui.SegmentedControl (SfSegmentedControl で充足) / GitTrends・WeatherTwentyOne・showcase (資料) / dotnet-maui-templates・MauiAppAccelerator (開発ツール) / Shiny Controls の DataGrid・FrostedGlass・Mermaid・Tray (コスト高 / デスクトップ向け)

### 不採用 (2) — 本サンプル側が優れた / 同等の実装を持つため参考自体が不要

AlohaKit.Controls (13/15 既存充足。設計思想も DrawingObject/DrawingControl として実装済み) / Grial SvgImage (SvgView が同構成) / The49 ViewClickListener・AiForms AddCommandEffect (TouchBehavior + ButtonOption で充足) / AiForms FAB (MapFabButton スタイルで充足) / slideshare 標準 UI 論 (UISocial 等で実装済み) / SimpleCalculator (ToTrimmedString のみ反映) / 数独記事 (題材のみ) / MauiScientificCalculator csproj (UI 構成のみ反映) / TimeRecorder アーキテクチャ (UI 要素のみ 9-2 へ) / PhotoAlbum バックエンド構成 / All the Lists (基準の明文化のみ 10-1 へ) / ライフサイクル記事 (実例のみ反映) / Doom.Mobile (Release 計測の知見のみ) / Breakout (プール / 論理解像度 / 状態機械を実装・反映済み) / Shiny の既存充足分 (Wizard / Parallax / SignaturePad / Toast / Shimmer / Badge / OTP / TreeView / CameraView ほか)
