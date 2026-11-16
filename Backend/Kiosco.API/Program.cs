using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Kiosco.API.Authorization;
using Kiosco.API.Middleware;
using Kiosco.Application;
using Kiosco.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication("Firebase")
    .AddScheme<AuthenticationSchemeOptions, FirebaseAuthHandler>("Firebase", null);

// Etapa 3 — Autenticación y autorización:
// Autorización por permisos granulares (RF-007, RF-008, RNF-033, RNF-034).
// El proveedor debe registrarse junto a AddAuthorization para que resuelva
// las políticas dinámicas "Permission:{codigo}".
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

// Validación de tokens trasladada a un puerto testeable (SDD §30.10).
builder.Services.AddScoped<IFirebaseTokenValidator, FirebaseTokenValidator>();
builder.Services.AddTransient<FirebaseAuthHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
