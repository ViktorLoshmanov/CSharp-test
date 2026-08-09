using apiTest;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace drawer.Models;

public struct DrawProperties
{
    /** Левый-верхний угол отрисовки */
    public double Left { get; set; }
    public double Top { get; set; }
    /** Коэффициент масштабирования */
    public double Scale { get; set; }
    /** Картографический масштаб, например 1:500 mashtab = 500 */
    public double Mashtab { get; set; }
}

public struct DrawProperties1
{
    public Vector<double> LeftTop { get; set; }
    /** Коэффициент масштабирования */
    public double Scale { get; set; }
    /** Картографический масштаб, например 1:500 mashtab = 500 */
    public double Mashtab { get; set; }
}

/** Результирующий слой для отображения */
//[JsonConverter(typeof(LayerConverter))]
public struct ILayer
{
    /** Уникальный идентификатор */
    public Int64 LegendId { get; set; }
    /** Координаты для отрисовки */
    public IList<IObraz> Obrazes { get; set; } = null!;

    public ILayer()
    {

    }
}
/** Данные для отображения */
public struct IObraz
{
    /** Имя графического образа */
    public string Name { get; set; }

    /** Координаты графического образа */
    public double[] Coords { get; set; }    
}

/** Данные для отображения */
//public struct IObrazResult
//{
//    /** Имя графического образа */
//    public string Name { get; set; }

//    /** Координаты графического образа */
//    public IList<double> Coords { get; set; }
//}


