using BenitezLabs.Persistence;
using EmpadronamientoBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmpadronamientoBackend.Infrastructure.Persistence;

/// <summary>
/// Registro de dependencias de la capa Infrastructure.
/// Aquí se configura EF Core y la conexión a MariaDB.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Obtener connection string desde appsettings.json
        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        // Registro del DbContext usando MariaDB/MySQL (Pomelo)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mysqlOptions =>
                {
                    // 🔥 Configuración enterprise recomendada
                    mysqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }));

        return services;
    }
}