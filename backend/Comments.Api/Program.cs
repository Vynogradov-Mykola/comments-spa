using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Comments.Api.Data;
using Comments.Api.Services;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);
// 1. Добавляем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // фронтенд
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .WithExposedHeaders("X-Captcha-Id");
        });
});
builder.Services.AddSingleton<CaptchaService>();
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Путь к папке uploads на сервере
var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

// Создаём папку, если не существует
if (!Directory.Exists(uploadsDir))
{
    Directory.CreateDirectory(uploadsDir);
}

// Разрешаем отдавать статические файлы из папки uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsDir),
    RequestPath = "/uploads"
});

// 2. Используем CORS
app.UseCors("AllowAngular");
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();