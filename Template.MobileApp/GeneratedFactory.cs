// ライブラリ内部で登録される型は生成ファクトリの対象外になるため、明示的に生成させる
// Types registered inside a library are out of scope for factory generation, so they are requested explicitly.
// provider.DescribeRuntimeFallbacks() の出力を貼り付けたもの
// Pasted from the output of provider.DescribeRuntimeFallbacks().
#pragma warning disable IDE0001
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::Shiny.AndroidPlatform))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::Shiny.BluetoothLE.BleManager))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::Shiny.BluetoothLE.Hosting.BleHostingManager))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::Shiny.BluetoothLE.Intrastructure.SemaphoreOperationQueue))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::Template.MobileApp.App))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::Template.MobileApp.Extender.PopupFocusPlugin))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::CommunityToolkit.Maui.Services.PopupService))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::MauiComponents.DefaultPopupFactory))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::MauiComponents.DialogImplementation))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::MauiComponents.DisplayImplementation))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::MauiComponents.FileLoggerProvider))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::MauiComponents.LocationService))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::MauiComponents.PopupNavigator))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::MauiComponents.ScreenImplementation))]
[assembly: global::BunnyTail.DependencyInjection.GenerateComponentFactory(typeof(global::MauiComponents.SpeechService))]
