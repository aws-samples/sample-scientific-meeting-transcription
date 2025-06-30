// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.

using Common;
using Microsoft.EntityFrameworkCore;

namespace ExscriboAPI;

/// <summary>
/// Extension methods for handling database migrations in the application
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Applies any pending database migrations during application startup
    /// </summary>
    /// <param name="app">The application builder instance</param>
    /// <remarks>
    /// This method creates a service scope, retrieves the database context,
    /// checks for any pending migrations, and applies them if necessary.
    /// Any exceptions during migration are logged and rethrown.
    /// </remarks>
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