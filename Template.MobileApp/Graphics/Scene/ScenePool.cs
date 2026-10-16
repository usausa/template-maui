namespace Template.MobileApp.Graphics.Scene;

public sealed class ScenePool<T>
    where T : class
{
    private readonly Func<T> factory;

    private readonly Stack<T> pool = new();

    public int CreatedCount { get; private set; }

    public ScenePool(Func<T> factory)
    {
        this.factory = factory;
    }

    public T Rent()
    {
        if (pool.TryPop(out var item))
        {
            return item;
        }

        CreatedCount++;
        return factory();
    }

    public void Return(T item) => pool.Push(item);
}
