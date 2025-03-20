using Common;
using Microsoft.EntityFrameworkCore;

namespace ExscriboAPI;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
                // Optionally check for pending migrations first
                if ((dbContext.Database.GetPendingMigrations()).Any())
                {
                    dbContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
                logger.LogError(ex, "An error occurred while applying migrations.");
                throw;
            }
        }
    }
}