using apiTest.Arena;
using drawer.Models;
using System.Buffers;
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
    private static IEnumerable<IObraz> ClipPrimitives(ILegend l, Rect rect)
    {
        foreach (var g in l.Primitives)
            if (g.Rect.Left >= rect.Left && g.Rect.Bottom >= rect.Bottom && g.Rect.Right <= rect.Right && g.Rect.Top <= rect.Top)
                // Целиком лежит внутри прямоугольника
                yield return new IObraz { Coords = [.. g.Coords], Name = g.Name };
            else if (g.Rect.Left < rect.Right && g.Rect.Bottom < rect.Top && g.Rect.Right > rect.Left && g.Rect.Top > rect.Bottom)
                // Необходимо отсекать
                switch (l.Type)
                {
                    case GrTypeEnum.Line:
                        foreach (var cs in Polyline.ClipPolyline(g, rect))
                            yield return new IObraz { Coords = cs, Name = g.Name };
                        break;
                    case GrTypeEnum.Polygon:
                        {
                            var cs = Polygon.ClipPolygon(g, rect);
                            if (cs.Length > 0)
                                yield return new IObraz { Coords = cs, Name = g.Name };
                        }
                        break;
                }

    }

    /// <summary>
    /// Отсечение графических образов по прямоугольнику
    /// </summary>
    /// <param name="l">Слой</param>
    /// <param name="rect">Прямоугольник для отсечения</param>
    /// <returns>Коллекция отсеченных графических образов</returns>
    private static IEnumerable<IObrazResult> ClipPrimitivesBlazing(this ILegend l, ArenaAllocator<double> allocator, Rect rect)
    {
        foreach (var g in l.Primitives)
            if (g.Rect.Left >= rect.Left && g.Rect.Bottom >= rect.Bottom && g.Rect.Right <= rect.Right && g.Rect.Top <= rect.Top)
            // Целиком лежит внутри прямоугольника
            {
                var coods = allocator.Alloc(g.Coords.Length);
                g.Coords.CopyTo(coods);

                yield return new IObrazResult { Coords = coods, Name = g.Name };
            }
            else if (g.Rect.Left < rect.Right && g.Rect.Bottom < rect.Top && g.Rect.Right > rect.Left && g.Rect.Top > rect.Bottom)
                // Необходимо отсекать
                switch (l.Type)
                {
                    case GrTypeEnum.Line:
                        foreach (var cs in Polyline.ClipPolyline(g, rect))
                        {
                            var coods = allocator.Alloc(cs.Length);
                            cs.CopyTo(coods);

                            yield return new IObrazResult { Coords = coods, Name = g.Name };
                        }
                        break;
                    case GrTypeEnum.Polygon:
                        {
                            var cs = Polygon.ClipPolygon(g, rect);
                            if (cs.Length > 0)
                            {
                                var coods = allocator.Alloc(cs.Length);
                                cs.CopyTo(coods);

                                yield return new IObrazResult { Coords = coods, Name = g.Name };
                            }
                        }
                        break;
                }

    }

    /// <summary>
    /// Подготовка данных для отрисовки
    /// </summary>
    /// <param name="ls">Слои</param>
    /// <param name="pr">Свойства отрисовки</param>
    /// <param name="rect">Прямоугольник для отсечения</param>
    /// <returns>Результат отсечения и преобразования к экранным координатам</returns>
    public static IEnumerable<ILayerResult> BuildGenerator(ILegend[] ls, DrawProperties1 pr, Rect rect)
    {
        var distance = 1 / pr.Scale[0];

        foreach (var l in ls)
        {
            if (l.MashtabRange.Min > pr.Mashtab || l.MashtabRange.Max < pr.Mashtab) continue;

            var mas = new List<IObrazResult>(l.Primitives.Length + 1000);


            foreach (var obraz in ClipPrimitives(l, rect))            
                mas.Add(new IObrazResult
                {
                    Name = obraz.Name,
                    Coords = obraz.Coords
                        .Optimize(distance)
                        .Translate(ref pr)
                });

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

    public static IEnumerable<ILayerResult> BuildBlazing(this ILegend[] ls, ArenaAllocator<double> allocator, DrawProperties1 pr, Rect rect)
    {
        var distance = 1 / pr.Scale[0];

        foreach (var l in ls)
        {
            if (l.MashtabRange.Min > pr.Mashtab || l.MashtabRange.Max < pr.Mashtab) continue;

            var mas = new List<IObrazResult>(l.Primitives.Length);

            foreach (var obraz in l.ClipPrimitivesBlazing(allocator, rect))
                mas.Add(new IObrazResult
                {
                    Name = obraz.Name,
                    Coords = obraz.Coords
                        .Optimize(allocator, distance)
                        .Translate(ref pr)
                });


            yield return new() { LegendId = l.Id, Obrazes = mas };
        }
    }
}
