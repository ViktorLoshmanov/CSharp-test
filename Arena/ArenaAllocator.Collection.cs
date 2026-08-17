using System.Runtime.CompilerServices;

namespace apiTest.Arena;

public struct ArenaList<T>
{
    private Memory<T> _items { get; set; }
    public int Count { get; set; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => _items.Span[..Count];

    public ArenaList(Memory<T> items)
    {
        _items = items;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<T> item)
    {
        var count = Count + item.Length;
        //if (count > items.Length)
        //    //items = ArenaAllocator<T>.Alloc(count);
        //    throw new Exception("Выход за отведённые размеры");

        item.CopyTo(_items.Span[Count..]);
        //items.Span[Count] = '\0';

        Count = count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(T item)
    {
        var count = Count + 1;
        //if (count > items.Length)
        //    //items = ArenaAllocator<T>.Alloc(count);
        //    throw new Exception("Выход за отведённые размеры");

        _items.Span[count] = item;

        Count = count;
    }
}

public partial class ArenaAllocator<T>
{
    public ArenaList<T> AllocList(int copacity) => new(Alloc(copacity));

}
