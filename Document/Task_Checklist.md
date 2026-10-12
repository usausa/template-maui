# ✅残作業チェックリスト

残作業(実機確認 / 実テスト / 保留)のマスターチェックリスト。経緯・実装内容・ナレッジ・開発ポリシーは `Change_Summary.md`(付録含む)を参照。
優先順 = 2 節(バックグラウンドタスク / タンキング送信)→ 3 節(プッシュ通知)→ 4 節(生体認証)。6 節(保留中の判断)はユーザーの決定待ち。12 節(Health Connect)と 13 節(取り込み候補)は候補の記録で、要否はユーザーの決定待ち。14 節(別のアプリでの導入候補)は本テンプレートでは対象外の記録。小さな項目は「取り込み候補」の章(13 節)にまとめ、大きな項目は章を分けている。

## 📋サマリ

| Category | Feature | 章 |
| --- | --- | --- |
| Device | Background task(WorkManager) | 2-1 |
| Network | Tanking send(端末で溜めて、通信できるときにまとめて送る) | 2-2 |
| Device | Push(FCM) | 3-2 |
| Device | Biometric(生体認証) | 4-1〜4-3、4-5 |
| Device | Passkey(パスキー) | 4-4 |
| Decision | 保留中の判断(CoreCLR の扱い) | 6 |
| Device | Health Connect(歩数・心拍・睡眠などの健康データ。候補) | 12 |
| Sample | AI tool calling(Sample > Chat のツール呼び出し。候補) | 13-1 |

## 📏運用ルール

- 作業はこの番号で指示・進行する(例:「2-1 を実施」)。完了した項目は本書から削除し、内容は `Change_Summary.md` に記録する
- **⚖️【判断】印の項目はユーザーが決定**(勝手に進めない)。デザイン判断を伴う差分は 1 項目ずつ指示を受けて実施
- 実装・変更を行なう場合の完了条件 = **ビルド警告ゼロ** + `Change_Summary.md` への記録(開発ポリシーは同 付録A)
- コミットはユーザーが実施(グループ単位を推奨)
- `README.md` の TODO 表は本書のサマリ表(2〜6・12・13 節)と同期させる(項目の追加・削除・完了時に両方を更新。TODO 表に本書の番号は書かない)
- リンク集 `■MAUI.txd` は全件に判定を付記済み(🟩 取り込む / 🟦 取り込まないが記事として有用 / 🟥 古い・参照不要 / 🟨 要判断)。🟩 の項目は本書へ移し、元行は同書から削除する
- 描画・性能の計測は **Release ビルド + 実機**(手順は `Development.md` の「Releaseビルドでの検証と計測」)

## 🧭前提(環境)

- **環境制約 (不具合ではない)**: ①地図タイルは Google Maps API キー未設定だと非表示 (ピン・カメラ移動は動作) ②Sample > CV Net は AI エンドポイント未設定だと画面に入れない ③CommunityToolkit CameraView の `CaptureAsync` がまれに未完了になる(5 秒で打ち切って「撮影できませんでした」を出し、プレビューのまま続行できる)
- 現在実機に入っているのは **Debug ビルド**(2026-09-23 デプロイ。Mono ランタイム。性能・描画の確認時は Release へ入れ替える)

---

## ⏰2. バックグラウンドタスク(WorkManager / タンキング送信)

遅延可で再起動後も残る処理を `WorkManager` に載せる(2-1)。端末で作ったデータを溜めておき、通信できるときにまとめてサーバーへ送る(2-2)。ファイルパスは `Template.MobileApp/` からの相対。

### ⏰2-1 バックグラウンド定期タスク(WorkManager)

参照: https://www.nuget.org/packages/Shaunebu.MAUI.BackgroundTaskManager — Android は `WorkManager`(最短 15 分間隔)、iOS は `BGTaskScheduler` を使い、CRON 式でフォアグラウンド / バックグラウンドのジョブを登録して `Preferences` に永続化する薄いラッパー(`RegisterJob<T>()` / `Schedule(jobId, cron, callback)` / `ScheduleInBackground(jobId, cron, jobType)`。2025-10、230 DL、リポジトリ公開なし)。採用はせず API の形(ジョブ登録 / CRON 近似 / 永続化)だけ参考にし、`AndroidX.Work` を直接使う。

範囲: 制約付き(ネットワーク接続時 / 充電中)の一回限りワーク + 15 分周期の定期ワーク。実行結果はローカル通知(`Components/NotificationService`)か画面のログに出す。正確な時刻指定は `AlarmManager`(通知で実装済み)の担当のままとし、WorkManager は遅延可・再起動後も残る処理に限定する。

| 現在のファイル名 | 何用か | 変更 |
| --- | --- | --- |
| `Template.MobileApp.csproj` | パッケージ | `Xamarin.AndroidX.Work.Runtime` を追加(`dotnet list package --include-transitive` で `Fragment.Ktx` ピン止めとの競合を確認) |
| `Components/BackgroundTask.cs` + `.android.cs` | — | 新規。登録(一回 / 定期)/ 取消 / 状態取得。`AndroidX.Work.Worker` 派生の `DoWork()` で処理 |
| `Modules/Device/DeviceMiscView.xaml` + `DeviceMiscViewModel.cs` | 雑多なデバイス機能(Device メニューは満杯) | `InfoCard` を追加(登録 / 取消 / 最終実行時刻) |
| `MauiProgram.cs` | `ConfigureComponents` | DI 登録 |

- [ ] **2-1-0**⚖️【判断】要否 — 再起動後も残る遅延処理(同期 / 送信キュー)の需要があるか。WorkManager は再起動後に自動で再スケジュールされるため `RECEIVE_BOOT_COMPLETED` は不要。常駐(前景サービス)は対象外

### 📦2-2 タンキング送信

端末で作ったデータを端末の DB に溜めておき、通信できるときにまとめてサーバーへ送る(圏外でも記録を続けられる)。対向(template-maui-server)の受け口も要る。差分同期・競合解決は扱わない。

- [ ] **2-2-0**⚖️【判断】範囲 — 送るデータ(サンプルの記録の形)、送るきっかけ(通信の回復 / 手動 / 2-1 の WorkManager のネットワーク条件付きワーク)、再送で重複させない仕組み(端末が Id を決め、サーバーは同じ Id を受け流す)、送った分の扱い(削除 / 送信済みとして残す)、未送信の件数の見せ方

## 🔔3. プッシュ通知

ローカル通知は `Components/NotificationService.cs` + `.android.cs` で実装済み(即時 / スケジュール / アクションボタン / タップ時ペイロード。Device > Misc の Notification カード)。サーバーからの通知は、FCM を使わない独自の仕組み(SignalR。アプリを開いている間に受け取る)を実装済みで、残りは FCM(3-2)。

### ☁️3-2 FCM

参照: `0_maui-samples/10.0/WebServices/PushNotificationsDemo`(`Xamarin.Firebase.Messaging` + 自前 `FirebaseMessagingService`)。

| 種別 | URL | 概要 | 適用先 |
| --- | --- | --- | --- |
| 記事 | https://www.andreasnesheim.no/push-notifications-in-net-maui-with-firebase/ | FCM を `Plugin.Firebase` で扱う手順。Firebase Console 登録 → `google-services.json` 配置 → `MauiProgram` 初期化 → `CrossFirebaseCloudMessaging.Current.GetTokenAsync()` でトークン取得 → Console からテスト送信(2022-09) | 採用時の方式候補①(`Plugin.Firebase`)。方式候補②は上記 PushNotificationsDemo |
| ライブラリ | https://github.com/Gekidoku/BetterFireBaseNotificationsPlugin | `CrossFirebasePlugin` の MAUI 移植。データ付き通知 / アクションボタン / サイレント通知 / アプリ終了時の受信に対応。NuGet 配布はなくプロジェクト参照前提(2026-01) | 実装範囲(データ / アクション / サイレント)のチェックリスト |

- [ ] **3-2-0**⚖️【判断】プッシュ通知(FCM)の要否 — Firebase プロジェクトと `google-services.json` が前提。採用する場合、トークン表示と受信ログを Notification カードに追記する

## ⏳外部待ち(README の Pending と同期)

| 項目 | 待ち先 | 状態 |
| --- | --- | --- |
| XAML の global xmlns(`http://schemas.microsoft.com/dotnet/maui/global`) | ReSharper の対応(ビルドは通るが inspectcode が解決できない) | 保留。名前の衝突と範囲は `Change_Summary.md` 区間 16 |
| CoreCLR ランタイム(`UseMonoRuntime=false`) | .NET 11(Shiny の `[Export]` で起動クラッシュ: dotnet/android#10996) | csproj は CoreCLR のまま、実機検証は `-p:UseMonoRuntime=true` の Mono ビルド |

## 🔐4. 生体認証

Device > Biometric に、本人確認、生体認証で守る秘密(Android Keystore の鍵で暗号化して保存し、生体認証で復号して取り出す)、鍵での署名(サーバーでの認証の流れを端末の中で疑似的に確かめる)を作る。Android 本体の `BiometricPrompt` / `BiometricManager` を直接使う(`Components/Biometric.cs` + `.android.cs`)。パスキーは計画だけ(4-4)。

- [ ] **4-1** 本人確認(使えるかどうかの 3 区分の表示・認証・結果の表示)— 実装済み。残りは画面ロックと指紋を登録した端末での確認(成功・キャンセル・失敗回数の超過・PIN。登録と認証は利用者の操作)
- [ ] **4-2** 生体認証で守る秘密(鍵の作成・暗号化して保存・生体認証で復号して表示・削除・鍵が使えなくなったときの作り直し)— 画面ロックの無い端末では鍵を作れないため、画面ロックと指紋を登録した端末が用意できるまで保留
- [ ] **4-3** 鍵での署名(サーバー認証の疑似。署名の鍵の作成と公開鍵の登録・チャレンジへの生体認証付きの署名・端末の中での検証・改ざんと使い回しの検出・鍵の削除と作り直し)— 実装済み。残りは画面ロックと指紋を登録した端末での確認(登録と署名の成功・キャンセル・生体情報の登録を足したときの無効化。登録と認証は利用者の操作)
- [ ] **4-5** 本人確認と鍵の追加(ダイアログのサブタイトルと説明・弱い生体認証・結果の方式・端末に合う文言・最後の本人確認からの時間・登録の画面・署名の鍵の StrongBox と置き場所の表示)— 実装済み。残りは実機での画面の確認と、画面ロックと指紋を登録した端末での確認

### 🔒4-2 の作り方

| 項目 | 内容 |
| --- | --- |
| 鍵 | Android Keystore の AES-256 鍵(GCM)。使うたびに強い生体認証(クラス 3)を求め(`setUserAuthenticationParameters(0, AUTH_BIOMETRIC_STRONG)`)、生体情報の登録が変わると使えなくなる(`setInvalidatedByBiometricEnrollment(true)`)。署名の鍵(4-3)とは別名にする |
| 保存 | 鍵が無ければ作る → `Cipher`(`AES/GCM/NoPadding`)を暗号化で初期化 → `CryptoObject` を付けて生体認証 → 成功したら暗号化し、暗号文と IV を Base64 で `Settings` に保存する(平文は保存しない) |
| 取り出し | 保存した IV で復号の `Cipher` を初期化 → `CryptoObject` を付けて生体認証 → 成功したら復号して表示する(画面を離れたら消す) |
| 鍵が使えないとき | `Cipher` の初期化で `KeyPermanentlyInvalidatedException` → 鍵と保存した値を消し、作り直しを促す |
| 画面 | 文字列の入力と「保存」「取り出し」「削除」(鍵と保存した値) |
| 確認 | 保存して取り出すと同じ文字列、キャンセルでは保存も表示もしない、削除、生体情報の登録を足すと鍵が使えなくなり作り直しを促す(エミュレーターで確かめる) |

- 鍵と結び付けられるのは強い生体認証(クラス 3)だけ。顔認証がクラス 3 でない機種では、秘密の保存と取り出しに顔認証は使えない
- 画面ロックが無い端末では鍵を作れない。成功・キャンセル・失敗回数の超過の確認には、画面ロックと生体情報を登録した端末が要る(登録と認証は利用者の操作)
- 4-3 の検証は端末の中なので、なりすましの防止にはならない(仕組みを確かめる見本)。実際の認証には、サーバーでの公開鍵の登録と検証と、Key Attestation の証明書チェーンの確認が要る
- 参照: https://developer.android.com/privacy-and-security/keystore?hl=ja(鍵の作成と、使うたびの本人確認を求める鍵)

### 🔑4-4 パスキー

Credential Manager(`androidx.credentials`)でパスキーを作ってログインする。4-3 と同じく公開鍵での署名と検証だが、鍵の作成・保管・同期は Google パスワード マネージャーが行い、やり取りは WebAuthn の形にそろう。参照: https://developer.android.com/identity/credential-manager?hl=ja

| 前提 | 内容 |
| --- | --- |
| サーバー | WebAuthn の登録(オプションの発行と、証明の検証と公開鍵の保存)と認証(チャレンジの発行と署名の検証)の API(template-maui-server。.NET の WebAuthn のライブラリを使う) |
| アプリとドメインの関連付け | サーバーのドメイン(Relying Party の ID)の `/.well-known/assetlinks.json` に、アプリのパッケージ名と署名の証明書の指紋を載せる |
| 端末 | Google アカウントと画面ロック |
| パッケージ | `Xamarin.AndroidX.Credentials` |

- [ ] **4-4-0**⚖️【判断】パスキー(Credential Manager + WebAuthn)の要否 — 当面対応しない。サーバーの WebAuthn の API(template-maui-server)、アプリとドメインの関連付け(`assetlinks.json`)、`Xamarin.AndroidX.Credentials` のダウンロードが前提

## ⏸️6. 保留中の判断

### ⚙️6-2 CoreCLR ランタイムの扱い

csproj は `UseMonoRuntime=false`(CoreCLR)。Shiny の `[Export]` ライフサイクルコールバックで起動時にクラッシュする(dotnet/android#10996、.NET 11 で修正)。「外部待ち」の表と README の Pending に記載。

- [ ] **6-2-0**⚖️【判断】.NET 11 まで CoreCLR のままにする(実機検証は `-p:UseMonoRuntime=true` の Mono ビルド)か、csproj を Mono に戻すか

## 🩺12. Health Connect

ほかのアプリ(歩数計・フィットネス・体重計など)が記録した歩数・心拍・睡眠・体重などを、Android の Health Connect で読み書きする。今の Device > Activity は歩数センサー(`Components/ActivityRecognizer`、起動からの累計)だけで、履歴とほかのアプリの記録は扱わない。候補の記録で、実施は 12-0 の決定の後。ファイルパスは `Template.MobileApp/` からの相対。

参照: https://allanritchie.com/blog/2026/06/shiny-health-v2/ — `Shiny.Health`(https://github.com/shinyorg/health、MIT)。iOS の HealthKit と Android の Health Connect を `IHealthService` 1 つで扱う。データは 30 種以上(歩数・距離・消費カロリー・階数・心拍の平均 / 安静時 / 変動・体重・身長・体脂肪・血圧・SpO₂・血糖・体温・呼吸数・VO₂ max・睡眠・水分・運動 21 種・栄養など)。API は `RequestPermissions(DataType…)`、期間ごとの集計(`GetStepCounts(start, end, Interval)` など)、`GetWorkouts(start, end)`、`Write(HealthResult)`、`Observe(DataType, token)`(`IAsyncEnumerable`。Android は変更トークンのポーリング)。登録は `builder.Services.AddHealthIntegration()`。

| 項目 | 内容 |
| --- | --- |
| パッケージ | `Shiny.Health` 2.0.1(2026-08、累計 4.9K ダウンロード)。依存は `Shiny.Core` 5.0.0-beta-0132 以上(導入済みの Shiny 5.7.2 で満たす)、`Xamarin.AndroidX.Health.Connect.ConnectClient` 1.1.0.2 以上、`Xamarin.AndroidX.Lifecycle.LiveData.Core` 2.10.0.2 以上。`dotnet list package --include-transitive` で `Fragment.Ktx` のピン止めとの競合を確かめる |
| AndroidX を直接使う場合 | Health Connect のクライアントの API は Kotlin の suspend 関数で、C# から呼ぶには継続(`Continuation`)の橋渡しが要る |
| 端末 | Android 14 以降は OS に含まれる(Pixel 9a はこれ)。Android 11〜13(minSdk 30)は Health Connect のアプリが別に要るので、使える状態(未導入・更新が要る)を確かめてから使う |
| マニフェスト | 読み書きするデータの種類ごとの `android.permission.health.READ_*` / `WRITE_*`。権限の要求には、権限の説明を出す画面の宣言が要る(Android 13 以前 = `androidx.health.ACTION_SHOW_PERMISSIONS_RATIONALE` の intent-filter、Android 14 以降 = `android.intent.action.VIEW_PERMISSION_USAGE` + `android.intent.category.HEALTH_PERMISSIONS` の activity-alias。対象は `template.mobileapp.MainActivity`)。Android 11〜13 向けの `<queries>`(`com.google.android.apps.healthdata`) |
| 確かめ方 | 端末に記録が無くても、アプリから書き込んで(歩数・体重など)読み戻せば確かめられる |
| AI のツール | `Shiny.Health.Extensions.AI`(`AddHealthAITools`)は健康データを `Microsoft.Extensions.AI` のツールにする。Sample > Chat のツール呼び出しの題材になる |

| 現在のファイル名 | 何用か | 変更 |
| --- | --- | --- |
| `Template.MobileApp.csproj` | パッケージ | `Shiny.Health` を追加 |
| `Platforms/Android/AndroidManifest.xml` | 権限・開く先のアプリの宣言 | health の権限(使う画面のコメント付き)、`<queries>`、権限の説明の activity-alias |
| `Platforms/Android/MainActivity.cs` | 起動の Activity | 権限の説明の intent-filter と、説明の画面への遷移 |
| `Modules/Device/DeviceHealthView.xaml` + `DeviceHealthViewModel.cs`(新規)か `DeviceActivityView.xaml` + `DeviceActivityViewModel.cs` | 健康データ / 歩数と行動の認識 | 使えるかどうか、権限の要求、今日と 7 日の歩数、心拍、睡眠、書き込みと読み戻し |
| `Modules/Device/DeviceMenuView.xaml` / `Modules/ViewId.cs` | メニュー / 画面 ID | 新しい画面にする場合(Device メニューは満杯) |
| `MauiProgram.cs` | DI 登録 | `AddHealthIntegration()` |

- [ ] **12-0**⚖️【判断】要否と範囲 — 新しいパッケージ(`Shiny.Health` と AndroidX の 2 つ)と、マニフェストの権限・説明の画面が前提。採用する場合、新しい画面か Device > Activity への追記か、扱うデータの種類(歩数・心拍・睡眠・体重など)を決める

## 🧩13. 取り込み候補

小さな候補の記録。実施は各項目の判断の後。ファイルパスは `Template.MobileApp/` からの相対。

### 🧠13-1 Sample > Chat のツール呼び出し

今の Sample > Chat は `OllamaApiClient`(`IChatClient`)のストリーミングの応答だけで、ツール呼び出しは無い。モデルが端末やアプリの情報を取るツールを呼び、その結果で応答を続ける形にする。

| 項目 | 内容 |
| --- | --- |
| 仕組み | `Microsoft.Extensions.AI` の `ChatClientBuilder(client).UseFunctionInvocation()` で包み、`ChatOptions.Tools` に `AIFunctionFactory.Create(...)` のツールを渡す。モデルがツールを呼ぶと、結果を返して応答を続ける。`UseFunctionInvocation` は `Microsoft.Extensions.AI` にある(OllamaSharp が入れる `Microsoft.Extensions.AI.Abstractions` には無い) |
| Ollama | `OllamaApiClient` はツールに対応する。モデルもツールに対応したものが要る(モデルは設定の `OllamaModel`) |
| 題材 | 端末の情報(電池・通信・位置・機種)、アプリのデータ(ToDo の一覧と追加)、健康データ(`Shiny.Health.Extensions.AI`。12 節) |
| トリミング | `AIFunctionFactory.Create` はリフレクションでパラメーターのスキーマを作るので、Release(トリミング)で確かめる。`Microsoft.Maui.AI.Attributes`(maui-labs。`[ExportAIFunction]` のソース生成、承認のゲート、AOT 向け)で置き換えられる |
| ツールの分け方 | 指標ごとに 1 ツールにしない(数が多いとモデルが選び間違える)。種類を引数にした少数のツールにする。権限が要るものは、ツールを使う前に取っておく |

| 現在のファイル名 | 何用か | 変更 |
| --- | --- | --- |
| `Template.MobileApp.csproj` | パッケージ | `Microsoft.Extensions.AI` を追加 |
| `Modules/Sample/SampleChatViewModel.cs` | AI とのチャット | クライアントを `UseFunctionInvocation()` で包み、ツールを渡す |

- [ ] **13-1-0**⚖️【判断】要否と範囲 — ツールに対応したモデル(Ollama)が前提。採用する場合、ツールの題材(端末の情報・アプリのデータ・健康データ)と、Release での確かめ方を決める

## 💡14. 別のアプリでの導入候補

本テンプレートでは対象外。別のアプリで導入するときの情報の記録で、サマリ表と README の TODO には載せない。ファイルパスは `Template.MobileApp/` からの相対。

### 🔗14-1 ディープリンク(App Links / カスタムスキーム)

URL からアプリを起動し、指定の画面へ移る。参照: https://github.com/redth/maui.applinks.sample — `MainActivity` に `[IntentFilter(new[] { Intent.ActionView }, AutoVerify = true, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = "https", DataHost = "...")]` を付けると、URL は `Application.OnAppLinkRequestReceived(Uri)` に渡る。`https` のリンクの検証には、ドメインの所有確認と `/.well-known/assetlinks.json`(パッケージ名 + 署名の SHA-256)の配置が要る。iOS は Universal Links(`apple-app-site-association`)。

範囲: カスタムスキーム(例 `template://device/nfc`)でアプリを起動し、初期遷移(`StartupState`)の後に該当の画面へ移る(通知のタップを起動後に扱う `TakePendingTap` と同じ流れ)。`https` の検証リンクは、配布のドメインが決まってから足す。

| 現在のファイル名 | 何用か | 変更 |
| --- | --- | --- |
| `Platforms/Android/MainActivity.cs` | 起動の Intent の受け口(`LaunchMode = SingleInstance`) | `[IntentFilter]`(`ActionView` + `CategoryDefault` / `CategoryBrowsable` + `DataScheme = "template"`)を足す |
| `App.xaml.cs` | アプリ | `OnAppLinkRequestReceived(Uri)` で URI を保持して知らせる |
| `MainPageViewModel.cs` | 初期遷移 | 初期遷移の後に、保持した URI の画面へ移る |
| `Modules/ViewId.cs` | 画面 ID | URI のパスから `ViewId` への対応表 |
| `Document/Development.md` | 手順 | `adb shell am start -a android.intent.action.VIEW -d "template://device/nfc"` の確認手順 |

確認: 起動していないときと起動中(`OnNewIntent`)の両方で、該当の画面へ移る。

### 📈14-2 クラッシュ・テレメトリのほかの方式

本テンプレートは OpenTelemetry(OTLP/HTTP で template-maui-server へ送る。内容は `Change_Summary.md`)。ほかのアプリで、端末の管理や外部のサービスが要るときの候補。

| 候補 | 内容 | 参照 |
| --- | --- | --- |
| DeviceManager 型のサーバー + 端末のライブラリ | 端末の登録・一覧・ダッシュボード、ログ・エラーレポート、設定の配信・メッセージが一体。テレメトリは gRPC(h2c の別ポート)で、タンキング(SQLite)と再送あり。独自のプロトコルなので、ほかの収集基盤とはつながらない | `D:\GitHubTemplate\DeviceManager`(`README.md` / `docs/features.md` / `docs/sdk.md`) |
| Sentry(`Sentry.Maui`) | クラッシュ・エラー・パンくず・性能。オフラインのキャッシュ、リリースと端末の情報の自動付与 | https://docs.sentry.io/platforms/dotnet/guides/maui/ |
| Firebase Crashlytics | Android / iOS のクラッシュの収集(FCM を使うなら同じ `google-services.json`) | https://github.com/mattleibow/CloudyCrashReporting(各サービスの比較) |
| Application Insights(TinyInsights) | クラッシュ・ページビュー・依存の呼び出し・カスタムイベント。ダッシュボードあり | https://github.com/dhindrik/TinyInsights.Maui |
| OpenTelemetry → Azure Monitor | OTel の経路で Application Insights へ送る | https://www.docswell.com/s/tanaka_733/K4P265-maui-observability |
