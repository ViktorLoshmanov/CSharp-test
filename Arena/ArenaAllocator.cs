using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace apiTest.Arena;

public partial class ArenaAllocator<T> : IDisposable
{
    private static int defaultPoolSize = 1 << 18;
    private static readonly ConcurrentBag<ArenaAllocator<T>> pools = [];

    private readonly T[] _buffer = new T[DefaultPoolSize];
    private int _count = 0;

    public static int DefaultPoolSize { get => defaultPoolSize; set => defaultPoolSize = value; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArenaAllocator<T> Get()
    {
        if (!pools.TryTake(out var result)) result = new ArenaAllocator<T>();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<T> Alloc(int length)
    {
        if (_count + length > _buffer.Length)
            return (new T[length]).AsMemory();

        var start = _count;
        _count += length;
        return _buffer.AsMemory(start, length);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        _count = 0;
        pools.Add(this);
    }
}
