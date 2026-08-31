using apiTest.Arena;
using drawer.Models;
using System.Buffers;
using System.Reflection;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace drawer;

internal static class Drawer
{
    /// <summary>
    /// Отсечение графических образов по прямоугольнику
    /// </summary>
    /// <param name="l">Слой</param>
    /// <param name="rect">Прямоугольник для отсечения</param>
    /// <returns>Коллекция отсеченных графических образов</returns>
    private static void ClipPrimitives(Legend l, ref Rect rect, Action<Obraz> visit)
    {
        foreach (var g in l.Primitives)
        {
            var r = g.Rect;
            if (r.Left >= rect.Left && r.Bottom >= rect.Bottom && r.Right <= rect.Right && r.Top <= rect.Top)
                // Целиком лежит внутри прямоугольника
                visit(new Obraz { Coords = [.. g.Coords], Name = g.Name });
            else //if (r.Left < rect.Right && r.Bottom < rect.Top && r.Right > rect.Left && r.Top > rect.Bottom)
                // Необходимо отсекать
                switch (l.Type)
                {
                    case GrTypeEnum.Line:
                        foreach (var cs in Polyline.ClipPolyline(g, rect))
                            visit(new Obraz { Coords = cs, Name = g.Name });
                        break;
                    case GrTypeEnum.Polygon:
                        {
                            var cs = Polygon.ClipPolygon(g, rect);
                            if (cs.Length > 0)
                                visit(new Obraz { Coords = cs, Name = g.Name });
                        }
                        break;
                }
        }

    }

    /// <summary>
    /// Подготовка данных для отрисовки
    /// </summary>
    /// <param name="ls">Слои</param>
    /// <param name="pr">Свойства отрисовки</param>
    /// <param name="rect">Прямоугольник для отсечения</param>
    /// <returns>Результат отсечения и преобразования к экранным координатам</returns>
    public static IEnumerable<LayerResult> BuildGenerator(Legend[] ls, DrawProperties1 pr, Rect rect)
    {
        var distance = pr.Scale;

        foreach (var l in ls)
        {
            if (l.MashtabRange.Min > pr.Mashtab || l.MashtabRange.Max < pr.Mashtab) continue;

            var mas = new List<ObrazResult>(l.Primitives.Length);

            ClipPrimitives(l, ref rect, obraz =>
                mas.Add(new ObrazResult
                {
                    Name = obraz.Name,
                    Coords = obraz.Coords
                       .Optimize(distance)
                       .Translate(ref pr)
                })
            );

            yield return new() { LegendId = l.Id, Obrazes = mas };
        }
    }


    /// <summary>
    /// Подготовка данных для отрисовки
    /// </summary>
    /// <param name="ls">Слои</param>
    /// <param name="pr">Свойства отрисовки</param>
    /// <param name="rect">Прямоугольник для отсечения</param>
    /// <returns>Результат отсечения и преобразования к экранным координатам</returns>

    public unsafe static Memory<LayerResultBlazing> BuildBlazing(this Legend[] ls,
        ArenaAllocator<double> allocator,
        ArenaAllocator<ObrazResultBlazing> allocatorObrazes,
        ArenaAllocator<LayerResultBlazing> allocatorResult,
        ref DrawProperties1 pr,
        ref Rect rect)
    {
        var distance = pr.Scale;

        var reuslt = allocatorResult.Alloc(ls.Length);

        var sp = reuslt.Span;
        var count = 0;

        var (left, top, right, bottom) = (rect.Left, rect.Top, rect.Right, rect.Bottom);

        for (int i = 0; i < ls.Length; i++)
        {
            var l = ls[i];

            if (l.MashtabRange.Min > pr.Mashtab || l.MashtabRange.Max < pr.Mashtab) continue;

            var mas = allocatorObrazes.Alloc(l.Primitives.Length);
            var gSp = mas.Span;

            var index = 0;


            for (int j = 0; j < l.Primitives.Length; j++)
            {

                var g = l.Primitives[j];
                var r = g.Rect;

                if (r.Left >= left && r.Bottom >= bottom && r.Right <= right && r.Top <= top)
                // Целиком лежит внутри прямоугольника
                {
                    gSp[index++] = new ObrazResultBlazing
                    {
                        Name = g.Name,
                        Coords = g.Coords.OptimizeBlazing(allocator, distance).Translate(ref pr)
                    };
                }
                else
                    // Необходимо отсекать
                    switch (l.Type)
                    {
                        case GrTypeEnum.Line:
                            foreach (var cs in Polyline.ClipPolyline(g, rect))
                                gSp[index++] = new ObrazResultBlazing
                                {
                                    Name = g.Name,
                                    Coords = cs.OptimizeBlazing(allocator, distance).Translate(ref pr)
                                };
                            break;
                        case GrTypeEnum.Polygon:
                            {
                                var cs = Polygon.ClipPolygon(g, rect);
                                if (cs.Length > 0)
                                    gSp[index++] = new ObrazResultBlazing
                                    {
                                        Name = g.Name,
                                        Coords = cs.OptimizeBlazing(allocator, distance).Translate(ref pr)
                                    };


                            }
                            break;
                    }
            }

            sp[count++] = new() { LegendId = l.Id, Obrazes = mas[..index] };
        }
        return reuslt[..count];

    }

    public static IEnumerable<LayerResult> AllTakeCount(this IEnumerable<LayerResult> result, int count)
    {
        foreach (var item in result)
            if (--count >= 0) yield return item;
    }
}
