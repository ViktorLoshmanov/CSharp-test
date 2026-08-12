using BenchmarkDotNet.Disassemblers;
using drawer.Models;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace drawer;

internal static class Calc
{
    /** Преобразование в систему координат экрана */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<double> Translate(this List<double> mas, DrawProperties1 pr)
    {
        var count = mas.Count - mas.Count % 4;
        var scale = pr.Scale;
        var scaleVector = new Vector<double>([scale, -scale, scale, -scale]);
        var cs = CollectionsMarshal.AsSpan(mas);

        for (var i = 0; i < count; i += 4)
        {
            var chank = cs[i..(i + 4)];
            var v = new Vector<double>(chank);
            var result = (v - pr.LeftTop) * scaleVector;
            result.CopyTo(chank);

            //var v = new Vector<double>(cs, i);
            //var result = (v - pr.LeftTop) * pr.Scale;
            //cs[i] = result[0];
            //cs[i + 1] = -result[1];
            //cs[i + 2] = result[2];
            //cs[i + 3] = -result[3];
        }

        if (count >= mas.Count) return mas;

        cs[^2] = (cs[^2] - pr.LeftTop[0]) * scale;
        cs[^1] = (pr.LeftTop[1] - cs[^1]) * scale;

        return mas;
    }

    /** Удаление точек которые не будут отображаться */
    public static List<double> Optimize(this List<double> mas, double l)
    {
        var count = mas.Count;
        if (count < 5) return mas;

        var coords = new List<double>(mas.Count);

        var sp = CollectionsMarshal.AsSpan(mas);

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

        return coords;
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
