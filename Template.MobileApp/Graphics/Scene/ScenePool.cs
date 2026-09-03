namespace Template.MobileApp.Graphics.Scene;

// 毎フレームの生成/破棄による GC 負荷 (ポーズ) を避けるための単純な再利用プール
// (Breakout の ReusableSpritePool 相当。パーティクル等の短命オブジェクトに使う)
public sealed class ScenePool<T>
    where T : class
{
    private readonly Func<T> factory;

    private readonly Stack<T> pool = new();

    // 実際に new された数 (プールが効いていれば伸びない)
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
