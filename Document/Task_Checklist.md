# 残作業チェックリスト

実装フェーズは全完了(2026-09-03)。**残作業(実機確認 / 実テスト / 画像アセット / 保留)を 1 本で管理する**マスターチェックリスト。
2026-09-03 に `UI_Verification_Checklist.md` + `Implementation_Checklist.md` + 旧 `UI_Task_Checklist.md` + `Image_Asset_Expansion_Plan.md` を統合した。
経緯・実装内容・ナレッジ・開発ポリシーは `Change_Summary.md`(付録含む)を参照。

**現在の優先事項 = 7 節**(`tmpl-plan-maui.md` からの移管課題。2026-09-06 組み込み)。以降は 1 節(実機確認)→ 2〜5 節の順。

## 運用ルール

- 作業はこの番号で指示・進行する(例:「4-4-A を実施」)。完了した項目は `[x]` にし、行末に完了日を追記。問題があれば行末にメモ
- **【判断】印の項目はユーザーが決定**(勝手に進めない)。デザイン判断を伴う差分は 1 項目ずつ指示を受けて実施
- 実装・変更を行なう場合の完了条件 = **ビルド警告ゼロ** + `Change_Summary.md` への記録(開発ポリシーは同 付録A)
- コミットはユーザーが実施(グループ単位を推奨)
- 描画・性能の計測は **Release ビルド + 実機**(手順は `Development.md` の「Releaseビルドでの検証と計測」)

## 前提(環境)

- **環境制約 (不具合ではない)**: ①地図タイルは Google Maps API キー未設定だと非表示 (ピン・カメラ移動は動作) ②SampleCvNet 系は AI エンドポイント未設定だと画面に入れない ③CommunityToolkit CameraView の `CaptureAsync` がまれに未完了になり Function キーが無反応化 (再起動で回復)
- 現在実機に入っているのは **Release ビルド**

---

## 0.【最優先】BACK 終了→再起動の白画面対策

**事象**: BACK でアプリを終了した直後にランチャーから再起動すると白画面になる(`am start` の直接起動でも同事象)。コールド起動とホーム→再開は正常。DI 移行(2026-09-03)とは独立の経路で、移行検証中に発見。

- [x] **0-1** 調査 (2026-09-03) — Pixel 9a / Android 16 / Release ビルドで再現・計測。**独立した 2 つの不具合の連鎖**と判明(下記 原因A / 原因B)

### 原因A: BACK がアプリに届かない (MAUI 10 での回帰)

実測 (`adb logcat -s AppLifecycle:D App:D` / `pidof`):

| 操作 | 実測結果 |
| --- | --- |
| Menu 画面で BACK | `OnPause`→`OnStop`→`OnDestroy`。**プロセスは生存**(pid 不変) |
| Basic メニュー画面で BACK | 親メニューへ戻らずアプリ終了。`BasicMenuViewModel.OnNotifyBackAsync` は呼ばれない |
| 再起動 (`am start`) | `LaunchState: WARM`・pid 不変・`App` コンストラクタのログ (`Application start`) が出ない |

→ 常に `true` を返す `MainPage.OnBackButtonPressed()` が**一度も呼ばれていない**。つまり BACK は「アプリ内戻り」も「終了の抑止」もできていない。

原因は MAUI 10.0.100 の Android 実装:

- `MauiAppCompatActivity` は `OnBackPressed()` の override を**廃止**し(メタデータ確認済み。MAUI 9 には存在した)、AndroidX `OnBackPressedDispatcher` に登録した `MauiOnBackPressedCallback` のみで BACK を処理する
- その `Enabled` は `ShouldRegisterPredictiveBackCallback()` = `Window is IBackNavigationState { CanConsumeBackNavigation: true }` で決まる
- `Window.CanConsumeBackNavigation(Page)` は Shell / NavigationPage / FlyoutPage / MultiPage 以外(= 素の `ContentPage`)では **常に false**(MAUI 側コメント: カスタムページの `OnBackButtonPressed` の戻り値は事前に判定できないため back-to-home アニメーションを潰さない方針)
- 本アプリの `Window.Page` は素の `ContentPage` (`MainPage`) なのでコールバックは無効のまま → システム既定の BACK (Activity finish) が走る

補足:

- Android 16 / targetSdk 36 では `onBackPressed` も `KEYCODE_BACK` も配送されない。加えて MAUI 側に override が無いため、`android:enableOnBackInvokedCallback="false"` の退避策では**直らない**(dispatcher に戻るだけで有効なコールバックが無い)
- 参考: dotnet/maui#31266 (Page.OnBackButtonPressed の見直し) / dotnet/maui#33523

### 原因B: プロセス生存中の Activity 再生成に耐えられない

原因A で Activity が finish されると、プロセス生存のまま Activity だけが作り直される。このとき:

1. `MainActivity.OnCreate` → `App.CreateWindow` が再度呼ばれ、**新しい `MainPage`** が DI (Transient) から生成される
2. MAUI の `Application.SendStart()` は `_isStarted` ガードでプロセス内 1 回のみ → **`App.OnStart()` は再実行されない** → 唯一の初回遷移 `navigator.ForwardAsync(ViewId.Menu)` が走らない
3. `INavigator` は Singleton なのでスタックは旧 View を保持したまま。`NavigationContainerBehavior` → `ContainerResolver.Attach()` は**参照を差し替えるだけ**で、表示中 View を新コンテナへ付け替えない
4. 結果として `AbsoluteLayout` が空 → 白画面。スクリーンショットではステータスバーだけ青い(新 `MainPage` の `StatusBarBehavior` は動作)、ヘッダー/ファンクションは新 `MainPageViewModel` の初期値で非表示

**原因B は BACK と無関係に再現する**(実測): アプリ表示中に端末のフォントサイズを変更すると `OnPause`→`OnStop`→`OnDestroy`→`OnCreate` と Activity が再生成され(pid 不変)、画面が完全に空になる(uiautomator でテキスト 0 件)。`ConfigurationChanges` に `FontScale` / `Locale` が無いため。**通常の利用操作で踏める不具合**なので、原因A を直しても原因B は必ず対処が必要。

**Activity の設定(launchMode 等)では直せない**(実測): BACK で Task / ActivityRecord は完全に消滅し(`Task #83` → 消滅 → 再起動で `Task #84`)、プロセスだけが `oom_score_adj` 0→900 の空プロセスとして残る。launchMode は「**既存インスタンスがあるとき**にどう再利用するか」の設定なので、finish 済みで再利用対象が無い今回は `singleTop` / `singleTask` / `singleInstance` のいずれでも新規 `OnCreate` になる。`alwaysRetainTaskState` もタスク生存中の話。manifest に「BACK で finish させない」スイッチは無い。

- [x] **0-2** 【判断】対策方針の選定 (2026-09-04) — **A-1 + B-1 + 任意項目**を採用
  - **A-1 (採用)** `MainActivity` で自前の `OnBackPressedCallback` を `Enabled = true` で `OnBackPressedDispatcher` に登録し、`Page.SendBackButtonPressed()` へ流す。`base.OnCreate` の後に追加することで MAUI のコールバックより後勝ちで確実に受け取れる。アプリ側ロジック(`MainPage.OnBackButtonPressed` → `ShellEvent.Back`)は現状のまま使える。代償はシステムの back-to-home アニメーションが出なくなること
  - A-2 (不採用) `MainPage` を `NavigationPage` 等でラップして `CanConsumeBackNavigation` を true にする案。シェル構造(ヘッダー/ファンクション/コンテナ)の作り直しが必要で影響が大きい
  - A-3 (不採用) MAUI 側の修正待ち。dotnet/maui#31266 は提案段階(.NET 10 SR11 マイルストーン)で時期未定
  - **B-1 (採用・2026-09-06 に方式変更)** 初期画面への遷移を `App` から `MainPageViewModel` へ移す。`MainPage` は `Window` 生成のたびに作り直され、`MainPage.xaml` の `s:AppLifecycleBehavior` が `Window.Created` を購読して `IAppLifecycle.OnCreated()` を呼ぶため、**Activity の作り直しのたびに必ず走る**(復帰時は `Resumed` なので呼ばれない)。`OnCreated` を `async void` にして「初期化状態の完了を待つ → `Navigator.Exit()` → `ForwardAsync(初期ViewId)`」の3行にする
    - 起動時の初期化(DB再構築・クラッシュレポート表示)は `App` に残し、完了を `State/StartupState.cs` へ通知する。`TaskCompletionSource` を隠して `Completed` / `NotifyCompleted()` だけを公開するため、`App` も `MainPageViewModel` も仕組みを意識しない。**完了後に待ち始めても即座に返る**ので、作り直しで生成し直された ViewModel でも取りこぼさない
    - 単発のイベントバス(`IReactiveMessenger`)は不可。`Subject<T>` 実装でリプレイしないため、再生成後に購読しても通知が来ず白画面に戻る
    - `OnDestroying` で立てる `destroying` フラグは、**初期化がまだ終わっていない最中に作り直された**場合に古い ViewModel と新しい ViewModel の両方が遷移してしまうのを防ぐためのもの
    - 旧方式(`App.CreateWindow` で 2 回目以降の `Window.Created` を拾い `RestoreInitialViewAsync` を実行)は撤去。`windowCreated` フラグ・`CurrentViewId` ガード・専用ログ2件が不要になり、`App` は元の姿へ戻った
    - 既知の副作用: `Navigator.Exit()` は `Controller` を経由せず `provider.CloseView` を直接呼ぶため `plugin.OnClose` が走らない(= `ScopePlugin` の参照カウントが減らない)。本アプリで `[Scope]` を使うのは Navigation > Wizard の 3 画面のみで、影響は「作り直し後に Wizard の入力値が残る」程度
    - 【判断保留】`StartupState` という名前は仮。「初期化状態」寄りにするか「スタートアップ状態」寄りにするかは要再確認
  - B-2 (不採用) コンテナ再接続。破棄済み Activity / MauiContext のハンドラを持つ View の付け替えになりリスク高(将来案)
  - B-3 (不採用) Window/MainPage の再利用。`Window.Destroying()` で `RemoveWindow` + `Handler.DisconnectHandler()` が走るため非推奨
  - **任意 (採用)** ルート画面 `MenuViewModel.OnNotifyBackAsync` → `AndroidHelper.MoveTaskToBack()`。A-1 だけだとルート画面の BACK が無反応になるため、Android の作法に合わせてバックグラウンドへ送る。Activity が生き残るので白画面経路も踏まない
- [x] **0-3** 対策A の実装 (2026-09-04) — `Platforms/Android/MainActivity.cs`(`BackPressedCallback` 追加。未処理時は自身を一時無効化して `OnBackPressedDispatcher.OnBackPressed()` へフォールバック)
- [x] **0-4** 対策B の実装 (2026-09-04 / 2026-09-06 に方式変更) — `State/StartupState.cs`(新規)、`MauiProgram.cs`(`services.AddSingleton<StartupState>();` 1行)、`App.xaml.cs`(`OnStart` の末尾で `NotifyCompleted()`。`CreateWindow` は素の実装に戻す)、`MainPageViewModel.cs`(`StartupState` 注入 + `OnCreated` を `async void` 化 + `OnDestroying` で `destroying`)。`Log.cs` への追加は不要。ビルド警告ゼロ(自コード由来 0 件。残 10 件は BLE バインディング由来の既存 Release 警告)
- [x] **0-5** 確認 (2026-09-04・Release ビルド・Pixel 9a / Android 16) — 全経路 OK

| 確認項目 | 結果 |
| --- | --- |
| ①コールド起動 | `LaunchState: COLD` → Menu 表示 |
| ②ホーム→再開 | `OnStop`→`OnStart`/`OnResume` のみ、Menu 表示 |
| ③サブ画面の BACK | Basic → BACK → **Menu へ復帰**(従来はアプリ終了) |
| ④ルート画面の BACK | ランチャーへ戻るが **`OnDestroy` 無し・pid 不変**(バックグラウンド化) |
| ⑤BACK 終了→即再起動 | BACK で終了しなくなったため経路自体が消滅。復帰は Menu 表示 |
| ⑥`am start` 直接起動 | Menu 表示 |
| ⑦フォントサイズ変更 | `OnDestroy`→`OnCreate` 後に `Window recreated. Restore initial view.` ログ → **Menu 表示**(従来は完全な空画面) |
| ⑧他アプリ切替→復帰 / プロセス kill→再起動 | いずれも Menu 表示 |

- [x] **0-6** 他テンプレートへの反映 — 全プロジェクトで **0 エラー・自コード由来の警告 0**

| プロジェクト | A-1 | B-1 の方式 | ルート BACK | 確認 |
| --- | --- | --- | --- | --- |
| `Works3/Template` | 済 | **新方式**(`StartupState` + `OnCreated`) | `MenuViewModel` | **実機確認済み (2026-09-06)** |
| `template-maui-keyboard` | 済 | **新方式**(`ViewId.KeyMenu`) | `KeyMenuViewModel` | **実機確認済み (2026-09-04)**。2026-09-06 に Works3 の確定形へ整合(ReSharper ディレクティブ追加 / コメント統一 / 登録順) |
| `template-maui` | 済 | 旧方式のまま | `MenuViewModel` | **対象外 (2026-09-06 ユーザー判断で反映不要)**。Works3 と 4 ファイル同一の不変条件は現在適用外 |
| `template-maui2` | 済 | 旧方式のまま | `MenuViewModel` | 未追従。既存警告 24 件は全て XA4301 |
| `template-maui-blazor` | 済 | **対象外**(`INavigator` の参照が無い) | `MainPage.OnBackButtonPressed` | 実機確認済み (2026-09-04 / 2026-09-06)。2026-09-06 に DB 初期化のエラー処理を Works3 と同形へ揃えた |

> `template-maui-blazor` は `App.OnStart` に画面遷移が無く、UI が `MainPage.xaml` の `BlazorWebView` として宣言済み、かつ `INavigator` の参照が 1 箇所も無いため **原因B が成立しない**。A-1 のみ入れ、TODO スタブだった `MainPage.OnBackButtonPressed` に `AndroidHelper.MoveTaskToBack()` を足して他テンプレートとルート挙動を揃えた

> **Works3/Template の実機確認 (2026-09-06・Release)**: コールド起動(DB初期化待ち後に Menu 表示)/ Data メニュー(DB依存画面)が開く / `--activity-clear-task` で Menu へ復帰 / Basic → ホーム → 復帰で Basic を維持 / Basic で BACK → Menu / ルート BACK でバックグラウンド化(pid 不変)/ **フォントサイズ変更で `OnDestroy`→`OnCreate` 後に Menu 表示**(検証中に偶然 1.15→1.0 の変更が発生した際も正しく復帰)

> 遷移前の待機中は **clickable 0 / focusable 0 / 表示テキストなし**(keyboard で起動を6秒遅らせて実測)。`MainPage` のヘッダー・ファンクションボタンはツリーには存在するが `IsVisible` が既定 `false` のため誤タップの余地は無い

**再現手順 / 確認手順** (adb は PATH 未登録・`C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe`):

```
adb shell am force-stop template.mobileapp
adb logcat -c
adb shell am start -W -n template.mobileapp/template.mobileapp.MainActivity
adb shell input keyevent KEYCODE_BACK
adb shell pidof template.mobileapp            # 対策後は BACK でも生存し続ける
adb shell am start -W -n template.mobileapp/template.mobileapp.MainActivity
adb shell uiautomator dump /sdcard/ui.xml && adb shell cat /sdcard/ui.xml   # テキスト 0 件なら白画面
adb logcat -d -s AppLifecycle:D App:D         # Window recreated ログの有無
adb shell settings put system font_scale 1.30 # 構成変更で Activity 再生成 (確認後 1.0 へ戻す)
```

> **実機への入れ替え時の注意**: 端末に入っていたビルドは **IDE デプロイの debug 署名**(`CN=Android Debug`)で、`dotnet build -c Release` が出す `example.keystore` 署名の APK では `INSTALL_FAILED_UPDATE_INCOMPATIBLE` になる。アンインストール(データ消去)を避けるには、ビルド済み APK を **`%LOCALAPPDATA%\Xamarin\Mono for Android\debug.keystore`**(storepass/keypass=`android`、alias=`androiddebugkey`)で `apksigner sign` し直してから `adb install -r` する。`~/.android/debug.keystore` は別鍵なので不可

---

## 1. 実機確認(人が実機で見て確認する)

外部リファレンス評価 フェーズ1〜9 の実機確認。確認観点は画面単位の表+チェック項目。

### 1-1. フェーズ1(2026-09-01・外部リファレンス評価)

基盤変更のため「見た目が変わらないこと」の確認が中心。

| 画面 | 現在のファイル名 | 変更 | 確認 |
| --- | --- | --- | --- |
| View > Svg | `Modules/View/ViewSvgView.xaml` | `SvgView.Source` バインドへ切替(ロードはコントロール内へ) | 下記 |
| UI > Flight | `Modules/UI/UIFlightView.xaml`(FlightHudScene) | レーダー盤面+ロール目盛をキャッシュ化 | 下記 |
| UI > Tactical | `Modules/UI/UITacticalView.xaml`(MechHudScene) | マップ静的層(枠/等高線/グリッド/ラベル)をキャッシュ化 | 下記 |
| UI > Telemetry | `Modules/UI/UITelemetryView.xaml`(TelemetryScene) | タコメーター盤面+Gフォース盤面をキャッシュ化 | 下記 |
| UI > Energy | `Modules/UI/UIEnergyView.xaml`(EnergyFlowScene) | ドット背景をキャッシュ化 | 下記 |
| (画面なし) | `MauiProgram.cs` | Android ライフサイクルのログフック追加 | logcat のみ |

- [ ] **View > Svg**: 3つのチップ(.NET Bot / Vite / React)で画像が切り替わる。2回目以降の切替が即時(キャッシュ)。ファイル名表示が追随する
- [ ] **UI > Flight**: レーダーの円/十字/目盛が従来どおり表示され、スイープ・機影・姿勢儀・テープ類が滑らかに動く。ロール目盛(上部円弧)が消えていない
- [ ] **UI > Tactical**: 等高線・グリッド・OBJ-A ダイヤ・D2/D3 ラベル・「TAC MAP」見出しが表示される。味方/敵マーカーとピングリングが動く
- [ ] **UI > Telemetry**: タコメーターの赤帯/目盛/ラベルが表示され、針と値アークが動く。Gフォースの円/十字/破線円が表示され、ドットが動く。「×1000 r/min」「ENGINE RPM」ラベルがある
- [ ] **UI > Energy**: ドット背景が表示され、フロー線のダッシュが流れる
- [ ] **4シーン共通**: 画面を出て入り直しても表示が壊れない(キャッシュの Dispose/再生成)
- [ ] (任意) 体感で従来よりカクつきが減っている(D8 で計測済み → ダブルバッファ本採用)

---

### 1-2. フェーズ2(2026-09-01・抽選ホイール)

| 画面 | 現在のファイル名 | 変更 |
| --- | --- | --- |
| UI メニュー | `Modules/UI/UIMenuView.xaml` | 11 行目追加 (Wheel + 無効ボタン 2 セル) |
| UI > Wheel | `Modules/UI/UIWheelView.xaml` (新規) | 抽選ホイール画面 |

- [ ] **UI メニュー**: 11 行目に Wheel (観覧車アイコン) が表示され、右 2 セルは無効ボタン。既存 30 ボタンの並びが崩れていない
- [ ] **UI > Wheel**: 8 色の扇形+ラベル (ラーメン/カレー/寿司/パスタ/焼肉/そば/ハンバーガー/サラダ) が円形に描かれ、文字が半径方向に沿って回転配置されている
- [ ] SPIN ボタンで約 4 秒かけて減速回転し、停止後に RESULT へ当選名が表示される (バウンド演出付き)。停止位置 (上部ポインタが指す扇形) と表示名が一致する
- [ ] ホイール自体をタップしても回転する。**回転中の再タップ/再押下は無視される** (二重回転しない)
- [ ] 回転中に Back で抜けて再入場しても壊れない (結果は「？」に戻る)
- [ ] 何回か回して異なる結果が出る

---

### 1-3. フェーズ3(2026-09-01・未使用機能のサンプル化)

| 画面 | 現在のファイル名 | 変更 |
| --- | --- | --- |
| Basic > Setting | `Modules/Basic/BasicSettingView.xaml` (新規) | 標準入力コントロール網羅 + ToolTip |
| View > Layout | `Modules/View/ViewLayoutView.xaml` (新規) | DockLayout / UniformItemsLayout |
| View > State | `Modules/View/ViewStateView.xaml` (新規) | StateContainer / LazyView |
| View > Toolkit | `Modules/View/ViewToolkitView.xaml` (新規) | SfTabView / SfBottomSheet / 入力・表示部品 |
| Sample > Sf Chart | `Modules/Sample/SampleSfChartView.xaml` (新規) | Syncfusion チャート 6 種 |
| View > Effect | `Modules/View/ViewEffectView.xaml` | 末尾に Confetti / Touch / IconTint 追加 |
| View > Refresh | `Modules/View/ViewRefreshView.xaml` | スケルトンに SfShimmer の波 |
| Basic > Behavior | `Modules/Basic/BasicBehaviorView.xaml` | Masked / UserStoppedTyping / EventToCommand |
| Basic > Validation | `Modules/Basic/BasicValidationView.xaml` | 相関検証 (Compare) + CT 検証 Behavior |
| View > Collection | `Modules/View/ViewCollectionView.xaml` | RemainingItemsThreshold 無限スクロール |

- [ ] **Basic > Setting**: Stepper/Slider/Switch/DatePicker/TimePicker/RadioButton/Picker/SearchBar が操作でき、下部 Summary に値が反映される。**任意のコントロールを長押しするとツールチップが出る** (A-12)
- [ ] **View > Layout**: DockLayout の 5 領域 (Top/Bottom/Left/Right/Fill) が色分け表示される。UniformItemsLayout が 4 列均等で A〜H を並べる
- [ ] **View > State**: 4 ボタンで Loading (スピナー) / Empty / Error / Success が切り替わる。「コンテンツを読み込む」で遅延パネルが FadeUp 表示される (押すまで出ない)
- [ ] **View > Toolkit**: タブ 3 枚がスワイプ/タップで切替可。OTP 4 桁入力が Value 表示に反映 / セグメント選択 / チップ選択が動く。表示タブでアバター 3 種・**星評価のタップ変更**・Expander/Accordion の開閉。シートタブのボタンで**ボトムシートが下から出て、グラバーで半開⇔全開**
- [ ] **Sample > Sf Chart**: Column/Doughnut/Polar/Funnel/Pyramid/Spark 3 種/Sunburst が描画される (入場アニメあり)
- [ ] **View > Effect** (末尾セクション): Celebrate で紙吹雪が降り、もう一度押すと止まる。TouchBehavior カードは押すと縮み、長押しでカウント増。account 画像 3 つのうち 2 つが青/赤に着色
- [ ] **View > Refresh**: 入場直後のスケルトンに**波 (シマー) が走る** (従来は静止だった)
- [ ] **Basic > Behavior** (末尾カード): 電話番号が自動で 000-0000-0000 に整形される。入力を止めて 800ms で「確定: …」が出る。Switch を切り替えるとイベント発火カウントが増える
- [ ] **Basic > Validation** (末尾 2 カード): Password と Confirm が不一致のまま Confirm からフォーカスを外すとエラー表示。**その後 Password 側を修正して一致させるとエラーが自動で消える** (相関再検証)。Email/数値 (1〜100) は入力の度に文字色が赤/緑に変わる
- [ ] **View > Collection**: 最下部までスクロールすると「追1」「追2」…グループが自動追加され、フッターの件数が増える (16 で停止)

---

### 1-4. フェーズ4(2026-09-02・地図強化)

| 画面 | 現在のファイル名 | 変更 |
| --- | --- | --- |
| Sample > Map | `Modules/Sample/SampleMap1View.xaml` | FAB 3 個追加 (MapElements) |
| Sample > Map2 | `Modules/Sample/SampleMap2View.xaml` | トグルパネル + マネージャ 5 種 + Skia オーバーレイ |

※ Map (Google) は API キー未設定だとタイルが出ない (要素の描画は動作する)。Map2 (Mapsui/OSM) はキー不要。

- [ ] **Sample > Map**: 右上 FAB の Route (青) で 4 スポットを結ぶ青線、Pentagon (緑) で皇居周辺の半透明緑ポリゴン、Circle (赤) で東京駅中心の赤円が表示され、再タップで消える
- [ ] **Sample > Map2 (Widget)**: 初期表示で左下にスケールバー、左上トグルパネルの下に +/− ズームウィジェット。OFF で消える
- [ ] **Sample > Map2 (Spot)**: ON で赤丸ピン 4 個。**ピンをタップすると吹き出し (名前+住所) が出て、再タップ/他のピンタップで閉じる**
- [ ] **Sample > Map2 (Shape)**: ON で青の経路線と緑の半透明ポリゴン
- [ ] **Sample > Map2 (GeoJSON)**: ON でオレンジの線 (隅田川)、紫のポリゴン (上野公園)、紫の三角 (秋葉原) + 名前ラベル
- [ ] **Sample > Map2 (Cluster)**: ON で青い数字入りクラスタ円。**ズームインすると分解されて緑の点になり、ズームアウトでまとまる**
- [ ] **Sample > Map2 (Overlay)**: ON で青→赤のグラデーション経路 (白縁取り) が表示され、**地図をパン/ズームしても経路が地図に追従する**
- [ ] Map2 で画面を出て入り直しても壊れない (マネージャの Detach/Dispose)

---

### 1-5. フェーズ5(2026-09-02・D&D / 電卓 / 数独)

| 画面 | 現在のファイル名 | 変更 |
| --- | --- | --- |
| トップメニュー | `Modules/Main/MenuView.xaml` | `10.App` 追加 (10 行目) |
| App メニュー | `Modules/App/AppMenuView.xaml` (新規) | Calculator / Sudoku |
| App > Calculator | `Modules/App/AppCalcView.xaml` (新規) | 科学電卓 (純モデル + DSEG7) |
| App > Sudoku | `Modules/App/AppGameView.xaml` (新規) | 数独 (純モデル + MVVM) |
| View > DragDrop | `Modules/View/ViewDragDropView.xaml` (新規) | ドラッグ&ドロップ 3 種 |

- [ ] **トップメニュー**: 10 行目に「10.App」が表示され、App メニュー (Calculator/Sudoku + 無効ボタン) に遷移できる
- [ ] **Calculator**: `2+3×4` → `=` で **14** (優先順位)。`sin(30)` → **0.5**。`5!` → **120**。`2π` → **6.2831853072** (暗黙の乗算)。`50%` → **0.5**。`2^3^2` → **512** (右結合)。`1÷0` → エラー表示。結果が **7 セグ (DSEG7)** で表示される。`=` の直後に `+2` と押すと結果から継続する。C / ⌫ が効く
- [ ] **Sudoku**: 9x9 盤面 (3x3 の区切りが太い) が表示され、太字=問題・空欄=入力可。セルをタップ→選択色、数字パッドで入力 (青字)。**同じ行/列/ボックスに重複させると赤字**になる。「消去」で消える。「新しい問題」で盤面が変わる。全マス正しく埋めると **COMPLETE!** が出る (お試しは矛盾なく埋めるだけでも可)
- [ ] **View > DragDrop**: 並べ替えカードで行を**長押しドラッグ**→別の行に重ねて離すと順序が変わる。TODO の項目を DONE 列へドラッグで移動 (逆も可)。**ゴミ箱の上に持っていくと赤くハイライト**され、離すと削除される
- [ ] 電卓/数独とも画面を出て入り直しても壊れない

---

### 1-6. フェーズ7(2026-09-02・自作レイアウト / コントロール)

| 画面 | 現在のファイル名 | 変更 |
| --- | --- | --- |
| View > Layout | `Modules/View/ViewLayoutView.xaml` | 自作 3 カード追記 (Circular / Staggered / Cascade) |
| View > Custom | `Modules/View/ViewCustomView.xaml` (新規) | 自作コントロール 4 種 |

- [ ] **View > Layout (CircularLayout)**: 月〜日の 7 チップが円周に均等配置され、中央に WEEK
- [ ] **View > Layout (StaggeredGrid)**: 高さの違うカード 1〜8 が 3 列に隙間少なく詰まれる (番号が列をまたいで低い列へ流れる)
- [ ] **View > Layout (Cascade)**: Window 1〜4 が右下へずれながら重なる (ILayoutManagerFactory による差し替え)
- [ ] **View > Custom (MarqueeLabel)**: 2 本のテキストが右→左へ流れ続ける (下は高速)。画面を出て戻っても流れる
- [ ] **View > Custom (TreeView)**: ▸ タップで展開/折りたたみ、行タップで選択され青ハイライト + 「選択: …」表示
- [ ] **View > Custom (ColorPicker)**: RGBA スライダでプレビューと hex が変わり、下の丸 (VM バインド) も同色になる
- [ ] **View > Custom (DurationPicker)**: 時間/分を変えると TimeSpan 表示が追随する (初期 01:30:00)

---

### 1-7. フェーズ8(2026-09-02・Graphics 拡張。今回は Release ビルド)

| 画面 | 現在のファイル名 | 変更 |
| --- | --- | --- |
| Sample > Chart | `Modules/Sample/SampleChartView.xaml` | セグメント 2 段化 + Stacked/Scatter/Heat、Line のグラデーション化 |
| View > Graphics | `Modules/View/ViewGraphicsView.xaml` | Sketch+PNG / Pulse ring / Countdown の 3 カード追加 |
| UI > Flight | `FlightHudScene` | レーダーブリップのタップ選択 |
| UI > Telemetry | `UITelemetryView` + `TelemetryScene` | Function2=Buffer でダブルバッファ切替 + 左上 MODE 表示 |

- [ ] **Sample > Chart**: Stacked=棒が左から順に伸びる / Scatter=点が順に拡大出現 / Heat=行が上から順にフェードイン (青→黄→赤)。**Line が値の低い区間=青、高い区間=赤のグラデーション線**になっている
- [ ] **View > Graphics (Sketch)**: 指でなぞると色が自動で変わるストロークが描け、Undo/Clear が効く。「PNG出力」で描いた絵がそのまま下の画像に出る
- [ ] **View > Graphics (Pulse ring)**: 青い波紋が広がり続ける。画面を出て戻っても動く
- [ ] **View > Graphics (Countdown)**: GO でリングが 5 秒かけて減り、完走で DONE! が出る。減っている途中で画面を出て戻ると DONE は出ない (中断=通知なし)
- [ ] **UI > Flight**: レーダー上の敵/味方ブリップをタップすると白いリングで囲まれ、下に「TGT HOS BRG xxx RNG x.xNM」が出る。再タップで解除。レーダー外のタップは無反応
- [ ] **UI > Telemetry**: 左上に「MODE DIRECT」。**Function2 (Buffer) で「MODE BUFFER」に切り替わり、表示が同等に動き続ける** (これが 8-7 の試験。滞在中の計測ログは Claude が logcat から回収する)

---

### 1-8. フェーズ6(2026-09-02・SCP / SSH.NET。D8 本採用も同梱)

| 画面 | 現在のファイル名 | 変更 |
| --- | --- | --- |
| Network メニュー | `Modules/Network/NetworkMenuView.xaml` | 空きスロットに SCP を結線 |
| Network > SCP | `Modules/Network/NetworkScpView.xaml` | 空スタブ → 転送画面として実装 |
| Main > Setting | `Modules/Main/SettingView.xaml` | SCP セクション追加 (QR 投入) |
| Scene 4 画面 | `SceneObject` | ダブルバッファ既定 ON (D8 本採用) |

- [ ] **Network メニュー**: 「SCP」が追加され遷移できる (未設定時は接続先が「未設定 (設定画面の QR で投入)」でボタン無効)
- [ ] **Main > Setting**: 項目の**ラベルと現在値が横並び**で表示される (2026-09-02 変更)。SCP セクション (Host/User/Password) があり、QR (`ScpHost=...` 形式) を読むと反映される
- [ ] **Network > SCP (要 SSH サーバ)**: QR 投入後、「アップロード」でファイル選択→進捗バー→完了ログ。「ダウンロード」で同ファイルがキャッシュへ取得される。転送中「キャンセル」で中断。接続後にサーバのホスト鍵指紋が参考表示される (照合は行わない=2026-09-02 に指紋設定を撤去)
- [ ] **UI > Flight/Tactical/Telemetry/Energy**: ダブルバッファ既定 ON で従来どおり表示・滑らかに動く (Telemetry 左上は「MODE BUFFER」開始)

---

### 1-9. フェーズ9(2026-09-02・既存画面の強化。Release ビルド)

| 画面 | 現在のファイル名 | 変更 |
| --- | --- | --- |
| View > Lottie | `Modules/View/ViewLottieView.xaml` | スクロール連動シーク + 長押し進行 (9-1) |
| UI > Schedule | `UIScheduleView` + `DayTimetableView` | カード化 / 所要時間 / 空き時間帯 / 日合計 + `IScheduleEventProvider` 化 (9-2) |
| Basic > Locale | `Modules/Basic/BasicLocaleView.xaml` | resx 参照一覧 (neutral/ja/current) + カルチャ別書式カード (9-3) |
| View > Shadow | `Modules/View/ViewShadowView.xaml` | ニューモーフィズム節を追加・全体を ScrollView 化 (9-4) |
| Navigation > Edit List | `Modules/Navigation/Edit/EditListView.xaml` | Function3=Select で複数選択 + 全選択/一括削除 (9-5) |
| Sample > Crop | `Modules/Sample/SampleCropView.xaml` | 新設。トリミング編集 + PNG 書き出し (9-6) |
| Sample > Chat | `Modules/Sample/SampleChatView.xaml` | マイク FAB → 音声フロー 4 ステップ (9-7) |

- [ ] **View > Lottie**: グラデーションの帯を横スクロールすると再生位置が追随する (再生中なら一時停止に切り替わる)。「長押しで進行 (2 秒)」を押し続けるとアニメが進み、完走前に離すと素早く巻き戻る
- [ ] **UI > Schedule**: 日チップの下に「予定 n 件 / 合計 xh / 空き yh」のサマリーバー。イベントが**白いカード + 色付きアクセントバー + 影**になり、右上に所要時間 (1h30m 等)。イベントの無い時間帯が薄緑になり 45 分以上の隙間に「空き xh」表示。日を切り替えるとサマリーも変わる
- [ ] **Basic > Locale**: Localized resources に Names.Label / Messages.Hello_World の neutral・ja・current 3 段表示 (current は青太字)。Culture formats カードに現在カルチャ (current チップ付き) + en-US / de-DE / ja-JP の Number/Currency/Date/Time 書式差が並ぶ
- [ ] **View > Shadow**: 最下部に「Neumorphism」節。同色タイルの凸 (右下暗・左上明) と凹 (逆) が並ぶ。画面全体が縦スクロールできる
- [ ] **Navigation > Edit > List**: Function3 (Select) で選択モード。行タップで青ハイライト選択 (行の編集/削除ボタンは消える)、下部バーに件数 + 全選択 + 削除。削除で確認 → 一括削除。Cancel (Function3) で通常へ戻る
- [ ] **Sample > Crop**: 画像上の枠をドラッグで移動、四隅ハンドルでリサイズ (枠は画像内にクランプ)。「書き出し」で左下にトリミング結果のサムネイルとサイズ (px/bytes)。「リセット」で枠が初期位置へ
- [ ] **Sample > Chat**: 右下のマイク FAB でオーバーレイ表示。①録音 (タップで開始→ボタンが赤 Stop + 脈動 + 秒数カウント→停止) ②文字起こし (スピナー 1.5 秒→固定文) ③抽出プレビュー (種別/期日/対象/参照) ④承認「入力欄へ反映」でチャット入力欄に文字が入る。× や画面離脱でリセットされる

### 1-10. 横断確認(最後に)

- [ ] 各画面の Back/Function キーが従来どおり動作する
- [ ] 回転・再入場(画面を出て入り直す)で入場アニメが再生され、表示が壊れない
- [ ] ダーク寄り画面(Nfc/Stream 系)で文字が読める
- [ ] 未結線ボタン(Money/Mail/Social/Super/Stream/StreamDetail/Login/Shop Filter 等)は押しても無反応=**仕様どおり**(UIShop 検索のみ実機能)

---

### 1-11. メニュー再編+画像前準備(2026-09-03/09-05・Release ビルド)

| 画面 | 現在のファイル名 | 変更 |
| --- | --- | --- |
| メインメニュー | `Modules/Main/MenuView.xaml` | 番号プレフィックス廃止・並び替え(Setting 最後)。**2026-09-05: 9 段×2 列化**(Data\|Network / Sample\|App / UI 1\|UI 2 をペア行に・8 段目は無効ボタン行)+**全ボタンに Material アイコン追加** |
| UI 1 / UI 2 | `Modules/UI/UIMenu1View.xaml` / `UIMenu2View.xaml` | 旧 UIMenu を分離。UI 1=アプリ系 18 / UI 2=可視化・計器・HUD 系 13。グループ毎に行分け+空きセル。F4 相互遷移。**2026-09-05: 各 3 列×9 段化**(列数は UI 1/UI 2 で統一。UI 1 が 2 列だと収まらないため 2 列化は見送り) |
| (全画面) | `Resources/Images/` 階層化 + プレースホルダ 42 枚 | 参照はファイル名のままのため表示への影響なし(確認対象) |

- [ ] メインメニュー: 番号なし・全ボタンにアイコン付きで Basic / Navigation / Device / Data\|Network / View / Sample\|App / UI 1\|UI 2 / (無効ボタン行) / Setting の 9 段(Setting が最終行)
- [ ] UI 1: 9 段×3 列。グループ毎に行が分かれ(決済 EC / ツール / プロフィール / 通信 / 日付・予定+拡張用の空き行)、余りセルは無効ボタン。F4「UI 2」で UI 2 へ、F1(Back)でメインメニューへ
- [ ] UI 2: 9 段×3 列。グループ毎に行が分かれ(データ可視化 / 計器・ゲージ / HUD / 描画デモ+拡張用の空き行)、余りセルは無効ボタン。F4「UI 1」で UI 1 へ
- [ ] UI 1 / UI 2 配下の画面から Back で**元のメニュー側**へ戻る(例: Shop→UI 1、Flight→UI 2)
- [ ] 画像を使う画面(Profile / Shop / Item / Cart / Stream / Character / Chat / Social / Mail / Login / Super / Pet)の表示が従来どおり(Images 階層化でリソース名不変の確認)

---

### 1-12. DI コンテナ移行(2026-09-03・BunnyTail.DependencyInjection・Release ビルド)

Smart.Resolver → BunnyTail.DependencyInjection への移行の横断確認。Debug でのスモーク(起動 / メニュー / Wizard スコープ共有・破棄 / UI 1↔UI 2 / Flight / タイトル同期)は確認済み。

- [ ] よく使う画面を一巡して Back で戻る(画面と VM の生成が DI ソース生成ファクトリへ全面的に変わったため)
- [ ] Navigation > Wizard: Input1→2→Result で入力値が引き継がれ、完了/離脱後に再入場すると空に戻る(スコープ破棄)
- [ ] ダイアログ/ポップアップ系(Basic > Dialog / Navigation > Dialog の InputNumber)が開閉できる
- [ ] 画面遷移でヘッダタイトルと F キーが遷移先の内容へ即時更新される(旧値上書き不具合の修正確認。Lottie / Edit List など F キーをバインドしている画面の出入りで特に)
- [ ] Device 系 1〜2 画面(Info / Sensor 等)と Setting(QR)が従来どおり動く

---

## 2. SCP 転送実テスト(要 SSH サーバ)

- [ ] ユーザーの SSH サーバで実施: 設定画面の QR 投入 → アップロード / ダウンロード / キャンセル(確認観点は 1-8)

---

## 3. 他案件の残課題(旧 Fix_Checklist.md 第 1 部より)

### 3-1. 未対応(優先度低)

- [ ] `SecureStorage.GetAsync` の復元・キーストア無効化時の例外が未捕捉 (エッジケース)
- [ ] `HttpService` の `CancellationToken` を `NetworkOperator` のデリゲート型経由で呼び出し側から渡せるようにする (現在は口が無く未使用。転送は 10 分の有限タイムアウトで暫定対応済み)

### 3-2. 実機検証が必要な項目(実物・サーバが必要)

| # | 内容 | 関連 |
| --- | --- | --- |
| 1 | NFC: タグ複数回読取→画面離脱→再入場。途中で離しても継続 | NFC 例外処理 |
| 2 | Bluetooth: ペアリング相手ありでの印刷成功パス | 状態復帰 |
| 3 | Android 11 実機でのレガシー Bluetooth 権限 | Manifest |
| 4 | Network: 大容量 Download/Upload の完走・進捗、サーバ停止時のリトライ上限 | 転送・リトライ |
| 5 | サーバ時刻表示の TZ (`ToLocalTime()` 追加済みだが要 API サーバ) | DateTime |
| 6 | CV サンプル: キャプチャ→検出の繰り返しでメモリ増加なし | SKBitmap |
| 7 | 端末 TZ を変えて DB 保存→表示 | DateTime |

---

## 4. 画像アセット拡充(素材待ち)

サンプル画面のプレースホルダ画像(profile.jpg の三重使い回し、縦長 social_background.png の 15 箇所流用、usa キャラ絵の商品転用)を専用画像に差し替える(2026-08-01 策定の `Image_Asset_Expansion_Plan.md` は 2026-09-03 に本節へ統合・削除)。

### 4-0. 指針(命名・配置・フォーマット)

- 配置先: XAML の `<Image Source>` から使う → `Resources/Images/`(MauiImage・密度別を自動生成)。コードで `OpenAppPackageFileAsync` から読む → `Resources/Raw/<カテゴリ>/`(MauiAsset・`LogicalName` はカテゴリ相対パス。例 `Social/player.jpg`)
- 命名(MauiImage は厳格): **小文字のみ・先頭は英字・使用可は英数字と `_`**(`-`・大文字・空白・日本語は不可)。連番はゼロ埋め 2 桁
- フォーマット: 写真=`.jpg`(品質 80 前後)/ 透過・図版・ロゴ=`.png` / ベクタで済む UI アイコンは `.svg`
- サイズ: 「一番大きく表示される箇所の dp × 3」を 1 枚用意(過大な原画はビルド/実行を重くする)。**表示スロットのアスペクト比に合わせるのが最重要**(縦長→横長流用の切り抜け問題の再発防止)
- **フォルダ構成(2026-09-03 階層化済み)**: `Resources/Images/` = Banner / Character / Chat / Common / Login / Onboard / Pet / Profile / Shop / Stream の用途別 10 フォルダ(Raw と同じ PascalCase)(csproj の `MauiImage` は `Resources\Images\**`)。**参照はフォルダ名を含まないファイル名のみ**のため全体で重複名不可
- **現状維持と判断済み(差し替え不要)**: UICharacter の usa 系(キャラ用途に合致)/ UIChat のスタンプ / UISocial の通貨・資源アイコン / UIDock のデッキボタン / UIMail の genbaneko・usausa

### 4-1.【判断】候補ファイル名の確定

- [ ] ファイル名を確定する(現候補: `avatar_user` / `profile_cover` / `gallery` / `product_apparel` / `product_beauty` / `poster` / `stream_hero` / `stream_clip` / `onboard` / `pet` / `banner` / `avatar_person` / `login_hero`)
  - 変更例: `poster`→`movie`、`gallery`→`photo` など。確定後、本書 4-3/4-4 の名前を一括更新する。
  - 制約: 小文字・先頭英字・英数字と `_` のみ(MauiImage)。

### 4-2.【判断】画像の作成手段

- [ ] 作成手段を決定する:
  - (a) 素材支給を待つ(従来方針。作成はスコープ外)
  - (b) Claude がプレースホルダをプログラム生成(正しいアスペクト比・スロット別に区別できる内容。後日、本素材へ同名差し替え可能)
  - (a)(b) 併用(例: ★★★ は支給・★★ 以下は生成)も可。

### 4-3. 画像の用意(新規 42 枚+差し替え 2 枚)

**前準備済み(2026-09-03)**: 42 枚すべてに**既存画像コピーのプレースホルダを配置済み**(比率・内容は仮)。実素材は各カテゴリフォルダへ**同名上書き**で反映できる。下記チェックは実素材に差し替えたら付ける。
作成できたファイルにチェック。サイズは「最大表示 dp×3」基準・**アスペクト比がスロットと一致していることが最重要**。
写真=`.jpg`(品質 80)、透過=`.png`(規約は 4-0)。

#### ★★★ プロフィール(8 枚)
- [ ] `avatar_user.jpg` — 512×512 / 1:1(自分のアバター。人物ポートレート/顔アイコン)
- [ ] `profile_cover.jpg` — 1600×800 / 2:1(カバー。横長の風景・抽象・グラデ)
- [ ] `gallery01.jpg` — 1000×1000 / 1:1(投稿写真: 旅行/料理/風景/日常)
- [ ] `gallery02.jpg` — 1000×1000 / 1:1
- [ ] `gallery03.jpg` — 1000×1000 / 1:1
- [ ] `gallery04.jpg` — 1000×1000 / 1:1
- [ ] `gallery05.jpg` — 1000×1000 / 1:1
- [ ] `gallery06.jpg` — 1000×1000 / 1:1

#### ★★★ ショッピング(9 枚)
- [ ] `product_apparel01.jpg` — 900×1200 / 3:4(ドレス。スタジオ物撮り縦位置)
- [ ] `product_apparel02.jpg` — 900×1200 / 3:4(ジャケット)
- [ ] `product_apparel03.jpg` — 900×1200 / 3:4(帽子)
- [ ] `product_beauty01.jpg` — 800×800 / 1:1(美容液 Aqua Serum)
- [ ] `product_beauty02.jpg` — 800×800 / 1:1(口紅 Velvet Lip)
- [ ] `product_beauty03.jpg` — 800×800 / 1:1(クリーム Glow Cream)
- [ ] `product_beauty04.jpg` — 800×800 / 1:1(ミスト Pure Mist)
- [ ] `product_beauty05.jpg` — 800×800 / 1:1(マスク Silky Mask)
- [ ] `product_beauty06.jpg` — 800×800 / 1:1(チーク Petal Blush)

#### ★★★ 動画配信(10 枚)
- [ ] `poster01.jpg` — 600×900 / 2:3(作品ポスター。各作品で異なるビジュアル)
- [ ] `poster02.jpg` — 600×900 / 2:3
- [ ] `poster03.jpg` — 600×900 / 2:3
- [ ] `poster04.jpg` — 600×900 / 2:3
- [ ] `poster05.jpg` — 600×900 / 2:3
- [ ] `poster06.jpg` — 600×900 / 2:3
- [ ] `stream_hero.jpg` — 1600×900 / 16:9(ヒーロー/詳細トップのキービジュアル)
- [ ] `stream_clip01.jpg` — 1280×720 / 16:9(予告編サムネ)
- [ ] `stream_clip02.jpg` — 1280×720 / 16:9
- [ ] `stream_clip03.jpg` — 1280×720 / 16:9

#### ★★ オンボーディング(3 枚)
- [ ] `onboard01.jpg` — 1080×1080 / 1:1(Welcome)
- [ ] `onboard02.jpg` — 1080×1080 / 1:1(Stay Connected)
- [ ] `onboard03.jpg` — 1080×1080 / 1:1(Get Started)

※ AspectFit・高さ 240 表示のため、余白込みの正方形イラストが収まりやすい(透過が必要なら `.png`)

#### ★★ ペット(3 枚)
- [ ] `pet01.jpg` — 1000×1000 / 1:1(動物写真。画面で使うのはまず 1 枚)
- [ ] `pet02.jpg` — 1000×1000 / 1:1(バリエーション)
- [ ] `pet03.jpg` — 1000×1000 / 1:1(バリエーション)

#### ★★ プロモ/Super バナー(3 枚)
- [ ] `banner01.jpg` — 1200×600 / 2:1(サマーフェス。文字が乗る余白構図)
- [ ] `banner02.jpg` — 1200×600 / 2:1(新キャラクター)
- [ ] `banner03.jpg` — 1200×600 / 2:1(プレミアム会員)

#### ★ チャット(5 枚)
- [ ] `avatar_person01.jpg` — 256×256 / 1:1(Alice)
- [ ] `avatar_person02.jpg` — 256×256 / 1:1(Bob)
- [ ] `avatar_person03.jpg` — 256×256 / 1:1(Carol)
- [ ] `avatar_person04.jpg` — 256×256 / 1:1(Dave)
- [ ] `avatar_person05.jpg` — 256×256 / 1:1(自分)

#### ★ ログイン(1 枚)
- [ ] `login_hero.png` — 512×512 / 1:1(透過 PNG。アプリロゴ/ヒーロー)

#### ★ Raw 差し替え(2 枚)
- [ ] `Resources/Raw/Social/player.jpg` — 256×256 / 1:1(プレイヤー顔。同名上書き)
- [ ] `Resources/Raw/Avatar/mofusand.jpg` — 256×256 / 1:1(差出人アバター。同名上書き)

### 4-4. コード反映(グループ単位・★★★→★★→★ の順)

実素材を `Resources/Images/<カテゴリ>/` のプレースホルダへ同名上書きしたうえで、下表の「現在→新」を差し替える。
Raw 2 件は**同名上書きのためコード変更不要**。各グループ完了後に 4-5 の実機表示確認へ。

#### 4-4-A. ★★★ プロフィール
- [ ] 反映する

| 修正ファイル | スロット(何用) | 現在のファイル | 新ファイル |
| --- | --- | --- | --- |
| `UIProfileView.xaml` | カバー(パララックス) | `social_background.png` | `profile_cover.jpg` |
| `UIProfileView.xaml` | アバター | `profile.jpg` | `avatar_user.jpg` |
| `UIProfileViewModel.cs` | 写真ギャラリー 6 件 | `usa1〜6_full.jpg` | `gallery01〜06.jpg` |

#### 4-4-B. ★★★ ショッピング
- [ ] 反映する

| 修正ファイル | スロット(何用) | 現在のファイル | 新ファイル |
| --- | --- | --- | --- |
| `UIShopViewModel.cs` | 化粧品商品 6 件 | `usa1〜6_face.jpg` | `product_beauty01〜06.jpg` |
| `UIShopViewModel.cs` | アパレル商品 3 件 | `usa1〜3_full.jpg` | `product_apparel01〜03.jpg` |
| `UIShopView.xaml` | ショップ主アバター | `profile.jpg` | `avatar_user.jpg`(共用) |
| `UIItemView.xaml` | 商品メイン画像 | `usa1_face.jpg` | `product_beauty01.jpg`(共用) |
| `UICartViewModel.cs` | カート明細 3 件 | `usa1〜3_face.jpg` | `product_beauty01〜03.jpg`(共用) |

#### 4-4-C. ★★★ 動画配信
- [ ] 反映する

| 修正ファイル | スロット(何用) | 現在のファイル | 新ファイル |
| --- | --- | --- | --- |
| `UIStreamView.xaml` | ヒーロー | `social_background.png` | `stream_hero.jpg` |
| `UIStreamViewModel.cs` | 作品ポスター 5 件 | `social_background.png`×5 | `poster01〜05.jpg` |
| `UIStreamDetailView.xaml` | プレイヤー画像 | `social_background.png` | `stream_hero.jpg`(共用) |
| `UIStreamDetailViewModel.cs` | 予告編 3 件 | `social_background.png`×3 | `stream_clip01〜03.jpg` |
| `UIStreamDetailViewModel.cs` | 関連作品 4 件 | `social_background.png`×4 | `poster03〜06.jpg`(共用) |
| `UIStreamDetailView.xaml` | 一緒に視聴中アバター 3 件 | `usa1〜3_face.jpg` | `avatar_person01〜03.jpg` ※ |

※ ★グループの `avatar_person01〜03` に依存。動画配信を先行する場合は「その 3 枚だけ先行作成」or「このスロットは現状維持で後回し」をその時点で選ぶ。

#### 4-4-D. ★★ オンボーディング
- [ ] 反映する

| 修正ファイル | スロット(何用) | 現在のファイル | 新ファイル |
| --- | --- | --- | --- |
| `UIKitOnboardViewModel.cs` | ページ画像 ①〜③ | `social_background.png`×3 | `onboard01〜03.jpg` |

#### 4-4-E. ★★ ペット
- [ ] 反映する

| 修正ファイル | スロット(何用) | 現在のファイル | 新ファイル |
| --- | --- | --- | --- |
| `UIPetView.xaml` | ペット写真 | `usa1_full.jpg` | `pet01.jpg` |

#### 4-4-F. ★★ プロモ(Super)
- [ ] 反映する

| 修正ファイル | スロット(何用) | 現在のファイル | 新ファイル |
| --- | --- | --- | --- |
| `UISuperViewModel.cs` | バナー①サマーフェス | `social_background.png` | `banner01.jpg` |
| `UISuperViewModel.cs` | バナー②新キャラクター | `usa3_full.jpg` | `banner02.jpg` |
| `UISuperViewModel.cs` | バナー③プレミアム会員 | `profile.jpg` | `banner03.jpg` |

#### 4-4-G. ★ チャット
- [ ] 反映する

| 修正ファイル | スロット(何用) | 現在のファイル | 新ファイル |
| --- | --- | --- | --- |
| `UIChatViewModel.cs` | Alice/Bob/Carol/Dave/自分 | `usa1〜5_face.jpg`(定数 5 件) | `avatar_person01〜05.jpg` |

#### 4-4-H. ★ ログイン
- [ ] 反映する

| 修正ファイル | スロット(何用) | 現在のファイル | 新ファイル |
| --- | --- | --- | --- |
| `UILoginView.xaml` | タイトル画像 | `profile.jpg` | `login_hero.png` |

#### 4-4-I. ★ Raw 差し替え(コード変更なし)
- [ ] `Social/player.jpg` を上書き(UISocial 表示確認のみ)
- [ ] `Avatar/mofusand.jpg` を上書き(UIMail 表示確認のみ)

### 4-5. 実機表示確認(Pixel 9a・グループ完了ごと)

- [ ] ★★★ プロフィール(カバー比率/パララックス/ギャラリー 6 枚)
- [ ] ★★★ ショッピング(Shop 一覧・Item 詳細・Cart 明細でサムネ一貫)
- [ ] ★★★ 動画配信(ヒーロー 16:9 切れなし/ポスター縦長/クリップ横長)
- [ ] ★★ オンボーディング(3 ページが別画像)
- [ ] ★★ ペット/プロモ(バナー 3 種が別画像)
- [ ] ★ チャット/ログイン/Social/Mail
- [ ] ビルド 0 エラー・新規警告ゼロの維持(既知 CS8785×1 / XA4301×7 のみ)

---

## 5. バックログ(任意・後日。指示があれば着手)

- [x] 旧 `CalendarView` の整理(C-14+D19)— **2026-09-06 完了**: 旧 XAML 版 CalendarView(未参照 1,490 行)を削除し、**`CalendarView2` を `CalendarView` へリネーム**(git mv・クラス名/x:Class/参照 70 箇所)。`UICalendarView.xaml` の切替コメントも実態合わせ。ビルド警告ゼロ・実機で表示/月送り/イベント/選択モード確認済み
- [ ]【判断】`Controls/ChatView` バブル色のバインダブル化(C-13・D18): 検討扱い・未確定
- [ ] UISocial 背景の専用化(1080×1920 / 9:16 のゲーム風背景。「任意」扱い・44 枚には含まず)
- [ ] `AnimationOption.ResetEnter` の Scale 1 固定リセット(静的 Scale+EnterAnimation 併用が将来出た場合に、TranslationY と同じ基準値退避パターンで対処)

---

## 6. ReSharper 指摘の残対応(2026-09-05 inspectcode 全 254 件を分類)

再実行コマンド(**Bash 系シェルで実行**。PowerShell は `--properties:` が分割され「Specify only one solution file」になる):

```bash
jb inspectcode Template.MobileApp.slnx -f=xml -o=results.xml --no-build --no-swea --properties:Configuration=Release
```

対応済み: **A 群=機械修正 68 件** / **C-1=Style の Row/ColumnDefinitions を Grid 個別記述化(方針決定)99 件** / **C-2=バインド誤検知を ReSharper disable/restore コメントで抑止 25 件(14 箇所)**。以下が残り。

### 6-1.【判断】B 群=SUGGESTION スタイル系 26 件(直すのは機械的・直すかどうかが判断)

- [x] B-1 target-typed new 化(2026-09-05 全対応済み)
- [x] B-2 for→foreach 5 件(2026-09-05 対応済み)
- [x] B-3 引数 IEnumerable 化(2026-09-05 全対応済み)
- [x] B-4 null 伝播化(2026-09-05 全対応済み。AppGameViewModel は C# 14 の `?.` 代入)
- [x] B-5(2026-09-05 全対応済み。`field` キーワード化も採用・実施=`#pragma IDE0032` 抑止を撤去)

### 6-2.【判断】C 群の残り 19 件

- [x] C-3 using 初期化子分解 4 件(2026-09-05 対応済み=SKPaint/SKFont のプロパティ設定へ分解)
- [x] C-4 AssignNullToNotNullAttribute 2 件(2026-09-05 対応=ライブラリ注釈と実態の乖離に null 免罪符+理由コメント。※DrawingControl 側は IDE0370 との板挟みが判明 → 6-4)
- [x] C-5 ConditionIsAlwaysTrueOrFalse(2026-09-05 対応=`value >= 0` 保証により冗長な `(peak > 0)` を削除)
- [x] C-6 ParameterHidesMember(2026-09-05 対応=引数を `bluetoothAdapter` へリネーム・ユーザー選択)
- [x] C-7 AsyncVoidLambda 1 件(2026-09-05 対応済み)
- [x] C-8 `Xaml.PossibleNullReferenceException`(2026-09-05 決定=**FallbackValue='-' を適用**(ユーザー決定)。DeviceLocationView 7+ViewCustomView 1。Motion 4 項目は末端 null(Course/Speed 等)向けに **TargetNullValue='-' も併用**。**`HighlightTrigger` の 1 件のみ意図的に未適用で残す**=FallbackValue を付けると初回位置取得時にハイライトが発火する挙動変化が出るため)
  - **2026-09-06(fix3)で方式変更**: DeviceLocationView は FallbackValue/TargetNullValue を撤回し、**測位待ちの空状態パネル**(`Location` の null 判定で切替)へデザイン変更(`Change_Summary.md` 区間 9)

### 6-3.【判断】未使用代入 17 件=Debug 計測パターンのため A 群から除外

`var t0 = sw.Elapsed;` 等が `[Conditional("DEBUG")]` の `Debug.WriteLine` でのみ使用され、**Release 解析でのみ未使用扱い**になるもの(削除すると Debug ビルドが壊れる)。対象: `CalendarView.xaml.cs` 11 / `MonthViewBuilder.cs` 5 / `UICalendarViewModel.cs` 1。

- [x] **決定(2026-09-05・ユーザー決定): (a) 現状維持**(Debug 診断として残す。Release 解析の inspectcode では 17 件が誤検知として残り続けるのを許容)
- [x] **2026-09-06(fix3)で方針変更**: `MonthViewBuilder` / `UICalendarViewModel` は計測ごと**撤去**、`CalendarView.xaml.cs` は計測を残して `// ReSharper disable RedundantAssignment` で抑止 → 17 件は解消見込み

### 6-4. ツール間の板挟み 2 件(2026-09-05 の修正で表面化→同日解消)

- [x] IDE0370(`DrawingControl.cs`): `null!` を素の `null`+`// ReSharper disable once AssignNullToNotNullAttribute` へ変更(ユーザー決定)。コンパイラは null 代入を許容・R# のみ誤検知の食い違いを両ツール解消
- [x] SA1500(`UICalendarViewModel.cs`): `field` キーワードの初期化子構文の誤検知を `#pragma warning disable SA1500` で局所抑止(ユーザー決定)

**最終状態(2026-09-05)**: inspectcode 残 **18 件=全て確定済みの許容**(6-3 の Debug 計測 17=現状維持+`HighlightTrigger` の FallbackValue 未適用 1=挙動変化回避)。6 節の作業はこれで完了。
※2026-09-06(fix3)の変更(6-3 の撤去/抑止・C-8 の空状態化)で残数の構成が変わったため、次回 `jb inspectcode` 実行時に再集計する。

---

## 7.【優先】`tmpl-plan-maui.md` からの移管課題(2026-09-06 組み込み)

`D:\GitHubTemplate\tmpl-plan-maui.md`(MAUI トラック強化プラン)のうち**未対応の項目を優先事項として本書へ移管**した(2026-09-06 ユーザー指示)。対応済みの項目は記載しない。**keyboard / blazor 向けの対応(同書 §4 / §5)は対象外**。
画像アセット拡充(同書 3-8)は本書 **4 節**で管理中、iOS 対応(同書 3-13)は**保留継続**のため、この節には含めない。

### template-maui への反映(同書 §1 の残り)

- [ ] **7-1** `Document/Development.md` を template-maui へ持ち込む(同書 1-4)。作業用ドキュメント 13 ファイルの除外時に**必須の Development.md まで落ちている**。DB マイグレーション未実装の唯一の緩和策(「実案件適用時の注意」章)なので必ず入れる
- [ ] **7-2** template-maui README の TODO 実態同期(同書 1-9)。実装済みの **Chat / Chart / Gauge / Calendar / Media / Cognitive(SampleCvNetFace+Azure.AI.Vision)/ HybridWebView(WebViewBind・WebViewController)** が TODO に残っている。未実装の WiFi manager / Biometric / Bottom sheet / Push / Local notification は TODO のまま残す

### 機能実装(同書 §3 継続課題)

- [ ] **7-3** WiFi manager 実装(同書 3-1。`DeviceWiFiViewModel` は 6 行の空スタブ)
- [ ] **7-4** 生体認証の完成(同書 3-2。`DeviceBiometricViewModel` も 6 行の空スタブ。画面と ViewId は登録済み)
- [ ] **7-5** Bottom sheet(同書 3-3。実装なし)
- [ ] **7-6** 【判断】Push 通知(FCM)/ Local notification(同書 3-4)
- [ ] **7-7** DB マイグレーション機構(同書 3-5。`DataService.RebuildAsync`=毎起動で物理削除→再作成の user_version ベース置換。7-1 の Development.md 章で緩和する前提のため連動)
- [ ] **7-8** ダークモード(同書 3-6。`UserAppTheme=Light` 固定・`AppThemeBinding` 0 件。Colors.xaml は 4 テンプレートでバイト一致のため**対応するなら 4 本同時が効率的**)
- [ ] **7-9** ローカライズ拡充(同書 3-7。resx は Messages / Names とも 5 件のみ。機構は動作済み)
- [ ] **7-10** `Controls/SocialControls.cs` の TODO 10 件整理(同書 3-9。2026-09-06 時点で 10 件現存を確認)
- [ ] **7-11** 【判断】TimeProvider の MAUI 方式(同書 3-10。設定は EmbeddedBuildProperty のビルド時注入方式のため、wpf / avalonia の `AddOptions<T>().ValidateOnStart()` はそのまま移植不可)
- [ ] **7-12** 【判断】Analyzers.ruleset 正典差分 11 ルールの扱い(同書 3-12。CA1416 / CA2007 は MAUI 固有の合理性あり単純追随不可。CA1014 / CA1305 / CA1824 / CA1861 は再検討余地。正典統一トラック〈aidd 側セッション〉と連動)

### ソースレビュー由来(同書 付録のうち、本リポジトリで未対応と実測確認した分)

- [ ] **7-13** `Converters/MailDateTimeStringConverter.cs` の `ConvertBack` 是正(現状 `NotSupportedException` を throw。`Binding.DoNothing` 返却か OneWay 専用の明示へ)

> 注: 同書 付録の「`ApplicationInitializer` の async void → 起動ゲート化」は **StartupState 方式(0 節 B-1)で対応済み**、「`App.xaml.cs` の権限要求遅延」は**区間 2 のコードレビュー対応で画面側へ移動済み**のため記載しない。BACK/白画面修正の template-maui 反映(同書 §2-1)は **0-6 で決着済み**(A-1 反映済み・新 B-1 は反映不要=ユーザー判断)。機能追加アイデア(オフライン同期・ディープリンク・アクセシビリティ設定・起動状態画面 等)は必要になったら同書 付録を参照。
