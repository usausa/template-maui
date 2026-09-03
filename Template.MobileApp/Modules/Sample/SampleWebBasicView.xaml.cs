namespace Template.MobileApp.Modules.Sample;

using System.ComponentModel;

using CommunityToolkit.Maui.Extensions;

[View(ViewId.SampleWebBasic)]
public sealed partial class SampleWebBasicView
{
    private SampleWebBasicViewModel? viewModel;

    public SampleWebBasicView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (viewModel is not null)
        {
            viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        viewModel = BindingContext as SampleWebBasicViewModel;

        if (viewModel is not null)
        {
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SampleWebBasicViewModel.Result))
        {
            return;
        }

        // 受信を一瞬ハイライトして通知する
        Dispatcher.Dispatch(async () =>
        {
            var resources = Application.Current!.Resources;
            StatusBorder.BackgroundColor = resources.FindResource<Color>("SelectFocusColor");
            await StatusBorder.BackgroundColorTo(resources.FindResource<Color>("GrayLighten3"), 16, 600, Easing.CubicOut);
        });
    }
}
