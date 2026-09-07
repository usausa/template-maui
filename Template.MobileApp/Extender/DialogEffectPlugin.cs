namespace Template.MobileApp.Extender;

using Smart.Navigation.Plugins;

using Template.MobileApp.Extender.Effects;
using Template.MobileApp.Modules;

public sealed class DialogEffectPlugin : PluginBase
{
    private readonly HashSet<ViewId> dialogViews;

    public DialogEffectPlugin(IEnumerable<KeyValuePair<ViewId, Type>> views)
    {
#pragma warning disable IDE0028
        dialogViews = views
            .Where(static x => x.Value.IsDefined(typeof(DialogViewAttribute), false))
            .Select(static x => x.Key)
            .ToHashSet();
#pragma warning restore IDE0028
    }

    public override void OnPrepareParameter(IPluginContext pluginContext, INavigationContext navigationContext, INavigationParameterPrepare parameter)
    {
        if (parameter.Effect is not null)
        {
            return;
        }

        if ((navigationContext.ToId is ViewId toId) && dialogViews.Contains(toId))
        {
            parameter.WithEffect(AppEffect.DialogOpen);
            return;
        }

        if ((navigationContext.FromId is ViewId fromId) && dialogViews.Contains(fromId))
        {
            parameter.WithEffect(AppEffect.DialogClose);
        }
    }
}
