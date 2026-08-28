using apiTest;
using apiTest.Arena;

//using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    /** Левый верхний угол */
    public Vector<double> LeftTop { get; set; }
}

public struct DrawProperties1
{
    /** Левый верхний угол */
    public Vector<double> LeftTop { get; set; }
    /** Коэффициент масштабирования */
    public Vector<double> Scale { get; set; }
    /** Картографический масштаб, например 1:500 mashtab = 500 */
    public double Mashtab { get; set; }
}

/** Результирующий слой для отображения */
public struct Layer
{
    /** Уникальный идентификатор */
    public Int64 LegendId { get; set; }
    /** Координаты для отрисовки */
    public IList<Obraz> Obrazes { get; set; } = null!;

    public Layer()
    {

    }
}

public struct LayerResult
{
    /** Уникальный идентификатор */
    public Int64 LegendId { get; set; }
    /** Координаты для отрисовки */
    public IList<ObrazResult> Obrazes { get; set; } = null!;

    public LayerResult()
    {

    }
}

public struct LayerResultBlazing
{
    /** Уникальный идентификатор */
    public Int64 LegendId { get; set; }
    /** Координаты для отрисовки */
    public Memory<ObrazResultBlazing> Obrazes { get; set; } = null!;

    public LayerResultBlazing()
    {

    }
}

/** Данные для отображения */
public struct Obraz
{
    /** Имя графического образа */
    public string Name { get; set; } = null!;

    /** Координаты графического образа */
    public double[] Coords { get; set; } = null!;
    public Obraz() { }
}

/** Данные для отображения */
public struct ObrazResult
{
    /** Имя графического образа */
    public string Name { get; set; }

    /** Координаты графического образа */    
    public Memory<double> Coords { get; set; }
}

/** Данные для отображения */
[JsonConverter(typeof(ObrazResultBlazingConverter))]
public struct ObrazResultBlazing
{
    /** Имя графического образа */
    public string Name { get; set; }

    // Добавляем быстрое преобразования double в строку
    /** Координаты графического образа */
    public Memory<double> Coords { get; set; }
}