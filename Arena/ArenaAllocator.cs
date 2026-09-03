using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace apiTest.Arena;

public partial class ArenaAllocator<T>() : IDisposable
{
    private static readonly ConcurrentBag<ArenaAllocator<T>> pools = [];

    private T[] _buffer = GC.AllocateUninitializedArray<T>(256);

    private int _count = 0;

    /** Получение Аллокатора с предполагаемой размерностью */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArenaAllocator<T> Get()
    {
        if (!pools.TryTake(out var result)) result = new ArenaAllocator<T>();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<T> Alloc(int length)
    {
        var newCount = _count + length;
        if (newCount > _buffer.Length)
            _buffer = GC.AllocateUninitializedArray<T>(GrowCap(_buffer.Length, newCount));

        var start = _count;
        _count = newCount;

        return new Memory<T>(_buffer, start, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Alloc(int length, out T[] array, out int start)
    {
        var newCount = _count + length;
        if (newCount > _buffer.Length)
            _buffer = GC.AllocateUninitializedArray<T>(GrowCap(_buffer.Length, newCount));

        start = _count;
        array = _buffer;
        _count = newCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GrowCap(int oldCap, int need)
    {
        const int minGrow = 256;

        var newCap = Math.Max(oldCap, minGrow);

        while (newCap < need) newCap *= 2;
        return newCap;
    }


    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        _count = 0;

        //while (_used.TryDequeue(out var result)) _availablе.Enqueue(result);
        pools.Add(this);

    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public void Clear() { _count = 0; }
}
