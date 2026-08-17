//using BenchmarkDotNet.Disassemblers;
using apiTest.Arena;
using drawer.Models;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace drawer;

public static class ListAdapter<T>
{
    private static readonly FieldInfo _arrayField = typeof(List<T>)
        .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
        .Single(x => x.FieldType == typeof(T[]));

    public static Memory<T> ToMemory(List<T> list)
    {
        T[] array = (T[])_arrayField.GetValue(list);
        return array.AsMemory(0, list.Count);
    }
}

internal static class Calc
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<double> Translate(this Memory<double> mas,ref DrawProperties1 pr)
    {
        
        var count = mas.Length - mas.Length % 4;
        var scale = pr.Scale;
        var cs = mas.Span;

        var vecArray = MemoryMarshal.Cast<double, Vector<double>>(cs[..count]);
        for (var i = 0; i < vecArray.Length; i++)
        
            vecArray[i] = (vecArray[i] - pr.LeftTop) * scale;

        if (count >= mas.Length) return mas;

        cs[^2] = (cs[^2] - pr.LeftTop[0]) * scale[0];
        cs[^1] = (pr.LeftTop[1] - cs[^1]) * scale[0];

        return mas;
    }

    /** Удаление точек которые не будут отображаться */
    public static Memory<double> Optimize(this double[] mas, double l)
    {
        var count = mas.Length;
        if (count < 5) return mas;

        var coords = new List<double>(mas.Length);

        var sp = mas.AsSpan();

        var lastCoord1 = sp[..2];
        var lastCoord2 = sp.Slice(2, 2);

        coords.AddRange(lastCoord1);
        var lSq = l * l;

        for (var i = 4; i < count; i += 2)
            if (!IsPointOnLine(lastCoord1, lastCoord2, sp.Slice(i, 2), lSq))
            {
                lastCoord1 = sp.Slice(i - 2, 2);
                lastCoord2 = sp.Slice(i, 2);

                coords.AddRange(lastCoord1);
            }

        coords.AddRange(sp.Slice(count - 2, 2));

        return ListAdapter<double>.ToMemory(coords);
    }


    /** Удаление точек которые не будут отображаться */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<double> Optimize(this Memory<double> mas, ArenaAllocator<double> allocator, double l)
    {
        if (mas.Length < 5) return mas;

        var result = allocator.Alloc(mas.Length);

        var sp = mas.Span;
        var coords = result.Span;

        var lastCoord1 = sp[..2];
        var lastCoord2 = sp.Slice(2, 2);

        var index = 0;
        coords[index++] = lastCoord1[0];
        coords[index++] = lastCoord1[1];
        var lSq = l * l;

        for (var i = 4; i < mas.Length; i += 2)
            if (!IsPointOnLine(lastCoord1, lastCoord2, sp.Slice(i, 2), lSq))
            {
                lastCoord1 = sp.Slice(i - 2, 2);
                lastCoord2 = sp.Slice(i, 2);

                coords[index++] = lastCoord1[0];
                coords[index++] = lastCoord1[1];
            }

        coords[index++] = sp[^2];
        coords[index++] = sp[^1];


        return result[..index];
    }


    /** Находится ли следующая точка на линии с определённым допуском */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static bool IsPointOnLine(ReadOnlySpan<double> p1, ReadOnlySpan<double> p2, ReadOnlySpan<double> p, double l)
    {
        fixed (double* P = p, P1 = p1, P2 = p2)
        {
            var vP = Vector128.Load(P);
            var vP1 = Vector128.Load(P1);
            var vP2 = Vector128.Load(P2);

            // ab = p - p1
            var ab = Sse41.Subtract(vP, vP1);
            // cd = p2 - p1
            var cd = Sse41.Subtract(vP2, vP1);

            // lenSQ = c*c + d*d (используем dot product с маской 255 для суммирования всех элементов)
            var lenSQ = Sse41.DotProduct(cd, cd, 255)[0];

            var param = (lenSQ != 0)
                ? Sse41.DotProduct(ab, cd, 255)[0] / lenSQ
                : -1.0d;

            // Вычисляем ближайшую точку на линии
            Vector128<double> xy;
            if (param < 0)
                xy = vP1;
            else if (param > 1)
                xy = vP2;
            else
                xy = Sse41.Add(vP1, Sse41.Multiply(cd, Vector128.Create(param)));

            var dP = Sse41.Subtract(vP, xy);

            return Sse41.DotProduct(dP, dP, 255)[0] < l;
        }
    }
}
