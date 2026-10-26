namespace Template.MobileApp.Behaviors;

using System.Runtime.CompilerServices;

public static class DropOption
{
    //--------------------------------------------------------------------------------
    // SideCommand
    //--------------------------------------------------------------------------------

    private static readonly ConditionalWeakTable<View, SideWatcher> Watchers = [];

    public static readonly BindableProperty SideCommandProperty = BindableProperty.CreateAttached(
        "SideCommand",
        typeof(ICommand),
        typeof(DropOption),
        null,
        propertyChanged: OnSideCommandChanged);

    public static ICommand? GetSideCommand(BindableObject bindable) => (ICommand?)bindable.GetValue(SideCommandProperty);

    public static void SetSideCommand(BindableObject bindable, ICommand? value) => bindable.SetValue(SideCommandProperty, value);

    private static void OnSideCommandChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is not View view)
        {
            return;
        }

        if (Watchers.TryGetValue(view, out var watcher))
        {
            watcher.Detach();
            Watchers.Remove(view);
        }

        if (newValue is not null)
        {
            Watchers.Add(view, new SideWatcher(view));
        }
    }

    private sealed class SideWatcher
    {
        private readonly View owner;

        private DropGestureRecognizer? recognizer;

        public SideWatcher(View owner)
        {
            this.owner = owner;
            owner.Loaded += HandleLoaded;
            owner.Unloaded += HandleUnloaded;

            if (owner.IsLoaded)
            {
                Attach();
            }
        }

        public void Detach()
        {
            owner.Loaded -= HandleLoaded;
            owner.Unloaded -= HandleUnloaded;
            Release();
        }

        private void HandleLoaded(object? sender, EventArgs e) => Attach();

        private void HandleUnloaded(object? sender, EventArgs e) => Release();

        private void Attach()
        {
            if (recognizer is not null)
            {
                return;
            }

            recognizer = owner.GestureRecognizers.FirstOrDefault(static x => x is DropGestureRecognizer) as DropGestureRecognizer;
            if (recognizer is not null)
            {
                recognizer.DragOver += HandleDragOver;
            }
        }

        private void Release()
        {
            if (recognizer is not null)
            {
                recognizer.DragOver -= HandleDragOver;
                recognizer = null;
            }
        }

        private void HandleDragOver(object? sender, DragEventArgs e)
        {
            if ((GetSideCommand(owner) is not { } command) ||
                (owner.Height <= 0) ||
                (e.GetPosition(owner) is not { } position))
            {
                return;
            }

            var after = position.Y > (owner.Height / 2);
            if (command.CanExecute(after))
            {
                command.Execute(after);
            }
        }
    }
}
