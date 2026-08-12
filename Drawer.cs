using drawer.Models;
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
                            if (cs.Count > 0)
                                yield return new IObraz { Coords = cs, Name = g.Name };
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
    //public static ILayer[] Build(ILegend[] ls, ref DrawProperties1 pr, ref Rect rect)
    public static List<ILayer> Build(ILegend[] ls, DrawProperties1 pr, Rect rect)
    {
        var result = new List<ILayer>(ls.Length);
        var mashtab = 1 / pr.Scale;

        foreach (var l in ls)
        {
            if (l.MashtabRange.Min > pr.Mashtab || l.MashtabRange.Max < pr.Mashtab) continue;

            var mas = new List<IObraz>();

            var i = 0;
            foreach (var obraz in ClipPrimitives(l, rect))
            {
                mas.Add(new IObraz
                {
                    Name = obraz.Name,
                    Coords = obraz.Coords
                       .Optimize(mashtab)
                       .Translate(pr)
                });
                i++;
            }

            //result.Add(new() { LegendId = l.Id, Obrazes = [.. mas] });
            result.Add(new() { LegendId = l.Id, Obrazes = mas });
        }

        //return [.. result];
        return result;

    }

    /// <summary>
    /// Подготовка данных для отрисовки
    /// </summary>
    /// <param name="ls">Слои</param>
    /// <param name="pr">Свойства отрисовки</param>
    /// <param name="rect">Прямоугольник для отсечения</param>
    /// <returns>Результат отсечения и преобразования к экранным координатам</returns>

    public static IEnumerable<ILayer> BuildGenerator(ILegend[] ls, DrawProperties1 pr, Rect rect)
    {   
        var distance = 1 / pr.Scale;

        foreach (var l in ls)
        {
            if (l.MashtabRange.Min > pr.Mashtab || l.MashtabRange.Max < pr.Mashtab) continue;

            var mas = new List<IObraz>();

            var i = 0;
            foreach (var obraz in ClipPrimitives(l, rect))
            {
                mas.Add(new IObraz
                {
                    Name = obraz.Name,
                    Coords = obraz.Coords
                        .Optimize(distance)
                        .Translate(pr)
                });
                i++;
            }

            yield return new() { LegendId = l.Id, Obrazes = mas };
        }
    }
}
