using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace apiTest.Arena;

/** Упрощённая запись double в строку */
public class BufferStringConverter : JsonConverter<BufferString>
{
    public override void Write(Utf8JsonWriter writer, BufferString value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.AsSpan());
    }

    public override BufferString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Изменяемая строка похожая на StringBuilder но на основе ArenaAllocator /// 
/// </summary>
[JsonConverter(typeof(BufferStringConverter))]
public struct BufferString
{
    private char[] _items;
    public char[] Items { readonly get => _items; private set => _items = value; }

    private readonly ArenaAllocator<char> Allocator;

    public int Count { get; private set; }

    private int _start;
    public int Start { readonly get => _start; private set => _start = value; }
    private int _copacity;

    public BufferString(ArenaAllocator<char> allocator, int copacity = 32)
    {
        _copacity = copacity;
        Allocator = allocator;
        allocator.Alloc(copacity, out _items, out _start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<char> AsSpan() => _items.AsSpan(_start, Count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(in ReadOnlySpan<char> item)
    {
        var count = Count + item.Length;
        if (count > _copacity) EnsureCapacity(count);

        item.CopyTo(_items.AsSpan(_start + Count, item.Length));

        Count = count;
        // Добавляем последний символ 
        _items[_start + Count] = '\0';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char item)
    {
        var count = Count + 1;
        if (count > Items.Length) EnsureCapacity(count);

        _items[_start + Count] = item;

        Count = count;
        // Добавляем последний символ
        _items[_start + Count] = '\0';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append<T>(T value, scoped ReadOnlySpan<char> format = default, int bufferSize = 36, IFormatProvider? formatProvider = null) where T : ISpanFormattable
    {   
        var sp = _items.AsSpan(_start + Count, _copacity - Count);

        if (!value.TryFormat(sp, out var charsWritten, format, formatProvider))
            throw new InvalidOperationException($"Не удалось вставить {value} в указанный буфер. Буфер размера: {bufferSize}) не достаточно");

        Count += charsWritten;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int copacity)
    {
        _copacity = copacity * 2 + 2;

        var oldarray = _items;
        var oldStart = _start;

        Allocator.Alloc(_copacity, out _items, out _start);

        oldarray.AsSpan(oldStart, Count).CopyTo(AsSpan());
    }

    public override readonly string ToString() => AsSpan().ToString();

}
