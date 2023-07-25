# Template project for MAUI

MAUI用テンプレートプロジェクトの使用方法について記述する。

----

# 🎉始め方

## ビルド

テンプレートプロジェクトを取得し、Visual Studioでビルドを行なうと機能サンプルの入ったテンプレートを作成できる。  
テンプレートプロジェクトはそのままAndroidで実行可能なので、動作確認を行ないながらソースを参照して構造を理解する。  

## ビルドバリアント

単一ソースでHW固有機能を使用する複数モデルのビルドに対応する場合、設定ファイルで対象とするHW用の定義を切り替えてビルドを行なう。  
プロジェクト中のDeviceProfile.sample.propsファイルを.DeviceProfile.propsとしてコピーすると、その内容がビルドに反映されるような設定となっている。  

なお、.DeviceProfile.propsは各ユーザーが開発環境でのみ使用するものとし、CI環境でリリース物をビルドする場合にはビルドオプションで同様の指定を行なう形とする。  
この仕掛けによりビルド環境の設定のみで、単一ソースを使用して、ソース管理中に含めるファイルには変更を加えずに、複数モデル向けのビルドが可能となる。  

以下に.DeviceProfile.propsの例と設定内容について記述する。  


```xml
<Project>

  <!-- 現在の開発対象であるプロファイルを指定する -->
  <PropertyGroup>
    <DeviceProfile>Default</DeviceProfile>
  </PropertyGroup>

  <!-- プロファイル(機種)毎に条件付きコンパイルで使用する定数を定義する、名称ルールは任意 -->
  <PropertyGroup Condition="'$(DeviceProfile)'=='Default'">
    <DefineConstants>$(DefineConstants);DEVICE_DEFAULT_ANDROID</DefineConstants>
  </PropertyGroup>

  <PropertyGroup Condition="'$(DeviceProfile)'=='DT-999'">
    <DefineConstants>$(DefineConstants);DEVICE_HAS_KEYPAD</DefineConstants>
  </PropertyGroup>

  <!-- ビルド環境に応じてアプリケーションに埋め込む定数値を定義する -->
  <PropertyGroup>
    <EmbeddedBuildProperty>DeviceProfile=$(DeviceProfile),Flavor=Development,AppCenterSecret=,ApiEndPoint=https://server/</EmbeddedBuildProperty>
  </PropertyGroup>

</Project>
```

* DefineConstants

DefineConstantsを設定すると、プリプロセッサで使用される条件付きコンパイルの定義を設定できる。  
物理キーボードの有無や、使用するバーコードライブラリの違い等はこの設定で条件付きコンパイルにより切り替える形とする。  

* EmbeddedBuildProperty

商用版とテスト版といったアプリケーションのフレーバーや、接続先に関するシークレット情報等について設定する。  
EmbeddedBuildPropertyを指定すると、その内容がアプリケーション中に埋め込まれて参照可能となる。
EmbeddedBuildPropertyの値の参照は、partialメソッドでBuildProperty属性を指定したクラスを作成すると参照処理が自動生成される形となっている。  
テンプレートプロジェクト中のVariants.cs参照、

## サーバー処理

通信処理の対となるサーバー側テンプレートについては [template-maui-server](../../template-maui-server) 参照。

### サーバ実装機能

- [X] ファイルアップロード・ダウンロードAPI(簡易FTP)
- [X] データベースCRUD API
- [X] サーバー側時刻取得API(簡易NTP)
- [X] WASM UI フロントエンド基盤
- [ ] APIリクエスト/レスポンス圧縮
- [ ] デバイスステータス通知
- [ ] リアルタイムサーバーPush
- [ ] クラウドサービス向けロギング
- [ ] フロントエンドUI(デバイスステータス、ファイル操作、DB操作)
- [ ] フロントエンド認証機能
- [ ] フロントエンドエラー画面
- [ ] フロントエンドプログレス画面

----

# 🔧開発環境

開発自体はVisual Studio 2022だけあればできるが、実際の開発時にはCIでの常時ビルドを行い、Analyzersによる静的チェックやVisual Studio拡張機能を使用したフォーマットの統一等を行ため、それらの項目について記述する。  
なお、AnalyzersやEditorConfig、Xaml Stylerについては、テンプレートプロジェクト内に設定ファイルのテンプレートも含めた形となっている。  

## CI

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

## Xaml Styler

XAML記述の統一にはXaml Stylerを使用する。  
Visual Studioの拡張機能としてインストールすること。  

----

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

----

# 📕アプリケーション構造

テンプレートプロジェクトのアプリケーション構造について記述する。  
この構成をアプリケーションを構築する際のスタンダードとする  

以下、プロジェクトの各フォルダとその中の主要なクラスについて内容を記述する。

## プロジェクト直下

プロジェクト直下のクラスでは、アプリケーションの初期化処理や画面枠、アプリケーションの共通情報に関する処理を定義する。  

### 初期化処理

アプリケーションの初期化処理は以下の順序で実行される。

1. MauiProgram.CreateMauiApp()でのコンポーネント設定
2. ApplicationInitializer.Initialize()でのコンポーネント初期化
3. App.OnStart()での初期画面表示
4. MainPageViewModelコンストラクタでの画面枠初期化
5. MainPageViewModel.OnCreated()でのライフサイクルイベントによる初期化処理

以下、各処理が呼び出されるタイミングとそこで行うべき処理について記述する。

#### MauiProgram.cs

アプリケーションのスタートアップルーチンとして、アプリケーションの設定と使用するコンポーネントの登録を行う。  

- MAUIアプリケーションのライフサイクル管理の設定を行う
- 使用するフォント登録を行う
- CommunityToolkit.Maui等のライブラリの設定を行う
- アプリケーション固有のControl、Behaviorの設定を行う
- DIコンテナをSmart.Resolverに切り替え、アプリケーションで使用するコンポーネントの登録はSmart.Resolverの初期化処理で行う
- ログ出力の設定を行う
- DB用やAPI用のライブラリ等、コンポーネントベースではない処理の初期化もこのタイミングで行ってしまう

コンポーネント登録処理では以下のコンポーネントの登録・設定を行う。  

- ダイアログ等、Components.Mauiで提供されるコンポーネントの登録を行う
- 画面遷移機能の登録を行う
- ライブラリでは提供されない、アプリケーション固有として実装するコンポーネントの登録を行う
- アプリケーションの状態管理クラスの登録を行う
- ServiceとUsecaseクラスの登録を行う
- 初期化処理ルーチンとしてApplicationInitializerの登録を行う

#### ApplicationInitializer.cs

MauiProgramで登録したコンポーネントのインスタンスが参照可能になった状態で最初に呼び出されるのはApplicationInitializer.Initialize()の処理となる。  

- 各コンポーネントのインスタンスが必要かつ画面の表示前に行うような初期化処理を記述する

#### App.xaml / App.xaml.cs

ApplicationInitializerでの初期化後に呼び出される。

- MainPageの表示処理とその前後で行うべき初期化処理を記述する

また、App.xamlではアプリケーション全体で使用するスタイルをResourceDictionaryに定義する。  
App.xamlに直接スタイルを定義するのではなく、Resources/Styles下に項目毎にxamlファイルを用意し、App.xamlではそれらをマージする形とする。  

スタイルはセマンティックな設計とし、各画面のxamlでは要素に対してはApp.xamlで定義したStyleのみを使用して、色、サイズ/マージン/パディング、フォントといったプロパティについては要素に個別指定しない形とする。
サイズの指定については、dpiを考慮した推奨値があるので、それを踏まえて統一的なデザインを行う。  
例えば3、6、9、12、18、24、36、48、72等。  

マテリアルカラーの定義等、アプリケーション固有ではない項目についてはアプリケーション固有の項目とファイルを分けて定義を行う。  

#### MainPage.xaml / MainPage.xaml.cs / MainPageViewModel.cs

Appクラスでメインウインドウが設定されると初期化処理が実行される。

- MainPage.xamlには画面のシェル部分(タイトルバー等共通部分)をレイアウトする
- MainPage.xaml.csではBackボタンのハンドリングを行い、個々の画面に要求をディスパッチする
- MainPageViewModelでは、シェル部分のイベント処理を実装し、現在の画面に要求をディスパッチする
- アプリケーションのライフサイクルイベントに関する処理をIAppLifecycleの各イベントで実装する

MainPageViewModel.OnCreate()はAndroidのActivityが作成された後に呼び出される。  
そのため、プラットフォーム固有の処理としてActivityの参照が必要な初期化処理はこのタイミングで呼び出す形となる。  

#### MainActivity.cs

プロジェクト直下ではなくPlatforms/Android下のクラスであるが、初期化処理と関連するのでMainActivity.csに定義する処理についても記述する。  
MainActivity.csでは以下の定義のみを行い、各機能で必要になるプラットフォーム固有の処理呼び出しはpartialクラスで実装する形とする。  

- ActivityAttributeによるActivity設定
- ActivityResolver.Init()によるライブラリ内で使用されるActivity情報の設定
- 物理キーを処理する場合、DispatchKeyEvent()でのキーフック

ActivityのOnResume()/OnPause()といったライフサイクルイベントでの呼び出しが必要な処理については、Activityに処理を記述するのではなく、MainPageViewModelで実装するIAppLifecycleのイベントに処理を記述する。  
IAppLifecycleはMAUIアプリケーションのライフサイクルイベントをViewModelに通知するために用意しているものであり、ActivityのOnResume()/OnPause()はそれぞれIAppLifecycleのOnActivated()/OnDeactivated()に該当する。  
MAUIのライフサイクル管理については以下を参照。
https://learn.microsoft.com/ja-jp/dotnet/maui/fundamentals/app-lifecycle

### 共通情報

以下のクラスではアプリケーションの共通情報に関する処理を定義する。

#### ApplicationState.cs

アプリケーション・デバイスの状態を集約的に管理する場合、ここに処理を定義する。  
ここで想定する内容としてはバッテリー状態や通信状態等であり、ログイン状態のようなものはState下のクラスで定義する。  

#### Permissions.cs

アクセス許可に関する処理を定義する。  
https://learn.microsoft.com/ja-jp/dotnet/maui/platform-integration/appmodel/permissions

#### Extensions.cs

アプリケーション固有の拡張メソッドを定義する。  

#### Log.cs

Source Generatorによるソース生成ログを定義する。  
https://learn.microsoft.com/ja-jp/dotnet/core/extensions/logger-message-generator

#### Variants.cs

Source GeneratorによるEmbeddedBuildPropertyの値の展開先を定義する。  

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

----

# 📦ライブラリ

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

----

# 🔖機能サンプル

機能サンプルとしてテンプレート中に実装している項目について記述する。  

## 画面遷移

|項目|概要|
|:----|:----|
|Edit|一覧画面と編集画面でのパラメータ受け渡しと、編集画面を新規用と編集用の2パターンで使用する時の遷移方法|
|Stack|前画面のインスタンスをスタックに保持しつつ、新規画面をPushし、前画面に戻る場合はPopして遷移する方法|
|Wizard|ScopeAttributeにより特定画面間で共通のデータを共有をする方法|
|Shared|共通の中間画面を間に挟み、中間画面からは異なる画面へ遷移する方法|
|Initialize|画面遷移時に非同期で初期化処理を実行する方法|
|Cancel|画面遷移時に条件分岐により遷移をやめて戻る方法|
|Dialog|Popupにより子画面を表示する方法|

### 🚧TODO

- Popup表示サンプルのFix

## ダイアログ

|項目|概要|
|:----|:----|
|Information|情報ダイアログの表示|
|Confirm|確認ダイアログの表示|
|Confirm3|3ボタン確認ダイアログの表示|
|Select|選択ダイアログの表示|
|Input|入力ダイアログの表示|
|Indicator|進捗インジゲーターの表示|
|Lock|オーバーレイによる画面のロック|
|Loading|オーバーレイによる処理状態の表示|
|Progress|オーバーレイによる進捗の表示|
|Snackbar|Snackbarによる通知|
|Toast|Toastによる通知|

## デバイス

|項目|概要|
|:----|:----|
|Info|デバイス情報、アプリケーション情報、画面情報表示|
|Status|バッテリー状態、通信状態参照|
|Sensor|各種センサー情報の参照|
|Location|GPSでの位置情報参照|
|QR|QRコードの表示|
|Misc|画面回転、バイブレーター制御、フラッシュライト制御、スクリーンショット取得、音声合成・認識|

## データベース

|項目|概要|
|:----|:----|
|Insert|データ挿入|
|Update|データ更新|
|Delete|データ削除|
|Query|データ取得|
|BulkInsert|一括インポート|
|DeleteAll|一括削除|
|QueryAll|一覧取得|

## 通信

|項目|概要|
|:----|:----|
|Get server time|サーバー時刻取得API呼び出し|
|Test error|サーバー側でエラーが発生するケースの動作確認|
|Test delay|サーバー側で処理遅延が発生するケースの動作確認|
|Data list|サーバー側DBから一覧を取得するAPI呼び出し|
|Download|ファイルダウンロードAPI呼び出し|
|Upload|ファイルアップロードAPI呼び出し|

### 🚧TODO

- APIでのCUDサンプル

## 物理キー制御

|項目|概要|
|:----|:----|
|Entry|物理キーのみによる入力制御|
|List|物理キーのみによるListView制御|
|Misc|物理キーのみによるフォーカス移動|

### 🚧TODO

- Xamarin.Formsからの移行Fix
- PopupとActionButtonでの動作確認

## 標準サンプル

|項目|概要|
|:----|:----|
|Get server time|サーバー時刻取得API呼び出し|
|Typography|テンプレートで定義しているラベル用スタイルの参照|
|Style|テンプレートで定義しているスタイル(Button用)の参照|
|Font|テンプレートに含んでいるフォントの参照|

### 🚧TODO

- Smart Coverterの使用例一覧
- Smart Behaviorの使用例一覧
- Validationサンプル
- Style拡充

## UIサンプル

### 🚧TODO

- Good Looking UIサンプル...

## 🚧未実装機能

テンプレートの機能サンプルとして未実装の機能について記述する。  
一部機能はXamarin.Forms用の実装はあるが、MAUI用への移行が終わっていないものを含む。  
これらの機能が必要になった時は先に相談すること。  

- QR読み取り (デバイス固有ライブラリを使用しない汎用的な方法)
- Camera
- NFC(FeliCa参照)
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

----

# 🎨アーキテクチャ

MAUIでアプリケーションを作成する際に必要となる知識及びアーキテクチャの方針について記述する。  

## XAML

UIはXAMLで構築する  
WPF/UWP/Xamarin.Forms等と同様だが、固有の方言や機能的な制限は存在する。  

Xamarin.Formsの内容であるが、同様の項目が多いので以下の内容は理解しておくこと。  
https://qiita.com/toshi0607/items/241a7161491092d2a3e0

## MVVM

画面の処理はコードビハインドで処理を記述するのではなく、バインディングを利用してMVVMで処理を記述する。  
この辺はWPF/UWP/MAUI等だけでなく、Native AndroidでのData bindingや、WebのSPA等にもある概念であり、考え方は同じものである。  

主要な概念について以下に記述する。  

|項目|概要|
|:----|:----|
|Binding|Observerベースで、コントロールのプロパティと変数の同期を行う概念|
|Command|ボタン押下時等の、実行処理/実行可否判定へのバインディング|
|Messenger|Control側でVM側からの要求を受け取るためのイベントへのバインディング|
|Converter|バインディング時の値の変換|
|Behavior|Controlの振る舞いの部品化|

### Converter

- ControlのプロパティとViewModelで定義するメンバについては、1:1になる値をViewModel側に定義するのでは無く、Converterを使って元の値からプロパティへの変更を行い、意味の重複する項目を定義しないようにする

例えば元となるbool値に応じて、コントロールの文言、色、Enable等を変更する場合、それぞれ用のプロパティを個別に定義するのでは無く、BoolToText、BoolToColor、(反転する場合はReverse)等のConverterを使用する例を示す。  

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

なお、上記例はConverterをMarkup拡張を使用して都度Converterのインスタンスを作成している形の例となるが、実際にはConverterはStyles.xamlで共通的に使用するものを定義する形とする。

- 汎用的なConverterについてはライブラリで定義されているものを使用し、アプリケーション固有のものは個別に作成する
- Controlへバインディングする値が複数の値から計算される値である場合はIMultiValueConverterを使用する

例として、複数のEntry全てに入力がある場合にのみCheckBoxを有効にする例を示す。

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

なお、上記のような単純な例ではRxを使用した方が意味として望ましい場合もあり、Rxを使用した記述例については後述する。

### Behavior

Controlの振る舞いに関する処理は、コードビハインドで記述するのではなくBehaviorとして分離して共通化を行う。

### ViewModel

- VMはGlueであることを意識して、画面からのコマンドの受付、サービスの呼び出し、状態の更新のみを行い、ロジックの詳細は下位のレイヤで行う形にする  
- 画面間を超えて使用されるStateオブジェクトに機能を分離したり、特定画面間だけで共有するデータがある場合はScopeAttributeを使用してデータの共有を行う
- 下位のレイヤとしては、プリミティブなDB操作やWeb API呼び出しを実装するServiceクラスと、VMとサービスの中間的な位置づけになるUsecaseクラスを用意する  

| ユースケースクラス実装単位例                         |
|:-----------------------------------------------------|
| サーバへの認証及び最新情報の取得                     |
| Web APIを使ってのマスタダウンロードとローカルDB更新  |
| ローカルDBに蓄積したデータの一括送信と送信後の後処理 |
| 定時後の締め処理                                     |
| アプリケーションダウンロードとバージョンアップ       |

## Dependency Injection

View、ViewModel、サービスやコンポーネントといったアプリケーションを構築するオブジェクトツリーの解決はDependency Injectionで処理する。  
基本的な処理はサーバ側のものに同じなので以下を参照。  
https://learn.microsoft.com/ja-jp/dotnet/core/extensions/dependency-injection

なお、ViewModelの自動登録等、処理の利便性のため、IServiceProviderFactoryはSmart.Resolverのものを使用する。

## 非同期処理

async/await、Taskについて理解をしておかないと、無駄なスレッド化、デッドロック、結果待ちの非同期処理中なのにボタンが押せてしまう、といった問題を作り込んでしまうので注意する必要がある。  
以下に非同期処理に関する基本について記述する。  

- モバイルアプリケーションにおいてはUIスレッドを長時間ブロックしない作りが求められるため、非同期APIの利用を基本とする
- 非同期処理の実行中は明示的に操作を禁止する仕組みが必要であり、ここに誤りがあるとボタンを連打すると落ちる/おかしな動きをする等の問題が発生するので、この部分を正しく認識した設計/実装を行うこと
- 実行中の制御は、フレームワーク/アプリケーション基盤で実装しており、個々の処理では意識しない仕組みは用意してある
- Thread.Sleep()はスレッドを占有するので、ウエイトが本当に必要ならTask.Delay()を使用する
- リトライウエイトのような処理でのDelayは妥当な処理であるが、デバイス制御・低レベルI/Oの応答待ち等でウエイトを入れようとしている処理は、そもそもの考えを誤っているケースが多い
- フレームワークがTaskを前提としている箇所では戻り値にTaskを使用するが、そうでない箇所はより軽量なValueTaskの方をを使用する
- ライブラリの場合はConfigureAwait(false)が基本、UI層の場合はConfigureAwait(true)(デフォルト)が基本となる
- なお、テンプレート内では明示的な指定は不要とする設定を静的チェックのルールとして指定している
- Task.Wait()、Task.Resultを使用しない、awaitして処理する、これらの記述があるコードは非同期処理を理解しておらず、デッドロックが発生するコードを記述している可能性があると推測される
- 自前でTask.Run()をしているようなケースは非同期処理の実装を理解せずに使っている可能性が高い

Task.Run()を使用するのが妥当なケースは、非同期APIの内部実装が同期処理になっており、そのままだとスレッドコンテキストが切り替わらず実行中を表示するため等のUI更新が動作しないようなケースで、明示的にスレッドコンテキストを切り替えてそれを有効にするような場合のみとなる。  
APIの形式が非同期ベースになっていたとしても、内部実装としては同期的に動くケースもある点に注意する。  

- AndroidのネイティブAPIでは応答がコールバックで提供されているものが多いが、これをシーケンシャルな呼び出しで扱いたい場合はCancellationTokenSourceを使ってコールバック待ちをすることで対処が可能となる

以下にCancellationTokenSourceの使用例として、カスタム確認ダイアログを実装し、コールバックイベントであるOK/Cancelボタンの結果を待って処理を抜けるメソッドの実装例を示す。

```csharp
// TaskCompletionSource使用例
public async ValueTask<bool> Confirm(string message, bool defaultPositive = false, string? title = null, string ok = "OK", string cancel = "Cancel")
{
    var dialog = new ConfirmDialog(activity);
    return await dialog.ShowAsync(message, defaultPositive, title, ok, cancel);
}

public class ConfirmDialog : Java.Lang.Object, IDialogInterfaceOnShowListener
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

また、非同期処理の一般的な注意事項については以下も参照しておくこと。  

https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md  
https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AspNetCoreGuidance.md  

## Rx(Reactive Extension)

Rxは、例えばLINQがシーケンスに対するPull(Interactive)な処理なのに対して、イベントシーケンスについてのPush型での処理である。  
アプリケーション実装上での頻出パターンとしては、eventベースの通知の購読、複数のテキストが全部埋まったらボタンを有効化等がある。  

複数項目から算出した値をView側にバインディングするだけではなくViewModel側でも使用するような場合、MultiValueConverterを使用するのではなく、評価結果用のプロパティをViewModel側に定義してRxで処理するような形とする。  
例として、MultiValueConverterの所に記述した複数のEntry全てに入力がある場合にのみCheckBoxを有効にする処理のRx版を以下に示す。  


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
        // NotificationValueをIObservableとして扱い、どれかが1つでも変化したらSubscribeの処理を行う
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

----

# ✨設計基礎

MAUIアプリケーションに限定されない、設計に関する一般的な基礎知識について記述する。

## ログ設計

- ログ出力は「誰」が「どう使う」のかを明確にした上で出力方式と出力内容を決定する
- 運用者が参照するのか、開発者がデバッグ用途に使うのか等で出力すべき内容は異なるので、それを意識して出力すべきログの内容とログレベルを統一する
- 情報過多にならず、かつ、ログから発生した処理のトレースが可能であることが望ましい形となる
- デバッグログの出力内容はなんでも出力するのではなく、統一されたレベルの設計に沿って出力する
- 例外の発生をログ出力する場合はスタックトレースを潰さずに出力する
- ログレベルについては次の考え方とする

| レベル | 概要                                     |
|:-------|:-----------------------------------------|
| ERROR  | 致命的な例外発生を記録                   |
| WARN   | 異常系のイベント発生を記録               |
| INFO   | 正常系の重要なイベントの発生を記録       |
| DEBUG  | 開発者が動作のトレースが可能な情報を記録 |

### ERROR

予期せぬ例外やバグ等により発生する致命的な例外の発生を記録する。  
例外設計とも関連するが、アプリケーション基盤部分でのみ出力する内容であり、各担当者が実装する機能部分でのERROR出力は存在しないのがあるべき形である。  

### WARN

異常系のイベント発生時に記録する。  
例えばデータベースのキーを条件とする削除処理において、削除結果が0件だった時等、処理続行は可能であるが想定外の状態を検知したようなケースで出力する。  

### INFO

正常系の重要なイベントの発生時を記録する。  
ログインイベント等、セキュリティ的に重要な処理を行うような場合に出力する。  

### DEBUG

開発者が動作のトレース可能な情報を記録する。  
外部と通信をする際の通信内容のダンプ、運用時にINFOで出すには冗長な内部処理に関するイベントの発生等を出力する。  
出力内容のレベルは統一し、送信と受信、生成イベントと破棄イベント等のような同種のログは出力内容も揃えて対になるような形で出力する。  

## 例外処理

- まず基本として、.NETの場合、アプリケーションレイヤにおける異常系の通知は例外ではなく戻り値で処理する
- エラーコードと値の双方が必要な場合、タプルを使用もしくは専用のデータ構造を用意して使用する
- 予期せぬ例外に対して、各メソッドで個別にcatchするようなコードを書かない
- 予期せぬ例外が発生するような状況はFail-astで考えるべきものであり、発生時はエラーの通知やログ出力のみを行って処理を続行させない
- 予期せぬ例外への対処はグローバルな例外ハンドラ等で処理すべきものであり、テンプレートではCrashReportクラスを用意している
- 発生が予期できる例外については、例えばパース処理についてはParse()ではなくTryParse()を使う等、例外が発生しない形の記述を行う

### 個別に例外処理を記述すべきケース

各メソッドで個別にtry/catchを記述すべき例外は、ランタイムでしか問題を検出できず、問題発生がライブラリからは例外で通知されるケースについてのみである。  
以下のその例を示す。  

- DBの重複登録

重複が通常のオペレーションで発生することが想定される設計の場合、重複の例外はログ出力等も行わず、戻り値で重複発生を返して上位層ではそれによって処理分岐を行う形とする。  
また、重複登録発生による処理の分岐が不要なも処理なのであれば、例えばINSERTではなくMERGEで処理する等、そもそも例外が発生しない作りとする。  

- HttpClientによる外部通信

処理の呼び出しでキャンセルやタイムアウトの例外が発生するケース、外部とのIFにより応答の値が信用できずにパース処理で例外が発生するようなケースでは、例外をcatchして上位層にはエラーの種別を戻り値で返すような処理とする。  
なお、テンプレートの通信処理はResterライブラリを使用しており、この例外からエラーコードへの変換をライブラリ内で行っているため、例外処理は不要である。  

### 例外への対処

- 例外をログに出力する際はログのスタックトレースを出力し、問題発生箇所が特定できるようにする
- ログ出力だけして再throwや、スタックトレースを潰しての再throwを行うようなコードを記述しない
- 発生した例外のうち、一部ケースでのみで例外をハンドルするケース、例えばDBの重複登録例外だけは処理するようなコードは以下の記述とする

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

    // throw ex;ではなくthrow;としてスタックトレースはオリジナルの情報を保持する
    throw;
}
```

## 異常系への対応

- 条件によっては発生する可能性の高いものについては、ユーザに対してインタラクションを発生させる設計とする
- 例えば、Web APIの通信エラー等は自動でリトライするのではなく、いったんユーザにエラーの発生を通知してリトライするのか判断させるような作りにする等
- また、Web APIのような処理においては、端末側ではなくサーバ側の設計の話となるが、冪統制のあるAPI設計にする

## データベース

- 端末内で使用するSQLiteについては、テーブル名、カラム名はPascalCaseにしてオブジェクトとのマッピングを簡略化する
- 更新日、更新者のような監査・追跡用途の列を業務的な処理に使用しない
- 業務的に意味のある日付が必要であれば、更新日等とは別にカラムを定義する
- 列挙値のカラムについて、格納されるコードの文字自体に意味がないのであればTEXTよりINTEGERを使用する
- SQLレベルで表示用の加工や構造変換は行わず、SQLは効率的なクエリでの処理に特化する形とし、構造上の加工はViewModelで、表示上の加工はView上のConverter等を使用して行う

表示用の加工はビュー層固有の要件であり、データ自体の処理とは仕様変更の頻度が異なる点等も踏まえ、データ処理と表示用の加工は処理を分離する。  
例えば「SELECT CONCAT(FirstName, LastName) AS Name ...」のようなことはせず、FirstNameとLastNameは個別に取得して、xaml上ではFormatやMultiValueConverterによる出力加工を行うような形となる。  

- SQLiteは型が弱いため、オブジェクトとのマッピング時に.NET上の型とSQLite上の型の変換を行う形とする

テンプレート中にあるGuidTypeHandler、DateTimeTypeHandlerがその実装例で、それぞれGuidをTEXT、DateTimeをINTEGERで扱うための変換処理となっている。  

- パラメータはSQLの文字列として構築するのでは無く、バインドパラメータを使用する

テンプレートではMicro-ORMとしてSmart.Data.Mapperを使用しており、パラメータとしてEntityクラスや匿名型を使用できるようになっているので、それを使用すればバインドパラメータの処理は意識する必要のない形となっている。

## レイヤ設計

- ViewModel、Usecase、Serviceの責務を意識し、ViewModelがFatにならないように注意する
- 文字列操作や数値操作など、汎用的な処理は個別に記述するのではなく、ヘルパー・ユーティリティクラスに実装を分離する
- ビジネスロジックを記述するレイヤでIO、Reflection等のクラスを使用しない、それらの処理は別レイヤ、ユーティリティーに分離すべきものである
- アプリケーション・ドメイン固有の共通処理は、ヘルパー・ユーティリティクラスではなく、Domain下のクラスに分離する
- Domainクラスが大量に存在する場合、定数定義、ロジック等の単位でサブフォルダを作成する
- ビュー固有の処理はConverter等で実装するが、その中でさらにDomainクラスを使用することも考慮する

Domainクラスとして想定するものは、単に定数定義だけではなく、業務ルールに基づいた率や期間の計算や、コードの値による可能可否の判定、文字コードの意味の解釈といったものも含む。  
例えば、期の判断についてdate.Month < 4のような判定は個別に記述するのではなく、以下のような処理を切り出してそれを再利用する形で実装する。  

```csharp
// 期の計算処理を切り出す
public static class Term
{
    public static int CalcYear(DateTime date) => date.Month < 4 ? date.Year - 1 : date.Year;
}
```

## ビルダー

- Web API用のURL生成等で、動的なパラメータ生成が必要なものは生成部分をビルダークラスとして分離することを検討する
- また、特定の処理に関連するビルダークラスは、Helper下ではなく、例えば特定Service内のみで使用するものであればService下に配置する

以下にビルダークラスの実装例を示す。  

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

    public ApiQueryBuilder AppendYearIf(int? value)
    {
        return value.HasValue ? AppendYear(value.Value) : this;
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

## ネーミング

- 変数名などだけでなく、URL(Controller/Action)、DBの項目名等も共通の名称を使用する
- 単語を省略した名称は使わない
- 翻訳サイト等を使って日本語名から英語名を決める場合、ニュアンスの誤った単語を採用してしまうケースが多いことに注意する
- クラス名、メソッド名には処理に応じたプリフィクス/サフィックスをつけてルールを統一する
- Web API用のデータ構造はXxxRequest/XxxResponse等
- データアクセスについては(Query|Insert|Update|Delete|Merge)Xxx(List)(ForYyy|ByYyy)等
- bool型の変数は意味に応じた接頭語をつける(HasXxx、UseXxx、IsXxx等)

一般的にプログラムでよく利用されている名称を使用する、以下のようなサイトも参照。  
https://codic.jp/

.NETの一般的な名前付きのガイドラインについては以下を参照。  
https://docs.microsoft.com/ja-jp/dotnet/standard/design-guidelines/naming-guidelines

## 言語仕様

.NETやプログラムの基礎について記述する。

### スコープ

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

### ガード句

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

### 文字列

- フォーマットには文字列補間を使用する  

https://docs.microsoft.com/ja-jp/dotnet/csharp/language-reference/tokens/interpolated

- リテラル同士の+演算子による結合は、コンパイル時に単一リテラルに変換されるので使用して問題ない
- 逆に非リテラルの+演算子による結合は、都度コピーが発生するので使用せず、StringBuilderを使用し、サイズ上限が確定できる場合には初期サイズを明示する

上記を踏まえた正しい記述は以下になる。

#### パターン1

```csharp
// ×
var sql = new StringBuilder();
sql.Append("SELECT * FROM Xxx ");
sql.Append("WHERE Id = @Id");

// ○
var sql = "SELECT * FROM Xxx " +
          "WHERE Id = @Id";
```

#### パターン2

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

### コレクション

- 変数/引数/戻り値について、IEnumerableで扱うべきものなか、それとも配列やListのようにマテリアライズした形で扱うべきものなのかを正しく処理する
- IEnumerableで扱うということは、それを使用する側としては評価が確定していないコレクションに対する処理だということを理解する
- 静的チェックツールの設定で、同じIEnumerableに対する繰り返しの処理は警告にしているが、これはIEnumerableの実態が遅延評価のDBアクセスのようなものの場合、マテリアライズのタイミングで都度別のクエリが実行されることになるという点を理解する
- 端末アプリケーションではあまり内処理かもしれないが、DBからの大量データをファイルに出力するような処理の場合、一覧をマテリアライズしてメモリを圧迫するのではなく、IEnumerableによるイテレーターでの処理で実装する
- LINQの記述はクエリ構文は使用せずメソッド構文で統一する

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

- LINQは遅延評価であることと、ToList()等によるマテリアライズのタイミングを正しく理解して使用する
- 上記までを理解した上で、必要なケースでは一端中間データをマテリアライズし、その結果を繰り返し使用するのが適切なケースがあることも理解する
- LINQの遅延評価に関しては静的チェックツールでも警告が出力されるようにしているが、Count() > 0ではなくAny()を使うべき意味を理解しておく
- List<T>.ForEach()は使わずforeachを使用する

### コメント

- ASCIIコードにある値は全角ではなく半角を使用する(A-Z、0-9、()等)
- カナは半角カナを使用しない
- コードと1:1になるようなコメントを記述しない

* 有名な格言

```
コードには How
テストコードには What
コミットログには Why
コードコメントには Why not
```
