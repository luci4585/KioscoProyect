using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Kiosco.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<KioscoDbContext>(options =>
    options.UseNpgsql(connectionString));

var firebaseJsonBase64 = builder.Configuration["Firebase:ServiceAccountJsonBase64"]
    ?? throw new InvalidOperationException(
        "Firebase:ServiceAccountJsonBase64 no está configurado.");

var firebaseJson = System.Text.Encoding.UTF8.GetString(
    Convert.FromBase64String(firebaseJsonBase64));

if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromJson(firebaseJson)
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();