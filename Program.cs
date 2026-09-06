using apiTest;
using apiTest.Arena;
using drawer;
using drawer.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.ResponseCompression;
using System.Diagnostics;
//using LinkDotNet.StringBuilder;
using System.Numerics;


Init.tt();

//var builder = WebApplication.CreateBuilder(args);
var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.AllowResponseHeaderCompression = false;
});

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new()
//    {
//        Title = "Тест REST C#",
//        Version = "v1"
//    });
//});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ResultsTypeJsonContext.Default);

    //options.SerializerOptions.DefaultBufferSize = 1024 * 1024;
    //Убрал глобальный обработчик, оставил только в виде аттрибута для ObrazResultBlazing
    // options.SerializerOptions.Converters.Add(new MemoryDoubleRyuConverter());
});

//builder.Services.AddResponseCompression(options =>
//{
//    // Включаем сжатие для HTTPS
//    //options.EnableForHttps = true;

//    // Добавляем провайдера Brotli
//    options.Providers.Add<BrotliCompressionProvider>();
//});


builder.Services.AddScoped<ArenasService>();

//// Регистрация службы ограничения скорости
//builder.Services.AddRateLimiter(options =>
//{
//    // Добавление политики ограничения параллелизма
//    options.AddConcurrencyLimiter("concurrency", concurrencyOptions =>
//    {
//        concurrencyOptions.PermitLimit = 32; // Максимальное число одновременных запросов
//        //concurrencyOptions.QueueLimit = 5;   // Сколько запросов может встать в очередь
//        //concurrencyOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // Порядок обработки очереди
//    });
//});

var app = builder.Build();

app.MapGet("/", static () => "Hello World dotnet!");

app.MapGet("/readfile", static () => File.ReadAllTextAsync("data.txt"))
.WithSummary("Чтение файла")
.Produces<string>(StatusCodes.Status200OK);

app.MapGet("/fibonacci", static () =>
{
    var (a, b) = (0UL, 1UL);
    for (var i = 2; i < 2000000; i++)
        (a, b) = (b, a + b);

    return a.ToString();
});


//var pr = new DrawProperties { Left = Init.r.Left, Top = Init.r.Top, Scale = 0.37037037037037035, Mashtab = 100, LeftTop = new Vector<double>([Init.r.Left, Init.r.Top, Init.r.Left, Init.r.Top]) };
//var rect = new Rect { Left = 1200, Bottom = 50, Right = 4000, Top = 2850 };

app.MapGet("/map", static (double x = 0, double y = 0) =>
{
    var pr = PropertiesConfig.pr;
    var rect = PropertiesConfig.rect;

    x /= 100;
    y /= 100;

    var pr1 = new DrawProperties1
    {
        Mashtab = pr.Mashtab,
        Scale = pr.Scale,
        ScaleVector = new Vector<double>([pr.Scale, -pr.Scale, pr.Scale, -pr.Scale]),
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

// Эта для тестирования с языками которые не умеют нормально хранить между запросами заранее загруженные данные, например PHP
app.MapGet("/mapPerformance", static (double x = 0, double y = 0) =>
{
    var pr = PropertiesConfig.pr;
    var rect = PropertiesConfig.rect;

    x /= 100;
    y /= 100;

    var pr1 = new DrawProperties1
    {
        Mashtab = pr.Mashtab,
        Scale = pr.Scale,
        ScaleVector = new Vector<double>([pr.Scale, -pr.Scale, pr.Scale, -pr.Scale]),
        LeftTop = new Vector<double>([pr.Left + x, pr.Top + y, pr.Left + x, pr.Top + y])
    };

    var rect1 = new Rect
    {
        Left = rect.Left + x,
        Top = rect.Top + y,
        Bottom = rect.Bottom,
        Right = rect.Right
    };

    var stopwatch = Stopwatch.StartNew();
    int count;
    for (var i = 0; i < 1000; i++)
    {
        count = Drawer.BuildGenerator(Init.ls, pr1, rect1).Count();
    }

    stopwatch.Stop();
    return ((double)stopwatch.ElapsedMilliseconds) / 1000;

});

//app.MapGet("/mapBlazing", (ArenasService arenas, double x = 0, double y = 0) =>
//{
//    x /= 100;
//    y /= 100;

//    var vadd = new Vector<double>([x, y, x, y]);
//    var pr1 = new DrawProperties1
//    {
//        Mashtab = pr.Mashtab,
//        Scale = pr.Scale,
//        ScaleVector = new Vector<double>([pr.Scale, -pr.Scale, pr.Scale, -pr.Scale]),
//        LeftTop = pr.LeftTop + vadd
//    };

//    var rect1 = new Rect
//    {
//        Left = rect.Left + x,
//        Top = rect.Top + y,
//        Bottom = rect.Bottom,
//        Right = rect.Right
//    };

//    return Init.ls.BuildBlazing(
//        arenas.Get<double>(),
//        arenas.Get<ObrazResultBlazing>(),
//        arenas.Get<LayerResultBlazing>(), ref pr1, ref rect1)
//        .Length;
//});


app.MapGet("/mapBlazing", static async (HttpContext context, double x = 0, double y = 0) =>
{
    var pr = PropertiesConfig.pr;
    var rect = PropertiesConfig.rect;

    x /= 100;
    y /= 100;

    var vadd = new Vector<double>([x, y, x, y]);
    var pr1 = new DrawProperties1
    {
        Mashtab = pr.Mashtab,
        Scale = pr.Scale,
        ScaleVector = new Vector<double>([pr.Scale, -pr.Scale, pr.Scale, -pr.Scale]),
        LeftTop = pr.LeftTop + vadd
    };

    var rect1 = new Rect
    {
        Left = rect.Left + x,
        Top = rect.Top + y,
        Bottom = rect.Bottom,
        Right = rect.Right
    };

    var dA = ArenaAllocator<double>.Get();
    var oA = ArenaAllocator<ObrazResultBlazing>.Get();
    var lA = ArenaAllocator<LayerResultBlazing>.Get();

    try
    {
        var rusult = Init.ls.BuildBlazing(
            dA,
            oA,
            lA, ref pr1, ref rect1)
            .Length;

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(rusult);
    }
    finally
    {
        dA.Dispose();
        oA.Dispose();
        lA.Dispose();
    }
});

//.WithTags("Map")
//.WithSummary("Blazing Получение преобразованных геоданных (тест без реального ответа)")
//.WithDescription("Выбирает геоданные по области, отсекает примитивы по области, оптимизирует координаты, преобразовывает к экранным")
//.Produces<int>(StatusCodes.Status200OK);


app.MapGet("/mapJSON", static (double x = 0, double y = 0) =>
{
    var pr = PropertiesConfig.pr;
    var rect = PropertiesConfig.rect;

    x /= 100;
    y /= 100;

    var pr1 = new DrawProperties1()
    {
        Mashtab = pr.Mashtab,
        Scale = 1 / pr.Scale,
        ScaleVector = new Vector<double>([pr.Scale, -pr.Scale, pr.Scale, -pr.Scale]),
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
        .AllTakeCount(5);
});
//.WithTags("Map")
//.WithSummary("Получение преобразованных геоданных")
//.WithDescription("Выбирает геоданные по области, отсекает приметивы по оласти, оптимизирует координаты, преобразовывает к экранным")
//.Produces<Layer[]>(StatusCodes.Status200OK);

//app.MapGet("/mapJSONBlazing", (ArenasService arenas, double x = 0, double y = 0) =>
//{
//    x /= 100;
//    y /= 100;

//    var pr1 = new DrawProperties1()
//    {
//        Mashtab = pr.Mashtab,
//        Scale = pr.Scale,
//        ScaleVector = new Vector<double>([pr.Scale, -pr.Scale, pr.Scale, -pr.Scale]),
//        LeftTop = new Vector<double>([pr.Left + x, pr.Top + y, pr.Left + x, pr.Top + y])
//    };

//    var rect1 = new Rect
//    {
//        Left = rect.Left + x,
//        Top = rect.Top + y,
//        Bottom = rect.Bottom,
//        Right = rect.Right
//    };

//    return Init.ls.BuildBlazing(
//        arenas.Get<double>(),
//        arenas.Get<ObrazResultBlazing>(),
//        arenas.Get<LayerResultBlazing>(),
//        ref pr1, ref rect1)[..5];
//});
//.WithTags("Map")
//.WithSummary("Blazing Получение преобразованных геоданных")
//.WithDescription("Выбирает геоданные по области, отсекает приметивы по оласти, оптимизирует координаты, преобразовывает к экранным")
//.Produces<Layer[]>(StatusCodes.Status200OK);


app.MapGet("/mapJSONBlazing", static async (HttpContext context, double x = 0, double y = 0) =>
{
    var pr = PropertiesConfig.pr;
    var rect = PropertiesConfig.rect;

    x /= 100;
    y /= 100;

    var pr1 = new DrawProperties1()
    {
        Mashtab = pr.Mashtab,
        Scale = pr.Scale,
        ScaleVector = new Vector<double>([pr.Scale, -pr.Scale, pr.Scale, -pr.Scale]),
        LeftTop = new Vector<double>([pr.Left + x, pr.Top + y, pr.Left + x, pr.Top + y])
    };

    var rect1 = new Rect
    {
        Left = rect.Left + x,
        Top = rect.Top + y,
        Bottom = rect.Bottom,
        Right = rect.Right
    };

    var dA = ArenaAllocator<double>.Get();
    var oA = ArenaAllocator<ObrazResultBlazing>.Get();
    var lA = ArenaAllocator<LayerResultBlazing>.Get();

    try
    {
        var result = Init.ls.BuildBlazing(
            dA,
            oA,
            lA,
            ref pr1, ref rect1)[..5];

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(result);
    }
    finally
    {
        dA.Dispose();
        oA.Dispose();
        lA.Dispose();
    }
});

const string STR1 = "asrgfsadf12421";
const string STR2 = "asrgfsadf12321";


// Функция используется в натуральной сортировке, цель теста выделить все 10000 пар строк в памяти и сравнить их, из-за лени выделение 10000 пар строк происходит в том же цикле где и сравнение (Для тех кто не прочитал ниже)
app.MapGet("/naturalsort", static () =>
{
    var sp1 = STR1.AsSpan();
    var sp2 = STR2.AsSpan();
    var result = 0;
    for (var i = 0; i < 10000; i++)
    {
        //result += Strings.CompareUnsafe($"{STR1}{i}", $"{STR2}{i}");
        result += Strings.CompareSafe($"{sp1}{i}", $"{sp2}{i}");
    }

    return result;
});
//.WithTags("String")
//.WithSummary("Натуральное сравнение 10000 пар строк")
//.WithDescription("Функция используется в натуральной сортировке, цель теста выделить все 10000 пар строк в памяти и сравнить их, из-за лени выделение 10000 пар строк происходит в том же цикле где и сравнение");


app.MapGet("/naturalsortblazing", static () =>
{
    var result = 0;

    var sp1 = STR1.AsSpan();
    var sp2 = STR2.AsSpan();
    var (l1, l2) = (STR1.Length + 5, STR2.Length + 5);

    using var allocator = ArenaAllocator<char>.Get();

    for (var i = 0U; i < 10000U; i++)
    {
        var s1 = new BufferString(allocator, l1);
        s1.Append(in sp1);
        s1.Append(i);

        var s2 = new BufferString(allocator, l2);
        s2.Append(sp2);
        s2.Append(i);

        result += Strings.CompareUnsafe(in s1, in s2);
    }

    return result;
});
//.WithTags("String")
//.WithSummary("Blazing Натуральное сравнение 10000 строк")
//.WithDescription("Функция используется в натуральной сортировке, цель теста выделить все 10000 пар строк в памяти и сравнить их, из-за лени выделение 10000 пар строк происходит в том же цикле где и сравнение");


static unsafe int NaturalSortHack()
{
    var result = 0;
    Span<char> vsb1 = stackalloc char[128];
    Span<char> vsb2 = stackalloc char[128];
    var s1 = STR1.AsSpan();
    var s2 = STR1.AsSpan();
    var l1 = STR1.Length;
    var l2 = STR2.Length;

    fixed (char* pointer1 = s1, pointer2 = s2)

        for (var i = 0; i < 10000; i++)
        {
            s1.CopyTo(vsb1);
            if (!i.TryFormat(vsb1[l1..], out var charsWritten1, default, null))
                throw new InvalidOperationException($"Не удалось вставить {i} в указанный буфер");

            vsb1[l1 + charsWritten1] = (char)0;


            s2.CopyTo(vsb2);
            if (!i.TryFormat(vsb2[l1..], out var charsWritten2, default, null))
                throw new InvalidOperationException($"Не удалось вставить {i} в указанный буфер");

            vsb2[l2 + charsWritten2] = (char)0;

            result += Strings.CompareUnsafe(pointer1, pointer2);
        }

    return result;
}

app.MapGet("/naturalsortHack", NaturalSortHack);
//.WithTags("String")
//.WithSummary("Хакерское Натуральное сравнение 10000 строк")
//.WithDescription("Фукнция используется в натуральной сортировке");


//app.UseSwagger();
//app.UseSwaggerUI();

app.Run();

static class PropertiesConfig
{
    public static readonly DrawProperties pr = new DrawProperties { Left = Init.r.Left, Top = Init.r.Top, Scale = 0.37037037037037035, Mashtab = 100, LeftTop = new Vector<double>([Init.r.Left, Init.r.Top, Init.r.Left, Init.r.Top]) };
    public static readonly Rect rect = new Rect { Left = 1200, Bottom = 50, Right = 4000, Top = 2850 };
}
