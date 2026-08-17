using mimalloc;
using System.Runtime.CompilerServices;

namespace apiTest.Arena;

/** Динамическая строка */
public struct BufferString(ArenaAllocator<char> allocator, int copacity = 32)
{
    private static readonly char[] _singleDigitCharCache = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

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
        ReAlloc(count);            

        Items.Span[count] = item;

        Count = count;
        Items.Span[Count] = '\0';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(uint number)
    {
        var numberLength = CountDigits(number);

        Count += numberLength;

        ReAlloc(Count);

        var sp = AsSpan();

        if (numberLength == 1)
            sp[^1] = _singleDigitCharCache[number];
        else
            UInt32ToDecChars(sp, number);

        Items.Span[Count] = '\0';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UInt32ToDecChars(Span<char> sp, uint value)
    {
        var count = sp.Length;

        do
        {
            var quotient = value / 10;
            (value, var remainder) = (quotient, value % 10);

            sp[--count] = (char)(remainder + '0');
        }
        while (value != 0);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CountDigits(uint value)
    {
        var digits = 1;
        while (value >= 100_000)
        {
            value /= 100_000;
            digits += 5;
        }

        return value switch
        {
            < 10 => digits,
            < 100 => digits + 1,
            < 1000 => digits + 2,
            < 10_000 => digits + 3,
            _ => digits + 4
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReAlloc(int copacity)
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



//public partial class ArenaAllocator<T>
//{
//    public BufferString AllocString(int copacity)
//    {
//        new(Alloc(copacity) as Memory<char>);

//}