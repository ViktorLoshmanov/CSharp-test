using apiTest;
using apiTest.Arena;
using drawer;
using drawer.Models;
using LinkDotNet.StringBuilder;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Numerics;

//var builder = WebApplication.CreateBuilder(args);
var builder = WebApplication.CreateSlimBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new()
//    {
//        Title = "Тест REST C#",
//        Version = "v1"
//    });
//});


var app = builder.Build();

Init.tt();


app.MapGet("/", () => "Hello World dotnet!");

app.MapGet("/readfile", () => File.ReadAllTextAsync("data.txt"));
    //.WithSummary("Чтение файла")
    //.Produces<string>(StatusCodes.Status200OK);

app.MapGet("/fibonacci", () =>
{
    var (a, b) = (0UL, 1UL);
    for (var i = 2; i < 2000000; i++)
        (a, b) = (b, a + b);

    return a.ToString();
});


var pr = new DrawProperties { Left = Init.r.Left, Top = Init.r.Top, Scale = 0.37037037037037035, Mashtab = 100 };
var rect = new Rect { Left = 1200, Bottom = 50, Right = 4000, Top = 2850 };

app.MapGet("/map", (double x = 0, double y = 0) =>
{
    x /= 100;
    y /= 100;

    var pr1 = new DrawProperties1
    {
        Mashtab = pr.Mashtab,
        //Scale = pr.Scale,
        Scale = new Vector<double>([pr.Scale, -pr.Scale, pr.Scale, -pr.Scale]),
        LeftTop = new Vector<double>([pr.Left + x, pr.Top + y, pr.Left + x, pr.Top + y])
    };

    var rect1 = new Rect
    {
        Left = rect.Left + x,
        Top = rect.Top + y,
        Bottom = rect.Bottom,
        Right = rect.Right
    };

    return Drawer.BuildGenerator(Init.ls, pr1, rect1)
        .Count();
});
    //.WithTags("Map")
    //.WithSummary("Получение преобразованных геоданных (тест без реального ответа)")
    //.WithDescription("Выбирает геоданные по области, отсекает приметивы по оласти, оптимизирует координаты, преобразовывает к экранным")
    //.Produces<int>(StatusCodes.Status200OK);


app.MapGet("/mapBlazing", (double x = 0, double y = 0) =>
{
    x /= 100;
    y /= 100;

    var pr1 = new DrawProperties1
    {
        Mashtab = pr.Mashtab,
        //Scale = pr.Scale,
        Scale = new Vector<double>([pr.Scale, -pr.Scale, pr.Scale, -pr.Scale]),
        LeftTop = new Vector<double>([pr.Left + x, pr.Top + y, pr.Left + x, pr.Top + y])
    };

    var rect1 = new Rect
    {
        Left = rect.Left + x,
        Top = rect.Top + y,
        Bottom = rect.Bottom,
        Right = rect.Right
    };

    using var allocator = ArenaAllocator<double>.Get();

    return Init.ls.BuildBlazing(allocator, pr1, rect1)
        .Count();
});
    //.WithTags("Map")
    //.WithSummary("Blazing Получение преобразованных геоданных (тест без реального ответа)")
    //.WithDescription("Выбирает геоданные по области, отсекает приметивы по оласти, оптимизирует координаты, преобразовывает к экранным")
    //.Produces<int>(StatusCodes.Status200OK);


app.MapGet("/mapJSON", (double x = 0, double y = 0) =>
{
    x /= 100;
    y /= 100;

    var pr1 = new DrawProperties1()
    {
        Mashtab = pr.Mashtab,
        Scale = new Vector<double>([pr.Scale, -pr.Scale, pr.Scale, -pr.Scale]),
        LeftTop = new Vector<double>([pr.Left + x, pr.Top + y, pr.Left + x, pr.Top + y])
    };

    var rect1 = new Rect
    {
        Left = rect.Left + x,
        Top = rect.Top + y,
        Bottom = rect.Bottom,
        Right = rect.Right
    };

    return Drawer.BuildGenerator(Init.ls, pr1, rect1)
        .ToArray()
        .Take(5);
});
    //.WithTags("Map")
    //.WithSummary("Получение преобразованных геоданных")
    //.WithDescription("Выбирает геоданные по области, отсекает приметивы по оласти, оптимизирует координаты, преобразовывает к экранным")
    //.Produces<ILayer[]>(StatusCodes.Status200OK);

app.MapGet("/mapJSONBlazing", (double x = 0, double y = 0) =>
{
    x /= 100;
    y /= 100;

    var pr1 = new DrawProperties1()
    {
        Mashtab = pr.Mashtab,
        Scale = new Vector<double>([pr.Scale, -pr.Scale, pr.Scale, -pr.Scale]),
        LeftTop = new Vector<double>([pr.Left + x, pr.Top + y, pr.Left + x, pr.Top + y])
    };

    var rect1 = new Rect
    {
        Left = rect.Left + x,
        Top = rect.Top + y,
        Bottom = rect.Bottom,
        Right = rect.Right
    };

    using var allocator = ArenaAllocator<double>.Get();

    return Init.ls.BuildBlazing(allocator, pr1, rect1)
        .ToArray()
        .Take(5);
});
    //.WithTags("Map")
    //.WithSummary("Blazing Получение преобразованных геоданных")
    //.WithDescription("Выбирает геоданные по области, отсекает приметивы по оласти, оптимизирует координаты, преобразовывает к экранным")
    //.Produces<ILayer[]>(StatusCodes.Status200OK);

const string STR1 = "asrgfsadf12421";
const string STR2 = "asrgfsadf12321";

app.MapGet("/naturalsort", () =>
{
    var result = 0;
    for (var i = 0; i < 10000; i++)
    {
        result += Strings.CompareUnsafe(STR1 + i, STR2 + i);
        //result += Strings.CompareSafe(STR1 + i, STR2 + i);
        //result += Strings.CompareIterator(STR1 + i, STR2 + i);
    }

    return result;
});
    //.WithTags("String")
    //.WithSummary("Натуральное сравнение 10000 пар строк")
    //.WithDescription("Фукнция используется в натуральной сортировке");


app.MapGet("/naturalsortblazing", () =>
{
    var result = 0;
    //for (var i = 0U; i < 10000U; i++)
    //{
    //    var s1 = new MimAllocString((uint)STR1.Length + 20U);
    //    s1.Add(STR1);
    //    s1.Add(i);


    //    var s2 = new MimAllocString((uint)STR1.Length + 20U);
    //    s1.Add(STR2);
    //    s1.Add(i);

    //    result += Strings.CompareUnsafe(s1, s2);

    //    s1.Dispose();
    //    s2.Dispose();

    //}

    using var allocator = ArenaAllocator<char>.Get();
    for (var i = 0U; i < 10000U; i++)
    {
        var s1 = new BufferString(allocator, STR1.Length + 20);
        s1.Append(STR1);
        s1.Append(i);

        var s2 = new BufferString(allocator, STR2.Length + 20);
        s2.Append(STR2);
        s2.Append(i);

        result += Strings.CompareUnsafe(ref s1, ref s2);
    }

    return result;
});
    //.WithTags("String")
    //.WithSummary("Blazing Натуральное сравнение 10000 строк")
    //.WithDescription("Фукнция используется в натуральной сортировке");

app.MapGet("/naturalsortHack", () =>
{
    var result = 0;
    using var vsb1 = new ValueStringBuilder(stackalloc char[128]);
    using var vsb2 = new ValueStringBuilder(stackalloc char[128]);

    for (var i = 0; i < 10000; i++)
    {
        vsb1.Clear();
        vsb1.Append(STR1);
        vsb1.Append(i);

        vsb2.Clear();
        vsb2.Append(STR2);
        vsb2.Append(i);

        result += Strings.CompareSBUnSafe(vsb1, vsb2);
    }

    return result;
});
    //.WithTags("String")
    //.WithSummary("Хакерское Натуральное сравнение 10000 строк")
    //.WithDescription("Фукнция используется в натуральной сортировке");


//app.UseSwagger();
//app.UseSwaggerUI();

app.Run();
