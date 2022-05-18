using ElementsWebAPI.Entities;
using ElementsWebAPI.Interfaces;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Web Service \"Elements\" API",
        Description = "API для работы с Веб-сервисом по свойствам химических элементов.",
        Contact = new OpenApiContact
        {
            Name = "Родионов Алексей",
            Url = new Uri("mailto:aarodionov_2@edu.hse.ru")
        },
        License = new OpenApiLicense
        {
            Name = "Исходное веб-приложение",
            Url = new Uri("https://phase.imet-db.ru/elements/main.aspx")
        }
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();
// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
