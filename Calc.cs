using drawer.Models;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace drawer;

internal class Calc
{
    readonly static double[] Neg8 = [1.0, -1.0, 1.0, -1.0, 1.0, -1.0, 1.0, -1.0];
    // public static Vector<double> NegY = new([1, -1, 1, -1]);

    /** Преобразование в систему координат экрана */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Translate(double[] cs, DrawProperties1 pr)
    {
        var count = cs.Length - cs.Length % 4;
        #region
        //var count8 = cs.Length > 8
        //    ? cs.Length / 8 // 8 элементов = 4 координаты
        //    : 0;

        //var count4 = (cs.Length - count8 * 8) / 4;

        //fixed (double* pointer = &cs[0], neg8P = &Neg8[0])
        //{
        //    if (count8 > 0)
        //    {
        //        var leftTop8 = Vector512.Create(pr.LeftTop[0], pr.LeftTop[1], pr.LeftTop[0], pr.LeftTop[1], pr.LeftTop[0], pr.LeftTop[1], pr.LeftTop[0], pr.LeftTop[1]);
        //        var neg8 = Vector512.Load(neg8P);
        //        var scale8 = Vector512.Create(pr.Scale);


        //        for (var i = 0; i < count8; i++)
        //        {
        //            var v = Vector512.Load(pointer);

        //            // Обработка 8 элементов (4 координаты)
        //            // v = [x0, y0, x1, y1, x2, y2, x3, y3]
        //            // leftTop = [l, t, l, t, l, t, l, t]

        //            var result = (v - leftTop8) * neg8 * scale8;
        //            result.Store(pointer);
        //            *pointer += 8;
        //        }
        //    }

        //    if (count4 > 0)
        //    {
        //        var neg = Vector256.Load(neg8P);
        //        var leftTop4 = Vector256.Create(pr.LeftTop[0], pr.LeftTop[1], pr.LeftTop[0], pr.LeftTop[1]);

        //        for (var i = 0; i < count4; i++)
        //        {
        //            var v = Vector256.Load(pointer);
        //            var result = (v - leftTop4) * neg * pr.Scale;
        //            result.Store(pointer);
        //            *pointer += 4;
        //        }
        //    }
        //}
        #endregion


        for (var i = 0; i < count; i += 4)
        {
            var v = new Vector<double>(cs, i);
            var result = (v - pr.LeftTop) * pr.Scale;
            cs[i] = result[0];
            cs[i + 1] = - result[1];
            cs[i + 2] = result[2];
            cs[i + 3] = - result[3];
        }

        if (count >= cs.Length) return;
        
        cs[^2] = (cs[^2] - pr.LeftTop[0]) * pr.Scale;
        cs[^1] = (pr.LeftTop[1] - cs[^1]) * pr.Scale;

    }

    /** Удаление точек которые не будут отображаться */
    public static double[] Optimize(double[] mas, double l = 1)
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

        return [.. coords];
    }

    //public static double[] OptimizeNotOptimize(double[] mas, double l = 1)
    //{
    //    var count = mas.Length;
    //    if (count < 5) return mas;

    //    var coords = new List<double>(mas.Length);

    //    var (lastCoordX1, lastCoordY1) = (mas[0], mas[1]);
    //    var (lastCoordX2, lastCoordY2) = (mas[2], mas[3]);

    //    coords.Add(lastCoordX1);
    //    coords.Add(lastCoordY1);

    //    for (var i = 4; i < count; i += 2)
    //        if (!IsPointOnLine1(lastCoordX1, lastCoordY1, lastCoordX2, lastCoordY2, mas[i], mas[i + 1], l))
    //        {
    //            (lastCoordX1, lastCoordY1) = (mas[i - 2], mas[i - 1]);
    //            (lastCoordX2, lastCoordY2) = (mas[i], mas[i + 1]);

    //            coords.Add(lastCoordX1);
    //            coords.Add(lastCoordY1);
    //        }

    //    coords.Add(mas[count - 2]);
    //    coords.Add(mas[count - 1]);

    //    return [.. coords];
    //}

    // public static bool IsPointOnLineNotOptimize(double pX1, double pY1, double pX2, double pY2, double pX, double pY, double l)
    // {
    //     var a = pX - pX1;
    //     var b = pY - pY1;

    //     var c = pX2 - pX1;
    //     var d = pY2 - pY1;

    //     var lenSQ = c * c + d * d;

    //     var param = (lenSQ != 0)
    //         ? (a * c + b * d) / lenSQ
    //         : -1.0;

    //     double xx, yy;

    //     if (param < 0)
    //         (xx, yy) = (pX1, pY1);
    //     else if (param > 1)
    //         (xx, yy) = (pX2, pY2);
    //     else
    //         (xx, yy) = (pX1 + param * c, pY1 + param * d);

    //     var (dx, dy) = (pX - xx, pY - yy);

    //     return Math.Sqrt(dx * dx + dy * dy) < l;
    // }

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
