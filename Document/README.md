# Template project for MAUI

MAUI用テンプレートプロジェクトの使用方法について記述する。

# 🎉始め方

## ビルド

テンプレートプロジェクトを取得しVisual Studioでビルドを行なうと機能サンプルの入ったテンプレートを作成できる。  
テンプレートプロジェクトはそのままAndroidで実行可能なので、動作確認を行ないながらソースを参照して構造を理解する。  

## ビルドバリアント

単一ソースでHW固有機能を使用する複数モデルのビルドに対応する場合、設定ファイルで対象とするHW用の定義を切り替えてビルドを行なう。  
プロジェクト中のDeviceProfile.sample.propsファイルを.DeviceProfile.propsとしてコピーすると、その内容がビルドに反映されるような設定となっている。  

なお、.DeviceProfile.propsは各ユーザーが開発環境でのみ使用するものとし、CI環境でリリース物をビルドする場合にはビルドオプションで同様の指定を行なう形とする。  

.DeviceProfile.props中では以下の項目を設定する。

* DefineConstants

DefineConstantsを設定するとプリプロセッサで使用される条件付きコンパイルの定義を設定できる。  
物理キーボードの有無や、使用するバーコードライブラリの違い等はこの設定で条件付きコンパイルにより切り替える形とする。  

* EmbeddedBuildProperty

商用版とテスト版といったアプリケーションのフレーバーや、接続先に関するシークレット情報等はEmbeddedBuildPropertyで指定すると、指定した内容がアプリケーション中から参照可能となる。  
プロジェクト中のVariants.csのように、BuildProperty属性を指定したメソッドでビルド時にEmbeddedBuildPropertyによって指定した値が参照可能となる。  

# 🌱新規プロジェクト作成

テンプレートプロジェクトを元に新規プロジェクトを作成する手順について記述する。  

## 名称変更

ソース中の「Template.MobileApp」の文言をアプリケーション固有のものに変更する。  
推奨する名称は「(システム名).MobileApp」、「(顧客企業名).(システム名).MobileApp」等。  

また、csproj中にあるApplicationTitle、ApplicationIdも併せて名称の変更を行なう。  

## 不要ライブラリ参照削除

テンプレートプロジェクトでは各種機能のサンプルを用意しているが、使用しない項目がある場合はライブラリへの参照を削除する。  
詳細は使用ライブラリの項目を参照。  

## 不要ソース削除

テンプレートプロジェクトでは各種機能のサンプルを用意しているが、不要な機能のサンプルや画面については削除してプロジェクトのベースとする。  
なお、Behaviors等についてはサンプルの実装を流用しても問題はない。  
以下に削除可能なソースについて記述する。  

### 削除可能ソース一覧

|フォルダ|概要|補足|
|:----|:----|:----|
|Behaviors|不要サンプル削除|全削除でも問題はない|
|Components|不要サンプル削除|後述|
|Controls|不要サンプル削除|全削除でも問題はない|
|Converters|不要サンプル削除|全削除でも問題はない|
|Domain|不要サンプル削除|全削除でも問題はない|
|Extender|不要拡張削除|後述|
|Helper|不要サンプル削除|後述|
|Input|物理キー制御|後述|
|Messaging|不要サンプル削除|全削除でも問題はない|
|Models|不要サンプル削除|全削除でも問題はない|
|Modules|不要サンプル削除|後述|
|Services|不要サンプル削除|後述|
|Ussecase|不要サンプル削除|後述|

### Components

StorageManagerは汎用的に使用するが、それ以外不要なものは削除しても問題ない。  

### Extender

画面遷移ライブラリのフォーカス制御拡張だが、キーパッドが無い機種の開発では削除しても問題ない。  

### Helper

Data、JsonはそれぞれDB、Web API用のヘルパーなので、それらの機能を未使用時は削除。  

### Input

キーパッドが無い機種向けの開発では削除。  

### Modules

各画面のサンプルなので、初期画面であるMain下のMenuView以外のサブフォルダは削除して問題ない。  
また、ダイアログを使用しない場合はAppDialogViewModelBase.cs、DialogId.cs、DialogSize.cs、PopupNavigatorExtensions.csは削除可能。  

### Services/Ussecase

未使用な機能のものについては削除で良いが、使用するものについてはサンプルのメソッド削除して使用する。
また、単純なアプリケーションの場合、Ussecaseは使用せずにServicesのみを使用する形でも問題ない。

## その他リソース

Resourceフォルダ中の項目から不要なリソースの削除及び変更を行なう。  
対象項目は以下。  

|フォルダ|概要|変更内容|
|:----|:----|:----|
|AppIcon|アプリケーション用アイコン|アプリケーション用に変更|
|Fonts|カスタムフォント|不要フォント削除|
|Images|画像|不要画像削除|
|Raw|その他リソースファイル|不要リソース削除|
|Splash|スプラッシュ用画像|アプリケーション用に変更|

# 📕アプリケーション構造

テンプレートプロジェクトのアプリケーション構造について記述する。  
この構成をアプリケーションを構築する際のスタンダードとする  

以下、プロジェクトの各フォルダとその中の主要なクラスについて内容を記述する。

## プロジェクト直下

(📝作成中)





- アプリケーションの初期化処理の順序について





初期化処理と、画面の枠部分(シェル)やアプリケーションライフサイクルで管理するデータに関する部分について。

- MainActivity.cs(Androidプロジェクト側)

Androidとしての初期化処理を行う。
実装がAndroid固有となるコンポーネントについてはIComponentProviderを通じて.NET Standard側に渡してDIコンテナに登録。

問題がなければAlwaysRetainTaskState、LaunchMode、ConfigurationChanges、ScreenOrientation等は制限する形で設定。

Android側のライブラリで、OnPause/OnResumeのタイミングで制御が必要なものはここで制御。

なお、AndroidではActivityに対するコールバック処理の実装が面倒なケースがあるが、Xamarinの場合CancellationTokenSourceを使ってコールバック待ちをすることで同期的な呼び出しが可能になるのでその形で実装。

- App.xaml.cs

.NET Standard側での初期化処理として、各種ライブラリの初期化、DIコンテナへのコンポーネントの登録、画面遷移用の画面の登録などを行う。
その後、メイン画面もDIコンテナから取得して初期画面へ遷移。

なお、Appクラスでの初期化処理、メイン画面の表示時の初期化処理、メイン画面中の最初の画面の表示等の処理順序を考慮して、適切なタイミングで各種処理の開始を行う。

- App.xaml/Resources

App.xamlではアプリケーション全体で使用するスタイルをResourceDictionaryに定義する。

Webアプリのcssのようなもので、可能な限りセマンティックな定義を行う。
各画面のxamlでは、要素に対してはApp.xamlで定義したStyleのみを指定し、色、サイズ/マージン/パディング、フォントといったプロパティについては個別に定義しない。
Gridを用いてレイアウトする際のHeight/Widthは個別にしないと難しいかもしれないが。

色については、アプリケーション用のテンプレートを用意してそれを使用し、デザインを統一する。
また、上記のような部分は切り出してResourcesフォルダに分離。
絵文字フォントを使用する際のリソースなどもResourcesフォルダに配置。

サイズの指定については、使用すべき値というものがあるので、それを踏まえて統一的なデザインをすること。
例えば3、6、9、12、18、24、36、48、72等。

- MainPage.xaml/MainPageViewModel.xaml

MainPage.xamlでは画面の共通枠部分(タイトルバー等)をレイアウト。
また、MainPage.xaml.csではBackボタンをハンドリングし、その処理は個々の画面に判断させるようにディスパッチを行う。

MainPageViewModelでは、画面枠部分と連携する部分の定義や、アプリケーション起動中常時受信するイベントの設定等を行う。





## Behaviors

コントロールの振る舞いに対する共通化はBehaviorとして切り出して定義する。  
共通的な振る舞いの例としては「Entryのフォーカス時は背景色を変更する」等であるが、これはVisualStateManagerで実現できる。  

XamarinにおけるEffects相当の実装もMAUIではBehaviorsとして実装する。

## Components

インスタンスベースの機能としてコンテナで管理し、ViewModelにインジェクションして使う部品をComponentと位置づける。  
主に、標準では提供されない・MauiComponentsや他のライブラリででは機能が存在しないプラットフォーム固有の処理を実装する。  
例としては、バーコードの処理等。  

プラットフォーム固有の呼び出しが必要なものはpartialクラスとして実装する。   

## Controls

主に複合コントロール等、アプリケーション固有のコントロールを作成する場合はここで定義する。
既存コントロールの拡張についてはBehaviorsで対応できるものはBehaviorsで対応する。

## Converters

アプリケーション固有のConverterを定義する。  
値によって色や文言を切り替えるケース等、アプリケーション固有の変換処理を実装する。  

なお、Converterにはロジックは記述せず、ロジック自体はDomainの処理などへ委譲する。  
また、色等の定義も決め打ちにせず、プロパティとして用意して実際の色自体の指定はApp.xamlで行う。  

## Domain

定数、データ長定義や、表示等に依存しない書式定義、金額計算、時間計算等、I/O処理等に依存しない純粋なアプリケーションドメインのロジックを配置する。  

ケースによってはこの部分はアセンブリを分けて、サーバ側と共通のものを使用することも考えられる。  

## Extender

他のフレームワークを拡張する機能を実装する。  

## Helpers

staticクラスとして作成する共通ユーティリティクラスや、他の処理で使用するヘルパークラス等を配置する。
なお、アプリケーション・ドメイン固有の処理に関するものはHelpers下ではなくDomain下のクラスとして定義するものである。

## Input

物理キー制御のためのクラスを配置する。  
物理キーの制御は、Androidのキーイベント前処理でフックを行ない、Inputo下のクラスを使用してルーティング、ショートカットの実行等を行なう形で共通的に処理する。  

物理キーのない端末ではInput下の機能は使用せず削除する。

## Markup

アプリケーション固有のXAMLマークアップ拡張を定義する。  
最低限必要なのは画面IDを定義するViewIdのみで、それ以外はドメイン固有のものが必要になるようであれば追加する。  

## Messaging

アプリケーション固有のBehaviorと対になるMessengerが必要な場合はここで定義する。  

## Models

アプリケーションで使用するデータ構造を定義する。  
データアクセスや通信といった用途のものについては、その特性上、賢いモデルというよりもデータ構造のみの定義に特化したものとしてPOCOで実装する。  
ドメインの処理についてははDomain下のクラスを使用したり、拡張メソッドなどで定義する形とする。  

用途に応じて、以下のようなサブフォルダーを用意してそこに配置する。  

|フォルダ|クラスサフィックス|概要|
|:----|:----|:----|
|Api|Request/Response|API呼び出しで使用するリクエスト・レスポンスの構造|
|Entity|Entity|データアクセスで使用するテーブルと対応する構造|
|View|View|データアクセスで使用するテーブルとは対応しない結果セット用の構造|
|-|-|その他、選択状態と対象のタプルやエラーコードと戻り値のタプル等|

## Modules

個々の画面の実装を配置する。  
Modules下に機能分類毎にさらにサブフォルダを作成し、そこに機能単位で実装をまとめる形とする。  

Modules直下には遷移用画面IDの定義や、画面間でパラメータを受け渡す際のパラメータ名定義やパラメータビルダー等のクラスも配置する。  

各画面はXxxView.xaml(.cs)とXxxViewModel.csをペアで作成する。  
各ViewではViewModelはコンテナから取得してBindingContextに設定する。  

View及びViewModelの実装要件を以下に記述する。  

### View

- XxxView.xaml.csでは画面遷移用のID定義以外は行わない
- ロジックはViewModelで実装、コントロールの振る舞いの共通化はBehaviorsに切り出して、コードビハインドは使用せずMVVMで実装する

### ViewModel

- Usa.Smart.Mauiで定義するViewModelBaseをベースクラスとして、アプリケーション固有要件(シェルとの連携等)を実装したAppViewModelBaseをベースクラスとして実装する
- Viewで発生するイベントはCommandで受けて処理を実行する
- ViewModelトリガーでViewの要素に対する処理要求を行う場合はMessengerを使用する
- ViewModelではあくまで画面内での制御のみを実装し、実際のロジックはUsecaseやServicesで実装し、Fat ViewModelにならないように注意する
- 表示項目の値の反映はINotifyPropertyChangedを使用するが、単純なものについてはNotificationValueを使用する形で問題ない

## Platforms

プラットフォーム固有の処理を実装するが、ここでは共通的に使用するプラットフォーム固有のヘルパーやActivityの初期化のような処理のみを実装する。  
各機能で使用するプラットフォーム固有の処理呼び出しについては、Platforms下の処理で初期化を行なうのでは無くMAUIアプリケーションのライフサイクルイベントで初期化を行なったり、コンポーネントをpartialクラスで実装してそちらで行なう形とする。

## Services

データベース処理、通信処理等のプリミティブな機能を処理単位で実装する。  
DbConnection、HttpClient等の処理はこのレイヤ内で完結させて上位層には露出させず、上位とのやりとりはModelsを引数/戻り値として処理する。  

このレイヤはアプリケーションの下位層であり、ServiceクラスからComponent等を参照してはいけない。  
初期化用の情報はコンポーネント登録時に渡す形とし、一連の処理でDialogを表示するような処理はServicesではなくUsecase層の責務とする。

## Shell

タイトルバーに表示する文言など、画面の共通部分と個々の画面でのやりとりをする機能を実装する。  
MAUI標準で提供されるシェルの機能は業務用アプリケーションでは機能が合致しない部分があり、またアプリケーション毎に必要とされる要件が異なることも多いので、この部分はアプリケーションの要件を検討して必要な機能を実装する。  
一般的に必要になる項目としては、タイトルバー、通知アイコン、共通ボタンの配置等。  

## State

アプリケーションの状態について、各画面内のものはViewModelで管理するが、ライフサイクルがアプリケーションスコープのものはここに定義する。  
アプリケーション実行中はずっとオンメモリに存在し、かつINotifyPropertyChangedのようなものを想定している。  
例えばログイン中のユーザ情報等。  

また、設定ファイルのように永続化して管理するものも便宜的にここに配置する。  
なお、DB処理はStateではなくServiceとして実装する。  

Stateのクラスは、ViewModelやUsecaseにインジェクションして使用する。  

## Usecase

本テンプレートのアーキテクチャでは、一連の処理を実装する部分をUsecaseクラスとして定義する。  
例えば、通信処理を行い、結果のメッセージを表示し、取得した値をDBに書き込む等の一連の処理について考える場合、通信やDB等のプリミティブな処理はServicesで実装するとして、これらの処理をViewModel内で実装すると処理が重複したり、ViewModelが肥大化する問題が発生する。  
そこで、一連の処理部分をViewModelとServiceの中間層となるUsecaseとして位置づけて切り出すことで、Serviceに過剰な責務を持たせたりViewModelをFatにしないようにする。  

UsecaseクラスはServiceクラスと同様にステートレスなシングルトンとしてインスタンスベースでコンテナでの管理を行なう。  
Serviceクラスとは異なり、UsecaseクラスではServicesの他、ダイアログや他のComponent、Stateの参照などをViewModelと同様に行なって良い。  

Usecaseクラスの実装単位のイメージとしては、「ログイン処理」や「バージョンアップ処理」等。

# 📦使用ライブラリ

使用するライブラリについて記述する。  

## 一覧

|名称|用途|必須|自前|
|:----|:----|:----|:----|
|Camera.MAUI|カメラ|カメラ未使用時は不要| |
|CommunityToolkit.Maui|MAUI準公式| | |
|Components.Maui|MAUI各主機能| |○|
|Microsoft.AppCenter.*|AppCenter|AppCenter未使用時は不要| |
|Microsoft.Data.Sqlite|SQLite|DB未使用時は不要| |
|Microsoft.Extensions.Logging.Debug|ログ出力| | |
|QRCoder|QR機能|QR出力がない場合は不要| |
|Rester|Web API|API未使用時は不要|○|
|System.Interactive|LINQ拡張| | |
|System.Linq.Async|LINQ拡張| | |
|System.Reactive|Rx| | |
|Usa.Smart.Core|共通ライブラリ| |○|
|Usa.Smart.Converter|型変換| |○|
|Usa.Smart.Data.*|データアクセス|DB未使用時は不要|○|
|Usa.Smart.Mapper|オブジェクトマッパー| |○|
|Usa.Smart.Maui.*|MAUI固有機能| |○|
|Usa.Smart.Navigation.*|画面遷移| |○|
|Usa.Smart.Reactive.*|Rx| |○|
|Usa.Smart.Resolver.*|Dependency Injection拡張| |○|

なお、必須としないライブラリについては、それに関する機能が必要ない場合には参照を削除する。  

## MAUI

### CommunityToolkit.Maui [[ドキュメント]](https://learn.microsoft.com/ja-jp/dotnet/communitytoolkit/maui/)

MAUI準公式の拡張機能としてCommunityToolkit.Mauiを使用する。

## System

### System.Interactive [[公式]](https://github.com/dotnet/reactive)

LINQ補助。  

### System.Linq.Async [[公式]](https://github.com/dotnet/reactive)

非同期LINQ。  
非同期処理はWebでもMAUIでも使用するので必須とする。  

### System.Reactive [[公式]](https://github.com/dotnet/reactive)

Reactive Extensions。  
WPF、MAUI等でのイベントの取り扱いに必須となる。  
Rx自体については https://reactivex.io/ を参照。  

## Microsoft

### Microsoft.Extensions.Logging [[ドキュメント]](https://learn.microsoft.com/ja-jp/dotnet/core/extensions/logging)

アプリケーションからのログ出力は自前で作るのではなくILoggerインターフェースを使用して行なう。  
Logging Providerの実装としては、MauiComponentsでAndroidのLogcatへ出力するものとファイルへの出力を行なうものを用意しており、それを使用する。  

### Microsoft.AppCenter [[公式]](https://azure.microsoft.com/ja-jp/products/app-center/)

クラッシュレポート、監視に利用する。  
コンシューマー用途等ではモニタリングは必須とする。  

### Microsoft.Data.Sqlite [[ドキュメント]](https://learn.microsoft.com/ja-jp/dotnet/standard/data/sqlite/)

端末内のデータベースとしてはSQLiteを使用し、ライブラリとしてはPC側と同じくMicrosoft.Data.Sqliteを使用する。  
Microsoft.Data.SqliteはADO.NET標準のインターフェースを実装しており、Usa.Smart.Data.Mapperをはじめ一般的なデータアクセスライブラリが使用可能である。  

## デバイス

### Camera.MAUI [[公式]](https://github.com/hjam40/Camera.MAUI)

CameraViewについてはCommunityToolkit.Mauiで提供予定であるが、そちらが提供されるまではCamera.MAUIを使用する。  

### QRCoder [[公式]](https://github.com/codebude/QRCoder/)

QRの出力に使用する。  

QRのデコード・出力に対応するZXingライブラリのMAUI版はCameraViewの対応待ちのところがあったりするので、それらの対応が終わるまではCamera.MAUIとQRCoderを使用する。  
なお、メーカー製ハンディーターミナルであれば固有のバーコードスキャンライブラリがあるはずなので、それを使用する。

## Smart

SmartライブラリのドキュメントについてはGitLabのリポジトリ内を参照。

### Usa.Smart.Maui

MAUI用のMVVM関連のライブラリ。  
Prism等を使用しない理由は、MAUI用の開発が活発でない点があるのと、ナビゲーションやDI等他のライブラリとの統合がしやすい点を考慮してのこと。  

提供する機能を以下に示す。  

|分類|名前空間|推奨Prefix|概要|
|:----|:----|:----|:----|
|Animation|Smart.Maui.Animations|sa|アニメーション用のBehaviorを提供|
|Converter|Smart.Maui.Data|sc|共通的なConverterを提供|
|Command|Smart.Maui.Input| |INotifyPropertyChangedと連携するCommand実装を提供|
|Behavior|Smart.Maui.Interactivity|si|共通的なBehaviorを提供、WPFのMicrosoft.Xaml.Behaviors.Wpfのようなもの|
|Markup|Smart.Maui.Markup|sm|共通的なMarkup拡張を提供|
|Messaging|Smart.Maui.Messaging| |MVVMのMessenger機能|
|Validation|Smart.Maui.Validation| |ヴァリデーション機能を提供|
|ViewModel|Smart.Maui.ViewModels| |ViewModel基盤を提供|

ViewModelのクラスをベースとして使用する事で、以下のような事が容易にできるようになっている。  

- 実行中の制御(非同期処理中にボタンを無効にする等)
- INotifyPropertyChangedと連動したCommandの状態制御
- Rxで購読したイベントの自動解除等を行なうためのIDisposable管理

### Components.Maui

使用頻度の高いプラットフォーム固有処理や他ライブラリを使用する際に必要となるボイラーテンプレート部分をライブラリ化したもの。  
以下のような機能を提供。  

|項目|概要|
|:----|:----|
|Dialog|標準的なものに加え、実行中表示用のProgressやIndicator等も実装|
|Location|IGeolocationサービスをイベントベースで扱えるようにしたもの|
|Logger|Logging ProviderとしてLogcatとファイル出力のものを用意|
|Popup|CommunityToolkitを使用したダイアログ表示機能|
|Screen|画面の方向制御、状態制御等を実装|
|Serialize|設定値の読み書き等に使用するシリアライザー|
|Speech|音声認識、音声合成機能のラッパー|

### Smart.Navigation

画面遷移用のライブラリ。  
MAUIが標準で持っているNavigationPage等はNext/Back型の遷移等を想定したものであり、自由な画面間の遷移が必要になる業務アプリケーションの用途にはマッチしない。  
また、標準の機能ではタイトルバー等共通領域のカスタマイズも面倒なため、画面の一部を任意に切り替えられるような機能をライブラリとして用意している。  
これはAndroidネイティブな画面遷移を1Activity+複数Fragmentで構成するのに似た考え方となる。  

遷移の方式としては、任意の画面へ遷移する機能の他、旧画面を一時的に非表示にしつつスタック的にPush/Popするような遷移機能も提供。   
画面間のパラメータ受け渡し、遷移前/後のイベントや、特定機能の画面を遷移する間だけ共通のオブジェクトを参照できるようにするコンテキスト機能等も提供。  

### Smart.Resolver

DIコンテナ。  
標準のMicrosoft.Extensions.DependencyInjectionと差し替えることで、画面遷移との連携やVMの自動登録が可能となるので使用。  

### Usa.Smart.Data.Mapper

MAUIでの使用を想定したMicro-ORM。  
Dapperと同様に使用可能。  

### Usa.Smart.Core

他ライブラリのための基本機能及びユーティリティー、拡張メソッドを提供。

### Usa.Smart.Converter

型変換機能。

### Usa.Smart.Mapper

AutoMapperと同種のオブジェクトマッパー。  

### Usa.Smart.Reactive

Rx拡張機能。  
イベント処理記述の簡略化用途で使用。  

XAMLのバインディングについては、NotificationValue<T>を使用してそのValueメンバにバインディングすることにより、個別のgetter/setterを定義することが不要となる。  
NotificationValue<T>はAndroidネイティブのData bindingにおけるObservableFieldと同じようなものと認識すれば良い。  

### Rester

REST処理用のHttpClient拡張メソッド。  
標準でもGET/POSTのJsonマッピング用の拡張メソッドは提供されるようになったが、データ圧縮への対応、ファイルアップロード/ダウンロード(進捗コールバック対応あり)機能等を実装しているためこちらを使用する。

## その他

テンプレート中には含めていないが採用を検討するライブラリについて。

- System.Text.Encoding.CodePages/Smart.Text.Japanese SJISを扱う必要がある場合
- Shiny (BLE、プッシュ通信等)

# 🔖機能サンプル解説

(📝作成中)

## 画面遷移

(📝作成中)



## ダイアログ

(📝作成中)



## デバイス

(📝作成中)



## 通信機能

(📝作成中)



## 物理キー制御

(📝作成中)



## 基本機能

(📝作成中)



## 標準サンプル

(📝作成中)



## UIサンプル

(📝作成中)




## 🚧未実装機能

テンプレートの機能サンプルとして未実装の機能について記述する。  
これらの機能が必要になった時は先に相談すること。  

- QR
- Camera
- NFC
- WiFi情報
- Bluetoothe(プリンター等)
- BLE(センサー値の取得等)
- Audio再生
- 指紋認証
- 地図
- Push通知
- AIサービス
- チャート/グラフ
- SSH/FTP

# 🎨アーキテクチャ

MAUIでアプリケーションを作成する際に必要となる知識及びアーキテクチャの方針について記述する。  

## XAML

(📝作成中)




## MVVM

(📝作成中)




## Messaging

(📝作成中)




## 非同期処理

(📝作成中)




## Reactive

(📝作成中)




# 開発TIPS

開発手法に関するTIPSを記述する。  
Analyzers及びEditorConfigについてはテンプレートプロジェクトにおいて設定済みとなっている。  

## CI

開発時はJenkinsを用いてCIを行う。  
CIでは静的チェックツールを使用し、品質を確保する。  

以降に記述するツールを使用し、ソース更新時は常時ビルドと静的チェックを行なうようにする。  
また、成果物についてはCIを使用してビルドしたもののみを正とする。  

静的チェックツールとして使用するAnalyzersについてはDirectory.Build.propsを使用して全プロジェクトに適用する。  
静的チェックの項目のうち、適合しないものはルールを無効化し、特定ケースのみマッチしないものについてはSuppressMessageして、「ルール自体は緩くするが警告は0」形を基本とする。  
CIの静的チェックで警告が出力された場合には優先して対処する。  

## .NET ソース コード分析

AnalysisModeの設定によりビルド時の静的チェックを有効にする。  

- https://learn.microsoft.com/ja-jp/dotnet/fundamentals/code-analysis/overview

## InspectCode

InspectCode(JetBrains.ReSharper.CommandLineTools)を使用して静的チェックを行なう。  

- https://www.nuget.org/packages/JetBrains.ReSharper.CommandLineTools
- https://pleiades.io/help/resharper/InspectCode.html

## StyleCop.Analyzers

コードスタイル統一のチェックにはStyleCop.Analyzersを使用する。  

## EditorConfig

書式の統一はVisual Studioが標準で対応しているEditorConfigを使用する。  

### Xaml Styler

XAML記述の統一にはXaml Stylerを使用する。  
Visual Studioの拡張機能としてインストールすること。  










----------

# 基礎知識

UIはXAMLで構築。
WPF/UWP等と同様だが、Xamarin.Forms固有の方言や機能的な制限がある。

以下のあたりの内容を踏まえておく。
https://qiita.com/toshi0607/items/241a7161491092d2a3e0

## MVVM(Binding、Command、Messenger、Converter、Behavior)

Binding: Observerベースでのコントロールの値と変数の同期
Command: ボタンを押下された時等の、実行と実行可否のバインディング
Messenger: イベントへのバインディング
Converter: バインディング時の値の変換
Behavior: 振る舞いの部品化

この辺はWPF/UWPだけでなく、Native AndroidでのData bindingや、WebでもSPAなんかでは同じ考え方。

## Dependency Injection

画面、VM、サービスやコンポーネントといったアプリケーションを構築するオブジェクトツリーの解決。
Webアプリで使っているものと考え方は同じ。

## 非同期

async/await、Taskについて。
この辺が何を行っているのかを理解しておかないと、無駄なスレッド化、デッドロック、結果待ちの非同期処理中なのにボタンが押せてしまう、等の問題を作り込んでしまうので注意。

## Rx(Reactive Extensions)

例えばLINQがシーケンスに対するPull(Interactive)な処理なのに対して、イベントシーケンスについてのPush型での処理。
頻出パターンとしては、eventベースの通知の購読、複数のテキストが全部埋まったらボタンを有効化等。












# 設計<a id="design"></a>

## 名称<a id="design-name"></a>

### メソッド/変数<a id="design-name-method"></a>

- 変数名などだけでなく、URL(Controller/Action)、DBの項目名等も共通の名称を使用する
- 単語を省略した名称は使わない
- 翻訳サイト等を使って日本語名から英語名を決める場合、ニュアンスの異なる単語を採用してしまうケースが多い  
  単語単体ではなく対象となる業種での使い方を含めた文章を変換してみる、一端英語にした単語を逆変換してみる等を行い、適切な単語であるかを確認する
- 一般的にプログラムでよく利用されている名称を使用する、以下のようなサイトも参照  
  [https://codic.jp/](codic)
- .NETの一般的な名前付きのガイドラインについては以下を参照  
  [https://docs.microsoft.com/ja-jp/dotnet/standard/design-guidelines/naming-guidelines](https://docs.microsoft.com/ja-jp/dotnet/standard/design-guidelines/naming-guidelines)


### プリフィクス/サフィックス<a id="design-name-prefix"></a>

- クラス名、メソッド名には処理に応じたプリフィクス/サフィックスをつけてルールを統一する
- Web API用のデータ構造はXxxRequest/XxxResponse等
- データアクセスについては(Query|Insert|Update|Delete|Merge)Xxx(List)(ForYyy|ByYyy)等
- bool型の変数は意味に応じた接頭語をつける(HasXxx、UseXxx、IsXxx等)



## レイヤ<a id="design-layer"></a>

### 責務の分離<a id="design-layer-soc"></a>

- 機能要件と非機能要件を混在した実装を各担当にまかせるのではなく、非機能要件を確保する枠組みを用意した上で、各機能の実装を担当者が行う形にする
- データ処理とプレゼンテーションの責務分けを正しく行う
- レイヤードアーキテクチャの決定については、プロジェクト内においてメンバー間で共通認識を持つことに重点をおく  
  どういった種類の処理がどこに実装されているか、処理を追加する場合にあどこに実装すべきかのガイドラインを示すようにする
- WebのController、デスクトップ/モバイルのViewModelにはロジックは記述せず、所謂Fat Controllerを避ける  
  ロジック自体はService、Usecase等の下位のレイヤに分離するして、それを使用するようにべきもの
- 文字列操作や数値操作などの汎用的な処理は個別に記述せず、ヘルパー・ユーティリティクラスに分離する  
  また、ライブラリに存在するものはそれを使用する
- ビジネスロジック、ControllerやVM層などでSystem.IO、System.Reflection等のクラスを直接使用しない  
  責務として別レイヤ、ユーティリティーに分離すべきものである
- 意味を考えずにアーキテクチャを適用することは、アーキテクチャ不在と同レベルである  
  アーキテクチャについて考えると言うことは、なぜそれが必要になったのかをゼロベースで考え、背景を理解し、必要であればレイヤを省略する判断も行う
- アーキテクチャの公式を覚えるのでは無く、その意味を理解するようにする
  アーキテクチャの基本要素、Obsearbable、Declarative、Functionalといったものの考え方は普遍的に通用するのでそれを理解する
- 汎用処理のうち、固有の設定があり、インスタンスベースで扱うコンポーネントはDIベースで管理する  
  これはCSVやPDF出力、セキュリティ機能、シリアライザ等を想定
- ドメイン固有の処理については汎用的なヘルパーやコンポーネントとは別に、定数やサイズ等とともにドメイン名前空間に分離する  
  想定するものは、業務ルールに基づいた率や期間の計算、コードの値が○と□の時は△△が可能などの判定や、文字コードの先頭○文字が△△の意味を表す等といった場合のその部分の切り出し処理等  
  例えば、期の判断についてdate.Month < 4のような記述がController等に出現した場合、その部分はドメインの処理として切り出すようにレビューで指摘事項が発生する

```csharp
// 期の計算処理を切り出す
public static class Term
{
    public static int CalcYear(DateTime date) => date.Month < 4 ? date.Year - 1 : date.Year;
}
```

- ビュー固有の変換処理等はビューヘルパーとしてビューの基盤部分に定義する  
  想定するものは、値に応じた表示する項目の色・CSS classの決定等
- インターフェースと実装を分離するのは「低レベル層/Externalとの境界領域は検証コストが高いので、抽象化してシミュレーション等をしやすくすることが開発の秘訣」が理由だからであり、思考停止してなんにでもインターフェースと実装を用意するような設計をしない
  そうなってしまうのであればそれは抽象化する層を誤っている証拠

### ヘルパー/ユーティリティ実装<a id="design-layer-helper"></a>

- 可変パラメータを使用するWeb APIのURL生成やSQL生成のような処理はビルダーとして作成することを検討する

```csharp
// Queryパラメータビルダーの例
public sealed class ApiQueryBuilder
{
    private readonly StringBuilder buffer = new StringBuilder();

    private void Prepare()
    {
        buffer.Append(buffer.Length > 0 ? '&' : '?');
    }

    public ApiQueryBuilder AppendName(string value)
    {
        Prepare();
        buffer.Append("name=").Append(HttpUtility.UrlEncode(value));
        return this;
    }

    public ApiQueryBuilder AppendYear(int value)
    {
        Prepare();
        buffer.Append("year=").Append(value);
        return this;
    }

    // ...

    public string ToQuery() => buffer.ToString();
}
```

```csharp
// Usage
var query = new ApiQueryBuilder()
    .AppendName("test")
    .AppendYear(2021)
    .ToQuery();
```

- 拡張メソッドを使用して使用性の高いAPI設計を行う  
  特定のデータクラスのメンバを複合的に判断する式等を共通的に使用する場合、その部分はデータクラスの拡張メソッドとして定義してそれを使用し、式を使用する箇所で個別に記述しない  
  ビュー層においてはプリミティブ型に対する拡張メソッドを用意し、文字列フォーマットの記述の統一や式の簡略化も行う

```csharp
// cshtmlで使用するための拡張メソッド
public static class FormatExtensions
{
    public static string Date(this DateTime value) => value.ToString("yyyy/MM/dd");

    public static string DateTime(this DateTime value) => value.ToString("yyyy/MM/dd HH:mm:ss");

    ...
}
```

```html
<!-- このような書き方はせず -->
<span>@date.ToString("yyyy/MM/dd HH:mm:ss")</span>
<!-- 拡張メソッドを以下のように使って記述を統一する -->
<span>@date.DateTime()</span>
```

## ログ<a id="design-log"></a>

### 用途<a id="design-log-use"></a>

- トレース、ログ(イベント)、メトリクス(集計数値)は分けて考える
- ログ出力は「誰」が「どう使う」のかを明確にした上で出力方式と出力内容を決定する
- 運用者が参照するのか、開発者がデバッグ用途に使うのか等で出力すべき内容は異なるので、それを意識して出力すべきログの内容とログレベルを統一する
- 情報過多にならず、かつ、ログから発生した処理のトレースが可能であることが望ましい
- 「アプリケーションのログ出力内容を見れば、作り手の技量は推測できる」

### 出力内容<a id="design-log-content"></a>

- アプリケーションとして重要なイベントはINFO、WARNレベルでログを出力する
- デバッグログの出力内容は各担当者の思いつきで出すのではなく、統一されたレベルの設計に沿って出力する
- 開発用のダンプやイベント発生の記録については、個々の機能に記述すべきものではなく、アプリケーションの基盤部分等で出力する
- メソッドの入口/出口でログ出力を行うような原始的なやりかたではなく、処理前後のログが本当に必要なのであればフィルタ等の他の手段も用いてログの出力を行う  
  また、このレベルでのログ出力というのはログ設計を何も考えていないことの裏返しの可能性があるので注意する
- 例外の発生をログ出力する場合はスタックトレースを潰さずに出力する
- ログの分析やエラー検知を行う場合、ツールが解析しやすい出力フォーマットにする  
  項目の桁を揃えたり、LTSVやJson形式での出力を検討する
- ログレベルについては次の考え方とする

| レベル | 概要                                     |
|:-------|:-----------------------------------------|
| ERROR  | 致命的な例外発生を記録                   |
| WARN   | 異常系のイベント発生を記録               |
| INFO   | 正常系の重要なイベントの発生を記録       |
| DEBUG  | 開発者が動作のトレースが可能な情報を記録 |

* ERROR

予期せぬ例外やバグ等により発生する致命的な例外の発生を記録する。  
例外設計部分の記述とも関連するが、アプリケーション基盤部分でのみ出力する内容であり、各担当者が実装する機能部分でのERROR出力は存在しないのがあるべき形。

* WARN

異常系のイベント発生時を記録する。  
Webアプリケーションで外部のAPI呼び出しに失敗したときや、データベースのキーを条件とする削除処理において削除結果が0件だった(既に他の処理で)時等に出力する。
なお、データベースの削除済みのケースのように、処理を続行して問題のケースではログの記録のみを行い、処理はそのまま続ける形とする。

* INFO

正常系の重要なイベントの発生時を記録する。  
セキュリティ的に重要な処理を行う場合等に出力する。

* DEBUG

開発者が動作のトレース可能な情報を記録する。  
外部と通信をする際の通信内容のダンプ、運用時にINFOで出すには冗長な内部処理に関するイベントの発生等を出力する。  
各機能の処理部分での出力と言うよりも、アプリケーションの基盤部分で共通的に出力したり、I/O処理のようなものは低レベルでのライブラリ部分での出力を基本とする。
出力内容のレベルは統一し、送信と受信、生成イベントと破棄イベント等のような同種のログは出力内容も揃える形にする。  
出力内容についてフォーマットを行う場合、現在のログのサポートレベルを確認してデバッグ出力が有効の時のみログ出力を行う形とし、デバッグ出力が無効な設定時には余計なフォーマット処理は動作しないようにする。

## 例外処理<a id="design-exception"></a>

### 基本<a id="design-exception-basic"></a>

- 大前提として、継続的にシステムを開発していくのであれば、例外処理やログに関する設計については運用・保守を想定した形で設計する必要がある  
  特に、システム開発においてで焼き畑農業しか経験のない人と、技術をベースにサービスなり商流なりを立ち上げた経験のある人の間には、考え方に埋めがたい溝があることがあるので注意する
- .NETの場合、アプリケーションレイヤにおける異常系の通知は例外ではなく戻り値で処理する  
  エラーコードと値の双方が必要な場合、タプルを使用もしくは専用のデータ構造を用意して使用する
- 予期せぬ例外を個別にcatchするようなコードを書かない
- 予期せぬ例外とはHWの致命的な障害やプログラムのバグによって起こされるものを指し、これらのケースはFail-astで考えるべきもので、発生時はエラーの通知やログ出力のみを行って処理を続行させない
- 予期せぬ例外に対する処理の記述はグローバルな例外ハンドラやフィルタで行う
- そもそも予期できる例外については例外が発生しないようなコードの記述を行う  
  I/O系については事前にファイルの存在をチェックしたり、パース系の処理についてはParse()ではなくTryParse()を使う等
- 一部の例外への対処を除き、アプリケーションレイヤでははcatchの記述が存在すること自体がレビューの指摘事項になる  

### 個別にcatchすべき例外<a id="design-exception-catch"></a>

- 実際に処理を実行したタイミングでしか問題が検出できず、問題発生がライブラリからは例外で通知されるケースについてのみ個別のcatchが適切なケースとなる  
  個別にcatchを行うケースを以下に示す

* DBの重複登録

このケースで例外を処理するということは、重複自体は通常のオペレーションで発生することが想定される設計にしているということである。  
その場合には例外のログ出力等も行わず戻り値で重複発生を返して上位層ではそれによって処理分岐を行う形になる。  
重複登録発生による判断が不要なも処理なのであれば、例えばINSERTではなくMERGEで処理する等、そもそも例外が発生しない作りにする。

* HttpClient等による外部通信  

例外のパターンとしては、キャンセルやタイムアウト、HTTPステータスがとれるケースととれないケース、通信自体は成功してもJsonの内容がおかしくシリアライズできないケース等が考えられる。  
こられの例外を上位のレイヤでは直接扱わず、拡張メソッド化やライブラリを使用して例外をエラーコードに変換し、アプリケーションの上位レイヤでは例外ではなくエラーコードでの分岐を行う形にする。

### 対処<a id="design-exception-action"></a>

- 例外をログに出力する際はログのスタックトレースを出力し、問題発生箇所が特定できるようにする  
  ログライブラリではExceptionを引数にとるメソッドを使用し、自前のフォーマットで情報を削って出力しないようにする
- 個別のcatchを記述する処理の場合、想定される例外なのであればエラーコードに変換するのみでこの段階ではログ出力は行わない  
  この場合、例外の発生自体は想定されるケースなのであり、ログを出力するのであれば上位のエラーコードを判断する側で出力する
- 発生した例外のうち一部のケースのみで例外をハンドルするケース、例えばDBに対する例外のうち重複登録のみを処理するようなケースでは、想定外の場合には例外の再throwを行う  
  この場合の記述はthrow ex;ではなくthrow;としてスタックトレースはオリジナルの情報を保持する

```csharp
try
{
    // 重複登録が発生する可能性のある処理
}
catch (DbException ex)
{
    if (Dialect.IsDuplicate(ex))
    {
        return false;
    }

    throw;
}
```

## 異常系<a id="design-error"></a>

### 処理漏れ<a id="design-error-poor"></a>

- 設計者と実装者が異なるような進め方をする場合、異常系の考慮漏れに帯する指摘事項が多くなり、レビュー段階で実質再設計が発生するようなケースが多いので注意する
- 設計書は異常系の処理を設計書に明確に記述する

### 異常系への対処<a id="design-error-action"></a>

- 致命的な異常の発生はその段階でユーザに対してインタラクションを発生させる設計とする
- 処理続行可能な問題(例えば、削除しようとしたデータが既に削除済みだった等)の場合、警告ログを出力して処理を続行するようにする
- 例外設計の部分の記述も踏まえた内容とする
- Web APIのように通信エラーが発生しやすい処理は冪統制がある設計にする

## 設定<a id="design-config"></a>

- 外部設定にすべきものと定数にすべきものを正しく扱う  
  通信の接続先の情報、使用するデバイスの情報等はプログラム中ではなく外部定義で扱う  
  なお、コードではなく設定ファイルで定義する場合でも、それがバイナリ中に埋め込まれるのであれば扱いはコードと同じであり、これらの情報はアプリケーションのバイナリの変更なしに切り替えられるようにする

<div style="page-break-before:always"></div>



# 言語仕様<a id="lang"></a>

## 共通<a id="lang-common"></a>

### 基本<a id="lang-common-basic"></a>

- 変数宣言のスコープは小さくする

```csharp
// ×
{
    bool result;

    ...

    for (var i = 0; i < data.Length; i++)
    {
        result = Execute(data[i]);
        ...
    }
}
```

```csharp
// ○
{
    ...

    for (var i = 0; i < data.Length; i++)
    {
        var result = Execute(data[i]);
        ...
    }
}
```

- 複数のチェックを通過した後に処理を行うようなものはガード句を使ってチェックに引っかかった時点でreturnする形とし、処理のネストを減らす

```csharp
// ×
public bool Sample()
{
    if (チェック1)
    {
        if (チェック2)
        {
            (処理)
            return true;
        }
    }
    
    return false;
}
```

```csharp
// ○
public bool Sample()
{
    if (!チェック1)
    {
        return false;
    }
    
    if (!チェック2)
    {
        return false;
    }
    
    (処理)
    
    return true;
}
```

- API間で引数の順序は意味が同じになるように統一する  
  例えば年度/部門/担当のようなものの順序について、DB設計等の時点で意味や効率、処理を踏まえた設計が行えていればそれにあわせるだけで問題ないのが想定する形となる
- 静的チェックツールでも一部は検知するようにしているが、C#型とCLR型の記述は統一する  
  string.Empty、String.IsNullOrEmpty()等

### 処理<a id="lang-common-code"></a>

- 同一処理中で現在時刻としてDateTime.Nowの値を複数買い使用する場合、同じ値を使用する意味の処理ではDateTime.Nowの値を一端変数に代入してそれを使用し、DateTime.Nowを都度使用しない
- DBのエンティティ用クラスを手動で作成したり詰め替えたりしている箇所は、正しい処理を行わず、冗長な記述をしている可能性があり指摘事項となる  
  例えばWebで選択項目の先頭項目用に未選択の意味のインスタンスを手動で作成している場合、それはoptionを個別に記述する内容となる

```csharp
// ×
public IActionResult Index()
{
    var offices = MasterService.QueryOfficeList();
    offices.Insert(0, new OfficeEntity { Code = string.Empty, Name = "(選択してください)" });
    ViewBag.Offices = offices;

    return View();
}
```

```html
@{
    var offices = new SelectList(ViewBag.Offices, "Code", "Name");
}

<form method="post">
    <select asp-for="Code" asp-items="offices">
    </select>
</form>
```

```csharp
// ○
public IActionResult Index()
{
    ViewBag.Offices = MasterService.QueryOfficeList();

    return View();
}
```

```html
@{
    var offices = new SelectList(ViewBag.Offices, "Code", "Name");
}

<form method="post">
    <select asp-for="Code" asp-items="offices">
        <option value="">(選択してください)</option>
    </select>
    ...
</form>
```

## 文字列<a id="lang-string"></a>

### 結合<a id="lang-string-join"></a>

- 文字列の結合については勘違いしているケースが多いので注意する
- リテラル同士の+演算子での結合はコンパイル時に単一リテラルに変換されるので使用して問題ない
- 逆に非リテラル+演算子での結合は都度コピーが発生するので使用せず、StringBuilderを使用して、サイズ上限が確定できる場合には初期サイズを明示する

上記を踏まえた正しい記述は以下になる。

* パターン1

```csharp
// ×
var sql = new StringBuilder();
sql.Append("SELECT * FROM Xxx ");
sql.Append("WHERE Id = @Id");

// ○
var sql = "SELECT * FROM Xxx " +
          "WHERE Id = @Id";
```

* パターン2

```csharp
// ×
var sql = "SELECT * FROM Xxx ";
if (Id.HasValue)
{
    sql += "WHERE Id = @Id";
}

// ×
var sql = new StringBuilder();
sql.Append("SELECT * FROM Xxx ");
if (Id.HasValue)
{
    sql.Append("WHERE Id = @Id");
}

// ○ 必要なバッファサイズが想定できるなら明示的に指定(多い分には厳密でなくて良い)
var sql = new StringBuilder(32);
sql.Append("SELECT * FROM Xxx ");
if (Id.HasValue)
{
    sql.Append("WHERE Id = @Id");
}
```

### フォーマット<a id="lang-string-format"></a>

- フォーマットには文字列補間を使用する  
  [https://docs.microsoft.com/ja-jp/dotnet/csharp/language-reference/tokens/interpolated](https://docs.microsoft.com/ja-jp/dotnet/csharp/language-reference/tokens/interpolated)

## IEnumerable<a id="lang-enumerable"></a>

### 基本<a id="lang-enumerable-basic"></a>

- IEnumerableで扱うということは、それを使用する側としては評価が確定していないと判断するものである(実態は遅延評価かもしれない)ということを理解する
- 変数/引数/戻り値について、IEnumerableで扱うべきものなか、それとも配列やListのようにマテリアライズした形で扱うべきものなのかを正しく処理する
- 件数の上限が DBからの大量データをファイルに出力するような場合、マテリアライズするのではなくIEnumerableで遅延評価しながらの処理として実装する

### LINQ<a id="lang-enumerable-linq"></a>

- 記述はクエリ構文は使用せずメソッド構文で統一する

```csharp
// クエリ構文
var query = from x in values
            where x % 2 == 0
            select x * 3;

// メソッド構文
var query = values
    .Where(x => x % 2 == 0)
    .Select(x => x * 3);
```

- 遅延評価であることと、ToList()等によるマテリアライズのタイミングを正しく理解して使用する
- 静的チェックツールで同じIEnumerableに対する繰り返しの処理は警告にしているが、これはIEnumerableの実態が遅延評価のDBアクセスのようなものの場合、それぞれおマテリアライズのタイミングで都度別のクエリが実行されることになるという点を理解する
- 上記を踏まえた上で、必要なケースでは一端中間データをマテリアライズし、その結果に対して更に複数の処理を行うケースもあるが、それが適切なケースについて理解しておく
- 遅延評価については、静的チェックツールでも警告がでるようにしているがCount() > 0ではなくAny()を使うべき意味を理解しておく
- List<T>.ForEach()は使わずforeachを使用する

## 非同期<a id="lang-async"></a>

- ASP.NET Coreはデフォルトで少数スレッドを非同期で動作させる形に最適化されており、DB処理等は非同期APIの利用を基本とする
- モバイルアプリケーションにおいてもUIスレッドを長時間ブロックしない作りが求められるため、非同期APIの利用を基本とする
- モバイルアプリケーションでは、非同期処理の実行中は明示的に操作を禁止する仕組みが必要であり、これに誤りがあるとボタンを連打すると落ちる/おかしな動きをする等の問題が発生するので、この部分を正しく認識した設計/実装を行う
- 実行中の制御はフレームワーク/アプリケーション基盤のレベルで実装し、個々の処理では意識させないようにする
- Thread.Sleep()はスレッドを占有するのでTask.Delay()を使用する  
  なお、それ以前に上記を使用してウエイトを入れているような処理はレビューでの指摘事項となる  
  リトライを行う際のウエイト等は適切な使用方法であるが、デバイス制御・低レベルI/Oの応答待ち用にウエイトを入れている所は、受信イベント or タイムアウトの発生を待つような形の実装にすべきケースがほとんどである  
  こういった部分の実装に誤りがあると運用時に致命的な問題を起こす可能性が高いので、デバイス制御・低レベルI/Oのような処理部分の実装はそのスキルを持った要員を割り当てるようにする
- フレームワークがTaskを前提としている箇所は戻り値にTaskを使用するが、そうでない箇所はValueTaskを使用する  
  Service層やASP.NET CoreのControllerの戻り等はValueTaskを使用、Xamarin.Formsのコマンド部分等はTaskを使用する形になる
- ライブラリの場合はConfigureAwait(false)が基本、UI層の場合はConfigureAwait(true)(デフォルト)が基本となる  
  ただし、UI層についてはアプリケーションの構造によっては明示的な指定は不要とする
- Task.Wait()、Task.Resultを使用しない、awaitして処理する  
  これらの記述があるコードはほぼ間違いなく非同期処理を理解しておらず、デッドロックが発生するコードを記述している可能性がある
- 自前でTask.Run()をしているようなケースは非同期処理の実装を理解せずに使っている可能性が高く、レビュー指摘事項になる  
  Task.Run()を使用するのが妥当なケースは非同期APIの内部実装が同期処理になっており、そのままだとスレッドコンテキストが切り替わらず実行中を表示するため等のUI更新が動作しないケースで、明示的にスレッドコンテキストを切り替えてそれを有効にする場合等のみとなる  
  APIの形式が非同期ベースになっていたとしても、内部実装としては同期的に動くケースもある点に注意する

- Androidのようにコールバックが基本のAPIを逐次的に使いたいケースではTaskCompletionSourceを使用する

```csharp
// TaskCompletionSource使用例
public class ApplicationDialog : ApplicationDialogBase
{
    private readonly Activity activity;

    public ApplicationDialog(Activity activity, IDialogs dialogs)
        : base(dialogs)
    {
        this.activity = activity;
    }

    public async override ValueTask<bool> Confirm(string message, bool defaultPositive = false, string? title = null, string ok = "OK", string cancel = "Cancel")
    {
        var dialog = new ConfirmDialog(activity);
        return await dialog.ShowAsync(message, defaultPositive, title, ok, cancel);
    }

    // Physical key supported dialog for handy terminal
    public class ConfirmDialog : Java.Lang.Object, IDialogInterfaceOnShowListener, IDialogInterfaceOnKeyListener
    {
        private readonly TaskCompletionSource<bool> result = new();

        private readonly Activity activity;

        [AllowNull]
        private AlertDialog alertDialog;

        private bool positive;

        public ConfirmDialog(Activity activity)
        {
            this.activity = activity;
        }

        public Task<bool> ShowAsync(string message, bool defaultPositive, string? title, string ok, string cancel)
        {
            positive = defaultPositive;

            alertDialog = new AlertDialog.Builder(activity)
                .SetTitle(title)!
                .SetMessage(message)!
                .SetOnKeyListener(this)!
                .SetCancelable(false)!
                .Create()!;
            alertDialog.SetOnShowListener(this);
            alertDialog.SetButton((int)DialogButtonType.Positive, ok, (_, _) => result.TrySetResult(true));
            alertDialog.SetButton((int)DialogButtonType.Negative, cancel, (_, _) => result.TrySetResult(false));

            alertDialog.Show();

            return result.Task;
        }

        public void OnShow(IDialogInterface? dialog)
        {
            var button = alertDialog.GetButton(positive ? (int)DialogButtonType.Positive : (int)DialogButtonType.Negative)!;
            button.RequestFocus();
        }

        public bool OnKey(IDialogInterface? dialog, Keycode keyCode, KeyEvent? e)
        {
            if ((e!.KeyCode == Keycode.Del) && (e.Action == KeyEventActions.Up))
            {
                dialog!.Dismiss();
                result.TrySetResult(false);
                return true;
            }

            return false;
        }
    }
}
```

```csharp
// 使用例
if (await dialog.Confirm("削除しますか？"))
{
    // 削除処理
    ...
}
```

- 非同期処理の一般的な注意事項については以下も参照しておく  
  [https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md)  
  [https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AspNetCoreGuidance.md](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AspNetCoreGuidance.md)

## コメント<a id="lang-comment"></a>

### 使用文字<a id="lang-comment-text"></a>

- ASCIIコードにある値は全角ではなく半角を使用する(A-Z、0-9、()等)
- カナは半角カナを使用しない

### 内容<a id="lang-comment-content"></a>

- コードと1:1になるようなコメントを記述しない

* 有名な格言

```
コードには How
テストコードには What
コミットログには Why
コードコメントには Why not
```

<div style="page-break-before:always"></div>



# データベース<a id="db"></a>

## 設計<a id="db-design"></a>

### DB設計<a id="db-design-design"></a>

- SQL Server等であれば名称もPascalCaseにしてプロパティとのマッピングを簡略化する  
  それ以外のデータベースでsnake_case等を使用している場合、ライブラリの名称変換設定やAttribute等を使ってマッピングを指定する

- 更新日、更新者のような監査・追跡用途の列を業務的な処理に使用しない  
  DB上で作成される更新日と業務的に意味のある日付は同じものとは限らない

- VARCHARの空文字とNULLについて、NULL可にする必要があるのかについて確認する  
  不要なNULL可のカラムを作らない
- 列挙値のカラムについて、格納されるコードの文字自体に意味がないのであればCHARよりINT等を使用する


## 処理記述<a id="db-code"></a>

### SQL<a id="db-code-sql"></a>

- SQLで表示用の加工や構造変換は行わず、SQLは効率的なクエリでの処理に特化する形とする  
  表示用の加工はビュー層固有の要件であり、データ自体の処理とは仕様変更の頻度が異なる点等も踏まえ、データ処理と表示用の加工は処理を分離する  
  例えば「SELECT CONCAT(FirstName, LastName) AS Name ...」のようなことはせず、FirstNameとLastNameは個別に取得して表示部分で表示の要件に応じた結合を行う
- 標準SQLの関数を使用する形で統一  
  例えばNULLIFではなくCOALESCEの使用等
- WHERE句の記述で型変換が必要になるケースはDB設計が間違っている可能性があるので確認する  
  日付時刻型の項目について、日時部分のみでの一致を検索するようなケースは、一致で判定するのでは無く当日から翌日までの範囲で検索する  
  また、カラム側の値を変換するのではなくパラメータ側を変換する記述として、インデックスがある場合にはそれが効くような形の記述にする
- 結合をする際のテーブルのエイリアスについて省略した名称はつけず、T0、T1…の名称で統一し、主体であるT0に対して他のクエリセットであるT1以降を結合していくという考え方で統一する
- 同じクエリ内で同一のサブクエリが繰り返し出現するような場合は共通式(CTE)を使用する  
  [https://docs.microsoft.com/ja-jp/sql/t-sql/queries/with-common-table-expression-transact-sql](https://docs.microsoft.com/ja-jp/sql/t-sql/queries/with-common-table-expression-transact-sql)

### データアクセス<a id="db-code-access"></a>

- パラメータはSQLの文字列として構築するのでは無く、バインドパラメータを使用する  
  また、低レベルのデータアクセス処理をそのまま使用せず、Micro-ORMや2-Way SQLライブラリを使用する  
  トラッキング型のORMは、アプリケーション要件としての細かいトランザクション制御に問題がない場合のみ使用する
- LIKEについてはエスケープしたものをパラメータとして使用する  
  エスケープの詳細はデータベースによって異なるが以下のような形になる

```csharp
public string LikeEscape(string value) => return Regex.Replace(value, @"[%_\[]", "[$0]");
```

- 既存のレコードをSELECTして、結果有無によってINSERT or UPDATEをするような処理は、同一トランザクション下で明示的にロックをして処理を行う  
  また、MERGEで処理できる処理はMERGEで実装する

```sql
-- SQL Serverでの例、HOLDLOCKによりSERIALIZABLEになる
SELECT * FROM Xxx WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id

-- レコード有無に応じて
INSERT ... 
-- or
UPDATE ...
```

<div style="page-break-before:always"></div>




# デスクトップ/モバイルアプリケーション<a id="desktop"></a>

## MVVM<a id="desktop-mvvm"></a>

### Converter<a id="desktop-mvvm-converter"></a>

- コントロールのプロパティと1:1になる値をVMに定義するのでは無く、Converterを使って元の値からプロパティへの変更を行い、意味の重複する項目を定義しない  
  例えば元となる値(例としてはbool)に応じてコントロールの文言、色、Enable等を変更する場合、文言用のプロパティを個別に定義するのでは無く、BoolToText、BoolToColor、(反転する場合はReverse)等のConverterを使用する  

```csharp
// ×
public class MainPageViewModel : ViewModelBase
{
    public NotificationValue<bool> Check { get; } = new();

    public NotificationValue<string> CheckText { get; } = new("OFF");

    public NotificationValue<Color> CheckColor { get; } = new(Color.Green);

...

    private void Switch()
    {
        Check.Value = !Check.Value;

        CheckText.Value = Check.Value ? "ON" : "OFF";
        CheckColor.Value = Check.Value ? Color.Red : Color.Green;
    }
}
```

```xml
<Label BackgroundColor="{Binding CheckColor.Value}"
       Text="{Binding CheckText.Value}" />
```

```csharp
// ○
public class MainPageViewModel : ViewModelBase
{
    public NotificationValue<bool> Check { get; } = new();

...

    private void Switch()
    {
        Check.Value = !Check.Value;
    }
}
```

```xml
<Label BackgroundColor="{Binding Check.Value, Converter={sm:BoolToColor True=Red, False=Green}}"
       Text="{Binding Check.Value, Converter={sm:BoolToText True='ON', False='OFF'}}" />
```

- 汎用的なConverterについてはライブラリで定義されているものを使用し、アプリケーション固有のものは個別に作成する
- 元となる項目が複数の値のケースについてはIMultiValueConverterを使用する  
  例としては、複数stringを接続/フォーマットした内容をプロパティに設定したり、複数項目(bool値やString.IsNullOrEmpty()結果)のORやANDをプロパティに設定するケース等

```csharp
// Entry全てが入力されたらCheckBoxを有効にする例:MultiBinding版
public class MainPageViewModel : ViewModelBase
{
    public NotificationValue<string> Text1 { get; } = new();
    public NotificationValue<string> Text2 { get; } = new();
    public NotificationValue<string> Text3 { get; } = new();
}
```

```xml
<Entry Text="{Binding Text1.Value}" />
<Entry Text="{Binding Text2.Value}" />
<Entry Text="{Binding Text3.Value}" />

<CheckBox>
    <CheckBox.IsEnabled>
        <MultiBinding Converter="{StaticResource AllConverter}">
            <Binding Path="Text1.Value"
                     Converter="{StaticResource NotEmptyToBoolConverter}" />
            <Binding Path="Text2.Value"
                     Converter="{StaticResource NotEmptyToBoolConverter}" />
            <Binding Path="Text3.Value"
                     Converter="{StaticResource NotEmptyToBoolConverter}" />
        </MultiBinding>
    </CheckBox.IsEnabled>
</CheckBox>
```

- 複数項目から算出した値をVM側でも使用する場合、評価結果用のプロパティを定義する形で良い(ButtonでCommandを使用する場合は実行可否の制御用にこちらの形になる)  
  この場合、評価結果のプロパティはReactive Extensionsを使用して元となるプロパティのINotifyPropertyChangedを購読して更新する形とし、元となるプロパティの更新時に評価結果のプロパティも手動で更新するのではなく、連鎖して自動で更新され形とする  

```csharp
// Entry全てが入力されたらCheckBoxを有効にする例:Rxで一端シンクする版
public class MainPageViewModel : ViewModelBase
{
    public NotificationValue<string> Text1 { get; } = new();
    public NotificationValue<string> Text2 { get; } = new();
    public NotificationValue<string> Text3 { get; } = new();

    public NotificationValue<bool> CanCheck { get; } = new();

    public MainPageViewModel()
    {
        Disposables.Add(Observable
            .CombineLatest(Text1.AsValueObservable(), Text2.AsValueObservable(), Text3.AsValueObservable())
            .Subscribe(x => CanCheck.Value = !x.Any(String.IsNullOrEmpty)));
    }
}
```

```xml
<Entry Text="{Binding Text1.Value}" />
<Entry Text="{Binding Text2.Value}" />
<Entry Text="{Binding Text3.Value}" />

<CheckBox IsEnabled="{Binding CanCheck.Value}" />
```

### Behavior<a id="desktop-mvvm-behavior"></a>

- コードビハインドは使用せず、コントロールの振る舞いについてはBehaviorで部品化する

### ViewModel<a id="desktop-mvvm-vm"></a>

- VMはGlueであることを意識して、画面からのコマンドの受付、サービスの呼び出し、状態の更新のみを行い、ロジックの詳細は下位のレイヤで行う形にする

- アプリケーションによっては画面間を超えて使用されるStateオブジェクトに機能を分離する

- 下位のレイヤとしては、プリミティブなDB操作やWeb API呼び出しを実装するサービスクラスと、VMとサービスの中間になるユースケースクラスを用意する  
  ここでのユースケースクラスの位置づけは、画面からの操作をトリガーとして開始し、プリミティブなサービスや各種コンポーネントの利用を行う形で一連の処理を行うものという認識で良い  
  例としては以下のような部分をユースケースとして実装し、VMやサービスとは責務を分ける

| ユースケースクラス実装単位例                         |
|:-----------------------------------------------------|
| サーバへの認証及び最新情報の取得                     |
| Web APIを使ってのマスタダウンロードとローカルDB更新  |
| ローカルDBに蓄積したデータの一括送信と送信後の後処理 |
| 定時後の締め処理                                     |
| アプリケーションダウンロードとバージョンアップ       |
