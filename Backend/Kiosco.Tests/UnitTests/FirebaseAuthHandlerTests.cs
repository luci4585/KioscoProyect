using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Kiosco.API.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kiosco.Tests.UnitTests;

/// <summary>
/// Tests de la Etapa 3 (RoadMap §6 — Testing):
/// 1. Usuario autenticado
/// 2. Usuario no autenticado
/// 3. Token inválido
/// Validan el comportamiento de autenticación de Firebase (RNF-024, RNF-025, RNF-026)
/// mediante un adaptador de tokens falso, tal como prevé el SDD §30.10
/// (reemplazar adaptadores reales por implementaciones de prueba).
/// </summary>
public class FirebaseAuthHandlerTests
{
    private static readonly AuthenticationScheme Scheme =
        new("Firebase", "Firebase", typeof(FirebaseAuthHandler));

    /// <summary>Validador falso: devuelve las claims indicadas o null (token inválido).</summary>
    private sealed class FakeTokenValidator : IFirebaseTokenValidator
    {
        private readonly FirebaseTokenClaims? _claims;

        public FakeTokenValidator(FirebaseTokenClaims? claims) => _claims = claims;

        public Task<FirebaseTokenClaims?> ValidateAsync(string idToken)
            => Task.FromResult(_claims);
    }

    /// <summary>Monitor de opciones mínimo para construir el handler en tests.</summary>
    private sealed class FakeOptionsMonitor : IOptionsMonitor<AuthenticationSchemeOptions>
    {
        public AuthenticationSchemeOptions CurrentValue => new AuthenticationSchemeOptions();

        public AuthenticationSchemeOptions Get(string? name) => new AuthenticationSchemeOptions();

        public IDisposable? OnChange(Action<AuthenticationSchemeOptions, string?> listener) => null;
    }

    private static FirebaseAuthHandler CreateHandler(IFirebaseTokenValidator validator)
    {
        return new FirebaseAuthHandler(
            new FakeOptionsMonitor(),
            LoggerFactory.Create(_ => { }),
            UrlEncoder.Default,
            validator);
    }

    private static DefaultHttpContext CreateContextWithHeader(string? authorization)
    {
        var context = new DefaultHttpContext();
        if (authorization != null)
            context.Request.Headers.Authorization = authorization;
        return context;
    }

    private static async Task<AuthenticateResult> AuthenticateAsync(
        FirebaseAuthHandler handler,
        HttpContext context)
    {
        var interfaceHandler = (IAuthenticationHandler)handler;
        await interfaceHandler.InitializeAsync(Scheme, context);
        return await interfaceHandler.AuthenticateAsync();
    }

    [Fact]
    public async Task ConTokenValido_UsuarioAutenticado_AutenticaExitosamente()
    {
        // Arrange — RNF-025: el backend valida el token y extrae la identidad.
        var validator = new FakeTokenValidator(new FirebaseTokenClaims
        {
            Uid = "uid-123",
            Email = "test@kiosco.com",
            Name = "Test User"
        });
        var handler = CreateHandler(validator);
        var context = CreateContextWithHeader("Bearer token-valido-123");

        // Act
        var result = await AuthenticateAsync(handler, context);

        // Assert
        result.Succeeded.Should().BeTrue("el token válido debe autenticar al usuario");
        result.Principal.Should().NotBeNull();
        result.Principal!.FindFirst("firebase_uid")!.Value.Should().Be("uid-123");
        result.Principal.FindFirst(ClaimTypes.Email)!.Value.Should().Be("test@kiosco.com");
        result.Principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task SinHeaderDeAuthorization_UsuarioNoAutenticado_NoResult()
    {
        // Arrange — RNF-026: sin token no hay autenticación → 401 en el pipeline.
        var handler = CreateHandler(new FakeTokenValidator(null));
        var context = CreateContextWithHeader(authorization: null);

        // Act
        var result = await AuthenticateAsync(handler, context);

        // Assert
        result.None.Should().BeTrue("sin header Authorization no debe intentar validar");
        result.Succeeded.Should().BeFalse();
        result.Principal.Should().BeNull();
    }

    [Fact]
    public async Task ConTokenInvalido_FallaLaAutenticacion()
    {
        // Arrange — RNF-025: token inválido → rechazo (401).
        var handler = CreateHandler(new FakeTokenValidator(claims: null));
        var context = CreateContextWithHeader("Bearer token-basura");

        // Act
        var result = await AuthenticateAsync(handler, context);

        // Assert
        result.Succeeded.Should().BeFalse("un token inválido no debe autenticar");
        result.None.Should().BeFalse("hubo intento de autenticación, no ausencia de token");
        result.Principal.Should().BeNull();
    }
}
