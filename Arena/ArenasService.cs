using drawer.Models;
using System.Collections.Concurrent;

namespace apiTest.Arena;

/** Набор арен */
public class ArenasService : IDisposable
{
    private int Count = 0;
    private readonly IDisposable?[] _arenas = new IDisposable?[10];

    public ArenasService() { }

    public ArenaAllocator<T> Get<T>()
    {
        var result = ArenaAllocator<T>.Get();

        _arenas[Count++] = result;

        return result;
    }

    public void Dispose()
    {
        for (var i = 0; i < Count; ++i)
            _arenas[i]?.Dispose();

        Count = 0;        
    }
}
