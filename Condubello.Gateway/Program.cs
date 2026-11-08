using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- JWT: misma configuración que Condubello.Api (la clave debe ser idéntica) ---
var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
if (jwtKey.Length < 32)
    throw new InvalidOperationException("Configure 'Jwt:Key' (la misma de la API) con al menos 32 caracteres.");

/// <summary>Respuesta con el mismo envoltorio ApiResponse que usa la API.</summary>
static Task Escribir(HttpContext context, int status, string mensaje)
{
    context.Response.StatusCode = status;
    return context.Response.WriteAsJsonAsync(new
    {
        success = false,
        message = mensaje,
        data = (object?)null,
        errors = Array.Empty<object>(),
        warnings = Array.Empty<string>()
    }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                return Escribir(context.HttpContext, StatusCodes.Status401Unauthorized, "Debe iniciar sesión para acceder a este recurso.");
            },
            OnForbidden = context => Escribir(context.HttpContext, StatusCodes.Status403Forbidden, "No tiene permisos para realizar esta acción.")
        };
    });

// "anonymous" es un nombre reservado de YARP (rutas públicas); "autenticado" exige un JWT válido.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("autenticado", policy => policy.RequireAuthenticatedUser());

// --- CORS centralizado: el frontend solo habla con el gateway ---
var origenes = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// --- Registro de peticiones: método, ruta, usuario, estado y duración ---
app.Use(async (context, next) =>
{
    var reloj = Stopwatch.StartNew();
    try
    {
        await next();
    }
    finally
    {
        reloj.Stop();
        var usuario = context.User.Identity?.IsAuthenticated == true
            ? $"{context.User.FindFirst("sub")?.Value} ({context.User.FindFirst("role")?.Value})"
            : "anónimo";
        app.Logger.LogInformation("{Metodo} {Ruta} -> {Estado} en {Ms} ms · usuario: {Usuario}",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, reloj.ElapsedMilliseconds, usuario);
    }
});

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapReverseProxy();

app.Run();
