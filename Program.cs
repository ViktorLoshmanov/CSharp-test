using apiTest;
using drawer;
using drawer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.Numerics;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.ConfigureHttpJsonOptions(options => {
//    //options.SerializerOptions.WriteIndented = true;
//    //options.SerializerOptions.IncludeFields = true;
//    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
//    options.SerializerOptions.PropertyNameCaseInsensitive = true;
//});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Тест REST C#",
        Version = "v1"
    });
});


var app = builder.Build();

Init.tt();


app.MapGet("/", () => "Hello World dotnet!");

app.MapGet("/readfile", () => File.ReadAllTextAsync("data.txt"));

app.MapGet("/fibonacci", () =>
{
    var (a, b) = (0.0, 1.0);
    for (var i = 2; i < 2000000; i++)
        (a, b) = (b, a + b);

    return a.ToString();
});


var pr = new DrawProperties { Left = Init.r.Left, Top = Init.r.Top, Scale = 0.37037037037037035, Mashtab = 100 };
var rect = new Rect { Left = 1200, Bottom = 50, Right = 4000, Top = 2850 };


app.MapGet("/map", (double x, double y) =>
{
    x /= 100;
    y /= 100;

    var pr1 = new DrawProperties1
    {
        Mashtab = pr.Mashtab,
        Scale = pr.Scale,
        LeftTop = new Vector<double>([pr.Left + x, pr.Top + y, pr.Left + x, pr.Top + y])
    };

    var rect1 = new Rect
    {
        Left = rect.Left + x,
        Top = rect.Top + y,
        Bottom = rect.Bottom,
        Right = rect.Right
    };


    var result = Drawer.Build(
        Init.ls,
        ref pr1,
        ref rect1);

    return result.Length;
})
    .WithTags("Map")
    .WithSummary("Получение преобразованных геоданных (тест без реального ответа)")
    .WithDescription("Выбирает геоданные по области, отсекает приметивы по оласти, оптимизирует координаты, преобразовывает к экранным")
    .Produces<int>(StatusCodes.Status200OK);


app.MapGet("/mapJSON", (double x = 0, double y = 0) =>
{
    x /= 100;
    y /= 100;

    var pr1 = new DrawProperties1()
    {
        Mashtab = pr.Mashtab,
        Scale = pr.Scale,
        LeftTop = new Vector<double>([pr.Left + x, pr.Top + y, pr.Left + x, pr.Top + y])
    };

    var rect1 = new Rect
    {
        Left = rect.Left + x,
        Top = rect.Top + y,
        Bottom = rect.Bottom,
        Right = rect.Right
    };

    var result = Drawer.Build(
        Init.ls,
        ref pr1,
        ref rect1);


    return result.Take(5);
})
    .WithTags("Map")
    .WithSummary("Получение преобразованных геоданных")
    .WithDescription("Выбирает геоданные по области, отсекает приметивы по оласти, оптимизирует координаты, преобразовывает к экранным")
    .Produces<ILayer[]>(StatusCodes.Status200OK); 

const string STR1 = "asrgfsadf12421";
const string STR2 = "asrgfsadf12321";

app.MapGet("/naturalsort", () =>
{
    var result = 0;
    for (var i = 0; i < 10000; i++)
        result += Strings.CompareUnsafe(STR1 + i, STR2 + i);

    return result;
})
    .WithTags("String")
    .WithSummary("Натуральное сравнение 10000 строк")
    .WithDescription("Фукнция используется в натуральной сортировке");


app.UseSwagger();
app.UseSwaggerUI();

app.Run();
