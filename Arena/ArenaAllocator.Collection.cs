using System.Runtime.CompilerServices;

namespace apiTest.Arena;

public struct ArenaList<T>(Memory<T> items, ArenaAllocator<T> arena)
{
    //private Memory<T> Items { get; set; } = items;
    public int Count { get; set; }
    //private ArenaAllocator<T> Arena { get; set; } = arena;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> AsSpan() => items.Span[..Count];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<T> item)
    {
        var count = Count + item.Length;
        if (count <= items.Length)
            items = arena.Alloc(count + 256);


        item.CopyTo(items.Span[Count..]);
        
        Count = count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(T item)
    {
        var count = Count + 1;
        if (count > items.Length)
            items = arena.Alloc(count + 256);

        items.Span[Count] = item;

        Count = count;
    }
}

public partial class ArenaAllocator<T>
{
    public ArenaList<T> AllocList(int copacity) => new(Alloc(copacity), this);

}
