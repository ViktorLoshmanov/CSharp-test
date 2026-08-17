using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace apiTest.Arena;

public partial class ArenaAllocator<T> : IDisposable
{
    private static int defaultPoolSize = 1 << 18;
    private static readonly ConcurrentBag<ArenaAllocator<T>> pools = [];

    private T[] _buffer = GC.AllocateUninitializedArray<T>(DefaultPoolSize);

    private Queue<T[]> _availablе = new();
    private Queue<T[]> _used = new();

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
        {
            _used.Enqueue(_buffer);
            _count = 0;

            if (_availablе.TryDequeue(out var buffer))
                _buffer = buffer;
            else
                _buffer = GC.AllocateUninitializedArray<T>(DefaultPoolSize);
        }

        var start = _count;
        _count += length;

        return _buffer.AsMemory(start, length);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        _count = 0;

        while (_used.TryDequeue(out var result)) _availablе.Enqueue(result);
        pools.Add(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() { _count = 0; }
}
