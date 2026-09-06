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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<double> Optimize(this double[] mas, double l)
    {
        int count = mas.Length;


        if (count < 5)
            return mas;
        

        ReadOnlySpan<double> sp = mas.AsSpan();

        double[] coordsArray = GC.AllocateUninitializedArray<double>(sp.Length);

        Span<double> coords = coordsArray.AsSpan();

        // lastCoord1 применялось только для загрузки vP1 - поэтому его можно не хранить в отдельной переменной а загружать в vP1 из sp.Slice(0, 2) в цикле
        //ReadOnlySpan<double> lastCoord1 = sp.Slice(0, 2);
        // lastCoord2 применялось только для загрузки vP2 - поэтому его можно не хранить в отдельной переменной а загружать в vP2 из sp.Slice(i, 2) в цикле
        //var lastCoord2 = sp.Slice(2, 2);

        double lSq = l * l;

        Vector128<double> vP1 = Vector128.Create(sp.Slice(0, 2));
        Vector128<double> vP2 = Vector128.Create(sp.Slice(2, 2));

        int index = 0;
        vP1.StoreUnsafe(ref MemoryMarshal.GetReference(coords.Slice(index, 2)));
        index += 2;

        // сохраняем в предыдущую позицию (sp.Slice(i - 2, 2)) в vP для первичной загрузки в vPrevious
        Vector128<double> vP = vP2;

        // для первой итерации выносим из цикла вычичисление cd и lenSQ так как они вычисляются из предварительно загруженных vP1 и vP2 из lastCoord1 и lastCoord2
        Vector128<double> cd = vP2 - vP1;
        double lenSQ = Vector128.Sum(cd * cd); //var lenSQ = cd[0] * cd[0] + cd[1] * cd[1];

        for (int i = 4; i < (uint)sp.Length; i += 2)
        {
            // сохраняем в предыдущую позицию vP
            Vector128<double> vPrevious = vP;
            vP = Vector128.Create(sp.Slice(i, 2));

            if (!IsPointOnLine(vP1, vP2, vP, lSq, cd, lenSQ))
            {
                //lastCoord1 = sp.Slice(i - 2, 2);
                //lastCoord2 = sp.Slice(i, 2);
                // так как lastCoord1 и lastCoord2 в цикле меняются только здесь то vP1 и vP2 можно загружать только здесь
                //vP1 = Vector128.Create(lastCoord1);
                // так как vP1 всегда загружается из предыдущей позиции то для предыдущей позиции заводим переменную vPrevious и будем загружать vP1 из vPrevious
                vP1 = vPrevious;

                // так как текущая позиция vP2 (sp.Slice(i, 2)) совпадает с vP то просто копируем vP в vP2 без загрузки из sp.Slice(i, 2)
                //vP2 = Vector128.Create(sp.Slice(i, 2));
                vP2 = vP;

                //в lastCoord1 содержимое vP1 поэтому можно в coords записать vP1
                //lastCoord1.CopyTo(coords.AsSpan(index));
                vP1.StoreUnsafe(ref MemoryMarshal.GetReference(coords.Slice(index, 2)));
                index += 2;

                // так как vP2 и vP1 в цикле меняются только здесь то cd и lenSQ можно вычислять только здесь
                cd = vP2 - vP1;
                lenSQ = Vector128.Sum(cd * cd); //var lenSQ = cd[0] * cd[0] + cd[1] * cd[1];
            }
        }

        sp.Slice(sp.Length - 2).CopyTo(coords.Slice(index, 2));
        index += 2;

        return new Memory<double>(coordsArray, 0, index);
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
    public static bool IsPointOnLine(Vector128<double> vP1, Vector128<double> vP2, Vector128<double> vP, double lSq,
        Vector128<double> cd, double lenSQ)
    {
        // Вычисляем ближайшую точку на линии
        Vector128<double> xy;
        if (lenSQ == 0)
            xy = vP1;
        else
        {
            Vector128<double> ab = vP - vP1;

            double param = Vector128.Sum(ab * cd); // var param = ab[0] * cd[0] + ab[1] * cd[1];
            if (param < 0)
                xy = vP1;
            else if (param > 1)
                xy = vP2;
            else
                xy = Vector128.MultiplyAddEstimate(cd, Vector128.CreateScalar(param), vP1); //xy = vP1 + cd * param;
        }

        Vector128<double> dP = vP - xy;

        return Vector128.Sum(dP * dP) < lSq; //return dP[0] * dP[0] + dP[1] * dP[1] < l;
    }
}
