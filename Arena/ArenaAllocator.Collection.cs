using System.Runtime.CompilerServices;

namespace apiTest.Arena;

public struct List<T>
{
    private Memory<T> _items { get; set; }
    public int Count { get; set; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => _items.Span[..Count];

    public List(Memory<T> items)
    {
        _items = items;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<T> item)
    {
        var count = Count + item.Length;
        if (count > _items.Length)
            //_items = ArenaAllocator<T>.Alloc(count);
            throw new Exception("Выход за отведённые размеры");

        item.CopyTo(_items.Span[Count..]);
        Count = count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(T item)
    {
        var count = Count + 1;
        if (count > _items.Length)
            //_items = ArenaAllocator<T>.Alloc(count);
            throw new Exception("Выход за отведённые размеры");

        _items.Span[count]=item;

        Count = count;
    }

}

public partial class ArenaAllocator<T>
{


    public List<T> AllocList(int length)
    {
        var buffer = Alloc(length);
        buffer.Span.Clear();

        return new List<T>(buffer);
    }
}
