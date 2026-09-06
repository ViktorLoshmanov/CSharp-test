using drawer.Models;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace drawer;

internal static class Polygon
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNextIndex(int curIndex, int len)
    {
        curIndex += 2;
        if (curIndex >= len) curIndex = 0;
        return curIndex;
    }

    private static double[] ClipLeft(double[] coords, double left, List<double> pl)
    {
        if (coords.Length == 0) return coords;

        CollectionsMarshal.SetCount(pl, 0);

        var curIndex = 0;

        var (px1, py1) = (coords[0], coords[1]);

        if (px1 >= left)
        {
            pl.AddRange(px1,
                py1);
        }

        var len = coords.Length / 2;
        for (var i = 1; i <= len; i++)
        {
            curIndex = GetNextIndex(curIndex, coords.Length);
            var (px2, py2) = (coords[curIndex], coords[curIndex + 1]);

            if (px1 >= left && px2 >= left)
            {
                pl.AddRange(px2,
                    py2);
            }
            else if (px1 < left && px2 > left)
            {
                pl.AddRange(left,
                    (left - px1) * (py2 - py1) / (px2 - px1) + py1,

                    px2,
                    py2);
            }
            else if (px1 > left && px2 < left)
            {
                pl.AddRange(left,
                    (left - px1) * (py2 - py1) / (px2 - px1) + py1);
            }
            (px1, py1) = (px2, py2);

        }

        return pl.ToArray();
    }
    private static double[] ClipRight(double[] coords, double right, List<double> pl)
    {
        if (coords.Length == 0) return coords;

        CollectionsMarshal.SetCount(pl, 0);

        var curIndex = 0;

        var (px1, py1) = (coords[0], coords[1]);

        if (px1 <= right)
        {
            pl.AddRange(px1,
                py1);
        }
        var len = coords.Length / 2;

        for (var i = 0; i < len; i++)
        {
            curIndex = GetNextIndex(curIndex, coords.Length);

            var px2 = coords[curIndex];
            var py2 = coords[curIndex + 1];

            if (px1 <= right && px2 <= right)
            {
                pl.AddRange(px2,
                    py2);
            }
            else if (px1 > right && px2 < right)
            {
                pl.AddRange(right,
                    (right - px1) * (py2 - py1) / (px2 - px1) + py1,

                    px2,
                    py2);
            }
            else if (px1 < right && px2 > right)
            {
                pl.AddRange(right,
                    (right - px1) * (py2 - py1) / (px2 - px1) + py1);
            }
            (px1, py1) = (px2, py2);
        }

        return pl.ToArray();
    }
    private static double[] ClipBottom(double[] coords, double bottom, List<double> pl)
    {
        if (coords.Length == 0) return coords;

        CollectionsMarshal.SetCount(pl, 0);

        var curIndex = 0;

        var (px1, py1) = (coords[0], coords[1]);

        if (py1 >= bottom)
        {
            pl.AddRange(px1,
                py1);
        }

        var len = coords.Length / 2;
        for (var i = 0; i < len; i++)
        {
            curIndex = GetNextIndex(curIndex, coords.Length);
            var (px2, py2) = (coords[curIndex], coords[curIndex + 1]);

            if (py1 >= bottom && py2 >= bottom)
            {
                pl.AddRange(px2,
                    py2);
            }
            else if (py1 < bottom && py2 > bottom)
            {
                pl.AddRange((bottom - py1) * (px2 - px1) / (py2 - py1) + px1,
                    bottom,

                    px2,
                    py2);
            }
            else if (py1 > bottom && py2 < bottom)
            {
                pl.AddRange((bottom - py1) * (px2 - px1) / (py2 - py1) + px1,
                    bottom);
            }
            (px1, py1) = (px2, py2);
        }

        return pl.ToArray();
    }
    private static double[] ClipTop(double[] coords, double top, List<double> pl)
    {
        if (coords.Length == 0) return coords;

        CollectionsMarshal.SetCount(pl, 0);

        var curIndex = 0;

        var (px1, py1) = (coords[0], coords[1]);

        if (py1 <= top)
        {
            pl.AddRange(px1,
                py1);
        }

        var len = coords.Length / 2;
        for (var i = 0; i < len; i++)
        {
            curIndex = GetNextIndex(curIndex, coords.Length);
            var (px2, py2) = (coords[curIndex], coords[curIndex + 1]);

            if (py1 <= top && py2 <= top)
            {
                pl.AddRange(px2,
                    py2);
            }
            else if (py1 > top && py2 < top)
            {
                pl.AddRange((top - py1) * (px2 - px1) / (py2 - py1) + px1,
                    top,

                    px2,
                    py2);
            }
            else if (py1 < top && py2 > top)
            {
                pl.AddRange((top - py1) * (px2 - px1) / (py2 - py1) + px1,
                    top);
            }

            (px1, py1) = (px2, py2);
        }

        return pl.ToArray();
    }

    /** Отсечение полигона по прямоугольнику */
    public static double[] ClipPolygon(Primitive g, Rect rect, List<double> pl)
    {
        var res = (g.Rect.Left < rect.Left)
            ? ClipLeft(g.Coords, rect.Left, pl)
            : [.. g.Coords];

        if (g.Rect.Bottom < rect.Bottom)
            res = ClipBottom(res, rect.Bottom, pl);

        if (g.Rect.Right > rect.Right)
            res = ClipRight(res, rect.Right, pl);

        if (g.Rect.Top > rect.Top)
            res = ClipTop(res, rect.Top, pl);

        return res;
    }
}
