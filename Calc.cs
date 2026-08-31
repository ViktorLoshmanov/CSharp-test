using apiTest.Arena;
using drawer.Models;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace drawer;

// public static class ListAdapter<T>
// {
//     private static readonly FieldInfo _arrayField = typeof(List<T>)
//         .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
//         .Single(x => x.FieldType == typeof(T[]));

//     public static Memory<T> ToMemory(List<T> list)
//     {
//         T[] array = (T[])_arrayField.GetValue(list);
//         return array.AsMemory(0, list.Count);
//     }
// }

internal static class Calc
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<double> Translate(this Memory<double> mas, ref DrawProperties1 pr)
    {
        var count = mas.Length - mas.Length % 4;
        var scale = pr.ScaleVector;
        var cs = mas.Span;

        var vecArray = MemoryMarshal.Cast<double, Vector<double>>(cs[..count]);

        for (var i = 0; i < vecArray.Length; i++)
            vecArray[i] = (vecArray[i] - pr.LeftTop) * scale;

        if (count >= mas.Length) return mas;

        cs[^2] = (cs[^2] - pr.LeftTop[0]) * scale[0];
        cs[^1] = (cs[^1] - pr.LeftTop[1]) * scale[1];

        //pr.LeftTop.AsVector128<double>

        return mas;
    }

    /** Удаление точек которые не будут отображаться */
    public static Memory<double> Optimize(this double[] mas, double l)
    {
        var count = mas.Length;
        if (count < 5) return mas;

        var coords = GC.AllocateUninitializedArray<double>(mas.Length);

        var sp = mas.AsSpan();

        var lastCoord1 = sp[..2];
        var lastCoord2 = sp.Slice(2, 2);
        var index = 0;

        (coords[index++], coords[index++]) = (lastCoord1[0], lastCoord1[1]);

        var lSq = l * l;

        for (var i = 4; i < count; i += 2)
            if (!IsPointOnLine(lastCoord1, lastCoord2, sp.Slice(i, 2), lSq))
            {
                lastCoord1 = sp.Slice(i - 2, 2);
                lastCoord2 = sp.Slice(i, 2);

                coords[index++] = lastCoord1[0];
                coords[index++] = lastCoord1[1];
            }

        coords[index++] = sp[^2];
        coords[index++] = sp[^1];

        return new Memory<double>(coords, 0, index);
    }


    /** Удаление точек которые не будут отображаться */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Memory<double> OptimizeBlazing(this double[] mas, ArenaAllocator<double> allocator, double l)
    {
        var count = mas.Length;
        if (count < 5)
        {
            var coords = allocator.Alloc(mas.Length);
            mas.CopyTo(coords);
            return coords;
        }

        var result = allocator.Alloc(mas.Length);

        var (index1, index2) = (0, 2);

        var lSq = l * l;

        fixed (double* src = mas, dest = result.Span)
        {
            var p1 = src;
            var p2 = dest;
            *p2 = *p1; p2++;
            *p2 = *(p1 + 1); p2++;

            for (var i = 4; i < mas.Length; i += 2)
            {
                var vP = Vector128.Load(p1 + i);
                var vP1 = Vector128.Load(p1 + index1);
                var vP2 = Vector128.Load(p1 + index2);

                //var ab = Sse41.Subtract(vP, vP1);
                var ab = vP - vP1;

                //var cd = Sse41.Subtract(vP2, vP1);
                var cd = vP2 - vP1;

                // lenSQ = c*c + d*d (используем dot product с маской 255 для суммирования всех элементов)
                //var lenSQ = Sse41.DotProduct(cd, cd, 255)[0];
                var lenSQ = cd[0] * cd[0] + cd[1] * cd[1];

                // Вычисляем ближайшую точку на линии
                Vector128<double> xy;
                if (lenSQ == 0)
                    xy = vP1;
                else
                {
                    //var param = Sse41.DotProduct(ab, cd, 255)[0] / lenSQ;
                    var param = ab[0] * cd[0] + ab[1] * cd[1];
                    if (param < 0)
                        xy = vP1;
                    else if (param > 1)
                        xy = vP2;
                    else
                        //xy = Sse41.Add(vP1, Sse41.Multiply(cd, Vector128.Create(param)));
                        xy = vP1 + cd * param;
                }

                //var dP = Sse41.Subtract(vP, xy);
                var dP = vP - xy;

                //if (Sse41.DotProduct(dP, dP, 255)[0] < l)
                if (dP[0] * dP[0] + dP[1] * dP[1] < l)
                {
                    (index1, index2) = (i - 2, i);


                    *p2 = *(p1 + index1); p2++;
                    *p2 = *(p1 + index1 + 1); p2++;
                }
            }

            *p2 = *(p1 + count - 2); p2++;
            *p2 = *(p1 + count - 1); p2++;

            return result[..(int)((double*)p2 - dest)];
        }
    }


    /** Находится ли следующая точка на линии с определённым допуском */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPointOnLine(ReadOnlySpan<double> p1, ReadOnlySpan<double> p2, ReadOnlySpan<double> p, double l)
    {
        var vP = Vector128.Create(p);
        var vP1 = Vector128.Create(p1);
        var vP2 = Vector128.Create(p2);

        var ab = vP - vP1;

        var cd = vP2 - vP1;

        var lenSQ = cd[0] * cd[0] + cd[1] * cd[1];

        // Вычисляем ближайшую точку на линии
        Vector128<double> xy;
        if (lenSQ == 0)
            xy = vP1;
        else
        {
            var param = ab[0] * cd[0] + ab[1] * cd[1];
            if (param < 0)
                xy = vP1;
            else if (param > 1)
                xy = vP2;
            else
                xy = vP1 + cd * param;
        }

        var dP = vP - xy;

        return dP[0] * dP[0] + dP[1] * dP[1] < l;
    }
}
