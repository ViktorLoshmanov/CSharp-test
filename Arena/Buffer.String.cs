//using mimalloc;
using System.Runtime.CompilerServices;

namespace apiTest.Arena;

/** Динамическая строка */
public struct BufferString(ArenaAllocator<char> allocator, int copacity = 32)
{
    private Memory<char> Items { get; set; } = allocator.Alloc(copacity);
    private readonly ArenaAllocator<char> Allocator = allocator;
    public int Count { get; set; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> AsSpan() => Items.Span[..Count];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> item)
    {
        var count = Count + item.Length;
        if (count > Items.Length)
        {

            //throw new Exception("Выход за отведённые размеры");
        }

        item.CopyTo(Items.Span.Slice(Count, item.Length));
        Count = count;
        Items.Span[Count] = '\0';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char item)
    {
        var count = Count + 1;
        EnsureCapacity(count);

        Items.Span[count] = item;

        Count = count;
        Items.Span[Count] = '\0';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append<T>(T value, scoped ReadOnlySpan<char> format = default, int bufferSize = 36, IFormatProvider? formatProvider = null) where T : ISpanFormattable
    {
        var sp = Items.Span[Count..];

        if (!value.TryFormat(sp, out var charsWritten, format, formatProvider))
            throw new InvalidOperationException($"Не удалось вставить {value} в указанный буфер. Буфер размера: {bufferSize}) не достаточно");

        Count += charsWritten;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int copacity)
    {
        if (copacity <= Items.Length) return;

        var newBuffer = Allocator.Alloc(copacity * 2 + 2);

        AsSpan().CopyTo(newBuffer.Span[..Count]);

        Items = newBuffer;
    }

    public override string ToString()
    {
        return AsSpan().ToString();
    }
}
