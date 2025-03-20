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
            var connectionString = "dummyconnection";

            optionsBuilder.UseNpgsql(connectionString);
            Console.WriteLine(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}