using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Condubello.Api.Common;
using Condubello.Api.Middleware;
using Condubello.Application;
using Condubello.Application.Common.Interfaces;
using Condubello.Application.Common.Results;
using Condubello.Infrastructure;
using Condubello.Infrastructure.Auth;
using Condubello.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddControllers()
    // Enums como texto: EN_CURSO, FRAUDE_DETECTADO, EN_MANTENIMIENTO...
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)))
    // Errores de validación de DTOs con el mismo envoltorio que el resto de la API.
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error => new ApiError(
                JsonNamingPolicy.CamelCase.ConvertName(entry.Key),
                string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Valor inválido." : error.ErrorMessage)))
            .ToList();

        return new BadRequestObjectResult(ApiResponse<object>.Fail("Los datos enviados no son válidos.", errors));
    });

var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (jwt is null || jwt.Key.Length < 32)
    throw new InvalidOperationException("Configure 'Jwt:Key' con una clave de al menos 32 caracteres.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
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
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Fail("Debe iniciar sesión para acceder a este recurso."));
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Fail("No tiene permisos para realizar esta acción."));
            }
        };
    });
builder.Services.AddAuthorization();

// CORS para el frontend Angular (orígenes configurables en Cors:Origins).
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                  ?? ["http://localhost:4200"];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Condubello API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega aquí el token obtenido en /api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// En contenedores se sirve por HTTP detrás del proxy; la redirección solo aplica fuera de ellos.
if (!app.Configuration.GetValue<bool>("DisableHttpsRedirection"))
    app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Aplica las migraciones al arrancar (activado en Docker con Database__MigrateOnStartup=true).
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
    await DbMigrator.MigrateAsync(app.Services);

await DbSeeder.SeedAsync(app.Services);

await app.RunAsync();
