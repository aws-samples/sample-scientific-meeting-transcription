// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Common
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Use a development connection string for migrations
            var connectionString = "server=localhost;port=3306;database=dummy;Username=dummy;password=dummy";

            optionsBuilder.UseNpgsql(connectionString);
            Console.WriteLine(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}