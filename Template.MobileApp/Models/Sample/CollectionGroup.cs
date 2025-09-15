namespace Template.MobileApp.Models.Sample;

using System.Collections.Specialized;

public class CollectionGroup<TKey, TItem> : IReadOnlyList<TItem>, INotifyPropertyChanged, INotifyCollectionChanged
{
    // ReSharper disable StaticMemberInGenericType
    private static readonly PropertyChangedEventArgs IsExpandedChangedEventArgs = new(nameof(IsExpanded));

    private static readonly NotifyCollectionChangedEventArgs ResetEventArgs = new(NotifyCollectionChangedAction.Reset);
    // ReSharper restore StaticMemberInGenericType

    public event PropertyChangedEventHandler? PropertyChanged;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private readonly List<TItem> sourceItems;

    private List<TItem> displayItems;

    private bool isExpanded;

    public TKey Key { get; }

    public IReadOnlyList<TItem> SourceItems => sourceItems.AsReadOnly();

    public int SourceCount => sourceItems.Count;

    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded != value)
            {
                isExpanded = value;

                displayItems = isExpanded ? sourceItems : [];

                PropertyChanged?.Invoke(this, IsExpandedChangedEventArgs);
                CollectionChanged?.Invoke(this, ResetEventArgs);
            }
        }
    }

    public int Count => displayItems.Count;

    public TItem this[int index] => displayItems[index];

    public CollectionGroup(TKey key)
    {
        Key = key;
        sourceItems = [];
        displayItems = [];
    }

    public CollectionGroup(TKey key, IEnumerable<TItem> items, bool isExpanded = true)
    {
        Key = key;
        this.isExpanded = isExpanded;
        sourceItems = items.ToList();
        displayItems = isExpanded ? sourceItems : [];
    }

    public IEnumerator<TItem> GetEnumerator() => displayItems.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => displayItems.GetEnumerator();

    // ------------------------------------------------------------
    // Source
    // ------------------------------------------------------------

    public int IndexOfSource(TItem item) => sourceItems.IndexOf(item);

    public void ClearSource()
    {
        sourceItems.Clear();
        if (isExpanded)
        {
            CollectionChanged?.Invoke(this, ResetEventArgs);
        }
    }

    public void AddToSource(TItem item)
    {
        sourceItems.Add(item);
        if (isExpanded)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, sourceItems.Count - 1));
        }
    }

    public void AddRangeToSource(IEnumerable<TItem> items)
    {
        var list = items.ToList();
        if (list.Count == 0)
        {
            return;
        }

        var index = sourceItems.Count;
        sourceItems.AddRange(list);
        if (isExpanded)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list, index));
        }
    }

    public void InsertToSource(int index, TItem item)
    {
        sourceItems.Insert(index, item);
        if (isExpanded)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }
    }

    public void ReplaceSource(TItem oldItem, TItem newItem)
    {
        var index = sourceItems.IndexOf(oldItem);
        if (index < 0)
        {
            return;
        }

        sourceItems[index] = newItem;
        if (isExpanded)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }
    }

    public bool RemoveFromSource(TItem item)
    {
        var index = sourceItems.IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        sourceItems.RemoveAt(index);
        if (isExpanded)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        return true;
    }

    public bool RemoveAtSource(int index)
    {
        var item = sourceItems[index];

        sourceItems.RemoveAt(index);
        if (isExpanded)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        return true;
    }
}
