using drawer.Models;
using System.Runtime.InteropServices;

namespace drawer;

public static class Polyline
{
    private static List<double[]> ClipLeft(double[] coords, double left, List<double> pl)
    {
        var res = new List<double[]>();
        if (coords.Length == 0) return res;

        CollectionsMarshal.SetCount(pl, 0);

        var (px1, py1) = (coords[0], coords[1]);

        if (px1 >= left)
        {
            pl.AddRange(px1,
                py1);
        }

        for (var i = 2; i < coords.Length; i += 2)
        {
            var (px2, py2) = (coords[i], coords[i + 1]);

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

                res.Add(pl.ToArray());
                CollectionsMarshal.SetCount(pl, 0);

            }
            (px1, py1) = (px2, py2);
        }
        if (pl.Count > 0) res.Add(pl.ToArray());
        return res;
    }
    private static List<double[]> ClipRight(double[] coords, double right, List<double> pl)
    {
        var res = new List<double[]>();
        if (coords.Length == 0) return res;

        CollectionsMarshal.SetCount(pl, 0);

        var (px1, py1) = (coords[0], coords[1]);

        if (px1 <= right)
        {
            pl.AddRange(px1,
                py1);
        }

        for (var i = 2; i < coords.Length; i += 2)
        {
            var (px2, py2) = (coords[i], coords[i + 1]);

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

                res.Add(pl.ToArray());

                CollectionsMarshal.SetCount(pl, 0);
            }
            (px1, py1) = (px2, py2);

        }
        if (pl.Count > 0) res.Add(pl.ToArray());
        return res;
    }
    private static List<double[]> ClipBottom(double[] coords, double bottom, List<double> pl)
    {
        var res = new List<double[]>();
        if (coords.Length == 0) return res;

        CollectionsMarshal.SetCount(pl, 0);

        var (px1, py1) = (coords[0], coords[1]);

        if (py1 >= bottom)
        {
            pl.AddRange(px1,
                py1);
        }

        for (var i = 2; i < coords.Length; i += 2)
        {
            var (px2, py2) = (coords[i], coords[i + 1]);

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
                pl.AddRange(px1,
                    py1,

                    (bottom - py1) * (px2 - px1) / (py2 - py1) + px1,
                    bottom);

                res.Add(pl.ToArray());

                CollectionsMarshal.SetCount(pl, 0);
            }
            (px1, py1) = (px2, py2);
        }
        if (pl.Count > 0) res.Add(pl.ToArray());
        return res;
    }
    private static List<double[]> ClipTop(double[] coords, double top, List<double> pl)
    {
        var res = new List<double[]>();
        if (coords.Length == 0) return res;

        CollectionsMarshal.SetCount(pl, 0);

        var (px1, py1) = (coords[0], coords[1]);

        if (py1 <= top)
        {
            pl.AddRange(px1,
                py1);
        }

        for (var i = 2; i < coords.Length; i += 2)
        {
            var (px2, py2) = (coords[i], coords[i + 1]);

            if (py1 <= top && py2 <= top)
            {
                pl.AddRange(px2,
                    py2);
            }
            else if (py1 < top && py2 > top)
            {
                pl.AddRange(px1,
                    py1,

                    (top - py1) * (px2 - px1) / (py2 - py1) + px1,
                    top);

                res.Add(pl.ToArray());
                CollectionsMarshal.SetCount(pl, 0);
            }
            else if (py1 > top && py2 < top)
            {
                pl.AddRange((top - py1) * (px2 - px1) / (py2 - py1) + px1,
                    top,

                    px2,
                    py2);


            }
            (px1, py1) = (px2, py2);
        }
        if (pl.Count > 0) res.Add(pl.ToArray());
        return res;
    }


    /** Отсечение полилинии по прямоугольнику */
    public static List<double[]> ClipPolyline(Primitive g, Rect rect, List<double> pl)
    {
        var res = (g.Rect.Left < rect.Left)
            ? ClipLeft(g.Coords, rect.Left, pl)
            : [[..g.Coords]];

        if (g.Rect.Bottom < rect.Bottom)
        {
            var tmp = new List<double[]>(res.Count * 2);
            foreach (var cs in res)
                tmp.AddRange(ClipBottom(cs, rect.Bottom, pl));
            res = tmp;
        }

        if (g.Rect.Right > rect.Right)
        {
            var tmp = new List<double[]>(res.Count * 2);
            foreach (var cs in res)
                tmp.AddRange(ClipRight(cs, rect.Right, pl));
            res = tmp;
        }

        if (g.Rect.Top > rect.Top)
        {
            var tmp = new List<double[]>(res.Count * 2);
            foreach (var cs in res)
                tmp.AddRange(ClipTop(cs, rect.Top, pl));
            res = tmp;
        }

        return res;
    }
}
