using Condubello.Application.Common.Interfaces;
using Condubello.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Condubello.Infrastructure.Persistence;

/// <summary>Crea el primer administrativo (Seed:AdminEmail / Seed:AdminPassword) si no existe ninguno.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbSeeder));
        var configuration = services.GetRequiredService<IConfiguration>();

        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            if (await context.Administrativos.AnyAsync(cancellationToken))
                return;

            var passwordHasher = services.GetRequiredService<IPasswordHasher>();
            context.Administrativos.Add(new Administrativo
            {
                Cedula = "0000000000",
                Nombre = "Administrador",
                Apellidos = "Condubello",
                Cargo = "Administrador del sistema",
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHasher.Hash(password)
            });
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Administrativo inicial creado con el email {Email}.", email);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "No se pudo crear el administrativo inicial. ¿Ejecutaste 'dotnet ef database update' y PostgreSQL está disponible?");
        }
    }
}
